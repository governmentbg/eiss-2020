// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за филтър за справка документи, постъпили чрез ЕЕСПП
    /// </summary>
    public class DocumentsReceivedEESPPFilterVM
    {
        /// <summary>
        /// Дата на постъпване на уведомителното писмо - от дата
        /// </summary>
        [Display(Name = "От дата на постъпване")]
        public DateTime? DateReturnedFrom { get; set; }

        /// <summary>
        /// Дата на постъпване на уведомителното писмо - до дата
        /// </summary>
        [Display(Name = "До дата на постъпване")]
        public DateTime? DateReturnedTo { get; set; }
    }
}
