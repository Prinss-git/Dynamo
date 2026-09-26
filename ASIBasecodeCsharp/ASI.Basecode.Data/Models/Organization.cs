using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
        public string Description { get; set; }
        public string Adviser { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
