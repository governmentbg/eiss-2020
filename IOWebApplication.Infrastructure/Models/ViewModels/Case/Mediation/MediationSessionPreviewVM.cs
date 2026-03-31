// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за медиация
    /// </summary>
    public class MediationSessionPreviewVM
    {
        /// <summary>
        /// Идентификатор на среща
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Описание на дело
        /// </summary>
        public string CaseLabel { get; set; }

        /// <summary>
        /// Вид среща: Информационна среща; Процедура по медиация
        /// </summary>
        [Display(Name = "Вид среща")]
        public string MediationTypeLabel { get; set; }

        /// <summary>
        /// Място на което се провежда
        /// </summary>
        [Display(Name = "Място на което се провежда")]
        public string MediationLocationLabel { get; set; }

        /// <summary>
        /// Допълнително пояснение за място на което се провежда
        /// </summary>
        [Display(Name = "Допълнително пояснение за място на което се провежда")]
        public string MediationLocationDescription { get; set; }

        /// <summary>
        /// Часови интервал на срещата
        /// </summary>
        [Display(Name = "Часови интервал на срещата")]
        public string SessionDateText { get; set; }

        /// <summary>
        /// Статус на срещата
        /// </summary>
        [Display(Name = "Статус на срещата")]
        public string MediationStateLabel { get; set; }

        /// <summary>
        /// Флаг дали може да се редактират данни за среща
        /// </summary>
        public bool IsEdit { get; set; }

        /// <summary>
        /// Медиатори към дело
        /// </summary>
        [Display(Name = "Медиатори")]
        public string Mediators { get; set; }

        /// <summary>
        /// Свързани дела
        /// </summary>
        [Display(Name = "Свързани дела")]
        public string LinkCases { get; set; }
    }
}
