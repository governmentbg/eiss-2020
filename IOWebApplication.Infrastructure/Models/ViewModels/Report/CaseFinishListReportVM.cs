using IOWebApplication.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за справка свършени дела за период - първоинстанционни дела/въззивни/касационни дела
    /// </summary>
    public class CaseFinishListReportVM
    {
        /// <summary>
        /// идентификатор на дело
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
        /// Съдия докладчик
        /// </summary>
        [Display(Name = "Съдия докладчик")]
        public string JudgeReporterName { get; set; }

        /// <summary>
        /// Съдия-докладчик (по фин. акт.)
        /// </summary>
        [Display(Name = "Съдия-докладчик (по фин. акт.)")]
        public string JudgeReporterFinalActName { get; set; }

        /// <summary>
        /// Предмет и шифър
        /// </summary>
        [Display(Name = "Предмет и шифър")]
        public string CaseCodeName { get; set; }

        /// <summary>
        /// Резултат/Степен на уважаване на иска
        /// </summary>
        [Display(Name = "Резултат/Степен на уважаване на иска")]
        public string ActComplainResultName { get; set; }

        /// <summary>
        /// Резултат от заседанието
        /// </summary>
        [Display(Name = "Резултат от заседанието")]
        public string SessionResultName { get; set; }
        public string SessionResultNameFirst { get; set; }

        /// <summary>
        /// Причина за прекратяване
        /// </summary>
        [Display(Name = "Причина за прекратяване")]
        public string SessionResultStopBaseName { get; set; }
        public string SessionResultStopBaseNameFirst { get; set; }

        /// <summary>
        /// Дата на приключване на делото
        /// </summary>
        [Display(Name = "Дата на приключване на делото")]
        public string CaseDateFinish { get; set; }
        [Display(Name = "Дата на приключване на делото")]
        public DateTime? CaseDateFinishDate { get; set; }
        [Display(Name = "Дата на приключване на делото")]
        public string CaseDateFinishDateString
        {
            get
            {
                return CaseDateFinishDate != null ? CaseDateFinishDate.DateToString() : string.Empty;
            }
        }

        /// <summary>
        /// Продължителност
        /// </summary>
        [Display(Name = "Продължителност")]
        public int CaseLifecycleMonths { get; set; }

        [Display(Name = "Продължителност")]
        public string CaseLifecycleMonthsString 
        { 
            get
            {
                return CaseLifecycleMonths.ToString();
            }
        }

        /// <summary>
        /// Първоинстанционен съд
        /// </summary>
        [Display(Name = "Първоинстанционен съд")]
        public string InitialCourtName
        {
            get
            {
                return string.IsNullOrEmpty(NewLinkNumber) == false ? NewLinkNumber : OldLinkNumber;
            }
        }

        public string OldLinkNumber { get; set; }

        public string NewLinkNumber { get; set; }

        /// <summary>
        /// Дата на образуване
        /// </summary>
        [Display(Name = "Дата на образуване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CaseRegDate { get; set; }

        /// <summary>
        /// Брой месеци от дата на образуване за граждански и търговски дела
        /// </summary>
        [Display(Name = "Продължителност от дата на образуване")]
        public int? NumberMonths { get; set; }

        /// <summary>
        /// Брой месеци от дата на образуване за граждански и търговски дела
        /// </summary>
        [Display(Name = "Продължителност от дата на образуване")]
        public string NumberMonthsText { get { return (NumberMonths != null && NumberMonths > 0) ? NumberMonths.ToString() : string.Empty; } }

        /// <summary>
        /// Дата на влизане в законна сила
        /// </summary>
        [Display(Name = "Дата на влизане в сила на делото")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CaseInforcedDate { get; set; }

        /// <summary>
        /// Дата на влизане в законна сила - стринг
        /// </summary>
        [Display(Name = "Дата на влизане в сила на делото")]
        public string CaseInforcedDateString 
        { 
            get
            {
                return CaseInforcedDate.DateToString();
            }
        }

        /// <summary>
        /// Дата на обявяване за решаване
        /// </summary>
        [Display(Name = "Дата на обявяване за решаване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateAnnouncedForResolution { get; set; }

        /// <summary>
        /// Дата на обявяване за решаване - string
        /// </summary>
        [Display(Name = "Дата на обявяване за решаване")]
        public string DateAnnouncedForResolutionString 
        { 
            get
            {
                return DateAnnouncedForResolution.DateToString();
            }
        }

        /// <summary>
        /// Вид Иницииращ документ
        /// </summary>
        [Display(Name = "Иницииращ документ")]
        public string DocumentTypeLabel { get; set; }

        /// <summary>
        /// Име на съд
        /// </summary>
        public string CourtLabel { get; set; }
    }

    /// <summary>
    /// Филтър Свършени дела за период
    /// </summary>
    public class CaseFinishListFilterReportVM
    {
        /// <summary>
        /// От дата на приключване
        /// </summary>
        [Display(Name = "От дата на приключване")]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// До дата на приключване
        /// </summary>
        [Display(Name = "До дата на приключване")]
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
        /// Съдия докладчик
        /// </summary>
        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

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
        /// Резултат/Степен на уважаване на иска
        /// </summary>
        [Display(Name = "Резултат/Степен на уважаване на иска")]
        public int ActComplainResultId { get; set; }

        /// <summary>
        /// Резултат от заседанието
        /// </summary>
        [Display(Name = "Резултат от заседанието")]
        public int SessionResultId { get; set; }

        /// <summary>
        /// Първоинстанционен съд
        /// </summary>
        [Display(Name = "Първоинстанционен съд")]
        public int InitialCourtId { get; set; }

        /// <summary>
        /// Съдия-докладчик (по фин. акт.)
        /// </summary>
        [Display(Name = "Съдия-докладчик (по фин. акт.)")]
        public int JudgeReporterFinalActId { get; set; }

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
        /// Идентификатор на съд
        /// </summary>
        [Display(Name = "Съд")]
        public int? CourtId { get; set; }
    }
}
