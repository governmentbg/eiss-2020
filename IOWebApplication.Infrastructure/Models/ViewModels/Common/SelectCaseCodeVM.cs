// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class SelectCaseCodeVM
    {
        public string ContainerId { get; set; }
        public int CaseTypeId { get; set; }

        public string SelectCallback { get; set; }
    }
}
