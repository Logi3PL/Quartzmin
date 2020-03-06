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
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                { //List<string> tables = new List<string>();  
                    DataTable dt = con.GetSchema("Tables");
                    foreach (DataRow row in dt.Rows)
                    {
                        string tablename = (string)row[1] + "." + (string)row[2];
                        list.Add(new { Selected = false, Name = tablename });
                    }
                }
            }

            return Ok(list);
        }
    }
}
