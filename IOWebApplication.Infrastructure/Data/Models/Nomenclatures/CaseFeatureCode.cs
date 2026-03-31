using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Шифри към Групи от критерии
    /// </summary>
    [Table("nom_case_feature_code")]
    public class CaseFeatureCode
    {
        [Column("case_feature_id")]
        public int CaseFeatureId { get; set; }

        [Column("case_code_id")]
        public int CaseCodeId { get; set; }

        [ForeignKey(nameof(CaseFeatureId))]
        public virtual CaseFeature CaseFeature { get; set; }

        [ForeignKey(nameof(CaseCodeId))]
        public virtual CaseCode CaseCode { get; set; }
    }
}
