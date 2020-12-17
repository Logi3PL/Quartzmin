using EmailSendJob.Manager;
using EmailSendJob.Model;
using HandlebarsDotNet;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using RestSharp;
using SelfHosting.API.AppSettings;
using SelfHosting.Common;
using SelfHosting.Common.Request;
using SelfHosting.Repository;
using SelfHosting.Repository.Domain;
using Serilog;
using Slf.NetCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
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

        public SendEmailJob()
        {
            try
            {
                Handlebars.RegisterHelper("formatDateTime", (writer, context, parameters) =>
                {
                    // parameters : datetime iso string, format, culture
                    try
                    {
                        var firstPrm = parameters[0].ToString();

                        if (firstPrm.ToLower() == "now")
                        {
                            firstPrm = DateTime.Now.ToString();
                        }

                        string res;
                        DateTime datetime = DateTime.Parse(firstPrm, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        string format = "dd/MM/yyyy";
                        if (parameters.Count() > 1)
                        {
                            format = parameters[1].ToString();
                        }
                        if (parameters.Count() > 2 && !string.IsNullOrWhiteSpace(parameters[2].ToString()))
                        {
                            string provider = parameters[2].ToString();
                            IFormatProvider formatprovider = null;
                            if (provider.ToLower() == "invariant")
                            {
                                formatprovider = CultureInfo.InvariantCulture;
                            }
                            else
                            {
                                formatprovider = CultureInfo.CreateSpecificCulture(provider);
                            }
                            res = datetime.ToString(format, formatprovider);
                        }
                        else
                        {
                            res = datetime.ToString(format);
                        }

                        writer.WriteSafeString(res);
                    }
                    catch (Exception e)
                    {
                        writer.WriteSafeString("");
                    }
                });
            }
            catch (Exception ex)
            {
                //TODO:?
            }
        }

        public async Task ExecuteJobAsync(IJobExecutionContext context,IServiceProvider serviceProvider,List<AssignJobParameterItem> jobParameterItems, List<AssignJobSubscriberItem> jobSubscriberItems)
        {
            var customerJobIdPrm = jobParameterItems?.Where(x => x.ParamKey == ConstantHelper.SchedulerJobHelper.CustomerJobIdKey).FirstOrDefault();

            Int32.TryParse(customerJobIdPrm.ParamValue, out int customerJobId);

            ICustomerJobHistoryRepository customerJobHistoryRepository = serviceProvider.GetRequiredService<ICustomerJobHistoryRepository>();
            await customerJobHistoryRepository.AddHistory(new AddHistoryRequest() { 
                CustomerJobId = customerJobId,
                ProcessStatus = SelfHosting.Common.JobScheduler.ProcessStatusTypes.Executing,
                ProcessTime = DateTimeOffset.Now
            });

            try
            {
                if (jobSubscriberItems?.Count<1)
                {
                    throw new ArgumentException("JobSubscriberItems required !");
                }

                #region Send Email

                var masterObjIdPrm = jobParameterItems?.Where(x => x.ParamKey == ConstantHelper.SchedulerJobHelper.MasterObjectIdKey).FirstOrDefault();

                var masterObjTypePrm = jobParameterItems?.Where(x => x.ParamKey == ConstantHelper.SchedulerJobHelper.MasterObjectTypeKey).FirstOrDefault();

                var templateContent = jobParameterItems?.Where(x => x.ParamKey == ConstantHelper.SchedulerJobHelper.TemplateKey).FirstOrDefault()?.ParamValue;

                var subjectData = jobParameterItems?.Where(x => x.ParamKey == ConstantHelper.SchedulerJobHelper.SubjectKey).FirstOrDefault()?.ParamValue;


                //TODO?Master object servislerini getir
                var apiUrl = "http://localhost:51500/PMSApi/issue";
                var endPoint = "getbyid";

                var client = new RestClient(apiUrl);

                var request = new RestRequest(endPoint, Method.GET);
                request.AddHeader("parameters", "{'id':" + masterObjIdPrm.ParamValue + "}");

                var tcs = new TaskCompletionSource<IRestResponse>();

                ///Client'ım endpointini tetikliyoruz.
                client.ExecuteAsync(request, response =>
                {
                    tcs.SetResult(response);
                });

                var restResponse = await tcs.Task;

                var template = Handlebars.Compile(templateContent);
                var dataContent = tcs.Task.Result.Content;
                
                var dataFromRemote = JObject.Parse(dataContent);

                var resultJson = dataFromRemote.SelectToken("Result").ToString();

                var remoteDataExp = JsonConvert.DeserializeObject<ExpandoObject>(resultJson);

                var data = new
                {
                    Model = remoteDataExp
                };

                var body = template(data);

                #region Subject Compile
                try
                {
                    var subjectTemplate = Handlebars.Compile(subjectData);

                    subjectData = subjectTemplate(new { });
                }
                catch (Exception ex)
                {
                }
                #endregion

                var scheduleName = context.Scheduler.SchedulerName;
                var jobName = context.JobDetail.Key.Name;
                var jobGroup = context.JobDetail.Key.Group;

                var trgName = context.Trigger.Key.Name;
                var trgGroup = context.Trigger.Key.Group;

                var sendDataItem = new SendDataItem()
                {
                    JobGroup = jobGroup,
                    JobName = jobName,
                    ScheduleName = scheduleName,
                    TriggerGroup = trgGroup,
                    TriggerName = trgName,
                    Type = 1, //TODO:Static - Email/Sms
                    CreatedDate = DateTimeOffset.Now,
                    Active = 1
                };

                var subject = subjectData;
                var bodyContent = body;
                var recipients = new List<string>();

                foreach (var item in jobSubscriberItems.Where(x=>x.SubscriberType == (byte)JobSubscriberType.Assigned))
                {
                    recipients.Add(item.Subscriber);
                }

                sendDataItem.Cc = jobSubscriberItems.Where(x => x.SubscriberType == (byte)JobSubscriberType.Related).Select(x=>x.Subscriber).Aggregate((x, y) => x + ";" + y);

                SendDataBy(serviceProvider, sendDataItem, subject, bodyContent, recipients);

                Log.Information($"SendEmailJob- Execute Methodu çalıştı dönen sonuç => {tcs.Task.Result.StatusCode}");

                #endregion
            }
            catch (Exception ex)
            {
                //TODO:?
            }

            await customerJobHistoryRepository.AddHistory(new AddHistoryRequest()
            {
                CustomerJobId = 1,
                ProcessStatus = SelfHosting.Common.JobScheduler.ProcessStatusTypes.Executed,
                ProcessTime = DateTimeOffset.Now
            });
        }

        private void SendDataBy(IServiceProvider serviceProvider,SendDataItem sendDataItem, string subject, string bodyContent, List<string> recipients)
        {
            var confg = serviceProvider.GetService<IOptions<ConfigParameter>>();

            var conStr = confg.Value.SchedulerConStr;

            var sendDataMailAccounts = SendDataMailAccountManager.GetMailAccounts(conStr);
            var sendDataMailAccount = sendDataMailAccounts.FirstOrDefault();

            #region Send Email
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Action<List<string>> sendMailAction = async (recipientList) =>
            {
                try
                {
                    sendDataItem.Recipient = recipients.Aggregate((x, y) => x + ";" + y);

                    var host = sendDataMailAccount.ServerName;
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(host);

                    var from = sendDataMailAccount.FromMailAddress;
                    mail.From = new MailAddress(from);

                    foreach (var recipient in recipientList.Distinct())
                    {
                        if (recipient.Contains(";") || recipient.Contains(","))
                        {
                            var rcp = recipient.Replace(";", ",").Trim().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            foreach (var rcpItem in rcp)
                            {
                                mail.To.Add(rcpItem);
                            }
                        }
                        else
                        {
                            mail.To.Add(recipient.Trim());
                        }

                    }

                    if (string.IsNullOrEmpty(sendDataItem.Cc) == false)
                    {
                        foreach (var ccItem in sendDataItem.Cc.Replace(";", ",").Trim().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (string.IsNullOrEmpty(ccItem.Trim()) == false)
                            {
                                mail.CC.Add(ccItem.Trim());
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(sendDataItem.Bcc) == false)
                    {
                        foreach (var bccItem in sendDataItem.Bcc.Replace(";", ",").Trim().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (string.IsNullOrEmpty(bccItem.Trim()) == false)
                            {
                                mail.Bcc.Add(bccItem.Trim());
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(sendDataItem.ReplyTo) == false)
                    {
                        foreach (var replyToItem in sendDataItem.ReplyTo.Replace(";", ",").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (string.IsNullOrEmpty(replyToItem.Trim()) == false)
                            {
                                mail.ReplyToList.Add(replyToItem.Trim());
                            }
                        }
                    }

                    mail.Subject = subject;
                    mail.Body = bodyContent;
                    mail.IsBodyHtml = true;

                    SmtpServer.Port = sendDataMailAccount.MailSmtpPort;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(sendDataMailAccount.AccountName, sendDataMailAccount.AccountPass);
                    SmtpServer.EnableSsl = false;

                    SmtpServer.Send(mail);
                    sendDataItem.SentDate = DateTimeOffset.Now;

                }
                catch (Exception ex)
                {
                    sendDataItem.ErrorMsg = ex.Message;

                    LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                    {
                        LoggerName = ConstantHelper.JobLog,
                        Title = "GenerateSendDataItemFrom Error",
                        Message = ex.Message,
                        LogItemProperties = new List<LogItemProperty>() {
                            new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                            new LogItemProperty("ActionName", "SendMailAction"),
                            new LogItemProperty("FormData", new {sendDataItem,recipientList })
                        },
                        LogLevel = LogLevel.Error,
                        Exception = ex
                    });
                }

                var saveDataItem = await InsertSendDataItem(conStr,sendDataItem);
            };

            recipients.AsParallel().ForAll(recipient =>
            {
                sendMailAction(new List<string>() { recipient });
            });

            stopwatch.Stop();

            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
            {
                LoggerName = ConstantHelper.JobLog,
                Title = "SendDataBy Executed",
                Message = "SendDataBy Executed",
                LogItemProperties = new List<LogItemProperty>() {
                                new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                new LogItemProperty("ActionName", "SendDataBy"),
                                new LogItemProperty("ElapsedTimeAssn", stopwatch.Elapsed.TotalSeconds),
                            },
                LogLevel = LogLevel.Trace
            });
            #endregion
        }

        private async Task<int> InsertSendDataItem(string conStr,SendDataItem sendDataDetail)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                using (SqlConnection connection = new SqlConnection(conStr))
                {
                    if (connection.State == System.Data.ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    #region commandStr
                    var insertCommand = @"

INSERT INTO [dbo].[PLG_SENDDATA_ITEMS]
           ([SCHED_NAME]
           ,[JOB_NAME]
           ,[JOB_GROUP]
           ,[TRIGGER_NAME]
           ,[TRIGGER_GROUP]
           ,[TYPE]
           ,[FROM]
           ,[RECIPIENTLIST]
           ,[CC]
           ,[BCC]
           ,[REPLYTO]
           ,[BODY]
           ,[SENTDATE]
           ,[ERRORMSG]
           ,[ACTIVE]
           ,[CREATEDDATE])
     VALUES
           (@SCHED_NAME
           ,@JOB_NAME
           ,@JOB_GROUP
           ,@TRIGGER_NAME
           ,@TRIGGER_GROUP
           ,@TYPE
           ,@FROM
           ,@RECIPIENTLIST
           ,@CC
           ,@BCC
           ,@REPLYTO
           ,@BODY
           ,@SENTDATE
           ,@ERRORMSG
           ,1
           ,@CREATEDDATE)

";
                    #endregion

                    #region Execute Command
                    using (SqlCommand command = new SqlCommand(insertCommand, connection))
                    {
                        command.Parameters.AddWithValue("@SCHED_NAME", sendDataDetail.ScheduleName);
                        command.Parameters.AddWithValue("@JOB_NAME", sendDataDetail.JobName);
                        command.Parameters.AddWithValue("@JOB_GROUP", sendDataDetail.JobGroup);
                        command.Parameters.AddWithValue("@TRIGGER_NAME", sendDataDetail.TriggerName);
                        command.Parameters.AddWithValue("@TRIGGER_GROUP", sendDataDetail.TriggerGroup);
                        command.Parameters.AddWithValue("@TYPE", sendDataDetail.Type);
                        command.Parameters.AddWithValue("@FROM", sendDataDetail.From);
                        command.Parameters.AddWithValue("@RECIPIENTLIST", sendDataDetail.Recipient);
                        command.Parameters.AddWithValue("@CC", sendDataDetail.Cc);
                        command.Parameters.AddWithValue("@BCC", sendDataDetail.Bcc);
                        command.Parameters.AddWithValue("@BODY", sendDataDetail.Body);

                        if (string.IsNullOrEmpty(sendDataDetail.ReplyTo) == false)
                        {
                            command.Parameters.AddWithValue("@REPLYTO", sendDataDetail.ReplyTo);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@REPLYTO", DBNull.Value);
                        }

                        if (sendDataDetail.SentDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@SENTDATE", sendDataDetail.SentDate);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@SENTDATE", DBNull.Value);
                        }

                        if (string.IsNullOrEmpty(sendDataDetail.ErrorMsg) == false)
                        {
                            command.Parameters.AddWithValue("@ERRORMSG", sendDataDetail.ErrorMsg);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@ERRORMSG", DBNull.Value);
                        }

                        command.Parameters.AddWithValue("@CREATEDDATE", sendDataDetail.CreatedDate);

                        var res = await command.ExecuteNonQueryAsync();

                        stopwatch.Stop();

                        if (res < 1)
                        {
                            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                            {
                                LoggerName = ConstantHelper.JobLog,
                                Title = "InsertSendDataItem Executed",
                                Message = "InsertSendDataItem Executed",
                                LogItemProperties = new List<LogItemProperty>() {
                                    new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                    new LogItemProperty("ActionName", "InsertSendDataItem"),
                                    new LogItemProperty("FormData", sendDataDetail),
                                    new LogItemProperty("ElapsedTimeAssn", stopwatch.Elapsed.TotalSeconds),
                                },
                                Exception = new ArgumentException("Insert Failed !"),
                                LogLevel = LogLevel.Error
                            });
                        }
                        else
                        {
                            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                            {
                                LoggerName = ConstantHelper.JobLog,
                                Title = "InsertSendDataItem Executed",
                                Message = "InsertSendDataItem Executed",
                                LogItemProperties = new List<LogItemProperty>() {
                                new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                new LogItemProperty("ActionName", "InsertSendDataItem"),
                                new LogItemProperty("ElapsedTimeAssn", stopwatch.Elapsed.TotalSeconds),
                            },
                                LogLevel = LogLevel.Trace
                            });
                        }

                        return res;
                    }
                    #endregion
                }

            }
            catch (Exception ex)
            {
                LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                {
                    LoggerName = ConstantHelper.JobLog,
                    Title = "InsertSendDataItem Error",
                    Message = ex.Message,
                    LogItemProperties = new List<LogItemProperty>() {
                        new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                        new LogItemProperty("ActionName", "InsertSendDataItem"),
                        new LogItemProperty("FormData", sendDataDetail),
                    },
                    LogLevel = LogLevel.Error,
                    Exception = ex
                });
                return -1;
            }
        }
    }
}
