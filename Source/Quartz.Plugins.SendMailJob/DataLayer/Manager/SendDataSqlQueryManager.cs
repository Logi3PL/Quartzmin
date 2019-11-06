using Quartz.Plugins.SendMailJob.DataLayer.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Plugins.SendMailJob.DataLayer.Manager
{
    public class SendDataSqlQueryManager
    {
        public static async Task<DataTable> GetQueryData(string sqlConStr,string selectQuery, List<SqlParameter> sqlParameters = null)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (SqlConnection connectionQueryData = new SqlConnection(sqlConStr))
                {
                    if (connectionQueryData.State == System.Data.ConnectionState.Closed)
                    {
                        connectionQueryData.Open();
                    }
                    
                    #region Execute Command
                    using (SqlCommand command = new SqlCommand(selectQuery, connectionQueryData))
                    {
                        if (sqlParameters?.Count>0)
                        {
                            foreach (var item in sqlParameters)
                            {
                                command.Parameters.Add(new SqlParameter(item.ParameterName, item.Value));
                            }
                        }
                        // create data adapter
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        // this will query your database and return the result to your datatable
                        da.Fill(dataTable);
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

            return dataTable;
        }
    }
}
