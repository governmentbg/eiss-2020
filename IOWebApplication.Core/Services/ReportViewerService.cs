// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
  public class ReportViewerService : BaseService, IReportViewerService
  {
    public ReportViewerService(
        ILogger<ReportService> _logger,
        IRepository _repo,
        IUserContext _userContext)
    {
      logger = _logger;
      repo = _repo;
      userContext = _userContext;
    }
    public string ReportRequest_Insert(int reportId)
    { string result = "";
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
      if ((courtTypeId??0)>0)
              selectedCourtType= x => x.CourtTypeId==courtTypeId;

      return repo.AllReadonly<Report>().Where(selectedCourtType).AsQueryable();
    }
        
    }

}
