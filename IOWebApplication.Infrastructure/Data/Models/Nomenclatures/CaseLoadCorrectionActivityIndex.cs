using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Коригиращи индекси за трудност на дело
    /// </summary>
    [Table("nom_case_load_correction_activity_index")]
    public class CaseLoadCorrectionActivityIndex 
    {

        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("case_load_correction_activity_id")]
        public int CaseLoadCorrectionActivityId { get; set; }

        [Column("court_type_id")]
        [Display(Name = "Вид съд")]
        public int CaseInstanceId { get; set; }

        [Column("load_index")]
        [Display(Name = "Индекс")]
        public decimal LoadIndex { get; set; }

        [Display(Name = "Активен")]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Display(Name = "Начална дата")]
        [Column("date_start")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        [Column("date_end")]
        public DateTime? DateEnd { get; set; }

        [ForeignKey(nameof(CaseLoadCorrectionActivityId))]
        public virtual CaseLoadCorrectionActivity CaseLoadCorrectionActivity { get; set; }

        [ForeignKey(nameof(CaseInstanceId))]
        public virtual CaseInstance CaseInstance { get; set; }
    }
}
