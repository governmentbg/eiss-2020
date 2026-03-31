// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за редакция на дело на данни свързани с медиация
    /// </summary>
    public class MediationCaseEditVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Флаг оказващ делото дали подлежи на медиация
        /// </summary>
        [Display(Name = "Делото подлежи на медиация")]
        public bool? IsMediation { get; set; }

        /// <summary>
        /// Идентификатор на подшифри в дело
        /// </summary>
        [Display(Name = "Подшифър")]
        public int? CaseCodeSubId { get; set; }

        /// <summary>
        /// Идентификатор на вид процедура по медиация
        /// </summary>
        [Display(Name = "Процедура по медиация")]
        public int? MediationProcedureId { get; set; }
    }
}
