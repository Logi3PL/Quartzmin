using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Repository
{
    public interface ICustomJobRepository
    {
        List<CustomerJob> GetAll();
    }
}
