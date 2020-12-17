using EmailSendJob.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace EmailSendJob.Manager
{
    public class SendDataMailAccountManager
    {
        public static List<SendDataMailAccount> GetMailAccounts(string sqlConStr)
        {
            List<SendDataMailAccount> sendDataMailAccountList = new List<SendDataMailAccount>();

            try
            {
                using (SqlConnection connectionQueryData = new SqlConnection(sqlConStr))
                {
                    if (connectionQueryData.State == System.Data.ConnectionState.Closed)
                    {
                        connectionQueryData.Open();
                    }

                    #region Execute Command
                    var selectQuery = @"

SELECT [MAILACCOUNTID]
      ,[MAILACCOUNTTITLE]
      ,[MAILSERVERNAME]
      ,[MAILACCOUNTNAME]
      ,[MAILACCOUNTPASSWORD]
      ,[MAILPOP3PORT]
      ,[MAILSMTPPORT]
      ,[FROMMAILADDRESS]
  FROM [dbo].[JMS_SENDDATA_MAILACCOUNT]
WHERE ACTIVE = 1 AND ISDEFAULT = 1
";


                    using (SqlCommand command = new SqlCommand(selectQuery, connectionQueryData))
                    {
                        DataTable dataTable = new DataTable();

                        // create data adapter
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        // this will query your database and return the result to your datatable
                        da.Fill(dataTable);

                        for (int i = 0; i < dataTable.Rows.Count ; i++)
                        {
                            var id = Int32.Parse(dataTable.Rows[i]["MAILACCOUNTID"].ToString());
                            var title = dataTable.Rows[i]["MAILACCOUNTTITLE"]?.ToString();
                            var serverName = dataTable.Rows[i]["MAILSERVERNAME"]?.ToString();
                            var accountName = dataTable.Rows[i]["MAILACCOUNTNAME"]?.ToString();
                            var accountPass = dataTable.Rows[i]["MAILACCOUNTPASSWORD"]?.ToString();
                            var pop3Port = Int32.Parse(dataTable.Rows[i]["MAILPOP3PORT"].ToString());
                            var smptpPort = Int32.Parse(dataTable.Rows[i]["MAILSMTPPORT"].ToString());
                            var fromAddress = dataTable.Rows[i]["FROMMAILADDRESS"]?.ToString();

                            var sendDataMailAccount = new SendDataMailAccount() {
                                AccountId = id,
                                AccountName = accountName,
                                AccountPass = accountPass,
                                Active = 1,
                                FromMailAddress = fromAddress,
                                MailPop3Port = pop3Port,
                                MailSmtpPort = smptpPort,
                                ServerName = serverName,
                                Title = title
                            };

                            sendDataMailAccountList.Add(sendDataMailAccount);
                        }
                    }
                    #endregion

                    try
                    {
                        connectionQueryData.Close();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO:Log                
            }

            return sendDataMailAccountList;
        }
    }
}
