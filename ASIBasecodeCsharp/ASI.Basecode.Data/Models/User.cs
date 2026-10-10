using System;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        /// <summary>Full display name, composed from first, middle and last name.</summary>
        public string Name { get; set; }
        public string Email { get; set; }
        public string StudentNumber { get; set; }
        public string Course { get; set; }
        public int? YearLevel { get; set; }
        public Role Role { get; set; }
        public bool IsActive { get; set; }
        /// <summary>Self-registered accounts stay unapproved until an admin approves them.</summary>
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedTime { get; set; }
        public string Password { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual ICollection<OrganizationMember> Memberships { get; set; } = new List<OrganizationMember>();
        public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    }
}
