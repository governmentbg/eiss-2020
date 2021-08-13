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
    /// Обстоятелства по заповедни производства
    /// </summary>
    [Table("case_money_claim")]
    public class CaseMoneyClaim : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_money_claim_group_id")]
        [Display(Name = "Вид")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int CaseMoneyClaimGroupId { get; set; }

        [Column("case_money_claim_type_id")]
        [Display(Name = "Тип")]
        public int? CaseMoneyClaimTypeId { get; set; }
       
        [Column("claim_number")]
        [Display(Name = "Номер")]
        //[Required(ErrorMessage = "Въведете {0}.")]
        public string ClaimNumber { get; set; }

        [Column("claim_date")]
        [Display(Name = "Дата")]
        //[Required(ErrorMessage = "Въведете {0}.")]
        public DateTime? ClaimDate { get; set; }

        [Column("party_names")]
        [Display(Name = "Страни")]
        public string PartyNames { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("motive")]
        [Display(Name = "Мотив")]
        public string Motive { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseMoneyClaimGroupId))]
        public virtual CaseMoneyClaimGroup CaseMoneyClaimGroup { get; set; }

        [ForeignKey(nameof(CaseMoneyClaimTypeId))]
        public virtual CaseMoneyClaimType CaseMoneyClaimType { get; set; }

        public virtual ICollection<CaseMoneyCollection> CaseMoneyCollections { get; set; }

        public CaseMoneyClaim()
        {
            CaseMoneyCollections = new HashSet<CaseMoneyCollection>();
        }
    }
}
