using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Election
{
    /// <summary>
    /// Статус на лице в процедура за избор
    /// 1-Участва, 2-Не участва, 3-Техническа грешка
    /// </summary>
    [Table("nom_election_person_state")]
    public class ElectionPersonState : BaseCommonNomenclature
    {
    }
}
