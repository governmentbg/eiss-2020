// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за справка разпределение на дела - бързо производство
    /// </summary>
    public class CaseSelectionProtokolFastProcessVM
    {
        /// <summary>
        /// Идентификатор на протокол
        /// </summary>
        public int Id { get; set; }

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
        /// Дата (дата на входиране на иницииращия документ в съда)
        /// </summary>
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Разпределен (разпределен съдия-докладчик)
        /// </summary>
        public string JudgeReporterFullName { get; set; }

        /// <summary>
        /// Начин на разпределение (начин на разпределение/преразпределение автоматично/ръчно)
        /// </summary>
        public string SelectionModeLabel { get; set; }

        /// <summary>
        /// Съд (съд, в който е разпределено делото)
        /// </summary>
        public string CaseCourtLabel { get; set; }

        /// <summary>
        /// Номер на дело (номер на образуваното дело)
        /// </summary>
        public string CaseRegNum { get; set; }

        /// <summary>
        /// Дата на образувано на дело (дата на образуване на дело)
        /// </summary>
        public DateTime CaseRegDate { get; set; }

        /// <summary>
        /// Вх. номер от ЕПЕП
        /// </summary>
        public string DocumentRegNumEpep { get; set; }

        /// <summary>
        /// Вх. номер (входящ номер от единия регистър на заповедни производства)
        /// </summary>
        public string DocumentRegNum { get; set; }

        /// <summary>
        /// Статус на дело (статус на делото към момента на изготвяне на справката)
        /// </summary>
        public string CaseStateName { get; set; }

        /// <summary>
        /// Дата на разпределение
        /// </summary>
        public DateTime SelectionDate { get; set; }
    }

    /// <summary>
    /// Филтър за справка разпределение на дела - бързо производство
    /// </summary>
    public class CaseSelectionProtokolFilterFastProcessVM
    {
        /// <summary>
        /// От дата на разпределение
        /// </summary>
        [Display(Name = "От дата на разпределение")]
        public DateTime? DistributionDateFrom { get; set; }

        /// <summary>
        /// До дата на разпределение
        /// </summary>
        [Display(Name = "До дата на разпределение")]
        public DateTime? DistributionDateTo { get; set; }

        /// <summary>
        /// Година на разпределение
        /// </summary>
        [Display(Name = "Година на разпределение")]
        public int? DistributionYear { get; set; }

        /// <summary>
        /// Начин на разпределение
        /// </summary>
        [Display(Name = "Начин на разпределение")]
        public int? SelectionModeId { get; set; }

        /// <summary>
        /// Име на разпределен
        /// </summary>
        [Display(Name = "Име на разпределен")]
        public string JudgeReporterFullName { get; set; }

        /// <summary>
        /// Основен вид дело
        /// </summary>
        [Display(Name = "Основен вид дело")]
        public string CaseGroupIds { get; set; }
        public string CaseGroupIds_text { get; set; }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        [Display(Name = "Точен вид дело")]
        public string CaseTypeIds { get; set; }
        public string CaseTypeIds_text { get; set; }

        /// <summary>
        /// Шифър
        /// </summary>
        [Display(Name = "Шифър")]
        public string[] CaseCodeIds { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }

        /// <summary>
        /// Вх. номер (входящ номер от единия регистър на заповедни производства)
        /// </summary>
        [Display(Name = "Вх. номер")]
        public string FastProcessRegNumber { get; set; }

        /// <summary>
        /// Съд
        /// </summary>
        [Display(Name = "Съд")]
        public int? CourtId { get; set; }

        /// <summary>
        /// Вх. номер – входящ номер от ЕПЕП
        /// </summary>
        [Display(Name = "Вх. номер ЕПЕП")]
        public string RegNumberEpep { get; set; }
    }
}
