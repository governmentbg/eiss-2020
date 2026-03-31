using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове съдилища към Групи от критерии
    /// </summary>
    [Table("nom_case_feature_court_type")]
    public class CaseFeatureCourtType
    {
        [Column("case_feature_id")]
        public int CaseFeatureId { get; set; }

        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        [ForeignKey(nameof(CaseFeatureId))]
        public virtual CaseFeature CaseFeature { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }
    }
}
