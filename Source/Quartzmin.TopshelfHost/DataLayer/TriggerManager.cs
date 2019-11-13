using Quartz.Plugins.SendMailJob.DataLayer.Model;
using Slf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Plugins.SendMailJob.DataLayer.Manager
{
    public class TriggerManager
    {
        public static List<KeyValuePair<string,string>> FindErrorStateTriggers()
        {
            List<KeyValuePair<string, string>> returnVal = new List<KeyValuePair<string, string>>();

            try
            {
                var conStr = ConfigurationManager.ConnectionStrings["QUARTZNETJOBDB"]?.ConnectionString;

                using (SqlConnection connection = new SqlConnection(conStr))
                {
                    connection.Open();
                    var selectCommand = @"SELECT [TRIGGER_NAME],[TRIGGER_GROUP] FROM [QUARTZNETJOBDB].[dbo].[QRTZ_TRIGGERS] WHERE [TRIGGER_STATE] = 'ERROR'";

                    #region Execute Command
                    using (SqlCommand command = new SqlCommand(selectCommand, connection))
                    {
                        var dataTable = new DataTable();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        // this will query your database and return the result to your datatable
                        da.Fill(dataTable);

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            var name = dataTable.Rows[i]["TRIGGER_NAME"].ToString();
                            var group = dataTable.Rows[i]["TRIGGER_GROUP"].ToString();

                            returnVal.Add(new KeyValuePair<string, string>(name,group));
                        }
                    }

                    connection.Close();
                    #endregion
                }

            }
            catch (Exception ex)
            {
                LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                {
                    LoggerName = ConstantHelper.JobLog,
                    Title = "FindErrorStateTriggers Error",
                    Message = ex.Message,
                    LogItemProperties = new List<LogItemProperty>() {
                                    new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                    new LogItemProperty("ActionName", "FindErrorStateTriggers"),
                                },
                    Exception = ex,
                    LogLevel = LogLevel.Error
                });
            }

            return returnVal;
        }
    }

    
}
