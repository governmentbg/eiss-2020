using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Съпровождащи документи, прикачени към заседание
    /// </summary>
    [Table("case_session_doc")]
    public class CaseSessionDoc: IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_session_id")]
        public int CaseSessionId { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }
        /// <summary>
        /// Неразгледан,разгледан,окончателно разгледан
        /// </summary>
        [Column("session_doc_state_id")]
        [Display(Name = "Статус")]
        public int SessionDocStateId { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Column("date_from")]
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(SessionDocStateId))]
        public virtual SessionDocState SessionDocState { get; set; }
    }
}
