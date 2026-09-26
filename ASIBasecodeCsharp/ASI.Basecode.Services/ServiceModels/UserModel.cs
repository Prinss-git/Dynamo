using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// User record as managed by an administrator
    /// </summary>
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Student Number")]
        public string StudentNumber { get; set; }

        [StringLength(100)]
        public string Course { get; set; }

        [Range(1, 6, ErrorMessage = "Year level must be between 1 and 6.")]
        [Display(Name = "Year Level")]
        public int? YearLevel { get; set; }

        [Required]
        public Role Role { get; set; } = Role.Student;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>Required when creating; leave blank on edit to keep the current password.</summary>
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
