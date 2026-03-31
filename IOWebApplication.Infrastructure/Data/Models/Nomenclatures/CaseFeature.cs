using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групи от критерии за определяне на дело
    /// </summary>
    [Table("nom_case_feature")]
    public class CaseFeature : BaseCommonNomenclature
    {
        [Column("all_court_types")]
        public bool AllCourtTypes { get; set; }

        [Column("all_case_types")]
        public bool AllCaseTypes { get; set; }

        [Column("all_case_codes")]
        public bool AllCaseCodes { get; set; }

        public virtual ICollection<CaseFeatureCourtType> CourtTypes { get; set; }
        public virtual ICollection<CaseFeatureType> CaseTypes { get; set; }
        public virtual ICollection<CaseFeatureCode> CaseCodes { get; set; }

        public CaseFeature()
        {
            CourtTypes = new HashSet<CaseFeatureCourtType>();
            CaseTypes = new HashSet<CaseFeatureType>();
            CaseCodes = new HashSet<CaseFeatureCode>();
        }
    }
}
