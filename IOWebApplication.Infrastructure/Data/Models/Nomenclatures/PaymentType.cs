using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Начин на плащане - POS, банка
    /// </summary>
    [Table("nom_payment_type")]
    public class PaymentType : BaseCommonNomenclature
    {
    }
}
