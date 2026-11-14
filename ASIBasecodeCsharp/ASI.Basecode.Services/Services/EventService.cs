using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class EventService : IEventService
    {
        private static readonly Dictionary<EventStatus, EventStatus[]> AllowedTransitions = new()
        {
            [EventStatus.Draft] = new[] { EventStatus.Open, EventStatus.Cancelled },
            [EventStatus.Open] = new[] { EventStatus.Closed, EventStatus.Completed, EventStatus.Cancelled },
            [EventStatus.Closed] = new[] { EventStatus.Open, EventStatus.Completed, EventStatus.Cancelled },
            [EventStatus.Completed] = Array.Empty<EventStatus>(),
            [EventStatus.Cancelled] = Array.Empty<EventStatus>(),
        };

        private readonly IEventRepository _repository;
        private readonly IAccessService _accessService;
        private readonly IRegistrationService _registrationService;
        private readonly IAttendanceService _attendanceService;
        private readonly IUserRepository _userRepository;

        public EventService(IEventRepository repository,
                            IAccessService accessService,
                            IRegistrationService registrationService,
                            IAttendanceService attendanceService,
                            IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _accessService = accessService;
            _registrationService = registrationService;
            _attendanceService = attendanceService;
        }

        public List<EventViewModel> GetPublishedEvents(CurrentUserModel currentUser, string search, bool upcomingOnly)
        {
            var query = Search(_repository.GetEvents(), search).Where(e => e.Status != EventStatus.Draft);
            if (currentUser.Role == Role.Student)
            {
                // a deactivated organization's events are hidden from students
                query = query.Where(e => e.Organization.IsActive);
            }
            if (upcomingOnly)
            {
                var now = AppClock.Now;
                query = query.Where(e => e.EndTime >= now && e.Status != EventStatus.Cancelled);
            }

            var events = query.OrderBy(e => e.StartTime).Select(EventProjection.ToViewModel(currentUser.AccountId, ViewerCourse(currentUser))).ToList();
            ApplyCanManage(events, currentUser);
            return events;
        }

        public List<EventViewModel> GetManagedEvents(CurrentUserModel currentUser, string search, EventStatus? status, int? organizationId = null, string sort = null)
        {
            var organizationIds = _accessService.GetManagedOrganizationIds(currentUser);
            var query = Search(_repository.GetEvents(), search).Where(e => organizationIds.Contains(e.OrganizationId));
            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }
            if (organizationId.HasValue)
            {
                query = query.Where(e => e.OrganizationId == organizationId.Value);
            }

            // newest (latest start date) first unless the user picks oldest first
            var ordered = sort == Const.SortOldest
                ? query.OrderBy(e => e.StartTime).ThenBy(e => e.Id)
                : query.OrderByDescending(e => e.StartTime).ThenByDescending(e => e.Id);
            var events = ordered.Select(EventProjection.ToViewModel(currentUser.AccountId)).ToList();
            events.ForEach(e => e.CanManage = true);
            return events;
        }

        public EventViewModel GetEvent(int id, CurrentUserModel currentUser)
        {
            var model = _repository.GetEvents().Where(e => e.Id == id).Select(EventProjection.ToViewModel(currentUser.AccountId, ViewerCourse(currentUser))).FirstOrDefault()
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);

            model.CanManage = _accessService.CanManageOrganization(model.OrganizationId, currentUser);
            if ((model.Status == EventStatus.Draft || !model.OrganizationIsActive) && !model.CanManage)
            {
                throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
            }
            return model;
        }

        public int CreateEvent(EventViewModel model, CurrentUserModel currentUser)
        {
            _accessService.EnsureCanManageOrganization(model.OrganizationId, currentUser);
            var now = AppClock.Now;
            if (model.StartTime <= now)
            {
                throw new InvalidDataException(Resources.Messages.Errors.EventMustStartInFuture);
            }
            if (model.RegistrationDeadline <= now)
            {
                throw new InvalidDataException(Resources.Messages.Errors.DeadlineMustBeInFuture);
            }

            var entity = new Event();
            model.ApplyTo(entity);
            entity.Status = EventStatus.Draft;
            entity.CreatedTime = AppClock.Now;
            entity.UpdatedTime = AppClock.Now;
            entity.CreatedBy = currentUser.UserId;
            entity.UpdatedBy = currentUser.UserId;
            foreach (var course in AllowedCoursesOf(model))
            {
                entity.AllowedCourses.Add(new EventAllowedCourse { Course = course });
            }

            _repository.AddEvent(entity);
            return entity.Id;
        }

        public void UpdateEvent(EventViewModel model, CurrentUserModel currentUser)
        {
            var entity = FindEvent(model.Id);
            _accessService.EnsureCanManageOrganization(entity.OrganizationId, currentUser);
            _accessService.EnsureCanManageOrganization(model.OrganizationId, currentUser);
            if (entity.Status == EventStatus.Completed || entity.Status == EventStatus.Cancelled)
            {
                throw new InvalidDataException(Resources.Messages.Errors.EventFinalized);
            }
            // moving the start or the deadline is only allowed to a time that hasn't passed
            var now = AppClock.Now;
            if (model.StartTime != entity.StartTime && model.StartTime <= now)
            {
                throw new InvalidDataException(Resources.Messages.Errors.EventMustStartInFuture);
            }
            if (model.RegistrationDeadline != entity.RegistrationDeadline && model.RegistrationDeadline <= now)
            {
                throw new InvalidDataException(Resources.Messages.Errors.DeadlineMustBeInFuture);
            }

            var registeredCount = entity.Registrations.Count(r => r.Status == RegistrationStatus.Registered);
            if (model.Capacity > 0 && model.Capacity < registeredCount)
            {
                throw new InvalidDataException(Resources.Messages.Errors.CapacityBelowRegistered);
            }

            model.ApplyTo(entity);
            entity.UpdatedTime = AppClock.Now;
            entity.UpdatedBy = currentUser.UserId;

            // Sync the allowed courses. Students already registered keep their registration even if their
            // course is no longer allowed; only new registrations are checked.
            var courses = AllowedCoursesOf(model);
            foreach (var removed in entity.AllowedCourses.Where(c => !courses.Contains(c.Course)).ToList())
            {
                entity.AllowedCourses.Remove(removed);
            }
            foreach (var added in courses.Where(c => entity.AllowedCourses.All(a => a.Course != c)))
            {
                entity.AllowedCourses.Add(new EventAllowedCourse { Course = added });
            }

            _repository.UpdateEvent(entity);

            // Capacity may have been raised: move waitlisted students into the free slots.
            _registrationService.PromoteWaitlisted(entity.Id);
        }

        public void DeleteEvent(int id, CurrentUserModel currentUser)
        {
            var entity = FindEvent(id);
            _accessService.EnsureCanManageOrganization(entity.OrganizationId, currentUser);
            if (entity.Registrations.Any())
            {
                throw new InvalidDataException(Resources.Messages.Errors.EventHasRegistrations);
            }

            _repository.DeleteEvent(entity);
        }

        public void ChangeStatus(int id, EventStatus status, CurrentUserModel currentUser)
        {
            var entity = FindEvent(id);
            _accessService.EnsureCanManageOrganization(entity.OrganizationId, currentUser);
            if (!AllowedTransitions[entity.Status].Contains(status))
            {
                throw new InvalidDataException(Resources.Messages.Errors.InvalidStatusChange);
            }

            entity.Status = status;
            entity.UpdatedTime = AppClock.Now;
            entity.UpdatedBy = currentUser.UserId;
            _repository.UpdateEvent(entity);

            if (status == EventStatus.Completed)
            {
                _attendanceService.MarkAbsentees(id, currentUser.UserId);
            }
        }

        /// <summary>Officers get this long after an event ends to finish attendance before it completes on its own.</summary>
        public static readonly TimeSpan AutoCompleteAfterEnd = TimeSpan.FromHours(24);

        /// <summary>Who the automatic changes are recorded as (UpdatedBy / RecordedBy).</summary>
        public const string SystemActor = "system";

        public int AdvanceEventStatuses()
        {
            var now = AppClock.Now;
            var completeBefore = now - AutoCompleteAfterEnd;
            var changed = 0;

            var toComplete = _repository.GetEvents()
                .Where(e => (e.Status == EventStatus.Open || e.Status == EventStatus.Closed) && e.EndTime <= completeBefore)
                .ToList();
            foreach (var entity in toComplete)
            {
                entity.Status = EventStatus.Completed;
                entity.UpdatedTime = now;
                entity.UpdatedBy = SystemActor;
                _repository.UpdateEvent(entity);
                _attendanceService.MarkAbsentees(entity.Id, SystemActor);
                changed++;
            }

            // registration can't happen after the start anyway; Closed says so on every list
            var toClose = _repository.GetEvents()
                .Where(e => e.Status == EventStatus.Open && e.StartTime <= now)
                .ToList();
            foreach (var entity in toClose)
            {
                entity.Status = EventStatus.Closed;
                entity.UpdatedTime = now;
                entity.UpdatedBy = SystemActor;
                _repository.UpdateEvent(entity);
                changed++;
            }

            return changed;
        }

        public List<EventStatus> GetAllowedStatusChanges(EventStatus current)
        {
            return AllowedTransitions[current].ToList();
        }

        private Event FindEvent(int id)
        {
            return _repository.GetEvents().Include(e => e.Registrations).Include(e => e.AllowedCourses).FirstOrDefault(e => e.Id == id)
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
        }

        /// <summary>The courses the form allows (none when the event is open to all students).</summary>
        private static List<string> AllowedCoursesOf(EventViewModel model)
        {
            if (!model.RestrictToCourses || model.AllowedCourses == null) return new List<string>();
            return model.AllowedCourses.Where(Courses.IsValid).Distinct().ToList();
        }

        /// <summary>The signed-in student's course (null for staff), used to work out event eligibility.</summary>
        private string ViewerCourse(CurrentUserModel currentUser)
        {
            if (currentUser.Role != Role.Student) return null;
            return _userRepository.GetUsers().Where(u => u.Id == currentUser.AccountId).Select(u => u.Course).FirstOrDefault();
        }

        private static IQueryable<Event> Search(IQueryable<Event> query, string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;

            var term = search.Trim().ToLower();
            return query.Where(e => e.Title.ToLower().Contains(term)
                                 || e.Venue.ToLower().Contains(term)
                                 || e.Organization.Name.ToLower().Contains(term)
                                 || (e.Organization.Acronym != null && e.Organization.Acronym.ToLower().Contains(term)));
        }

        private void ApplyCanManage(List<EventViewModel> events, CurrentUserModel currentUser)
        {
            var managed = _accessService.GetManagedOrganizationIds(currentUser);
            events.ForEach(e => e.CanManage = managed.Contains(e.OrganizationId));
        }
    }
}
