using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    public class CheckInResultModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? RegistrationId { get; set; }
        public string StudentName { get; set; }
        public string StudentNumber { get; set; }
        public string Status { get; set; }
        public string Time { get; set; }
    }
}
