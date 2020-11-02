// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseDeadLineFilterVM
    {
        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Начална дата от")]
        public DateTime? DateStartFrom { get; set; }

        [Display(Name = "Начална дата до")]
        public DateTime? DateStartTo { get; set; }

        [Display(Name = "Крайна дата от")]
        public DateTime? DateEndFrom { get; set; }

        [Display(Name = "Крайна дата до")]
        public DateTime? DateEndTo { get; set; }

        [Display(Name = "Съдия")]
        public int LawUnitId { get; set; }
        
        [Display(Name = "Вид срок")]
        public int DeadlineTypeId { get; set; }

        [Display(Name = "Номер дело")]
        public string RegNumber { get; set; }

        public int CaseId { get; set; }
    }
}
