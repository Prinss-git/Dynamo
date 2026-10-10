using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    public class HomeController : ControllerBase<HomeController>
    {
        private readonly IReportService _reportService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="reportService"></param>
        /// <param name="mapper"></param>
        public HomeController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IReportService reportService,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Returns the role-specific dashboard.
        /// </summary>
        /// <returns> Home View </returns>
        public IActionResult Index()
        {
            var model = _reportService.GetDashboard(CurrentUser, UserName);
            return View(model);
        }

        /// <summary>
        /// Shown for unhandled exceptions outside Development. The exception is logged here.
        /// </summary>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (feature?.Error != null)
            {
                _logger.LogError(feature.Error, "Unhandled error on {Path}", feature.Path);
            }
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Friendly page for 404 and other error status codes.
        /// </summary>
        [AllowAnonymous]
        public IActionResult Status(int code)
        {
            Response.StatusCode = code;
            ViewBag.Code = code;
            return View();
        }
    }
}
