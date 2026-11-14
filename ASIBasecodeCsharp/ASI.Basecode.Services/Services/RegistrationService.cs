using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class RegistrationService : IRegistrationService
    {
        // No 0/O or 1/I so codes are easy to read out and type in.
        private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int CodeLength = 10;

        private readonly IEventRegistrationRepository _repository;
        private readonly IEventRepository _eventRepository;
        private readonly IAccessService _accessService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrationService(IEventRegistrationRepository repository,
                                   IEventRepository eventRepository,
                                   IAccessService accessService,
                                   IUserRepository userRepository,
                                   IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _eventRepository = eventRepository;
            _accessService = accessService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Registers the student, or waitlists them when the event is full. Runs in a transaction that first locks the
        /// event row, so two students taking the last slot at the same moment are handled one after the other
        /// (the second one is waitlisted) instead of both being registered past the capacity.
        /// </summary>
        public RegistrationStatus Register(int eventId, CurrentUserModel currentUser)
        {
            var database = _unitOfWork.Database.Database;
            if (!database.IsRelational())
            {
                return RegisterLocked(eventId, currentUser);
            }

            try
            {
                using var transaction = database.BeginTransaction(IsolationLevel.ReadCommitted);
                // UPDLOCK + HOLDLOCK: other registrations for this event wait here until this one commits
                database.ExecuteSqlInterpolated($"SELECT id FROM events WITH (UPDLOCK, HOLDLOCK) WHERE id = {eventId}");
                var status = RegisterLocked(eventId, currentUser);
                transaction.Commit();
                return status;
            }
            catch (Exception ex) when (ex is DbUpdateException || ex is SqlException || ex is InvalidOperationException && ex.InnerException is SqlException)
            {
                // deadlock victim, lock timeout or a duplicate row from a double click
                _unitOfWork.Database.ChangeTracker.Clear();
                throw new InvalidDataException(Resources.Messages.Errors.RegistrationBusy);
            }
        }

        /// <summary>
        /// The registration rules, checked in this order (any failure throws, and ServiceExceptionFilter shows the
        /// message to the student): event exists -> organization active -> registration open -> capacity decides
        /// Registered/Waitlisted -> not registered twice -> course allowed -> no overlapping event -> save.
        /// </summary>
        private RegistrationStatus RegisterLocked(int eventId, CurrentUserModel currentUser)
        {
            // STEP 1: the event must exist and be published (students never see Draft events)
            var ev = _eventRepository.GetEvents().FirstOrDefault(e => e.Id == eventId && e.Status != EventStatus.Draft)
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
            // STEP 2: a deactivated organization's events can't be joined
            if (!_eventRepository.GetEvents().Any(e => e.Id == eventId && e.Organization.IsActive))
            {
                throw new InvalidDataException(Resources.Messages.Errors.OrganizationInactive);
            }

            // STEP 3: registration is open only while the event is Open, the deadline hasn't passed and it hasn't started
            // (AppClock.Now = Philippine time, whatever time zone the server is in)
            var now = AppClock.Now;
            if (ev.Status != EventStatus.Open || now > ev.RegistrationDeadline || now >= ev.StartTime)
            {
                throw new InvalidDataException(Resources.Messages.Errors.RegistrationClosed);
            }

            // STEP 4: capacity. Count only Registered students (waitlisted/cancelled don't hold a slot).
            // Full -> Waitlisted, otherwise Registered. Capacity 0 means unlimited.
            var registeredCount = _repository.GetRegistrations()
                .Count(r => r.EventId == eventId && r.Status == RegistrationStatus.Registered);
            var status = ev.Capacity > 0 && registeredCount >= ev.Capacity
                ? RegistrationStatus.Waitlisted
                : RegistrationStatus.Registered;

            // STEP 5: one registration per student per event. CurrentUser comes from the sign-in cookie,
            // so a student can only ever register themselves.
            var existing = _repository.GetRegistrations()
                .FirstOrDefault(r => r.EventId == eventId && r.StudentId == currentUser.AccountId);
            if (existing != null)
            {
                if (existing.Status != RegistrationStatus.Cancelled)
                {
                    throw new InvalidDataException(Resources.Messages.Errors.AlreadyRegistered);
                }
            }

            // STEP 6: course restriction: a new registration (or re-registering after a cancellation) needs an allowed course.
            EnsureCourseAllowed(eventId, currentUser);

            // STEP 7: no double booking: refuse when the student already holds a registration for an event at the same time.
            var clash = ScheduleConflicts.Overlapping(_repository.GetRegistrations(), currentUser.AccountId, eventId, ev.StartTime, ev.EndTime)
                .OrderBy(r => r.Event.StartTime)
                .Select(r => new { r.Event.Title, r.Event.StartTime })
                .FirstOrDefault();
            if (clash != null)
            {
                throw new InvalidDataException(string.Format(Resources.Messages.Errors.ScheduleConflict, clash.Title, clash.StartTime.ToString("MMM d, h:mm tt")));
            }

            // STEP 8: save. Re-registering reuses the old cancelled row; otherwise insert a new one with a random
            // code (that code is what the QR pass contains).
            if (existing != null)
            {
                // Re-registering after a cancellation reuses the row (one row per student per event).
                existing.Status = status;
                existing.RegisteredTime = now;
                existing.CancelledTime = null;
                _repository.UpdateRegistration(existing);
                return status;
            }

            _repository.AddRegistration(new EventRegistration
            {
                EventId = eventId,
                StudentId = currentUser.AccountId,
                RegistrationCode = GenerateUniqueCode(),
                Status = status,
                RegisteredTime = now,
            });
            return status;
        }

        /// <summary>
        /// Cancels a registration (the student's own, or by an officer/admin of the event's organization).
        /// If it held a slot, the first waitlisted students move up into it.
        /// </summary>
        public void Cancel(int registrationId, CurrentUserModel currentUser)
        {
            // STEP 1: load the registration (projected so CanCancel can see the event's status and time)
            var registration = _repository.GetRegistrations()
                .Where(r => r.Id == registrationId)
                .Select(RegistrationProjection.ToViewModel)
                .FirstOrDefault()
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);

            // STEP 2: only the owner or a manager; and only before the event starts with no attendance recorded
            EnsureOwnerOrManager(registration, currentUser);
            if (!registration.CanCancel)
            {
                throw new InvalidDataException(Resources.Messages.Errors.CannotCancelRegistration);
            }

            // STEP 3: mark it Cancelled (the row is kept for history; it is not deleted)
            var entity = _repository.GetRegistrations().First(r => r.Id == registrationId);
            var wasRegistered = entity.Status == RegistrationStatus.Registered;
            entity.Status = RegistrationStatus.Cancelled;
            entity.CancelledTime = AppClock.Now;
            _repository.UpdateRegistration(entity);

            // STEP 4: a slot was freed (a waitlisted cancellation frees nothing) -> promote from the waitlist
            if (wasRegistered)
            {
                PromoteWaitlisted(entity.EventId);
            }
        }

        public List<RegistrationViewModel> GetMyRegistrations(int accountId)
        {
            return _repository.GetRegistrations()
                .Where(r => r.StudentId == accountId)
                .OrderByDescending(r => r.Event.StartTime)
                .Select(RegistrationProjection.ToViewModel)
                .ToList();
        }

        public RegistrationViewModel GetTicket(int registrationId, CurrentUserModel currentUser)
        {
            var registration = _repository.GetRegistrations()
                .Where(r => r.Id == registrationId)
                .Select(RegistrationProjection.ToViewModel)
                .FirstOrDefault()
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);

            EnsureOwnerOrManager(registration, currentUser);
            // a QR code only while it can still be scanned: registered, and the event is open or closed (not cancelled/completed)
            if (registration.Status == RegistrationStatus.Registered
                && (registration.EventStatus == EventStatus.Open || registration.EventStatus == EventStatus.Closed))
            {
                registration.QrCodeSvg = QrCodeManager.GenerateSvg(registration.RegistrationCode);
            }
            return registration;
        }

        /// <summary>
        /// Fills free slots from the waitlist, first come first served (oldest RegisteredTime first).
        /// Called after a cancellation and after an officer edits the event (the capacity may have gone up).
        /// </summary>
        public void PromoteWaitlisted(int eventId)
        {
            // only while the event is still running (not Draft, Completed or Cancelled)
            var ev = _eventRepository.GetEvents().FirstOrDefault(e => e.Id == eventId);
            if (ev == null || (ev.Status != EventStatus.Open && ev.Status != EventStatus.Closed))
            {
                return;
            }

            var waitlist = _repository.GetRegistrations()
                .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
                .OrderBy(r => r.RegisteredTime)
                .ToList();
            if (waitlist.Count == 0) return;

            // free slots = capacity - registered (unlimited capacity: everyone on the waitlist gets in)
            var freeSlots = waitlist.Count;
            if (ev.Capacity > 0)
            {
                var registeredCount = _repository.GetRegistrations()
                    .Count(r => r.EventId == eventId && r.Status == RegistrationStatus.Registered);
                freeSlots = Math.Max(ev.Capacity - registeredCount, 0);
            }

            foreach (var registration in waitlist.Take(freeSlots))
            {
                registration.Status = RegistrationStatus.Registered;
                _repository.UpdateRegistration(registration);
            }
        }

        /// <summary>
        /// Course eligibility, enforced on the server. The Event page also disables the button for ineligible
        /// students, but this check is what actually stops a forced request.
        /// </summary>
        private void EnsureCourseAllowed(int eventId, CurrentUserModel currentUser)
        {
            // the event's allowed courses (event_allowed_courses table)
            var allowed = _eventRepository.GetEvents()
                .Where(e => e.Id == eventId)
                .SelectMany(e => e.AllowedCourses.Select(c => c.Course))
                .ToList();
            if (allowed.Count == 0) return;   // open to all students

            // the student's course is read from the database, never from the form
            var course = _userRepository.GetUsers().Where(u => u.Id == currentUser.AccountId).Select(u => u.Course).FirstOrDefault();
            if (course == null || !allowed.Contains(course))
            {
                throw new InvalidDataException(Resources.Messages.Errors.NotEligibleForEvent);
            }
        }

        private void EnsureOwnerOrManager(RegistrationViewModel registration, CurrentUserModel currentUser)
        {
            if (registration.StudentId == currentUser.AccountId) return;

            var organizationId = _eventRepository.GetEvents()
                .Where(e => e.Id == registration.EventId)
                .Select(e => e.OrganizationId)
                .First();
            _accessService.EnsureCanManageOrganization(organizationId, currentUser);
        }

        private string GenerateUniqueCode()
        {
            string code;
            do
            {
                var chars = new char[CodeLength];
                for (var i = 0; i < CodeLength; i++)
                {
                    chars[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
                }
                code = new string(chars);
            }
            while (_repository.GetRegistrations().Any(r => r.RegistrationCode == code));

            return code;
        }
    }
}
