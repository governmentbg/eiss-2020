// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonMeasureEditVM
    {
        public int Id { get; set; }
        public int CourtId { get; set; }
        public int CaseId { get; set; }
        public int CasePersonId { get; set; }
        public int? ParentId { get; set; }

        [Display(Name = "Вид институция")]
        public int? MeasureInstitutionTypeId { get; set; }

        [Display(Name = "Институция, определила мярката")]
        public int? MeasureInstitutionId { get; set; }

        [Display(Name = "Съд, определил мярката")]
        public int? MeasureCourtId { get; set; }

        [Display(Name = "Вид мярка")]
        [Required(ErrorMessage = "Изберете {0}.")]
        // eispp_tbl_code =214
        public string MeasureType { get; set; }

        [Display(Name = "Дата на мярката")]
        [Required(ErrorMessage = "Изберете {0}.")]
        public DateTime MeasureStatusDate { get; set; }

        [Display(Name = "Гаранция, лв")]
        public double BailAmount { get; set; }

        [Display(Name = "Статус")]
        [Required(ErrorMessage = "Изберете {0}.")]
        public string MeasureStatus { get; set; }
    }
}
