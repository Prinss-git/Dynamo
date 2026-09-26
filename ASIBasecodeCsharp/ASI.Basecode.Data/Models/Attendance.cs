using System;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Models
{
    public partial class Attendance
    {
        public int Id { get; set; }
        public int RegistrationId { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public AttendanceStatus Status { get; set; }
        public string Remarks { get; set; }
        public string RecordedBy { get; set; }
        public DateTime RecordedTime { get; set; }

        public virtual EventRegistration Registration { get; set; }
    }
}
