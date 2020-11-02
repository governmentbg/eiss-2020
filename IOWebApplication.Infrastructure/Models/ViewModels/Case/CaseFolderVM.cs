// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Електронна папка на дело
    /// </summary>
    public class CaseFolderVM : CaseVM
    {
        public List<CaseFolderItemVM> Items { get; set; }

        public CaseFolderVM()
        {
            Items = new List<CaseFolderItemVM>();
        }
    }
}
