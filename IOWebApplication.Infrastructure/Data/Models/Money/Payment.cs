using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    /// <summary>
    /// Плащания
    /// </summary>
    [Table("money_payment")]
    public class Payment : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("payment_type_id")]
        [Display(Name = "Начин на плащане")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int PaymentTypeId { get; set; }

        [Column("is_avans")]
        public bool IsAvans { get; set; }

        [Column("court_bank_account_id")]
        [Display(Name = "По сметка")]
        public int? CourtBankAccountId { get; set; }

        [Column("amount")]
        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Column("paid_date")]
        [Display(Name = "Платено на")]
        public DateTime PaidDate { get; set; }

        [Column("sender_name")]
        [Display(Name = "Вносител")]
        public string SenderName { get; set; }

        [Column("payment_Info")]
        [Display(Name = "Основание")]
        public string PaymentInfo { get; set; }

        [Column("payment_description")]
        [Display(Name = "Основание2")]
        public string PaymentDescription { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("date_disabled")]
        public DateTime? DateDisabled { get; set; }

        [Column("user_disabled")]
        public string UserDisabledId { get; set; }

        [Column("payment_number")]
        public string PaymentNumber { get; set; }


        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(PaymentTypeId))]
        public virtual PaymentType PaymentType { get; set; }

        [ForeignKey(nameof(CourtBankAccountId))]
        public virtual CourtBankAccount CourtBankAccount { get; set; }

        [ForeignKey(nameof(UserDisabledId))]
        public virtual ApplicationUser UserDisabled { get; set; }

        public virtual ICollection<ObligationPayment> ObligationPayments { get; set; }
        public virtual ICollection<PosPaymentResult> PosPaymentResults { get; set; }

        public Payment()
        {
            ObligationPayments = new HashSet<ObligationPayment>();
            PosPaymentResults = new HashSet<PosPaymentResult>();
        }
    }
}
