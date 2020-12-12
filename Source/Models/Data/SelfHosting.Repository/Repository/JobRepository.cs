using SelfHosting.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using SelfHosting.Common;

namespace SelfHosting.Repository
{
    public class JobRepository : IJobRepository
    {
        private readonly JobContext _jobContext;
        public JobRepository(JobContext jobContext)
        {
            _jobContext = jobContext;
        }
        public List<Job> GetAll() => _jobContext.Jobs.ToList();
    }
}
