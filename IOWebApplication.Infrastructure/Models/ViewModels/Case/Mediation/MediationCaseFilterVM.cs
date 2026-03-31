// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Филтър за извличане на дела за медиация
    /// </summary>
    public class MediationCaseFilterVM
    {
        /// <summary>
        /// От дата
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Година
        /// </summary>
        [Display(Name = "Година")]
        public int? CaseYear { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }

        /// <summary>
        /// Основен вид
        /// </summary>
        [Display(Name = "Основен вид")]
        public int CaseGroupId { get; set; }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        /// <summary>
        /// Шифър
        /// </summary>
        [Display(Name = "Шифър")]
        public string[] CaseCodeIds { get; set; }

        /// <summary>
        /// Подшифър
        /// </summary>
        [Display(Name = "Подшифър")]
        public int? CaseCodeSubId { get; set; }
    }
}
