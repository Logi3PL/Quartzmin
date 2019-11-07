using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.SendMailJob
{
    public struct ConstantHelper
    {
        public const string JobLog = "JOB";
        public const string CustomData = "CustomData";
        public const string DataSourceKey = "DataSource";

        public static string GetConnectionString()
        {
            //TODO:Static
            var conStr = "Data Source=127.0.0.1,1000;Integrated Security=True;Initial Catalog=QuartzNetJobDb;UID=sa;PWD=I@mJustT3st1ing;Integrated Security=False";

            return conStr;
        }

        public struct CustomDataProps
        {
            public const string Id = "id";
            public const string To = "to";
            public const string Cc = "cc";
            public const string Bcc = "bcc";
            public const string Sqlquery = "sqlquery";
            public const string SqlqueryToField = "sqlquerytofield";
            public const string SqlQueryConnectionString = "sqlqueryconstr";
            public const string Subject = "subject";
            public const string Header = "header";
            public const string Body = "body";
            public const string UseSendDataDetailQueryForTemplate = "usesenddatadetailqueryfortemplate";
            public const string UseDetailForEveryone = "usedetailforeveryone";
            public const string Footer = "footer";
            public const string DetailSqlquery = "detailsqlquery";
            public const string DetailSqlQueryConnectionString = "detailsqlqueryconstr";
        }
    }
}
