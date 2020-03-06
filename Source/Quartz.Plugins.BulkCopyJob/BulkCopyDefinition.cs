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
           

        }
    }
}
