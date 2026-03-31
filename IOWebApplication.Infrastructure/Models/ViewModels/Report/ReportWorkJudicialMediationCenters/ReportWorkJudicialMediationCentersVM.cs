// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report.ReportWorkJudicialMediationCenters
{
    /// <summary>
    /// Модел за отчет за работата на съдебните центрове по медиация
    /// </summary>
    public class ReportWorkJudicialMediationCentersVM
    {
        /// <summary>
        /// Име на медиатор
        /// </summary>
        public string MediatorName { get; set; }

        /// <summary>
        /// Име на център за медиация
        /// </summary>
        public string MediationCenterName { get; set; }

        /// <summary>
        /// Брой проведена информационна среща
        /// </summary>
        public int InformationMeetingsCount { get; set; }

        /// <summary>
        /// Брой проведена информационна среща при които страните са избрали медиатора
        /// </summary>
        public int InformationMeetingsElectedPartyCaseCount { get; set; }

        /// <summary>
        /// Информационна среща - продължителност
        /// </summary>
        public string InformationMeetingDurationText { get; set; }

        /// <summary>
        /// Брой процедуара по медиация с резултат от срещата  "Прекратена процедура"
        /// </summary>
        public int MediationProceduresERTPCount { get; set; }

        /// <summary>
        /// Брой процедуара по медиация с резултат в делото от срещата "Прекратена процедура"
        /// </summary>
        public int MediationProceduresECRTPCount { get; set; }

        /// <summary>
        /// Брой процедуара по медиация с резултат в делото от срещата "Прекратена процедура" - продължителност
        /// </summary>
        public string MediationProceduresECRTPDurationText { get; set; }

        /// <summary>
        /// Брой процедуара по медиация при които страните са избрали медиатора
        /// </summary>
        public int MediationProceduresElectedPartyCaseCount { get; set; }

        /// <summary>
        /// Обща удовлетвореност на страните от проведени процедури по медиация
        /// </summary>
        public string Appraisals {  get; set; }

        /// <summary>
        /// Прекратяване на дело със спогодба
        /// </summary>
        public int TerminationCaseSettlementCount { get; set; }

        /// <summary>
        /// Прекратяване на дело поради оттегляне или отказ
        /// </summary>
        public int TerminationCaseTerminationWithdrawalRefusalCount { get; set; }

        /// <summary>
        /// Общ брой прекратени дела
        /// </summary>
        public int TerminationCaseCount 
        { 
            get
            {
                return TerminationCaseSettlementCount + TerminationCaseTerminationWithdrawalRefusalCount;
            }
        }

        /// <summary>
        /// Частично прекратяване на дело със спогодба
        /// </summary>
        public int PartiallyTerminationCaseSettlementCount { get; set; }

        /// <summary>
        /// Частично прекратяване на дело поради оттегляне или отказ
        /// </summary>
        public int PartiallyTerminationCaseTerminationWithdrawalRefusalCount { get; set; }

        /// <summary>
        /// Общ брой частично прекратени дела
        /// </summary>
        public int PartiallyTerminationCaseCount
        {
            get
            {
                return PartiallyTerminationCaseSettlementCount + PartiallyTerminationCaseTerminationWithdrawalRefusalCount;
            }
        }
    }

    /// <summary>
    /// Модел за филтър за отчет за работата на съдебните центрове по медиация 
    /// </summary>
    public class ReportWorkJudicialMediationCentersFilterVM
    {
        /// <summary>
        /// Флаг който показва дали да се търси само по съдилища на кординатора
        /// </summary>
        public bool IsFindCoordinatorCourt { get; set; } = false;

        /// <summary>
        /// Съд
        /// </summary>
        [Display(Name = "Съд")]
        public int? CourtId { get; set; }

        /// <summary>
        /// От дата на среща
        /// </summary>
        [Display(Name = "От дата на среща")]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// До дата на среща
        /// </summary>
        [Display(Name = "До дата на среща")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Медиатор
        /// </summary>
        [Display(Name = "Медиатор")]
        public int? MediatorId { get; set; }

        /// <summary>
        /// Основен вид
        /// </summary>
        [Display(Name = "Основен вид")]
        public int CaseGroupId { get; set; }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        /// <summary>
        /// Шифър
        /// </summary>
        [Display(Name = "Шифър")]
        public string[] CaseCodeIds { get; set; }

        /// <summary>
        /// Подшифър
        /// </summary>
        [Display(Name = "Подшифър")]
        public int? CaseCodeSubId { get; set; }

        /// <summary>
        /// Тип справка: 1 - Отчет за работата на съдебните центрове по медиация / 2 - Справка за дейността на медиаторите към СЦМ по делата
        /// </summary>
        public int TypeReport { get; set; } = 1;
    }
}
