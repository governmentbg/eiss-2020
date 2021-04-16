using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Групи дела по СЪД
    /// </summary>
    [Table("common_court_group")]
    public class CourtGroup 
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_group_id")]
        [Display(Name = "Основен вид делo")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете oсновен вид делo")]
        public int CaseGroupId { get; set; }

        [Column("label")]
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Изберете наименование на група")]
        public string Label { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }       

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер по ред")]
        [Column("order_number")]
        public int OrderNumber { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        public virtual ICollection<CourtGroupCode> CourtGroupCodes { get; set; }
        public virtual ICollection<CourtLawUnitGroup> CourtLawUnitGroups { get; set; }

        public CourtGroup()
        {
            CourtGroupCodes = new HashSet<CourtGroupCode>();
            CourtLawUnitGroups = new HashSet<CourtLawUnitGroup>();
        }
    }
}
