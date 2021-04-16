using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Специалности към заседатели
    /// </summary>
    [Table("common_lawunit_speciality")]
    public class LawUnitSpeciality
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("lawunit_id")]
        public int LawUnitId { get; set; }

        [Column("speciality_id")]
        public int SpecialityId { get; set; }

        [Column("date_from")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(SpecialityId))]
        public virtual Speciality Speciality { get; set; }
    }
}
