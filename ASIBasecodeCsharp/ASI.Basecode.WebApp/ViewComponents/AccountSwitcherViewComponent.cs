using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace ASI.Basecode.WebApp.ViewComponents
{
    /// <summary>
    /// Development-only account switcher. Renders nothing outside the Development environment.
    /// </summary>
    public class AccountSwitcherViewComponent : ViewComponent
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IUserService _userService;

        public AccountSwitcherViewComponent(IWebHostEnvironment environment, IUserService userService)
        {
            _environment = environment;
            _userService = userService;
        }

        public IViewComponentResult Invoke()
        {
            if (!_environment.IsDevelopment())
            {
                return Content(string.Empty);
            }

            var users = _userService.GetUsers(null, null).Where(u => u.IsActive).ToList();
            ViewBag.CurrentAccountId = UserClaimsPrincipal.FindFirst(Const.ClaimAccountId)?.Value;
            return View(users);
        }
    }
}
