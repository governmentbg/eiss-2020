// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseFirstInstanceReportVM
    {
        public int CaseLifecycleMonths { get; set; }

        public int CaseTypeId { get; set; }

        [Display(Name = "№ на делото")]
        public string RegNumber { get; set; }

        [Display(Name = "Източник на постъпване (новообразувано, по подсъдност, върнато за ново разглеждане)")]
        public string InputDocument { get; set; }

        [Display(Name = "Дата на образуване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime RegDate { get; set; }
        
        [Display(Name = "Дата и час на съдебното заседание")]
        public string SessionDates { get; set; }

        public string CaseInstitution { get; set; }

        [Display(Name = "Източник на постъпване (новообразувано, по подсъдност, върнато за ново разглеждане)")]
        public string InputDocumentCriminal { get { return this.InputDocument + " - " + CaseTypeMigr + Environment.NewLine + this.CaseInstitution; } }

        [Display(Name = "Съдия-докладчик")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "Предмет на делото")]
        public string CaseCodeLabel { get; set; }

        [Display(Name = "Статистически код")]
        public string CaseCodeCode { get; set; }

        public int CaseCodeId { get; set; }

        public string CasePersonLeft { get; set; }
        public string CasePersonRight { get; set; }

        [Display(Name = "Страни")]
        public string CasePersonString 
        {
            get 
            {
                if (string.IsNullOrEmpty(CasePersonRight) == false)
                    return CasePersonLeft + Environment.NewLine + "срещу" + Environment.NewLine + CasePersonRight;
                else
                    return CasePersonLeft;
            }
        }

        [Display(Name = "Дата на обявяване на делото за решаване")]
        public string SlovingDate { get; set; }

        [Display(Name = "Дата на постановяване на съдебния акт")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FinalAct { get; set; }

        [Display(Name = "Резултат от делото")]
        public string Result { get; set; }

        [Display(Name = "е изпратено на др. инстанция ")]
        public string SendOtherInstance { get; set; }

        [Display(Name = "получено от друга инстанция")]
        public string ReceiveOtherInstance { get; set; }

        [Display(Name = "Резултат от инстанционната проверка и новия номер на делото, ако е върнато за ново разглеждане")]
        public string ResultOtherInstance { get; set; }

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

        [Display(Name = "Дата на предаване в архив")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateArch { get; set; }

        [Display(Name = "Номер на архивно дело")]
        public string NumArch { get; set; }

        [Display(Name = "Номер на архивна връзка")]
        public string NumLinkArch { get; set; }

        public int CaseRegNumberValue { get; set; }

        public bool IsNewNumber { get; set; }

        public bool AcceptJurisdiction { get; set; }
        public bool AcceptDiffJurisdiction { get; set; }

        [Display(Name = "Източник на постъпване (новообразувано, по подсъдност, върнато за ново разглеждане)")]
        public string InputDocumentText
        {
            get
            {
                return InputDocument + " - " + CaseTypeMigr;
            }
        }

        public string CaseTypeMigr
        {
            get
            {
                string result = "";
                if (IsNewNumber)
                    result = "върнато за ново разглеждане под нов номер";
                else if (AcceptJurisdiction)
                    result = "по подсъдност";
                else if (AcceptDiffJurisdiction)
                    result = "върнато за ново разглеждане";
                else
                    result = "новообразувано";

                return result;
            }
        }
    }

    public class CaseFirstInstanceFilterReportVM
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
