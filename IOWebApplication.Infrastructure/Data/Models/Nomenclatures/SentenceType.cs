using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид присъда: лишаване от свобода,глоба и т.н
    /// </summary>
    [Table("nom_sentence_type")]
    public class SentenceType : BaseCommonNomenclature
    {
        /// <summary>
        /// Период
        /// </summary>
        [Column("has_period")]
        public bool? HasPeriod { get; set; }

        /// <summary>
        /// Пари
        /// </summary>
        [Column("has_money")]
        public bool? HasMoney { get; set; }

        /// <summary>
        /// Пробация
        /// </summary>
        [Column("has_probation")]
        public bool? HasProbation { get; set; }

        /// <summary>
        /// Ефективна присъда
        /// </summary>
        [Column("is_effective")]
        public bool? IsEffective { get; set; }

        /// <summary>
        /// Предварително задържане
        /// </summary>
        [Column("has_preliminary_detention")]
        public bool? HasPreliminaryDetention { get; set; }
    }
}
