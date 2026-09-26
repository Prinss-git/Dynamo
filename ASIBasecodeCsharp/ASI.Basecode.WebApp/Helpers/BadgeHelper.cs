using Microsoft.AspNetCore.Html;
using System;
using System.Net;

namespace ASI.Basecode.WebApp.Helpers
{
    /// <summary>
    /// Renders the colored status pills used across the views.
    /// </summary>
    public static class BadgeHelper
    {
        public static IHtmlContent Badge(Enum status)
        {
            return Badge(status?.ToString());
        }

        public static IHtmlContent Badge(string text, string cssKey = null)
        {
            var label = string.IsNullOrEmpty(text) ? "Pending" : text;
            var key = (cssKey ?? label).ToLowerInvariant();
            return new HtmlString($"<span class=\"badge badge-status badge-{WebUtility.HtmlEncode(key)}\">{WebUtility.HtmlEncode(label)}</span>");
        }
    }
}
