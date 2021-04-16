using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Точен вид на обстоятелство: Договор за кредит, застрахователен договор
    /// </summary>
    [Table("nom_case_money_claim_type")]
    public class CaseMoneyClaimType : BaseCommonNomenclature
    {
        [Column("case_money_claim_group_id")]
        public int CaseMoneyClaimGroupId { get; set; }

        [ForeignKey(nameof(CaseMoneyClaimGroupId))]
        public virtual CaseMoneyClaimGroup CaseMoneyClaimGroup { get; set; }
    }
}
