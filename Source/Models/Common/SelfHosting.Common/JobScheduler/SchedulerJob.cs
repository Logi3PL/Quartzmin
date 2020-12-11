using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Common
{
    public class SchedulerJob
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Type JobType { get; set; }
        public ISchedulerJob Instance { get; set; }

        public bool IsActive { get; set; }
    }
}
