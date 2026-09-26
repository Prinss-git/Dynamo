using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Attendance taking: QR / manual check-in, check-out and status overrides
    /// </summary>
    [Authorize(Roles = Const.RolesManagers)]
    public class AttendanceController : ControllerBase<AttendanceController>
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IHttpContextAccessor httpContextAccessor,
                                    ILoggerFactory loggerFactory,
                                    IConfiguration configuration,
                                    IAttendanceService attendanceService,
                                    IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _attendanceService = attendanceService;
        }

        [HttpGet]
        public IActionResult Index(int eventId)
        {
            return View(_attendanceService.GetAttendanceSheet(eventId, CurrentUser));
        }

        /// <summary>
        /// Called by the QR scanner / manual entry box (AJAX).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckIn(int eventId, string code)
        {
            return Json(_attendanceService.CheckIn(eventId, code, CurrentUser));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckOut(int id, int eventId)
        {
            _attendanceService.CheckOut(id, CurrentUser);
            NotifySuccess(Resources.Messages.Common.CheckOutSuccess);
            return RedirectToAction(nameof(Index), new { eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetStatus(int id, int eventId, AttendanceStatus status, string remarks)
        {
            _attendanceService.SetStatus(id, status, remarks, CurrentUser);
            NotifySuccess(Resources.Messages.Common.StatusUpdated);
            return RedirectToAction(nameof(Index), new { eventId });
        }
    }
}
