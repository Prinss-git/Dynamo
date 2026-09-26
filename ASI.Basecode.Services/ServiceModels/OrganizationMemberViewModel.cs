using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    public class OrganizationMemberViewModel
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public int MemberId { get; set; }
        public string UserId { get; set; }
        public string MemberName { get; set; }
        public string StudentNumber { get; set; }
        public Role Role { get; set; }
        public string Position { get; set; }
        public DateTime JoinedTime { get; set; }
    }
}
