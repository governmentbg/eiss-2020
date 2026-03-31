// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за извличане на данни за справка от заявка 13
    /// </summary>
    public class SpecializedReportVM
    {
        /// <summary>
        /// Идентификатор на делото
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Съд, в който е образувано делото
        /// </summary>
        public string CourtLabel { get; set; }

        /// <summary>
        /// Инстанция на делото
        /// </summary>
        public string CaseInstanceLabel { get; set; }

        /// <summary>
        /// Номер на делото
        /// </summary>
        public string RegNumber { get; set; }

        /// <summary>
        /// Дата на образуване
        /// </summary>
        public DateTime RegDate { get; set; }

        /// <summary>
        /// Номер на свързани дела
        /// </summary>
        public string CaseMigrationRegNumber 
        { 
            get
            {
                return CaseMigrationRegNumbers != null ? string.Join(", ", CaseMigrationRegNumbers.Distinct()) : string.Empty;
            } 
        }

        /// <summary>
        /// Списък с номер на свързани дела
        /// </summary>
        public string[] CaseMigrationRegNumbers { get; set; }

        /// <summary>
        /// ЕИСПП номер
        /// </summary>
        public string EisspNumber { get; set; }

        /// <summary>
        /// Основен вид дело
        /// </summary>
        public string CaseGroupLabel { get; set; }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        public string CaseTypeLabel { get; set; }

        /// <summary>
        /// Шифър
        /// </summary>
        public string CaseCodeLabel { get; set; }

        /// <summary>
        /// Индикатори по дело
        /// </summary>
        public string CaseClassificationLabel 
        { 
            get
            {
                return CaseClassifications != null ? string.Join(", ", CaseClassifications) : string.Empty;
            }  
        }

        /// <summary>
        /// Списък с индикатори по дело
        /// </summary>
        public string[] CaseClassifications { get; set; }

        /// <summary>
        /// Статус на дело
        /// </summary>
        public string CaseStateLabel { get; set; }

        /// <summary>
        /// Дата на влизане в сила на акта
        /// </summary>
        public DateTime? ActInforcedDate 
        { 
            get
            {
                return (Acts != null && Acts.Any()) ? Acts.OrderBy(a => a.RegDate)
                                                          .Select(a => a.ActInforcedDate)
                                                          .FirstOrDefault() : null;
            }
        }

        /// <summary>
        /// Резултат/степен на уважаване на иска
        /// </summary>
        public string ActComplainResultLabel 
        { 
            get
            {
                return (Acts != null && Acts.Any()) ? Acts.OrderBy(a => a.RegDate)
                                                          .Select(a => a.ActComplainResultLabel)
                                                          .FirstOrDefault() : null;
            }
        }

        /// <summary>
        /// Актове - финализиращи
        /// </summary>
        public string CaseSessionActs 
        { 
            get
            {
                return (Acts != null && Acts.Any()) ? string.Join("", Acts.OrderBy(a => a.RegDate)
                                                                          .Select(a => "<div class='cdn-listview' style='margin-left:5px;'><a href='#' class='cdn-loader' data-sourceType='" + @SourceTypeSelectVM.CaseSessionActAllFiles + "' data-sourceId='" + a.Id + "'><i class='fa fa-file-text'></i> " + a.ActTypeLabel + " " + (!string.IsNullOrEmpty(a.RegNumber) ? a.RegNumber + "/" + (a.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) + "</a></div>")) : null;
            }
        }

        /// <summary>
        /// Списък с актове - финализиращи
        /// </summary>
        public List<ActDetailsReportVM> Acts { get; set; } = new List<ActDetailsReportVM>();
    }
}
