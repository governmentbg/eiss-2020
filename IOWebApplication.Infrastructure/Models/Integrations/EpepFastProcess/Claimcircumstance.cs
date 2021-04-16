using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class Claimcircumstance
    {
        public string GUID { get; set; }
        public int CircumstanceType { get; set; }
        public string ContractNumber { get; set; }
        public DateTime Date { get; set; }
        public string Participants { get; set; }
        public int DifferentParticipantsFlag { get; set; }
        public string DifferentParticipantsReason { get; set; }
        public string Description { get; set; }
    }
}
