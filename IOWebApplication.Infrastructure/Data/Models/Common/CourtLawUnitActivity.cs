using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Допълнителни и административни дейности към съдии по съд
    /// </summary>
    [Table("common_court_lawunit_activity")]
    public class CourtLawUnitActivity : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("lawunit_id")]
        [Display(Name = "Съдия")]
        public int LawUnitId { get; set; }

        [Column("activity_date")]
        [Display(Name = "Дата")]
        public DateTime ActivityDate { get; set; }

        [Column("judge_load_activity_id")]
        [Display(Name = "Дейност")]
        public int JudgeLoadActivityId { get; set; }

        [Column("load_index")]
        [Display(Name = "Индекс")]
        public decimal LoadIndex { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("date_to")]
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

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

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(JudgeLoadActivityId))]
        public virtual JudgeLoadActivity JudgeLoadActivity { get; set; }
    }
}
