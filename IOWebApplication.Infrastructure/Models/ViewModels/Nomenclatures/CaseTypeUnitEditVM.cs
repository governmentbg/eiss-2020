// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class CaseTypeUnitEditVM
    {
        public int Id { get; set; }
        public int CaseTypeId { get; set; }
        [Display(Name = "Име")]
        public string Label { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Display(Name = "Активен")]
        public bool IsActive { get; set; }
        [Display(Name = "Начална дата")]
        public DateTime DateStart { get; set; }
        [Display(Name = "Крайна дата")]
        public DateTime? DateEnd { get; set; }
        public virtual ICollection<ListNumberVM> CaseTypeUnitCounts { get; set; }
    }
}
