// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Модел за добавяне/рекация на медиатор
    /// </summary>
    public class MediationMediatorVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на центъра
        /// </summary>
        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Display(Name = "Име")]
        public string Name { get; set; }

        /// <summary>
        /// Допълнителна квалификация
        /// </summary>
        [Display(Name = "Допълнителна квалификация")]
        public string AdditionalQualification { get; set; }

        /// <summary>
        /// Основна професия
        /// </summary>
        [Display(Name = "Основна професия")]
        public string MainProfession { get; set; }

        /// <summary>
        /// Практика в определена област на правото
        /// </summary>
        [Display(Name = "Професионален опит")]
        public int? PracticeSpecificAreaLawYear { get; set; }

        /// <summary>
        /// Опит на медиатора в медиация по опредени видове спорове
        /// </summary>
        [Display(Name = "Опит в медиацията")]
        public string ExperienceMediation { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        /// <summary>
        /// Образование
        /// </summary>
        [Display(Name = "Образование")]
        public string Education { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Дата на вписване
        /// </summary>
        [Display(Name = "Дата на вписване")]
        public DateTime? DateEntry { get; set; }

        /// <summary>
        /// Идентификатори на центрове
        /// </summary>
        [Display(Name = "Центрове")]
        public string[] CenterIds { get; set; }

        /// <summary>
        /// Стринг с идентификатори на центрове, разделени със запетая
        /// </summary>
        public string StringCenterIds
        {
            get
            {
                if ((CenterIds != null) && CenterIds.Length > 0)
                    return string.Join(",", CenterIds);
                else
                    return string.Empty;
            }
        }
    }
}
