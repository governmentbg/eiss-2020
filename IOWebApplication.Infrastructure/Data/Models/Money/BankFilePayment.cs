using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    [Table("money_bank_file_payment")]
    [Comment("Плащанията в един банков файл")]
    public class BankFilePayment : IExpiredInfo
    {
        [Key]
        [Column("id")]
        [Comment("Идентификатор")]
        public long Id { get; set; }

        [Column("bank_file_id")]
        [Comment("Идентификатор на банковия файл")]
        public long BankFileId { get; set; }

        [Column("bank_id")]
        [Comment("Банков идентификатор")]
        public string BankId { get; set; }

        [Column("paid_date")]
        [Comment("Дата на плащане")]
        public DateTime PaidDate { get; set; }

        [Column("amount")]
        [Comment("Сума")]
        public decimal Amount { get; set; }

        [Column("currency_id")]
        [Comment("Валута")]
        public int CurrencyId { get; set; }

        [Column("payment_type_id")]
        [Comment("Начин на плащане - ПОС, Платежно")]
        public int PaymentTypeId { get; set; }

        [Column("debtor_names")]
        [Comment("Име на задълженото лице")]
        public string DebtorNames { get; set; }

        [Column("debtor_identifier")]
        [Comment("Идентификатор на задълженото лице")]
        public string DebtorIdentifier { get; set; }

        [Column("sender_name")]
        [Comment("Вносител")]
        public string SenderName { get; set; }

        [Column("payment_Info")]
        [Comment("Основание")]
        public string PaymentInfo { get; set; }

        [Column("payment_description")]
        [Comment("Основание2")]
        public string PaymentDescription { get; set; }

        [Column("description")]
        [Comment("Описание от обработката")]
        public string Description { get; set; }

        [Column("payment_id")]
        [Comment("Идентификатор на плащане")]
        public int? PaymentId { get; set; }

        [Column("payment_status")]
        [Comment("Статус от обработката")]
        public int PaymentStatus { get; set; }

        [Column("payment_type_code_id")]
        [Comment("кредит/дебит")]
        public int PaymentTypeCodeId { get; set; }

        [Column("line_text")]
        [Comment("Целият ред както е във файла")]
        public string LineText { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [Column("old_amount")]
        [Comment("Сума")]
        public decimal? OldAmount { get; set; }

        [Column("old_currency_id")]
        [Comment("Валута")]
        public int? OldCurrencyId { get; set; }

        [ForeignKey(nameof(BankFileId))]
        public virtual BankFile BankFile { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public virtual Payment Payment { get; set; }

        [ForeignKey(nameof(CurrencyId))]
        public virtual Currency Currency { get; set; }

        [ForeignKey(nameof(PaymentTypeCodeId))]
        public virtual BankFilePaymentTypeCode PaymentTypeCode { get; set; }

        [ForeignKey(nameof(PaymentTypeId))]
        public virtual BankFilePaymentType PaymentType { get; set; }
    }
}
