using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Организационна структура на съд
    /// </summary>
    [Table("common_court_organization")]
    public class CourtOrganization
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("parent_id")]
        [Display(Name = "Горно ниво")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете горно ниво")]
        public int? ParentId { get; set; }

        [Column("label")]
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Въведете наименование")]
        public string Label { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("organization_level_id")]
        [Display(Name = "Ниво")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете ниво")]
        public int OrganizationLevelId { get; set; }

        [Column("is_document_registry")]
        [Display(Name = "Деловодна регистратура")]
        public bool? IsDocumentRegistry { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual CourtOrganization ParentOrganization { get; set; }

        [ForeignKey(nameof(OrganizationLevelId))]
        public virtual OrganizationLevel OrganizationLevel { get; set; }        
    }
}
