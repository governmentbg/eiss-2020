using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Дела към Разпореждания по документ
    /// </summary>
    [Table("document_resolution_case")]
    public class DocumentResolutionCase : UserDateWRT
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("document_resolution_id")]
        public long DocumentResolutionId { get; set; }

        [Column("case_id")]
        [Display(Name ="Изберете дело")]
        public int CaseId { get; set; }

        [ForeignKey(nameof(DocumentResolutionId))]
        public virtual DocumentResolution DocumentResolution { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }
    }
}
