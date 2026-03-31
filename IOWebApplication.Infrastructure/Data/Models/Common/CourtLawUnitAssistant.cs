// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Асистент/помощник/секретар
    /// </summary>
    [Table("common_court_lawunit_assistant")]
    public class CourtLawUnitAssistant
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на лице към съдилище
        /// </summary>
        [Column("court_law_unit_id")]
        public int CourtLawUnitId { get; set; }

        /// <summary>
        /// Идентификатор на асистент/помощник/секретар
        /// </summary>
        [Column("lawunit_id")]
        public int LawUnitId { get; set; }

        /// <summary>
        /// Идентификатор на роля
        /// </summary>
        [Column("judge_role_id")]
        public int JudgeRoleId { get; set; }

        /// <summary>
        /// Дата на добавяне
        /// </summary>
        [Column("date_from")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Дата на анулиране
        /// </summary>
        [Column("date_expired")]
        public DateTime? DateExpired { get; set; }

        /// <summary>
        /// Асистент/помощник/секретар
        /// </summary>
        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        /// <summary>
        /// Лице към съдилище
        /// </summary>
        [ForeignKey(nameof(CourtLawUnitId))]
        public virtual CourtLawUnit CourtLawUnit { get; set; }

        /// <summary>
        /// Роля
        /// </summary>
        [ForeignKey(nameof(JudgeRoleId))]
        public virtual JudgeRole JudgeRole { get; set; }
    }
}
