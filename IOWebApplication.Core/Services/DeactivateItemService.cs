using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
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
using System.Linq.Expressions;

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
                    {
                        Expression<Func<Document, bool>> filterDescription = x => true;
                        if (!string.IsNullOrEmpty(filter.SourceInfo))
                        {
                            filterDescription = x => x.DocumentNumber == filter.SourceInfo;
                        }

                        var documents = repo.AllReadonly<Document>()
                                        .Where(x => x.DateExpired != null)
                                        .Where(x => x.CourtId == userContext.CourtId);


                        if (userContext.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                        {

                            documents = documents.Union(repo.AllReadonly<Document>()
                                                   .Where(x => x.DateExpired != null)
                                                   .Where(x => x.CreatedCourtId == userContext.CourtId));
                        }

                        result = documents
                                        .Where(x => filter.SourceDateFrom.OrMinDate() <= x.DocumentDate && filter.SourceDateTo.OrMaxDate() >= x.DocumentDate)
                                        .Where(x => filter.DeactivateDateFrom.OrMinDate() <= x.DateExpired && filter.DeactivateDateTo.OrMaxDate() >= x.DateExpired)
                                        .Where(filterDescription)
                                        .Select(x => new DeactivateItemVM
                                        {
                                            SourceType = filter.SourceType,
                                            SourceId = x.Id,
                                            SourceInfo = $"{x.DocumentType.Label} {x.DocumentNumber} " + ((x.CourtId == NomenclatureConstants.Courts.RandomAssignment) ? " (ЦР)" : ""),
                                            SourceDate = x.DocumentDate,
                                            DeactivateUserName = x.UserExpired.LawUnit.FullName,
                                            DeactivateDate = x.DateExpired.Value,
                                            DeactivateDescription = x.DescriptionExpired
                                        });
                    }
                    break;
                case SourceTypeSelectVM.DeactivateAttachedFiles:
                    {
                        Expression<Func<Document, bool>> filterDescription = x => true;
                        if (!string.IsNullOrEmpty(filter.SourceInfo))
                        {
                            filterDescription = x => x.DocumentNumber == filter.SourceInfo;
                        }
                        var files = repo.AllReadonly<MongoFile>()
                                        .Where(x => x.DateExpired != null)
                                        .Where(x => x.SourceType == SourceTypeSelectVM.Document)
                                        .Where(x => filter.DeactivateDateFrom.OrMinDate() <= x.DateExpired && filter.DeactivateDateTo.OrMaxDate() >= x.DateExpired);
                        var tblDoc = repo.AllReadonly<Document>().Where(x => x.CourtId == userContext.CourtId).Where(filterDescription);

                        result = files.Join(
                            tblDoc,
                            a => a.SourceIdNumber,
                            a => a.Id,
                            (f, d) => new DeactivateItemVM
                            {
                                SourceType = filter.SourceType,
                                SourceId = f.Id,
                                SourceInfo = $"{d.DocumentType.Label} {d.DocumentNumber} : {f.FileName}",
                                SourceDate = f.DateUploaded,
                                DeactivateUserName = f.UserExpired.LawUnit.FullName,
                                DeactivateDate = f.DateExpired.Value,
                                DeactivateDescription = f.DescriptionExpired
                            });

                        // var fetched = result.Take(10).ToList();


                        //result = repo.AllReadonly<MongoFile>()
                        //                .Where(x => x.DateExpired != null)
                        //                .Where(x => filter.DeactivateDateFrom.OrMinDate() <= x.DateExpired && filter.DeactivateDateTo.OrMaxDate() >= x.DateExpired)
                        //                .Where(x => x.SourceType == SourceTypeSelectVM.Document)
                        //                .Select(f => new DeactivateItemVM
                        //                {
                        //SourceType = filter.SourceType,
                        //                    SourceId = f.Id,
                        //                    //SourceInfo = string.Concat(d.DocumentType.Label, " ", d.DocumentNumber, " : ", f.FileName),
                        //                    // SourceInfo = string.Concat(f.FileName),
                        //                    SourceDate = f.DateUploaded,
                        //                    //DeactivateUserName = f.UserExpired.LawUnit.FullName,
                        //                    DeactivateDate = f.DateExpired.Value,
                        //                    DeactivateDescription = f.DescriptionExpired
                        //                }).AsQueryable();

                        // var fetched = result.Take(10).ToList();


                        //result = (from f in files.Where(x => x.SourceType == SourceTypeSelectVM.Document)
                        //          join d in tbl on f.SourceIdNumber equals d.Id
                        //          where d.CourtId == userContext.CourtId
                        //          select new DeactivateItemVM
                        //          {
                        //              SourceType = filter.SourceType,
                        //              SourceId = f.Id,
                        //              SourceInfo = string.Concat(d.DocumentType.Label, " ", d.DocumentNumber, " : ", f.FileName),
                        //              SourceDate = f.DateUploaded,
                        //              DeactivateUserName = f.UserExpired.LawUnit.FullName,
                        //              DeactivateDate = f.DateExpired.Value,
                        //              DeactivateDescription = f.DescriptionExpired
                        //          });
                        //var tblN = repo.AllReadonly<CaseNotification>();
                        //var notificationFiles = (from f in files.Where(x => x.SourceType == SourceTypeSelectVM.CaseNotificationReturn)
                        //                         join n in tblN on f.SourceIdNumber equals n.Id
                        //                         where n.CourtId == userContext.CourtId
                        //                         select new DeactivateItemVM
                        //                         {
                        //                             SourceType = filter.SourceType,
                        //                             SourceId = f.Id,
                        //                             SourceInfo = string.Concat(f.Title, " : ", f.FileName),
                        //                             SourceDate = f.DateUploaded,
                        //                             DeactivateUserName = f.UserExpired.LawUnit.FullName,
                        //                             DeactivateDate = f.DateExpired.Value,
                        //                             DeactivateDescription = f.DescriptionExpired
                        //                         });
                        //result = result.Union(notificationFiles)
                        //               .AsQueryable();
                    }
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    {
                        Expression<Func<CaseNotification, bool>> filterDescription = x => true;
                        if (!string.IsNullOrEmpty(filter.SourceInfo))
                        {
                            filterDescription = x => EF.Functions.ILike(x.Case.RegNumber, filter.SourceInfo.ToCasePaternSearch());
                        }
                        result = repo.AllReadonly<CaseNotification>()
                                        .Where(x => x.DateExpired != null)
                                        .Where(x => x.CourtId == userContext.CourtId)
                                        .Where(filterDescription)
                                        .Select(x => new DeactivateItemVM
                                        {
                                            SourceType = filter.SourceType,
                                            SourceId = x.Id,
                                            SourceInfo = string.Concat(x.Case.CaseType.Code, " ", x.Case.RegNumber, "; ", x.NotificationType.Label, " ", x.RegNumber),
                                            SourceDate = x.RegDate,
                                            DeactivateUserName = x.UserExpired.LawUnit.FullName,
                                            DeactivateDate = x.DateExpired.Value,
                                            DeactivateDescription = x.DescriptionExpired
                                        });
                    }
                    break;

                case SourceTypeSelectVM.CaseSessionAct:
                    {
                        Expression<Func<CaseSessionAct, bool>> filterDescription = x => true;
                        if (!string.IsNullOrEmpty(filter.SourceInfo))
                        {
                            filterDescription = x => EF.Functions.ILike(x.Case.RegNumber, filter.SourceInfo.ToCasePaternSearch());
                        }
                        result = repo.AllReadonly<CaseSessionAct>()
                                        .Where(x => x.DateExpired != null)
                                        .Where(x => x.CourtId == userContext.CourtId)
                                        .Where(filterDescription)
                                        .Select(x => new DeactivateItemVM
                                        {
                                            SourceType = filter.SourceType,
                                            SourceId = x.Id,
                                            SourceInfo = string.Concat(x.Case.CaseType.Code ?? "", " ", x.Case.RegNumber, "; ", x.ActType.Label, " ", x.RegNumber ?? ""),
                                            SourceDate = (x.RegDate ?? x.DateWrt),
                                            DeactivateUserName = x.UserExpired.LawUnit.FullName,
                                            DeactivateDate = x.DateExpired.Value,
                                            DeactivateDescription = x.DescriptionExpired
                                        });
                    }
                    break;
                default:
                    return null;
            }
            Expression<Func<DeactivateItemVM, bool>> filterSourceDateFrom = x => true;
            if (filter.SourceDateFrom.HasValue)
            {
                filterSourceDateFrom = x => x.SourceDate > filter.SourceDateFrom.Value.Date;
            }
            Expression<Func<DeactivateItemVM, bool>> filterSourceDateTo = x => true;
            if (filter.SourceDateTo.HasValue)
            {
                filterSourceDateTo = x => x.SourceDate < filter.SourceDateTo.Value.MakeEndDate();
            }
            Expression<Func<DeactivateItemVM, bool>> filterDeactivatedDateFrom = x => true;
            if (filter.DeactivateDateFrom.HasValue)
            {
                filterDeactivatedDateFrom = x => x.DeactivateDate > filter.DeactivateDateFrom.Value;
            }
            Expression<Func<DeactivateItemVM, bool>> filterDeactivatedDateTo = x => true;
            if (filter.DeactivateDateTo.HasValue)
            {
                filterDeactivatedDateTo = x => x.DeactivateDate <= filter.DeactivateDateTo.Value.MakeEndDate();
            }
            Expression<Func<DeactivateItemVM, bool>> filterDeactivateUserName = x => true;
            if (!string.IsNullOrEmpty(filter.DeactivateUserName))
            {
                filterDeactivateUserName = x => EF.Functions.ILike(x.DeactivateUserName, filter.DeactivateUserName.ToPaternSearch());
            }
            //return result;

            return result
                         .Where(filterSourceDateFrom)
                         .Where(filterSourceDateTo)
                         .Where(filterDeactivatedDateFrom)
                         .Where(filterDeactivatedDateTo)
                         .Where(filterDeactivateUserName);
        }
    }
}
