// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawyerHelpAssignedLawyerEditVM
    {
        public int Id { get; set; }
        public int CaseLawyerHelpId { get; set; }
        [Display(Name = "Съдебен акт")]
        public int? CaseSessionActAssignedId { get; set; }
        [Display(Name = "Статус")]
        public int LawyerStateId { get; set; }
        public int CaseId { get; set; }
    }
}
