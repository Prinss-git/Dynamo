using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    public class DashboardModel
    {
        public string Name { get; set; }
        public Role Role { get; set; }

        // Admin / Officer
        public int OrganizationCount { get; set; }
        public int StudentCount { get; set; }
        public int ActiveEventCount { get; set; }
        public int TotalRegistrations { get; set; }
        public decimal OverallAttendanceRate { get; set; }

        // Student
        public int MyUpcomingCount { get; set; }
        public int MyAttendedCount { get; set; }
        public int OpenEventCount { get; set; }
        public List<RegistrationViewModel> MyUpcoming { get; set; } = new();

        public List<EventViewModel> UpcomingEvents { get; set; } = new();
    }
}
