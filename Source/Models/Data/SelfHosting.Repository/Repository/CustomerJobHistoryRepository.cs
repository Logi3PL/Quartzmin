using SelfHosting.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using SelfHosting.Common;
using SelfHosting.Common.Request;
using Logi3PL.Business.Core.Logging.BusinessLoggers;

namespace SelfHosting.Repository
{
    public class CustomerJobHistoryRepository : ICustomerJobHistoryRepository
    {
        private readonly DbContextOptions<JobContext> _option;
        public CustomerJobHistoryRepository(DbContextOptions<JobContext> option)
        {
            _option = option;
        }

        public async Task<dynamic> AddHistory(AddHistoryRequest addHistoryRequest)
        {
            try
            {
                using (JobContext context = new JobContext(_option))
                {
                    var customerJobHistory = new CustomerJobHistory()
                    {
                        CustomerJobId = addHistoryRequest.CustomerJobId,
                        ProcessStatus = (byte)addHistoryRequest.ProcessStatus,
                        ProcessTime = addHistoryRequest.ProcessTime
                    };

                    context.CustomerJobHistories.Add(customerJobHistory);
                    var res = await context.SaveChangesAsync();

                    if (res < 1)
                    {
                        var exp = new InvalidOperationException("CustomerJobHistory -> Save Error");
                    }

                    return Helper.ReturnOk(customerJobHistory.Id);
                }
                
            }
            catch (Exception ex)
            {
                BusinessLogger.Log(ConstantHelper.JobLog, "AddHistory", exception: ex, extraParams: new Dictionary<string, object>() {
                    {"AddHistoryRequest",addHistoryRequest },
                    {"RepositoryName",this.GetType().Name }
                });
                return Helper.ReturnError(ex);
            }
        }
    }
}
