using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Configuration
{
    /// <summary>
    /// Background job that moves events along on their own every few minutes, so nobody has to remember to:
    /// an Open event is Closed once it starts, and an Open/Closed event is Completed (unmarked attendees become
    /// Absent) a day after it ends. See <see cref="IEventService.AdvanceEventStatuses"/>.
    /// </summary>
    public class EventStatusUpdater : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventStatusUpdater> _logger;

        public EventStatusUpdater(IServiceScopeFactory scopeFactory, ILogger<EventStatusUpdater> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // the event service and the database context are per-request (scoped), so make a scope per run
                    using var scope = _scopeFactory.CreateScope();
                    var changed = scope.ServiceProvider.GetRequiredService<IEventService>().AdvanceEventStatuses();
                    if (changed > 0)
                    {
                        _logger.LogInformation("Event status updater changed {Count} event(s).", changed);
                    }
                }
                catch (Exception ex)
                {
                    // never let one failed run (e.g. the database briefly unreachable) stop the job
                    _logger.LogError(ex, "Event status updater failed.");
                }

                try
                {
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
