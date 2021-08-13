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
                if (LifeCycleCount > 1)
                    return "Продължено под същия номер";
                else if (migration == null && IsNewCaseNewNumber == false)
                    return "Новообразувано";
                else if (migration?.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction)
                    return "Получено по подсъдност";
                else if (migration?.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptProsecutors)
                    return "Връщане след доразследване";
                else if (IsNewCaseNewNumber == true)
                    return "Върнато за ново разглеждане";

                return "Новообразувано";
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

        public int LifeCycleCount { get; set; }

        public bool IsNewCaseNewNumber { get; set; }
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
