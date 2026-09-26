using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Attendance reports and CSV exports
    /// </summary>
    [Authorize(Roles = Const.RolesManagers)]
    public class ReportController : ControllerBase<ReportController>
    {
        private readonly IReportService _reportService;
        private readonly IAttendanceService _attendanceService;
        private readonly IOrganizationService _organizationService;

        public ReportController(IHttpContextAccessor httpContextAccessor,
                                ILoggerFactory loggerFactory,
                                IConfiguration configuration,
                                IReportService reportService,
                                IAttendanceService attendanceService,
                                IOrganizationService organizationService,
                                IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _reportService = reportService;
            _attendanceService = attendanceService;
            _organizationService = organizationService;
        }

        [HttpGet]
        public IActionResult Index(int? organizationId)
        {
            ViewBag.OrganizationId = organizationId;
            ViewBag.Organizations = new SelectList(_organizationService.GetManagedOrganizationOptions(CurrentUser), "Id", "Text", organizationId);
            return View(_reportService.GetEventSummaries(CurrentUser, organizationId));
        }

        [HttpGet]
        public IActionResult Event(int id)
        {
            return View(_attendanceService.GetAttendanceSheet(id, CurrentUser));
        }

        [HttpGet]
        public IActionResult ExportEvent(int id)
        {
            var content = _reportService.ExportEventCsv(id, CurrentUser, out var fileName);
            return File(content, "text/csv", fileName);
        }

        [HttpGet]
        public IActionResult ExportSummary(int? organizationId)
        {
            var content = _reportService.ExportSummaryCsv(CurrentUser, organizationId);
            return File(content, "text/csv", $"event_summary_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }
    }
}
