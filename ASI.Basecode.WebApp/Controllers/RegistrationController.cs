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
    /// Student event registration, cancellation and QR tickets
    /// </summary>
    public class RegistrationController : ControllerBase<RegistrationController>
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(IHttpContextAccessor httpContextAccessor,
                                      ILoggerFactory loggerFactory,
                                      IConfiguration configuration,
                                      IRegistrationService registrationService,
                                      IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _registrationService = registrationService;
        }

        [HttpGet]
        [Authorize(Roles = Const.RoleStudent)]
        public IActionResult MyEvents()
        {
            return View(_registrationService.GetMyRegistrations(CurrentUser.AccountId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Const.RoleStudent)]
        public IActionResult Register(int eventId)
        {
            var status = _registrationService.Register(eventId, CurrentUser);
            if (status == RegistrationStatus.Waitlisted)
            {
                TempData["WarningMessage"] = Resources.Messages.Common.EventWaitlisted;
            }
            else
            {
                NotifySuccess(Resources.Messages.Common.EventRegisterSuccess);
            }
            return RedirectToAction("Details", "Event", new { id = eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id, string returnUrl)
        {
            _registrationService.Cancel(id, CurrentUser);
            NotifySuccess(Resources.Messages.Common.RegistrationCancelled);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(MyEvents));
        }

        [HttpGet]
        public IActionResult Ticket(int id)
        {
            return View(_registrationService.GetTicket(id, CurrentUser));
        }
    }
}
