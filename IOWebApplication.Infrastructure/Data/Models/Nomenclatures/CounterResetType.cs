using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Режим на нулиране на броячи: Годишен, ръчен
    /// </summary>
    [Table("nom_counter_reset_type")]
    public class CounterResetType : BaseCommonNomenclature
    {
    }
}
