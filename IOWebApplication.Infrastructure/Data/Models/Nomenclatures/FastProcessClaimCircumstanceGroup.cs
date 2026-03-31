using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Конфигуриране на допустимите видове основания
    /// </summary>
    [Table("nom_fastprocess_claim_circumstance_group")]
    public class FastProcessClaimCircumstanceGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [MaxLength(20)]
        [Column("group_code")]
        public string GroupCode { get; set; }

        [Column("fastprocess_claim_circumstance_id")]
        public int FastProcessClaimCircumstanceId { get; set; }

        [ForeignKey(nameof(FastProcessClaimCircumstanceId))]
        public virtual FastProcessClaimCircumstance FastProcessClaimCircumstance { get; set; }
    }
}
