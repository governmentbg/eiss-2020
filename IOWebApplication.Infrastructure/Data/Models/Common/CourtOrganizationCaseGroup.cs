using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Видове дела към регистратура
    /// </summary>
    [Table("common_court_organization_case_group")]
    public class CourtOrganizationCaseGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
      
        [Column("court_organization_id")]
        public int CourtOrganizationId { get; set; }

        [Column("case_group_id")]
        [Display(Name ="Основен вид дело")]
        public int CaseGroupId { get; set; }

        [ForeignKey(nameof(CourtOrganizationId))]
        public virtual CourtOrganization CourtOrganization { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }        
    }
}
