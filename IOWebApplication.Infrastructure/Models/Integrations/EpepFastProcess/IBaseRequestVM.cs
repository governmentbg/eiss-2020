
using System;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public interface IBaseRequestVM
    {
        BaseRequestPersonInfoVM[] LeftSide { get; set; }
        BaseRequestPersonInfoVM[] RightSide { get; set; }

        long DocumentId { get; set; }
        int CaseId { get; set; }
        string RequestTypeCode { get; set; }
        decimal TaxAmount { get; set; }
        string RequestFullTitle { get; set; }
        string RequestTitle { get; set; }
        bool HasRepresentatives { get; set; }

        void RecreateObject();
        List<ValidationErrorModel> ValidateRequest(bool isInEuro);
    }
}