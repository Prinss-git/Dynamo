using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    public class AttendanceSheetModel
    {
        public EventViewModel Event { get; set; }
        public List<RegistrationViewModel> Attendees { get; set; } = new();
        public List<RegistrationViewModel> Waitlist { get; set; } = new();

        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int PendingCount { get; set; }

        public decimal AttendanceRate => Attendees.Count == 0 ? 0 : Math.Round((PresentCount + LateCount) * 100m / Attendees.Count, 1);
    }
}
