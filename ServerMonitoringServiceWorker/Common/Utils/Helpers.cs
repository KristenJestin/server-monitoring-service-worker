using DeviceId;

namespace ServerMonitoringServiceWorker.Common.Utils
{
    public static class Helpers
    {
        //public static readonly Lazy<string> UniqueDeviceId = new(() => GetUniqueDeviceId());

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
    }
}
