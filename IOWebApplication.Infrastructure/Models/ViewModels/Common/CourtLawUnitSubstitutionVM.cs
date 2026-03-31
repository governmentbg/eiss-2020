using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtLawUnitSubstitutionVM
    {
        public int Id { get; set; }
        public int LawUnitId { get; set; }
        public string LawUnitName { get; set; }
        public int SubstituteLawUnitId { get; set; }
        public string SubstituteLawUnitName { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Description { get; set; }

        public bool IsSubstituted { get; set; }
    }

    public class CourtLawUnitSubstitutionFilter
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }
    }
}
