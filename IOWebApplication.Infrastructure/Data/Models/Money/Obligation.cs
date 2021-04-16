using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    /// <summary>
    /// Задължения
    /// </summary>
    [Table("money_obligation")]
    public class Obligation : PersonNamesBase, IUserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_session_act_id")]
        public int? CaseSessionActId { get; set; }

        [Column("case_session_id")]
        public int? CaseSessionId { get; set; }

        [Column("case_session_meeting_id")]
        public int? CaseSessionMeetingId { get; set; }

        [Column("document_id")]
        public long? DocumentId { get; set; }

        [Column("obligation_Info")]
        [Display(Name = "Основание")]
        public string ObligationInfo { get; set; }

        [Column("obligation_description")]
        [Display(Name = "Пояснение")]
        public string ObligationDescription { get; set; }

        [Column("obligation_number")]
        public string ObligationNumber { get; set; }

        [Column("obligation_date")]
        public DateTime ObligationDate { get; set; }

        [Column("money_type_id")]
        [Display(Name = "Вид сума")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int MoneyTypeId { get; set; }

        [Column("amount")]
        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("date_wrt")]
        public DateTime DateWrt { get; set; }

        [Column("date_transfered_dw")]
        public DateTime? DateTransferedDW { get; set; }


        //За заседателите, парите които са до минималната им сума на ден
        [Column("is_for_min_amount")]
        public bool? IsForMinAmount { get; set; }

        //Видовете държавни такси - за тях има и цени
        [Column("money_fee_type_id")]
        public int? MoneyFeeTypeId { get; set; }

        /// <summary>
        /// 1:Приход,-1:Разход
        /// </summary>
        [Column("money_sign")]
        public int? MoneySign { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        //Вид глоба
        [Column("money_fine_type_id")]
        public int? MoneyFineTypeId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(CaseSessionMeetingId))]
        public virtual CaseSessionMeeting CaseSessionMeeting { get; set; }

        [ForeignKey(nameof(MoneyTypeId))]
        public virtual MoneyType MoneyType { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey(nameof(MoneyFeeTypeId))]
        public virtual MoneyFeeType MoneyFeeType { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(MoneyFineTypeId))]
        public virtual MoneyFineType MoneyFineType { get; set; }

        public virtual ICollection<ObligationPayment> ObligationPayments { get; set; }
        public virtual ICollection<ExpenseOrderObligation> ExpenseOrderObligations { get; set; }
        public virtual ICollection<ObligationReceive> ObligationReceives { get; set; }
        public virtual ICollection<ExecListObligation> ExecListObligations { get; set; }

        public Obligation()
        {
            ObligationPayments = new HashSet<ObligationPayment>();
            ExpenseOrderObligations = new HashSet<ExpenseOrderObligation>();
            ObligationReceives = new HashSet<ObligationReceive>();
            ExecListObligations = new HashSet<ExecListObligation>();
        }
    }
}
