using System;

namespace ASI.Basecode.Resources.Constants
{
    /// <summary>
    /// The current date and time in the Philippines (UTC+8), whatever time zone the web server runs in.
    /// Every event time in the database is Philippine time, so all "now" comparisons must use this clock
    /// instead of DateTime.Now (which is the server's time zone; the hosting server may be in another country).
    /// </summary>
    public static class AppClock
    {
        private static readonly TimeZoneInfo Zone = FindZone();

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);

        public static DateTime Today => Now.Date;

        private static TimeZoneInfo FindZone()
        {
            // Windows id first, then the IANA id (Linux), then a fixed +8 offset (the Philippines has no daylight saving)
            foreach (var id in new[] { "Singapore Standard Time", "Asia/Manila" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return TimeZoneInfo.CreateCustomTimeZone("Philippine Time", TimeSpan.FromHours(8), "Philippine Time", "Philippine Time");
        }
    }
}
