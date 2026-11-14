using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Data.Models;
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
    public class AttendanceService : IAttendanceService
    {
        /// <summary>Check-in opens this many hours before the event starts.</summary>
        public const int CheckInOpensHoursBefore = 2;

        /// <summary>Same as the remarks column length.</summary>
        public const int RemarksMaxLength = 250;

        private readonly IAttendanceRepository _repository;
        private readonly IEventRegistrationRepository _registrationRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IAccessService _accessService;

        public AttendanceService(IAttendanceRepository repository,
                                 IEventRegistrationRepository registrationRepository,
                                 IEventRepository eventRepository,
                                 IAccessService accessService)
        {
            _repository = repository;
            _registrationRepository = registrationRepository;
            _eventRepository = eventRepository;
            _accessService = accessService;
        }

        public AttendanceSheetModel GetAttendanceSheet(int eventId, CurrentUserModel currentUser)
        {
            var ev = _eventRepository.GetEvents()
                .Where(e => e.Id == eventId)
                .Select(EventProjection.ToViewModel(currentUser.AccountId))
                .FirstOrDefault()
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
            _accessService.EnsureCanManageOrganization(ev.OrganizationId, currentUser);
            ev.CanManage = true;

            var registrations = _registrationRepository.GetRegistrations()
                .Where(r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled)
                .OrderBy(r => r.Student.Name)
                .Select(RegistrationProjection.ToViewModel)
                .ToList();

            var attendees = registrations.Where(r => r.Status == RegistrationStatus.Registered).ToList();
            return new AttendanceSheetModel
            {
                Event = ev,
                Attendees = attendees,
                Waitlist = registrations.Where(r => r.Status == RegistrationStatus.Waitlisted)
                                        .OrderBy(r => r.RegisteredTime)
                                        .ToList(),
                PresentCount = attendees.Count(a => a.AttendanceStatus == AttendanceStatus.Present),
                LateCount = attendees.Count(a => a.AttendanceStatus == AttendanceStatus.Late),
                AbsentCount = attendees.Count(a => a.AttendanceStatus == AttendanceStatus.Absent),
                PendingCount = attendees.Count(a => a.AttendanceStatus == null),
            };
        }

        /// <summary>
        /// QR check-in at the door. The scanner page sends the scanned code (or a typed student number) and gets
        /// this result back as JSON. Problems return a message instead of throwing, so scanning can continue.
        /// </summary>
        public CheckInResultModel CheckIn(int eventId, string codeOrStudentNumber, CurrentUserModel currentUser)
        {
            // STEP 1: only an officer of this event's organization (or an admin), and only while the event is Open/Closed
            var ev = FindEvent(eventId);
            _accessService.EnsureCanManageOrganization(ev.OrganizationId, currentUser);
            EnsureAttendanceAllowed(ev);

            // STEP 2: check-in opens 2 hours before the start
            if (AppClock.Now < ev.StartTime.AddHours(-CheckInOpensHoursBefore))
            {
                return Fail(string.Format(Resources.Messages.Errors.CheckInNotOpenYet, CheckInOpensHoursBefore));
            }

            // STEP 3: find the registration for THIS event by its QR code, or by student number as a backup
            var input = (codeOrStudentNumber ?? string.Empty).Trim();
            var code = input.ToUpperInvariant();
            var registration = _registrationRepository.GetRegistrations()
                .Include(r => r.Student)
                .Include(r => r.Attendance)
                .FirstOrDefault(r => r.EventId == eventId
                                  && (r.RegistrationCode == code || r.Student.StudentNumber == input));

            if (registration == null)
            {
                return Fail(Resources.Messages.Errors.InvalidRegistrationCode);
            }

            var result = new CheckInResultModel
            {
                RegistrationId = registration.Id,
                StudentName = registration.Student.Name,
                StudentNumber = registration.Student.StudentNumber,
            };

            // STEP 4: waitlisted or cancelled passes can't check in
            if (registration.Status != RegistrationStatus.Registered)
            {
                result.Message = Resources.Messages.Errors.RegistrationNotActive;
                return result;
            }
            // STEP 5: a pass can't be used twice; show when they already checked in
            if (registration.Attendance?.CheckInTime != null)
            {
                result.Message = Resources.Messages.Errors.AlreadyCheckedIn;
                result.Status = registration.Attendance.Status.ToString();
                result.Time = registration.Attendance.CheckInTime.Value.ToString("hh:mm tt");
                return result;
            }
            // STEP 6: students registered before the overlap rule (or whose event was rescheduled) can't be present twice
            var elsewhere = ScheduleConflicts.CheckedInElsewhere(_registrationRepository.GetRegistrations(), registration.StudentId, eventId, ev.StartTime, ev.EndTime);
            if (elsewhere != null)
            {
                result.Message = string.Format(Resources.Messages.Errors.ScheduleConflictCheckIn, registration.Student.Name, elsewhere);
                return result;
            }

            // STEP 7: Present or Late. Late = checked in after the start time + the event's late-after minutes
            // (e.g. starts 1:00 PM with 15 minutes -> 1:16 PM is Late)
            var now = AppClock.Now;
            var status = now > ev.StartTime.AddMinutes(ev.LateAfterMinutes) ? AttendanceStatus.Late : AttendanceStatus.Present;
            // STEP 8: save the attendance row (one per registration), recording which officer scanned it
            var attendance = registration.Attendance;
            if (attendance == null)
            {
                _repository.AddAttendance(new Attendance
                {
                    RegistrationId = registration.Id,
                    CheckInTime = now,
                    Status = status,
                    RecordedBy = currentUser.UserId,
                    RecordedTime = now,
                });
            }
            else
            {
                // e.g. previously marked Absent manually and the student arrives after all
                attendance.CheckInTime = now;
                attendance.Status = status;
                attendance.RecordedBy = currentUser.UserId;
                attendance.RecordedTime = now;
                _repository.UpdateAttendance(attendance);
            }

            result.Success = true;
            result.Message = Resources.Messages.Common.CheckInSuccess;
            result.Status = status.ToString();
            result.Time = now.ToString("hh:mm tt");
            return result;
        }

        public void CheckOut(int registrationId, CurrentUserModel currentUser)
        {
            var registration = FindRegistration(registrationId);
            _accessService.EnsureCanManageOrganization(registration.Event.OrganizationId, currentUser);
            EnsureAttendanceAllowed(registration.Event);

            var attendance = registration.Attendance;
            if (attendance?.CheckInTime == null)
            {
                throw new InvalidDataException(Resources.Messages.Errors.NotCheckedIn);
            }

            attendance.CheckOutTime = AppClock.Now;
            attendance.RecordedBy = currentUser.UserId;
            attendance.RecordedTime = AppClock.Now;
            _repository.UpdateAttendance(attendance);
        }

        public void SetStatus(int registrationId, AttendanceStatus status, string remarks, CurrentUserModel currentUser)
        {
            var registration = FindRegistration(registrationId);
            _accessService.EnsureCanManageOrganization(registration.Event.OrganizationId, currentUser);
            if (registration.Event.Status == EventStatus.Draft || registration.Event.Status == EventStatus.Cancelled)
            {
                throw new InvalidDataException(Resources.Messages.Errors.AttendanceNotAllowed);
            }
            if (registration.Status != RegistrationStatus.Registered)
            {
                throw new InvalidDataException(Resources.Messages.Errors.RegistrationNotActive);
            }
            if ((remarks?.Trim().Length ?? 0) > RemarksMaxLength)
            {
                throw new InvalidDataException(Resources.Messages.Errors.RemarksTooLong);
            }

            var now = AppClock.Now;
            if (status != AttendanceStatus.Absent && now < registration.Event.StartTime.AddHours(-CheckInOpensHoursBefore))
            {
                throw new InvalidDataException(string.Format(Resources.Messages.Errors.CheckInNotOpenYet, CheckInOpensHoursBefore));
            }
            if (status != AttendanceStatus.Absent)
            {
                var elsewhere = ScheduleConflicts.CheckedInElsewhere(_registrationRepository.GetRegistrations(), registration.StudentId,
                                                                     registration.EventId, registration.Event.StartTime, registration.Event.EndTime);
                if (elsewhere != null)
                {
                    throw new InvalidDataException(string.Format(Resources.Messages.Errors.ScheduleConflictCheckIn, registration.Student.Name, elsewhere));
                }
            }
            var attendance = registration.Attendance;
            if (attendance == null)
            {
                _repository.AddAttendance(new Attendance
                {
                    RegistrationId = registration.Id,
                    CheckInTime = status == AttendanceStatus.Absent ? null : now,
                    Status = status,
                    Remarks = remarks?.Trim(),
                    RecordedBy = currentUser.UserId,
                    RecordedTime = now,
                });
                return;
            }

            attendance.Status = status;
            attendance.Remarks = remarks?.Trim();
            if (status == AttendanceStatus.Absent)
            {
                attendance.CheckInTime = null;
                attendance.CheckOutTime = null;
            }
            else if (attendance.CheckInTime == null)
            {
                attendance.CheckInTime = now;
            }
            attendance.RecordedBy = currentUser.UserId;
            attendance.RecordedTime = now;
            _repository.UpdateAttendance(attendance);
        }

        /// <summary>
        /// When an event is completed: every Registered student with no attendance row gets an Absent row,
        /// saved together in one batch. Called by ChangeStatus(Completed) and by the automatic status job.
        /// </summary>
        public void MarkAbsentees(int eventId, string actor)
        {
            var now = AppClock.Now;
            var absentees = _registrationRepository.GetRegistrations()
                .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Registered && r.Attendance == null)
                .Select(r => r.Id)
                .ToList()
                .Select(id => new Attendance
                {
                    RegistrationId = id,
                    Status = AttendanceStatus.Absent,
                    RecordedBy = actor,
                    RecordedTime = now,
                })
                .ToList();

            if (absentees.Count > 0)
            {
                _repository.AddAttendances(absentees);
            }
        }

        private Event FindEvent(int eventId)
        {
            return _eventRepository.GetEvents().FirstOrDefault(e => e.Id == eventId)
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
        }

        private EventRegistration FindRegistration(int registrationId)
        {
            return _registrationRepository.GetRegistrations()
                       .Include(r => r.Event)
                       .Include(r => r.Student)
                       .Include(r => r.Attendance)
                       .FirstOrDefault(r => r.Id == registrationId)
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
        }

        private static void EnsureAttendanceAllowed(Event ev)
        {
            if (ev.Status != EventStatus.Open && ev.Status != EventStatus.Closed)
            {
                throw new InvalidDataException(Resources.Messages.Errors.AttendanceNotAllowed);
            }
        }

        private static CheckInResultModel Fail(string message)
        {
            return new CheckInResultModel { Success = false, Message = message };
        }
    }
}
