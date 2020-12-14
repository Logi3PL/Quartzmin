using SelfHosting.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using SelfHosting.Common;
using SelfHosting.Common.Request;

namespace SelfHosting.Repository
{
    public class CustomerJobHistoryRepository : ICustomerJobHistoryRepository
    {
        private readonly JobContext _jobContext;
        public CustomerJobHistoryRepository(JobContext jobContext)
        {
            _jobContext = jobContext;
        }

        public async Task<dynamic> AddHistory(AddHistoryRequest addHistoryRequest)
        {
            try
            {
                var customerJobHistory = new CustomerJobHistory()
                {
                    CustomerJobId = addHistoryRequest.CustomerJobId,
                    ProcessStatus = (byte)addHistoryRequest.ProcessStatus,
                    ProcessTime = addHistoryRequest.ProcessTime
                };

                _jobContext.CustomerJobHistories.Add(customerJobHistory);
                var res = await _jobContext.SaveChangesAsync();

                if (res < 1)
                {
                    var exp = new InvalidOperationException("CustomerJobHistory -> Save Error");
                }

                return Helper.ReturnOk(customerJobHistory.Id);
            }
            catch (Exception ex)
            {
                //TODO:Log
                return Helper.ReturnError(ex);
            }
        }
    }
}
