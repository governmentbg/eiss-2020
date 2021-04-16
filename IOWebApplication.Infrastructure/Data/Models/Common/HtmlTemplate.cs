using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// HTML Бланки на документи
    /// </summary>
    [Table("common_html_template")]
    public class HtmlTemplate
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Вид бланка
        /// </summary>
        [Column("html_template_type_id")]
        [Display(Name = "Вид документ")]
        public int HtmlTemplateTypeId { get; set; }

        [Column("alias")]
        [Display(Name = "Указател на бланка")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string Alias { get; set; }

        [Column("label")]
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string Label { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("content")]
        public byte[] Content { get; set; }

        [Column("file_name")]
        public string FileName { get; set; }

        [Column("content_type")]
        public string ContentType { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [Column("date_uploaded")]
        public DateTime DateUploaded { get; set; }

        [Column("line_height")]
        public decimal? LineHeight { get; set; }

        [Column("style_template_id")]
        public int? StyleTemplateId { get; set; }

        [Column("frame_template_id")]
        public int? FrameTemplateId { get; set; }

        [Column("document_type_alias")]
        [Display(Name = "Код на тип документ")]
        public string DocumentTypeAlias { get; set; }

        [Column("smart_shrinking_pdf")]
        [Display(Name = "Smart Shrinking PDF")]
        public bool? SmartShrinkingPDF { get; set; }

        [Column("have_session_act")]
        [Display(Name = "Насочено е към aкт/протокол")]
        public bool? HaveSessionAct { get; set; }

        [Column("have_session_act_complain")]
        [Display(Name = "Насочено е към жалба към aкт/протокол")]
        public bool? HaveSessionActComplain { get; set; }

        [Column("required_session_act_complain")]
        [Display(Name = "Задължителна има жалба към aкт/протокол")]
        public bool? RequiredSessionActComplain { get; set; }

        [Column("have_multi_act_complain")]
        [Display(Name = "Насочено е към много жалби")]
        public bool? HaveMultiActComplain { get; set; }

        [Column("have_act_complain_free")]
        [Display(Name = "Допуска се избиране на жалба която не е насочена към избраният акт")]
        public bool? HaveActComplainFree { get; set; }

        [Column("have_expert_report")]
        [Display(Name = "Има данни за експериза")]
        public bool? HaveExpertReport { get; set; }

        [Column("xls_title_row")]
        [Display(Name = "Ред за филтър")]
        public int? XlsTitleRow { get; set; }

        [Column("xls_data_row")]
        [Display(Name = "Ред за данни")]
        public int? XlsDataRow { get; set; }

        [Column("xls_recap_row")]
        [Display(Name = "Ред за рекапитулация")]
        public int? XlsRecapRow { get; set; }

        [Column("is_create")]
        [Display(Name = "Създадена в системата")]
        public bool? IsCreate { get; set; }

        /// <summary>
        /// Да се избира ли лице от делото в DocumentTemplate
        /// </summary>
        [Column("have_case_person")]
        [Display(Name = "Избор на лице от делото")]
        public bool? HaveCasePerson { get; set; }

        [Column("have_document_sender_person")]
        [Display(Name = "Има подател нa съпровождащ документ")]
        public bool? HaveDocumentSenderPerson { get; set; }

        [ForeignKey(nameof(HtmlTemplateTypeId))]
        public virtual HtmlTemplateType HtmlTemplateType { get; set; }

        public virtual ICollection<HtmlTemplateLink> HtmlTemplateLinks { get; set; }

        [NotMapped]
        [Display(Name = "Landscape")]
        public bool LandscapeVM { get { return (StyleTemplateId == 224); } set { StyleTemplateId = value ? 224 : 223; } }
        
    }
}
