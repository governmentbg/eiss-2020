// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Възможнa причина за недоставени призовки по статус
    /// </summary>
    [Table("nom_delivery_state_reason")]
    public class DeliveryStateReason
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("delivery_reason_id")]
        public int DeliveryReasonId { get; set; }

        [Column("notification_state_id")]
        public int NotificationStateId { get; set; }

        [ForeignKey(nameof(DeliveryReasonId))]
        public virtual DeliveryReason DeliveryReason { get; set; }

        [ForeignKey(nameof(NotificationStateId))]
        public virtual NotificationState NotificationState { get; set; }
    }
}
