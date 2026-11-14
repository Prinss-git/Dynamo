using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using ASI.Basecode.Resources.Constants;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _environment;
        private readonly LoginThrottle _loginThrottle;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IUserService userService,
                            IWebHostEnvironment environment,
                            LoginThrottle loginThrottle) : base(httpContextAccessor, loggerFactory, configuration)
        {
            this._loginThrottle = loginThrottle;
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._userService = userService;
            this._environment = environment;
        }

        /// <summary>
        /// Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }

        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            this._session.SetString("HasSession", "Exist");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // too many wrong passwords for this username: refuse without checking the password at all
            if (_loginThrottle.IsLocked(model.UserId, out var minutesLeft))
            {
                TempData["ErrorMessage"] = string.Format(Resources.Messages.Errors.TooManyLoginAttempts, minutesLeft);
                return View(model);
            }

            User user = null;
            var loginResult = _userService.AuthenticateUser(model.UserId, model.Password, ref user);
            if (loginResult == LoginResult.Failed)
            {
                _loginThrottle.RecordFailure(model.UserId);
            }
            else
            {
                _loginThrottle.Reset(model.UserId);
            }

            if (loginResult == LoginResult.Success)
            {
                // 認証OK
                await this._signInManager.SignInAsync(user);
                this._session.SetString("UserName", user.Name);
                this._session.SetString("Role", user.Role.ToString());

                if (user.MustChangePassword)
                {
                    TempData["WarningMessage"] = Resources.Messages.Common.MustChangePassword;
                    return RedirectToAction(nameof(ChangePassword));
                }

                returnUrl ??= TempData["returnUrl"] as string;
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            // 認証NG
            TempData["ErrorMessage"] = loginResult switch
            {
                LoginResult.Pending => Resources.Messages.Errors.AccountPending,
                LoginResult.Inactive => Resources.Messages.Errors.AccountInactive,
                _ => Resources.Messages.Errors.InvalidLogin,
            };
            return View(model);
        }

        /// <summary>
        /// "Forgot password": the user enters their username and student number, and an administrator
        /// sets a temporary password for them. The same message shows whether or not the account exists.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _userService.RequestPasswordReset(model.UserId, model.StudentNumberOrEmail);
            TempData["SuccessMessage"] = Resources.Messages.Common.PasswordResetRequested;
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// The signed-in user chooses a new password. Required after an admin reset; also reachable from the account menu.
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewBag.Forced = User.HasClaim(c => c.Type == Const.ClaimMustChangePassword);
            return View(new ChangePasswordModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            ViewBag.Forced = User.HasClaim(c => c.Type == Const.ClaimMustChangePassword);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user;
            try
            {
                user = _userService.ChangeOwnPassword(CurrentUser.AccountId, model.CurrentPassword, model.Password);
            }
            catch (InvalidDataException ex)
            {
                var field = ex.Message == Resources.Messages.Errors.CurrentPasswordIncorrect ? nameof(model.CurrentPassword) : nameof(model.Password);
                ModelState.AddModelError(field, ex.Message);
                return View(model);
            }
            await this._signInManager.SignInAsync(user);   // refresh the cookie without the must-change claim
            NotifySuccess(Resources.Messages.Common.PasswordChanged);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _userService.AddUser(model);
                TempData["SuccessMessage"] = Resources.Messages.Common.RegisterPendingApproval;
                return RedirectToAction("Login", "Account");
            }
            catch(InvalidDataException ex)
            {
                AddServiceError(ex.Message);
            }
            catch(Exception ex)
            {
                this.HandleExceptionLog(ex, model.UserId);
                ModelState.AddModelError(string.Empty, Resources.Messages.Errors.ServerError);
            }
            return View(model);
        }

        /// <summary>
        /// Shown when a signed-in user opens a page their role cannot access.
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Development only: signs in as another account without a password so every role can be
        /// tried quickly. Returns 404 outside the Development environment.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwitchAccount(int id)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            var user = _userService.GetActiveUser(id);
            await this._signInManager.SignOutAsync();
            await this._signInManager.SignInAsync(user);
            this._session.SetString("UserName", user.Name);
            this._session.SetString("Role", user.Role.ToString());
            TempData["SuccessMessage"] = $"Signed in as {user.Name} ({user.Role}).";
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
