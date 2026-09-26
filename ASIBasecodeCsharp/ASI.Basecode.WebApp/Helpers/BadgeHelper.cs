using Microsoft.AspNetCore.Html;
using System;
using System.Linq;
using System.Net;

namespace ASI.Basecode.WebApp.Helpers
{
    /// <summary>
    /// Renders the status tags and small text helpers used across the views.
    /// </summary>
    public static class BadgeHelper
    {
        public static IHtmlContent Badge(Enum status)
        {
            return Badge(status?.ToString());
        }

        public static IHtmlContent Badge(string text, string cssKey = null, string extraClass = null)
        {
            var label = string.IsNullOrEmpty(text) ? "Pending" : text;
            var key = (cssKey ?? label).ToLowerInvariant();
            var css = $"tag tag-{WebUtility.HtmlEncode(key)}" + (extraClass == null ? "" : " " + WebUtility.HtmlEncode(extraClass));
            return new HtmlString($"<span class=\"{css}\">{WebUtility.HtmlEncode(label)}</span>");
        }

        /// <summary>
        /// Initials for the monogram / ID photo slot, e.g. "Ana Cruz" -> "AC".
        /// </summary>
        public static string Initials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var letters = parts.Length == 1
                ? parts[0].Take(2)
                : new[] { parts.First()[0], parts.Last()[0] };
            return new string(letters.ToArray()).ToUpperInvariant();
        }
    }
}
