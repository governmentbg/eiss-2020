using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
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
                case (SourceTypeSelectVM.Files - SourceTypeSelectVM.Document):
                    {
                        var files = repo.AllReadonly<MongoFile>()
                                        .Include(x => x.UserExpired)
                                        .ThenInclude(x => x.LawUnit)
                                        .Where(x => x.DateExpired != null)
                                        .Where(x => x.SourceType == SourceTypeSelectVM.Document)
                                        .Where(x => filter.DeactivateDateFrom.OrMinDate() <= x.DateExpired && filter.DeactivateDateTo.OrMaxDate() >= x.DateExpired);
                        var tbl = repo.All<Document>().Include(x => x.DocumentType);

                        result = (from f in files
                                  join d in tbl on f.SourceIdNumber equals d.Id
                                  where d.CourtId == userContext.CourtId
                                  select new DeactivateItemVM
                                  {
                                      SourceType = filter.SourceType,
                                      SourceId = f.Id,
                                      SourceInfo = $"{d.DocumentType.Label} {d.DocumentNumber} : {f.FileName}",
                                      SourceDate = f.DateUploaded,
                                      DeactivateUserName = f.UserExpired.LawUnit.FullName,
                                      DeactivateDate = f.DateExpired.Value,
                                      DeactivateDescription = f.DescriptionExpired
                                  }).AsQueryable();
                    }
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    result = repo.AllReadonly<CaseNotification>()
                                    .Include(x => x.Case)
                                    .ThenInclude(x => x.CaseType)
                                    .Include(x => x.NotificationType)
                                    .Include(x => x.UserExpired)
                                    .ThenInclude(x => x.LawUnit)
                                    .Where(x => x.DateExpired != null)
                                    .Where(x => filter.DeactivateDateFrom.OrMinDate() <= x.DateExpired && filter.DeactivateDateTo.OrMaxDate() >= x.DateExpired)
                                    .Where(x => x.CourtId == userContext.CourtId)
                                    .Select(x => new DeactivateItemVM
                                    {
                                        SourceType = filter.SourceType,
                                        SourceId = x.Id,
                                        SourceInfo = $"{x.Case.CaseType.Code} {x.Case.RegNumber}; {x.NotificationType.Label} {x.RegNumber}",
                                        SourceDate = x.RegDate,
                                        DeactivateUserName = x.UserExpired.LawUnit.FullName,
                                        DeactivateDate = x.DateExpired.Value,
                                        DeactivateDescription = x.DescriptionExpired
                                    }).AsQueryable();
                    break;
                //case (SourceTypeSelectVM.Files - SourceTypeSelectVM.CaseNotification):
                //    {
                //        var files = repo.AllReadonly<MongoFile>()
                //                        .Include(x => x.UserExpired)
                //                        .ThenInclude(x => x.LawUnit)
                //                        .Where(x => x.DateExpired != null)
                //                        .Where(x => x.SourceType == SourceTypeSelectVM.CaseNotification)
                //                        .Where(x => filter.DeactivateDateFrom.OrMinDate() <= x.DateExpired && filter.DeactivateDateTo.OrMaxDate() >= x.DateExpired);
                //        var tbl = repo.All<CaseNotification>();

                //        result = (from f in files
                //                  from d in tbl
                //                  where d.Id == f.SourceIdNumber
                //                  select new DeactivateItemVM
                //                  {
                //                      SourceType = filter.SourceType,
                //                      SourceId = f.Id,
                //                      SourceInfo = $"{f.FileName}",
                //                      SourceDate = f.DateUploaded,
                //                      DeactivateUserName = f.UserExpired.LawUnit.FullName,
                //                      DeactivateDate = f.DateExpired.Value,
                //                      DeactivateDescription = f.DescriptionExpired
                //                  }).AsQueryable();
                //    }
                //    break;
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
