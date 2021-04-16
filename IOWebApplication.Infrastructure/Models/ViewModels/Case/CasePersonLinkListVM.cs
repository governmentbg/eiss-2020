using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CasePersonLinkListVM
    {
        public int Id { get; set; }

        public string CasePersonName { get; set; }

        public string LinkDirectionName { get; set; }

        public string CasePersonRelName { get; set; }

        public string LinkDirectionSecondName { get; set; }

        public string CasePersonSecondRelName { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

    }
}
