using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class FastProcessSelectionCourtReportRowVM
    {


        public int CourtId { get; set; }
        public string CourtName { get; set; }

        public int? JudgeCount { get; set; }

        /// Базов коефициент (Натовареност на СЪД)
        public decimal? BazovKoefCourt { get; set; }

        /// Базов коефициент (Натовареност на съдия в СЪД)
        public decimal? BazovKoefCourtJudge { get; set; }

        /// Среден базов коефициент- на съдия (обща средна натовареност на районните съдилища) (съгласно чл.9, т. 1 от Правилата)
        public decimal? BazovKoefSredenALLCourtJudge { get; set; }

        public int YearSel { get; set; }

        public int MonthSel { get; set; }



        public int D01 { get; set; }
        public int D02 { get; set; }
        public int D03 { get; set; }
        public int D04 { get; set; }
        public int D05 { get; set; }
        public int D06 { get; set; }
        public int D07 { get; set; }
        public int D08 { get; set; }
        public int D09 { get; set; }
        public int D10 { get; set; }
        public int D11 { get; set; }
        public int D12 { get; set; }
        public int D13 { get; set; }
        public int D14 { get; set; }
        public int D15 { get; set; }
        public int D16 { get; set; }
        public int D17 { get; set; }
        public int D18 { get; set; }
        public int D19 { get; set; }
        public int D20 { get; set; }
        public int D21 { get; set; }
        public int D22 { get; set; }
        public int D23 { get; set; }
        public int D24 { get; set; }
        public int D25 { get; set; }
        public int D26 { get; set; }
        public int D27 { get; set; }
        public int D28 { get; set; }
        public int D29 { get; set; }
        public int D30 { get; set; }
        public int D31 { get; set; }
        public int StartTarget { get; set; }

        public int AddedAfterZero { get; set; }

        public int OkonchatelenTarget { get; set; }
        public int PolucheniDela { get; set; }

        public decimal NatovarbvenePolucheniDela { get; set; }
        public decimal NatovarbveneBazovNachalo_PolucheniDela { get; set; }
        public decimal? BazovKoefSredenALLVkraqCourtJudge { get; set; }
        public decimal BazovNaSydiaVkraq { get; set; }
        public int LeftForSelection { get; set; }
        public List<int> listOfDates { get; set; }




    }


}
