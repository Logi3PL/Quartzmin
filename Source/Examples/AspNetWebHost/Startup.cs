using Microsoft.Owin;
using Owin;
using Quartz;
using Quartz.Impl;
using Quartzmin.AspNet;
using Slf;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

[assembly: OwinStartup(typeof(Startup))]

namespace Quartzmin.AspNet
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            IScheduler scheduler = null;
            var conStr = ConfigurationManager.ConnectionStrings["QUARTZNETJOBDB"]?.ConnectionString;
            try
            {
                NameValueCollection configuration = new NameValueCollection
                {
                     { "quartz.scheduler.instanceName", "LocalServer" },
                     { "quartz.scheduler.instanceId", "LocalServer" },
                     { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" },
                     //{ "quartz.jobStore.useProperties", "true" },
                     { "quartz.jobStore.dataSource", "default" },
                     { "quartz.jobStore.tablePrefix", "QRTZ_" },
                     //{ "quartz.dataSource.default.connectionString", "Data Source=127.0.0.1,1000;Integrated Security=True;Initial Catalog=QuartzNetJobDb;UID=sa;PWD=I@mJustT3st1ing;Integrated Security=False" },
                     { "quartz.dataSource.default.connectionString",conStr },
                     { "quartz.dataSource.default.provider", "SqlServer" },
                     //{ "quartz.threadPool.threadCount", "1" },
                     { "quartz.serializer.type", "binary" }
                };

                configuration["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
                configuration["quartz.threadPool.threadCount"] = "5";
                configuration["quartz.threadPool.threadPriority"] = "Normal";
                configuration["quartz.plugin.recentHistory.type"] = "Quartz.Plugins.RecentHistory.ExecutionHistoryPlugin, Quartz.Plugins.RecentHistory";

                configuration["quartz.plugin.recentHistory.storeType"] = "Quartz.Plugins.RecentHistory.Impl.InProcExecutionHistoryStore, Quartz.Plugins.RecentHistory";

                StdSchedulerFactory factory = new StdSchedulerFactory(configuration);
                scheduler = factory.GetScheduler().GetAwaiter().GetResult();

                scheduler.Start();

                if (scheduler.IsStarted)
                {
                    LoggerService.GetLogger("LOGIJMS").Log(new LogItem()
                    {
                        LoggerName = "LOGIJMS",
                        Title = "Scheduler Started",
                        Message = "Scheduler Started",
                        LogItemProperties = new List<LogItemProperty>() {
                                new LogItemProperty("ServiceName", "JOB") ,
                                new LogItemProperty("ActionName", "DemoScheduler.Create()")
                            },
                        LogLevel = LogLevel.Info
                    });
                }
                else
                {
                    LoggerService.GetLogger("LOGIJMS").Log(new LogItem()
                    {
                        LoggerName = "LOGIJMS",
                        Title = "Scheduler Not Started",
                        Message = "Scheduler Not  Started",
                        LogItemProperties = new List<LogItemProperty>() {
                                new LogItemProperty("ServiceName", "JOB") ,
                                new LogItemProperty("ActionName", "DemoScheduler.Create()")
                            },
                        LogLevel = LogLevel.Info
                    });
                }

                //scheduler = DemoScheduler.Create().Result;
            }
            catch (System.Exception ex)
            {
                LoggerService.GetLogger("LOGIJMS").Log(new LogItem()
                {
                    LoggerName = "LOGIJMS",
                    Title = "Scheduler Create",
                    Message = "Scheduler Create",
                    LogItemProperties = new List<LogItemProperty>() {
                                new LogItemProperty("ServiceName", "JOB") ,
                                new LogItemProperty("ActionName", "DemoScheduler.Create()")
                            },
                    LogLevel = LogLevel.Error,
                    Exception =ex
                });
            }

            try
            {
                app.UseQuartzmin(new QuartzminOptions()
                {
                    Scheduler = scheduler,

                    DefaultDateFormat = "dd.MM.yyyy"
                });
            }
            catch (System.Exception ex)
            {
                LoggerService.GetLogger("LOGIJMS").Log(new LogItem()
                {
                    LoggerName = "LOGIJMS",
                    Title = "UseQuartzmin",
                    Message = "UseQuartzmin",
                    LogItemProperties = new List<LogItemProperty>() {
                                new LogItemProperty("ServiceName", "JOB") ,
                                new LogItemProperty("ActionName", "UseQuartzmin")
                            },
                    LogLevel = LogLevel.Error,
                    Exception = ex
                });
            }
        }
    }
}