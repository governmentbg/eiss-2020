// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseSecondInstanceReportVM
    {
        public int CaseLifecycleMonths { get; set; }

        public int CaseTypeId { get; set; }

        public int CaseRegNumberValue { get; set; }
        public int CaseCodeId { get; set; }

        [Display(Name = "№ на делото")]
        public string RegNumber { get; set; }

        [Display(Name = "Дата на образуване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime RegDate { get; set; }

        public string OldLinkNumber { get; set; }
        public string MigrationLinkNumber { get; set; }

        [Display(Name = "№ и година на първоинстанционно дело и от кой първоинстанционен съд идва")]
        public string InitialCaseData
        {
            get
            {
                return string.IsNullOrEmpty(OldLinkNumber) == false ? OldLinkNumber : MigrationLinkNumber;
            }
        }

        [Display(Name = "Предмет на делото")]
        public string CaseCodeLabel { get; set; }

        [Display(Name = "Статистически код")]
        public string CaseCodeCode { get; set; }

        [Display(Name = "Жалбоподател")]
        public string CasePersonLeft { get; set; }

        [Display(Name = "Ответник")]
        public string CasePersonRight { get; set; }

        [Display(Name = "Дата на обявяване на делото за решаване")]
        public string SlovingDate { get; set; }

        [Display(Name = "Дата на постановяване на съдебния акт")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FinalAct { get; set; }

        public string ActResultIds { get; set; }

        [Display(Name = "")]
        public string CaseStop { get; set; }

        [Display(Name = "Съдия-докладчик")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "Дата на изпращане на друга инстанция")]
        public string SendOtherInstance { get; set; }

        [Display(Name = "Дата на връщане от друга инстанция")]
        public string ReceiveOtherInstance { get; set; }

        public string ActResultOtherInstanceIds { get; set; }

        [Display(Name = "")]
        public string Interval1M { get { return this.CaseLifecycleMonths == 1 ? "*" : ""; } }

        [Display(Name = "")]
        public string Interval3M { get { return (this.CaseLifecycleMonths > 1 && this.CaseLifecycleMonths <= 3) ? "*" : ""; } }

        [Display(Name = "")]
        public string Interval6M { get { return (this.CaseLifecycleMonths > 3 && this.CaseLifecycleMonths <= 6) ? "*" : ""; } }

        [Display(Name = "")]
        public string Interval1Y { get { return (this.CaseLifecycleMonths > 6 && this.CaseLifecycleMonths <= 12) ? "*" : ""; } }

        [Display(Name = "")]
        public string IntervalMore1Y { get { return this.CaseLifecycleMonths > 12 ? "*" : ""; } }

        [Display(Name = "Дата на връщане на делото")]
        public string CaseReturnDate { get; set; }

        public string AcceptAll { get; set; }
        public string AcceptNotAll { get; set; }
        public string CancelAndNew { get; set; }
        public string CancelAndReturn { get; set; }
        public string MakeNull { get; set; }

        public string OtherInstanceAcceptAll { get; set; }
        public string OtherInstanceAcceptNotAll { get; set; }
        public string OtherInstanceCancelAndNew { get; set; }
        public string OtherInstanceCancelAndReturn { get; set; }
        public string OtherInstanceMakeNull { get; set; }

        [Display(Name = "Дата и час на съдебното заседание")]
        public string SessionDates { get; set; }

        public int DocumentTypeId { get; set; }

        public string Complaint { get; set; }
        public string Protest { get; set; }
        public string AcceptSentence { get; set; }
        public string Applied66 { get; set; }
        public string Cancel66 { get; set; }
        public string SentenceDown { get; set; }
        public string SentenceUp { get; set; }
        public string ChangeCriminalPart { get; set; }
        public string ChangeCivilPart { get; set; }
        public string AppliedNew { get; set; }
        public string ReturnNew { get; set; }
        public string SentenceNew { get; set; }
    }

    public class CaseSecondInstanceFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime DateTo { get; set; }

        [Display(Name = "От номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? FromNumber { get; set; }

        [Display(Name = "До номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? ToNumber { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }
    }
}
