// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseFastSearchVM
    {
        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }

        [Display(Name = "Година")]
        public int CaseYear { get; set; }

        public CaseFastSearchVM()
        {
            CaseGroupId = NomenclatureConstants.CaseGroups.GrajdanskoDelo;
            CaseYear = DateTime.Now.Year;
        }
    }
}
