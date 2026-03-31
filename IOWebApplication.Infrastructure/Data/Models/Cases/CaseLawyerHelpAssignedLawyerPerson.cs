using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Връзки между подадени лица към адвокат
    /// </summary>
    [Table("case_lawyer_help_assigned_lawyer_person")]
    public class CaseLawyerHelpAssignedLawyerPerson
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_lawyer_help_assigned_lawyer_id")]
        public int CaseLawyerAssignedLawyerId { get; set; }

        [Column("case_lawyer_help_person_id")]
        public int CaseLawyerHelpPersonId { get; set; }

        [ForeignKey(nameof(CaseLawyerAssignedLawyerId))]
        public virtual CaseLawyerHelpAssignedLawyer CaseLawyerAssignedLawyer { get; set; }

        [ForeignKey(nameof(CaseLawyerHelpPersonId))]
        public virtual CaseLawyerHelpPerson CaseLawyerHelpPerson { get; set; }
    }
}
