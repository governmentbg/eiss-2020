// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ExecListVM
    {
        public int Id { get; set; }

        [Display(Name = "№ на изпълн. лист")]
        public string RegNumber { get; set; }

        [Display(Name = "Дата на издаване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime RegDate { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Издаден срещу кого")]
        public string FullName { get; set; }

        [Display(Name = "Издаден в полза на")]
        public string FullNameReceive { get; set; }

        [Display(Name = "Размер")]
        public decimal Amount { get; set; }

        [Display(Name = "Тип")]
        public string ExecListTypeName { get; set; }

        [Display(Name = "Възлагателно писмо до")]
        public string InstitutionNames { get; set; }

        [Display(Name = "Протокол")]
        public string ExchangeDocNumber { get; set; }

        [Display(Name = "Дело")]
        public string CaseData { get;set; }

        [Display(Name = "Съдебен акт")]
        public string SessionAct { get;set; }

        [Display(Name = "Основание")]
        public string MoneyTypeName { get;set; }

        [Display(Name = "Номер на дело")]
        public string CaseNumber { get;set; }

        public int? CaseId { get; set; }
    }

    public class ExecListFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Display(Name = "Задължено лице")]
        public string FullName { get; set; }

        [Display(Name = "Наименование В полза на трето лице")]
        public string FullNameReceive { get; set; }

        [Display(Name = "Тип")]
        public int ExecListTypeId { get; set; }

        [Display(Name = "ТД на НАП")]
        public int InstitutionId { get; set; }

        [Display(Name = "Активни ИЛ")]
        public bool ActiveExecList { get; set; }
    }

}
