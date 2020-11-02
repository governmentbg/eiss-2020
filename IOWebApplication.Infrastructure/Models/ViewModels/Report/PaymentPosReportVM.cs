// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class PaymentPosReportVM
    {
        [Display(Name = "Дата")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PaidDate { get; set; }

        [Display(Name = "Час")]
        [DisplayFormat(DataFormatString = "{0:HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime PaidDateHour { get; set; }

        [Display(Name = "Транзакция")]
        public string PaymentNumber { get; set; }

        [Display(Name = "Терминал")]
        public string Tid { get; set; }

        [Display(Name = "Платец")]
        public string SenderName { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        public int MoneyGroupId { get; set; }
    }

    public class PaymentPosFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Платец")]
        public string FullName { get; set; }

        [Display(Name = "Сметка")]
        public int MoneyGroupId { get; set; }
    }
}
