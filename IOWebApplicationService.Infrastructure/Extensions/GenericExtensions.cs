// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
