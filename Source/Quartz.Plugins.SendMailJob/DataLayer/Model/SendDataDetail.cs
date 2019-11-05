using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.SendMailJob.DataLayer.Model
{
    [Serializable]
    public class SendDataDetail
    {
        public int Id { get; set; }
        public int SendDataId { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public string Subject { get; set; }
        public string Detail { get; set; }
        public string Recipient { get; set; }
        public string Body { get; set; }
        public DateTimeOffset? SentDate { get; set; }
        public string DetailToSqlQuery { get; set; }
        public string DetailToSqlQueryConStr { get; set; }
        public byte Status { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public byte Active { get; set; }
    }
}
