using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Решения за Document
    /// </summary>
    [Table("document_decision")]
    public class DocumentDecision : UserDateWRT, IHaveLongId
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("decision_type_id")]
        [Display(Name = "Решение")]
        public int? DecisionTypeId { get; set; }

        [Column("reg_number")]
        public string RegNumber { get; set; }

        [Column("reg_date")]
        public DateTime? RegDate { get; set; }

        [Column("out_document_id")]
        [Display(Name = "Документ за пренасочване")]
        public long? OutDocumentId { get; set; }

        [Column("user_decision_id")]
        public string UserDecisionId { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Column("document_decision_state_id")]
        [Display(Name = "Статус")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int DocumentDecisionStateId { get; set; }


        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(DecisionTypeId))]
        public virtual DecisionType DecisionType { get; set; }

        [ForeignKey(nameof(OutDocumentId))]
        public virtual Document OutDocument { get; set; }

        [ForeignKey(nameof(UserDecisionId))]
        public virtual ApplicationUser UserDecision { get; set; }

        [ForeignKey(nameof(DocumentDecisionStateId))]
        public virtual DocumentDecisionState DocumentDecisionState { get; set; }
    }
}
