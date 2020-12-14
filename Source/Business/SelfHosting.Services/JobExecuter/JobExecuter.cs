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

        /// <summary>
        /// Job'ı çalıştıran method.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {

            JobDataMap dataMap = context.MergedJobDataMap;

            //Gidilecek Url bilgisini string olarak alıyoruz.
            var BaseUrl = dataMap.GetString("BaseUrl");


            //Endpoint bilgisini string olarak alıyoruz.
            var EndPoint = dataMap.GetString("EndPoint");


            ///Generic bir yapı oluşturabilmek için abstract factory method ile yakaladığımız sınıfı burda tetikliyoruz.
            ///Hangi Job Sınıfı bize döndürülürse onu çalıştıracağız.
            
            //var SchedulerJob = (ISchedulerJob)dataMap.Get("SchedulerJob");
            var SchedulerJobName = (string)dataMap.Get("SchedulerJobName");
            var root = (string)dataMap.Get("SchedulerJobPathRoot");
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

            await SchedulerJob.ExecuteJobAsync(BaseUrl, EndPoint, jobParameters);

        }
    }
}
