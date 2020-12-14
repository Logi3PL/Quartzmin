using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Common.JobScheduler
{
    public enum ProcessStatusTypes:byte
    {
        Executing = 1,
        Executed = 2,
        Cancelled = 3,
        Terminated = 4
    }
}
