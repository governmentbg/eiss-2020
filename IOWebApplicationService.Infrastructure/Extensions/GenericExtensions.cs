using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Extensions
{
    public static class GenericExtensions
    {
        public static bool IsEmpty(this Guid? value)
        {
            if(value == null || value == Guid.Empty)
            {
                return true;
            }
            return false;
        }
    }
}
