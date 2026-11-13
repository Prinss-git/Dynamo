using System;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// A "forgot password" request. An admin completes it by setting a temporary password.
    /// </summary>
    public partial class PasswordResetRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime RequestedTime { get; set; }
        public PasswordResetStatus Status { get; set; }
        public string HandledBy { get; set; }
        public DateTime? HandledTime { get; set; }

        public virtual User User { get; set; }
    }
}
