using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Services
{
    public interface ICustomJobService
    {
        List<CustomerJob> GetAll();
    }
}
