using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.SendMailJob.DataLayer.Model
{
    [Serializable]
    public class SendData
    {
        public int Id { get; set; }
        public string ScheduleName { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }
        public byte Type { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string ToSqlQuery { get; set; }
        public string ToSqlQueryConStr { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public byte Active { get; set; }
    }
}
