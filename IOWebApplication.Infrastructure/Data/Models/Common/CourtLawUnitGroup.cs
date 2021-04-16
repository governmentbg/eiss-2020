using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Групи дела по СЪД към съдия в съд
    /// </summary>
    [Table("common_court_lawunit_group")]
    public class CourtLawUnitGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("lawunit_id")]
        public int LawUnitId { get; set; }

        [Column("court_group_id")]
        public int CourtGroupId { get; set; }

        [Column("load_index")]
        [Display(Name = "Натовареност")]
        public int LoadIndex { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(CourtGroupId))]
        public virtual CourtGroup CourtGroup { get; set; }

    }
}
