using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class PaymentFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Вид сметка")]
        public int MoneyGroupId { get; set; }

        [Display(Name = "Вносител")]
        public string SenderName { get; set; }

        [Display(Name = "Начин на плащане")]
        public int PaymentTypeId { get; set; }

        [Display(Name = "Изберете потребител")]
        public string UserId { get; set; }

        [Display(Name = "ПОС устройство")]
        public string PosDeviceTid { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Активни плащания")]
        public bool ActivePayment { get; set; }
    }
}
