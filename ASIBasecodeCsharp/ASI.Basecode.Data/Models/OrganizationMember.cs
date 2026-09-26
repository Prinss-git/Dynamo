using System;

namespace ASI.Basecode.Data.Models
{
    public partial class OrganizationMember
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public int MemberId { get; set; }
        public string Position { get; set; }
        public DateTime JoinedTime { get; set; }

        public virtual Organization Organization { get; set; }
        public virtual User Member { get; set; }
    }
}
