using SelfHosting.Common.JobScheduler;
using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Common.Request
{
    public class AddHistoryRequest
    {
        public int CustomerJobId { get; set; }
        public ProcessStatusTypes ProcessStatus { get; set; }
        public DateTimeOffset ProcessTime { get; set; }
    }
}
