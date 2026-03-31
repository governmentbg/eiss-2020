// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSelectionChangeVM
    {
        public int Id { get; set; }
        public int ChangeTypeId { get; set; }
        public string ChangeTypeLabel { get; set; }
        public string CourtGroupLabel { get; set; }
        public string FromLawunitName { get; set; }
        public string ToLawunitName { get; set; }
        public string ToLawunitDepartmentName { get; set; }
        public DateTime DateWrt { get; set; }
        public string UserName { get; set; }
        public string StateName { get; set; }
    }

    public class CaseSelectionChangeListVM
    {
        public int CaseId { get; set; }
        public string RegNumber { get; set; }
        public DateTime RegDate { get; set; }
        public string[] Lawunits { get; set; }
        public string DepartmentName { get; set; }
        public string CaseTypeLabel { get; set; }
        public string CaseCodeLabel { get; set; }
        public string ProcessPriorityLabel { get; set; }
        public string DepartmentOtdelenieText { get; set; }
        public string CaseStateLabel { get; set; }
    }

    public class CaseSelectionChangeFilterVM
    {
        public int Id { get; set; }

        [Display(Name = "Вид преразпределение")]
        public int ChangeTypeId { get; set; }

        [Display(Name = "Група за разпределяне")]
        public int CourtGroupId { get; set; }

        [Display(Name = "Основен вид дело")]
        public string CaseGroupIds { get; set; }
        public string CaseGroupIds_text { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Съдия")]
        public int? FromLawunitId { get; set; }

        [Display(Name = "Нов съдия")]
        public int? ToLawunitId { get; set; }

        [Display(Name = "Нов състав")]
        public int? ToLawunitDepartmentId { get; set; }

        [Display(Name = "Основание")]
        [IORequired]
        public string Description { get; set; }

        public string CaseList { get; set; }
    }


}
