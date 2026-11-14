using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Authentication
{
    /// <summary>
    /// Checks the sign-in cookie against the database on every request, so a change by an administrator takes
    /// effect immediately instead of when the cookie expires: the session ends if the account was deactivated
    /// or is no longer approved, its role changed, its password changed or was reset, or the
    /// "must change password" flag no longer matches.
    /// </summary>
    public static class SessionValidator
    {
        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            var principal = context.Principal;
            if (principal?.Identity?.IsAuthenticated != true) return;

            if (!int.TryParse(principal.FindFirst(Const.ClaimAccountId)?.Value, out var accountId))
            {
                await RejectAsync(context);
                return;
            }

            var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            var user = userService.GetSessionUser(accountId);
            var stillValid = user != null
                && user.IsActive
                && user.IsApproved
                && principal.FindFirst(ClaimTypes.Role)?.Value == user.Role.ToString()
                && principal.FindFirst(Const.ClaimPasswordStamp)?.Value == PasswordManager.Stamp(user.Password)
                && principal.HasClaim(c => c.Type == Const.ClaimMustChangePassword) == user.MustChangePassword;

            if (!stillValid)
            {
                await RejectAsync(context);
            }
        }

        private static async Task RejectAsync(CookieValidatePrincipalContext context)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(Const.AuthenticationScheme);
        }
    }
}
