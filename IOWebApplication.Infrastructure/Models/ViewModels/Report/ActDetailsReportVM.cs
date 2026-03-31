// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за детайли по акт
    /// </summary>
    public class ActDetailsReportVM
    {
        /// <summary>
        /// Идентификатор на акта
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата на влизане в сила на акта
        /// </summary>
        public DateTime? ActInforcedDate { get; set; }

        /// <summary>
        /// Резултат/степен на уважаване на иска
        /// </summary>
        public string ActComplainResultLabel { get; set; }

        /// <summary>
        /// Номер на акт
        /// </summary>
        public string RegNumber { get; set; }

        /// <summary>
        /// Дата на акт
        /// </summary>
        public DateTime? RegDate { get; set; }

        /// <summary>
        /// Вид на документ
        /// </summary>
        public string ActTypeLabel { get; set; }
    }
}
