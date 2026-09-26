using System;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Models
{
    public partial class Event
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Venue { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public int Capacity { get; set; }
        public int LateAfterMinutes { get; set; }
        public EventStatus Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual Organization Organization { get; set; }
        public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    }
}
