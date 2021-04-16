using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSelectionProtokolListVM
    {
        public int Id { get; set; }

        public DateTime SelectionDate { get; set; }

        public string JudgeRoleName { get; set; }

        public string SelectionModeName { get; set; }

        public string SelectedLawUnitName { get; set; }

    public string SelectionProtokolStateName { get; set; }

  }
}
