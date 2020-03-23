using Newtonsoft.Json;
using Slf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System.IO.Compression;
using Quartz.Plugins.ScriptExecuterJob.DataLayer.Manager;
using Quartz.Plugins.BulkCopyJob.Models;
using Quartz.Plugins.BulkCopyJob;

#if NETSTANDARD
//using Slf.NetCore;
#endif
#if NETFRAMEWORK
using Slf;
#endif

namespace Quartz.Plugins
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class BulkCopyDefinition : IJob
    {
        private static readonly Random Random = new Random();

        public async Task Execute(IJobExecutionContext context)
        {
            var schedulerName = context.Scheduler.SchedulerName;

            var scheduleName = context.Scheduler.SchedulerName;
            var jobName = context.JobDetail.Key.Name;
            var jobGroup = context.JobDetail.Key.Group;

            var trgName = context.Trigger.Key.Name;
            var trgGroup = context.Trigger.Key.Group;

            var jobDataKeys = context.JobDetail.JobDataMap.GetKeys();

            var customFormDataModel = new BulkCopyViewModel();

            if (jobDataKeys.Contains(ConstantHelper.CustomData))
            {
                var customFormData = context.JobDetail.JobDataMap.GetString(ConstantHelper.CustomData);
                customFormDataModel = JsonConvert.DeserializeObject<BulkCopyViewModel>(customFormData);
            }

            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
            {
                LoggerName = ConstantHelper.JobLog,
                Title = $"{this.GetType()} Started",
                Message = $"{this.GetType()} Started",
                LogItemProperties = new List<LogItemProperty>() {
                    new LogItemProperty("ServiceName", ConstantHelper.JobLog),
                    new LogItemProperty("ScheduleName", scheduleName),
                    new LogItemProperty("JobName", jobName),
                    new LogItemProperty("JobGroup", jobGroup),
                    new LogItemProperty("TriggerName", trgName),
                    new LogItemProperty("TriggerGroup", trgGroup)
                },
                LogLevel = LogLevel.Info
            });

            if (customFormDataModel != null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    new BulkCopyManager().ExecuteQuery(customFormDataModel);
                }
                catch (Exception ex)
                {
                    LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                    {
                        LoggerName = ConstantHelper.JobLog,
                        Title = $"{this.GetType()} Execution Error",
                        Message = ex.Message,
                        LogItemProperties = new List<LogItemProperty>() {
                                        new LogItemProperty("ServiceName", ConstantHelper.JobLog),
                                        new LogItemProperty("ScheduleName", scheduleName),
                                        new LogItemProperty("JobName", jobName),
                                        new LogItemProperty("JobGroup", jobGroup),
                                        new LogItemProperty("TriggerName", trgName),
                                        new LogItemProperty("TriggerGroup", trgGroup),
                                        new LogItemProperty("ActionName", "ExecuteQuery"),
                                        new LogItemProperty("FormData", new { CustomFormDataModel =customFormDataModel}),
                                    },
                        LogLevel = LogLevel.Error,
                        Exception = ex
                    });
                }

                stopwatch.Stop();

            }

        }
    }
}
