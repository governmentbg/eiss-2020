using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Забрана за начисляване ако съществуват добавените елементи
    /// </summary>
    [Table("nom_case_load_element_type_stop")]
    public class CaseLoadElementTypeStop : IExpiredInfo
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("case_load_element_type_id")]
        [Display(Name = "Елемент")]
        public int CaseLoadElementTypeId { get; set; }

        [Column("case_load_element_type_stop_id")]
        [Display(Name = "Стопиращ елемент")]
        public int CaseLoadElementTypeStopId { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CaseLoadElementTypeId))]
        public virtual CaseLoadElementType CaseLoadElementType { get; set; }

        [ForeignKey(nameof(CaseLoadElementTypeStopId))]
        public virtual CaseLoadElementType CaseLoadElementTypeStopElement { get; set; }
    }
}
