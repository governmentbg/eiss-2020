using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class JudgeLoadActivityIndexVM
    {
        public int Id { get; set; }
        public string CourtTypeLabel { get; set; }
        public decimal LoadIndex { get; set; }
        public string IsActiveLabel { get; set; }
    }
}
