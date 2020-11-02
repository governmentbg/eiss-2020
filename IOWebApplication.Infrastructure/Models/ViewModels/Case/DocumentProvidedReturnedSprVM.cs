// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class DocumentProvidedReturnedSprVM
    {
        public int Id { get; set; }
        public string CaseInfo { get; set; }
        public string DocumentInfo { get; set; }
        public string StateLabel { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
    }
}
