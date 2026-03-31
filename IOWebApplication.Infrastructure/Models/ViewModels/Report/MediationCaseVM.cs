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
    public class MediationCaseVM
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
        public List<InformationMeeting> InformationMeetings { get; set; } = [];

        /// <summary>
        /// Проведена информационна среща
        /// </summary>
        public string InformationMeetingLabel 
        { 
            get
            {
                return string.Join(";<br />", InformationMeetings.Select(i => i.Label));
            } 
        }

        /// <summary>
        /// Проведена информационна среща - продължителност
        /// </summary>
        public string InformationMeetingDuration
        {
            get
            {
                return string.Join(";<br />", InformationMeetings.Select(i => i.Duration));
            }
        }

        /// <summary>
        /// Проведена информационна среща - място на провеждане
        /// </summary>
        public string InformationMeetingLocation
        {
            get
            {
                return string.Join(";<br />", InformationMeetings.Where(i => !string.IsNullOrEmpty(i.Location)).Select(i => i.Location));
            }
        }

        /// <summary>
        /// Насрочена/пренасрочена информационна среща
        /// </summary>
        public string InformationMeetingScheduled {  get; set; }

        /// <summary>
        /// Процедуара по медиация
        /// </summary>
        public List<MediationProcedureVM> MediationProcedureDatas { get; set; } = [];

        /// <summary>
        /// Процедуара по медиация - Проведена медиация
        /// </summary>
        public string MediationProcedureLabel
        {
            get
            {
                return string.Join(";<br />", MediationProcedureDatas.Select(i => i.MediationProcedureLabel));
            }
        }

        /// <summary>
        /// Процедуара по медиация - Причина за прекратяване
        /// </summary>
        public string MediationProcedureReasonTermination
        {
            get
            {
                return string.Join(";<br />", MediationProcedureDatas.Where(i => !string.IsNullOrEmpty(i.ReasonTermination)).Select(i => i.ReasonTermination));
            }
        }

        /// <summary>
        /// Процедуара по медиация - Място на провеждане на среща
        /// </summary>
        public string MediationProcedureLocation
        {
            get
            {
                return string.Join(";<br />", MediationProcedureDatas.Where(i => !string.IsNullOrEmpty(i.Location)).Select(i => i.Location));
            }
        }

        /// <summary>
        /// Процедуара по медиация - продължителност
        /// </summary>
        public string MediationProcedureDurationText
        {
            get
            {
                int hours = 0;
                int minutes = 0;
                int seconds = 0;
                foreach (var duration in MediationProcedureDatas)
                {
                    TimeSpan timeSpan = (duration.DateTo ?? DateTime.Now).Subtract(duration.DateFrom);
                    hours += timeSpan.Hours;
                    minutes += timeSpan.Minutes;
                    seconds += timeSpan.Seconds;
                }

                TimeSpan returnTimeSpan = new TimeSpan(hours, minutes, seconds);
                return string.Format("{0:00}:{1:00}:{2:00}", returnTimeSpan.Hours, returnTimeSpan.Minutes, returnTimeSpan.Seconds);
            }
        }

        /// <summary>
        /// Проведена информационна среща - причина за непровеждане
        /// </summary>
        public string ReasonNonImplementation { get; set; }

        /// <summary>
        /// Процедуара по медиация - брой срещо
        /// </summary>
        public string MediationProcedureSessionCount 
        { 
            get
            {
                return MediationProcedureDatas.Count.ToString();
            }
        }

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
    /// Данни за проведена информационна среща
    /// </summary>
    public class InformationMeeting
    {
        /// <summary>
        /// Проведена информационна среща - статус
        /// </summary>
        public string StateLabel { get; set; }

        /// <summary>
        /// Проведена информационна среща - от дата
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Проведена информационна среща - от дата
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Проведена информационна среща
        /// </summary>
        public string Label
        {
            get
            {
                return string.IsNullOrEmpty(StateLabel) ? string.Empty : StateLabel + " (" + (DateFrom ?? DateTime.Now).ToString("dd.MM.yyyy HH:mm") + ")";
            }
        }

        /// <summary>
        /// Проведена информационна среща - продължителност
        /// </summary>
        public string Duration
        {
            get
            {
                if (DateFrom == null || DateTo == null)
                    return string.Empty;

                TimeSpan timeSpan = (DateTo ?? DateTime.Now).Subtract(DateFrom ?? DateTime.Now);
                return string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
        }

        /// <summary>
        /// Проведена информационна среща - място на провеждане
        /// </summary>
        public string Location { get; set; }
    }

    /// <summary>
    /// Данни за последната процедуара по медиация
    /// </summary>
    public class MediationProcedureVM
    {
        /// <summary>
        /// Процедуара по медиация
        /// </summary>
        public string MediationProcedureLabel { get; set; }

        /// <summary>
        /// Процедуара по медиация - причина за прекратяване
        /// </summary>
        public string ReasonTermination { get; set; }

        /// <summary>
        /// Процедуара по медиация - място на провеждане
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Проведена информационна среща - от дата
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Проведена информационна среща - от дата
        /// </summary>
        public DateTime? DateTo { get; set; }
    }

    /// <summary>
    /// Модел за филтър за справка медиация
    /// </summary>
    public class MediationCaseFilterVM
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
