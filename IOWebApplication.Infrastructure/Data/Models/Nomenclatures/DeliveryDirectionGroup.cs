// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Допустими начини на получаване/изпращане по направление на документ
    /// </summary>
    [Table("nom_delivery_direction_group")]
    public class DeliveryDirectionGroup
    {
        /// <summary>
        /// Начин на изпращане: Призовкар,Поща,куриер,факс
        /// </summary>
        [Column("delivery_group_id")]
        public int DeliveryGroupId { get; set; }

        /// <summary>
        /// Посока на движение на документ: Входящи, Изходящи, вътрешен
        /// </summary>
        [Column("document_direction_id")]
        public int DocumentDirectionId { get; set; }

        [ForeignKey(nameof(DeliveryGroupId))]
        public virtual DeliveryGroup DeliveryGroup { get; set; }

        [ForeignKey(nameof(DocumentDirectionId))]
        public virtual DocumentDirection DocumentDirection { get; set; }
    }
}
