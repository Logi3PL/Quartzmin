using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SelfHosting.Common
{
    public class Helper
    {
        public static dynamic ReturnOk(dynamic data)
        {
            return Task.FromResult(
                    new
                    {
                        Errors = new List<dynamic>(),
                        ResponseStatus = true,
                        Result = data
                    }
                );
        }

        public static dynamic ReturnError(Exception ex)
        {
            return Task.FromResult(
                    new
                    {
                        Errors = new List<dynamic>()
                        {
                            new { ex.Message}
                        },
                        ResponseStatus = false,
                        Result = default(Object)
                    }
                );
        }
    }
}
