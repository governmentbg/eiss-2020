using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    // Видове заявления и настройки на документ/дело
    [Table("nom_document_request_type")]
    public class DocumentRequestType
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("request_code")]
        [MaxLength(20)]
        public string RequestCode { get; set; }

        [Column("label")]
        public string Label { get; set; }

        [Display(Name = "Основен вид документ")]
        [Column("document_group_id")]
        public int DocumentGroupId { get; set; }

        [Display(Name = "Точен вид документ")]
        [Column("document_type_id")]
        public int DocumentTypeId { get; set; }

        [Column("case_group_id")]
        public int? CaseGroupId { get; set; }

        [Column("case_type_id")]
        public int? CaseTypeId { get; set; }

        [Column("case_code_id")]
        public int? CaseCodeId { get; set; }

        [Column("init_request_code")]
        [MaxLength(20)]
        public string InitRequestCode { get; set; }
    }
}
