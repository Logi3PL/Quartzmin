using Newtonsoft.Json;
using Quartz.Plugins.WebApiCallJob;
using Quartz.Plugins.WebApiCallJob.Models;
using Slf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System.IO.Compression;

#if NETSTANDARD
//using Slf.NetCore;
#endif
#if NETFRAMEWORK
using Slf;
#endif

namespace Quartz.Plugins
{
    //NOTE:WebApiCall class ismi view'de check edilmekte !
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class WebApiCallJobDefinition : IJob
    {
        private static readonly Random Random = new Random();

        public async Task Execute(IJobExecutionContext context)
        {
            var schedulerName = context.Scheduler.SchedulerName;

            var scheduleName = context.Scheduler.SchedulerName;
            var jobName = context.JobDetail.Key.Name;
            var jobGroup = context.JobDetail.Key.Group;

            var trgName = context.Trigger.Key.Name;
            var trgGroup = context.Trigger.Key.Group;

            var jobDataKeys = context.JobDetail.JobDataMap.GetKeys();
            var customFormDataModel = new List<CustomDataModel>();

            if (jobDataKeys.Contains(ConstantHelper.CustomData))
            {
                var customFormData = context.JobDetail.JobDataMap.GetString(ConstantHelper.CustomData);
                customFormDataModel = JsonConvert.DeserializeObject<List<CustomDataModel>>(customFormData);
            }

            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
            {
                LoggerName = ConstantHelper.JobLog,
                Title = "WebApiCallJobDefinition Started",
                Message = "WebApiCallJobDefinition Started",
                LogItemProperties = new List<LogItemProperty>() {
                    new LogItemProperty("ServiceName", ConstantHelper.JobLog),
                    new LogItemProperty("ScheduleName", scheduleName),
                    new LogItemProperty("JobName", jobName),
                    new LogItemProperty("JobGroup", jobGroup),
                    new LogItemProperty("TriggerName", trgName),
                    new LogItemProperty("TriggerGroup", trgGroup)
                },
                LogLevel = LogLevel.Info
            });
            //Debug.WriteLine("DummyJob > " + DateTime.Now);

            if (customFormDataModel.Count>0)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    #region Get CustomFormDataModel Props
                    var httpMethodData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.HttpMethod);
                    var httpMethodHeader = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.HttpMethodHeader);
                    var httpMethodParameters = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.HttpMethodParameters);
                    var httpMethodParameterType = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.HttpMethodParameterType);
                    var urlData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Url);
                    var mediaTypeData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.MediaType);
                    #endregion

                    #region Send Call
                    
                    var headers = httpMethodHeader.Value.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => 
                    {
                        var firstIndexOfSplitter = x.IndexOf(':');
                        var headerKey = x.Substring(0, firstIndexOfSplitter);
                        var headerValue = x.Replace(headerKey, "");
                        return new { Key = headerKey, Value = headerValue };

                        //var splittedHeaderItem = x.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                        //return new { Key = splittedHeaderItem[0], Value = splittedHeaderItem[1] };

                    }).ToList();

                    var url = urlData.Value;
                    var httpMethod = httpMethodData.Value;
                    
                    using (HttpClient client = new System.Net.Http.HttpClient())
                    {
                        if (headers.Count > 0)
                        {
                            foreach (var headerItem in headers)
                            {
                                if (headerItem.Key != "Content-Type")
                                {
                                    client.DefaultRequestHeaders.Add(headerItem.Key, headerItem.Value);
                                }
                            }
                        }

                        var builder = new UriBuilder(new Uri(url));
                        System.Net.Http.HttpMethod httpMethodParam = System.Net.Http.HttpMethod.Get;

                        switch (httpMethod.ToLower())
                        {
                            case "post":
                                httpMethodParam = HttpMethod.Post;
                                break;
                            case "put":
                                httpMethodParam = HttpMethod.Put;
                                break;
                            case "delete":
                                httpMethodParam = HttpMethod.Delete;
                                break;
                            default:
                                break;
                        }

                        HttpRequestMessage request = new HttpRequestMessage(httpMethodParam, builder.Uri);

                        if (string.IsNullOrEmpty(httpMethodParameters.Value.Trim()) == false)
                        {
                            var content = new StringContent(httpMethodParameters.Value.Trim(), Encoding.UTF8, mediaTypeData.Value);
                            request.Content = content;//CONTENT-TYPE header
                        }

                        HttpResponseMessage response = await client.SendAsync(request);

                        var contents = "";

                        if (response.IsSuccessStatusCode == false)
                        {
                            contents = await response.Content.ReadAsStringAsync();
                        }

                        //if (response.IsSuccessStatusCode == true)
                        //{
                        //    var contents = await response.Content.ReadAsStringAsync();

                        //    //dynamic objectContent = JObject.Parse(contents);
                        //    returnResponse.Data = JObject.Parse(contents);
                        //}

                        //returnResponse.StatusCode = response.StatusCode;

                        LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                        {
                            LoggerName = ConstantHelper.JobLog,
                            Title = $"WebApiCallJobDefinition Call Finished",
                            Message = "WebApiCallJobDefinition Finished",
                            LogItemProperties = new List<LogItemProperty>() {
                                new LogItemProperty("ServiceName", ConstantHelper.JobLog),
                                new LogItemProperty("ScheduleName", scheduleName),
                                new LogItemProperty("JobName", jobName),
                                new LogItemProperty("JobGroup", jobGroup),
                                new LogItemProperty("TriggerName", trgName),
                                new LogItemProperty("TriggerGroup", trgGroup),
                                new LogItemProperty("CallStatus", response.IsSuccessStatusCode),
                                new LogItemProperty("FormData", new { CustomFormDataModel =customFormDataModel, CallResponse = contents})
                            },
                            LogLevel = LogLevel.Info
                        });
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                    {
                        LoggerName = ConstantHelper.JobLog,
                        Title = "WebApiCallJobDefinition Execution Error",
                        Message = ex.Message,
                        LogItemProperties = new List<LogItemProperty>() {
                                        new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                        new LogItemProperty("ActionName", "GenerateSendDataItemFrom"),
                                        //new LogItemProperty("FormData", new { CustomFormDataModel =customFormDataModel, SendDataItem = sendDataItem}),
                                    },
                        LogLevel = LogLevel.Error,
                        Exception = ex
                    });
                }

                stopwatch.Stop();

            }

        }
    }
}
