// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class EuropeanHeritageReportVM
    {
        [Display(Name = "Наследодател")]
        public string Inheritor { get; set; }

        [Display(Name = "Заявител")]
        public string Notifier { get; set; }

        [Display(Name = "№ на дело")]
        public string RegNumber { get; set; }

        [Display(Name = "Дата на образуване на делото")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime RegDate { get; set; }

        [Display(Name = "Дата на свършване на делото")]
        public string FinishDate { get; set; }

        [Display(Name = "Съдебен акт")]
        public string ActTypeName { get; set; }

        [Display(Name = "Европейско удостоверение за наследство")]
        public string ActNumber { get; set; }

        [Display(Name = "Лица, получили заверено копие от ЕУН")]
        public string PersonNameReceive { get; set; }
    }

    public class EuropeanHeritageFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }
    }
}
