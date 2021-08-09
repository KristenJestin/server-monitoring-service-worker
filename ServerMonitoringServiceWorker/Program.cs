using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServerMonitoringServiceWorker.Models;
using ServerMonitoringServiceWorker.Workers;
using System;

namespace ServerMonitoringServiceWorker
{
    public class Program
    {
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

                    // settings
                    var settingsSection = configuration.GetSection("AppSettings");
                    services.Configure<AppSettings>(settingsSection);

                    // http
                    ConfigureHttpClient(settingsSection.Get<AppSettings>().Server);

                    // workers
                    services.AddHostedService<AliveWorker>();
                    services.AddHostedService<DriveWorker>();
                });


        #region privates
        public static void ConfigureHttpClient(string baseUrl)
        {
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
                    .WithHeader("Accept", "application/json")
                    .WithTimeout(5);
            });
        }
        #endregion
    }
}
