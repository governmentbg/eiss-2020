using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Заповедни дела
    /// </summary>
    [Table("case_fast_process")]
    public class CaseFastProcess : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }        

        [Column("description")]
        [Display(Name = "Допълнителни изявления")]
        public string Description { get; set; }

        [Column("tax_amount")]
        [Display(Name = "Държавна такса")]
        public decimal TaxAmount { get; set; }

        [Column("currency_id")]
        [Display(Name = "Валута")]
        public int CurrencyId { get; set; }

        [Column("is_respected_amount")]
        [Display(Name = "Уважено")]
        public bool? IsRespectedAmount { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CurrencyId))]
        public virtual Currency Currency { get; set; }
    }
}
