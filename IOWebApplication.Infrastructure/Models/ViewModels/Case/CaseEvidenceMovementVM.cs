using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseEvidenceMovementVM
    {
        public int Id { get; set; }
        public int CaseEvidenceId { get; set; }
        public string CaseEvidenceLabel { get; set; }
        public int EvidenceMovementTypeId { get; set; }
        public string EvidenceMovementTypeLabel { get; set; }
        public DateTime MovementDate { get; set; }
        public string ActDescription { get; set; }
        public string CaseSessionActName { get; set; }

        public string Description { get; set; }
    }
}
