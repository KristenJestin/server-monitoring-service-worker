using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerMonitoringServiceWorker.Common.Utils;
using ServerMonitoringServiceWorker.Models;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMonitoringServiceWorker.Workers
{
    public class AliveWorker : BackgroundService
    {
        private readonly ILogger<AliveWorker> _logger;
        private readonly AppSettings _settings;
        private readonly IHostEnvironment _env;

        private readonly string device;

        public AliveWorker(ILogger<AliveWorker> logger, IOptions<AppSettings> appSettings, IHostEnvironment env)
        {
            _logger = logger;
            _settings = appSettings.Value;
            _env = env;

            device = Helpers.GetUniqueDeviceId(!env.IsProduction());
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
                if (error != null)
                    _logger.LogError($"Error returned from {ex.Call.Request.Url}: {error.SomeDetails}");
                else
                    _logger.LogError($"Error returned");
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    var result = await _settings.Server
                        .AppendPathSegments("devices", "alive")
                        .PostJsonAsync(new
                        {
                            device,
                        });
                }
                catch (FlurlHttpException ex)
                {
                    var error = await ex.GetResponseJsonAsync();
                    if (error != null)
                        _logger.LogError($"Error returned from {ex.Call.Request.Url}: {error.SomeDetails}");
                    else
                        _logger.LogError($"Error returned");
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
                if (error != null)
                    _logger.LogError($"Error returned from {ex.Call.Request.Url}: {error.SomeDetails}");
                else
                    _logger.LogError($"Error returned");
            }

            await base.StopAsync(cancellationToken);
        }


        #region privates
        private async Task<IFlurlResponse> SendStatusAsync(string status)
        {
            var os = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            var result = Regex.Match(os, @"(windows|linux)", RegexOptions.IgnoreCase);

            return await _settings.Server
                .AppendPathSegments("devices", "status", status)
                .PostJsonAsync(new
                {
                    device,
                    name = Environment.MachineName,
                    os = result?.Value?.ToLower(),
                    osVersion = os
                });
        }
        #endregion
    }
}
