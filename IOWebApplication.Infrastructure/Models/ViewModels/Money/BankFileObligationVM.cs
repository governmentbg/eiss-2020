

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class BankFileObligationVM
    {
        public int ObligationId { get; set; }

        public decimal Amount { get; set; }

        public string DocumentNumber { get; set; }

        public string CaseNumber { get; set; }

        public string CaseShortNumberYear { get; set; }

        public string EpepNumber { get; set; }

        public string Uic { get; set; }

        public string[] DocumentUic { get; set; }
    }
}
