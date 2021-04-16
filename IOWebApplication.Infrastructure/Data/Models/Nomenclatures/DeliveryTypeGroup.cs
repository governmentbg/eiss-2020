using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Възможен статус на призоваване към операция по доставка
    /// </summary>
    [Table("nom_delivery_type_group")]
    public class DeliveryTypeGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("notification_delivery_group_id")]
        public int NotificationDeliveryGroupId { get; set; }

        [Column("notification_type_id")]
        public int NotificationTypeId { get; set; }

        [ForeignKey(nameof(NotificationDeliveryGroupId))]
        public virtual NotificationDeliveryGroup NotificationDeliveryGroup { get; set; }

        [ForeignKey(nameof(NotificationTypeId))]
        public virtual NotificationType NotificationType { get; set; }
    }
}
