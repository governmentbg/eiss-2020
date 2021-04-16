namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class Fee
    {
        public string GUID { get; set; }
        public int FeeType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
