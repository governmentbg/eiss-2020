using IOWebApplication.Infrastructure.Data.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Мерки към наложени наказания
    /// </summary>
    [Table("case_person_sentence_punishment_measure")]
    public class CasePersonSentencePunishmentMeasure : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_person_sentence_punishment_id")]
        public int CasePersonSentencePunishmentId { get; set; }

        [Column("case_person_measure_id")]
        public int CasePersonMeasureId { get; set; }

        [ForeignKey(nameof(CasePersonSentencePunishmentId))]
        public virtual CasePersonSentencePunishment CasePersonSentencePunishment { get; set; }

        [ForeignKey(nameof(CasePersonMeasureId))]
        public virtual CasePersonMeasure CasePersonMeasure { get; set; }

    }
}
