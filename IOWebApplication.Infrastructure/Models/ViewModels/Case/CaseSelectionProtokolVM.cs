// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSelectionProtokolVM
    {
        public int Id { get; set; }

        public int CaseId { get; set; }

        public int CourtId { get; set; }

    [Display(Name = "Тип разпределение")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int JudgeRoleId { get; set; }

        [Display(Name = "Начин на разпределение")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int SelectionModeId { get; set; }

        [Display(Name = "Дежурство")]
        public int? CourtDutyId { get; set; }

        [Display(Name = "Направление/отделение")]
        public int? CourtDepartmentId { get; set; }

        [Display(Name = " ")]
        public int CaseGroupId { get; set; }

        public int? CourtGroupId { get; set; }

        public int CaseCodeId { get; set; }

        [Display(Name = "Специалност")]
        public int? SpecialityId { get; set; }

        [Display(Name = "Основание за ръчен избор")]
        public string Description { get; set; }

        public string SelectedTab { get; set; }

        [Display(Name = "По отвод")]
        public int? CaseLawUnitDismisalId { get; set; }

        public string IdStr { get; set; }

    public bool IsProtokolNoSelection { get; set; }

    public IList<CaseSelectionProtokolLawUnitVM> LawUnits { get; set; }

        public CaseSelectionProtokolVM()
        {
            LawUnits = new List<CaseSelectionProtokolLawUnitVM>();
        }
    }
}
