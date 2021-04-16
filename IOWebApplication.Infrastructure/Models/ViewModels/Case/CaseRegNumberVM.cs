using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseRegNumberVM
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public int CourtId { get; set; }
        public int Year { get; set; }
        public int CaseCharacterId { get; set; }
        public string ShortNumber { get; set; }
        public int ShortNumberInt { get; set; }
    }
}
