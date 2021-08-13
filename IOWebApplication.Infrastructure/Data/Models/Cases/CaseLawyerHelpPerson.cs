using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Лица, за които се иска правна помощ
    /// </summary>
    [Table("case_lawyer_help_person")]
    public class CaseLawyerHelpPerson: IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_lawyer_help_id")]
        public int CaseLawyerHelpId { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Лице")]
        public int CasePersonId { get; set; }

        [Column("assigned_lawyer_id")]
        [Display(Name = "Назначен адвокат")]
        public int? AssignedLawyerId { get; set; }

        [Column("specified_lawyer")]
        [NotMapped]
        public string SpecifiedLawyer { get; set; }

        [Column("specified_lawyer_lawunit_id")]
        [Display(Name = "Искан адвокат от лицето")]
        public int? SpecifiedLawyerLawUnitId { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(SpecifiedLawyerLawUnitId))]
        public virtual LawUnit SpecifiedLawyerLawUnit { get; set; }

        [ForeignKey(nameof(CaseLawyerHelpId))]
        public virtual CaseLawyerHelp CaseLawyerHelp { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(AssignedLawyerId))]
        public virtual CasePerson AssignedLawyer { get; set; }
    }
}
