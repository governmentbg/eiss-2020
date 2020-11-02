// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове призоваване към статус на призоваване
    /// </summary>
    [Table("nom_notification_delivery_group_state")]
    public class NotificationDeliveryGroupState
    {
        [Column("notification_delivery_group_id")]
        public int NotificationDeliveryGroupId { get; set; }

        [Column("notification_state_id")]
        public int NotificationStateId { get; set; }

        [ForeignKey(nameof(NotificationDeliveryGroupId))]
        public virtual NotificationDeliveryGroup NotificationDeliveryGroup { get; set; }

        [ForeignKey(nameof(NotificationStateId))]
        public virtual NotificationState NotificationState { get; set; }
    }
}
