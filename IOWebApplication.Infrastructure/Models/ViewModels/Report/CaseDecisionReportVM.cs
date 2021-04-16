using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseDecisionReportVM
    {
        [Display(Name = "Пореден №")]
        public int Index { get; set; }

        [Display(Name = "Вид дело")]
        public string CaseGroupName { get; set; }

        [Display(Name = "№/година на делото")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Съдия докладчик")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "Служител, обезличил съдебния акт")]
        public string DepersonalizeUser { get; set; }

        [Display(Name = "Дата на обявяване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ActDeclaredDate { get; set; }

        [Display(Name = "Съдебен акт (линк)")]
        public string ActLink { get; set; }

        [Display(Name = "Съдебен акт (линк)")]
        public string FileAct { get; set; }

        [Display(Name = "Мотиви (линк)")]
        public string FileMotive { get; set; }
    }

    public class CaseDecisionFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "С диспозитив")]
        public bool WithActDescription { get; set; }

        [Display(Name = "Без диспозитив за дела с ограничен достъп")]
        public bool WithoutActDescriptionCaseRestriction { get; set; }
    }
}
