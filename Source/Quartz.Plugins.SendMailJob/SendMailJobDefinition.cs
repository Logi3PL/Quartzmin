using Newtonsoft.Json;
using Quartz.Plugins.SendMailJob;
using Quartz.Plugins.SendMailJob.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quartz.Plugins
{
    public class SendMailJobDefinition : IJob
    {
        private static readonly Random Random = new Random();

        public async Task Execute(IJobExecutionContext context)
        {
            //Debug.WriteLine("DummyJob > " + DateTime.Now);

            var schedulerName =context.Scheduler.SchedulerName;

            var jobName = context.JobDetail.Key.Name;
            var jobGroup = context.JobDetail.Key.Group;

            var trgName = context.Trigger.Key.Name;
            var trgGroup = context.Trigger.Key.Group;

            var jobDataKeys = context.JobDetail.JobDataMap.GetKeys();

            if (jobDataKeys.Contains(ConstantHelper.CustomData))
            {
                var customFormData = context.JobDetail.JobDataMap.GetString(ConstantHelper.CustomData);

                var customFormDataModel = JsonConvert.DeserializeObject<List<CustomDataModel>>(customFormData);
            }

            await Task.Delay(TimeSpan.FromSeconds(Random.Next(1, 20)));
        }
    }
}
