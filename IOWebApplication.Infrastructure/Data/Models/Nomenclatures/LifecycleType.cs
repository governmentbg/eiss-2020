using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид интервал по дело:1-В процес,2-Временно преустановено,3-Възобновено
    /// TODO: да се уточнят точните елементи!
    /// </summary>
    [Table("nom_lifecycle_type")]
    public class LifecycleType : BaseCommonNomenclature
    {      

    }
}
