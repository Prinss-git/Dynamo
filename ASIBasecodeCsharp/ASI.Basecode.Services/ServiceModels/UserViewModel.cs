using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ASI.Basecode.Resources.Constants;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Self-registration form (always creates a Student account)
    /// </summary>
    public class UserViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50)]
        [RegularExpression(Const.UsernamePattern, ErrorMessage = Const.UsernameRuleMessage)]
        public string UserId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "School email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address, e.g. name@school.edu.")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Student number is required.")]
        [StringLength(20)]
        [Display(Name = "Student Number")]
        public string StudentNumber { get; set; }

        [Required(ErrorMessage = "Course is required.")]
        [StringLength(100)]
        public string Course { get; set; }

        [Required(ErrorMessage = "Year level is required.")]
        [Range(1, 6, ErrorMessage = "Year level must be between 1 and 6.")]
        [Display(Name = "Year Level")]
        public int? YearLevel { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, ErrorMessage = "Password must be 50 characters or fewer.")]
        [RegularExpression(Const.PasswordPattern, ErrorMessage = Const.PasswordRuleMessage)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirmation Password is required.")]
        [Compare("Password", ErrorMessage = "Passwords don't match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Course) && !Courses.IsValid(Course))
            {
                yield return new ValidationResult(Resources.Messages.Errors.InvalidCourse, new[] { nameof(Course) });
            }
        }
    }
}
