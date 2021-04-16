using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Ставки за заплащане на заседатели по съд
    /// </summary>
    [Table("common_jury_fee")]
    public class CourtJuryFee : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("hour_fee")]
        [Display(Name = "Възнаграждение на час")]
        public decimal HourFee { get; set; }

        [Column("min_date_fee")]
        [Display(Name = "Минимална сума на ден")]
        public decimal MinDayFee { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

    }
}
