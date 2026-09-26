using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Student organizations and their members (Admin only)
    /// </summary>
    [Authorize(Roles = Const.RoleAdmin)]
    public class OrganizationController : ControllerBase<OrganizationController>
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IHttpContextAccessor httpContextAccessor,
                                      ILoggerFactory loggerFactory,
                                      IConfiguration configuration,
                                      IOrganizationService organizationService,
                                      IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        public IActionResult Index(string search)
        {
            ViewBag.Search = search;
            return View(_organizationService.GetOrganizations(search));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Form", new OrganizationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OrganizationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            try
            {
                _organizationService.CreateOrganization(model, UserId);
                NotifySuccess(Resources.Messages.Common.SaveSuccess);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Form", model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            return View("Form", _organizationService.GetOrganization(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OrganizationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            try
            {
                _organizationService.UpdateOrganization(model, UserId);
                NotifySuccess(Resources.Messages.Common.SaveSuccess);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Form", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _organizationService.DeleteOrganization(id);
            NotifySuccess(Resources.Messages.Common.DeleteSuccess);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Members(int id)
        {
            return View(_organizationService.GetMembers(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMember(int organizationId, int memberId, string position)
        {
            _organizationService.AddMember(organizationId, memberId, position);
            NotifySuccess(Resources.Messages.Common.SaveSuccess);
            return RedirectToAction(nameof(Members), new { id = organizationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateMember(int id, int organizationId, string position)
        {
            _organizationService.UpdateMemberPosition(id, position);
            NotifySuccess(Resources.Messages.Common.SaveSuccess);
            return RedirectToAction(nameof(Members), new { id = organizationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveMember(int id)
        {
            var organizationId = _organizationService.RemoveMember(id);
            NotifySuccess(Resources.Messages.Common.DeleteSuccess);
            return RedirectToAction(nameof(Members), new { id = organizationId });
        }
    }
}
