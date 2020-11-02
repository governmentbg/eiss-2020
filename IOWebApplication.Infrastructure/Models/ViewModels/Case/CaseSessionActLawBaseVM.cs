// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionActLawBaseVM
    {
        public int Id { get; set; }

        public string LawBaseName { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

    }
}
