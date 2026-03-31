// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за филтър за справка от заявка 13
    /// </summary>
    public class SpecializedReportFilterVM
    {
        /// <summary>
        /// Съд, в който е образувано делото
        /// </summary>
        [Display(Name = "Съд, в който е образувано делото")]
        public int? CourtId { get; set; }

        /// <summary>
        /// Инстанция
        /// </summary>
        [Display(Name = "Инстанция")]
        public int? InstanceId { get; set; }

        /// <summary>
        /// Основен вид дело
        /// </summary>
        [Display(Name = "Основен вид дело")]
        public string[] CaseGroupIds { get; set; }

        /// <summary>
        /// Основен вид дело
        /// </summary>
        public string StringCaseGroupIds
        {
            get
            {
                if ((CaseGroupIds != null) && CaseGroupIds.Length > 0)
                    return string.Join(",", CaseGroupIds);
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        [Display(Name = "Точен вид дело")]
        public string[] CaseTypeIds { get; set; }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        public string StringCaseTypeIds
        {
            get
            {
                if ((CaseTypeIds != null) && CaseTypeIds.Length > 0)
                    return string.Join(",", CaseTypeIds);
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Шифър
        /// </summary>
        [Display(Name = "Шифър")]
        public string[] CaseCodeIds { get; set; }

        /// <summary>
        /// Шифър
        /// </summary>
        public string StringCaseCodeIds
        {
            get
            {
                if ((CaseCodeIds != null) && CaseCodeIds.Length > 0)
                    return string.Join(",", CaseCodeIds);
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// От дата на образуване
        /// </summary>
        [Display(Name = "От дата на образуване")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До дата на образуване
        /// </summary>
        [Display(Name = "До дата на образуване")]
        public DateTime DateTo { get; set; }

        /// <summary>
        /// От дата на влизане в сила на акта
        /// </summary>
        [Display(Name = "От дата на влизане в сила на акта")]
        public DateTime? ActInforcedDateFrom { get; set; }

        /// <summary>
        /// До дата на влизане в сила на акта
        /// </summary>
        [Display(Name = "До дата на влизане в сила на акта")]
        public DateTime? ActInforcedDateTo { get; set; }

        /// <summary>
        /// Статус на делото
        /// </summary>
        [Display(Name = "Статус на делото")]
        public int? CaseStateId { get; set; }

        /// <summary>
        /// Индикатори по дело
        /// </summary>
        [Display(Name = "Индикатори по дело")]
        public string[] CaseClassificationIds { get; set; }

        /// <summary>
        /// Индикатори по дело
        /// </summary>
        public string StringCaseClassificationIds
        {
            get
            {
                if ((CaseClassificationIds != null) && CaseClassificationIds.Length > 0)
                    return string.Join(",", CaseClassificationIds);
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Резултат/степен на уважаване на иска
        /// </summary>
        [Display(Name = "Резултат/степен на уважаване на иска")]
        public int? ActComplainResultId { get; set; }
    }
}
