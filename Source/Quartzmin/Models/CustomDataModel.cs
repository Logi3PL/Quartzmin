using Quartz;
using System;

namespace Quartzmin.Models
{
    [Serializable]
    public struct CustomDataModel
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
