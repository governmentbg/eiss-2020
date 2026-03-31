// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Migrations;
using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за справка актове за бързо производство
    /// </summary>
    public class CaseSessionActFastProcessVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Име на съд
        /// </summary>
        public string CaseCourtLabel { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string CaseRegNum { get; set; }

        /// <summary>
        /// Дата на дело
        /// </summary>
        public DateTime CaseRegDate { get; set; }

        /// <summary>
        /// Шифър на дело
        /// </summary>
        public string CaseCode { get; set; }

        /// <summary>
        /// Номер на акт
        /// </summary>
        public string ActRegNum { get; set; }

        /// <summary>
        /// Дата на акт
        /// </summary>
        public DateTime ActRegDate { get; set; }

        /// <summary>
        /// Дата на обявяване на акта: подписване от последния съдия
        /// </summary>
        public DateTime? ActDeclarDate { get; set; }

        /// <summary>
        /// Тип на акта
        /// </summary>
        public string ActTypeLabel { get; set; }
    }

    /// <summary>
    /// Модел за филтър за справка актове за бързо производство
    /// </summary>
    public class CaseSessionActFastProcessFilterVM
    {
        /// <summary>
        /// От дата на акта
        /// </summary>
        [Display(Name = "От дата на акта")]
        public DateTime? ActDateFrom { get; set; }

        /// <summary>
        /// До дата на акта
        /// </summary>
        [Display(Name = "До дата на акта")]
        public DateTime? ActDateTo { get; set; }

        /// <summary>
        /// Вид акт
        /// </summary>
        [Display(Name = "Вид акт")]
        public int? ActTypeId { get; set; }
    }
}
