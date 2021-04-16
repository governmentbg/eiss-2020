using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class CaseLoadCorrectionActivityIndexVM
    {
        public int Id { get; set; }
        public string CaseInstanceLabel { get; set; }
        public decimal LoadIndex { get; set; }
    }
}
