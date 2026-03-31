// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за справка изпълнителни листове
    /// </summary>
    public class ExecListFatsProcessVM
    {
        /// <summary>
        /// Идентификатор на изпълнителен лист
        /// </summary>
        public int ExecListId { get; set; }

        /// <summary>
        /// Номер на изпълнителен лист
        /// </summary>
        public string ExecListRegNumber { get; set; }

        /// <summary>
        /// Дата на подписване
        /// </summary>
        public DateTime? ExecListDateSign { get; set; }

        /// <summary>
        /// Вид изпълнителен лист
        /// </summary>
        public string ExecListTypeLabel { get; set; }

        /// <summary>
        /// Съд в който е издаден
        /// </summary>
        public string ExecListCourtLabel { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string CaseRegNum { get; set; }
    }

    /// <summary>
    /// Модел за филтър за справка изпълнителни листове
    /// </summary>
    public class ExecListFilterFatsProcessVM
    {
        /// <summary>
        /// Номер на изпълнителен лист
        /// </summary>
        [Display(Name = "Номер на изпълнителен лист")]
        public string ExecListRegNumber { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        /// <summary>
        /// Съд
        /// </summary>
        [Display(Name = "Съд в който е издаден")]
        public int? CourtId { get; set; }

        /// <summary>
        /// От дата на подписване
        /// </summary>
        [Display(Name = "От дата на подписване")]
        public DateTime? ExecListSignDateFrom { get; set; }

        /// <summary>
        /// До дата на подписване
        /// </summary>
        [Display(Name = "До дата на подписване")]
        public DateTime? ExecListSignDateTo { get; set; }
    }
}
