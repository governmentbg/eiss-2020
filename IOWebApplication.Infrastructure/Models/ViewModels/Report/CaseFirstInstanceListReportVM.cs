using IOWebApplication.Infrastructure.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Справка постъпили дела за период – първоинстанционни дела
    /// </summary>
    public class CaseFirstInstanceListReportVM
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
        /// Източник на постъпване
        /// </summary>
        [Display(Name = "Източник на постъпване")]
        public string CaseCreateFromName
        {
            get
            {
                if (IsJurisdiction == true && IsNewCaseNewNumber == false)
                    return "Получено по подсъдност";
                else if (LifeCycleCount > 1 && IsNewCaseNewNumber == false)
                    return "Продължено под същия номер";
                else if (migration == null && IsNewCaseNewNumber == false)
                    return "Новообразувано";
                else if (IsNewCaseNewNumber == true)
                    return "Върнато за ново разглеждане";

                return "Новообразувано";
            }
        }

        /// <summary>
        /// Движение на дело
        /// </summary>
        public CaseMigrationDataReportVM migration { get; set; }

        /// <summary>
        /// Брой интервали
        /// </summary>
        public int LifeCycleCount { get; set; }

        /// <summary>
        /// Флаг за приемане в равен по степен съд по подсъдност
        /// </summary>
        public bool IsJurisdiction { get; set; }

        /// <summary>
        /// Флаг за върнато за ново разглеждане под нов номер
        /// </summary>
        public bool IsNewCaseNewNumber { get; set; }

        /// <summary>
        /// Флаг дали има отворен главен интервал
        /// </summary>
        public bool HaveOpenLifeCycle { get; set; }

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
    }

    /// <summary>
    /// Филтър за Справка Постъпили дела за период – първоинстанционни дела
    /// </summary>
    public class CaseFirstInstanceListFilterReportVM
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
        /// Вид Иницииращ документ
        /// </summary>
        [Display(Name = "Иницииращ документ")]
        public int DocumentTypeId { get; set; }

        /// <summary>
        /// Съдебен състав
        /// </summary>
        [Display(Name = "Съдебен състав")]
        public int? CaseTypeUnitId { get; set; }

        /// <summary>
        /// Вид производство
        /// </summary>
        [Display(Name = "Вид производство")]
        public int? ProcessPriorityId { get; set; }

        /// <summary>
        /// Тип състав
        /// </summary>
        [Display(Name = "Съдебен състав")]
        public int CourtDepartmentId { get; set; }

        /// <summary>
        /// Съд
        /// </summary>
        [Display(Name = "Съд")]
        public int? CourtId { get; set; }
    }
}
