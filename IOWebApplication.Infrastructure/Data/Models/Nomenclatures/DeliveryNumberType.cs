using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове номера: Адреси в район за призовки (Четни/Нечетни/...)
    /// </summary>
    [Table("nom_delivery_number_type")]
    public class DeliveryNumberType : BaseCommonNomenclature
    {
    }
}
