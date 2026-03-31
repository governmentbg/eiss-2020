// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за справка медиация
    /// </summary>
    public class MediationCaseSessionVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Име на съд
        /// </summary>
        public string CourtLabel { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string CaseRegNumber { get; set; }

        /// <summary>
        /// Предмет и шифър
        /// </summary>
        public string CaseCodeName { get; set; }

        /// <summary>
        /// Подшифър
        /// </summary>
        public string CaseCodeSubName { get; set; }

        /// <summary>
        /// Проведена информационна среща
        /// </summary>
        public string InformationMeetingLabel { get; set; }

        /// <summary>
        /// Проведена информационна среща - място на провеждане
        /// </summary>
        public string InformationMeetingLocation { get; set; }

        /// <summary>
        /// Насрочена/пренасрочена информационна среща
        /// </summary>
        public string InformationMeetingScheduled {  get; set; }

        /// <summary>
        /// Проведена информационна среща - от дата
        /// </summary>
        public DateTime? InformationMeetingDateFrom { get; set; }

        /// <summary>
        /// Проведена информационна среща - от дата
        /// </summary>
        public DateTime? InformationMeetingDateTo { get; set; }

        /// <summary>
        /// Проведена информационна среща
        /// </summary>
        public string InformationMeetingTextLabel
        {
            get
            {
                return string.IsNullOrEmpty(InformationMeetingLabel) ? string.Empty : InformationMeetingLabel + " (" + (InformationMeetingDateFrom ?? DateTime.Now).ToString("dd.MM.yyyy HH:mm") + ")";
            }
        }

        /// <summary>
        /// Проведена информационна среща - продължителност
        /// </summary>
        public string InformationMeetingDuration
        {
            get
            {
                if (InformationMeetingDateFrom == null || InformationMeetingDateTo == null)
                    return string.Empty;

                TimeSpan timeSpan = (InformationMeetingDateTo ?? DateTime.Now).Subtract(InformationMeetingDateFrom ?? DateTime.Now);
                return string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
        }


        /// <summary>
        /// Процедуара по медиация - Проведена медиация
        /// </summary>
        public string MediationProcedureLabel { get; set; }

        /// <summary>
        /// Процедуара по медиация - причина за прекратяване
        /// </summary>
        public string MediationProcedureReasonTermination { get; set; }

        /// <summary>
        /// Процедуара по медиация - място на провеждане
        /// </summary>
        public string MediationProcedureLocation { get; set; }

        /// <summary>
        /// Проведена информационна среща - от дата
        /// </summary>
        public DateTime? MediationProcedureDateFrom { get; set; }

        /// <summary>
        /// Проведена информационна среща - от дата
        /// </summary>
        public DateTime? MediationProcedureDateTo { get; set; }

        /// <summary>
        /// Процедуара по медиация - продължителност
        /// </summary>
        public string MediationProcedureDurationText
        {
            get
            {
                if (MediationProcedureDateFrom == null || MediationProcedureDateTo == null)
                    return string.Empty;

                TimeSpan timeSpan = (MediationProcedureDateTo ?? DateTime.Now).Subtract(MediationProcedureDateFrom ?? DateTime.Now);
                return string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
        }

        /// <summary>
        /// Проведена информационна среща - причина за непровеждане
        /// </summary>
        public string ReasonNonImplementation { get; set; }

        /// <summary>
        /// Процедуара по медиация - брой срещо
        /// </summary>
        public string MediationProcedureSessionCount { get; set; }

        /// <summary>
        /// Насрочена/пренасрочена процедуара по медиация
        /// </summary>
        public string MediationProcedureScheduled { get; set; }

        /// <summary>
        /// Процедуара по медиация - сключено споразумение
        /// </summary>
        public string ConcludedAgreement { get; set; }

        /// <summary>
        /// Прекратяване на дело със спогодба
        /// </summary>
        public string TerminationCaseSettlement { get; set; }

        /// <summary>
        /// Прекратяване на дело поради оттегляне или отказ
        /// </summary>
        public string TerminationCaseTerminationWithdrawalRefusal { get; set; }

        /// <summary>
        /// Частично прекратяване на дело със спогодба
        /// </summary>
        public string PartiallyTerminationCaseSettlement { get; set; }

        /// <summary>
        /// Частично прекратяване на дело поради оттегляне или отказ
        /// </summary>
        public string PartiallyTerminationCaseTerminationWithdrawalRefusal { get; set; }

        /// <summary>
        /// Медиатори
        /// </summary>
        public string CaseMediators { get; set; }

        /// <summary>
        /// Съдия - докладчик от заседанието в което има отбелязан резултат Препращане за провеждане на информационна среща по медиация
        /// </summary>
        public string JudgeReporterName { get; set; }
    }

    /// <summary>
    /// Модел за филтър за справка медиация
    /// </summary>
    public class MediationCaseSessionFilterVM
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
        /// От дата на образуване
        /// </summary>
        [Display(Name = "От дата на образуване")]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// До дата на образуване
        /// </summary>
        [Display(Name = "До дата на образуване")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// От дата на среща
        /// </summary>
        [Display(Name = "От дата на среща")]
        public DateTime? SessionDateFrom { get; set; }

        /// <summary>
        /// До дата на среща
        /// </summary>
        [Display(Name = "До дата на среща")]
        public DateTime? SessionDateTo { get; set; }

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
        /// Съдия докладчик
        /// </summary>
        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        /// <summary>
        /// Медиатор
        /// </summary>
        [Display(Name = "Медиатор")]
        public int? MediatorId { get; set; }

        /// <summary>
        /// Идентификатор на вид процедура по медиация
        /// </summary>
        [Display(Name = "Процедура по медиация")]
        public int? MediationProcedureId { get; set; }
    }
}
