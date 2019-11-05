using Quartz.Plugins.SendMailJob.DataLayer.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Plugins.SendMailJob.DataLayer.Manager
{
    public class SendDataDetailManager
    {
        public static async Task<int> FindSendDataDetailId(SqlConnection connection, SqlTransaction transaction, SendDataDetail sendDataDetail)
        {
            try
            {
                /*
                 STATUS : 
                 0: Hazır
                 1: Gönderildi
                 2: Gönderilemedi

                 */
                #region commandStr
                var updateCommand = @"

SELECT [ID]  FROM [dbo].[PLG_SENDDATA_DETAILS]

WHERE SENTDATE is not null AND STATUS IN (0,2) AND ACTIVE = 1 
        AND PLG_SENDDATAID = @PLG_SENDDATAID
        AND SUBJECT = @SUBJECT
        AND HEADER = @HEADER
        AND FOOTER = @FOOTER
        AND DETAIL = @DETAIL
        AND RECIPIENTLIST = @RECIPIENTLIST
        AND BODY = @BODY
        AND DETAILSQLQUERY = @DETAILSQLQUERY
        AND DETAILSQLQUERYCONSTR = @DETAILSQLQUERYCONSTR
";

                #endregion

                #region Execute Command
                using (SqlCommand command = new SqlCommand(updateCommand, connection, transaction))
                {
                    command.Parameters.AddWithValue("@PLG_SENDDATAID", sendDataDetail.SendDataId);
                    command.Parameters.AddWithValue("@SUBJECT", sendDataDetail.Subject);
                    command.Parameters.AddWithValue("@HEADER", sendDataDetail.Header);
                    command.Parameters.AddWithValue("@FOOTER", sendDataDetail.Footer);
                    command.Parameters.AddWithValue("@DETAIL", sendDataDetail.Detail);
                    command.Parameters.AddWithValue("@RECIPIENTLIST", sendDataDetail.Recipient);
                    command.Parameters.AddWithValue("@BODY", sendDataDetail.Body);
                    command.Parameters.AddWithValue("@DETAILSQLQUERY", sendDataDetail.DetailToSqlQuery);
                    command.Parameters.AddWithValue("@DETAILSQLQUERYCONSTR", sendDataDetail.DetailToSqlQueryConStr);

                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    try
                    {
                        while (reader.Read())
                        {
                            return (int)reader["ID"];
                        }

                        return -1;
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                //TODO:Log
                return -1;
            }
        }

        public static async Task<int> InsertSendDataDetail(SqlConnection connection,SqlTransaction transaction,SendDataDetail sendDataDetail)
        {
            try
            {
                #region commandStr
                var insertCommand = @"

INSERT INTO [dbo].[PLG_SENDDATA_DETAILS]
           ([PLG_SENDDATAID]
           ,[SUBJECT]
           ,[HEADER]
           ,[FOOTER]
           ,[DETAIL]
           ,[RECIPIENTLIST]
           ,[BODY]
           ,[SENTDATE]
           ,[DETAILSQLQUERY]
           ,[DETAILSQLQUERYCONSTR]
           ,[CREATEDDATE]
           ,[STATUS]
           ,[ACTIVE])
     VALUES
           (@PLG_SENDDATAID
           ,@SUBJECT
           ,@HEADER
           ,@FOOTER
           ,@DETAIL
           ,@RECIPIENTLIST
           ,@BODY
           ,@SENTDATE
           ,@DETAILSQLQUERY
           ,@DETAILSQLQUERYCONSTR
           ,@CREATEDDATE
           ,@STATUS
           ,@ACTIVE
)

";
                #endregion

                #region Execute Command
                using (SqlCommand command = new SqlCommand(insertCommand, connection, transaction))
                {
                    command.Parameters.AddWithValue("@PLG_SENDDATAID", sendDataDetail.SendDataId);
                    command.Parameters.AddWithValue("@SUBJECT", sendDataDetail.Subject);
                    command.Parameters.AddWithValue("@HEADER", sendDataDetail.Header);
                    command.Parameters.AddWithValue("@FOOTER", sendDataDetail.Footer);
                    command.Parameters.AddWithValue("@DETAIL", sendDataDetail.Detail);
                    command.Parameters.AddWithValue("@RECIPIENTLIST", sendDataDetail.Recipient);
                    command.Parameters.AddWithValue("@BODY", sendDataDetail.Body);
                    command.Parameters.AddWithValue("@SENTDATE", sendDataDetail.SentDate);
                    command.Parameters.AddWithValue("@DETAILSQLQUERY", sendDataDetail.DetailToSqlQuery);
                    command.Parameters.AddWithValue("@DETAILSQLQUERYCONSTR", sendDataDetail.DetailToSqlQueryConStr);
                    command.Parameters.AddWithValue("@CREATEDDATE", sendDataDetail.CreatedDate);
                    command.Parameters.AddWithValue("@STATUS", sendDataDetail.Status);
                    command.Parameters.AddWithValue("@ACTIVE", sendDataDetail.Active);

                    var res = await command.ExecuteNonQueryAsync();

                    return res;
                }
                #endregion

            }
            catch (Exception ex)
            {
                //TODO:Log
                return -1;
            }
        }
    }
}
