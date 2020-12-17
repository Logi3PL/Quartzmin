using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Common.Request
{
    public class AssignJobRequest
    {
        public string CustomerCode { get; set; }
        public string JobCode { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string CronExp { get; set; }
        public List<AssignJobParameterItem> JobParameters { get; set; }
        public List<AssignJobSubscriberItem> JobSubscriberItems { get; set; }
    }
}
