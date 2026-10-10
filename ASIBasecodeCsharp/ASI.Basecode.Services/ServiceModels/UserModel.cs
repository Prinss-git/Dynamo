using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ASI.Basecode.Resources.Constants;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// User record as managed by an administrator
    /// </summary>
    public class UserModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50)]
        [RegularExpression(Const.UsernamePattern, ErrorMessage = Const.UsernameRuleMessage)]
        [Display(Name = "Username")]
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

        /// <summary>Full display name (read-only; composed from the three name fields).</summary>
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
        [StringLength(50, ErrorMessage = "Password must be 50 characters or fewer.")]
        [RegularExpression(Const.PasswordPattern, ErrorMessage = Const.PasswordRuleMessage)]
        public string Password { get; set; }

        public DateTime CreatedTime { get; set; }

        public bool IsApproved { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Course) && !Courses.IsValid(Course))
            {
                yield return new ValidationResult(Resources.Messages.Errors.InvalidCourse, new[] { nameof(Course) });
            }
        }
    }
}
