using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Linq;
using System.Linq.Expressions;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Shared EF projection from Event to EventViewModel (with registration counts)
    /// </summary>
    internal static class EventProjection
    {
        public static Expression<Func<Event, EventViewModel>> ToViewModel(int accountId)
        {
            return e => new EventViewModel
            {
                Id = e.Id,
                OrganizationId = e.OrganizationId,
                OrganizationName = e.Organization.Name,
                Title = e.Title,
                Description = e.Description,
                Venue = e.Venue,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                RegistrationDeadline = e.RegistrationDeadline,
                Capacity = e.Capacity,
                LateAfterMinutes = e.LateAfterMinutes,
                Status = e.Status,
                RegisteredCount = e.Registrations.Count(r => r.Status == RegistrationStatus.Registered),
                WaitlistedCount = e.Registrations.Count(r => r.Status == RegistrationStatus.Waitlisted),
                AttendedCount = e.Registrations.Count(r => r.Attendance != null && r.Attendance.Status != AttendanceStatus.Absent),
                MyRegistrationId = e.Registrations.Where(r => r.StudentId == accountId).Select(r => (int?)r.Id).FirstOrDefault(),
                MyRegistrationStatus = e.Registrations.Where(r => r.StudentId == accountId).Select(r => (RegistrationStatus?)r.Status).FirstOrDefault(),
            };
        }
    }
}
