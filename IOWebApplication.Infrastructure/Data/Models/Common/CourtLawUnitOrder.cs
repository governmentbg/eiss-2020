using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Подреждане на съдии за съдебни актове
    /// </summary>
    [Table("common_court_lawunit_order")]
    public class CourtLawUnitOrder
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("lawunit_id")]
        public int LawUnitId { get; set; }

        [Column("order_number")]
        public int OrderNumber { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }
    }
}
