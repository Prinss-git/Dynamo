namespace ASI.Basecode.Resources.Constants
{
    /// <summary>
    /// Class for enumerated values
    /// </summary>
    public class Enums
    {
        /// <summary>
        /// API Result Status
        /// </summary>
        public enum Status
        {
            Success,
            Error,
            CustomErr,
        }

        /// <summary>
        /// Login Result
        /// </summary>
        public enum LoginResult
        {
            Success = 0,
            Failed = 1,
            Inactive = 2,
        }

        /// <summary>
        /// User Role
        /// </summary>
        public enum Role
        {
            Admin,
            Officer,
            Student,
        }

        /// <summary>
        /// Event Status
        /// </summary>
        public enum EventStatus
        {
            Draft,
            Open,
            Closed,
            Completed,
            Cancelled,
        }

        /// <summary>
        /// Event Registration Status
        /// </summary>
        public enum RegistrationStatus
        {
            Registered,
            Waitlisted,
            Cancelled,
        }

        /// <summary>
        /// Attendance Status
        /// </summary>
        public enum AttendanceStatus
        {
            Present,
            Late,
            Absent,
        }
    }
}
