using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using ServerMonitoringServiceWorker.Common.Models;
using ServerMonitoringServiceWorker.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMonitoringServiceWorker.Services
{
    public class DriveService
    {
        private readonly Device _device;
        private readonly AppSettings _settings;

        public DriveService(Device device, IOptions<AppSettings> settings)
        {
            _device = device;
            _settings = settings.Value;
        }


        /// <summary>
        /// Send drives usage
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task<IFlurlResponse> UsageAsync(CancellationToken cancellationToken)
        {
            var drivesInfo = DriveInfo.GetDrives().Where(d => d.DriveType.Equals(DriveType.Fixed));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                drivesInfo = drivesInfo.Where(d => !d.Name.StartsWith("/sys/"));

            var drives = drivesInfo.Select(d => Drive.TransformFromDriveInfo(d));


            return await _settings.Server
                .AppendPathSegments(Program.BASE_URL_SEGMENT, "drives")
                .PostJsonAsync(new
                {
                    device = _device.Id,
                    drives
                }, cancellationToken);
        }
    }
}
