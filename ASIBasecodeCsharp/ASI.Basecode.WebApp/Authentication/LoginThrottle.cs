using Microsoft.Extensions.Caching.Memory;
using System;

namespace ASI.Basecode.WebApp.Authentication
{
    /// <summary>
    /// Limits password guessing: after <see cref="MaxFailures"/> wrong passwords for one username within
    /// <see cref="Window"/>, that username is locked for <see cref="LockTime"/>. Kept in memory, so an app restart
    /// clears it (good enough for a single-server site).
    /// </summary>
    public class LoginThrottle
    {
        public const int MaxFailures = 5;
        public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan LockTime = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache _cache;

        public LoginThrottle(IMemoryCache cache)
        {
            _cache = cache;
        }

        private sealed class Entry
        {
            public int Failures;
            public DateTime? LockedUntilUtc;
        }

        private static string Key(string username) => "login-throttle:" + (username ?? string.Empty).Trim().ToLowerInvariant();

        /// <summary>True while the username is locked; <paramref name="minutesLeft"/> is rounded up.</summary>
        public bool IsLocked(string username, out int minutesLeft)
        {
            minutesLeft = 0;
            if (_cache.TryGetValue(Key(username), out Entry entry) && entry.LockedUntilUtc > DateTime.UtcNow)
            {
                minutesLeft = (int)Math.Ceiling((entry.LockedUntilUtc.Value - DateTime.UtcNow).TotalMinutes);
                return true;
            }
            return false;
        }

        public void RecordFailure(string username)
        {
            var key = Key(username);
            var entry = _cache.Get<Entry>(key) ?? new Entry();
            lock (entry)
            {
                entry.Failures++;
                if (entry.Failures >= MaxFailures)
                {
                    entry.LockedUntilUtc = DateTime.UtcNow.Add(LockTime);
                    entry.Failures = 0;
                }
            }
            _cache.Set(key, entry, entry.LockedUntilUtc.HasValue ? LockTime : Window);
        }

        public void Reset(string username)
        {
            _cache.Remove(Key(username));
        }
    }
}
