// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за медиация
    /// </summary>
    public class MediationCasePreviewVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int CourtId { get; set; }

        /// <summary>
        /// Съд
        /// </summary>
        [Display(Name = "Съд")]
        public string CourtLabel { get; set; }

        /// <summary>
        /// Основен вид дело
        /// </summary>
        [Display(Name = "Основен вид дело")]
        public string CaseGroupLabel { get; set; }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        [Display(Name = "Точен вид дело")]
        public string CaseTypeLabel { get; set; }

        /// <summary>
        /// Шифри по точен вид дело
        /// </summary>
        [Display(Name = "Шифри по точен вид дело")]
        public string CaseCodeLabel { get; set; }

        /// <summary>
        /// Кратък номер
        /// </summary>
        [Display(Name = "Кратък номер")]
        public string ShortNumber { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }

        /// <summary>
        /// Дата на образуване
        /// </summary>
        [Display(Name = "Дата на образуване")]
        public DateTime RegDate { get; set; }

        /// <summary>
        /// Обвързано с дела
        /// </summary>
        [Display(Name = "Обвързано с дела")]
        public string ParentLinkCases { get; set; }
    }
}
