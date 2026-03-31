using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за филтър на справки свързани с актове
    /// </summary>
    public class CaseSessionActReportFilterVM
    {
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
        /// Тип акт
        /// </summary>
        [Display(Name = "Тип акт")]
        public int ActTypeId { get; set; }

        /// <summary>
        /// Основен вид докумет
        /// </summary>
        [Display(Name = "Основен вид докумет")]
        public int DocumentGroupId { get; set; }

        /// <summary>
        /// Точен вид документ
        /// </summary>
        [Display(Name = "Точен вид документ")]
        public int DocumentTypeId { get; set; }

        /// <summary>
        /// Съдия докладчик
        /// </summary>
        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        /// <summary>
        /// Резултат/степен на уважаване на иска
        /// </summary>
        [Display(Name = "Резултат/степен на уважаване на иска")]
        public int? ActComplainResultId { get; set; }

        /// <summary>
        /// Влизане в сила от
        /// </summary>
        [Display(Name = "Влизане в сила от")]
        public DateTime? ActInforcedDateFrom { get; set; }

        /// <summary>
        /// Влизане в сила до
        /// </summary>
        [Display(Name = "Влизане в сила до")]
        public DateTime? ActInforcedDateTo { get; set; }

        /// <summary>
        /// Вид производство
        /// </summary>
        [Display(Name = "Вид производство")]
        public int? ProcessPriorityId { get; set; }

        /// <summary>
        /// Резултат от заседание
        /// </summary>
        [Display(Name = "Резултат от заседание")]
        public int? SessionResultId { get; set; }

        /// <summary>
        /// Статус на акт
        /// </summary>
        [Display(Name = "Статус на акт")]
        public int? ActStateId { get; set; }

        /// <summary>
        /// Финализиращ акт
        /// </summary>
        [Display(Name = "Финализиращ акт")]
        public bool IsFinalDoc { get; set; }

        /// <summary>
        /// Подлежи на обжалване
        /// </summary>
        [Display(Name = "Подлежи на обжалване")]
        public bool CanAppeal { get; set; }
    }
}
