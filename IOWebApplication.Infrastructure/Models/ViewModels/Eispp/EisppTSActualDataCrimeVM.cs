using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppTSActualDataCrimeVM
    {
        public string Sid { get; set; }
        public string CrimeNumber { get; set; }
        public string CrimeNameCode { get; set; }
        public string CrimeName { get; set; }
        public string CrimeLawbase { get; set; }
        public string CrimeDescription { get; set; }
    }
}
