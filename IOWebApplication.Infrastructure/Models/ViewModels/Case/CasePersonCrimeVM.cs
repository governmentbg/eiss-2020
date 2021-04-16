using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonCrimeVM
    {
        public int Id { get; set; }
        public string CasePersonName { get; set; }
        public string CaseCrimeLabel { get; set; }
        public string CaseCrimeCode { get; set; }
        public string RecidiveTypeLabel { get; set; }
        public string PersonRoleInCrimeLabel { get; set; }
    }
}
