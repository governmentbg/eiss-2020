using IOWebApplication.Infrastructure.Contracts;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    public class BaseCommonNomenclature : ICommonNomenclature
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Display(Name = "Номер по ред")]
        [Column("order_number")]
        public int OrderNumber { get; set; }

        [Column("code")]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [Column("label")]
        [Display(Name = "Етикет")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string Label { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }


        [Display(Name = "Активен")]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Display(Name = "Начална дата")]
        [Column("date_start")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        [Column("date_end")]
        public DateTime? DateEnd { get; set; }
    }
}
