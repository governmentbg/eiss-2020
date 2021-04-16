using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Роля на съдебен служител: съдия,член,заседател
    /// </summary>
    [Table("nom_judge_role")]
    public class JudgeRole : BaseCommonNomenclature
    {      

    }
}
