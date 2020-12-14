using SelfHosting.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using SelfHosting.Common;
using SelfHosting.Common.Request;

namespace SelfHosting.Repository
{
    public class CustomerJobRepository : ICustomerJobRepository
    {
        private readonly JobContext _jobContext;
        public CustomerJobRepository(JobContext jobContext)
        {
            _jobContext = jobContext;
        }
        public List<CustomerJob> GetAll() => _jobContext.CustomerJob.Include(x=> x.Job).ToList();

        public async Task<dynamic> AssignJob(AssignJobRequest assignJobRequest)
        {
            var transaction = await _jobContext.Database.BeginTransactionAsync();
            try
            {
                var customerId = _jobContext.Customers.Where(x => x.CustomerCode == assignJobRequest.CustomerCode).Select(x => x.Id).FirstOrDefault();
                if (customerId < 1)
                {
                    throw new InvalidOperationException($"CustomerCode:{assignJobRequest.CustomerCode} bulunamadı !");
                }

                var jobId = _jobContext.Jobs.Where(x => x.Code == assignJobRequest.JobCode).Select(x => x.Id).FirstOrDefault();
                if (jobId < 1)
                {
                    throw new InvalidOperationException($"JobCode:{assignJobRequest.JobCode} bulunamadı !");
                }

                var customerJob = new CustomerJob()
                {
                    CustomerId = customerId,
                    JobId = jobId,
                    Cron = assignJobRequest.CronExp,
                    StartDate = assignJobRequest.StartDate,
                    EndDate = assignJobRequest.EndDate,
                    Active = 1,
                    CreatedBy = 1, //TODO ?
                    CreatedTime = DateTime.Now
                };

                _jobContext.CustomerJob.Add(customerJob);
                var res = await _jobContext.SaveChangesAsync();

                if (res < 1)
                {
                    var exp = new InvalidOperationException("CustomerJob -> Save Error");
                }

                if (assignJobRequest.JobParameters?.Count>0)
                {
                    foreach (var jobItem in assignJobRequest.JobParameters)
                    {
                        var customerJobParam = new CustomerJobParameter()
                        {
                            CustomerJobId = customerJob.Id,
                            ParamSource = jobItem.ParamSource,
                            ParamKey = jobItem.ParamKey,
                            ParamValue = jobItem.ParamValue,
                            Active = 1,
                            CreatedBy = 1, //TODO ?
                            CreatedTime = DateTime.Now
                        };

                        _jobContext.CustomerJobParameter.Add(customerJobParam);
                    }

                    res = await _jobContext.SaveChangesAsync();

                    if (res < 1)
                    {
                        var exp = new InvalidOperationException("CustomerJobParameter -> Save Error");
                    }
                }

                await transaction.CommitAsync();

                return Helper.ReturnOk(customerJob.Id);
            }
            catch (Exception ex)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception)
                {
                }
                return Helper.ReturnError(ex);
            }
        }
    }
}
