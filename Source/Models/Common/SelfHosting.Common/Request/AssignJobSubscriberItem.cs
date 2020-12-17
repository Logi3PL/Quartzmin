using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Common.Request
{
    public class AssignJobSubscriberItem
    {
        public byte SubscriberType { get; set; }
        public string Subscriber { get; set; }
        public string Description { get; set; }
    }
}
