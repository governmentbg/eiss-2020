using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    public class ElectronicDocumentPayment
    {
        public Guid ElectronicDocumentId { get; set; }

        public string CourtCode { get; set; }

        public string CurrencyCode { get; set; }

        public int PaymentAmount { get; set; }

        public int PaymentKind { get; set; }

        public DateTime DatePaid { get; set; }
    }
}
