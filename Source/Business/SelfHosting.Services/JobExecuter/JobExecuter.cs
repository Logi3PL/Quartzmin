using Logi3PL.Business.Core.Logging.BusinessLoggers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz;
using RestSharp;
using SelfHosting.Common;
using SelfHosting.Common.Request;
using SelfHosting.Repository;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SelfHosting.Services.JobExecuter
{
    public class JobExecuter : IJob
    {
        private ICustomerJobHistoryRepository _customerJobHistoryRepository;
        private readonly IServiceProvider _provider;
        public JobExecuter(IServiceProvider provider)
        {
            _provider = provider;
        }

        //public JobExecuter(IServiceProvider provider)
        //{
        //    _provider = provider;
        //}
        /// <summary>
        /// Job'ı çalıştıran method.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {

            // Create a new scope
            try
            {
                using (var scope = _provider.CreateScope())
                {
                    // Resolve the Scoped service
                    _customerJobHistoryRepository = scope.ServiceProvider.GetRequiredService<ICustomerJobHistoryRepository>();

                    JobDataMap dataMap = context.MergedJobDataMap;

                    //var SchedulerJob = (ISchedulerJob)dataMap.Get("SchedulerJob");
                    var SchedulerJobName = dataMap.GetString(ConstantHelper.SchedulerJobHelper.SchedulerJobDataMapHelper.SchedulerJobNameKey);
                    var root = dataMap.GetString(ConstantHelper.SchedulerJobHelper.SchedulerJobDataMapHelper.SchedulerJobPathRootKey);
                    var jobParameters = new List<AssignJobParameterItem>();

                    if (dataMap.ContainsKey(ConstantHelper.SchedulerJobHelper.SchedulerJobDataMapHelper.SchedulerJobParametersKey))
                    {
                        var prmString = dataMap.Get(ConstantHelper.SchedulerJobHelper.SchedulerJobDataMapHelper.SchedulerJobParametersKey).ToString();

                        jobParameters = JsonConvert.DeserializeObject<List<AssignJobParameterItem>>(prmString);
                    }

                    var jobSubscribers = new List<AssignJobSubscriberItem>();

                    if (dataMap.ContainsKey(ConstantHelper.SchedulerJobHelper.SchedulerJobDataMapHelper.SchedulerJobSubscribersKey))
                    {
                        var prmString = dataMap.Get(ConstantHelper.SchedulerJobHelper.SchedulerJobDataMapHelper.SchedulerJobSubscribersKey).ToString();

                        jobSubscribers = JsonConvert.DeserializeObject<List<AssignJobSubscriberItem>>(prmString);
                    }

                    string pluginFolder = Path.Combine(root, "JobPlugins");

                    string[] filePaths = Directory.GetFiles(pluginFolder, $"{SchedulerJobName}.dll");

                    var jobAssembly = Assembly.LoadFile(filePaths[0]);

                    var jobType = jobAssembly.GetTypes().Where(x => x.GetInterface(nameof(ISchedulerJob)) != null).FirstOrDefault();

                    var SchedulerJob = (ISchedulerJob)Activator.CreateInstance(jobType);

                    await SchedulerJob.ExecuteJobAsync(context, scope.ServiceProvider, jobParameters, jobSubscribers);

                }
            }
            catch (Exception ex)
            {
                BusinessLogger.Log(ConstantHelper.JobLog, "JobExecuter->Execute", exception: ex, extraParams: new Dictionary<string, object>() {
                    {"JobDataMap",context.MergedJobDataMap }
                });
            }

        }
    }
}
