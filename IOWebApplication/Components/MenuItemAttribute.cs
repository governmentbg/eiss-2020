// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    public class MenuItemAttribute : Attribute
    {
        public string Value { get; set; }

        public MenuItemAttribute(string value)
        {
            this.Value = value;
        }
    }
}
