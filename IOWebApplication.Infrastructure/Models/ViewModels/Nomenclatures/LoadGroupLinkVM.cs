using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class LoadGroupLinkVM
    {
        public int Id { get; set; }

        public string CourtTypeName { get; set; }

        public string CaseInstanceName { get; set; }

        public decimal LoadIndex { get; set; }

    }
}
