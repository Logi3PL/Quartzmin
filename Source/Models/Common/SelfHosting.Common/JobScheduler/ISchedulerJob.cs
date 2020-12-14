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
        Task ExecuteJobAsync(dynamic customerJobHistoryRepository,string apiUrl, string EndPoint,List<AssignJobParameterItem> jobParameterItems);
    }
}
