using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class ReportViewerService : BaseService, IReportViewerService
    {
        public ReportViewerService(
            ILogger<ReportViewerService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }
        public string ReportRequest_Insert(int reportId)
        {
            string result = "";
            try
            {
                ReportRequest request = new ReportRequest();
                request.CourtList = userContext.CourtId.ToString();
                request.ReportId = reportId;
                request.UserId = userContext.UserId;
                request.DateWrt = DateTime.Now;
                repo.Add<ReportRequest>(request);
                repo.SaveChanges();
                result = request.Id;

            }
            catch (Exception ex)
            {

                throw;
            }

            return result;
        }
        public IEnumerable<Report> Report_Select(int? courtTypeId)
        {
            Expression<Func<Report, bool>> selectedCourtType = x => true;
            if ((courtTypeId ?? 0) > 0)
                selectedCourtType = x => x.CourtTypeId == courtTypeId;

            return repo.AllReadonly<Report>().Where(selectedCourtType).AsQueryable();
        }

    }

}
