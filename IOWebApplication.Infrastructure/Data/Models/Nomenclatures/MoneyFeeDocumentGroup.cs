// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основни видове документ към държавна такса
    /// </summary>
    [Table("nom_money_fee_document_group")]
    public class MoneyFeeDocumentGroup
    {
        [Column("money_fee_type_id")]
        public int MoneyFeeTypeId { get; set; }

        [Column("document_group_id")]
        public int DocumentGroupId { get; set; }

        [ForeignKey(nameof(DocumentGroupId))]
        public virtual DocumentGroup DocumentGroup { get; set; }

        [ForeignKey(nameof(MoneyFeeTypeId))]
        public virtual MoneyFeeType MoneyFeeType { get; set; }
    }
}
