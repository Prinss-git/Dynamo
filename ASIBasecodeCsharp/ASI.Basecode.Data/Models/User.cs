using System;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string StudentNumber { get; set; }
        public string Course { get; set; }
        public int? YearLevel { get; set; }
        public Role Role { get; set; }
        public bool IsActive { get; set; }
        public string Password { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual ICollection<OrganizationMember> Memberships { get; set; } = new List<OrganizationMember>();
        public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    }
}
