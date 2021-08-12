using DeviceId;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMonitoringServiceWorker.Common.Utils
{
    public static class Helpers
    {
        public static string GetUniqueDeviceId(bool test = false)
        {
            var testing = !test ? "" : "-testing";

            return new DeviceIdBuilder()
                .AddMachineName()
                .AddOsVersion()
                .AddOsVersion()
                .OnWindows(windows => windows
                    .AddProcessorId()
                    .AddMotherboardSerialNumber())
                .OnLinux(linux => linux
                    .AddMotherboardSerialNumber())
                .ToString() + testing;
        }

        public static async Task HandlingHttpRequestException(Func<CancellationToken, Task<IFlurlResponse>> request, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation($"new logger");
                await request(cancellationToken);
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.GetResponseJsonAsync();
                if (error != null)
                    logger.LogError($"Error returned from {ex.Call.Request.Url}");
                else
                    logger.LogError($"Error returned");
            }
        }
    }
}
