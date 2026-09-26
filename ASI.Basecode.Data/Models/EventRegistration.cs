using System;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Models
{
    public partial class EventRegistration
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int StudentId { get; set; }
        public string RegistrationCode { get; set; }
        public RegistrationStatus Status { get; set; }
        public DateTime RegisteredTime { get; set; }
        public DateTime? CancelledTime { get; set; }

        public virtual Event Event { get; set; }
        public virtual User Student { get; set; }
        public virtual Attendance Attendance { get; set; }
    }
}
