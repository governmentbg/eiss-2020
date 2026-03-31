// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.IndexService
{
    public class SearchFilterModel
    {
        public int CourtId { get; set; }
        [Display(Name = "Текст за търсене")]
        public string Query { get; set; }

        [Display(Name = "Основен вид дело")]
        public string CaseGroupIds { get; set; }
        [Display(Name = "Точен вид дело")]
        public string CaseTypeIds { get; set; }
        [Display(Name = "Отделение")]
        public int? OtdelenieId { get; set; }
        [Display(Name = "Състав")]
        public int? JudicalCompositionId { get; set; }
        [Display(Name = "Вид акт")]
        public string ActTypeIds { get; set; }
        [Display(Name = "Постановен от")]
        public DateTime? ActDateFrom { get; set; }
        [Display(Name = "Постановен до")]
        public DateTime? ActDateTo { get; set; }
        [Display(Name = "Финализиращ акт")]
        public bool? IsFinalDoc { get; set; }
        [Display(Name = "Съдия-докладчик")]
        public int? JudgeReporterId { get; set; }

        public int Skip { get; set; }
        public int Take { get; set; }

        public SearchFilterModel()
        {
            Skip = 0;
            Take = 20;
        }
    }
}
