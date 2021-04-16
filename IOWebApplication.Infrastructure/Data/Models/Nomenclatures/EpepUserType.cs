using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид потребител в ЕПЕП:1-адвокат,2-лице
    /// </summary>
    [Table("nom_epep_user_type")]
    public class EpepUserType : BaseCommonNomenclature
    {      

    }
}
