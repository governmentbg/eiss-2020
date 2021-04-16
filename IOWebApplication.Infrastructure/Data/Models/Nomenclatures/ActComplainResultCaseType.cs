using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Резултат / Степен на уважаване на иска - към вид дело
    /// </summary>
    [Table("nom_act_complain_result_case_type")]
    public class ActComplainResultCaseType 
    {
        [Column("act_complain_result_id")]
        public int ActComplainResultId { get; set; }

        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        [ForeignKey(nameof(ActComplainResultId))]
        public virtual ActComplainResult ActComplainResult { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual CaseType CaseType { get; set; }
    }
}
