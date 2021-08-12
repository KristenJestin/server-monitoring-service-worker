using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerMonitoringServiceWorker.Common.Utils;
using ServerMonitoringServiceWorker.Common.Models;
using System.Threading;
using System.Threading.Tasks;
using ServerMonitoringServiceWorker.Services;

namespace ServerMonitoringServiceWorker.Workers
{
    public class DriveWorker : BackgroundService
    {
        private readonly DriveService _service;
        private readonly AppSettings _settings;
        private readonly ILogger<DriveWorker> _logger;

        public DriveWorker(ILogger<DriveWorker> logger, IOptions<AppSettings> appSettings, DriveService service)
        {
            _service = service;
            _settings = appSettings.Value;
            _logger = logger;
        }


        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start");
            await Helpers.HandlingHttpRequestException(_service.UsageAsync, _logger, cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // wait
                await Task.Delay(_settings.DriveSettings.DurationGap, stoppingToken);

                _logger.LogInformation("Running");
                await Helpers.HandlingHttpRequestException(_service.UsageAsync, _logger, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stop");
            await Helpers.HandlingHttpRequestException(_service.UsageAsync, _logger, cancellationToken);

            await base.StartAsync(cancellationToken);
        }
    }
}
