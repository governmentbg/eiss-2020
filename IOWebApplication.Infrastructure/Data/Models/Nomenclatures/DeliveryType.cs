using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид пратка: писмо,пакет,колет
    /// </summary>
    [Table("nom_delivery_type")]
    public class DeliveryType : BaseCommonNomenclature
    {
        [Column("delivery_group_id")]
        public int DeliveryGroupId { get; set; }

        [ForeignKey(nameof(DeliveryGroupId))]
        public virtual DeliveryGroup DeliveryGroup { get; set; }

    }
}
