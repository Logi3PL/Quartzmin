﻿using SelfHosting.Common.Request;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SelfHosting.Services
{
    public interface ICustomerJobService
    {
        List<CustomerJob> GetAll();
        Task<dynamic> AssignJob(AssignJobRequest assignJobRequest);
    }
}
