// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseProceedingsObjectsVM
    {
        public int Id { get; set; }
        public long LongId { get; set; }
        public int? ParentId { get; set; }
        public DateTime Date { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
    }
}
