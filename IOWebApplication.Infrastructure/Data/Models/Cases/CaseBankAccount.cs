// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Начин на плащане/изпълнение, Заповедни производства
    /// </summary>
    [Table("case_bank_account")]
    public class CaseBankAccount : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_bank_account_type_id")]
        [Display(Name = "Начин на плащане/изпълнение")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int CaseBankAccountTypeId { get; set; }

        [Column("iban")]
        [Display(Name = "IBAN")]
        [RegularExpression("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{6}([a-zA-Z0-9]?){0,16}", ErrorMessage = "Невалиден {0}.")]
        public string IBAN { get; set; }

        [Column("bic")]
        [Display(Name = "BIC")]
        public string BIC { get; set; }

        [Column("bank_name")]
        [Display(Name = "Име на банката")]
        public string BankName { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Кредитор")]
        public int? CasePersonId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseBankAccountTypeId))]
        public virtual CaseBankAccountType CaseBankAccountType { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }
    }
}
