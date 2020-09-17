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
        Dictionary<string, string> tableRowCounts ;

        public BulkCopyManager()
        {
            tableRowCounts = new Dictionary<string, string>();
        }

        public async Task<bool> ExecuteQuery(BulkCopyViewModel viewModel)
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

                        if (string.IsNullOrEmpty(item.Target) == false)
                        {
                            tableName = item.Target;
                        }

                        #region Check Exist
                        try
                        {
                            
                            if (ConstantHelper.TableActions.TruncateAdd == item.Action)
                            {
                                // checking whether the table selected from the dataset exists in the database or not
                                var checkTableIfExistsCommand = new SqlCommand("IF EXISTS (SELECT 1 FROM sysobjects WHERE name =  '" + tableName + "' and  xtype = 'U') SELECT 1 ELSE SELECT 0", destConnection);
                                var exists = checkTableIfExistsCommand.ExecuteScalar().ToString().Equals("1");

                                // if does not exist
                                if (!exists)
                                {
                                    var createTableBuilder = new StringBuilder("CREATE TABLE [" + tableName + "]");
                                    createTableBuilder.AppendLine("(");

                                    SqlDataAdapter sqlDataAdapterClmn = new SqlDataAdapter($@"SELECT name, system_type_name, is_nullable FROM
  sys.dm_exec_describe_first_result_set('select * from {item.Name}', NULL, 0)", sourceConnection);
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
                                else
                                {
                                    var createTableCommand = new SqlCommand($"TRUNCATE TABLE {tableName}", destConnection);
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

                            var whereClause = item.Where?.Trim();

                            if (string.IsNullOrEmpty(item.Where) == false)
                            {
                                whereClause = item.Where;

                                if (whereClause.ToLower().StartsWith("where")==false)
                                {
                                    whereClause = "WHERE " + whereClause;
                                }
                            }

                            SqlCommand myCommand = new SqlCommand($"SELECT * FROM {item.Name} {whereClause}", sourceConnection);

                            SqlCommand myRowCommand = new SqlCommand($"SELECT count(*) FROM {item.Name} {whereClause}", sourceConnection);
                            var rowCount = myRowCommand.ExecuteScalar()?.ToString();

                            tableRowCounts[tableName] = rowCount;

                            reader = myCommand.ExecuteReader();

                            // if table exists, just copy the data to the destination table in the database
                            // copying the data from datatable to database table
                            using (var bulkCopy = new SqlBulkCopy(destConnection))
                            {
                                bulkCopy.DestinationTableName = tableName;
                                bulkCopy.BatchSize = 50;
                                bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(OnSqlRowsCopied);
                                bulkCopy.NotifyAfter = 50;
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

        private void OnSqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            var tableName = ((System.Data.SqlClient.SqlBulkCopy)sender).DestinationTableName;

            var rowCaount = this.tableRowCounts[tableName];
            var title = "Kopyalanan kayıt sayısı";

            LoggerService.GetLogger(ConstantHelper.JobLog).Log(new LogItem()
            {
                LoggerName = ConstantHelper.JobLog,
                Title = title,
                Message = title,
                LogItemProperties = new List<LogItemProperty>() {
                    new LogItemProperty("ServiceName", ConstantHelper.JobLog) ,
                    new LogItemProperty("ActionName", "BulkCopy"),
                    new LogItemProperty("TableName", tableName),
                    new LogItemProperty("BulkCopy_Info", $"Kopyalanan kayıt sayısı: {e.RowsCopied}/{rowCaount}")
                },
                LogLevel = LogLevel.Info
            });
        }
    }
}
