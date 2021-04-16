using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionResultVM
    {
        public int Id { get; set; }
        public int CaseSessionId { get; set; }
        public string SessionResultLabel { get; set; }
        public int SessionResultId { get; set; }
        public string SessionResultBaseLabel { get; set; }
        public string IsActiveText { get; set; }
        public string IsMainText { get; set; }
        public DateTime DateWrt { get; set; }
    }
}
