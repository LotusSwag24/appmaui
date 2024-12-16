using Microsoft.Extensions.DependencyInjection;
using sbm.Server.Interfaces;

namespace sbm.Server.Services.BackgroundServices
{
    public class MySchedulerJob : CronBackgroundJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MySchedulerJob> _log;

        public MySchedulerJob(CronSettings<MySchedulerJob> settings, ILogger<MySchedulerJob> log, IServiceProvider serviceProvider)
            : base(settings.CronExpression, settings.TimeZone)
        {
            _log = log;
            _serviceProvider = serviceProvider;
        }

        protected override async Task DoWork(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var catalogService = scope.ServiceProvider.GetRequiredService<ICatalogService>();
                //await catalogService.PopulateAllCatalogsAsync().ConfigureAwait(false);
            }
            _log.LogInformation("Running... at {0}", DateTime.UtcNow);
        }
    }

}
