// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActCommandVM
    {
        public virtual CaseSessionActPrintVM CaseSessionActPrint { get; set; }
        public virtual CaseFastProcessViewVM CaseFastProcessView { get; set; }
    }
}
