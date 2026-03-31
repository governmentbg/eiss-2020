using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Върнати адвокати по заявка за правна помощ от ЕЕСПП
    /// </summary>
    [Table("case_lawyer_help_assigned_lawyer")]
    public class CaseLawyerHelpAssignedLawyer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_lawyer_help_id")]
        public int CaseLawyerHelpId { get; set; }

        public DateTime DateReturned { get; set; }

        [Column("lawyer_number")]
        public string LawyerNumber { get; set; }

        [Column("lawyer_name")]
        public string LawyerName { get; set; }

        /// <summary>
        /// САМО за целите на трансфера
        /// </summary>
        [Column("notification_id")]
        public string NotificationId { get; set; }

        [Column("lawyer_state_id")]
        public int LawyerStateId { get; set; }

        [Column("lawyer_state_date")]
        public DateTime? LawyerStateDate { get; set; }

        [Column("state_remark")]
        public string StateRemark { get; set; }

        [Column("case_session_act_assigned_id")]
        public int? CaseSessionActAssignedId { get; set; }

        [ForeignKey(nameof(CaseLawyerHelpId))]
        public virtual CaseLawyerHelp CaseLawyerHelp { get; set; }

        [ForeignKey(nameof(LawyerStateId))]
        public virtual EesppLawyerState LawyerState { get; set; }

        [ForeignKey(nameof(CaseSessionActAssignedId))]
        public virtual CaseSessionAct CaseSessionActAssigned { get; set; }

        public virtual ICollection<CaseLawyerHelpAssignedLawyerPerson> Persons { get; set; }

        public CaseLawyerHelpAssignedLawyer()
        {
            Persons = new HashSet<CaseLawyerHelpAssignedLawyerPerson>();
        }
    }
}
