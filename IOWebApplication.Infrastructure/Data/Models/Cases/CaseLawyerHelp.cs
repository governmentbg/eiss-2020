using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Искане за Правна помощ
    /// </summary>
    [Table("case_lawyer_help")]
    public class CaseLawyerHelp : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("lawyer_help_base_id")]
        [Display(Name = "Основание за изпращане на искането")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}")]
        public int LawyerHelpBaseId { get; set; }

        [Column("lawyer_help_type_id")]
        [Display(Name = "Вид правна помощ")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}")]
        public int LawyerHelpTypeId { get; set; }

        [Column("case_session_act_id")]
        [Display(Name = "Съдебен акт за допускане на правна помощ")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}")]
        public int CaseSessionActId { get; set; }

        [Column("has_interest_conflict")]
        [Display(Name = "Противоречиви интереси ")]
        public bool HasInterestConflict { get; set; }

        [Column("prev_defender_name")]
        [Display(Name = "Служебен защитник на предходна инстанция")]
        public string PrevDefenderName { get; set; }

        [Column("description")]
        [Display(Name = "Допълнителна информация относно искането")]
        public string Description { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [Column("case_session_to_go_id")]
        [Display(Name = "Да се явят на заседание")]
        public int? CaseSessionToGoId { get; set; }

        [Column("act_appointment_id")]
        [Display(Name = "Съдебен акт за назначаване")]
        public int? ActAppointmentId { get; set; }

        [Column("lawyer_help_basis_appointment_id")]
        [Display(Name = "Основание за назначаване")]
        public int? LawyerHelpBasisAppointmentId { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(LawyerHelpBaseId))]
        public virtual LawyerHelpBase LawyerHelpBase { get; set; }

        [ForeignKey(nameof(LawyerHelpTypeId))]
        public virtual LawyerHelpType LawyerHelpType { get; set; }

        [ForeignKey(nameof(CaseSessionToGoId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(ActAppointmentId))]
        public virtual CaseSessionAct ActAppointment { get; set; }

        [ForeignKey(nameof(LawyerHelpBasisAppointmentId))]
        public virtual LawyerHelpBasisAppointment LawyerHelpBasisAppointment { get; set; }

        public virtual ICollection<CaseLawyerHelpOtherLawyer> CaseLawyerHelpOtherLawyers { get; set; }
        public virtual ICollection<CaseLawyerHelpPerson> CaseLawyerHelpPersons { get; set; }

        public CaseLawyerHelp()
        {
            CaseLawyerHelpOtherLawyers = new HashSet<CaseLawyerHelpOtherLawyer>();
            CaseLawyerHelpPersons = new HashSet<CaseLawyerHelpPerson>();
        }
    }
}
