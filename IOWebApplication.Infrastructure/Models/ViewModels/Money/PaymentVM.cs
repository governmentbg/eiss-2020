using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class PaymentVM
    {
        public int Id { get; set; }

        public int CourtId { get; set; }

        [Display(Name = "Начин на плащане")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int PaymentTypeId { get; set; }

        [Display(Name = "Вид сметка")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int? CourtBankAccountId { get; set; }

        [Display(Name = "Сума")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public decimal Amount { get; set; }

        [Display(Name = "Платено на")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime PaidDate { get; set; }

        [Display(Name = "Вносител")]
        public string SenderName { get; set; }

        [Display(Name = "Основание")]
        public string PaymentInfo { get; set; }

        [Display(Name = "Още пояснения")]
        public string PaymentDescription { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        public string ObligationIds { get; set; }

        public bool IsAvans { get; set; }

        [Display(Name = "Номер")]
        public string PaymentNumber { get; set; }

        public int PosPaymentResultId { get; set; }

        public bool ForPopUp { get; set; }
    }
}
