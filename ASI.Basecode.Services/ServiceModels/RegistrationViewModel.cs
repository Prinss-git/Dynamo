using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    public class RegistrationViewModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public string OrganizationName { get; set; }
        public string Venue { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EventStatus EventStatus { get; set; }

        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentNumber { get; set; }
        public string Course { get; set; }
        public int? YearLevel { get; set; }

        public string RegistrationCode { get; set; }
        public RegistrationStatus Status { get; set; }
        public DateTime RegisteredTime { get; set; }

        public AttendanceStatus? AttendanceStatus { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Remarks { get; set; }

        /// <summary>QR code (inline SVG markup) of the registration code; only filled for tickets</summary>
        public string QrCodeSvg { get; set; }

        public bool CanCancel => Status != RegistrationStatus.Cancelled
                                 && AttendanceStatus == null
                                 && (EventStatus == EventStatus.Open || EventStatus == EventStatus.Closed)
                                 && DateTime.Now < StartTime;
    }
}
