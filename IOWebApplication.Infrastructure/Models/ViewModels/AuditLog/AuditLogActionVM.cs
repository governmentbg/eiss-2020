// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.AuditLog
{
    public class AuditLogActionVM
    {
        public string UserName { get; set; }
        public string ViewName { get; set; }
        public string ActionName { get; set; }
        public string HttpMethod { get; set; }
        public string RequestUrl { get; set; }
    }
}
