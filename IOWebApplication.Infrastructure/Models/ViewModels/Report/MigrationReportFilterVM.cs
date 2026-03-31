// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за филтър на данни за движение на делата
    /// </summary>
    public class MigrationReportFilterVM
    {
        /// <summary>
        /// От дата на изпращане - дата на документ
        /// </summary>
        [Display(Name = "От дата на изпращащо движение")]
        public DateTime? DocumentRegDateFrom { get; set; }

        /// <summary>
        /// До дата на изпращане - дата на документ
        /// </summary>
        [Display(Name = "До дата на изпращащо движение")]
        public DateTime? DocumentRegDateTo { get; set; }

        /// <summary>
        /// От дата на връщане от насрещния съд - дата на връщащото движение
        /// </summary>
        [Display(Name = "От дата на връщащо движение")]
        public DateTime? MigrationDateWrtFrom { get; set; }

        /// <summary>
        /// До дата на връщане от насрещния съд - дата на връщащото движение
        /// </summary>
        [Display(Name = "До дата на връщащо движение")]
        public DateTime? MigrationDateWrtTo { get; set; }

        /// <summary>
        /// Вид движение
        /// </summary>
        [Display(Name = "Вид движение")]
        public int? MigrationTypeId { get; set; }

        /// <summary>
        /// Насрещен съд
        /// </summary>
        [Display(Name = "Насрещен съд")]
        public int? SendToCourtId { get; set; }
    }
}
