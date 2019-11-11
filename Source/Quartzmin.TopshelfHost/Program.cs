using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Quartzmin.TopshelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                x.Service<QuartzSchedulerService>(s =>
                {
                    s.ConstructUsing(name => new QuartzSchedulerService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.UseNLog();

                x.SetDescription("LogiJMS Topshelf Host");
                x.SetDisplayName("LogiJMS Topshelf Host");
                x.SetServiceName("LogiJMS Service");
            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;

        }
    }
}
