// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Text.RegularExpressions;

namespace IOWebApplication.Core.Extensions.XML
{
    public static class XmlExtensions
    {
        /// <summary>
        /// Премахва специалните UNICODE символи от текстове, които пречат на сериализирането в XML
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ClearXmlText(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var rg = new Regex("[\u0000-\u0008\u000C-\u001F]");
            return rg.Replace(value, "");
        }
    }
}
