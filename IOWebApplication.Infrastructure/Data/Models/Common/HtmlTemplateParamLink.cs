using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// HTML Бланки на документи: Описание на параметри по бланки
    /// </summary>
    [Table("common_html_template_param_link")]
    public class HtmlTemplateParamLink
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("html_template_id")]
        public int HtmlTemplateId { get; set; }

        [Column("html_template_param_id")]
        public int HtmlTemplateParamId { get; set; }

        [ForeignKey(nameof(HtmlTemplateId))]
        public virtual HtmlTemplate HtmlTemplate { get; set; }

        [ForeignKey(nameof(HtmlTemplateParamId))]
        public virtual HtmlTemplateParam HtmlTemplateParam { get; set; }
    }
}
