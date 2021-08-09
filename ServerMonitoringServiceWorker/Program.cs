using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerMonitoringServiceWorker.Models;
using ServerMonitoringServiceWorker.Workers;

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
                });


        #region privates
        public static void ConfigureHttpClient(string baseUrl)
        {
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
