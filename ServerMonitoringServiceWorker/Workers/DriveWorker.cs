using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServerMonitoringServiceWorker.Common.Utils;
using ServerMonitoringServiceWorker.Data;
using ServerMonitoringServiceWorker.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
            _logger.LogInformation($"{nameof(DriveWorker)} start at: {DateTimeOffset.Now}");
            await Helpers.HandlingHttpRequestException(_service.UsageAsync, _logger, cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // wait
                await Task.Delay(_settings.DriveSettings.DurationGap, stoppingToken);

                _logger.LogInformation($"{nameof(DriveWorker)} running at: {DateTimeOffset.Now}");
                await Helpers.HandlingHttpRequestException(_service.UsageAsync, _logger, stoppingToken); _logger.LogError($"Error returned");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(DriveWorker)} stop at: {DateTimeOffset.Now}");
            await Helpers.HandlingHttpRequestException(_service.UsageAsync, _logger, cancellationToken);

            await base.StartAsync(cancellationToken);
        }
    }
}
