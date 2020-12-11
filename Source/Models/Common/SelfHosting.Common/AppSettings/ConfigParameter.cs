using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SelfHosting.API.AppSettings
{

    /// <summary>
    /// Worker servisimiz içerisinden CustomerJobları alabilmek için AppSetting.Json dosyasında belirtiğimiz alanları sınıf içerisinde mapliyoruz.
    /// </summary>
    public class ConfigParameter
    {
        public string jobApiUrl { get; set; }
        public string MethodName { get; set; }
        public string SchedulerConStr { get; set; }
    }
}
