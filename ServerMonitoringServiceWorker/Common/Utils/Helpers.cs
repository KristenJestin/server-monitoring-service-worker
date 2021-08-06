using DeviceId;
using System;

namespace ServerMonitoringServiceWorker.Common.Utils
{
    public static class Helpers
    {
        public static readonly Lazy<string> UniqueDeviceId = new(() => GetUniqueDeviceId());

        public static string GetUniqueDeviceId()
        {
            return new DeviceIdBuilder()
                .AddMachineName()
                .AddMacAddress()
                .AddProcessorId()
                .AddMotherboardSerialNumber()
                .ToString();
        }
    }
}
