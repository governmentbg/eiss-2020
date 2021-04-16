using IOWebApplication.Infrastructure.Data.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Точен вид документ
    /// </summary>
    [Table("nom_document_type")]
    public class DocumentType : BaseCommonNomenclature
    {
        [Column("document_group_id")]
        [Display(Name ="Основен вид документ")]
        public int DocumentGroupId { get; set; }

        [Column("document_template_name")]
        [Display(Name ="Наименование на бланка")]
        public string DocumentTemplateName { get; set; }

        /// <summary>
        /// Тип на бланка
        /// </summary>
        [Column("html_template_type_id")]
        [Display(Name = "Тип на бланка")]
        public int? HtmlTemplateTypeId { get; set; }

        [Column("alias")]
        [Display(Name = "Код на тип документ")]
        public string Alias { get; set; }

        [Column("decision_case_select")]
        public bool? DecisionCaseSelect { get; set; }

        //Бланка по подразбиране
        [Column("default_html_template_id")]
        public int? DefaultHtmlTemplateId { get; set; }


        [ForeignKey(nameof(DocumentGroupId))]
        public virtual DocumentGroup DocumentGroup { get; set; }

        [ForeignKey(nameof(HtmlTemplateTypeId))]
        public virtual HtmlTemplateType HtmlTemplateType { get; set; }

        [ForeignKey(nameof(DefaultHtmlTemplateId))]
        public virtual HtmlTemplate DefaultHtmlTemplate { get; set; }
    }
}
