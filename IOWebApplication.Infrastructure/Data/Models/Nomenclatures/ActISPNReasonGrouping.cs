using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групиране на Основание по ТЗ за актове по дела по несъстоятелност - за справки и на всеки за каквото му трябва
    /// </summary>
    [Table("nom_act_ispn_reason_grouping")]
    public class ActISPNReasonGrouping
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("act_ispn_reason_id")]
        public int ActISPNReasonId { get; set; }

        [Column("act_ispn_reason_group")]
        public int ActISPNReasonGroup { get; set; }

        [ForeignKey(nameof(ActISPNReasonId))]
        public virtual ActISPNReason ActISPNReason { get; set; }
    }
}
