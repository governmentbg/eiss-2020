using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyCollectionVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int? MainCaseMoneyCollectionId { get; set; }
        public int CaseMoneyClaimId { get; set; }
        public int CaseMoneyCollectionGroupId { get; set; }
        public string CaseMoneyCollectionGroupLabel { get; set; }
        public string CaseMoneyCollectionTypeLabel { get; set; }
        public string CaseMoneyCollectionKindLabel { get; set; }
        public int? CaseMoneyCollectionKindId { get; set; }
        public int? CaseMoneyCollectionKindOrder { get; set; }
        public string CurrencyLabel { get; set; }
        public string CurrencyCode { get; set; }
        public int CurrencyId { get; set; }
        public decimal InitialAmount { get; set; }
        public string InitialAmountString { get; set; }
        public decimal PretendedAmount { get; set; }
        public string PretendedAmountString { get; set; }
        public decimal RespectedAmount { get; set; }
        public string RespectedAmountString { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string DateToLabel { get; set; }
        public string Description { get; set; }
        public string JointDistribution { get; set; }
        public bool JointDistributionBool { get; set; }
        public bool IsMoney { get; set; }
        public bool IsMovables { get; set; }
        public bool IsItem { get; set; }

        public IList<CaseMoneyCollectionPersonVM> MoneyCollectionPersons { get; set; }
        public virtual ICollection<CaseMoneyCollectionVM> CaseMoneyCollectionExtras { get; set; }
    }
}
