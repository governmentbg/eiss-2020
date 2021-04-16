using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Веществени доказателства по дело
    /// </summary>
    [Table("case_evidence")]
    public class CaseEvidence : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("evidence_type")]
        [Display(Name = "Тип доказателство")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете тип доказателство")]
        public int EvidenceTypeId { get; set; }

        [Column("reg_number")]
        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Column("file_number")]
        [Display(Name = "Служебно дело")]
        public string FileNumber { get; set; }

        [Column("date_accept")]
        [Display(Name = "Дата на регистрация")]
        public DateTime DateAccept { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        [Required(ErrorMessage = "Въведете '{0}'")]
        public string Description { get; set; }

        [Column("add_info")]
        [Display(Name = "Пояснения")]
        public string AddInfo { get; set; }

        [Column("location")]
        [Display(Name = "Местоположение")]
        public string Location { get; set; }

        [Column("evidence_state_id")]
        [Display(Name = "Статус")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете статус")]
        public int EvidenceStateId { get; set; }

        [Column("reg_number_value")]
        public int? RegNumberValue { get; set; }

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

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(EvidenceTypeId))]
        public virtual EvidenceType EvidenceType { get; set; }

        [ForeignKey(nameof(EvidenceStateId))]
        public virtual EvidenceState EvidenceState { get; set; }
    }
}
