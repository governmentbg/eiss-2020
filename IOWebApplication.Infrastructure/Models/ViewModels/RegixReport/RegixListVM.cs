// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public class RegixListVM
    {
        public int Id { get; set; }
        
        public int CaseId { get; set; }

        [Display(Name = "Вид справка")]
        public string RegixTypeName { get; set; }

        [Display(Name = "Потребител")]
        public string UserName { get; set; }

        [Display(Name = "Дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Акт")]
        public string ActRegNumber { get; set; }

        [Display(Name = "Документ")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Дата и час")]
        public DateTime DateWrt { get; set; }
    }
}
