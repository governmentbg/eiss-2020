using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Списък на експортирани бюлетини за съдимост
    /// </summary>
    [Table("case_person_sentence_bulletin_file")]
    public class CasePersonSentenceBulletinFile : UserDateWRT, IHaveId
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_person_sentence_bulletin__id")]
        public int CasePersonSentenceBulletinId { get; set; }

        [Display(Name = "Регистрационен номер")]
        [Column("reg_number")]
        public string RegNumber { get; set; }

        [Display(Name = "Дата на регистриране")]
        [Column("reg_date")]
        public DateTime? RegDate { get; set; }

        [Column("reg_user_id")]
        [Display(Name = "Потребител, регистрирал на бюлетина")]
        public string RegUserId { get; set; }

        [Display(Name = "Дата на последно подписване")]
        [Column("date_signed")]
        public DateTime? DateSigned { get; set; }

        [Display(Name = "Дата на успешен експорт към ЦАЙС Съдебен статус")]
        [Column("date_submited")]
        public DateTime? DateSubmited { get; set; }

        [Display(Name = "Дата на регистриране в ЦАЙС")]
        [Column("date_registered_in_cais")]
        public DateTime? DateRegisteredInCais { get; set; }

        [ForeignKey(nameof(RegUserId))]
        public virtual ApplicationUser RegUser { get; set; }

        [ForeignKey(nameof(CasePersonSentenceBulletinId))]
        public virtual CasePersonSentenceBulletin CasePersonSentenceBulletin { get; set; }

    }
}
