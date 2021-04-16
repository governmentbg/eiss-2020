using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonLinkSideItemVM
    {
        public int Id { get; set; }
        public bool IsChecked { get; set; }

        public string PersonName { get; set; }
    }
}
