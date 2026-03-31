using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове бланки по вид съд/дело
    /// </summary>
    [Table("nom_blank_template_link")]
    public class BlankTemplateLink
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("blank_type_id")]
        public int BlankTemplateId { get; set; }

        [Column("court_type_id")]
        public int? CourtTypeId { get; set; }

        [Column("case_instance_id")]
        public int? CaseInstanceId { get; set; }

        [Column("case_group_id")]
        public int? CaseGroupId { get; set; }

        [ForeignKey(nameof(BlankTemplateId))]
        public virtual BlankTemplate BlankTemplate { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [ForeignKey(nameof(CaseInstanceId))]
        public virtual CaseInstance CaseInstance { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
