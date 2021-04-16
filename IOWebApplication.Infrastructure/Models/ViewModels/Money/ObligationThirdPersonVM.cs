using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ObligationThirdPersonVM
    {
        public int Id { get; set; }

        public bool CheckRow { get; set; }

        public DateTime ObligationDate { get; set; }

        public string PersonName { get; set; }

        public string PersonReceiveName { get; set; }

        public string MoneyTypeName { get; set; }

        public decimal Amount { get; set; }

        public string ObligationInfo { get; set; }

        public string CaseData { get; set; }

        public string RegNumberExecList { get; set; }

        public int ExecListId { get; set; }
    }

    public class ObligationThirdPersonFilterVM
    {
        [Display(Name = "Идентификатор")]
        public string PersonUicSearch { get; set; }

        [Display(Name = "Наименование/Част от Наименование")]
        public string PersonNameSearch { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Вид сума")]
        public int MoneyTypeId { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }
    }
}
