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
    [Table("nom_delivery_oper_state")]
    public class DeliveryOperState
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("delivery_oper_id")]
        public int DeliveryOperId { get; set; }

        [Column("notification_state_id")]
        public int NotificationStateId { get; set; }

        [ForeignKey(nameof(DeliveryOperId))]
        public virtual DeliveryOper DeliveryOper { get; set; }

        [ForeignKey(nameof(NotificationStateId))]
        public virtual NotificationState NotificationState { get; set; }
    }
}
