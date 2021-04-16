using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public class RegixRequestReasonVM
    {
        public int? RegixReasonCaseId { get; set; }

        public long? RegixReasonDocumentId { get; set; }

        public string RegixReasonDescription { get; set; }

        public string RegixReasonGuid { get; set; }

        public int? RegixRequestTypeId { get; set; }

    }
}
