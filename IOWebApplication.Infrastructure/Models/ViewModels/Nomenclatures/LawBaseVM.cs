using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class LawBaseVM
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public string CourtTypeLabel { get; set; }
        public string CaseInstanceLabel { get; set; }
        public string CaseGroupLabel { get; set; }
        public bool IsActive { get; set; }
    }
}
