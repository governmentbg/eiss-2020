// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class FileContentVM
    {
        public string FileError { get; set; }
        public string RegNumber { get; set; }
        public DateTime? RegDate { get; set; }

        public byte[] Content { get; set; }
    }
}
