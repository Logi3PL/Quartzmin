using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.SendMailJob.DataLayer.Model
{
    [Serializable]
    public class SendDataViewModel
    {
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string SqlQuery { get; set; }
        public string SqlQueryToField { get; set; }
        public string SqlQueryCcField { get; set; }
        public string SqlQueryBccField { get; set; }
        public string SqlQueryConStr { get; set; }
        public bool DetailBodyForAll { get; set; }
        public string DetailSubject { get; set; }
        public string DetailContent { get; set; }
        public string DetailHeader { get; set; }
        public string DetailFooter { get; set; }
        public string DetailSqlQuery { get; set; }
        public string DetailSqlQueryConStr { get; set; }
        public bool DetailQueryForTemplate { get; set; }
    }
}
