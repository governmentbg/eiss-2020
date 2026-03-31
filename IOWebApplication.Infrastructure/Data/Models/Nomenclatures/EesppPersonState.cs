using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на подадено лице - ЕЕСПП: 1-Изпратено,2-С върнат адвокат 
    /// </summary>
    [Table("nom_eespp_person_state")]
    public class EesppPersonState : BaseCommonNomenclature
    {      

    }
}
