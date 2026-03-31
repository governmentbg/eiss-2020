// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class NotificationFileVM
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }

        public bool IsPdf { get; set; }
    }
}
