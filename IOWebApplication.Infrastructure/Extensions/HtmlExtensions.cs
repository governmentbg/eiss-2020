// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Extensions.HTML
{
    public static class HtmlExtensions
    {
        public static string Decode(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return System.Web.HttpUtility.HtmlDecode(value);
        }
    }
}
