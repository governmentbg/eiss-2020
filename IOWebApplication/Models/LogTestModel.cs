// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Models
{
    public class LogTestModel
    {
        public int ID { get; set; }
        [Display(Name = "Ползвател")]
        public string TenantName { get; set; }

        [Display(Name = "Електронна поща")]
        public string Email { get; set; }

        [Display(Name = "Имена")]
        public string FullName { get; set; }

        public LogTestModel()
        {
            this.ID = 42;
        }
    }
}
