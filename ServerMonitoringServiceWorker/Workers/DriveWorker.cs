using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServerMonitoringServiceWorker.Common.Utils;
using ServerMonitoringServiceWorker.Data;
using ServerMonitoringServiceWorker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMonitoringServiceWorker.Workers
{
    public class DriveWorker : BackgroundService
    {

        private readonly ILogger<DriveWorker> _logger;
        private readonly AppSettings _settings;
        private readonly IHostEnvironment _env;

        private readonly string device;

        public DriveWorker(ILogger<DriveWorker> logger, IOptions<AppSettings> appSettings, IHostEnvironment env)
        {
            _logger = logger;
            _settings = appSettings.Value;
            _env = env;

            device = Helpers.GetUniqueDeviceId(!env.IsProduction());
        }

        #region workers
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await SendDrivesAsync(cancellationToken);
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.GetResponseJsonAsync();
                if (error != null)
                    _logger.LogError($"Error returned from {ex.Call.Request.Url}");
                else
                    _logger.LogError($"Error returned");
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // wait
                await Task.Delay(_settings.DriveSettings.GetDurationGapInMilliseconds(), stoppingToken);

                _logger.LogInformation($"{nameof(DriveWorker)} running at: {DateTimeOffset.Now}");

                try
                {
                    await SendDrivesAsync(stoppingToken);
                }
                catch (FlurlHttpException ex)
                {
                    var error = await ex.GetResponseJsonAsync();
                    if (error != null)
                        _logger.LogError($"Error returned from {ex.Call.Request.Url}");
                    else
                        _logger.LogError($"Error returned");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await SendDrivesAsync(cancellationToken);
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.GetResponseJsonAsync();
                if (error != null)
                    _logger.LogError($"Error returned from {ex.Call.Request.Url}");
                else
                    _logger.LogError($"Error returned");
            }

            await base.StartAsync(cancellationToken);
        }
        #endregion

        #region privates
        private async Task<IFlurlResponse> SendDrivesAsync(CancellationToken cancellationToken)
        {
            return await _settings.Server
                .AppendPathSegments("devices", "drives")
                .PostJsonAsync(new
                {
                    device,
                    drives = GetDrives()
                }, cancellationToken);
        }

        private static IEnumerable<Drive> GetDrives()
        {
            var drives = DriveInfo.GetDrives().Where(d => d.DriveType.Equals(DriveType.Fixed));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                drives = drives.Where(d => !d.Name.StartsWith("/sys/"));

            return drives.Select(d => Drive.TransformFromInfo(d));
        }
        #endregion
    }
}
