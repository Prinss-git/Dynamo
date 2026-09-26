using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Self-registration form (always creates a Student account)
    /// </summary>
    public class UserViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50)]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Student number is required.")]
        [StringLength(20)]
        [Display(Name = "Student Number")]
        public string StudentNumber { get; set; }

        [StringLength(100)]
        public string Course { get; set; }

        [Range(1, 6, ErrorMessage = "Year level must be between 1 and 6.")]
        [Display(Name = "Year Level")]
        public int? YearLevel { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirmation Password is required.")]
        [Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
