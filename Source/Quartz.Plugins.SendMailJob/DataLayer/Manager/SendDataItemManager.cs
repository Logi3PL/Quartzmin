using HandlebarsDotNet;
using Newtonsoft.Json;
using Quartz.Plugins.SendMailJob.DataLayer.Model;
using Quartz.Plugins.SendMailJob.Models;
using Slf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
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
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

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

                        stopwatch.Stop();

                        if (res<1)
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
                                Exception =new ArgumentException("Insert Failed !"),
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
                var sqlqueryCcFieldData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.SqlqueryCcField);
                var sqlqueryBccFieldData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.SqlqueryBccField);
                var headerData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Header);
                var footerData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.Footer);
                var detailSqlQueryConnectionString = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.DetailSqlQueryConnectionString);
                var detailSqlqueryData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.DetailSqlquery);
                var useSendDataDetailQueryForTemplateData = customFormDataModel.FirstOrDefault(x => x.Key == ConstantHelper.CustomDataProps.UseSendDataDetailQueryForTemplate);
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
                            var value = item.Value.Replace("\"[", "").Replace("]\"", "").Trim();

                            if (listTemplateTokens.ContainsKey(value) == false)
                            {
                                listTemplateTokens.Add(value, item.Value);
                            }
                            
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
                    LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                    {
                        LoggerName = ConstantHelper.JobLog,
                        Title = "GenerateSendDataItemFrom GetMailAccounts Not Found",
                        Message = "GetMailAccounts Not Found",
                        LogItemProperties = new List<LogItemProperty>() {
                                        new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                        new LogItemProperty("ActionName", "GenerateSendDataItemFrom"),
                                        new LogItemProperty("FormData", new { CustomFormDataModel =customFormDataModel, SendDataItem = sendDataItem})
                                    },
                        LogLevel = LogLevel.Error,
                        Exception = new ArgumentException("GetMailAccounts Not Found")
                    });
                    return false;
                }

                var toDataSource = new DataTable();
                var detailDataSource = new DataTable();

                var recipients = toData.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                /*TODO: 
                 - Validsyon1 => To ve Detail Sql var ama detail sql sonuç dönmüyorsa ?
                 */

                Action<List<string>,string, List<KeyValuePair<string,object>>,string> invokeDetailQuery = async (to,changedBodyContent,columnNames,subject) =>
                {
                    #region Invoke
                    #region Initialize Detail Sqlect Query
                    var detailQuery = detailSqlqueryData.Value.Replace("@[", "@").Replace("]@", "@");
                    List<SqlParameter> parameterList = new List<SqlParameter>();

                    if (columnNames?.Count>0)
                    {
                        foreach (var col in columnNames)
                        {
                            if (detailQuery.Contains($"@{col.Key.ToLower(new System.Globalization.CultureInfo("en-US"))}@") ||
                                detailQuery.Contains($"@{col.Key}@"))
                            {
                                parameterList.Add(new SqlParameter($"@{col.Key}@", col.Value));
                            }
                        }
                    
                        foreach (var col in columnNames)
                        {
                            if (listTemplateTokens.ContainsKey(col.Key))
                            {
                                var oldToken = listTemplateTokens[col.Key];
                                var newValue = col.Value?.ToString();
                                changedBodyContent = changedBodyContent.Replace(oldToken, newValue);
                            }
                        }                    
                    }

                    #endregion

                    detailDataSource = await SendDataSqlQueryManager.GetQueryData(detailSqlQueryConnectionString.Value, detailQuery, parameterList);

                    string[] detailDataSourceColumnNames = detailDataSource.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToArray();

                    var newSendDataItem = JsonConvert.DeserializeObject<SendDataItem>(JsonConvert.SerializeObject(sendDataItem));

                    //detay sorgunun her bir satırı için bir mail mi yoksa detay sorguyu template içinde kullanmak mı ?
                    if (useSendDataDetailQueryForTemplateData.Value?.ToLower() == "on")
                    {

                        changedBodyContent = changedBodyContent.Replace("\"[", "{{{").Replace("]\"", "}}}");

                        var jsonDataSource = JsonConvert.SerializeObject(detailDataSource);
                        var jsonDataSourceExpando = JsonConvert.DeserializeObject<List<ExpandoObject>>(jsonDataSource);
                        var template = Handlebars.Compile(changedBodyContent);

                        changedBodyContent = template(new { DataSource = jsonDataSourceExpando });
                        newSendDataItem.Body = changedBodyContent;
                        newSendDataItem.From = sendDataMailAccount.FromMailAddress;

                        //var to = toDataSource.Rows[i][sqlqueryToFieldData.Value]?.ToString().Trim().Replace("[", "").Replace("]", "");

                        await SendDataBy(sendDataMailAccount, newSendDataItem, subject, changedBodyContent, to, useDetailForEveryoneData.Value);
                    }
                    else
                    {
                        foreach (DataRow item in detailDataSource.Rows)
                        {
                            foreach (var col in detailDataSourceColumnNames)
                            {
                                if (listTemplateTokens.ContainsKey(col))
                                {
                                    var oldToken = listTemplateTokens[col];
                                    var newValue = item[col]?.ToString();
                                    changedBodyContent = changedBodyContent.Replace(oldToken, newValue);
                                }
                                //var val = item[col]?.ToString();
                            }

                            var template = Handlebars.Compile(changedBodyContent);

                            changedBodyContent = template(new { Item = item });
                            newSendDataItem.Body = changedBodyContent;
                            newSendDataItem.From = sendDataMailAccount.FromMailAddress;

                            //var to = toDataSource.Rows[i][sqlqueryToFieldData.Value]?.ToString();

                            await SendDataBy(sendDataMailAccount, newSendDataItem, subject, changedBodyContent, to, useDetailForEveryoneData.Value);
                        }
                    }
                    #endregion
                };

                //string[] toDataSourceColumns
                if (string.IsNullOrEmpty(sqlqueryData.Value) == false)
                {
                    toDataSource = await SendDataSqlQueryManager.GetQueryData(sqlQueryConnectionString.Value, sqlqueryData.Value);
                    if (toDataSource.Rows.Count > 0)
                    {
                        var toFormField = sqlqueryToFieldData.Value?.Trim();
                        var ccFormField = sqlqueryCcFieldData.Value?.Trim();
                        var bccFormField = sqlqueryBccFieldData.Value?.Trim();

                        recipients = new List<string>();

                        var toDataSourceColumnNames = toDataSource.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToList();

                        if (string.IsNullOrEmpty(toFormField) == false && toDataSourceColumnNames.Contains(toFormField) == false)
                        {
                            throw new ArgumentException("TO Field Select row'a ait değil !");
                        }

                        Func<string, string, string, string> replaceStringFromToQuery = (sourceString, key, newData) =>
                         {
                             var changedStr = sourceString.ToString();

                             var contentColumn = "";

                             if (sourceString.Contains(key))
                             {
                                 contentColumn = key;
                             }

                             if (string.IsNullOrEmpty(contentColumn) == false)
                             {
                                 changedStr = sourceString.Replace(contentColumn, newData);
                             }

                             return changedStr;
                         };

                        for (int i = 0; i < toDataSource.Rows.Count; i++)
                        {
                            try
                            {
                                var to = toDataSource.Rows[i][toFormField]?.ToString().Trim().Replace("[", "").Replace("]", "");
                                var ccField = "";
                                var bccField = "";

                                if (string.IsNullOrEmpty(ccFormField) == false)
                                {
                                    ccField = toDataSource.Rows[i][ccFormField]?.ToString().Trim().Replace("[", "").Replace("]", "");
                                }

                                if (string.IsNullOrEmpty(bccFormField) == false)
                                {
                                    bccField = toDataSource.Rows[i][bccFormField]?.ToString().Trim().Replace("[", "").Replace("]", "");
                                }
                                
                                if (string.IsNullOrEmpty(sendDataItem.Cc) && string.IsNullOrEmpty(ccField) == false)
                                {
                                    sendDataItem.Cc = ccField;
                                }

                                if (string.IsNullOrEmpty(sendDataItem.Bcc) && string.IsNullOrEmpty(bccField) == false)
                                {
                                    sendDataItem.Bcc = bccField;
                                }

                                var headerContent = headerData.Value;
                                var footerContent = footerData.Value;
                                var subjectContent = subjectData.Value;

                                var changedBodyContent = bodyContent.ToString();

                                #region Initialize Tokens From To Select Query
                                foreach (var col in toDataSourceColumnNames)
                                {
                                    var newValue = toDataSource.Rows[i][col]?.ToString();

                                    var lowerColumn = $"@{col.ToLower(new System.Globalization.CultureInfo("en-US"))}@";
                                    var column = $"@{col}@";

                                    changedBodyContent = replaceStringFromToQuery(changedBodyContent, lowerColumn, newValue);

                                    changedBodyContent = replaceStringFromToQuery(changedBodyContent, column, newValue);

                                    headerContent = replaceStringFromToQuery(headerContent, lowerColumn, newValue);

                                    footerContent = replaceStringFromToQuery(footerContent, column, newValue);

                                    subjectContent = replaceStringFromToQuery(subjectContent, column, newValue);
                                }
                                #endregion

                                if (string.IsNullOrEmpty(detailSqlqueryData.Value) == false)
                                {
                                    changedBodyContent = changedBodyContent.Replace("\"[HEADER]\"", headerContent);

                                    changedBodyContent = changedBodyContent.Replace("\"[FOOTER]\"", footerContent);

                                    var columnDatas = toDataSourceColumnNames.Select(colName=>new KeyValuePair<string,object>(colName, toDataSource.Rows[i][colName])).ToList();

                                    invokeDetailQuery(new List<string>() { to }, changedBodyContent, columnDatas, subjectContent);
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(to) == false)
                                    {
                                        changedBodyContent = changedBodyContent.Replace("\"[HEADER]\"", headerContent);

                                        changedBodyContent = changedBodyContent.Replace("\"[FOOTER]\"", footerContent);

                                        var newSendDataItem = JsonConvert.DeserializeObject<SendDataItem>(JsonConvert.SerializeObject(sendDataItem));
                                        await SendDataBy(sendDataMailAccount, newSendDataItem, subjectContent, changedBodyContent, new List<string>() { to }, useDetailForEveryoneData.Value);
                                    }

                                }
                            }
                            catch (Exception exFor)
                            {
                                var rowData = "";

                                try
                                {
                                    rowData = JsonConvert.SerializeObject(toDataSource.Rows);
                                }
                                catch (Exception)
                                {

                                }

                                LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                                {
                                    LoggerName = ConstantHelper.JobLog,
                                    Title = "GenerateSendDataItemFrom SqlqueryData Loop Error",
                                    Message = exFor.Message,
                                    LogItemProperties = new List<LogItemProperty>() {
                                        new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                        new LogItemProperty("ActionName", "GenerateSendDataItemFrom"),
                                        new LogItemProperty("FormData", new { CustomFormDataModel =customFormDataModel, SendDataItem = sendDataItem,RowData = rowData,ToDataSourceColumnNames = toDataSourceColumnNames}),
                                    },
                                    LogLevel = LogLevel.Error,
                                    Exception = exFor
                                });
                            }
                        }
                    }
                    else
                    {
                        LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                        {
                            LoggerName = ConstantHelper.JobLog,
                            Title = "GenerateSendDataItemFrom SqlqueryData Row Count = 0",
                            Message = "SqlqueryData Row Count = 0",
                            LogItemProperties = new List<LogItemProperty>() {
                                        new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                        new LogItemProperty("ActionName", "GenerateSendDataItemFrom"),
                                        new LogItemProperty("FormData", new { CustomFormDataModel =customFormDataModel, SendDataItem = sendDataItem}),
                                    },
                            LogLevel = LogLevel.Error,
                            Exception = new ArgumentException("SqlqueryData Row Count = 0")
                        });
                    }

                }
                else if (string.IsNullOrEmpty(detailSqlqueryData.Value) == false)
                {
                    invokeDetailQuery(recipients, bodyContent, null,subjectData.Value);
                }
                else if (recipients.Count > 0)
                {
                    var newSendDataItem = JsonConvert.DeserializeObject<SendDataItem>(JsonConvert.SerializeObject(sendDataItem));
                    await SendDataBy(sendDataMailAccount, newSendDataItem, subjectData.Value, bodyContent, recipients, useDetailForEveryoneData.Value);
                }

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
                        new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
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
                        if (recipient.Contains(";"))
                        {
                            mail.To.Add(recipient.Replace(";", ",").Trim());
                        }
                        else
                        {
                            mail.To.Add(recipient.Trim());
                        }
                        
                    }

                    if (string.IsNullOrEmpty(sendDataItem.Cc) == false)
                    {
                        mail.CC.Add(sendDataItem.Cc);
                    }

                    if (string.IsNullOrEmpty(sendDataItem.Bcc) == false)
                    {
                        mail.Bcc.Add(sendDataItem.Bcc);
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

                var saveDataItem = await InsertSendDataItem(sendDataItem);
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
    }
}
