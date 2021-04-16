namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class Debtor
    {
        public string GUID { get; set; }
        public int Type { get; set; }
        public Person Person { get; set; }
        public Entity Entity { get; set; }
        public AddressType[] Addresses { get; set; }
        public Contact[] Contacts { get; set; }

        // No information for objects of type LegalRepresentative
        //public LegalRepresentative[] LegalRepresentatives { get; set; }
    }
}
