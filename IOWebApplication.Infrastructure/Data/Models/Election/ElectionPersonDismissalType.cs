using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Election
{
    /// <summary>
    /// Статус на лице в процедура за избор
    /// 1-Самоотвод,2-Отвод,3-Смърт
    /// </summary>
    [Table("nom_election_person_dismissal_type")]
    public class ElectionPersonDismissalType : BaseCommonNomenclature
    {
    }
}
