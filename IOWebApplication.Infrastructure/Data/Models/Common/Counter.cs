using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Броячи/регистри
    /// </summary>
    [Table("common_counter")]
    public class Counter
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        /// <summary>
        /// По: документ, дело, документ в дело
        /// </summary>
        [Column("counter_type")]
        [Display(Name ="Вид брояч")]
        public int CounterTypeId { get; set; }

        /// <summary>
        /// Вид нулиране
        /// </summary>
        [Column("reset_type")]
        [Display(Name ="Режим на нулиране")]
        public int ResetTypeId { get; set; }

        [Column("label")]
        [Display(Name ="Наименование")]
        public string Label { get; set; }

        [Column("value")]
        public int Value { get; set; }

        [Column("init_value")]
        [Display(Name ="Начална стойност")]
        public int InitValue { get; set; }

        [Column("last_used")]
        public DateTime LastUsed { get; set; }

        [Column("preffix")]
        [Display(Name ="Префикс")]
        public string Prefix { get; set; }
        [Column("suffix")]
        [Display(Name ="Суфикс")]
        public string Suffix { get; set; }
        [Column("digit_count")]
        [Display(Name ="Брой разреди")]
        public int DigitCount { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CounterTypeId))]
        public virtual CounterType CounterType { get; set; }

        [ForeignKey(nameof(ResetTypeId))]
        public virtual CounterResetType ResetType { get; set; }

        public virtual ICollection<CounterDocument> CounterDocument { get; set; }
        public virtual ICollection<CounterCase> CounterCase { get; set; }
        public virtual ICollection<CounterSessionAct> CounterSessionAct { get; set; }

        
    }
}
