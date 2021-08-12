using System;

namespace ServerMonitoringServiceWorker.Common.Models
{
    public class AppSettings
    {
        public string Server { get; set; }
        public string ApiKey { get; set; }

        public AliveWorkerSettings AliveSettings { get; set; }
        public DriveWorkerSettings DriveSettings { get; set; }
    }


    public class AliveWorkerSettings : WorkerSettings { }
    public class DriveWorkerSettings : WorkerSettings { }

    public abstract class WorkerSettings
    {
        public TimeSpan DurationGap { get; set; }
    }
}
