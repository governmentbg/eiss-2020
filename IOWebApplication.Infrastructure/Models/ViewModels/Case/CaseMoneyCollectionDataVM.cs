using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyCollectionDataVM
    {
        public int CaseMoneyCollectionKindId { get; set; }
        public string CaseMoneyCollectionKindText { get; set; }

        [Display(Name = "")]
        public bool CaseMoneyCollectionKindBool { get; set; }

        [Display(Name = "Първоначално")]
        public decimal InitialAmount { get; set; }
        
        [Display(Name = "Претендирано")]
        public decimal PretendedAmount { get; set; }
        
        [Display(Name = "Уважено")]
        public decimal RespectedAmount { get; set; }
        
        [Display(Name = "Начало")]
        public DateTime? DateFrom { get; set; }
        
        [Display(Name = "Край")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Вид крайна дата до")]
        public int? MoneyCollectionEndDateTypeId { get; set; }

        [Display(Name = "Солидарно разпределение")]
        public bool JointDistribution { get; set; }

        [Display(Name = "Дроб")]
        public bool? IsFraction { get; set; }

        public IList<CasePersonListDecimalVM> CasePersonListDataDecimals { get; set; }
    }
}
