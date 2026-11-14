using ASI.Basecode.Data.Models;
using System;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// A student can't be in two places at once: these queries find a student's other events whose time overlaps
    /// an event's. Two events overlap when each one starts before the other ends (one ending at 3:00 and the next
    /// starting at 3:00 do NOT overlap). Cancelled events and cancelled registrations don't count.
    /// </summary>
    internal static class ScheduleConflicts
    {
        /// <summary>The student's active (Registered or Waitlisted) registrations for other events overlapping start–end.</summary>
        public static IQueryable<EventRegistration> Overlapping(IQueryable<EventRegistration> registrations,
                                                                int studentId, int eventId, DateTime start, DateTime end)
        {
            return registrations.Where(r => r.StudentId == studentId
                                         && r.EventId != eventId
                                         && r.Status != RegistrationStatus.Cancelled
                                         && r.Event.Status != EventStatus.Cancelled
                                         && r.Event.StartTime < end
                                         && r.Event.EndTime > start);
        }

        /// <summary>Title of an overlapping event the student was already checked in to (present or late), or null.</summary>
        public static string CheckedInElsewhere(IQueryable<EventRegistration> registrations,
                                                int studentId, int eventId, DateTime start, DateTime end)
        {
            return Overlapping(registrations, studentId, eventId, start, end)
                .Where(r => r.Attendance != null && r.Attendance.CheckInTime != null && r.Attendance.Status != AttendanceStatus.Absent)
                .Select(r => r.Event.Title)
                .FirstOrDefault();
        }
    }
}
