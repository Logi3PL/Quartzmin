using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.SendMailJob
{
    public struct ConstantHelper
    {
        public const string CustomData = "CustomData";

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
            public const string Detail = "detail";
            public const string Footer = "footer";
            public const string DetailSqlquery = "detailsqlquery";
            public const string DetailSqlQueryConnectionString = "detailsqlqueryconstr";
        }
    }
}
