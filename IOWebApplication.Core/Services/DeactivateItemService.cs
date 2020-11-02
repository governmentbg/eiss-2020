// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace IOWebApplication.Core.Services
{
    public class DeactivateItemService : BaseService, IDeactivateItemService
    {

        public DeactivateItemService(
            ILogger<DeactivateItemService> _logger,
            IRepository _repo,
            IUserContext _userContext
            )
        {
            this.logger = _logger;
            this.repo = _repo;
            this.userContext = _userContext;
        }
        public IQueryable<DeactivateItemVM> Select(DeactivateItemFilterVM filter)
        {
            IQueryable<DeactivateItemVM> result = null;
            switch (filter.SourceType)
            {
                case SourceTypeSelectVM.Document:
                    result = repo.AllReadonly<Document>()
                                    .Include(x => x.DocumentType)
                                    .Include(x => x.UserExpired)
                                    .ThenInclude(x => x.LawUnit)
                                    .Where(x => x.DateExpired != null)
                                    .Where(x => filter.DeactivateDateFrom.OrMinDate() <= x.DateExpired && filter.DeactivateDateTo.OrMaxDate() >= x.DateExpired)
                                    .Where(x => x.CourtId == userContext.CourtId)
                                    .Select(x => new DeactivateItemVM
                                    {
                                        SourceType = filter.SourceType,
                                        SourceId = x.Id,
                                        SourceInfo = $"{x.DocumentType.Label} {x.DocumentNumber}",
                                        SourceDate = x.DocumentDate,
                                        DeactivateUserName = x.UserExpired.LawUnit.FullName,
                                        DeactivateDate = x.DateExpired.Value,
                                        DeactivateDescription = x.DescriptionExpired
                                    }).AsQueryable();
                    break;
                case SourceTypeSelectVM.CaseSessionAct:
                    result = repo.AllReadonly<CaseSessionAct>()
                                    .Include(x => x.Case)
                                    .ThenInclude(x => x.CaseType)
                                    .Include(x => x.ActType)
                                    .Include(x => x.UserExpired)
                                    .ThenInclude(x => x.LawUnit)
                                    .Where(x => x.DateExpired != null)
                                    .Where(x => filter.DeactivateDateFrom.OrMinDate() <= x.DateExpired && filter.DeactivateDateTo.OrMaxDate() >= x.DateExpired)
                                    .Where(x => x.CourtId == userContext.CourtId)
                                    .Select(x => new DeactivateItemVM
                                    {
                                        SourceType = filter.SourceType,
                                        SourceId = x.Id,
                                        SourceInfo = $"{x.Case.CaseType.Code} {x.Case.RegNumber}; {x.ActType.Label} {x.RegNumber}",
                                        SourceDate = (x.RegDate ?? x.DateWrt),
                                        DeactivateUserName = x.UserExpired.LawUnit.FullName,
                                        DeactivateDate = x.DateExpired.Value,
                                        DeactivateDescription = x.DescriptionExpired
                                    }).AsQueryable();
                    break;
                default:
                    return null;
            }
            return result.Where(x => filter.SourceDateFrom.OrMinDate() <= x.SourceDate && filter.SourceDateTo.OrMaxDate() >= x.SourceDate)
                         .Where(x => EF.Functions.ILike(x.SourceInfo, filter.SourceInfo.ToPaternSearch()))
                         .Where(x => EF.Functions.ILike(x.DeactivateUserName, filter.DeactivateUserName.ToPaternSearch()));
        }
    }
}
