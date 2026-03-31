using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Лица по съдебен акт: Секретари и други по JudgeRole
    /// </summary>
    [Table("case_session_act_lawunit")]
    public class CaseSessionActLawunit
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("case_session_act_id")]
        public int CaseSessionActId { get; set; }

        [Column("judge_role_id")]
        [Required(ErrorMessage = "Изберете {0}.")]
        public int JudgeRoleId { get; set; }

        [Column("lawunit_id")]
        public int LawUnitId { get; set; }

        [Column("lawunit_user_id")]
        public string LawUnitUserId { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(JudgeRoleId))]
        public virtual JudgeRole JudgeRole { get; set; }

        [ForeignKey(nameof(LawUnitUserId))]
        public virtual ApplicationUser LawUnitUser { get; set; }
    }
}
