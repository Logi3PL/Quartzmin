using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Plugins.ScriptExecuterJob.Models
{
    [Serializable]
    public class ScriptExecuterViewModel
    {
        public ScriptType ScriptType { get; set; }
        public string ScriptSource { get; set; }
        public string ConnectionString { get; set; }
    }
}
