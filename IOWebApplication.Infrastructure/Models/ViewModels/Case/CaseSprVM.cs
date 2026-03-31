using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за визуализация на данни свързани с дела
    /// </summary>
    public class CaseSprVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на съд
        /// </summary>
        public string CourtLabel { get; set; }

        /// <summary>
        /// Група дело
        /// </summary>
        public string CaseGroupLabel { get; set; }

        /// <summary>
        /// Вид дело
        /// </summary>
        [Display(Name = "Вид дело")]
        public string CaseTypeLabel { get; set; }

        /// <summary>
        /// Номер/Година
        /// </summary>
        [Display(Name = "Номер/Година")]
        public string CaseRegNum { get; set; }

        /// <summary>
        /// Образувано
        /// </summary>
        [Display(Name = "Образувано")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CaseRegDate { get; set; }

        /// <summary>
        /// Предмет
        /// </summary>
        [Display(Name = "Предмет")]
        public string CaseCodeLabel { get; set; }

        /// <summary>
        /// Дата на образуване на дело
        /// </summary>
        public DateTime CaseBeginDate { get; set; }

        /// <summary>
        /// Дата на свършване
        /// </summary>
        [Display(Name = "Свършило")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CaseEndDate { get; set; }

        /// <summary>
        /// данни за лице
        /// </summary>
        public string CasePersons { get; set; }

        /// <summary>
        /// данни за лице
        /// </summary>
        public string CasePerson { get; set; }

        /// <summary>
        /// Роля на лице
        /// </summary>
        public string CasePersonRoles { get; set; }

        /// <summary>
        /// Роля на лице
        /// </summary>
        public string CasePersonRole { get; set; }

        /// <summary>
        /// Съдия-докладчик
        /// </summary>
        [Display(Name = "Съдия-докладчик")]
        public string JudgeReport { get; set; }

        /// <summary>
        /// Информация за присъда
        /// </summary>
        public string CasePersonSentenceInfo { get; set; }

        /// <summary>
        /// Срок от първото образуване
        /// </summary>
        [Display(Name = "Срок от първото образуване")]
        public string CaseLifeCycle { get; set; }

        /// <summary>
        /// Начална дата на заседание
        /// </summary>
        public DateTime? SessionDateFrom { get; set; }

        /// <summary>
        /// Дата на акт
        /// </summary>
        public DateTime? SessionActDate { get; set; }

        /// <summary>
        /// Вид заседание
        /// </summary>
        public string SessionTypeLabel { get; set; }

        /// <summary>
        /// Дата на документ
        /// </summary>
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Резолюция на съдия
        /// </summary>
        public string ResolutionJudge { get; set; }

        /// <summary>
        /// Информация за финализиращ акт
        /// </summary>
        public string ActFinalInfo { get; set; }

        /// <summary>
        /// Резултат на заседание
        /// </summary>
        public string SessionResult { get; set; }
        public string SessionResultFirst { get; set; }

        /// <summary>
        /// Вид акт
        /// </summary>
        public string ActTypeLabel { get; set; }

        /// <summary>
        /// Дата на връщане на акта
        /// </summary>
        public DateTime? ActReturnDate { get; set; }

        /// <summary>
        /// Дата на постановяване на мотив
        /// </summary>
        public DateTime? ActMotivesDeclaredDate { get; set; }

        /// <summary>
        /// Дата на постановяване на акта
        /// </summary>
        public DateTime? ActDeclaredDate { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [Display(Name = "Статус")]
        public string CaseStateName { get; set; }

        /// <summary>
        /// Флаг дали съществува финализиращ акт
        /// </summary>
        public string IsExistFinalAct { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int? CourtId { get; set; }

        /// <summary>
        /// Първоинстанционен съд
        /// </summary>
        [Display(Name = "Първоинстанционен съд")]
        public string FirstInstanceCourtLabel { get; set; }

        /// <summary>
        /// Дата на приключване на делото
        /// </summary>
        public DateTime? DateFinishCase { get; set; }

        #region Регион свързан с изчисляване на продължителност на дело

        /// <summary>
        /// Начална дата на интервала
        /// </summary>
        public DateTime? LifeCycleDateFrom { get; set; }

        /// <summary>
        /// Крайна дата на интервала
        /// </summary>
        public DateTime? LifeCycleDateTo { get; set; }

        /// <summary>
        /// Продължителност в месеци
        /// </summary>
        public int DurationMonths { get; set; }

        /// <summary>
        /// Брой дни на главен интервал
        /// </summary>
        public int LifeCycleDaysProgress { get; set; }

        /// <summary>
        /// Брой дни на стопиращ интервал
        /// </summary>
        public int LifeCycleDaysStop { get; set; }

        /// <summary>
        /// Инфо за дела за продължителност на дело
        /// </summary>
        public string CaseMigrationLifeCycleInfo { get; set; }

        [Display(Name = "Срок от първото образуване")]
        public string LifeCycleInfo 
        { 
            get
            {
                return (LifeCycleDateFrom != null) ? (string.IsNullOrEmpty(CaseMigrationLifeCycleInfo) ? "" : CaseMigrationLifeCycleInfo + " ") +
                                                     (((Math.Abs(12 * ((LifeCycleDateFrom ?? DateTime.Now).AddDays(LifeCycleDaysProgress - LifeCycleDaysStop).Year - (LifeCycleDateFrom ?? DateTime.Now).Year) + (LifeCycleDateFrom ?? DateTime.Now).AddDays(LifeCycleDaysProgress - LifeCycleDaysStop).Month - (LifeCycleDateFrom ?? DateTime.Now).Month) +
                                                      ((LifeCycleDateFrom ?? DateTime.Now).AddDays(LifeCycleDaysProgress - LifeCycleDaysStop).Day > (LifeCycleDateFrom ?? DateTime.Now).Day ? 1 : 0)) == 0 ? 1 : (Math.Abs(12 * ((LifeCycleDateFrom ?? DateTime.Now).AddDays(LifeCycleDaysProgress - LifeCycleDaysStop).Year - (LifeCycleDateFrom ?? DateTime.Now).Year) + (LifeCycleDateFrom ?? DateTime.Now).AddDays(LifeCycleDaysProgress - LifeCycleDaysStop).Month - (LifeCycleDateFrom ?? DateTime.Now).Month) +
                                                                                                                                                                                                                  ((LifeCycleDateFrom ?? DateTime.Now).AddDays(LifeCycleDaysProgress - LifeCycleDaysStop).Day > (LifeCycleDateFrom ?? DateTime.Now).Day ? 1 : 0))) + DurationMonths).ToString() : string.Empty;
            }
        }

        #endregion
    }
}
