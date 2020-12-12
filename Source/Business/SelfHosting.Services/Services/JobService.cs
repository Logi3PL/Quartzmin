using SelfHosting.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SelfHosting.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }
        public List<Job> GetAll() => _jobRepository.GetAll();
    }
}
