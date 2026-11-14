using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
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
                              IUserService userService) : base(httpContextAccessor, loggerFactory, configuration)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index(string search, Role? role, string department, string course, string status, string sort)
        {
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Department = department;
            ViewBag.Course = course;
            ViewBag.UserStatus = status;
            ViewBag.Sort = sort;
            bool? active = status == "active" ? true : status == "inactive" ? false : null;
            return View(_userService.GetUsers(search, role, department, course, active, sort));
        }

        /// <summary>
        /// Self-registered accounts waiting for approval, filterable by department, year level and search.
        /// </summary>
        [HttpGet]
        public IActionResult Approvals(PendingUserFilter filter)
        {
            filter ??= new PendingUserFilter();
            return View(new PendingApprovalsModel
            {
                Filter = filter,
                Users = _userService.GetPendingUsers(filter),
                TotalPending = _userService.CountPendingUsers(),
                DepartmentCounts = _userService.CountPendingUsersByDepartment(),
                ResetRequests = _userService.GetPendingResets(),
            });
        }

        /// <summary>
        /// Sets a temporary password for a "forgot password" request.
        /// </summary>
        [HttpGet]
        public IActionResult ResetPassword(int id)
        {
            return View(_userService.GetPendingReset(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(PasswordResetRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                var request = _userService.GetPendingReset(model.Id);
                return View(request);
            }

            var username = _userService.CompletePasswordReset(model.Id, model.Password, UserId);
            NotifySuccess(string.Format(Resources.Messages.Common.PasswordResetCompleted, username));
            return RedirectToAction(nameof(Approvals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DismissReset(int id)
        {
            _userService.DismissPasswordReset(id, UserId);
            NotifySuccess(Resources.Messages.Common.PasswordResetDismissed);
            return RedirectToAction(nameof(Approvals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int[] ids, PendingUserFilter filter)
        {
            var count = _userService.ApproveUsers(ids, UserId);
            NotifySuccess(string.Format(Resources.Messages.Common.UsersApproved, count));
            return BackToApprovals(filter);
        }

        /// <summary>
        /// Approves every account the filtered list showed (ids up to <paramref name="throughId"/>).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveAll(PendingUserFilter filter, int throughId)
        {
            var count = _userService.ApproveAllUsers(filter, throughId, UserId);
            NotifySuccess(string.Format(Resources.Messages.Common.UsersApproved, count));
            return BackToApprovals(filter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int[] ids, PendingUserFilter filter)
        {
            var count = _userService.RejectUsers(ids);
            NotifySuccess(string.Format(Resources.Messages.Common.UsersRejected, count));
            return BackToApprovals(filter);
        }

        /// <summary>
        /// Rejects (deletes) every request the filtered list showed (ids up to <paramref name="throughId"/>).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectAll(PendingUserFilter filter, int throughId)
        {
            var count = _userService.RejectAllUsers(filter, throughId);
            NotifySuccess(string.Format(Resources.Messages.Common.UsersRejected, count));
            return BackToApprovals(filter);
        }

        /// <summary>Returns to the approvals list with the same filters still applied.</summary>
        private IActionResult BackToApprovals(PendingUserFilter filter)
        {
            filter ??= new PendingUserFilter();
            return RedirectToAction(nameof(Approvals), new
            {
                filter.Department,
                filter.Course,
                filter.YearLevel,
                filter.Search,
                filter.Sort,
            });
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
            // an admin can't deactivate their own account or change their own role (that could lock everyone out)
            if (model.Id == CurrentUser.AccountId && (!model.IsActive || model.Role != CurrentUser.Role))
            {
                ModelState.AddModelError(string.Empty, Resources.Messages.Errors.SelfAccountChange);
            }
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
