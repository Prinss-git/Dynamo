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
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// User accounts and roles (Admin only)
    /// </summary>
    [Authorize(Roles = Const.RoleAdmin)]
    public class UserController : ControllerBase<UserController>
    {
        private readonly IUserService _userService;

        public UserController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IUserService userService,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index(string search, Role? role)
        {
            ViewBag.Search = search;
            ViewBag.Role = role;
            return View(_userService.GetUsers(search, role));
        }

        /// <summary>
        /// Self-registered accounts waiting for approval.
        /// </summary>
        [HttpGet]
        public IActionResult Approvals()
        {
            return View(_userService.GetPendingUsers());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int[] ids)
        {
            var count = _userService.ApproveUsers(ids, UserId);
            NotifySuccess(string.Format(Resources.Messages.Common.UsersApproved, count));
            return RedirectToAction(nameof(Approvals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveAll()
        {
            var count = _userService.ApproveAllUsers(UserId);
            NotifySuccess(string.Format(Resources.Messages.Common.UsersApproved, count));
            return RedirectToAction(nameof(Approvals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int[] ids)
        {
            var count = _userService.RejectUsers(ids);
            NotifySuccess(string.Format(Resources.Messages.Common.UsersRejected, count));
            return RedirectToAction(nameof(Approvals));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Form", new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password is required.");
            }
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            try
            {
                _userService.CreateUser(model, UserId);
                NotifySuccess(Resources.Messages.Common.SaveSuccess);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidDataException ex)
            {
                AddServiceError(ex.Message);
                return View("Form", model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            return View("Form", _userService.GetUser(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            try
            {
                _userService.UpdateUser(model, UserId);
                NotifySuccess(Resources.Messages.Common.SaveSuccess);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidDataException ex)
            {
                AddServiceError(ex.Message);
                return View("Form", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActive(int id)
        {
            if (id == CurrentUser.AccountId)
            {
                NotifyError(Resources.Messages.Errors.NotAuthorized);
                return RedirectToAction(nameof(Index));
            }

            _userService.ToggleActive(id, UserId);
            NotifySuccess(Resources.Messages.Common.StatusUpdated);
            return RedirectToAction(nameof(Index));
        }
    }
}
