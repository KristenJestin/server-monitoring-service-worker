using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerMonitoringServiceWorker.Common.Utils;
using ServerMonitoringServiceWorker.Models;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMonitoringServiceWorker.Workers
{
    public class AliveWorker : BackgroundService
    {
        private readonly ILogger<AliveWorker> _logger;
        private readonly AppSettings _settings;

        public AliveWorker(ILogger<AliveWorker> logger, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _settings = appSettings.Value;
        }


        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await SendStatusAsync("up");
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.GetResponseJsonAsync();
                _logger.LogError($"Error returned from {ex.Call.Request.Url}: {error.SomeDetails}");
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    var result = await _settings.Server
                        .AppendPathSegments("devices", "alive")
                        .PostJsonAsync(new
                        {
                            device = Helpers.UniqueDeviceId.Value,
                        });
                }
                catch (FlurlHttpException ex)
                {
                    var error = await ex.GetResponseJsonAsync();
                    _logger.LogError($"Error returned from {ex.Call.Request.Url}: {error.SomeDetails}");
                }

                // wait
                await Task.Delay(_settings.AliveSettings.GetDurationGapInMilliseconds(), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await SendStatusAsync("down");
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.GetResponseJsonAsync();
                _logger.LogError($"Error returned from {ex.Call.Request.Url}: {error.SomeDetails}");
            }

            await base.StopAsync(cancellationToken);
        }


        #region privates
        private async Task<IFlurlResponse> SendStatusAsync(string status)
        {
            return await _settings.Server
                .AppendPathSegments("devices", "status", status)
                .PostJsonAsync(new
                {
                    device = Helpers.UniqueDeviceId.Value,
                });
        }
        #endregion
    }
}
