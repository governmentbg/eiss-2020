using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppTSActualDataVM
    {
        public string EisppNumber { get; set; }
        public string PhazeName { get; set; }
        public string ProcessName { get; set; }
        public string ProcessDate { get; set; }

        public IList<EisppTSActualDataPersonVM> Persons { get; set; }
        public IList<EisppTSActualDataCrimeVM> Crimes { get; set; }
        public IList<EisppTSActualDataCaseVM> Cases { get; set; }

        public EisppTSActualDataVM()
        {
            Persons = new List<EisppTSActualDataPersonVM>();
            Crimes = new List<EisppTSActualDataCrimeVM>();
            Cases = new List<EisppTSActualDataCaseVM>();
        }
    }
}
