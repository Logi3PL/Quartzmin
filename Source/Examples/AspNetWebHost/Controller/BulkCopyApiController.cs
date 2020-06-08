using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Quartz.Plugins.BulkCopyJob.Controller
{
    public class BulkCopyApiController : System.Web.Http.ApiController
    {
        [HttpGet]
        public async Task<IHttpActionResult> LoadTables(string conString)
        {
            List<dynamic> list = new List<dynamic>();
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT  name FROM  SYSOBJECTS WHERE  xtype = 'U'", con))
                { //List<string> tables = new List<string>();  
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable("Tables");

                    sqlDataAdapter.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        string tablename = (string)row[0];

                        using (SqlCommand clmCmd = new SqlCommand($@"SELECT COLUMN_NAME,DATA_TYPE,IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'{tablename}'", con))
                        { 
                            List<dynamic> clmList = new List<dynamic>();
                            //DataTable clmDt = con.GetSchema("Columns");

                            SqlDataAdapter sqlDataAdapterClmn = new SqlDataAdapter(clmCmd);
                            DataTable clmDt = new DataTable("Columns");

                            sqlDataAdapterClmn.Fill(clmDt);

                            foreach (DataRow clmRow in clmDt.Rows)
                            {
                                clmList.Add(new { Selected = false, Name = (string)clmRow[0], Type = (string)clmRow[1], IsNullable = (string)clmRow[2] });
                            }

                            list.Add(new { Selected = false, Name = tablename, Columns = clmList });
                        }
                    }
                }
            }

            var jsonData = JsonConvert.SerializeObject(list);

            return Ok(jsonData);
        }
    }
}
