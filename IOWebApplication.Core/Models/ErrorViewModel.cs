// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Core.Models
{
    public class ErrorViewModel
    {
        public string Title { get; set; }
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string Message { get; set; }
        public string InnerException { get; set; }

        public ErrorViewModel()
        {
            Title = "Грешка";
        }
    }
}