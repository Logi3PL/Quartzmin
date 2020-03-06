using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.ScriptExecuterJob.Models
{
    [Serializable]
    public class BulkCopyViewModel
    {
        public ScriptType ScriptType { get; set; }
        public string ScriptSource { get; set; }
        public string ConnectionString { get; set; }
    }
}
