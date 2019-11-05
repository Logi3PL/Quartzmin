using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.SendMailJob.DataLayer.Model
{
    [Serializable]
    public class SendDataMailAccount
    {
        public int AccountId { get; set; }
        public string Title { get; set; }
        public string ServerName { get; set; }
        public string AccountName { get; set; }
        public string AccountPass { get; set; }
        public int MailPop3Port { get; set; }
        public int MailSmtpPort { get; set; }
        public string FromMailAddress { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public byte Active { get; set; }
    }
}
