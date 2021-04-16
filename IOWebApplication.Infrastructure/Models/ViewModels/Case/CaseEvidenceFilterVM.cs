using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseEvidenceFilterVM
    {
        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Тип доказателство")]
        public int EvidenceTypeId { get; set; }
    }
}
