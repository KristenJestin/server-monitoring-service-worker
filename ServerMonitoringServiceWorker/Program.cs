using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServerMonitoringServiceWorker.Api;
using ServerMonitoringServiceWorker.Common.Models;
using ServerMonitoringServiceWorker.Common.Utils;
using ServerMonitoringServiceWorker.Services;
using ServerMonitoringServiceWorker.Workers;
using System;

namespace ServerMonitoringServiceWorker
{
    public class Program
    {
        public const string BASE_URL_SEGMENT = "devices";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    // data
                    var configuration = hostContext.Configuration;
                    var env = hostContext.HostingEnvironment;


                    // settings
                    var settingsSection = configuration.GetSection("AppSettings");
                    services.Configure<AppSettings>(settingsSection);

                    // http
                    var settings = settingsSection.Get<AppSettings>();
                    ConfigureHttpClient(settings);

                    // services
                    services.AddSingleton(Device.Build(env.IsProduction()));
                    services.AddTransient<AliveService>();
                    services.AddTransient<DriveService>();

                    // workers
                    services.AddHostedService<AliveWorker>();
                    services.AddHostedService<DriveWorker>();
                });


        #region privates
        private static void ConfigureHttpClient(AppSettings settings)
        {
            // variables
            var baseUrl = settings.Server;
            var apiKey = settings.ApiKey;

            // configures
            FlurlHttp.Configure(settings =>
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                settings.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
            });
            FlurlHttp.ConfigureClient(baseUrl, cli =>
            {
                cli
                    .WithHeaders(new
                    {
                        Accept = "application/json",
                        Authorization = "Bearer " + apiKey
                    })
                    .WithTimeout(5);
            });
        }
        #endregion
    }
}
