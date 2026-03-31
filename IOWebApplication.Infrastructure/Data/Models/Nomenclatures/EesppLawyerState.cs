using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на подаден адвокат - ЕЕСПП: 1-Подаден от ЕЕСПП,2-Назначен в ЕИСС,3-Отказан в ЕИСС
    /// </summary>
    [Table("nom_eespp_lawyer_state")]
    public class EesppLawyerState : BaseCommonNomenclature
    {

    }
}
