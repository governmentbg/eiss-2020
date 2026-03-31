using IOWebApplication.Infrastructure.Data.Models.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид група за натовареност по дела - основни дейности
    /// </summary>
    [Table("nom_case_load_element_group")]
    public class CaseLoadElementGroup : BaseCommonNomenclature
    {
        [Column("is_additional")]
        [Display(Name = "Допълнителна дейност")]
        public bool IsAdditional { get; set; }

        [Column("is_ND")]
        [Display(Name = "Наказателно дело")]
        public bool IsND { get; set; }

        [Column("case_instance_id")]
        [Display(Name = "Инстанция")]
        public int CaseInstanceId { get; set; }

        [Column("case_type_id")]
        [Display(Name = "Точен вид")]
        public int? CaseTypeId { get; set; }

        [Column("document_type_id")]
        [Display(Name = "Вид документ")]
        public int? DocumentTypeId { get; set; }

        [Column("document_type_ids")]
        [Display(Name = "Видове документи")]
        public string DocumentTypeIds { get; set; }

        [Column("case_code_id")]
        [Display(Name = "Шифър")]
        public int? CaseCodeId { get; set; }

        [Column("process_priority_id")]
        [Display(Name = "Вид производство")]
        public int? ProcessPriorityId { get; set; }

        [Column("court_id")]
        [Display(Name = "Съд")]
        public int? CourtId { get; set; }

        [Column("court_type_id")]
        [Display(Name = "Вид съд")]
        public int? CourtTypeId { get; set; }

        [NotMapped]
        [Display(Name = "Видове документи")]
        public string[] ArrayDocumentTypeIds { get; set; }

        [NotMapped]
        public string StringDocumentTypeIds
        {
            get
            {
                if ((ArrayDocumentTypeIds != null) && ArrayDocumentTypeIds.Length > 0)
                    return string.Join(",", ArrayDocumentTypeIds);
                else
                    return string.Empty;
            }
        }

        [ForeignKey(nameof(CaseInstanceId))]
        public virtual CaseInstance CaseInstance { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual CaseType CaseType { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType DocumentType { get; set; }

        [ForeignKey(nameof(CaseCodeId))]
        public virtual CaseCode CaseCode { get; set; }

        [ForeignKey(nameof(ProcessPriorityId))]
        public virtual ProcessPriority ProcessPriority { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        public virtual ICollection<CaseLoadElementType> CaseLoadElementTypes { get; set; }
    }
}
