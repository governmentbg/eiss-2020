using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Модел за филтър на данни свързани с лица
    /// </summary>
    public class CasePersonFilterVM
    {
        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        [Display(Name = "Идентификатор на лице")]
        public string Uic { get; set; }

        /// <summary>
        /// Имена/Наименование на лице
        /// </summary>
        [Display(Name = "Имена/Наименование на лице")]
        public string FullName { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

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
        /// От дата на свършване
        /// </summary>
        [Display(Name = "От дата на свършване")]
        public DateTime? FinalDateFrom { get; set; }

        /// <summary>
        /// До дата на свършване
        /// </summary>
        [Display(Name = "До дата на свършване")]
        public DateTime? FinalDateTo { get; set; }

        /// <summary>
        /// Несвършило към дата
        /// </summary>
        [Display(Name = "Несвършило към дата")]
        public DateTime? WithoutFinalDateTo { get; set; }

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
        public string[] CaseCodeIds { get; set; }
    }
}
