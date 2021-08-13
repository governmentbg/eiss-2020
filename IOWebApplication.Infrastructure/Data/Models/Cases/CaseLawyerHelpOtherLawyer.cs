using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Адвокати на другата страна
    /// </summary>
    [Table("case_lawyer_help_other_lawyer")]
    public class CaseLawyerHelpOtherLawyer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_lawyer_help_id")]
        public int CaseLawyerHelpId { get; set; }

        [Column("case_person_id")]
        public int CasePersonId { get; set; }      

        [ForeignKey(nameof(CaseLawyerHelpId))]
        public virtual CaseLawyerHelp CaseLawyerHelp { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }       
    }
}
