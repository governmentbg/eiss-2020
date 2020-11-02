// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ObligationEditVM
    {
        public int Id { get; set; }

        public int CourtId { get; set; }

        public int? CaseSessionActId { get; set; }

        public long? DocumentId { get; set; }

        public int? CaseSessionId { get; set; }

        /// <summary>
        /// Ид-то на CasePerson или DocumentPerson
        /// </summary>
        [Display(Name = "Лице")]
        [Range(1, long.MaxValue, ErrorMessage = "Изберете")]
        public long? Person_SourceId { get; set; }

        [Display(Name = "Вид сума / задължение")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int MoneyTypeId { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Основание")]
        public int? MoneyFeeTypeId { get; set; }

        [Display(Name = "Вид")]
        public int MoneySign { get; set; }

        [Display(Name = "В полза на")]
        public int? ExecListTypeId { get; set; }

        [Display(Name = "В полза на")]
        public int? PersonReceiveId { get; set; }

        [Display(Name = "В полза на")]
        public string CountryReceiveId { get; set; }

        [Display(Name = "Банкова сметка")]
        [RegularExpression("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{6}([a-zA-Z0-9]?){0,16}", ErrorMessage = "Невалиден IBAN.")]
        public string Iban { get; set; }

        [Display(Name = "BIC")]
        public string BIC { get; set; }

        [Display(Name = "Име на банката")]
        public string BankName { get; set; }

        public int? Person_SourceType { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Основание за налагане на глоба")]
        public int? MoneyFineTypeId { get; set; }

    }
}
