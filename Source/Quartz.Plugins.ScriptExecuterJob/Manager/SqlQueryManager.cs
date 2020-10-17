using Quartz.Plugins.ScriptExecuterJob;
using Slf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Plugins.ScriptExecuterJob.DataLayer.Manager
{
    public class SqlQueryManager
    {
        public static async Task<bool> ExecuteQuery(string sqlConStr,string executeQuery)
        {
            bool returnValue = false;
            int executeResult = 0;
            try
            {
                using (SqlConnection connectionQueryData = new SqlConnection(sqlConStr))
                {
                    if (connectionQueryData.State == System.Data.ConnectionState.Closed)
                    {
                        connectionQueryData.Open();
                    }

                    try
                    {
                        int count = 0;
                        SqlCommand command = new SqlCommand(executeQuery, connectionQueryData);

                        IAsyncResult result = command.BeginExecuteNonQuery();
                        while (!result.IsCompleted)
                        {
                            Console.WriteLine("Waiting ({0})", count++);
                            System.Threading.Thread.Sleep(50);
                        }
                        Console.WriteLine("Command complete. Affected {0} rows.",
                            command.EndExecuteNonQuery(result));
                    }
                    catch (SqlException exInr)
                    {
                        LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                        {
                            LoggerName = ConstantHelper.JobLog,
                            Title = "ExecuteQuery Error",
                            Message = exInr.Message,
                            LogItemProperties = new List<LogItemProperty>() {
                                new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                new LogItemProperty("ActionName", "ExecuteQuery"),
                                new LogItemProperty("ExecuteResult", executeResult),
                                new LogItemProperty("ExecuteQuery", executeQuery)
                            },
                            LogLevel = LogLevel.Error,
                            Exception = exInr
                        });
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine("Error: {0}", ex.Message);
                    }
                    catch (Exception ex)
                    {
                        // You might want to pass these errors
                        // back out to the caller.
                        Console.WriteLine("Error: {0}", ex.Message);
                    }

                    //#region Execute Command
                    //using (SqlCommand command = new SqlCommand(executeQuery, connectionQueryData))
                    //{
                    //    executeResult = command.ExecuteNonQuery();

                    //    returnValue = true;
                    //}
                    //#endregion

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
                LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                {
                    LoggerName = ConstantHelper.JobLog,
                    Title = "ExecuteQuery Error",
                    Message = ex.Message,
                    LogItemProperties = new List<LogItemProperty>() {
                        new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                        new LogItemProperty("ActionName", "ExecuteQuery"),
                        new LogItemProperty("ExecuteResult", executeResult),
                        new LogItemProperty("ExecuteQuery", executeQuery)
                    },
                    LogLevel = LogLevel.Error,
                    Exception = ex
                });
            }

            return returnValue;
        }
    }
}
