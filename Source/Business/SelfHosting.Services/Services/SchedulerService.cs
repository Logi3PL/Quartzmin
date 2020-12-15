using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Quartz.Plugins.RecentHistory;
using Quartz.Plugins.RecentHistory.Db;
using RestSharp;
using SelfHosting.API.AppSettings;
using SelfHosting.Common;
using SelfHosting.Common.Request;
using SelfHosting.Repository.CurrentJobs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SelfHosting.Services
{
    public class SchedulerService : ISchedulerService
    {
        private PluginContext PluginContext;
        private IServiceProvider _serviceProvider;
        public SchedulerService(IServiceProvider serviceProvider)
        {
            PluginContext = new PluginContext();
            _serviceProvider = serviceProvider;
        }

        public async Task Load(string root, ConfigParameter configParameter)
        {
            string pluginFolder = Path.Combine(root, "JobPlugins");

            string[] filePaths = Directory.GetFiles(pluginFolder, "*.dll");

            var JobList = new List<SchedulerJob>();

            foreach (var filePath in filePaths)
            {
                var jobAssembly = Assembly.LoadFile(filePath);

                List<Type> jobTypes = jobAssembly.GetTypes().Where(x => x.GetInterface(nameof(ISchedulerJob)) != null).ToList();

                await Task.Run(() =>
                 {
                     foreach (Type jobType in jobTypes)
                     {
                         var jobInstance = (ISchedulerJob)Activator.CreateInstance(jobType);

                         //PluginContext.Jobs.Add(new SchedulerJob
                         //{
                         //    Id = jobInstance.Guid,
                         //    JobType = jobType,
                         //    Instance = jobInstance,
                         //    Name = jobType.Name
                         //});

                         JobList.Add(new SchedulerJob
                         {
                             IsActive = true,
                             Id = jobInstance.Guid,
                             Name = jobInstance.Name,
                         });
                     }
                 });
                
                if (JobList != null && JobList.Count > 0)
                {
                    var client = new RestClient(configParameter.jobApiUrl);

                    var request = new RestRequest(configParameter.MethodName, Method.POST);

                    request.AddJsonBody(JobList);

                    // execute the request
                    var response = client.Execute(request);

                    Log.Information($"{response.StatusCode}");
                }
            }

            await ExecuteScheduler(root, configParameter);
        }
        private async Task ExecuteScheduler(string root,ConfigParameter configParameter)
        {
            //var _scheduler = await new StdSchedulerFactory().GetScheduler();

            NameValueCollection configuration = new NameValueCollection
            {
                { "quartz.scheduler.instanceName", "LocalServer" },
                { "quartz.scheduler.instanceId", "LocalServer" },
                { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" },
                //{ "quartz.jobStore.useProperties", "true" },
                { "quartz.jobStore.dataSource", "default" },
                { "quartz.jobStore.tablePrefix", "QRTZ_" },
                { "quartz.dataSource.default.connectionString",configParameter.SchedulerConStr },
                { "quartz.dataSource.default.provider", "SqlServer" },
                { "quartz.serializer.type", "binary" }
            };

            configuration["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            configuration["quartz.threadPool.threadCount"] = "5";
            configuration["quartz.threadPool.threadPriority"] = "Normal";
            //configuration["quartz.plugin.recentHistory.type"] = "Quartz.Plugins.RecentHistory.ExecutionHistoryPlugin, Quartz.Plugins.RecentHistory";
            //configuration["quartz.plugin.recentHistory.storeType"] = "Quartz.Plugins.RecentHistory.Db.DbExecutionHistoryStore, SelfHosting.Services";

            IScheduler _scheduler = null;

            try
            {
                ISchedulerFactory schedulerFactory = _serviceProvider.GetRequiredService<ISchedulerFactory>();
                _scheduler = await schedulerFactory.GetScheduler();

                //_scheduler = await new StdSchedulerFactory(configuration).GetScheduler();
                //var executionHistoryPlugin = new ExecutionHistoryPlugin() {Name = "ExecutionHistoryPlugin",StoreType = typeof(DbExecutionHistoryStore) };
                //_scheduler.ListenerManager.AddJobListener(executionHistoryPlugin);

                //_scheduler.JobFactory = new JobFactory(_serviceProvider);
            }
            catch (Exception exs)
            {
                throw;
            }

            //Zamanlayıcı başlatıyoruz.
            //await _scheduler.Start();

            Log.Information("Scheduler Başlatıldı");


            // Çalışacak olan Müşteri joblarını Apimize istekte bulunarak alıyoruz.

            var client = new RestClient(configParameter.jobApiUrl);

            var request = new RestRequest(configParameter.MethodName, Method.GET);

            request.AddHeader("Content-Type", "application/json");

            var tcs = new TaskCompletionSource<IRestResponse>();

            client.ExecuteAsync(request, response =>
            {
                tcs.SetResult(response);
            });

            await tcs.Task;

            var customerJobs = JsonConvert.DeserializeObject<List<CustomerJob>>(tcs.Task.Result.Content);

            Log.Information($"{customerJobs.Count} adet customer job alındı");


            customerJobs.ForEach(async customerJob =>
            {
                // Uygun Job ı Abstract Factory Method ile alıyoruz.
                //var currentJob = CheckSelectJob(customerJob.Job.Id);

                ///Her bir job için uniq bir key olması lazım yoksa runtime hatası alırız.
                var jobName = $"{ConstantHelper.JobNamePrefix}{customerJob.Id}";
                var jobGroup = $"{ConstantHelper.JobGroupPrefix}{customerJob.Id}";

                var jobKey = new JobKey(jobName, jobGroup);

                //Scheduler'a eklenen bir job'ı tekrar eklememek için check ediyoruz. varsa aynı job'ı tekrar kurmayacak.
                var exists = await _scheduler.CheckExists(jobKey);

                if (!exists)
                {
                    //Çalışacak olan joblara dışarıdan parametre vermemizi sağlar Execute olmadan bu parametreleri alıp işleyebiliriz.
                    JobDataMap jobdataMap = new JobDataMap();

                    // Job parametrelerini Execute Scheduler'a parametre olarak geçiyoruz. Execute tetiklendiğinde kullanabilmek için 

                    //TODO ???
                    //jobdataMap.Add("BaseUrl", customerJob.Job.BaseUrl);
                    //jobdataMap.Add("EndPoint", customerJob.Job.EndPoint);

                    if (customerJob.CustomerJobParameters == null)
                    {
                        customerJob.CustomerJobParameters = new List<CustomerJobParameter>();
                    }

                    customerJob.CustomerJobParameters.Add(new CustomerJobParameter() { ParamKey = ConstantHelper.SchedulerJobHelper.CustomerJobIdKey, ParamValue = customerJob.Id.ToString(),ParamSource = customerJob.Customer.CustomerCode });

                    var customerJobParameters = JsonConvert.SerializeObject(customerJob.CustomerJobParameters.Select(x => new AssignJobParameterItem() { ParamKey = x.ParamKey, ParamSource = x.ParamSource, ParamValue = x.ParamValue }).ToList());

                    jobdataMap.Add("SchedulerJobName", customerJob.Job.Name);
                    jobdataMap.Add("SchedulerJobPathRoot", root);
                    jobdataMap.Add("SchedulerJobParameters", customerJobParameters);

                    IJobDetail jobDetail = JobBuilder.Create<JobExecuter.JobExecuter>().WithIdentity(jobName, jobGroup).Build();
                    /*Uniq oluşturduğumuz keyleri burada kullanıyoruz ve Build ile Joblarımızı çalıştıracak olan JobExecuter sınıfını
                    ayağa kaldırıyoruz. */

                    var triggerBuilder = TriggerBuilder.Create()
                           .WithIdentity(jobName, jobGroup)
                           .UsingJobData(jobdataMap);

                    if (string.IsNullOrEmpty(customerJob.Cron) == false)
                    {
                        triggerBuilder = triggerBuilder.WithCronSchedule(customerJob.Cron);//Zamanlayıcımızı Trigger içerisinde belirtip hangi zaman diliminde çalışacağını belirtiyoruz.
                    }

                    if (customerJob.StartDate.HasValue)
                    {
                        triggerBuilder.StartAt(customerJob.StartDate.Value);
                    }

                    if (customerJob.EndDate.HasValue)
                    {
                        triggerBuilder.EndAt(customerJob.EndDate.Value);
                    }

                    ITrigger jobTrigger = triggerBuilder.Build();
                    var result = await _scheduler.ScheduleJob(jobDetail, jobTrigger);
                }

            });
        }

        private ISchedulerJob CheckSelectJob(Guid jobId)
        {
            return PluginContext.Jobs.FirstOrDefault(x => x.Id == jobId).Instance;
        }

    }
}
