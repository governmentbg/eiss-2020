// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за справка на заповедните производства
    /// </summary>
    public class CaseFastProcessVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Флаг дали е от един и същ съд с на потребителя
        /// </summary>
        public bool IsLinkCase { get; set; }

        /// <summary>
        /// Шифър на дело
        /// </summary>
        public string CaseCode { get; set; }

        /// <summary>
        /// Вх. номер (входящ номер от единия регистър на заповедни производства)
        /// </summary>
        public string DocumentRegNum { get; set; }

        /// <summary>
        /// Вх. номер - стойност (входящ номер от единия регистър на заповедни производства)
        /// </summary>
        public int? DocumentRegNumValue { get; set; }

        /// <summary>
        /// Номер на дело (номер на образуваното дело)
        /// </summary>
        public string CaseRegNum { get; set; }

        /// <summary>
        /// Съд (съд, в който е образувано делото)
        /// </summary>
        public string CaseCourtLabel { get; set; }

        /// <summary>
        /// Вносител (имената на лице с качество вносител в документа)
        /// </summary>
        public string ImporterPersonFullName { get; set; }

        /// <summary>
        /// Вх. номер от ЕПЕП
        /// </summary>
        public string DocumentRegNumEpep { get; set; }

        /// <summary>
        /// Статус на дело (статус на делото към момента на изготвяне на справката)
        /// </summary>
        public string CaseStateName { get; set; }
    }

    /// <summary>
    /// Модел за филтър за справка на заповедните производства
    /// </summary>
    public class CaseFilterFastProcessVM
    {
        /// <summary>
        /// Вх. номер (входящ номер от единия регистър на заповедни производства)
        /// </summary>
        [Display(Name = "Вх. номер")]
        public string FastProcessRegNumber { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }

        /// <summary>
        /// Съд
        /// </summary>
        [Display(Name = "Съд")]
        public int? CourtId { get; set; }

        /// <summary>
        /// От дата на регистрация (дело)
        /// </summary>
        [Display(Name = "От дата на регистрация (дело)")]
        public DateTime? CaseRegDateFrom { get; set; }

        /// <summary>
        /// До дата на регистрация (дело)
        /// </summary>
        [Display(Name = "До дата на регистрация (дело)")]
        public DateTime? CaseRegDateTo { get; set; }

        /// <summary>
        /// От дата на регистрация (дело)
        /// </summary>
        [Display(Name = "От дата на регистрация (ЕПЕП)")]
        public DateTime? EpepDocumentRegDateFrom { get; set; }

        /// <summary>
        /// До дата на регистрация (дело)
        /// </summary>
        [Display(Name = "До дата на регистрация (ЕПЕП)")]
        public DateTime? EpepDocumentRegDateTo { get; set; }

        /// <summary>
        /// Вносител (имената на лице с качество вносител в документа)
        /// </summary>
        [Display(Name = "Вносител")]
        public string ImporterPersonName { get; set; }

        /// <summary>
        /// Вх. номер от ЕПЕП
        /// </summary>
        [Display(Name = "Вх. номер от ЕПЕП")]
        public string DocumentRegNumEpep { get; set; }

        /// <summary>
        /// Статус на дело (статус на делото към момента на изготвяне на справката)
        /// </summary>
        [Display(Name = "Статус на дело")]
        public int? CaseStateId { get; set; }
    }
}
