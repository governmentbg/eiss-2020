// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Натовареност на съдии - допълнителни дейности
    /// </summary>
    [Table("nom_judge_load_activity")]
    public class JudgeLoadActivity : BaseCommonNomenclature
    {
        /// <summary>
        /// Елементи с еднакви групи неможе да се повтарят в рамките на годината
        /// </summary>
        [Column("group_no")]
        [Display(Name = "Група")]
        public int? GroupNo { get; set; }
    }
}
