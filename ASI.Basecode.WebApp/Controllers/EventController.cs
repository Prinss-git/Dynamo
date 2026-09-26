using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Browsing events (everyone) and managing them (Admin / Officer)
    /// </summary>
    public class EventController : ControllerBase<EventController>
    {
        private readonly IEventService _eventService;
        private readonly IOrganizationService _organizationService;

        public EventController(IHttpContextAccessor httpContextAccessor,
                               ILoggerFactory loggerFactory,
                               IConfiguration configuration,
                               IEventService eventService,
                               IOrganizationService organizationService,
                               IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _eventService = eventService;
            _organizationService = organizationService;
        }

        [HttpGet]
        public IActionResult Index(string search, bool showPast = false)
        {
            ViewBag.Search = search;
            ViewBag.ShowPast = showPast;
            return View(_eventService.GetPublishedEvents(CurrentUser, search, !showPast));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var model = _eventService.GetEvent(id, CurrentUser);
            ViewBag.StatusChanges = model.CanManage ? _eventService.GetAllowedStatusChanges(model.Status) : null;
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = Const.RolesManagers)]
        public IActionResult Manage(string search, EventStatus? status)
        {
            ViewBag.Search = search;
            ViewBag.Status = status;
            return View(_eventService.GetManagedEvents(CurrentUser, search, status));
        }

        [HttpGet]
        [Authorize(Roles = Const.RolesManagers)]
        public IActionResult Create()
        {
            LoadOrganizations();
            return View("Form", new EventViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Const.RolesManagers)]
        public IActionResult Create(EventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadOrganizations();
                return View("Form", model);
            }

            var id = _eventService.CreateEvent(model, CurrentUser);
            NotifySuccess(Resources.Messages.Common.SaveSuccess);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [Authorize(Roles = Const.RolesManagers)]
        public IActionResult Edit(int id)
        {
            var model = _eventService.GetEvent(id, CurrentUser);
            if (!model.CanManage)
            {
                throw new UnauthorizedAccessException(Resources.Messages.Errors.NotAuthorized);
            }

            LoadOrganizations();
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Const.RolesManagers)]
        public IActionResult Edit(EventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadOrganizations();
                return View("Form", model);
            }

            try
            {
                _eventService.UpdateEvent(model, CurrentUser);
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                LoadOrganizations();
                return View("Form", model);
            }

            NotifySuccess(Resources.Messages.Common.SaveSuccess);
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Const.RolesManagers)]
        public IActionResult Delete(int id)
        {
            _eventService.DeleteEvent(id, CurrentUser);
            NotifySuccess(Resources.Messages.Common.DeleteSuccess);
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Const.RolesManagers)]
        public IActionResult ChangeStatus(int id, EventStatus status)
        {
            _eventService.ChangeStatus(id, status, CurrentUser);
            NotifySuccess(status == EventStatus.Completed
                ? Resources.Messages.Common.AbsenteesMarked
                : Resources.Messages.Common.StatusUpdated);
            return RedirectToAction(nameof(Details), new { id });
        }

        private void LoadOrganizations()
        {
            ViewBag.Organizations = new SelectList(_organizationService.GetManagedOrganizationOptions(CurrentUser), "Id", "Text");
        }
    }
}
