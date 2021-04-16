using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class DocumentInReportVM
    {
        [Display(Name = "Входящ номер")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Дата и час на постъпване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DocumentDate { get; set; }

        [Display(Name = "Описание на документа")]
        public string Description { get; set; }

        [Display(Name = "Подател")]
        public string DocumentPersonName { get; set; }

        [Display(Name = "Направление")]
        public string TaskName { get; set; }
        
        public int DocumentNumberValue { get; set; }
    }

    public class DocumentInFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Вид документ")]
        public int DocumentKindId { get; set; }

        [Display(Name = "От номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? FromNumber { get; set; }

        [Display(Name = "До номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? ToNumber { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeId { get; set; }

        public string CaseTypeIds { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Съдебен състав")]
        public int DepartmentId { get; set; }
    }
}
