using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// HTML Бланки на документи: Връзки по вид съд/дело
    /// </summary>
    [Table("common_html_template_link")]
    public class HtmlTemplateLink
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("html_template_id")]
        public int HtmlTemplateId { get; set; }

        [Column("court_type_id")]
        [Display(Name = "Вид съд")]
        public int? CourtTypeId { get; set; }

        [Column("case_group_id")]
        [Display(Name = "Основен вид дело")]
        public int? CaseGroupId { get; set; }

        [Column("is_active")]
        [Display(Name = "Активен")]
        public bool? IsActive { get; set; }

        [Column("source_type")]
        [Display(Name = "Вид обект")]
        public int? SourceType { get; set; }

        [ForeignKey(nameof(HtmlTemplateId))]
        public virtual HtmlTemplate HtmlTemplate { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
