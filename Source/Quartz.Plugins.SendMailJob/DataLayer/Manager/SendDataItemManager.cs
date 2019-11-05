using Quartz.Plugins.SendMailJob.DataLayer.Model;
using Quartz.Plugins.SendMailJob.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Text;
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
                        
                        command.Parameters.AddWithValue("@ERRORMSG", sendDataDetail.ErrorMsg);
                        command.Parameters.AddWithValue("@CREATEDDATE", sendDataDetail.CreatedDate);

                        var res = await command.ExecuteNonQueryAsync();

                        return res;
                    }
                    #endregion
                }

            }
            catch (Exception ex)
            {
                //TODO:Log
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

                sendDataItem.Bcc = bccData.Value;
                sendDataItem.Body = bodyData.Value;
                sendDataItem.Cc = ccData.Value;
                sendDataItem.Type = 1; //TODO:Static - Email/Sms

                var toDataSource = new DataTable();
                var recipients = toData.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (string.IsNullOrEmpty(sqlqueryData.Value) == false)
                {
                    toDataSource = await SendDataSqlQueryManager.GetQueryData(sqlQueryConnectionString.Value, sqlqueryData.Value);

                    if (toDataSource.Rows.Count > 0)
                    {
                        recipients = new List<string>();

                        for (int i = 0; i < toDataSource.Rows.Count - 1; i++)
                        {
                            var to = toDataSource.Rows[i][sqlqueryToFieldData.Value]?.ToString();

                            if (string.IsNullOrEmpty(to) == false)
                            {
                                recipients.Add(to);
                            }
                        }
                    }
                }

                if (recipients.Count>0)
                {
                    var sendDataMailAccounts = await SendDataMailAccountManager.GetMailAccounts();
                    var sendDataMailAccount = sendDataMailAccounts.FirstOrDefault();

                    if (sendDataMailAccount == null)
                    {
                        //TODO: Log
                        return false;
                    }

                    sendDataItem.MailAccountId = sendDataMailAccount.AccountId;
                    sendDataItem.From = sendDataMailAccount.FromMailAddress;

                    #region Send Email

                    Action<List<string>> sendMailAction = async (recipientList) =>
                    {
                        try
                        {
                            sendDataItem.Recipient = recipientList.Aggregate((x, y) => x + ";" + y);

                            var host = sendDataMailAccount.ServerName;
                            MailMessage mail = new MailMessage();
                            SmtpClient SmtpServer = new SmtpClient(host);

                            var from = sendDataMailAccount.FromMailAddress;
                            mail.From = new MailAddress(sendDataItem.From);
                            
                            foreach (var recipient in recipientList)
                            {
                                mail.To.Add(recipient);
                            }

                            mail.Subject = subjectData.Value;
                            mail.Body = bodyData.Value;
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
                        }

                        var saveDataItem = await InsertSendDataItem(sendDataItem);
                        if (saveDataItem < 1)
                        {
                            //TODO: Logla
                        }
                    };

                    //herkes icin tek template kullan
                    if (useDetailForEveryoneData.Value.ToLower() == "on")
                    {
                        sendMailAction(recipients);
                    }
                    else
                    {
                        recipients.AsParallel().ForAll(recipient =>
                        {
                            sendMailAction(new List<string>() { recipient });
                        });

                        //foreach (var recipient in recipients)
                        //{
                        //    sendMailAction(new List<string>() { recipient });
                        //}
                    } 
                    #endregion
                }

                return true;
            }
            catch (Exception ex)
            {
                //TODO:Log
                return false;
            }
        }
    }
}
