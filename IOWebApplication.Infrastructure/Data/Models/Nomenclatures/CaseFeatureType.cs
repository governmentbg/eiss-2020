using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове дела към Групи от критерии
    /// </summary>
    [Table("nom_case_feature_type")]
    public class CaseFeatureType
    {
        [Column("case_feature_id")]
        public int CaseFeatureId { get; set; }

        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        [ForeignKey(nameof(CaseFeatureId))]
        public virtual CaseFeature CaseFeature { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual CaseType CaseType { get; set; }
    }
}
