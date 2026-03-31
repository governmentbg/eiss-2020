using IOWebApplication.Infrastructure.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    [Table("money_bank_file")]
    [Comment("Банкови файлове с извлечения по сметка")]
    public class BankFile
    {
        [Key]
        [Column("id")]
        [Comment("Идентификатор")]
        public long Id { get; set; }

        [Column("file_name")]
        [Comment("Име на файла")]
        public string FileName { get; set; }

        /// <summary>
        /// Имам и IBAN за да знам какво точно е дошло, тъй като в CourtBankAccount може да се редактира
        /// </summary>
        [Column("iban")]
        [Comment("IBAN")]
        public string Iban { get; set; }

        [Column("court_id")]
        [Comment("Съд")]
        public int CourtId { get; set; }

        [Column("date_created")]
        [Comment("Дата на създаване")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        [Column("court_bank_account_id")]
        [Comment("По сметка")]
        public int CourtBankAccountId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CourtBankAccountId))]
        public virtual CourtBankAccount CourtBankAccount { get; set; }

        public virtual ICollection<BankFilePayment> Payments { get; set; }

    }
}
