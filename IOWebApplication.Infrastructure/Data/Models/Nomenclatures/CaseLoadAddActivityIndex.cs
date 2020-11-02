// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Натовареност по дела - допълнителни дейности - стойности по вид съд
    /// </summary>
    [Table("nom_case_load_add_activity_index")]
    public class CaseLoadAddActivityIndex : BaseCommonNomenclature
    {
        [Column("case_load_add_activity_id")]
        public int CaseLoadAddActivityId { get; set; }

        [Column("court_type_id")]
        [Display(Name = "Вид съд")]
        public int CourtTypeId { get; set; }

        [Column("load_index")]
        [Display(Name = "Индекс")]
        public decimal LoadIndex { get; set; }

        [ForeignKey(nameof(CaseLoadAddActivityId))]
        public virtual CaseLoadAddActivity CaseLoadAddActivity { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }
    }
}
