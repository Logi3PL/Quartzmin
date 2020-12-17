using System;
using System.Collections.Generic;
using System.Text;

namespace SelfHosting.Common
{
    public struct ConstantHelper
    {
        public const string JobLog = "JMS";
        public const string ServiceKey = "ServiceName";
        public const string JobNamePrefix = "job-";
        public const string JobGroupPrefix = "group-";

        public struct SchedulerJobHelper
        {
            public const string CustomerJobIdKey = "CustomerJobId";
            public const string CallerAppNameKey = "CallerAppName";
            public const string MasterObjectIdKey = "MASTEROBJECTID";
            public const string MasterObjectTypeKey = "MASTEROBJECTTYPE";
            public const string TemplateKey = "TEMPLATE";
            public const string SubjectKey = "SUBJECT";

            public struct SchedulerJobDataMapHelper
            {
                public const string SchedulerJobNameKey = "SchedulerJobName";
                public const string SchedulerJobPathRootKey = "SchedulerJobPathRoot";
                public const string SchedulerJobParametersKey = "SchedulerJobParameters";
                public const string SchedulerJobSubscribersKey = "SchedulerJobSubscribers";
            }
        }

    }
}
