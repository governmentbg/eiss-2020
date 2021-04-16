using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IReportViewerService
    {

    string ReportRequest_Insert(int reportId);
    IEnumerable<Report> Report_Select(int? courtTypeId);
    }
}
