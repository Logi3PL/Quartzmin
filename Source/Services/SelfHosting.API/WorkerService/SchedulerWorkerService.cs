using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SelfHosting.API.AppSettings;
using SelfHosting.Services;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SelfHosting.API.WorkerService
{

    /// <summary>
    /// SchedulerWorkerService adında bir sınıf oluşturup, BackGroundService'den kalıtım alıyorum. 
    /// </summary>
    public class SchedulerWorkerService : BackgroundService
    {
        private readonly ISchedulerService _schedulerService;
        private readonly ConfigParameter configParameter; //Self-Host Api appsetting.json dosyasından url ve method adlarını alıyoruz.
        private readonly IHostingEnvironment _hostingEnvironment;
        public SchedulerWorkerService(ISchedulerService schedulerService,
            IOptions<ConfigParameter> options,
            IHostingEnvironment hostingEnvironment)
        {
            configParameter = options.Value;
            _schedulerService =  schedulerService;
            _hostingEnvironment = hostingEnvironment;
        }
        /// <summary>
        /// Execute Methodunu override ederek ihtiyacımıza göre çalıştırıyoruz.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {


            while (!stoppingToken.IsCancellationRequested)
            {
                Log.Information("Worker Servis Çalıştı");

                await _schedulerService.Load(_hostingEnvironment.ContentRootPath, configParameter);

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
