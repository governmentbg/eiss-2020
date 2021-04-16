using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_institution")]
    public class Institution : PersonNamesBase
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 1-съд,2-прокуратура,3-од мвр,чси,нотариуси и други
        /// </summary>
        [Column("institution_type_id")]
        public int InstitutionTypeId { get; set; }       

        [Column("code")]
        [Display(Name ="Код/Номер")]
        public string Code { get; set; }

        [Column("eispp_code")]
        [Display(Name ="ЕИСПП код")]
        public string EISPPCode { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [Column("court_region_id")]
        public int? CourtRegionId { get; set; }

        [ForeignKey(nameof(InstitutionTypeId))]
        public virtual InstitutionType InstitutionType { get; set; }

        [ForeignKey(nameof(CourtRegionId))]
        public virtual CourtRegion CourtRegion { get; set; }

        public Institution()
        {
            Person = new Person();
        }
    }
}
