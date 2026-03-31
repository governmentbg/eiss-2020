using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Модел за филър за справки свързани с дела
    /// </summary>
    public class CaseFilterReport
    {
        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        [Display(Name = "Съд")]
        public int? CourtId { get; set; }

        /// <summary>
        /// Основен вид дело
        /// </summary>
        [Display(Name = "Основен вид дело")]
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
        /// Вид производство
        /// </summary>
        [Display(Name = "Вид производство")]
        public int? ProcessPriorityId { get; set; }

        /// <summary>
        /// Към дата
        /// </summary>
        [Display(Name = "Към дата")]
        public DateTime? DateToSpr { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        [Display(Name = "От дата на образуване")]
        public DateTime? DateFromNew { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        [Display(Name = "До дата на образуване")]
        public DateTime? DateToNew { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        [Display(Name = "От дата на обявяване за решаване")]
        public DateTime? SessionWithResultAnnouncedForResolutionDateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        [Display(Name = "До дата на обявяване за решаване")]
        public DateTime? SessionWithResultAnnouncedForResolutionDateTo { get; set; }

        /// <summary>
        /// От дата на влизане в законна сила
        /// </summary>
        [Display(Name = "От дата на влизане в законна сила")]
        public DateTime? CaseInforcedDateFrom { get; set; }

        /// <summary>
        /// До дата на влизане в законна сила
        /// </summary>
        [Display(Name = "До дата на влизане в законна сила")]
        public DateTime? CaseInforcedDateTo { get; set; }

        /// <summary>
        /// Номер дело
        /// </summary>
        [Display(Name = "Номер дело")]
        public string RegNumber { get; set; }

        /// <summary>
        /// Година
        /// </summary>
        [Display(Name = "Година")]
        public int? CaseYear { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [Display(Name = "Статус")]
        public int? CaseStateId { get; set; }

        /// <summary>
        /// ЕИСПП Номер
        /// </summary>
        [Display(Name = "ЕИСПП Номер")]
        public string EisppNumber { get; set; }

        /// <summary>
        /// Свързано дело от съд
        /// </summary>
        [Display(Name = "Свързано дело от съд")]
        public int? LinkDelo_CourtId { get; set; }

        /// <summary>
        /// Свързано дело номер
        /// </summary>
        [Display(Name = "Свързано дело номер")]
        public string LinkDelo_RegNumber { get; set; }

        /// <summary>
        /// Свързано дело забележка
        /// </summary>
        [Display(Name = "Свързано дело забележка")]
        public string LinkDelo_Description { get; set; }

        /// <summary>
        /// Дело от друга система номер
        /// </summary>
        [Display(Name = "Дело от друга система номер")]
        public string RegNumberOtherSystem { get; set; }

        /// <summary>
        /// Дело от друга система
        /// </summary>
        [Display(Name = "Дело от друга система")]
        public bool VisibleOtherSystem { get; set; }

        /// <summary>
        /// Флаг за визуализация на дела от друга система
        /// </summary>
        public bool VisibleOtherSystemHidden { get; set; }

        /// <summary>
        /// Дело от друга система година
        /// </summary>
        [Display(Name = "Дело от друга система година")]
        public int? YearOtherSystem { get; set; }

        /// <summary>
        /// Дело от друга система от съд
        /// </summary>
        [Display(Name = "Дело от друга система от съд")]
        public int? CourtOtherSystem { get; set; }

        /// <summary>
        /// Свързано дело на външна институция
        /// </summary>
        [Display(Name = "Свързано дело на външна институция")]
        public int Institution_InstitutionTypeId { get; set; }

        /// <summary>
        /// Институция
        /// </summary>
        [Display(Name = "Институция")]
        public int? Institution_InstitutionId { get; set; }

        /// <summary>
        /// Година на дело на външна институция
        /// </summary>
        [Display(Name = "Година на дело на външна институция")]
        public int? Institution_CaseYear { get; set; }

        /// <summary>
        /// Свързано дело номер на външна институция
        /// </summary>
        [Display(Name = "Свързано дело номер на външна институция")]
        public string Institution_RegNumber { get; set; }

        /// <summary>
        /// Дата на документ
        /// </summary>
        [Display(Name = "Дата на документ")]
        public DateTime? DateDoc { get; set; }

        /// <summary>
        /// Номер на документ
        /// </summary>
        [Display(Name = "Номер на документ")]
        public string NumberDoc { get; set; }

        /// <summary>
        /// Име на лице
        /// </summary>
        [Display(Name = "Име")]
        public string NamePerson { get; set; }

        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        [Display(Name = "Идентификатор")]
        public string IdentifikatorPerson { get; set; }

        /// <summary>
        /// Заседание от дата
        /// </summary>
        [Display(Name = "Заседание от дата")]
        public DateTime? Session_DateFrom { get; set; }

        /// <summary>
        /// Заседание до дата
        /// </summary>
        [Display(Name = "Заседание до дата")]
        public DateTime? SessionDateTo { get; set; }

        /// <summary>
        /// Вид заседание
        /// </summary>
        [Display(Name = "Вид заседание")]
        public int SessionTypeId { get; set; }

        /// <summary>
        /// Вид заседание
        /// </summary>
        [Display(Name = "Вид заседание")]
        public string[] SessionTypeIds { get; set; }

        /// <summary>
        /// Зала
        /// </summary>
        [Display(Name = "Зала")]
        public int CourtHallId { get; set; }

        /// <summary>
        /// Статус на заседание
        /// </summary>
        [Display(Name = "Статус")]
        public int SessionStateId { get; set; }

        /// <summary>
        /// Резултат
        /// </summary>
        [Display(Name = "Резултат")]
        public int SessionResultId { get; set; }

        /// <summary>
        /// Вид акт
        /// </summary>
        [Display(Name = "Вид акт")]
        public int ActTypeId { get; set; }

        /// <summary>
        /// От дата на акт
        /// </summary>
        [Display(Name = "От дата на акт")]
        public DateTime? ActRegDateFrom { get; set; }

        /// <summary>
        /// От дата на акт
        /// </summary>
        [Display(Name = "До дата на акт")]
        public DateTime? ActRegDateTo { get; set; }

        /// <summary>
        /// Номер на акт
        /// </summary>
        [Display(Name = "Номер на акт")]
        public string ActNumber { get; set; }

        /// <summary>
        /// Финализиращ акт
        /// </summary>
        [Display(Name = "Финализиращ акт")]
        public bool ActIsFinalDoc { get; set; }

        /// <summary>
        /// Подлежи на обжалване
        /// </summary>
        [Display(Name = "Подлежи на обжалване")]
        public bool CanAppeal { get; set; }
        
        /// <summary>
        /// Флаг за финализиращ акт
        /// </summary>
        public bool ActIsFinalDocHidden { get; set; }

        /// <summary>
        /// Нормативен текст/Ключова дума или израз
        /// </summary>
        [Display(Name = "Нормативен текст/Ключова дума или израз")]
        public int ActLawBaseId { get; set; }

        /// <summary>
        /// Име на служител
        /// </summary>
        [Display(Name = "Име")]
        public string NameCaseLawUnit { get; set; }

        /// <summary>
        /// Идентификатор на служител
        /// </summary>
        [Display(Name = "Идентификатор")]
        public string IdentifikatorCaseLawUnit { get; set; }

        /// <summary>
        /// Съдия докладчик
        /// </summary>
        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        /// <summary>
        /// Основен вид документ
        /// </summary>
        [Display(Name = "Основен вид документ")]
        public int DocumentGroupId { get; set; }

        /// <summary>
        /// От дата на постановяване на акт
        /// </summary>
        [Display(Name = "От дата на постановяване на акт")]
        public DateTime? ActDateFrom { get; set; }

        /// <summary>
        /// До дата на постановяване на акт
        /// </summary>
        [Display(Name = "До дата на постановяване на акт")]
        public DateTime? ActDateTo { get; set; }

        /// <summary>
        /// Към дата на акт
        /// </summary>
        [Display(Name = "Към дата на акт")]
        public DateTime? ActDateToSpr { get; set; }

        /// <summary>
        /// Към дата на мотив
        /// </summary>
        [Display(Name = "Към дата на мотив")]
        public DateTime? ActDateMotiveToSpr { get; set; }

        /// <summary>
        /// Флаг за двойна размяна на документи
        /// </summary>
        public bool IsDoubleExchangeDoc { get; set; }

        /// <summary>
        /// От дата на свършване
        /// </summary>
        [Display(Name = "От дата на свършване")]
        public DateTime? ActDeclaredDateFrom { get; set; }

        /// <summary>
        /// До дата на свършване
        /// </summary>
        [Display(Name = "До дата на свършване")]
        public DateTime? ActDeclaredDateTo { get; set; }

        /// <summary>
        /// Изготвяне на съдебен акт до/над
        /// </summary>
        [Display(Name = "Изготвяне на съдебен акт до/над")]
        public string[] ActDateToIds { get; set; }

        /// <summary>
        /// Изготвяне на мотиви до/над
        /// </summary>
        [Display(Name = "Изготвяне на мотиви до/над")]
        public int? ActMotiveDateToId { get; set; }

        /// <summary>
        /// Насрочване до/над
        /// </summary>
        [Display(Name = "Насрочване до/над")]
        public int? SessionDateToId { get; set; }

        /// <summary>
        /// Индикатор
        /// </summary>
        [Display(Name = "Индикатор")]
        public int CaseClassificationId { get; set; }

        /// <summary>
        /// Състав
        /// </summary>
        [Display(Name = "Състав")]
        public int CourtDepartmentId { get; set; }

        /// <summary>
        /// Първоинстанционен съд
        /// </summary>
        [Display(Name = "Първоинстанционен съд")]
        public int? FirstInstanceCourtId { get; set; }

        /// <summary>
        /// Дата на влизане в сила на акта: след приключване на обжалването
        /// </summary>
        [Display(Name = "От дата на влизане в сила на акта")]
        public DateTime? ActInforcedDateFrom { get; set; }

        /// <summary>
        /// Дата на влизане в сила на акта: след приключване на обжалването
        /// </summary>
        [Display(Name = "До дата на влизане в сила на акта")]
        public DateTime? ActInforcedDateTo { get; set; }

        /// <summary>
        /// Флаг дали да има финализиращ акт
        /// </summary>
        public bool WithFinalAct { get; set; }

        /// <summary>
        /// От дата на приключване на делото
        /// </summary>
        [Display(Name = "От дата на приключване на делото")]
        public DateTime? LifecycleDateToFrom { get; set; }

        /// <summary>
        /// До дата на приключване на делото
        /// </summary>
        [Display(Name = "До дата на приключване на делото")]
        public DateTime? LifecycleDateToTo { get; set; }
    }
}
