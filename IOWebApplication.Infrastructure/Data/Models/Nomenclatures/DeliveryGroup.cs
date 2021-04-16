using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Начин на подаване/изпращане: Призовкар,поща,куриеска фирма,имейл
    /// </summary>
    [Table("nom_delivery_group")]
    public class DeliveryGroup : BaseCommonNomenclature
    {
        
    }
}
