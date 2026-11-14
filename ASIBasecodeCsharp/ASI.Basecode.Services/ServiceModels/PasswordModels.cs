using ASI.Basecode.Resources.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// "Forgot password" form: the user proves who they are with their username and student number.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string UserId { get; set; }

        /// <summary>Students enter their student number; staff accounts (no student number) enter their email.</summary>
        [Required(ErrorMessage = "Student number or email is required.")]
        [StringLength(100)]
        [Display(Name = "Student number or email")]
        public string StudentNumberOrEmail { get; set; }
    }

    /// <summary>
    /// A new password typed twice: the admin's temporary password, or the user's own new password.
    /// </summary>
    public class NewPasswordModel
    {
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, ErrorMessage = "Password must be 50 characters or fewer.")]
        [RegularExpression(Const.PasswordPattern, ErrorMessage = Const.PasswordRuleMessage)]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirmation Password is required.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords don't match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// The signed-in user's own password change: the current password plus the new one typed twice.
    /// </summary>
    public class ChangePasswordModel : NewPasswordModel
    {
        [Required(ErrorMessage = "Current password is required.")]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }
    }

    /// <summary>
    /// A pending "forgot password" request, with the account it is for.
    /// </summary>
    public class PasswordResetRequestModel : NewPasswordModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string StudentNumber { get; set; }
        public string Course { get; set; }
        public int? YearLevel { get; set; }
        public string Email { get; set; }
        public DateTime RequestedTime { get; set; }
    }
}
