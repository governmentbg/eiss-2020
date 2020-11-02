// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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

        [Column("case_code_id")]
        [Display(Name = "Шифър")]
        public int? CaseCodeId { get; set; }

        [Column("process_priority_id")]
        [Display(Name = "Вид производство")]
        public int? ProcessPriorityId { get; set; }

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

        public virtual ICollection<CaseLoadElementType> CaseLoadElementTypes { get; set; }
    }
}
