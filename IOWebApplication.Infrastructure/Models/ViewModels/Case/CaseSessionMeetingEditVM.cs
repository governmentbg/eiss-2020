// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionMeetingEditVM
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? CaseId { get; set; }
        public int CaseSessionId { get; set; }

        [Display(Name = "Тип сесия")]
        public int SessionMeetingTypeId { get; set; }

        [Display(Name = "От дата и час")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До час")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateTo { get; set; }

        [Display(Name = "Забележка")]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public bool? IsAutoCreate { get; set; }

        [Display(Name = "Зала")]
        public int? CourtHallId { get; set; }

        public bool? IsSessionProvedeno { get; set; }

        public virtual List<CheckListVM> CaseSessionMeetingUser { get; set; }
    }
}
