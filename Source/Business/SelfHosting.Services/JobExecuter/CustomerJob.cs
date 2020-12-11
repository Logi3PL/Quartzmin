using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Services.JobExecuter
{
    public class CustomerJob
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid JobId { get; set; }
        public string Cron { get; set; }
        public bool IsActive { get; set; }
    }
}
