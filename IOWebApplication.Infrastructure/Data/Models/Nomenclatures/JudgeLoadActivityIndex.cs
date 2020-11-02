// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Натовареност по дела - допълнителни дейности - стойности по вид съд
    /// </summary>
    [Table("nom_judge_load_activity_index")]
    public class JudgeLoadActivityIndex
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("judge_load_activity_id")]
        public int JudgeLoadActivityId { get; set; }

        [Column("court_type_id")]
        [Display(Name = "Вид съд")]
        public int? CourtTypeId { get; set; }

        [Column("load_index")]
        [Display(Name = "Индекс в часове")]
        public decimal LoadIndex { get; set; }

        [Display(Name = "Активен")]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Display(Name = "Начална дата")]
        [Column("date_start")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        [Column("date_end")]
        public DateTime? DateEnd { get; set; }

        [ForeignKey(nameof(JudgeLoadActivityId))]
        public virtual JudgeLoadActivity JudgeLoadActivity { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }
    }
}
