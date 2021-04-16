using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.EISPP
{
    /// <summary>
    /// Сигнални Събития от ЕИСПП 
    /// </summary>
    [Table("common_eispp_signal")]
    public class EisppSignal 
    { 
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Дата получаване
        /// </summary>
        [Display(Name = "Дата получаване")]
        [Column("date_create")]
        public DateTime DateCreate { get; set; }


        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("response_data")]
        public string ResponseData { get; set; }

        /// <summary>
        ///  adrstr
        /// структура, създала събитието
        /// елемент от номенклатурата в Стандарт 7
        /// съответстващ на текущия съд
        /// </summary>
        [Column("structure_id")]
        public int? StructureId { get; set; }

        /// <summary>
        ///  adrnprnmr
        /// ЕИСПП номер
        /// </summary>
        [Column("eispp_number")]
        [Display(Name = "ЕИСПП номер")]
        public string EISSPNumber { get; set; }

        /// <summary>
        /// adrdlovid
        /// вид дело
        /// </summary>
        [Display(Name = "Вид дело")]
        [Column("case_type")]
        public int? CaseType { get; set; }

        /// <summary>
        /// adrdlosig
        /// точен вид дело
        /// системен код на елемент от nmk_dlosig
        /// </summary>
        [Display(Name = "Точен вид дело")]
        [Column("exact_case_type")]
        public int? ExactCaseType { get; set; }


        /// <summary>
        /// adrdlogdn
        /// година на делото
        /// </summary>
        [Display(Name = "Година")]
        [Column("case_year")]
        public int? CaseYear { get; set; }

        /// <summary>
        /// adrdlonmr
        /// кратък номер, а не 14-цифрен  номер
        /// </summary>
        [Display(Name = "Номер")]
        [Column("short_number")]
        public int? ShortNumber { get; set; }

        /// <summary>
        /// съдържание на съобщението
        /// </summary>
        [Column("message")]
        public string Message { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }
    }
}
