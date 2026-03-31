// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за справка необработени документ от ЕИСС
    /// </summary>
    public class RawDocumentsFastProcessVM
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
        /// Дата на регистрация в ЕИСС (дата на регистрация на документа в ЕИСС)
        /// </summary>
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Вид документ
        /// </summary>
        public string DocumentTypeLabel { get; set; }

        /// <summary>
        /// Шифър
        /// </summary>
        public string DocumentCodeLabel { get; set; }
    }

    /// <summary>
    /// Модел за филтър за справка необработени документ от ЕИСС
    /// </summary>
    public class RawDocumentsFilterFastProcessVM
    {
        /// <summary>
        /// Вх. номер (входящ номер от единия регистър на заповедни производства)
        /// </summary>
        [Display(Name = "Вх. номер")]
        public string DocumentRegNum { get; set; }

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
    }
}
