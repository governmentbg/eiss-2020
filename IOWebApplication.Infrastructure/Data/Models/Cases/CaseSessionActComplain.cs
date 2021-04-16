using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Обжалвания към съдебен акт
    /// </summary>
    [Table("case_session_act_complain")]
    public class CaseSessionActComplain: UserDateWRT, IExpiredInfo
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_session_act_id")]
        public int CaseSessionActId { get; set; }
        
        /// <summary>
        /// Id на Document от CaseSessionDoc - съпровождащ документ със жалбата
        /// </summary>
        [Column("complain_document_id")]
        [Display(Name = "Съпровождащ документ")]
        [Range(1, long.MaxValue, ErrorMessage = "Изберете {0}.")]
        public long ComplainDocumentId { get; set; }

        /// <summary>
        /// Забележка при връщане,
        /// </summary>
        [Column("reject_description")]
        [Display(Name = "Забележка при връщане")]
        public string RejectDescription { get; set; }

        /// <summary>
        /// Статус на обжалването
        /// </summary>
        [Column("complaint_state_id")]
        [Display(Name = "Статус")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int ComplainStateId { get; set; }

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

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(ComplainDocumentId))]
        public virtual Document ComplainDocument { get; set; }

        [ForeignKey(nameof(ComplainStateId))]
        public virtual ComplainState ComplainState { get; set; }

        public virtual ICollection<CaseSessionActComplainPerson> CasePersons { get; set; }
        public virtual ICollection<CaseSessionActComplainResult> ComplainResults { get; set; }
    }
}
