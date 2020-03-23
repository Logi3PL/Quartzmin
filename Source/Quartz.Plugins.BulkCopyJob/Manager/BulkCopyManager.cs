using Quartz.Plugins.BulkCopyJob;
using Quartz.Plugins.BulkCopyJob.Models;
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
    public class BulkCopyManager
    {
        public static async Task<bool> ExecuteQuery(BulkCopyViewModel viewModel)
        {
            bool returnValue = false;

            try
            {
                using (SqlConnection destConnection = new SqlConnection(viewModel.DestinationConnectionString))
                using (SqlConnection sourceConnection = new SqlConnection(viewModel.SourceConnectionString))
                {
                    if (destConnection.State == System.Data.ConnectionState.Closed)
                    {
                        destConnection.Open();
                    }

                    if (sourceConnection.State == System.Data.ConnectionState.Closed)
                    {
                        sourceConnection.Open();
                    }

                    #region For Each Table
                    foreach (var item in viewModel.ChangedItems)
                    {
                        var tableName = item.Name;

                        #region Check Exist
                        try
                        {
                            
                            if (ConstantHelper.TableActions.TruncateAdd == item.Action)
                            {
                                // checking whether the table selected from the dataset exists in the database or not
                                var checkTableIfExistsCommand = new SqlCommand("IF EXISTS (SELECT 1 FROM sysobjects WHERE name =  '" + tableName + "') SELECT 1 ELSE SELECT 0", destConnection);
                                var exists = checkTableIfExistsCommand.ExecuteScalar().ToString().Equals("1");

                                // if does not exist
                                if (!exists)
                                {
                                    var createTableBuilder = new StringBuilder("CREATE TABLE [" + tableName + "]");
                                    createTableBuilder.AppendLine("(");

                                    SqlDataAdapter sqlDataAdapterClmn = new SqlDataAdapter($@"SELECT name, system_type_name, is_nullable FROM
  sys.dm_exec_describe_first_result_set('select * from {tableName}', NULL, 0)", sourceConnection);
                                    DataTable clmDt = new DataTable("Columns");

                                    sqlDataAdapterClmn.Fill(clmDt);

                                    foreach (DataRow clmRow in clmDt.Rows)
                                    {
                                        var nullStr = "NULL";

                                        if (clmRow[2].ToString() == "1")
                                        {
                                            nullStr = "NOT NULL";
                                        }

                                        createTableBuilder.AppendLine($"  [{(string)clmRow[0]}] {(string)clmRow[1]} {nullStr},");
                                    }

                                    createTableBuilder.Remove(createTableBuilder.Length - 1, 1);
                                    createTableBuilder.AppendLine(")");

                                    var createTableCommand = new SqlCommand(createTableBuilder.ToString(), destConnection);
                                    createTableCommand.ExecuteNonQuery();
                                }
                            }
                            
                        }
                        catch (Exception exTbl)
                        {
                            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                            {
                                LoggerName = ConstantHelper.JobLog,
                                Title = "Check Exist Error",
                                Message = exTbl.Message,
                                LogItemProperties = new List<LogItemProperty>() {
                                    new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                    new LogItemProperty("ActionName", "ExecuteQuery"),
                                    new LogItemProperty("TableName", tableName),
                                    new LogItemProperty("ViewModel", viewModel)
                                },
                                LogLevel = LogLevel.Error,
                                Exception = exTbl
                            });
                        }

                        #endregion

                        SqlDataReader reader = null;
                        try
                        {
                            #region Execute Command

                            if (sourceConnection.State == System.Data.ConnectionState.Closed)
                            {
                                sourceConnection.Open();
                            }

                            SqlCommand myCommand = new SqlCommand($"SELECT * FROM {tableName}", sourceConnection);

                            reader = myCommand.ExecuteReader();

                            // if table exists, just copy the data to the destination table in the database
                            // copying the data from datatable to database table
                            using (var bulkCopy = new SqlBulkCopy(destConnection))
                            {
                                bulkCopy.DestinationTableName = tableName;
                                bulkCopy.WriteToServer(reader);
                            }

                            #endregion
                        }
                        catch (Exception exTbl)
                        {
                            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
                            {
                                LoggerName = ConstantHelper.JobLog,
                                Title = "BbulkCopy Error",
                                Message = exTbl.Message,
                                LogItemProperties = new List<LogItemProperty>() {
                                    new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                                    new LogItemProperty("ActionName", "ExecuteQuery"),
                                    new LogItemProperty("TableName", tableName),
                                    new LogItemProperty("ViewModel", viewModel)
                                },
                                LogLevel = LogLevel.Error,
                                Exception = exTbl
                            });
                        }
                        finally
                        {
                            if (reader!=null)
                            {
                                try
                                {
                                    reader.Close();
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                    #endregion

                    try
                    {
                        destConnection.Close();
                        sourceConnection.Close();
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
                        new LogItemProperty("ViewModel", viewModel)
                    },
                    LogLevel = LogLevel.Error,
                    Exception = ex
                });
            }

            return returnValue;
        }
    }
}
