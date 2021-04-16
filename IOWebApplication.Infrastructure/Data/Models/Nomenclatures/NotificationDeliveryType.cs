using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове призоваване: С призовкар, с ДВ
    /// </summary>
    [Table("nom_notification_delivery_type")]
    public class NotificationDeliveryType : BaseCommonNomenclature
    {
        [Column("notification_delivery_group_id")]
        public int NotificationDeliveryGroupId { get; set; }

        [ForeignKey(nameof(NotificationDeliveryGroupId))]
        public virtual NotificationDeliveryGroup NotificationDeliveryGroup { get; set; }
    }
}
