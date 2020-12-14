using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Common.Request
{
    public class AssignJobParameterItem
    {
        public string ParamSource { get; set; }
        public string ParamKey { get; set; }
        public string ParamValue { get; set; }
    }
}
