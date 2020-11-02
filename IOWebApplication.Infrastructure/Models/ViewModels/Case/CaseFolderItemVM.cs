// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Елемент от електронна папка на дело
    /// </summary>
    public class CaseFolderItemVM
    {
        public string ItemName { get; set; }
        public int SourceType { get; set; }
        public string SourceId { get; set; }
        public DateTime? Date { get; set; }
        public string Title { get; set; }
        public string TypeName { get; set; }
        public string Label { get; set; }

        public string ElementUrl { get; set; }
    }
}
