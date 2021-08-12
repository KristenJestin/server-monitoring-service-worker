using ServerMonitoringServiceWorker.Common.Utils;

namespace ServerMonitoringServiceWorker.Common.Models
{
    public class Device
    {
        public string Id{ get; set; }


        #region statics
        public static Device Build(bool production)
        {
            return new Device
            {
                Id = Helpers.GetUniqueDeviceId(!production)
            };
        }
        #endregion
    }
}
