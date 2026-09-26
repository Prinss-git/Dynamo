using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Linq.Expressions;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Shared EF projection from EventRegistration to RegistrationViewModel
    /// </summary>
    internal static class RegistrationProjection
    {
        public static readonly Expression<Func<EventRegistration, RegistrationViewModel>> ToViewModel = r => new RegistrationViewModel
        {
            Id = r.Id,
            EventId = r.EventId,
            EventTitle = r.Event.Title,
            OrganizationName = r.Event.Organization.Name,
            Venue = r.Event.Venue,
            StartTime = r.Event.StartTime,
            EndTime = r.Event.EndTime,
            EventStatus = r.Event.Status,
            StudentId = r.StudentId,
            StudentName = r.Student.Name,
            StudentNumber = r.Student.StudentNumber,
            Course = r.Student.Course,
            YearLevel = r.Student.YearLevel,
            RegistrationCode = r.RegistrationCode,
            Status = r.Status,
            RegisteredTime = r.RegisteredTime,
            AttendanceStatus = r.Attendance != null ? r.Attendance.Status : (AttendanceStatus?)null,
            CheckInTime = r.Attendance != null ? r.Attendance.CheckInTime : null,
            CheckOutTime = r.Attendance != null ? r.Attendance.CheckOutTime : null,
            Remarks = r.Attendance != null ? r.Attendance.Remarks : null,
        };
    }
}
