using System;

namespace ServerMonitoringServiceWorker.Models
{
    public class AppSettings
    {
        public string Server { get; set; }
        public AliveWorkerSettings AliveSettings { get; set; }
    }

    public class AliveWorkerSettings
    {
        public TimeSpan DurationGap { get; set; }


        #region methods
        public int GetDurationGapInMilliseconds()
            => (int)DurationGap.TotalMilliseconds;
        #endregion
    }
}
