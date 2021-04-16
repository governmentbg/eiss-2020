namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class Bankaccount
    {
        public string BankName { get; set; }
        public string IBAN { get; set; }
        public string BIC { get; set; }
        public string Country { get; set; }
        public string AccountHolder { get; set; }
        public string Description { get; set; }
    }
}
