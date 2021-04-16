using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Справка Съдени и осъдени лица
    /// </summary>
    public class CasePersonDefendantListReportVM
    {
        [Display(Name = "Точен вид дело")]
        public string CaseTypeName { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Съдия докладчик")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "Предмет и шифър")]
        public string CaseCodeName { get; set; }

        [Display(Name = "Резултат от заседанието")]
        public string SessionResultName { get; set; }

        [Display(Name = "Дата на приключване на делото")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CaseEndDate { get; set; }

        [Display(Name = "Подсъдим")]
        public string CasePersonName { get; set; }

        [Display(Name = "Характер на лицето")]
        public string PersonMaturityName { get; set; }

        [Display(Name = "Наложено наказание по НК")]
        public string SentenceTypeName { get; set; }

        [Display(Name = "Лишаване от свобода")]
        public string SentenceTime { get; set; }
    }

    /// <summary>
    /// Филтър за Справка Съдени и осъдени лица
    /// </summary>
    public class CasePersonDefendantListFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Шифър")]
        public int CaseCodeId { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Резултат от заседание")]
        public int SessionResultId { get; set; }

        [Display(Name = "Идентификатор")]
        public string PersonUicSearch { get; set; }

        [Display(Name = "Наименование/Част от Наименование")]
        public string PersonNameSearch { get; set; }

        [Display(Name = "Характер на лицето")]
        public int PersonMaturityId { get; set; }

        [Display(Name = "Наложено наказание по НК")]
        public int SentenceTypeId { get; set; }

        [Display(Name = "Приложение на")]
        public int SentenceLawbaseId { get; set; }
    }
}
