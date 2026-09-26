using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// The signed-in user, passed from controllers to services for permission checks
    /// </summary>
    public class CurrentUserModel
    {
        public int AccountId { get; set; }
        public string UserId { get; set; }
        public Role Role { get; set; }

        public bool IsAdmin => Role == Role.Admin;
        public bool IsStudent => Role == Role.Student;
    }
}
