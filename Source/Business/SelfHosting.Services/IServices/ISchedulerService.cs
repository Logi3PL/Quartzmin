using SelfHosting.API.AppSettings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SelfHosting.Services
{
    public interface ISchedulerService
    {
        Task Load(string root, ConfigParameter configParameter);
    }
}
