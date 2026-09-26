using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    public class OrganizationMembersModel
    {
        public OrganizationViewModel Organization { get; set; }
        public List<OrganizationMemberViewModel> Members { get; set; } = new();
        public List<OptionModel> AvailableUsers { get; set; } = new();
    }
}
