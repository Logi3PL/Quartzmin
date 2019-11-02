using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.SendMailJob.DataLayer.Model
{
    [Serializable]
    public class SendDataDetail
    {
        public int Id { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }
        public byte Type { get; set; }
        public string Subject { get; set; }
        public string Recipient { get; set; }
        public string Body { get; set; }
        public DateTimeOffset? SendDate { get; set; }
        public byte Status { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public byte Active { get; set; }
    }
}
