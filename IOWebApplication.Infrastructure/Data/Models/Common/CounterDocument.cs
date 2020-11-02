// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Връзки към брояч за документи
    /// </summary>
    [Table("common_counter_document")]
    public class CounterDocument
    {
       
        [Column("counter_id")]
        public int CounterId { get; set; }

        /// <summary>
        /// Посока на движение на документ: Входящи, Изходящи
        /// </summary>
        [Column("document_direction_id")]
        public int DocumentDirectionId { get; set; }

        [ForeignKey(nameof(CounterId))]
        public virtual Counter Counter { get; set; }

        [ForeignKey(nameof(DocumentDirectionId))]
        public virtual DocumentDirection DocumentDirection { get; set; }
    }
}
