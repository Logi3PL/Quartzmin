using Newtonsoft.Json;
using Quartz.Plugins.ScriptExecuterJob;
using Quartz.Plugins.ScriptExecuterJob.Models;
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
    public class ScriptExecuterJobDefinition : IJob
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
            var customFormDataModel = new ScriptExecuterViewModel();

            if (jobDataKeys.Contains(ConstantHelper.CustomData))
            {
                var customFormData = context.JobDetail.JobDataMap.GetString(ConstantHelper.CustomData);
                customFormDataModel = JsonConvert.DeserializeObject<ScriptExecuterViewModel>(customFormData);
            }

            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
            {
                LoggerName = ConstantHelper.JobLog,
                Title = "ScriptExecuterJobDefinition Started",
                Message = "ScriptExecuterJobDefinition Started",
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

            if (customFormDataModel!=null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    #region Get CustomFormDataModel Props
                    var scriptType = customFormDataModel.ScriptType;
                    var scriptSource = customFormDataModel.ScriptSource;
                    var conStr = customFormDataModel.ConnectionString;
                    #endregion

                    #region Execute Script

                    switch (scriptType)
                    {
                        case ScriptType.TSql:

                            var res = await SqlQueryManager.ExecuteQuery(conStr, scriptSource);

                            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                            {
                                LoggerName = ConstantHelper.JobLog,
                                Title = $"ScriptExecuterJobDefinition Call Finished",
                                Message = "ScriptExecuterJobDefinition Finished",
                                LogItemProperties = new List<LogItemProperty>() {
                                    new LogItemProperty("ServiceName", ConstantHelper.JobLog),
                                    new LogItemProperty("ScheduleName", scheduleName),
                                    new LogItemProperty("JobName", jobName),
                                    new LogItemProperty("JobGroup", jobGroup),
                                    new LogItemProperty("TriggerName", trgName),
                                    new LogItemProperty("TriggerGroup", trgGroup),
                                    new LogItemProperty("ExecutionStatus", res),
                                    new LogItemProperty("FormData", new { CustomFormDataModel =customFormDataModel })
                                },
                                LogLevel = LogLevel.Info
                            });

                            break;
                        default:
                            break;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                    {
                        LoggerName = ConstantHelper.JobLog,
                        Title = "ScriptExecuterJobDefinition Execution Error",
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
