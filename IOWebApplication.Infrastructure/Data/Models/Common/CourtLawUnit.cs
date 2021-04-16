using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Лица към съдилища
    /// </summary>
    [Table("common_court_lawunit")]
    public class CourtLawUnit
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("lawunit_id")]
        public int LawUnitId { get; set; }

        /// <summary>
        /// 1-съдия,2-заседател,3-прокурор,4-вещи лица
        /// </summary>
        [Column("law_unit_type_id")]
        public int? LawUnitTypeId { get; set; }

        [Column("period_type_id")]
        public int PeriodTypeId { get; set; }

        [Column("lawunit_position_id")]
        [Display(Name = "Длъжност")]
        public int? LawUnitPositionId { get; set; }

        [Column("court_organization_id")]
        [Display(Name = "Структура")]
        public int? CourtOrganizationId { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(PeriodTypeId))]
        public virtual PeriodType PeriodType { get; set; }

        [ForeignKey(nameof(LawUnitPositionId))]
        public virtual LawUnitPosition LawUnitPosition { get; set; }

        [ForeignKey(nameof(CourtOrganizationId))]
        public virtual CourtOrganization CourtOrganization { get; set; }

        [ForeignKey(nameof(LawUnitTypeId))]
        public virtual LawUnitType LawUnitType { get; set; }

        [NotMapped]
        public int MasterLawUnitTypeId { get; set; }

    }
}
