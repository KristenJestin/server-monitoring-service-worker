using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerMonitoringServiceWorker.Common.Utils;
using ServerMonitoringServiceWorker.Common.Models;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ServerMonitoringServiceWorker.Api;

namespace ServerMonitoringServiceWorker.Workers
{
    public class AliveWorker : BackgroundService
    {
        private readonly AliveService _service;
        private readonly AppSettings _settings;
        private readonly ILogger<AliveWorker> _logger;

        private bool StatusUpSent { get; set; }

        public AliveWorker(ILogger<AliveWorker> logger, IOptions<AppSettings> appSettings, AliveService service)
        {
            _service = service;
            _settings = appSettings.Value;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(AliveWorker)} start at: {DateTimeOffset.Now}");
            await Helpers.HandlingHttpRequestException(_service.StatusUpAsync, _logger, cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(AliveWorker)} running at: {DateTimeOffset.Now}");
                await Helpers.HandlingHttpRequestException(_service.AliveAsync, _logger, stoppingToken);

                // if status up failed before
                if (!StatusUpSent)
                    await Helpers.HandlingHttpRequestException(async (cancellationToken) =>
                    {
                        var result = await _service.StatusUpAsync(cancellationToken);
                        StatusUpSent = true;

                        return result;
                    }, _logger, stoppingToken);


                // wait
                await Task.Delay(_settings.AliveSettings.DurationGap, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(AliveWorker)} stop at: {DateTimeOffset.Now}");
            await Helpers.HandlingHttpRequestException(_service.StatusDownAsync, _logger, cancellationToken);

            await base.StartAsync(cancellationToken);
        }
    }
}
