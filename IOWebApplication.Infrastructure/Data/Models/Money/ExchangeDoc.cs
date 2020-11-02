// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    /// <summary>
    /// Приемо предавателен протокол за ИЛ
    /// </summary>
    [Table("money_exchange_doc")]
    public class ExchangeDoc : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("reg_number")]
        public string RegNumber { get; set; }

        [Column("reg_date")]
        public DateTime? RegDate { get; set; }

        [Column("institution_id")]
        public int InstitutionId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("out_document_id")]
        public long? OutDocumentId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(OutDocumentId))]
        public virtual Document OutDocument { get; set; }

        [ForeignKey(nameof(InstitutionId))]
        public virtual Institution Institution { get; set; }

        public virtual ICollection<ExchangeDocExecList> ExchangeDocExecLists { get; set; }

        public ExchangeDoc()
        {
            ExchangeDocExecLists = new HashSet<ExchangeDocExecList>();
        }
    }
}
