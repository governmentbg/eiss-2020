using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Съдебен състав по дело - отводи
    /// </summary>
    [Table("case_lawunit_dismisal")]
    public class CaseLawUnitDismisal : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_lawunit_id")]
        public int CaseLawUnitId { get; set; }

        [Column("case_session_act_id")]
        [Display(Name = "Акт")]
        public int? CaseSessionActId { get; set; }

        [Column("dismisal_type_id")]
        [Display(Name = "Тип")]
        public int DismisalTypeId { get; set; }

        [Column("dismisal_date")]
        [Display(Name = "От дата")]
        public DateTime DismisalDate { get; set; }

        [Column("description")]
        [Display(Name = "Мотив")]
        public string Description { get; set; }

        [Column("dismisal_kind_id")]
        public int? DismisalKindId { get; set; }

        [Column("dismissal_state_id")]
        [Display(Name = "Статус на отвода")]
        public int? DismissalStateId { get; set; }

        [Display(Name = "Документ, с който се иска отвода")]
        [Column("document_id")]
        public long? DocumentId { get; set; }

        [Display(Name = "Вносител на искането")]
        [Column("document_person_id")]
        public long? DocumentPersonId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseLawUnitId))]
        public virtual CaseLawUnit CaseLawUnit { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(DismisalTypeId))]
        public virtual DismisalType DismisalType { get; set; }

        [ForeignKey(nameof(DismissalStateId))]
        public virtual DismissalState DismissalState { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(DocumentPersonId))]
        public virtual DocumentPerson DocumentPerson { get; set; }
    }
}
