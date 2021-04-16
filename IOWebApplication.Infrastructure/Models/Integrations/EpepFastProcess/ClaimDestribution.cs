namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class ClaimDestribution
    {
        public string GUID { get; set; }

        /// <summary>
        /// GUID, Debtor identificator
        /// </summary>
        public string Debtors { get; set; }
        public decimal Amount { get; set; }
    }
}
