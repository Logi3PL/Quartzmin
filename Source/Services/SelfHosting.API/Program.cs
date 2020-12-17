using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Logi3plJMS.API.WorkerService;
using SelfHosting.Services;
using Slf.NetCore;
using System;
using SelfHosting.Common;
using Microsoft.Extensions.Configuration;
using SLF.NetCore.NLogFacade;
using System.Threading.Tasks;

namespace Logi3plJMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
            var nlogLogger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            var slfNlogger = new NLogLogger(nlogLogger);

            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile(path: "appsettings.json").Build();
            NLog.Extensions.Logging.ConfigSettingLayoutRenderer.DefaultConfiguration = config;

            LoggerService.SetLogger(slfNlogger);
            var logger = LoggerService.GetLogger("System");

            try
            {
                Task.Run(() => {
                    logger.Log(new LogItem()
                    {
                        LoggerName = "System",
                        LogLevel = Slf.NetCore.LogLevel.Trace,
                        Timestamp = System.DateTime.UtcNow,
                        Title = "WEBAPI Started",
                        LogItemProperties = new System.Collections.Generic.List<LogItemProperty>()
                        {
                            new LogItemProperty(ConstantHelper.ServiceKey,ConstantHelper.JobLog)
                        }
                    });
                });

                Console.WriteLine("CurrentProcessorId:" + System.Threading.Thread.GetCurrentProcessorId());

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped Service because of exception");
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                //İlk olarak joblarımızı alabilmek için API ' yi ayağa kaldırıyoruz.
                webBuilder.UseStartup<Startup>();  //.UseUrls();
            })
           .ConfigureServices((hostContext, services) =>
           {
  
               services.AddHostedService<SchedulerWorkerService>(); //Hosted Servisimizi ekliyoruz.

           }).UseWindowsService(); //Windows Servis olarak çalışacağını belirtiyoruz.
    }
}
