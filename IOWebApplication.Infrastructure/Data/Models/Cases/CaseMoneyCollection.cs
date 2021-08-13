using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Вземане към обстоятелство по заповедни производства
    /// </summary>
    [Table("case_money_collection")]
    public class CaseMoneyCollection : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("main_case_money_collection_id")]
        public int? MainCaseMoneyCollectionId { get; set; }

        [Column("case_money_claim_id")]
        public int CaseMoneyClaimId { get; set; }

        /// <summary>
        /// Основен вид на вземането: Парично вземане, предаване на заместими вещи
        /// </summary>
        [Column("case_money_collection_group_id")]
        [Display(Name = "Вид")]
        public int CaseMoneyCollectionGroupId { get; set; }

        /// <summary>
        /// Точен вид на вземането: Парично вземане: главница, друг вид вземане, Предавания на движима вещ, 
        /// </summary>
        [Column("case_money_collection_type_id")]
        [Display(Name = "Тип")]
        public int? CaseMoneyCollectionTypeId { get; set; }

        /// <summary>
        /// Вид допълнително парично вземане: лихва, такса, друго
        /// </summary>
        [Column("case_money_collection_kind_id")]
        [Display(Name = "Вид допълнително вземане")]
        public int? CaseMoneyCollectionKindId { get; set; }

        [Column("currency_id")]
        [Display(Name = "Валута")]
        public int CurrencyId { get; set; }

        [Column("initial_amount")]
        [Display(Name = "Първоначално")]
        public decimal InitialAmount { get; set; }

        [Column("pretended_amount")]
        [Display(Name = "Претендирано")]
        public decimal PretendedAmount { get; set; }

        [Column("respected_amount")]
        [Display(Name = "Уважено")]
        public decimal RespectedAmount { get; set; }

        [Column("date_from")]
        [Display(Name = "Начало")]
        public DateTime? DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Край")]
        public DateTime? DateTo { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("motive")]
        [Display(Name = "Мотив")]
        public string Motive { get; set; }

        [Column("label")]
        [Display(Name = "Име")]
        public string Label { get; set; }

        [Column("joint_distribution")]
        [Display(Name = "Солидарно разпределение")]
        public bool JointDistribution { get; set; }

        [Column("is_fraction")]
        [Display(Name = "Дроб")]
        public bool? IsFraction { get; set; }

        [Column("money_collection_end_date_type_id")]
        [Display(Name = "Вид крайна дата")]
        public int? MoneyCollectionEndDateTypeId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseMoneyClaimId))]
        public virtual CaseMoneyClaim CaseMoneyClaim { get; set; }

        [ForeignKey(nameof(MainCaseMoneyCollectionId))]
        public virtual CaseMoneyCollection MainCaseMoneyCollection { get; set; }

        [ForeignKey(nameof(CaseMoneyCollectionGroupId))]
        public virtual CaseMoneyCollectionGroup CaseMoneyCollectionGroup { get; set; }

        [ForeignKey(nameof(CaseMoneyCollectionTypeId))]
        public virtual CaseMoneyCollectionType CaseMoneyCollectionType { get; set; }

        [ForeignKey(nameof(CaseMoneyCollectionKindId))]
        public virtual CaseMoneyCollectionKind CaseMoneyCollectionKind { get; set; }

        [ForeignKey(nameof(CurrencyId))]
        public virtual Currency Currency { get; set; }

        [ForeignKey(nameof(MoneyCollectionEndDateTypeId))]
        public virtual MoneyCollectionEndDateType MoneyCollectionEndDateType { get; set; }

        public virtual ICollection<CaseMoneyCollectionPerson> CaseMoneyCollectionPersons { get; set; }
        public virtual ICollection<CaseMoneyCollection> CaseMoneyCollections { get; set; }

        [NotMapped]
        public string ApiModelId { get; set; }

        public CaseMoneyCollection()
        {
            CaseMoneyCollections = new HashSet<CaseMoneyCollection>();
        }
    }
}
