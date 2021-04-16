using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Старшинство на съдиите
    /// </summary>
    [Table("nom_judge_seniority")]
    public class JudgeSeniority : BaseCommonNomenclature
    {

    }
}
