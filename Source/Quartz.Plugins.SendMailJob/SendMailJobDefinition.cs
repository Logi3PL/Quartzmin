using Newtonsoft.Json;
using Quartz.Plugins.SendMailJob;
using Quartz.Plugins.SendMailJob.DataLayer.Manager;
using Quartz.Plugins.SendMailJob.DataLayer.Model;
using Quartz.Plugins.SendMailJob.Models;
using Slf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
    public class SendMailJobDefinition : IJob
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

            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
            {
                LoggerName = ConstantHelper.JobLog,
                Title = "SendMailJobDefinition Started",
                Message = "SendMailJobDefinition Started",
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
            //Debug.WriteLine("DummyJob > " + DateTime.Now);

            var jobDataKeys = context.JobDetail.JobDataMap.GetKeys();

            if (jobDataKeys.Contains(ConstantHelper.CustomData))
            {
                var customFormData = context.JobDetail.JobDataMap.GetString(ConstantHelper.CustomData);
                var customFormDataModel = JsonConvert.DeserializeObject<List<CustomDataModel>>(customFormData);

                var sendDataItem = new SendDataItem()
                {
                    JobGroup = jobGroup,
                    JobName = jobName,
                    ScheduleName = scheduleName,
                    TriggerGroup = trgGroup,
                    TriggerName = trgName,
                    Type = 1, //TODO:Static - Email/Sms
                    CreatedDate = DateTimeOffset.Now,
                    Active = 1
                };

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    await SendDataItemManager.GenerateSendDataItemFrom(customFormDataModel, sendDataItem);
                }
                catch (Exception ex)
                {
                    LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                    {
                        LoggerName = ConstantHelper.JobLog,
                        Title = "GenerateSendDataItemFrom Execution Error",
                        Message = ex.Message,
                        LogItemProperties = new List<LogItemProperty>() {
                                        new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                        new LogItemProperty("ActionName", "GenerateSendDataItemFrom"),
                                        new LogItemProperty("FormData", new { CustomFormDataModel =customFormDataModel, SendDataItem = sendDataItem}),
                                    },
                        LogLevel = LogLevel.Error,
                        Exception = ex
                    });
                }

                stopwatch.Stop();

                if (context.JobDetail.JobDataMap.GetBoolean("trace"))
                {
                    LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                    {
                        LoggerName = ConstantHelper.JobLog,
                        Title = "GenerateSendDataItemFrom Executed",
                        Message = "GenerateSendDataItemFrom Executed",
                        LogItemProperties = new List<LogItemProperty>() {
                                new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                new LogItemProperty("ActionName", "GenerateSendDataItemFrom"),
                                new LogItemProperty("SendDataItem", sendDataItem),
                                new LogItemProperty("ElapsedTimeAssn", stopwatch.Elapsed.TotalSeconds),
                            },
                        LogLevel = LogLevel.Trace
                    });
                }

            }

        }
    }
}
