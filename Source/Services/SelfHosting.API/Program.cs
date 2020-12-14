using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Logi3plJMS.API.WorkerService;
using SelfHosting.Services;

namespace Logi3plJMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                //İlk olarak joblarımızı alabilmek için API ' yi ayağa kaldırıyoruz.
                webBuilder.UseStartup<Startup>();  //.UseUrls();
            })
           .ConfigureServices((hostContext, services) =>
           {
  
               services.AddHostedService<SchedulerWorkerService>(); //Hosted Servisimizi ekliyoruz.

           }).UseWindowsService(); //Windows Servis olarak çalışacağını belirtiyoruz.
    }
}
