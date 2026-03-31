// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за визуализация на данни за движение на делата
    /// </summary>
    public class MigrationReportVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Дело номер
        /// </summary>
        public string CaseInfo { get; set; }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        public string CaseTypeLabel { get; set; }

        /// <summary>
        /// Дата на изпращане - дата на документ
        /// </summary>
        public DateTime? DocumentRegDate { get; set; }

        /// <summary>
        /// Дата на връщане от насрещния съд - дата на връщащото движение
        /// </summary>
        public DateTime? MigrationDateWrt { get; set; }

        /// <summary>
        /// Вид движение
        /// </summary>
        public string MigrationTypeLabel { get; set; }

        /// <summary>
        /// Насрещен съд
        /// </summary>
        public string SendToCourtLabel { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
    }
}
