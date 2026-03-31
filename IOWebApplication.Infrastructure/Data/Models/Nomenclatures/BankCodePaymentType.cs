using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Maping Банковите кодове към начин на плащане при нас
    /// </summary>
    [Table("nom_bank_code_payment_type")]
    public class BankCodePaymentType
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("bank_id")]
        [Comment("Идентификатор банка")]
        public int BankId { get; set; }

        [Column("payment_type_id")]
        [Comment("Идентификатор начин на плащане")]
        public int PaymentTypeId { get; set; }

        [Column("bank_codes", TypeName = "jsonb")]
        [Comment("Банковите кодове към начин на плащане при нас - ще се ползва за файл MT940")]
        public List<string> BankCodes { get; set; }

        [ForeignKey(nameof(PaymentTypeId))]
        public virtual BankFilePaymentType PaymentType { get; set; }

        [ForeignKey(nameof(BankId))]
        public virtual Bank Bank { get; set; }

    }
}
