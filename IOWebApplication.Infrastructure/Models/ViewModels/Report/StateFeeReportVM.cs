// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class StateFeeReportVM
    {
        public int Id { get; set; }
        public int? CaseId { get; set; }
        public bool? ExistCase { get; set; }

        [Display(Name = "Вид дело/документ")]
        public string DocumentData { get; set; }

        [Display(Name = "Документ")]
        public string PaymentData { get; set; }

        [Display(Name = "Точен вид")]
        public string CaseTypeCode { get; set; }

        [Display(Name = "Дата")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ObligationDate { get; set; }

        [Display(Name = "Лице")]
        public string SenderName { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Display(Name = "Внесена на")]
        public string PaidDate { get; set; }

        [Display(Name = "Вносна бележка")]
        public string PaymentDescription { get; set; }

        [Display(Name = "Текст")]
        public string Description { get; set; }
    }

    public class StateFeeFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Основен вид на документа")]
        public int DocumentGroupId { get; set; }

        [Display(Name = "Точен вид на документа")]
        public int DocumentTypeId { get; set; }

    }
}
