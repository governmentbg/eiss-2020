// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за данни за справка документи, постъпили чрез ЕЕСПП
    /// </summary>
    public class DocumentsReceivedEESPPReportVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Дело номер
        /// </summary>
        public string CaseNumber { get; set; }

        /// <summary>
        /// Дата на постъпване на уведомителното писмо
        /// </summary>
        public DateTime? DateReturned { get; set; }

        /// <summary>
        /// Лица, за които се иска правна помощ
        /// </summary>
        public string Persons { get; set; }
        public string Person { get; set; }

        /// <summary>
        /// Вид правна помощ
        /// </summary>
        public string LawyerHelpTypeLabel { get; set; }

        /// <summary>
        /// Определен адвокат
        /// </summary>
        public string LawyerInfo { get; set; }
    }
}
