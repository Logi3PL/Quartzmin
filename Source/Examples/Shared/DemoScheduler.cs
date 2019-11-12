using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Listener;
using Slf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Quartzmin
{
    public static class DemoScheduler
    {
        public static async Task<IScheduler> Create(bool start = true)
        {
            try
            {
                var conStr = ConfigurationManager.ConnectionStrings["QUARTZNETJOBDB"]?.ConnectionString;

                LoggerService.GetLogger("LOGIJMS").Log(new LogItem()
                {
                    LoggerName = "LOGIJMS",
                    Title = "Scheduler Create",
                    Message = "Scheduler Create",
                    LogItemProperties = new List<LogItemProperty>() {
                                new LogItemProperty("ServiceName", "JOB") ,
                                new LogItemProperty("ActionName", "GenerateSendDataItemFrom"),
                                new LogItemProperty("CONSTR", conStr),
                            },
                    LogLevel = LogLevel.Trace
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("--------------------------");
            }

            NameValueCollection configuration = new NameValueCollection
            {
                 { "quartz.scheduler.instanceName", "LocalServer" },
                 { "quartz.scheduler.instanceId", "LocalServer" },
                 { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" },
                 //{ "quartz.jobStore.useProperties", "true" },
                 { "quartz.jobStore.dataSource", "default" },
                 { "quartz.jobStore.tablePrefix", "QRTZ_" },
                 //{ "quartz.dataSource.default.connectionString", "Data Source=127.0.0.1,1000;Integrated Security=True;Initial Catalog=QuartzNetJobDb;UID=sa;PWD=I@mJustT3st1ing;Integrated Security=False" },
                 { "quartz.dataSource.default.connectionString",@"Data Source=192.168.5.43\LOGITEST,1434;Initial Catalog=QUARTZNETJOBDB;Persist Security Info=True;User ID=apiUser;Password=123456;MultipleActiveResultSets=True;Encrypt=False;Application Name=LOGIJOB" },
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
            IScheduler scheduler = await factory.GetScheduler();
            //var scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            //var jobb = JobBuilder.Create<SendMailJobDefinition>()
            //        .WithIdentity("SendMail", "SEND")
            //        .WithDescription("Hello Job!")
            //        .StoreDurably()
            //        .Build();
            //var triggerr = TriggerBuilder.Create()
            //    .WithIdentity("MorningSend")
            //    .StartNow()
            //    .WithCronSchedule("0 0 8 1/1 * ? *")
            //    .Build();
            //await scheduler.ScheduleJob(jobb, triggerr);

            //if (start)
            //    await scheduler.Start();

            return scheduler;

            {
                var jobData = new JobDataMap();
                jobData.Put("DateFrom", DateTime.Now);
                jobData.Put("QuartzAssembly", File.ReadAllBytes(typeof(IScheduler).Assembly.Location));

                var job = JobBuilder.Create<DummyJob>()
                    .WithIdentity("Sales", "REPORTS")
                    .WithDescription("Hello Job!")
                    .UsingJobData(jobData)
                    .StoreDurably()
                    .Build();
                var trigger = TriggerBuilder.Create()
                    .WithIdentity("MorningSales")
                    .StartNow()
                    .WithCronSchedule("0 0 8 1/1 * ? *")
                    .Build();
                await scheduler.ScheduleJob(job, trigger);

                trigger = TriggerBuilder.Create()
                    .WithIdentity("MonthlySales")
                    .ForJob(job.Key)
                    .StartNow()
                    .WithCronSchedule("0 0 12 1 1/1 ? *")
                    .Build();
                await scheduler.ScheduleJob(trigger);

                #region Dependent Jobs
                //JobKey jobKey1 = new JobKey("job1", "group1");
                //JobKey jobKey2 = new JobKey("job2", "group2");

                //var job1 = JobBuilder.Create<DummyJob>().WithIdentity(jobKey1).Build();
                //var job2 = JobBuilder.Create<DummyJob>().WithIdentity(jobKey2).StoreDurably(true).Build();

                //ITrigger trigger1 = TriggerBuilder.Create()
                //   .WithIdentity("trigger1", "group1")
                //   .StartNow()
                //   .Build();

                //JobChainingJobListener chain = new JobChainingJobListener("testChain");
                //chain.AddJobChainLink(jobKey1, jobKey2);
                //scheduler.ListenerManager.AddJobListener(chain, GroupMatcher<JobKey>.AnyGroup());

                //await scheduler.ScheduleJob(job1, trigger1);
                //await scheduler.AddJob(job2, true); 
                #endregion
            }

            {
                var job = JobBuilder.Create<DummyJob>().WithIdentity("Job1").StoreDurably().Build();
                await scheduler.AddJob(job, false);
                job = JobBuilder.Create<DummyJob>().WithIdentity("Job2").StoreDurably().Build();
                await scheduler.AddJob(job, false);
                job = JobBuilder.Create<DummyJob>().WithIdentity("Job3").StoreDurably().Build();
                await scheduler.AddJob(job, false);
                job = JobBuilder.Create<DummyJob>().WithIdentity("Job4").StoreDurably().Build();
                await scheduler.AddJob(job, false);
                job = JobBuilder.Create<DummyJob>().WithIdentity("Job5").StoreDurably().Build();
                await scheduler.AddJob(job, false);
                job = JobBuilder.Create<DummyJob>().WithIdentity("Send SMS", "CRITICAL").StoreDurably().RequestRecovery().Build();
                await scheduler.AddJob(job, false);

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("PushAds  (US)")
                    .ForJob(job.Key)
                    .UsingJobData("Location", "US")
                    .StartNow()
                    .WithCronSchedule("0 0/5 * 1/1 * ? *")
                    .Build();
                await scheduler.ScheduleJob(trigger);

                trigger = TriggerBuilder.Create()
                    .WithIdentity("PushAds (EU)")
                    .ForJob(job.Key)
                    .UsingJobData("Location", "EU")
                    .StartNow()
                    .WithCronSchedule("0 0/7 * 1/1 * ? *")
                    .Build();
                await scheduler.ScheduleJob(trigger);
                await scheduler.PauseTriggers(GroupMatcher<TriggerKey>.GroupEquals("LONGRUNNING"));

                job = JobBuilder.Create<DummyJob>().WithIdentity("Send Push", "CRITICAL").StoreDurably().RequestRecovery().Build();
                await scheduler.AddJob(job, false);
            }

            
        }

        public class DummyJob : IJob
        {
            private static readonly Random Random = new Random();

            public async Task Execute(IJobExecutionContext context)
            {
                Debug.WriteLine("DummyJob > " + DateTime.Now);

                await Task.Delay(TimeSpan.FromSeconds(Random.Next(1, 20)));

                if (Random.Next(2) == 0)
                    throw new Exception("Fatal error example!");
            }
        }

        [DisallowConcurrentExecution, PersistJobDataAfterExecution]
        public class DisallowConcurrentJob : IJob
        {
            private static readonly Random Random = new Random();

            public async Task Execute(IJobExecutionContext context)
            {
                Debug.WriteLine("DisallowConcurrentJob > " + DateTime.Now);

                context.JobDetail.JobDataMap.Put("LastExecuted", DateTime.Now);

                await Task.Delay(TimeSpan.FromSeconds(Random.Next(1, 5)));

                if (Random.Next(4) == 0)
                    throw new Exception("Fatal error example!");
            }
        }
    }
}
