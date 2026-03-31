using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Ограничения на ниво съд
    /// </summary>
    [Table("common_court_restriction")]
    public class CourtRestriction
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("restriction_type")]
        public int RestrictionType { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Display(Name = "Начална дата")]
        [Column("date_start")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        [Column("date_end")]
        public DateTime? DateEnd { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }


    }
}
