// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class CaseLoadElementGroupVM
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string IsNDLabel { get; set; }
        public string CaseInstanceLabel { get; set; }
        public string CaseTypeLabel { get; set; }
        
        [Display(Name = "Начална дата")]
        public DateTime DateStart { get; set; }
        [Display(Name = "Крайна дата")]
        public DateTime? DateEnd { get; set; }
    }
}
