using SelfHosting.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Repository.CurrentJobs
{
    public class PluginContext
    {
        public PluginContext()
        {
            Jobs = new List<SchedulerJob>();
        }

        public List<SchedulerJob> Jobs { get; set; }
    }
}
