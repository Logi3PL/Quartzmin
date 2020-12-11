using SelfHosting.Repository;
using System.Collections.Generic;

namespace SelfHosting.Services
{
    public class CustomJobService : ICustomJobService
    {
        private readonly ICustomJobRepository _customJobRepository;
        public CustomJobService(ICustomJobRepository customJobRepository)
        {
            _customJobRepository = customJobRepository;
        }
        public List<CustomerJob> GetAll() => _customJobRepository.GetAll();
    }
}
