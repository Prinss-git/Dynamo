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
    }
}
