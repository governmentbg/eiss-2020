namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class Monetaryclaim
    {
        public string GUID { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public string ClaimCircumstanceGUID { get; set; }
        public decimal ClaimAmountInNumbers { get; set; }
        public string ClaimAmountInWords { get; set; }
        public decimal ClaimAmountInNumbersBGNEquivalent { get; set; }
        public string ClaimAmountInWordsBGNEquivalent { get; set; }
        public int FeeUntilPaymentRequested { get; set; }
        public ClaimDestribution[] ClaimDestributions { get; set; }
        public AdditionalMonetaryClaim[] AdditionalMonetaryClaims { get; set; }
        public string Description { get; set; }
    }
}
