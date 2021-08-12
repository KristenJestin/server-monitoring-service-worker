using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using ServerMonitoringServiceWorker.Common.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMonitoringServiceWorker.Api
{
    public class AliveService
    {
        private readonly Device _device;
        private readonly AppSettings _settings;

        public AliveService(Device device, IOptions<AppSettings> settings)
        {
            _device = device;
            _settings = settings.Value;
        }


        /// <summary>
        /// Sends a request to the server to let it know it's up
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task<IFlurlResponse> StatusUpAsync(CancellationToken cancellationToken)
            => await StatusAsync("up", cancellationToken);

        /// <summary>
        /// Sends a request to the server to let it know it's down
        /// </summary>
        public async Task<IFlurlResponse> StatusDownAsync(CancellationToken cancellationToken)
            => await StatusAsync("down", cancellationToken);

        /// <summary>
        /// Sends a request to the server to let it know it's down
        /// </summary>
        public async Task<IFlurlResponse> AliveAsync(CancellationToken cancellationToken)
            => await _settings.Server
                .AppendPathSegments(Program.BASE_URL_SEGMENT, "alive")
                .PostJsonAsync(new
                {
                    device = _device.Id,
                }, cancellationToken);


        #region privates
        private async Task<IFlurlResponse> StatusAsync(string status, CancellationToken cancellationToken)
        {
            var os = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            var result = Regex.Match(os, @"(windows|linux)", RegexOptions.IgnoreCase);

            return await _settings.Server
                .AppendPathSegments(Program.BASE_URL_SEGMENT, "status", status)
                .PostJsonAsync(new
                {
                    device = _device.Id,
                    name = Environment.MachineName,
                    os = result?.Value?.ToLower(),
                    osVersion = os
                }, cancellationToken);
        }
        #endregion
    }
}
