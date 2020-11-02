// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Справка Постъпили дела за период – въззивни дела
    /// </summary>
    public class CaseSecondInstanceListReportVM
    {
        public int CaseId { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeName { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Дата на образуване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CaseRegDate { get; set; }

        [Display(Name = "Съдия докладчик")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "Предмет и шифър")]
        public string CaseCodeName { get; set; }

        [Display(Name = "Източник на постъпване")]
        public string CaseCreateFromName
        {
            get
            {
                if (migration == null)
                    return "Новообразувано";
                else
                {
                    if (migration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction)
                        return "Получено по подсъдност";
                    else if (migration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptProsecutors)
                        return "Връщане след доразследване";
                    else if (migration.ReturnCaseId != this.CaseId)
                        return "Върнато за ново разглеждане";
                    else if (migration.ReturnCaseId == this.CaseId)
                        return "Продължено под същия номер";

                }
                return "";
            }
        }

        public string OldLinkNumber { get; set; }

        public string NewLinkNumber { get; set; }

        [Display(Name = "Първоинстанционен съд")]
        public string FromCourtName 
        { 
            get 
            { 
                return string.IsNullOrEmpty(NewLinkNumber) == false ? NewLinkNumber : OldLinkNumber; 
            } 
        }

        [Display(Name = "Иницииращ документ")]
        public string DocumentTypeName { get; set; }


        public CaseMigrationDataReportVM migration { get; set; }
    }

    /// <summary>
    /// Филтър Справка Постъпили дела за период – въззивни дела
    /// </summary>
    public class CaseSecondInstanceListFilterReportVM
    {
        [Display(Name = "От дата на образуване")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата на образуване")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Шифър")]
        public int CaseCodeId { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Източник на постъпване")]
        public int CaseCreateFromId { get; set; }

        [Display(Name = "Първоинстанционен съд")]
        public int FromCourtId { get; set; }
    }
}
