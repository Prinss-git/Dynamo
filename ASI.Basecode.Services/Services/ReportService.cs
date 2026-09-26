using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class ReportService : IReportService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventRegistrationRepository _registrationRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAttendanceService _attendanceService;
        private readonly IAccessService _accessService;

        public ReportService(IEventRepository eventRepository,
                             IEventRegistrationRepository registrationRepository,
                             IOrganizationRepository organizationRepository,
                             IUserRepository userRepository,
                             IAttendanceService attendanceService,
                             IAccessService accessService)
        {
            _eventRepository = eventRepository;
            _registrationRepository = registrationRepository;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _attendanceService = attendanceService;
            _accessService = accessService;
        }

        public List<EventReportItemModel> GetEventSummaries(CurrentUserModel currentUser, int? organizationId)
        {
            var organizationIds = _accessService.GetManagedOrganizationIds(currentUser);
            var query = _eventRepository.GetEvents()
                .Where(e => organizationIds.Contains(e.OrganizationId) && e.Status != EventStatus.Draft);
            if (organizationId.HasValue)
            {
                query = query.Where(e => e.OrganizationId == organizationId.Value);
            }

            return query.OrderByDescending(e => e.StartTime)
                .Select(e => new EventReportItemModel
                {
                    EventId = e.Id,
                    Title = e.Title,
                    OrganizationName = e.Organization.Name,
                    StartTime = e.StartTime,
                    Status = e.Status,
                    Registered = e.Registrations.Count(r => r.Status == RegistrationStatus.Registered),
                    Present = e.Registrations.Count(r => r.Status == RegistrationStatus.Registered && r.Attendance != null && r.Attendance.Status == AttendanceStatus.Present),
                    Late = e.Registrations.Count(r => r.Status == RegistrationStatus.Registered && r.Attendance != null && r.Attendance.Status == AttendanceStatus.Late),
                    Absent = e.Registrations.Count(r => r.Status == RegistrationStatus.Registered && r.Attendance != null && r.Attendance.Status == AttendanceStatus.Absent),
                })
                .ToList();
        }

        public byte[] ExportEventCsv(int eventId, CurrentUserModel currentUser, out string fileName)
        {
            var sheet = _attendanceService.GetAttendanceSheet(eventId, currentUser);
            fileName = $"attendance_{eventId}_{DateTime.Now:yyyyMMdd_HHmm}.csv";

            var rows = sheet.Attendees.Select(a => new
            {
                StudentNumber = a.StudentNumber,
                Name = a.StudentName,
                Course = a.Course,
                YearLevel = a.YearLevel,
                RegistrationCode = a.RegistrationCode,
                RegisteredTime = a.RegisteredTime.ToString("yyyy-MM-dd HH:mm"),
                Attendance = a.AttendanceStatus?.ToString() ?? "Not yet recorded",
                CheckIn = a.CheckInTime?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
                CheckOut = a.CheckOutTime?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
                Remarks = a.Remarks,
            });
            return WriteCsv(rows);
        }

        public byte[] ExportSummaryCsv(CurrentUserModel currentUser, int? organizationId)
        {
            var rows = GetEventSummaries(currentUser, organizationId).Select(s => new
            {
                Event = s.Title,
                Organization = s.OrganizationName,
                Date = s.StartTime.ToString("yyyy-MM-dd HH:mm"),
                Status = s.Status.ToString(),
                s.Registered,
                s.Present,
                s.Late,
                s.Absent,
                AttendanceRatePercent = s.AttendanceRate,
            });
            return WriteCsv(rows);
        }

        public DashboardModel GetDashboard(CurrentUserModel currentUser, string name)
        {
            var now = DateTime.Now;
            var model = new DashboardModel { Name = name, Role = currentUser.Role };

            if (currentUser.Role == Role.Student)
            {
                var mine = _registrationRepository.GetRegistrations()
                    .Where(r => r.StudentId == currentUser.AccountId);

                model.MyUpcoming = mine
                    .Where(r => r.Status != RegistrationStatus.Cancelled && r.Event.EndTime >= now
                             && r.Event.Status != EventStatus.Cancelled)
                    .OrderBy(r => r.Event.StartTime)
                    .Select(RegistrationProjection.ToViewModel)
                    .ToList();
                model.MyUpcomingCount = model.MyUpcoming.Count;
                model.MyAttendedCount = mine.Count(r => r.Attendance != null && r.Attendance.Status != AttendanceStatus.Absent);
                model.OpenEventCount = _eventRepository.GetEvents()
                    .Count(e => e.Status == EventStatus.Open && e.RegistrationDeadline >= now && e.StartTime > now);
                model.UpcomingEvents = _eventRepository.GetEvents()
                    .Where(e => e.Status == EventStatus.Open && e.RegistrationDeadline >= now && e.StartTime > now)
                    .OrderBy(e => e.StartTime)
                    .Take(5)
                    .Select(EventProjection.ToViewModel(currentUser.AccountId))
                    .ToList();
                return model;
            }

            var organizationIds = _accessService.GetManagedOrganizationIds(currentUser);
            var events = _eventRepository.GetEvents().Where(e => organizationIds.Contains(e.OrganizationId));

            model.OrganizationCount = organizationIds.Count;
            model.StudentCount = currentUser.Role == Role.Admin
                ? _userRepository.GetUsers().Count(u => u.Role == Role.Student && u.IsActive)
                : _organizationRepository.GetMembers().Where(m => organizationIds.Contains(m.OrganizationId))
                                         .Select(m => m.MemberId).Distinct().Count();
            model.ActiveEventCount = events.Count(e => e.Status == EventStatus.Open || e.Status == EventStatus.Closed);

            var registrations = _registrationRepository.GetRegistrations()
                .Where(r => organizationIds.Contains(r.Event.OrganizationId) && r.Status == RegistrationStatus.Registered);
            model.TotalRegistrations = registrations.Count();

            var recorded = registrations.Count(r => r.Attendance != null);
            var attended = registrations.Count(r => r.Attendance != null && r.Attendance.Status != AttendanceStatus.Absent);
            model.OverallAttendanceRate = recorded == 0 ? 0 : Math.Round(attended * 100m / recorded, 1);

            model.UpcomingEvents = events
                .Where(e => e.EndTime >= now && e.Status != EventStatus.Cancelled && e.Status != EventStatus.Completed)
                .OrderBy(e => e.StartTime)
                .Take(5)
                .Select(EventProjection.ToViewModel(currentUser.AccountId))
                .ToList();
            model.UpcomingEvents.ForEach(e => e.CanManage = true);
            return model;
        }

        private static byte[] WriteCsv<T>(IEnumerable<T> rows)
        {
            using var memoryStream = new MemoryStream();
            // UTF-8 with BOM so Excel opens names with accents correctly.
            using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true)))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rows);
            }
            return memoryStream.ToArray();
        }
    }
}
