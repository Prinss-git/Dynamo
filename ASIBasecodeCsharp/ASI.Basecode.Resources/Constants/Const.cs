namespace ASI.Basecode.Resources.Constants
{
    /// <summary>
    /// Class for variables with constant values
    /// </summary>
    public class Const
    {
        /// <summary>
        ///API result success
        /// </summary>
        public const string ApiResultSuccess = "Success";

        /// <summary>
        ///API result error
        /// </summary>
        public const string ApiResultError = "Error";

        /// <summary>
        /// System
        /// </summary>
        public const string System = "sys";

        /// <summary>
        /// Api Key Header Name
        /// </summary>
        public const string ApiKey = "X-Basecode-API-Key";

        /// <summary>
        /// authentication scheme Name
        /// </summary>
        public const string AuthenticationScheme = "ASI_Basecode";

        /// <summary>
        /// authentication Issuer
        /// </summary>
        public const string Issuer = "asi.basecode";

        /// <summary>
        /// Role names used by [Authorize(Roles = ...)]
        /// </summary>
        public const string RoleAdmin = "Admin";
        public const string RoleOfficer = "Officer";
        public const string RoleStudent = "Student";
        public const string RolesManagers = "Admin,Officer";

        /// <summary>
        /// Claim that holds the numeric primary key of the signed-in user
        /// </summary>
        public const string ClaimAccountId = "AccountId";

        /// <summary>
        /// Password policy: at least 8 characters with an uppercase letter, a lowercase letter,
        /// a number and a special character. Used by both server and client validation.
        /// </summary>
        public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$";
        public const string PasswordRuleMessage = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a special character.";

        /// <summary>Usernames: 4-50 letters, numbers, dots, underscores or hyphens.</summary>
        public const string UsernamePattern = @"^[A-Za-z0-9._-]{4,50}$";
        public const string UsernameRuleMessage = "Username must be 4-50 characters using letters, numbers, dots, underscores, or hyphens (no spaces).";
    }
}
