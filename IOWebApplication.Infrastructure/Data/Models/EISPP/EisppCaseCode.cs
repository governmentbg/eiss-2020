using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.EISPP
{
    /// <summary>
    /// Допустими събития ЕИСПП по Шифри и инстанция
    /// </summary>
    [Table("nom_eispp_case_code")]
    public class EisppCaseCode 
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Шифър
        /// </summary>
        [Column("code")]
        [Display(Name = "Шифър")]
        public string Code { get; set; }

        /// <summary>
        /// Инстанция
        /// </summary>
        [Column("case_instance")]
        [Display(Name = "Инстанция")]
        public int CaseInstance { get; set; }

        /// <summary>
        /// dlosprvid
        /// вид съдебно производство
        /// системен код на елемент от nmk_dlosprvid
        /// Задължително за всички събития с изключение на събитие 913 „Получаване на дело“
        /// </summary>
        [Column("legal_proceeding_type")]
        [Display(Name = "Вид съдебно производство")]
        public int LegalProceedingType { get; set; }

        /// <summary>
        /// Допустими събития
        /// </summary>
        [Column("event_codes")]
        [Display(Name = "Допустими събития")]
        public string EventCodes { get; set; }
    }
}
