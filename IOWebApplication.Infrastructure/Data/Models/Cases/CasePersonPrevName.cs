using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Предишни имена на лице за бюлетин на съдимост
    /// </summary>
    [Table("case_person_prev_name")]
    public class CasePersonPrevName : PersonNamesBase, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Лице")]
        public int CasePersonId { get; set; }

        [Column("prev_name_type_id")]
        [Display(Name = "Вид предходно име")]
        [Range(1, 999999, ErrorMessage = "Изберете {0}.")]
        public int PersonPrevNamesTypeId { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(PersonPrevNamesTypeId))]
        public virtual PersonPrevNamesType PersonPrevNamesType { get; set; }

        //################################################################################
        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }
        [Column("date_wrt")]
        public DateTime DateWrt { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
    }
}
