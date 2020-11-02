// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

namespace IOWebApplication.Infrastructure.Utils
{
    /// <summary>
    /// String Writer with Encoding set to UTF-8
    /// </summary>
    internal class Utf8StringWriter : StringWriter
    {
        /// <summary>
        /// Setting the encoding to UTF-8
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;
    }
}