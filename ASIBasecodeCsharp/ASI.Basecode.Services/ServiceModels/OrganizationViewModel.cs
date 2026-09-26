using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    public class OrganizationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Organization name is required.")]
        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Acronym { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Adviser { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public int MemberCount { get; set; }
        public int EventCount { get; set; }
    }
}
