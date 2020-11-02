// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Infrastructure.Models
{
    public class EisppDropDownVM
    {
        public string Label;
        public int Flags;
        public List<SelectListItem> DDList;
        public string GetSelectedText(int? selected)
        {
            return DDList?.Where(x => x.Value == selected.ToString()).FirstOrDefault()?.Text ?? "";
        }
    }
}
