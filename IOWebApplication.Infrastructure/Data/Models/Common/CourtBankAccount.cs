// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Банкови сметки към съд
    /// </summary>
    [Table("common_court_bank_account")]
    public class CourtBankAccount : BaseCommonNomenclature
    {
        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("iban")]
        [Display(Name = "IBAN")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string Iban { get; set; }

        [Column("money_group_id")]
        [Display(Name = "Вид сметка")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int MoneyGroupId { get; set; }

        [Column("com_port_pos")]
        [Display(Name = "COM port ПОС")]
        public string ComPortPos { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(MoneyGroupId))]
        public virtual MoneyGroup MoneyGroup { get; set; }
    }
}
