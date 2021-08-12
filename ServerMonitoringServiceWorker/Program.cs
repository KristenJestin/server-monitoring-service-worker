using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;
using ServerMonitoringServiceWorker.Api;
using ServerMonitoringServiceWorker.Common.Models;
using ServerMonitoringServiceWorker.Services;
using ServerMonitoringServiceWorker.Workers;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ServerMonitoringServiceWorker
{
    public class Program
    {
        public const string APP_NAME = "ServerMonitoring";
        public const string BASE_URL_SEGMENT = "devices";

        public static int Main(string[] args)
        {
            ConfigureLogger();

            try
            {
                Log.Information("Starting host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
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
                })
                .UseSerilog();


        #region privates
        private static void ConfigureLogger()
        {
            string logPath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                logPath = Path.Join("/var", "log", APP_NAME.ToLower(), "log.txt");
            else
                logPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_NAME, "logs", "log.txt");

            var logTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u}] [{SourceContext}] {Message}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: logTemplate)
                .WriteTo.File(logPath,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    outputTemplate: logTemplate)
                .CreateLogger();

            Log.Information($"Define log path to : {logPath}");
        }

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
