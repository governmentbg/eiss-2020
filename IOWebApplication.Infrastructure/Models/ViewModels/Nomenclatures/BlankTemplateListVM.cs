// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class BlankTemplateListVM
    {
        public int Id { get; set; }

        public int SourceType { get; set; }
        public string SourceTypeText
        {
            get
            {
                return SourceTypeSelectVM.GetSourceTypeName(this.SourceType);
            }
        }

        public int SourceId { get; set; }
        public string SourceIdText { get; set; }

        public string Label { get; set; }

        public bool IsActive { get; set; }
    }
}
