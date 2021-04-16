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
    [Table("money_pos_payment_result")]
    public class PosPaymentResult : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("payment_id")]
        public int? PaymentId { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("authcode")]
        public string Authcode { get; set; }

        [Column("rrn")]
        public string Rrn { get; set; }

        [Column("tid")]
        public string Tid { get; set; }

        [Column("fperiod")]
        public string Fperiod { get; set; }

        [Column("cardid")]
        public string Cardid { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("reasonCode")]
        public string ReasonCode { get; set; }

        [Column("reasonText")]
        public string ReasonText { get; set; }

        [Column("errorCode")]
        public string ErrorCode { get; set; }

        [Column("posError")]
        public string PosError { get; set; }

        [Column("posLog")]
        public string PosLog { get; set; }

        [Column("court_bank_account_id")]
        public int CourtBankAccountId { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("paid_date")]
        public DateTime PaidDate { get; set; }

        [Column("sender_name")]
        public string SenderName { get; set; }


        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public virtual Payment Payment { get; set; }

        [ForeignKey(nameof(CourtBankAccountId))]
        public virtual CourtBankAccount CourtBankAccount { get; set; }

    }
}
