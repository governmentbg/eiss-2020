using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyClaimVM
    {
        public int Id { get; set; }
        public string CaseMoneyClaimGroupLabel { get; set; }
        public string CaseMoneyClaimTypeLabel { get; set; }
        public string ClaimNumber { get; set; }
        public DateTime? ClaimDate { get; set; }
        public string Description { get; set; }
        public string Motive { get; set; }
        public string PartyNames { get; set; }
        public virtual ICollection<CaseMoneyCollectionVM> CaseMoneyCollections { get; set; }
        public virtual ICollection<CaseMoneyCollectionTotalSumVM> CaseMoneyCollectionTotalSums { get; set; }
    }
}
