// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public class RegixReportVM
    {
        public int Id { get; set; }
        public int CourtId { get; set; }

        [Display(Name = "Номер дело")]
        public int? CaseId { get; set; }

        [Display(Name = "Акт")]
        public int? CaseSessionActId { get; set; }

        [Display(Name = "Документ")]
        public long? DocumentId { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        public RegixHeaderFooterVM HeaderFooter { get; set; }

        public RegixReportVM()
        {
            HeaderFooter = new RegixHeaderFooterVM();
        }
    }


    public class RegixHeaderFooterVM
    {
        public string RegixTypeName { get; set; }

        public string CourtName { get; set; }

        [Display(Name = "Изготвил:")]
        public string UserName { get; set; }

        [Display(Name = "Дата на изготвяне:")]
        public string Date { get; set; }

        [Display(Name = "Дело:")]
        public string CaseNumber { get; set; }

        [Display(Name = "Акт:")]
        public string CaseSessionActNumber { get; set; }

        [Display(Name = "Описание:")]
        public string DescriptionReason { get; set; }

        [Display(Name = "Документ:")]
        public string DocumentNumber { get; set; }
    }
}
