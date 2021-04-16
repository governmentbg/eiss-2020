using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Разпределение на вземане към обстоятелство по длъжници
    /// </summary>
    [Table("case_money_collection_person")]
    public class CaseMoneyCollectionPerson : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_money_collection_id")]
        public int CaseMoneyCollectionId { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Лице")]
        public int CasePersonId { get; set; }

        [Column("person_amount")]
        [Display(Name = "Сума")]
        public decimal PersonAmount { get; set; }

        [Column("respected_amount")]
        [Display(Name = "Уважено")]
        public decimal RespectedAmount { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseMoneyCollectionId))]
        public virtual CaseMoneyCollection CaseMoneyCollection { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }       
    }
}
