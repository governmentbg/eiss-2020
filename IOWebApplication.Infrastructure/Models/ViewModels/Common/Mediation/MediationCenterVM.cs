// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Модел за добавяне/рекация на център за медиация
    /// </summary>
    public class MediationCenterVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на родител
        /// </summary>
        [Display(Name = "Горно ниво")]
        public int? ParentId { get; set; }

        /// <summary>
        /// Име на центъра
        /// </summary>
        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Display(Name = "Име")]
        public string Name { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        [Display(Name = "Адрес")]
        public string AddressText { get; set; }

        /// <summary>
        /// Контактни данни
        /// </summary>
        [Display(Name = "Контактни данни")]
        public string ContactDetails { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Display(Name = "Забележка")]
        public string Description { get; set; }

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
        /// Идентификатори на съдилища които се обслужват от текущият център
        /// </summary>
        [Display(Name = "Съдилища")]
        public string[] CourtIds { get; set; }

        /// <summary>
        /// Стринг със съдилища които се обслужват от текущият център, разделени със запетая
        /// </summary>
        public string StringCourtIds
        {
            get
            {
                if ((CourtIds != null) && CourtIds.Length > 0)
                    return string.Join(",", CourtIds);
                else
                    return string.Empty;
            }
        }
    }
}
