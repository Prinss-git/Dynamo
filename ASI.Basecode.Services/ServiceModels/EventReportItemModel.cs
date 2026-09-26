using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    public class EventReportItemModel
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string OrganizationName { get; set; }
        public DateTime StartTime { get; set; }
        public EventStatus Status { get; set; }
        public int Registered { get; set; }
        public int Present { get; set; }
        public int Late { get; set; }
        public int Absent { get; set; }

        public decimal AttendanceRate => Registered == 0 ? 0 : Math.Round((Present + Late) * 100m / Registered, 1);
    }
}
