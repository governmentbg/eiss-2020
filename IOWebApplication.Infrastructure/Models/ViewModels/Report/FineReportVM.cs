// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class FineReportVM
    {
        public int Id { get; set; }
        public int? CaseId { get; set; }

        [Display(Name = "Вид дело")]
        public string CaseGroupName { get; set; }

        [Display(Name = "Номер/Година")]
        public string CaseNumber { get; set; }

        [Display(Name = "Вид заседание")]
        public string SessionTypeName { get; set; }

        [Display(Name = "Дата")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ObligationDate { get; set; }

        [Display(Name = "Лице")]
        public string SenderName { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        public decimal AmountPay { get; set; }

        [Display(Name = "Дата на внасяне")]
        public string PaidDate { get; set; }

        [Display(Name = "Статус")]
        public string State
        {
            get
            {
                string result = "";
                if (this.AmountPay > 0)
                {
                    if (this.Amount - this.AmountPay > 0.001M)
                        result = "Частично платена";
                    else
                        result = "Платена";
                }
                else
                {
                    result = "Наложена";
                }
                return result;
            }
        }

        [Display(Name = "Забележка")]
        public string Description { get; set; }
    }

    public class FineFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }
    }
}
