using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Election
{
    /// <summary>
    /// Тип друг избор на съдия
    /// </summary>
    [Table("nom_election_type")]
    public class ElectionType : BaseCommonNomenclature
    {

    }
}
