// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class ElectronicDocumentInfoVM
    {
        [Display(Name = "Подадено от")]
        public string EpepUserInfo { get; set; }

        [Display(Name ="За дело")]
        public string CaseInfo { get; set; }
        [Display(Name ="За страна")]
        public string PersonInfo { get; set; }

        [Display(Name ="Номер")]
        public string ApplyNumber { get; set; }

        [Display(Name ="Дата на подаване")]
        public DateTime ApplyDate { get; set; }

        [Display(Name ="Валута")]
        public string CurrencyCode { get; set; }

        public int? MoneyFeeTypeId { get; set; }

        [Display(Name ="Материален интерес")]
        public decimal? BaseAmount { get; set; }

        [Display(Name ="Дължима такса")]
        public decimal? TaxAmount { get; set; }

        [Display(Name ="Дата на плащане")]
        public DateTime? PaidDate { get; set; }

        [Display(Name = "Избран начин на плащане")]
        public string PaymentType { get; set; }
        public int PaymentTypeId { get; set; }

        [Display(Name = "Платено в съд")]
        public string PaidInCourtName { get; set; }


        [Display(Name ="Забележка")]
        public string Description { get; set; }
    }
}
