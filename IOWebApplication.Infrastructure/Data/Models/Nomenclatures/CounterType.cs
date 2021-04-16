using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид брояч: По: документ, дело, документ в дело
    /// </summary>
    [Table("nom_counter_type")]
    public class CounterType : BaseCommonNomenclature
    {
    }
}
