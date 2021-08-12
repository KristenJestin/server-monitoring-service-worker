using System.IO;

namespace ServerMonitoringServiceWorker.Data
{
    public class Drive
    {   
        public long AvailableFreeSpace { get; set; }
        public string DriveFormat { get; set; }
        public string DriveType { get; set; }
        public bool IsReady { get; set; }
        public string Name { get; set; }
        public long TotalFreeSpace { get; set; }
        public long TotalSize { get; set; }
        public string VolumeLabel { get; set; }


        #region statics
        public static Drive TransformFromDriveInfo(DriveInfo info)
        {
            return new Drive
            {
                AvailableFreeSpace = info.AvailableFreeSpace,
                DriveFormat = info.DriveFormat,
                DriveType = info.DriveType.ToString(),
                IsReady = info.IsReady,
                Name = info.Name,
                TotalFreeSpace = info.TotalFreeSpace,
                TotalSize = info.TotalSize,
                VolumeLabel = info.VolumeLabel
            };
        }
        #endregion
    }
}
