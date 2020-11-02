// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyCollectionEditVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int? CourtId { get; set; }
        public int? MainCaseMoneyCollectionId { get; set; }
        public int CaseMoneyClaimId { get; set; }
        [Display(Name = "Вид")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int CaseMoneyCollectionGroupId { get; set; }
        [Display(Name = "Тип")]
        public int? Money_CaseMoneyCollectionTypeId { get; set; }
        [Display(Name = "От какво произтича задължението за предаване")]
        public int? Movables_CaseMoneyCollectionTypeId { get; set; }
        [Display(Name = "Вид допълнително вземане")]
        public int? CaseMoneyCollectionKindId { get; set; }
        [Display(Name = "Валута")]
        public int CurrencyId { get; set; }
        [Display(Name = "Първоначално")]
        public decimal InitialAmount { get; set; }
        [Display(Name = "Претендирано")]
        public decimal PretendedAmount { get; set; }
        [Display(Name = "Уважено")]
        public decimal RespectedAmount { get; set; }
        [Display(Name = "Сума")]
        public decimal Amount { get; set; }
        [Display(Name = "Начало")]
        public DateTime? DateFrom { get; set; }
        [Display(Name = "Край")]
        public DateTime? DateTo { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Display(Name = "Солидарно разпределение")]
        public bool JointDistribution { get; set; }
        [Display(Name = "Вид крайна дата до")]
        public int? MoneyCollectionEndDateTypeId { get; set; }
        [Display(Name = "Име")]
        public string Label { get; set; }

        public IList<CasePersonListDecimalVM> CasePersonListDecimals { get; set; }
    }
}
