using Quartz;
using SelfHosting.Common.Request;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SelfHosting.Common
{
    public interface ISchedulerJob
    {
        Guid Guid { get; }
        string Name { get; }
        Task ExecuteJobAsync(IJobExecutionContext context,IServiceProvider serviceProvider,List<AssignJobParameterItem> jobParameterItems, List<AssignJobSubscriberItem> jobSubscriberItems);
    }
}
