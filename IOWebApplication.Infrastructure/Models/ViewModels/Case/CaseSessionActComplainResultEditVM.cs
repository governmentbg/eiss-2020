// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActComplainResultEditVM
    {
        public int Id { get; set; }
        public int? ComplainCourtId { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        [Display(Name = "Дело")]
        public int? ComplainCaseId { get; set; }

        public int CaseSessionActComplainId { get; set; }
        public int CourtId { get; set; }
        public int CaseId { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        [Display(Name = "Акт")]
        public int CaseSessionActId { get; set; }

        [Display(Name = "Резултат от обжалване")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int? ActResultId { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Дата на отразяване на резултат")]
        public DateTime? DateResult { get; set; }

        [Display(Name = "Стартирай нов интервал по делото")]
        public bool IsStartNewLifecycle { get; set; }

        public virtual List<CheckListVM> CaseSessionActComplains { get; set; }
    }
}
