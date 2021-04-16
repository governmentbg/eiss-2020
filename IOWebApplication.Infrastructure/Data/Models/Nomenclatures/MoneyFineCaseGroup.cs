using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основания за налагане на глоба към основен вид дело
    /// </summary>
    [Table("nom_money_fine_case_group")]
    public class MoneyFineCaseGroup
    {
        [Column("money_fine_type_id")]
        public int MoneyFineTypeId { get; set; }

        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        [ForeignKey(nameof(MoneyFineTypeId))]
        public virtual MoneyFineType MoneyFineType { get; set; }
    }
}
