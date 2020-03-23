using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.BulkCopyJob.Models
{
    [Serializable]
    public class BulkCopyGridItemViewModel
    {
        public string Name { get; set; }
        public string Action { get; set; }
    }
}
