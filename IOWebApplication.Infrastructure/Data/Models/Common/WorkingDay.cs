using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Календар на почивни/извънредно работни дни
    /// </summary>
    [Table("common_working_day")]
    public class WorkingDay 
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("day")]
        [Display(Name = "Дата")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public DateTime Day { get; set; }

        /// <summary>
        /// Ако е null, важи за всички съдилища
        /// </summary>
        [Column("court_id")]
        [Display(Name = "Съд")]        
        public int? CourtId { get; set; }

        /// <summary>
        /// Вид ден: 1-Почивен,2-Работен
        /// </summary>
        [Column("day_type_id")]
        [Display(Name = "Тип ден")]       
        public int DayTypeId { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }


        [ForeignKey(nameof(DayTypeId))]
        public virtual DayType DayType { get; set; }
    }
}
