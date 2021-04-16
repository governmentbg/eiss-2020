using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Номенклатура Решения
    /// </summary>
    [Table("nom_decision_type")]
    public class DecisionType: BaseCommonNomenclature
    {
    }
}
