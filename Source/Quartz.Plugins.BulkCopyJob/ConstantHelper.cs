using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Quartz.Plugins.BulkCopyJob
{
    public struct ConstantHelper
    {
        public const string JobLog = "JOB";
        public const string CustomData = "CustomData";

        public static string GetConnectionString()
        {
            //TODO:Static
            //var conStr = "Data Source=127.0.0.1,1000;Integrated Security=True;Initial Catalog=QUARTZNETJOBDB;UID=sa;PWD=I@mJustT3st1ing;Integrated Security=False";

            var conStr = ConfigurationManager.ConnectionStrings["QUARTZNETJOBDB"]?.ConnectionString;

            return conStr;
        }

        public struct TableActions
        {
            public const string TruncateAdd = "1";
            public const string Add = "2";
        }
    }
}
