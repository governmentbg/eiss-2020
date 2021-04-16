using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Създаване на изходящи документи
    /// </summary>
    [Table("document_template")]
    public class DocumentTemplate : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [Column("source_id")]
        public long SourceId { get; set; }

        [Column("document_kind_id")]
        [Display(Name = "Вид документ")]
        public int DocumentKindId { get; set; }

        [Display(Name = "Основен вид документ")]
        [Column("document_group_id")]
        public int DocumentGroupId { get; set; }

        [Display(Name = "Точен вид документ")]
        [Column("document_type_id")]
        public int DocumentTypeId { get; set; }

        [Column("document_id")]
        public long? DocumentId { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("author_id")]
        [Display(Name = "Автор")]
        public string AuthorId { get; set; }

        [Column("document_template_state_id")]
        [Display(Name = "Статус")]
        public int DocumentTemplateStateId { get; set; }

        /// <summary>
        /// Вид бланка
        /// </summary>
        [Column("html_template_id")]
        [Display(Name = "Бланка")]
        public int? HtmlTemplateId { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Лице")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Лице")]
        public int? CasePersonId { get; set; }

        [Column("case_person_address_id")]
        [Display(Name = "Адрес")]
        public int? CasePersonAddressId { get; set; }


        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(DocumentKindId))]
        public virtual DocumentKind DocumentKind { get; set; }

        [ForeignKey(nameof(DocumentGroupId))]
        public virtual DocumentGroup DocumentGroup { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType DocumentType { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(DocumentTemplateStateId))]
        public virtual DocumentTemplateState DocumentTemplateState { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public virtual ApplicationUser Author { get; set; }

        [ForeignKey(nameof(HtmlTemplateId))]
        public virtual HtmlTemplate HtmlTemplate { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(CasePersonAddressId))]
        public virtual CasePersonAddress CasePersonAddress { get; set; }
    }
}
