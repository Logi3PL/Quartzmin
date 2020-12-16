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
            using (var scope = _provider.CreateScope())
            {
                // Resolve the Scoped service
                _customerJobHistoryRepository = scope.ServiceProvider.GetRequiredService<ICustomerJobHistoryRepository>();

                JobDataMap dataMap = context.MergedJobDataMap;

                //var SchedulerJob = (ISchedulerJob)dataMap.Get("SchedulerJob");
                var SchedulerJobName = dataMap.GetString("SchedulerJobName");
                var root = dataMap.GetString("SchedulerJobPathRoot");
                var jobParameters = new List<AssignJobParameterItem>();

                if (dataMap.ContainsKey("SchedulerJobParameters"))
                {
                    var prmString = dataMap.Get("SchedulerJobParameters").ToString();

                    jobParameters = JsonConvert.DeserializeObject<List<AssignJobParameterItem>>(prmString);
                }

                string pluginFolder = Path.Combine(root, "JobPlugins");

                string[] filePaths = Directory.GetFiles(pluginFolder, $"{SchedulerJobName}.dll");

                var jobAssembly = Assembly.LoadFile(filePaths[0]);

                var jobType = jobAssembly.GetTypes().Where(x => x.GetInterface(nameof(ISchedulerJob)) != null).FirstOrDefault();

                var SchedulerJob = (ISchedulerJob)Activator.CreateInstance(jobType);

                await SchedulerJob.ExecuteJobAsync(scope.ServiceProvider, jobParameters);

            }

        }
    }
}
