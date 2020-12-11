using SelfHosting.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
    }
}
