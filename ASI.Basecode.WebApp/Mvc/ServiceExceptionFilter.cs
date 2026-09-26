using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;

namespace ASI.Basecode.WebApp.Mvc
{
    /// <summary>
    /// Turns business-rule exceptions thrown by the services into a toast message
    /// (or a JSON error for AJAX calls) instead of an error page.
    /// </summary>
    public class ServiceExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            if (exception is not (InvalidDataException or KeyNotFoundException or UnauthorizedAccessException))
            {
                return;
            }

            var request = context.HttpContext.Request;
            var isAjax = request.Headers["X-Requested-With"] == "XMLHttpRequest"
                         || request.Headers.Accept.ToString().Contains("application/json");
            if (isAjax)
            {
                context.Result = new JsonResult(new { success = false, message = exception.Message })
                {
                    StatusCode = exception is UnauthorizedAccessException ? StatusCodes.Status403Forbidden : StatusCodes.Status400BadRequest,
                };
                context.ExceptionHandled = true;
                return;
            }

            var tempData = context.HttpContext.RequestServices
                .GetRequiredService<ITempDataDictionaryFactory>()
                .GetTempData(context.HttpContext);
            tempData["ErrorMessage"] = exception.Message;

            // Go back to the page the user came from; fall back to the dashboard.
            var referer = request.Headers.Referer.ToString();
            var target = "/Home/Index";
            if (Uri.TryCreate(referer, UriKind.Absolute, out var refererUri)
                && refererUri.Host == request.Host.Host
                && refererUri.PathAndQuery != request.Path + request.QueryString)
            {
                target = refererUri.PathAndQuery;
            }

            context.Result = new RedirectResult(target);
            context.ExceptionHandled = true;
        }
    }
}
