using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.BulkCopyJob.Models
{
    [Serializable]
    public class BulkCopyViewModel
    {
        public string SourceConnectionString { get; set; }
        public string DestinationConnectionString { get; set; }

        public List<BulkCopyGridItemViewModel> ChangedItems { get; set; }
    }
}
