// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{    
    /// <summary>
    /// банкови сметки към различни обекти
    /// </summary>
    [Table("common_bank_account")]
    public class BankAccount : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [Column("source_id")]
        public long SourceId { get; set; }

        [Column("is_main_account")]
        public bool IsMainAccount { get; set; }

        [Column("iban")]
        public string IBAN { get; set; }

        [Column("bic")]
        public string BIC { get; set; }

        [Column("bank_name")]
        public string BankName { get; set; }
    }
}
