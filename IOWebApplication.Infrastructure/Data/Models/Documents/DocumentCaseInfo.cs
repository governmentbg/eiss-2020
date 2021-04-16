using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Данни за съпровождащ документ/свързани дела към деловоден документ
    /// </summary>
    [Table("document_case_info")]
    public class DocumentCaseInfo
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("session_act_id")]
        public int? SessionActId { get; set; }

        /// <summary>
        /// true при избор на старо свързано дело от друга система
        /// </summary>
        [Column("is_legacy_case")]
        public bool? IsLegacyCase { get; set; }

        /// <summary>
        /// Кодирания 14-цифрен номер на делото
        /// </summary>
        [Column("case_reg_number")]
        public string CaseRegNumber { get; set; }

        /// <summary>
        /// Кодирания 5-цифрен номер на делото
        /// </summary>
        [Column("case_short_number")]
        public string CaseShortNumber { get; set; }

        [Column("case_year")]
        public int? CaseYear { get; set; }       

       
      

        [Column("description")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }
    }
}
