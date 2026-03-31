using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Справка Постъпили дела за период – въззивни дела
    /// </summary>
    public class CaseSecondInstanceListReportVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        [Display(Name = "Точен вид дело")]
        public string CaseTypeName { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        /// <summary>
        /// Дата на образуване
        /// </summary>
        [Display(Name = "Дата на образуване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CaseRegDate { get; set; }

        /// <summary>
        /// Съдия докладчик
        /// </summary>
        [Display(Name = "Съдия докладчик")]
        public string JudgeReporterName { get; set; }

        /// <summary>
        /// Предмет и шифър
        /// </summary>
        [Display(Name = "Предмет и шифър")]
        public string CaseCodeName { get; set; }

        /// <summary>
        /// Източник на постъпван
        /// </summary>
        [Display(Name = "Източник на постъпване")]
        public string CaseCreateFromName
        {
            get
            {
                if (AcceptedCh80 == true) //Ако има прието такова движение не може да влезе в никое от другите условия
                    return "Постъпили дела по чл. 80, ал.10 ПАС";
                else if (LifeCycleCount > 1 && IsNewCaseNewNumber == false)
                    return "Продължено под същия номер";
                else if (migration == null && IsNewCaseNewNumber == false)
                    return "Новообразувано";
                else if (migration?.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction && IsNewCaseNewNumber == false)
                    return "Получено по подсъдност";
                else if (migration?.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptProsecutors && IsNewCaseNewNumber == false)
                    return "Връщане след доразследване";
                else if (IsNewCaseNewNumber == true)
                    return "Върнато за ново разглеждане";

                return "Новообразувано";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string OldLinkNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string NewLinkNumber { get; set; }

        /// <summary>
        /// Първоинстанционен съд
        /// </summary>
        [Display(Name = "Първоинстанционен съд")]
        public string FromCourtName 
        { 
            get 
            { 
                return string.IsNullOrEmpty(NewLinkNumber) == false ? NewLinkNumber : OldLinkNumber; 
            } 
        }

        /// <summary>
        /// Иницииращ документ
        /// </summary>
        [Display(Name = "Иницииращ документ")]
        public string DocumentTypeName { get; set; }

        /// <summary>
        /// Движения на дело
        /// </summary>
        public CaseMigrationDataReportVM migration { get; set; }

        /// <summary>
        /// Брой интервали
        /// </summary>
        public int LifeCycleCount { get; set; }

        /// <summary>
        /// Флаг дали е дело под нов номер
        /// </summary>
        public bool IsNewCaseNewNumber { get; set; }

        /// <summary>
        /// Дата на свършване на делото - дата до на главен интервал
        /// </summary>
        [Display(Name = "Дата на свършване на делото")]
        public DateTime? DateFinishCase { get; set; }

        /// <summary>
        /// Дата на свършване на делото - дата до на главен интервал - string
        /// </summary>
        [Display(Name = "Дата на свършване на делото")]
        public string DateFinishCaseString
        {
            get
            {
                return DateFinishCase.DateToString();
            }
        }

        /// <summary>
        /// Начална дата на главен интервал който не е затворен след първи интервал
        /// </summary>
        [Display(Name = "Нов интервал")]
        public DateTime? DateFromNewLifeCycle { get; set; }

        /// <summary>
        /// Начална дата на главен интервал който не е затворен след първи интервал
        /// </summary>
        [Display(Name = "Нов интервал")]
        public string DateFromNewLifeCycleString
        {
            get
            {
                return DateFromNewLifeCycle.DateToString();
            }
        }

        public bool AcceptedCh80 { get; set; }
    }

    /// <summary>
    /// Филтър Справка Постъпили дела за период – въззивни дела
    /// </summary>
    public class CaseSecondInstanceListFilterReportVM
    {
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
        public int CaseCodeId { get; set; }

        /// <summary>
        /// Шифър
        /// </summary>
        [Display(Name = "Шифър")]
        public string[] CaseCodeIds { get; set; }

        /// <summary>
        /// Съдия докладчик
        /// </summary>
        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        /// <summary>
        /// Източник на постъпване
        /// </summary>
        [Display(Name = "Източник на постъпване")]
        public int CaseCreateFromId { get; set; }

        /// <summary>
        /// Първоинстанционен съд
        /// </summary>
        [Display(Name = "Първоинстанционен съд")]
        public int FromCourtId { get; set; }

        /// <summary>
        /// От дата на образуване
        /// </summary>
        [Display(Name = "Нов интервал от дата")]
        public DateTime? LifeCycleDateFrom { get; set; }

        /// <summary>
        /// До дата на образуване
        /// </summary>
        [Display(Name = "Нов интервал до дата")]
        public DateTime? LifeCycleDateTo { get; set; }

        /// <summary>
        /// Съдебен състав
        /// </summary>
        [Display(Name = "Съдебен състав")]
        public int? CaseTypeUnitId { get; set; }

        /// <summary>
        /// Тип състав
        /// </summary>
        [Display(Name = "Съдебен състав")]
        public int CourtDepartmentId { get; set; }
    }
}
