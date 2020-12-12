using SelfHosting.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using SelfHosting.Common;

namespace SelfHosting.Repository
{
    public class CustomerJobRepository : ICustomerJobRepository
    {
        private readonly JobContext _jobContext;
        public CustomerJobRepository(JobContext jobContext)
        {
            _jobContext = jobContext;
        }
        public List<CustomerJob> GetAll() => _jobContext.CustomerJob.Include(x=> x.Job).ToList();

        public async Task<dynamic> AssignJob(int customerId, int jobId,string cron = "")
        {
            var customerJob = new CustomerJob() { 
                CustomerId = customerId,
                JobId = jobId,
                Cron = cron,
                Active = 1,
                CreatedBy = 1, //TODO ?
                CreatedTime = DateTime.Now
            };
            
            _jobContext.CustomerJob.Add(customerJob);
            var res = await _jobContext.SaveChangesAsync();
            if (res < 1)
            {
                var exp = new InvalidOperationException("Save Error");

                return Helper.ReturnError(exp);
            }

            return Helper.ReturnOk(customerJob.Id);
        }
    }
}
