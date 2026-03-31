// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за справка за регистрираните документи за определен период 
    /// </summary>
    public class RegisteredDocumentsFastProcessVM
    {
        /// <summary>
        /// Идентификатор на документ
        /// </summary>
        public long DocumentId { get; set; }

        /// <summary>
        /// Вх. номер (входящ номер от единия регистър на заповедни производства)
        /// </summary>
        public string DocumentRegNum { get; set; }

        /// <summary>
        /// Флаг дали е от един и същ съд с на потребителя
        /// </summary>
        public bool IsLinkDocument { get; set; }

        /// <summary>
        /// Начин на подаване (начин на подаване на документа)
        /// </summary>
        public string DocumentDeliveryGroupLabel { get; set; }

        /// <summary>
        /// Дата на регистрация в ЕИСС (дата на регистрация на документа в ЕИСС)
        /// </summary>
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Вх. номер от ЕПЕП
        /// </summary>
        public string DocumentRegNumEpep { get; set; }

        /// <summary>
        /// Дата на регистрация в ЕПЕП (дата на регистрация на документа в ЕПЕП)
        /// </summary>
        public DateTime? DocumentDateEpep { get; set; }

        /// <summary>
        /// Съд (съд, в който е входиран документа)
        /// </summary>
        public string DocumentCourtLabel { get; set; }
    }

    /// <summary>
    /// Модел за филтър справка за регистрираните документи за определен период 
    /// </summary>
    public class RegisteredDocumentsFilterFastProcessVM
    {
        /// <summary>
        /// Вх. номер (входящ номер от единия регистър на заповедни производства)
        /// </summary>
        [Display(Name = "Вх. номер")]
        public string DocumentRegNum { get; set; }

        /// <summary>
        /// Съд
        /// </summary>
        [Display(Name = "Съд")]
        public int? DocumentCourtId { get; set; }

        /// <summary>
        /// Начин на подаване (начин на подаване на документа)
        /// </summary>
        [Display(Name = "Начин на подаване")]
        public int? DocumentDeliveryGroupId { get; set; }

        /// <summary>
        /// От дата на регистрация в ЕИСС (дата на регистрация на документа в ЕИСС)
        /// </summary>
        [Display(Name = "От дата на регистрация")]
        public DateTime? DocumentDateFrom { get; set; }

        /// <summary>
        /// До дата на регистрация в ЕИСС (дата на регистрация на документа в ЕИСС)
        /// </summary>
        [Display(Name = "До дата на регистрация")]
        public DateTime? DocumentDateTo { get; set; }

        /// <summary>
        /// Вх. номер от ЕПЕП
        /// </summary>
        [Display(Name = "Вх. номер от ЕПЕП")]
        public string DocumentRegNumEpep { get; set; }

        /// <summary>
        /// От дата на регистрация в ЕПЕП (дата на регистрация на документа в ЕПЕП)
        /// </summary>
        [Display(Name = "От дата на регистрация в ЕПЕП")]
        public DateTime? DocumentDateEpepFrom { get; set; }

        /// <summary>
        /// До дата на регистрация в ЕПЕП (дата на регистрация на документа в ЕПЕП)
        /// </summary>
        [Display(Name = "До дата на регистрация в ЕПЕП")]
        public DateTime? DocumentDateEpepTo { get; set; }
    }
}
