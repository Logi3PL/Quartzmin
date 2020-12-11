using SelfHosting.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace SelfHosting.Repository
{
    public class CustomJobRepository : ICustomJobRepository
    {
        private readonly JobContext _jobContext;
        public CustomJobRepository(JobContext jobContext)
        {
            _jobContext = jobContext;
        }
        public List<CustomerJob> GetAll() => _jobContext.CustomerJob.Include(x=> x.Job).ToList();

        //public async Task<dynamic> CreateJob(Job job) {

        //    job.IsActive = 1;
        //    _jobContext.Jobs.Add(job);
        //    var res = await _jobContext.SaveChangesAsync();
        //    if (res < 1)
        //    {
        //        throw new InvalidOperationException("Issue Not Saved !");
        //    }

        //};
    }
}
