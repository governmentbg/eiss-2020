
// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplicationApi.Helper
{
    public static class DateTimeExtension
    {
        public static DateTime ConvertUtcToBGTime(this DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc || dt.Kind == DateTimeKind.Local)
            {
                dt = dt.ToUniversalTime();
                var bgTimeZone = DateTimeZoneProviders.Tzdb["Europe/Sofia"];
                dt = Instant.FromDateTimeUtc(dt)
                              .InZone(bgTimeZone)
                              .ToDateTimeUnspecified();
            }

            return dt;
        }

    }
}
