using RestSharp;
using SelfHosting.Common;
using Serilog;
using System;
using System.Threading.Tasks;

namespace EmailSendJob
{
    /// <summary>
    /// SendEmailJob tetiklendiğinde belirtilen Email gönderme api si tetiklenecektir.
    /// </summary>
    public class SendEmailJob : ISchedulerJob
    {
        public Guid Guid => new Guid("A338639F-2174-4383-9297-6A970F1AA020");

        public string Name => "SendEmailJob";

        public async Task ExecuteJobAsync(string apiUrl, string EndPoint)
        {
            var client = new RestClient(apiUrl);

            var request = new RestRequest(EndPoint, Method.GET);

            var tcs = new TaskCompletionSource<IRestResponse>();

            ///Client'ım endpointini tetikliyoruz.
            client.ExecuteAsync(request, response =>
            {
                tcs.SetResult(response);
            });

            var restResponse = await tcs.Task;

            Log.Information($"SendEmailJob- Execute Methodu çalıştı dönen sonuç => {tcs.Task.Result.StatusCode} -- {tcs.Task.Result.Content}");
        }
    }
}
