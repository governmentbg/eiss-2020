// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMigrationPriorVM
    {
        public int PriorCourtId { get; set; }
        public int CaseId { get; set; }
        public int PriorCaseId { get; set; }
        [Display(Name = "Съд на предходно дело")]
        public string CourtName { get; set; }
        [Display(Name = "Предходно дело")]
        public string CaseInfo { get; set; }

        public bool HasMigrations { get; set; }

        [Display(Name = "Вид движение")]
        public int? CaseMigrationTypeId { get; set; }
    }
}
