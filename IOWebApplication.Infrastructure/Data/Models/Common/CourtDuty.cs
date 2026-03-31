using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Дежурства към съд
    /// </summary>
    [Table("common_court_duty")]
    [Comment("Дежурства към съд")]
    public class CourtDuty
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификатор на записа")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        [Column("court_id")]
        [Comment("Идентификатор на съд")]
        public int CourtId { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        [Column("label")]
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Comment("Наименование")]
        public string Label { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Column("description")]
        [Display(Name = "Описание")]
        [Comment("Описание")]
        public string Description { get; set; }

        /// <summary>
        /// Номер заповед
        /// </summary>
        [Column("act_number")]
        [Display(Name = "Номер заповед")]
        [Comment("Номер заповед")]
        public string ActNomer { get; set; }

        /// <summary>
        /// Дата заповед
        /// </summary>
        [Column("act_date")]
        [Display(Name = "Дата заповед")]
        [Comment("Дата заповед")]
        public DateTime? ActDate { get; set; }

        /// <summary>
        /// Дата от
        /// </summary>
        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        [Comment("Дата от")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Дата до
        /// </summary>
        [Column("date_to")]
        [Display(Name = "Дата до")]
        [Comment("Дата до")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Съд
        /// </summary>
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        /// <summary>
        /// Избрани служители
        /// </summary>
        public virtual ICollection<CourtDutyLawUnit> CourtDutyLawUnits { get; set; }

        [NotMapped]
        public virtual List<CheckListVM> CheckCourtDutyLawUnits { get; set; }
    }
}
