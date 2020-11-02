// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSprVM
    {
        public int Id { get; set; }
        public string CourtLabel { get; set; }
        public string CaseGroupLabel { get; set; }

        [Display(Name = "Вид дело")]
        public string CaseTypeLabel { get; set; }

        [Display(Name = "Номер/Година")]
        public string CaseRegNum { get; set; }

        [Display(Name = "Образувано")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CaseRegDate { get; set; }

        [Display(Name = "Предмет")]
        public string CaseCodeLabel { get; set; }

        public DateTime CaseBeginDate { get; set; }

        [Display(Name = "Свършило")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CaseEndDate { get; set; }

        public string CasePersons { get; set; }
        public string CasePersonRoles { get; set; }

        [Display(Name = "Съдия-докладчик")]
        public string JudgeReport { get; set; }

        public string CasePersonSentenceInfo { get; set; }

        [Display(Name = "Срок от първото образуване")]
        public string CaseLifeCycle { get; set; }
        public DateTime? SessionDateFrom { get; set; }
        public DateTime? SessionActDate { get; set; }
        public string SessionTypeLabel { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string ActFinalInfo { get; set; }
        public string SessionResult { get; set; }
        public string ActTypeLabel { get; set; }
        public DateTime? ActReturnDate { get; set; }
        public string LifecycleInfo { get; set; }
    }
}
