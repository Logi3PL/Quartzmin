using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.WebApiCallJob.Models
{
    [Serializable]
    public class WebApiCallViewModel
    {
        public string WebApiCallUrl { get; set; }
        public string HttpMethod { get; set; }
        public string HttpMethodParamType { get; set; }
        public string HttpMethodContentType { get; set; }
        public string HttpMethodHeader { get; set; }
        public string HttpMethodParameter { get; set; }
        //public bool CheckResponse { get; set; }
        //public string CheckResponseQuery { get; set; }
        //public bool SendResponseAsEmail { get; set; }
    }
}
