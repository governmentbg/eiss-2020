using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Регистър на ръчно добавени съдии при масов отвод
    /// </summary>
    [Table("case_lawunit_manual_judge")]
    public class CaseLawUnitManualJudge : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_id")]
        [Display(Name ="Съдебно дело")]
        public int CaseId { get; set; }

        [Column("lawunit_id")]
        [Display(Name ="Съдия")]
        public int LawUnitId { get; set; }

        [Column("judge_role_id")]
        [Display(Name ="Роля в делото")]
        [Required(ErrorMessage ="Изберете {0}.")]
        public int JudgeRoleId { get; set; }

        [Column("date_from")]
        [Display(Name ="Дата от")]
        public DateTime DateFrom { get; set; }        

        [Column("description")]
        [Display(Name ="Основание")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(JudgeRoleId))]
        public virtual JudgeRole JudgeRole { get; set; }
    }
}
