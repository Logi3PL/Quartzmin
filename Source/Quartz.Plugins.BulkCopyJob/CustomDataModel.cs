using Quartz;
using System;

namespace Quartz.Plugins.ScriptExecuterJob.Models
{
    [Serializable]
    public struct CustomDataModel
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
