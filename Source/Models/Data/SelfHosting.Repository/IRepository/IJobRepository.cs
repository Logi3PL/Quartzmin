using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SelfHosting.Repository
{
    public interface IJobRepository
    {
        List<Job> GetAll();
    }
}
