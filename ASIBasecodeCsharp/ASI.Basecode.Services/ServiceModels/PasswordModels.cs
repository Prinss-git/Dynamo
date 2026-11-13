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

        [Required(ErrorMessage = "Student number is required.")]
        [StringLength(20)]
        [Display(Name = "Student Number")]
        public string StudentNumber { get; set; }
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
