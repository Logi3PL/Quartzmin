﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EmailSendJob.Model
{
    [Serializable]
    public class SendDataItem
    {
        public int MailAccountId { get; set; }
        public string ScheduleName { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public string TriggerName { get; set; }
        public string TriggerGroup { get; set; }
        public byte Type { get; set; }
        public string From { get; set; }
        public string Recipient { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string ReplyTo { get; set; }
        public string Body { get; set; }
        public DateTimeOffset? SentDate { get; set; }
        public string ErrorMsg { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public byte Active { get; set; }
    }
}
