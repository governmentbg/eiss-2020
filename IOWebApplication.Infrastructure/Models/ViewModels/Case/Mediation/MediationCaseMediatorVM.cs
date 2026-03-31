// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за медиатори към дело
    /// </summary>
    public class MediationCaseMediatorVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Идентификатор на медиатор
        /// </summary>
        [Display(Name = "Медиатор")]
        public int MediationMediatorId { get; set; }

        /// <summary>
        /// Идентификатор на вид избор на медиатора в делото
        /// </summary>
        [Display(Name = "Избран от")]
        public int? MediationTypeChoiceMediatorId { get; set; }

        /// <summary>
        /// От
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До
        /// </summary>
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        /// <summary>
        /// Флаг дали се разрешава сторно
        /// </summary>
        public bool AllowedExpired { get; set; } = true;
    }
}
