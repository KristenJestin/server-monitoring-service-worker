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
                await request(cancellationToken);
            }
            catch (FlurlHttpException ex)
            {
                logger.LogError(ex, "Error when sending to " + (ex.Call?.Request?.Url ?? ""));
            }
        }
    }
}
