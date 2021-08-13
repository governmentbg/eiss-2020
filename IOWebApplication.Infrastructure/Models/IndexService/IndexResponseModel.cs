// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.IndexService
{
    public class IndexResponseModel
    {
        public bool SendOk { get; set; }
        public string Id { get; set; }
        public string ErrorMessage { get; set; }
    }
}
