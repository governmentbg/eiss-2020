using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ObligationVM
    {
        public int Id { get; set; }

        public bool CheckRow { get; set; }

        public string ObligationNumber { get; set; }

        public DateTime ObligationDate { get; set; }

        public string CasePersonUic { get; set; }

        public string CasePersonName { get; set; }

        public string MoneyTypeName { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountBGN { get; set; }

        public decimal AmountPay { get; set; }

        public decimal AmountForPay { get { return this.Amount - this.AmountPay; } }

        public bool IsActive { get; set; }

        public string RegNumberExpenseOrder { get; set; }

        public string RegNumberExecList { get; set; }

        public int ExpenseOrderId { get; set; }

        public int ExecListId { get; set; }
        public string DocumentText
        {
            get
            {
                if (DocumentDate.HasValue)
                {
                    return $"{DocumentNumber} / {DocumentDate:dd.MM.yyyy}";
                }
                else
                {
                    return "";
                }
            }
        }
        public string DocumentNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public long? DocumentId { get; set; }
        public string DocumentTypeLabel { get; set; }

        public string ObligationCourtName { get; set; }

        public ObligationPaymentInfo[] Payments { get; set; }
    }

    public class ObligationPaymentInfo
    {
        public string PaymentCourtName { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Description { get; set; }
        public bool IsAutomatic { get; set; }
        public string PaymentTypeName { get; set; }
    }
}
