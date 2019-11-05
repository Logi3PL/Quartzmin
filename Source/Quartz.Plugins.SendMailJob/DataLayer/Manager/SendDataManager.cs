using Quartz.Plugins.SendMailJob.DataLayer.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Plugins.SendMailJob.DataLayer.Manager
{
    public class SendDataManager
    {
        public static async Task<int> FindSendDataId(SqlConnection connection, SqlTransaction transaction, SendData sendData)
        {
            try
            {
                #region commandStr
                var updateCommand = @"

SELECT ID FROM [dbo].[PLG_SENDDATAS]
WHERE [SCHED_NAME] = @SCHED_NAME
      AND [JOB_NAME] = @JOB_NAME
      AND [JOB_GROUP] =@JOB_GROUP      
      AND [TYPE] = @TYPE
      AND [TO] = @TO
      AND [CC] = @CC
      AND [BCC] = @BCC
      AND [TOSQLQUERY] = @TOSQLQUERY
      AND [TOSQLQUERYCONSTR] = @TOSQLQUERYCONSTR
      AND [ACTIVE] = 1
";

                #endregion

                #region Execute Command
                using (SqlCommand command = new SqlCommand(updateCommand, connection, transaction))
                {
                    command.Parameters.AddWithValue("@SCHED_NAME", sendData.ScheduleName);
                    command.Parameters.AddWithValue("@JOB_NAME", sendData.JobName);
                    command.Parameters.AddWithValue("@JOB_GROUP", sendData.JobGroup);
                    command.Parameters.AddWithValue("@TYPE", sendData.Type);
                    command.Parameters.AddWithValue("@TO", sendData.To);
                    command.Parameters.AddWithValue("@CC", sendData.Cc);
                    command.Parameters.AddWithValue("@BCC", sendData.Bcc);
                    command.Parameters.AddWithValue("@TOSQLQUERY", sendData.ToSqlQuery);
                    command.Parameters.AddWithValue("@TOSQLQUERYCONSTR", sendData.ToSqlQueryConStr);

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

        public static int InsertSendData(SqlConnection connection, SqlTransaction transaction, SendData sendData)
        {
            try
            {
                #region commandStr
                var insertCommand = @"

    INSERT INTO [dbo].[PLG_SENDDATAS]
           ([SCHED_NAME]
           ,[JOB_NAME]
           ,[JOB_GROUP]
           ,[TRIGGER_NAME]
           ,[TRIGGER_GROUP]
           ,[TYPE]
           ,[TO]
           ,[CC]
           ,[BCC]
           ,[TOSQLQUERY]
           ,[TOSQLQUERYCONSTR]
           ,[CREATEDDATE]
           ,[ACTIVE])
     VALUES
           (@SCHED_NAME
           ,@JOB_NAME
           ,@JOB_GROUP
           ,@TRIGGER_NAME
           ,@TRIGGER_GROUP
           ,@TYPE
           ,@TO
           ,@CC
           ,@BCC
           ,@TOSQLQUERY
           ,@TOSQLQUERYCONSTR
           ,@CREATEDDATE
           ,@ACTIVE)

";

                #endregion

                #region Execute Command
                using (SqlCommand command = new SqlCommand(insertCommand, connection, transaction))
                {
                    command.Parameters.AddWithValue("@SCHED_NAME", sendData.ScheduleName);
                    command.Parameters.AddWithValue("@JOB_NAME", sendData.JobName);
                    command.Parameters.AddWithValue("@JOB_GROUP", sendData.JobGroup);
                    command.Parameters.AddWithValue("@TRIGGER_NAME", sendData.TriggerName);
                    command.Parameters.AddWithValue("@TRIGGER_GROUP", sendData.TriggerGroup);
                    command.Parameters.AddWithValue("@TYPE", sendData.Type);
                    command.Parameters.AddWithValue("@TO", sendData.To);
                    command.Parameters.AddWithValue("@CC", sendData.Cc);
                    command.Parameters.AddWithValue("@BCC", sendData.Bcc);
                    command.Parameters.AddWithValue("@TOSQLQUERY", sendData.ToSqlQuery);
                    command.Parameters.AddWithValue("@TOSQLQUERYCONSTR", sendData.ToSqlQueryConStr);
                    command.Parameters.AddWithValue("@CREATEDDATE", sendData.CreatedDate);
                    command.Parameters.AddWithValue("@ACTIVE", sendData.Active);

                    var res = command.ExecuteNonQuery();

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

        public static async Task<bool> GenerateSendDataDetailFrom(SqlConnection connection, SqlTransaction transaction,string sqlQueryConnectionString, string toData,string sqlqueryData,string sqlqueryToField, SendDataDetail sendDataDetail)
        {
            try
            {
                var toDataSource = new DataTable();
                var recipients = toData.Split(new[] {";"},StringSplitOptions.RemoveEmptyEntries).ToList();

                if (string.IsNullOrEmpty(sqlqueryData) == false)
                {
                    toDataSource = await SendDataSqlQueryManager.GetQueryData(sqlQueryConnectionString, sqlqueryData);

                    if (toDataSource.Rows.Count>0)
                    {
                        recipients = new List<string>();

                        for (int i = 0; i < toDataSource.Rows.Count-1; i++)
                        {
                            var to = toDataSource.Rows[i][sqlqueryToField]?.ToString();

                            if (string.IsNullOrEmpty(to) == false)
                            {
                                recipients.Add(to);
                            }
                        }
                    }
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
