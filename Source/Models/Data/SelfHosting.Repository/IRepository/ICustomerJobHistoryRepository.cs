using SelfHosting.Common.Request;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SelfHosting.Repository
{
    public interface ICustomerJobHistoryRepository
    {
        Task<dynamic> AddHistory(AddHistoryRequest addHistoryRequest);
    }
}
