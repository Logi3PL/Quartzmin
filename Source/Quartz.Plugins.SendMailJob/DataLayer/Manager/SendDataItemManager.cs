using HandlebarsDotNet;
using Newtonsoft.Json;
using Quartz.Plugins.SendMailJob.DataLayer.Model;
using Quartz.Plugins.SendMailJob.Models;
using Slf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quartz.Plugins.SendMailJob.DataLayer.Manager
{
    public class SendDataItemManager
    {
        public static async Task<int> InsertSendDataItem(SendDataItem sendDataDetail)
        {
            try
            {
                var conStr = ConstantHelper.GetConnectionString();
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
                        new LogItemProperty("ServiceName", "JOB") ,
                        new LogItemProperty("ActionName", "InsertSendDataItem"),
                        new LogItemProperty("FormData", sendDataDetail),
                    },
                    LogLevel = LogLevel.Error,
                    Exception = ex
                });
                return -1;
            }
        }

        public static async Task<bool> GenerateSendDataItemFrom(List<CustomDataModel> customFormDataModel, SendDataItem sendDataItem)
        {
            try
            {
                #region Get CustomFormDataModel Props
                var sendDataId = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Id);
                var toData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.To);
                var subjectData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Subject);
                var sqlQueryConnectionString = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.SqlQueryConnectionString);
                var sqlqueryData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Sqlquery);
                var sqlqueryToFieldData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.SqlqueryToField);
                var headerData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Header);
                var footerData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Footer);
                var detailSqlQueryConnectionString = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.DetailSqlQueryConnectionString);
                var detailSqlqueryData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.DetailSqlquery);
                var detailData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Detail);
                var ccData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Cc);
                var bodyData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Body);
                var bccData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Bcc);
                var useDetailForEveryoneData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.UseDetailForEveryone);
                #endregion

                var bodyContent = bodyData.Value;

                var listTemplateTokens = new Dictionary<string,string>();
                var templateTokenRegexMatchRes = Regex.Matches(bodyContent, @"""\[(.*?)]""");
                if (templateTokenRegexMatchRes.Count>0)
                {
                    foreach (Match item in templateTokenRegexMatchRes)
                    {
                        if (item.Success)
                        {
                            listTemplateTokens.Add(item.Value.Replace("\"[","").Replace("]\"","").Trim(), item.Value);
                        }
                    }
                }

                sendDataItem.Bcc = bccData.Value;

                sendDataItem.Cc = ccData.Value;
                sendDataItem.Type = 1; //TODO:Static - Email/Sms

                var sendDataMailAccounts = await SendDataMailAccountManager.GetMailAccounts();
                var sendDataMailAccount = sendDataMailAccounts.FirstOrDefault();

                if (sendDataMailAccount == null)
                {
                    //TODO: Log
                    return false;
                }

                var toDataSource = new DataTable();
                var detailDataSource = new DataTable();

                var recipients = toData.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                //string[] toDataSourceColumns
                if (string.IsNullOrEmpty(sqlqueryData.Value) == false)
                {
                    toDataSource = await SendDataSqlQueryManager.GetQueryData(sqlQueryConnectionString.Value, sqlqueryData.Value);
                    if (toDataSource.Rows.Count > 0)
                    {
                        recipients = new List<string>();

                        string[] toDataSourceColumnNames = toDataSource.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToArray();

                        for (int i = 0; i < toDataSource.Rows.Count; i++)
                        {

                            #region Initialize Tokens From To Select Query
                            foreach (var col in toDataSourceColumnNames)
                            {
                                var bodyContentColumn = "";

                                if (bodyContent.Contains($"@{col.ToLower(new System.Globalization.CultureInfo("en-US"))}@"))
                                {
                                    bodyContentColumn = $"@{col.ToLower(new System.Globalization.CultureInfo("en-US"))}@";
                                }

                                if (bodyContent.Contains($"@{col}@"))
                                {
                                    bodyContentColumn = $"@{col}@";
                                }

                                if (string.IsNullOrEmpty(bodyContentColumn) == false)
                                {
                                    var newValue = toDataSource.Rows[i][col]?.ToString();
                                    bodyContent = bodyContent.Replace(bodyContentColumn, newValue);
                                }
                            } 
                            #endregion

                            if (string.IsNullOrEmpty(detailSqlqueryData.Value) == false)
                            {
                                #region Initialize Detail Sqlect Query
                                var detailQuery = detailSqlqueryData.Value;
                                List<SqlParameter> parameterList = new List<SqlParameter>();

                                foreach (var col in toDataSourceColumnNames)
                                {
                                    if (detailQuery.Contains($"@{col.ToLower(new System.Globalization.CultureInfo("en-US"))}@") ||
                                        detailQuery.Contains($"@{col}@"))
                                    {
                                        parameterList.Add(new SqlParameter($"@{col}@", toDataSource.Rows[i][col]));
                                    }
                                } 
                                #endregion

                                detailDataSource = await SendDataSqlQueryManager.GetQueryData(detailSqlQueryConnectionString.Value, detailQuery, parameterList);

                                string[] detailDataSourceColumnNames = detailDataSource.Columns.Cast<DataColumn>()
                                             .Select(x => x.ColumnName)
                                             .ToArray();

                                var newSendDataItem = JsonConvert.DeserializeObject<SendDataItem>(JsonConvert.SerializeObject(sendDataItem));

                                foreach (DataRow item in detailDataSource.Rows)
                                {
                                    foreach (var col in detailDataSourceColumnNames)
                                    {
                                        if (listTemplateTokens.ContainsKey(col))
                                        {
                                            var oldToken = listTemplateTokens[col];
                                            var newValue = item[col]?.ToString();
                                            bodyContent = bodyContent.Replace(oldToken, newValue);
                                        }
                                        //var val = item[col]?.ToString();
                                    }

                                    var template = Handlebars.Compile(bodyContent);

                                    bodyContent = template(new { Item = item });
                                    newSendDataItem.Body = bodyContent;
                                    newSendDataItem.From = sendDataMailAccount.FromMailAddress;

                                    var to = toDataSource.Rows[i][sqlqueryToFieldData.Value]?.ToString();

                                    await SendDataBy(sendDataMailAccount, newSendDataItem,subjectData.Value,bodyContent,new List<string>() {to },useDetailForEveryoneData.Value); 
                                }
                                
                            }
                            else
                            {
                                var to = toDataSource.Rows[i][sqlqueryToFieldData.Value]?.ToString();

                                if (string.IsNullOrEmpty(to) == false)
                                {
                                    recipients.Add(to);
                                }
                            }
                        }
                    }
                    else
                    {
                        //TODO:
                    }

                }
                if (recipients.Count > 0)
                {
                    var newSendDataItem = JsonConvert.DeserializeObject<SendDataItem>(JsonConvert.SerializeObject(sendDataItem));
                    await SendDataBy(sendDataMailAccount, newSendDataItem, subjectData.Value, bodyContent, recipients, useDetailForEveryoneData.Value);
                }

                #region Old
                //if (recipients.Count > 0)
                //{
                //    var sendDataMailAccounts = await SendDataMailAccountManager.GetMailAccounts();
                //    var sendDataMailAccount = sendDataMailAccounts.FirstOrDefault();

                //    if (sendDataMailAccount == null)
                //    {
                //        //TODO: Log
                //        return false;
                //    }

                //    sendDataItem.MailAccountId = sendDataMailAccount.AccountId;
                //    sendDataItem.From = sendDataMailAccount.FromMailAddress;

                //    #region Send Email

                //    Action<List<string>> sendMailAction = async (recipientList) =>
                //    {
                //        try
                //        {
                //            sendDataItem.Recipient = recipientList.Aggregate((x, y) => x + ";" + y);

                //            var host = sendDataMailAccount.ServerName;
                //            MailMessage mail = new MailMessage();
                //            SmtpClient SmtpServer = new SmtpClient(host);

                //            var from = sendDataMailAccount.FromMailAddress;
                //            mail.From = new MailAddress(sendDataItem.From);

                //            foreach (var recipient in recipientList)
                //            {
                //                mail.To.Add(recipient);
                //            }

                //            if (string.IsNullOrEmpty(sendDataItem.Cc) == false)
                //            {
                //                mail.CC.Add(sendDataItem.Cc);
                //            }

                //            if (string.IsNullOrEmpty(sendDataItem.Bcc) == false)
                //            {
                //                mail.CC.Add(sendDataItem.Bcc);
                //            }

                //            mail.Subject = subjectData.Value;
                //            mail.Body = bodyContent;
                //            mail.IsBodyHtml = true;

                //            SmtpServer.Port = sendDataMailAccount.MailSmtpPort;
                //            SmtpServer.Credentials = new System.Net.NetworkCredential(sendDataMailAccount.AccountName, sendDataMailAccount.AccountPass);
                //            SmtpServer.EnableSsl = false;

                //            SmtpServer.Send(mail);
                //            sendDataItem.SentDate = DateTimeOffset.Now;
                //        }
                //        catch (Exception ex)
                //        {
                //            //TODO:Logla
                //            sendDataItem.ErrorMsg = ex.Message;
                //        }

                //        var saveDataItem = await InsertSendDataItem(sendDataItem);
                //        if (saveDataItem < 1)
                //        {
                //            //TODO: Logla
                //        }
                //    };

                //    //herkes icin tek template kullan
                //    if (useDetailForEveryoneData.Value?.ToLower() == "on")
                //    {
                //        sendMailAction(recipients);
                //    }
                //    else
                //    {
                //        recipients.AsParallel().ForAll(recipient =>
                //        {
                //            sendMailAction(new List<string>() { recipient });
                //        });

                //        //foreach (var recipient in recipients)
                //        //{
                //        //    sendMailAction(new List<string>() { recipient });
                //        //}
                //    }
                //    #endregion
                //} 
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                {
                    LoggerName = ConstantHelper.JobLog,
                    Title = "GenerateSendDataItemFrom Error",
                    Message = ex.Message,
                    LogItemProperties = new List<LogItemProperty>() {
                        new LogItemProperty("ServiceName", "JOB") ,
                        new LogItemProperty("ActionName", "GenerateSendDataItemFrom"),
                        new LogItemProperty("FormData", new { CustomFormDataModel =customFormDataModel, SendDataItem = sendDataItem}),
                    },
                    LogLevel = LogLevel.Error,
                    Exception = ex
                });

                return false;
            }
        }

        private static async Task SendDataBy(SendDataMailAccount sendDataMailAccount, SendDataItem sendDataItem,string subject,string bodyContent, List<string> recipients,string useDetailForEveryoneDataValue)
        {
            #region Send Email

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

                    foreach (var recipient in recipientList)
                    {
                        mail.To.Add(recipient);
                    }

                    if (string.IsNullOrEmpty(sendDataItem.Cc) == false)
                    {
                        mail.CC.Add(sendDataItem.Cc);
                    }

                    if (string.IsNullOrEmpty(sendDataItem.Bcc) == false)
                    {
                        mail.CC.Add(sendDataItem.Bcc);
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
                    //TODO:Logla
                    sendDataItem.ErrorMsg = ex.Message;

                    LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                    {
                        LoggerName = ConstantHelper.JobLog,
                        Title = "GenerateSendDataItemFrom Error",
                        Message = ex.Message,
                        LogItemProperties = new List<LogItemProperty>() {
                            new LogItemProperty("ServiceName", "JOB") ,
                            new LogItemProperty("ActionName", "SendMailAction"),
                            new LogItemProperty("FormData", recipientList),
                        },
                        LogLevel = LogLevel.Error,
                        Exception = ex
                    });
                }

                var saveDataItem = await InsertSendDataItem(sendDataItem);
                if (saveDataItem < 1)
                {
                    //TODO: Logla
                }
            };

            //herkes icin tek template kullan
            if (useDetailForEveryoneDataValue?.ToLower() == "on")
            {
                sendMailAction(recipients);
            }
            else
            {
                recipients.AsParallel().ForAll(recipient =>
                {
                    sendMailAction(new List<string>() { recipient });
                });
            }
            #endregion
        }
    }
}
