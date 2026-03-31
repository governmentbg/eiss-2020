using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class BalanceBankPaymentVM
    {
        [Display(Name = " ")]
        public long Id { get; set; }

        [Display(Name = "Сума на задължението")]
        public decimal Amount { get; set; }

        public int ObligationId { get; set; }

        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Display(Name = "Описание")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Полето трябва да е най-малко {2} символа")]
        public string Description { get; set; }
    }
}
