using RestSharp;
using SelfHosting.Common;
using SelfHosting.Common.Request;
using SelfHosting.Repository;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailSendJob
{
    /// <summary>
    /// SendEmailJob tetiklendiğinde belirtilen Email gönderme api si tetiklenecektir.
    /// </summary>
    public class SendEmailJob : ISchedulerJob
    {
        public SendEmailJob()
        {
        }

        public Guid Guid => new Guid("A338639F-2174-4383-9297-6A970F1AA020");

        public string Name => "SendEmailJob";

        public async Task ExecuteJobAsync(dynamic customerJobHistoryRepository,string apiUrl, string EndPoint, List<AssignJobParameterItem> jobParameterItems)
        {
            var customerJobIdPrm = jobParameterItems?.Where(x => x.ParamKey == ConstantHelper.SchedulerJobHelper.CustomerJobIdKey).FirstOrDefault();

            Int32.TryParse(customerJobIdPrm.ParamValue, out int customerJobId);

            await customerJobHistoryRepository.AddHistory(new AddHistoryRequest() { 
                CustomerJobId = customerJobId,
                ProcessStatus = SelfHosting.Common.JobScheduler.ProcessStatusTypes.Executing,
                ProcessTime = DateTimeOffset.Now
            });

            var client = new RestClient(apiUrl);

            var request = new RestRequest(EndPoint, Method.GET);

            var tcs = new TaskCompletionSource<IRestResponse>();

            ///Client'ım endpointini tetikliyoruz.
            client.ExecuteAsync(request, response =>
            {
                tcs.SetResult(response);
            });

            var restResponse = await tcs.Task;

            await customerJobHistoryRepository.AddHistory(new AddHistoryRequest()
            {
                CustomerJobId = 1,
                ProcessStatus = SelfHosting.Common.JobScheduler.ProcessStatusTypes.Executed,
                ProcessTime = DateTimeOffset.Now
            });

            Log.Information($"SendEmailJob- Execute Methodu çalıştı dönen sonuç => {tcs.Task.Result.StatusCode} -- {tcs.Task.Result.Content}");
        }
    }
}
