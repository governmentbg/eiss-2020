using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Съдии към дежурство
    /// </summary>
    [Table("common_court_duty_lawunit")]
    public class CourtDutyLawUnit
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_duty_id")]
        public int CourtDutyId { get; set; }

        [Column("lawunit_id")]
        public int LawUnitId { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtDutyId))]
        public virtual CourtDuty CourtDuty { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }
    }
}
