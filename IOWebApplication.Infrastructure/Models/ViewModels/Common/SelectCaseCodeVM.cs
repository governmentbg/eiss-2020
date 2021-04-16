using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class SelectCaseCodeVM
    {
        public string ContainerId { get; set; }
        public string CaseTypeId { get; set; }

        public string SelectCallback { get; set; }
    }
}
