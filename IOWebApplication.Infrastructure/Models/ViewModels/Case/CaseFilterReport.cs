using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseFilterReport
    {
        public int CourtId { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Шифър")]
        public int CaseCodeId { get; set; }

        [Display(Name = "Към дата")]
        public DateTime? DateToSpr { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер дело")]
        public string RegNumber { get; set; }

        [Display(Name = "Година")]
        public int? CaseYear { get; set; }

        [Display(Name = "Статус")]
        public int? CaseStateId { get; set; }

        [Display(Name = "ЕИСПП Номер")]
        public string EisppNumber { get; set; }

        [Display(Name = "Свързано дело от съд")]
        public int? LinkDelo_CourtId { get; set; }

        [Display(Name = "Свързано дело номер")]
        public string LinkDelo_RegNumber { get; set; }

        [Display(Name = "Свързано дело забележка")]
        public string LinkDelo_Description { get; set; }

        //[Display(Name = "Свързано дело номер")]
        //public int? LinkDelo_CaseId { get; set; }

        [Display(Name = "Дело от друга система номер")]
        public string RegNumberOtherSystem { get; set; }

        [Display(Name = "Дело от друга система")]
        public bool VisibleOtherSystem { get; set; }

        public bool VisibleOtherSystemHidden { get; set; }

        [Display(Name = "Дело от друга система година")]
        public int? YearOtherSystem { get; set; }

        [Display(Name = "Дело от друга система от съд")]
        public int? CourtOtherSystem { get; set; }

        [Display(Name = "Свързано дело на външна институция")]
        public int Institution_InstitutionTypeId { get; set; }

        [Display(Name = "Институция")]
        public int? Institution_InstitutionId { get; set; }

        [Display(Name = "Година на дело")]
        public int? Institution_CaseYear { get; set; }

        [Display(Name = "Свързано дело номер")]
        public string Institution_RegNumber { get; set; }

        [Display(Name = "Дата на документ")]
        public DateTime? DateDoc { get; set; }

        [Display(Name = "Номер на документ")]
        public string NumberDoc { get; set; }

        [Display(Name = "Име")]
        public string NamePerson { get; set; }

        [Display(Name = "Идентификатор")]
        public string IdentifikatorPerson { get; set; }

        [Display(Name = "Заседание от дата")]
        public DateTime? Session_DateFrom { get; set; }

        [Display(Name = "Заседание до дата")]
        public DateTime? SessionDateTo { get; set; }

        [Display(Name = "Вид заседание")]
        public int SessionTypeId { get; set; }

        [Display(Name = "Зала")]
        public int CourtHallId { get; set; }

        [Display(Name = "Статус")]
        public int SessionStateId { get; set; }

        [Display(Name = "Резултат")]
        public int SessionResultId { get; set; }

        [Display(Name = "Вид акт")]
        public int ActTypeId { get; set; }

        [Display(Name = "Дата на акт")]
        public DateTime? ActDate { get; set; }

        [Display(Name = "Номер на акт")]
        public string ActNumber { get; set; }

        [Display(Name = "Финализиращ акт")]
        public bool ActIsFinalDoc { get; set; }
        public bool ActIsFinalDocHidden { get; set; }

        [Display(Name = "Нормативен текст/Ключова дума или израз")]
        public int ActLawBaseId { get; set; }

        [Display(Name = "Име")]
        public string NameCaseLawUnit { get; set; }

        [Display(Name = "Идентификатор")]
        public string IdentifikatorCaseLawUnit { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Основен вид документ")]
        public int DocumentGroupId { get; set; }

        [Display(Name = "Акт от дата")]
        public DateTime? ActDateFrom { get; set; }

        [Display(Name = "Акт до дата")]
        public DateTime? ActDateTo { get; set; }

        [Display(Name = "Към дата на акт")]
        public DateTime? ActDateToSpr { get; set; }

        [Display(Name = "Към дата на мотив")]
        public DateTime? ActDateMotiveToSpr { get; set; }

        public bool IsDoubleExchangeDoc { get; set; }

        [Display(Name = "Вид производство")]
        public int? ProcessPriorityId { get; set; }

        [Display(Name = "От дата на свършване")]
        public DateTime? ActDeclaredDateFrom { get; set; }

        [Display(Name = "До дата на свършване")]
        public DateTime? ActDeclaredDateTo { get; set; }

        [Display(Name = "Изготвяне на съдебен акт до/над")]
        public int? ActDateToId { get; set; }

        [Display(Name = "Насрочване до/над")]
        public int? SessionDateToId { get; set; }

        [Display(Name = "Индикатор")]
        public int CaseClassificationId { get; set; }

        [Display(Name = "Състав")]
        public int CourtDepartmentId { get; set; }
    }
}
