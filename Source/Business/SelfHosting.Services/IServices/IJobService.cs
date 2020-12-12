using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SelfHosting.Services
{
    public interface IJobService
    {
        List<Job> GetAll();
    }
}
