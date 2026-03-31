using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class TransactionService : BaseService, ITransactionService
    {
        private readonly IUrlHelper urlHelper;
        public TransactionService(IRepository _repo,
                               IUserContext _userContext,
                               IUrlHelper _urlHelper)
        {
            repo = _repo;
            urlHelper = _urlHelper;
            userContext = _userContext;
        }

        public string GetOperationUrl(int sourceType, long sourceId)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.ElectronicDocument:
                    return urlHelper.Action("AddFromElectronicDocument", "Document", new { electronicDocumentId = sourceId });
                default:
                    return string.Empty;
            }
        }
        public string GetOperationFailUrl(int sourceType)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.ElectronicDocument:
                    return urlHelper.Action("Index", "Document");
                default:
                    return string.Empty;
            }
        }

        public async Task<int> SelectWaiting(int sourceType)
        {
            var newItems = await repo.AllReadonly<MainGroup>()
                                    .Where(x => x.CourtId == userContext.CourtId && x.SourceType == sourceType)
                                    .Where(x => x.LastTransationId == null)
                                    .CountAsync().ConfigureAwait(false);

            var returnedItems = await repo.AllReadonly<MainGroup>()
                                    .Where(x => x.CourtId == userContext.CourtId && x.SourceType == sourceType)
                                    .Where(x => x.LastTransationId != null)
                                    .Where(x => x.LastTransation.OperationTypeId == NomenclatureConstants.MainTransactionTypes.Waiting)
                                    .CountAsync().ConfigureAwait(false);
            return newItems + returnedItems;
        }

        public async Task<bool> CheckFinishedOperation(int sourceType, long sourceId)
        {
            bool isFinished = await repo.AllReadonly<MainGroup>()
                                        .Include(x => x.LastTransation)
                                        .Where(x => x.CourtId == userContext.CourtId && x.SourceType == sourceType && x.SourceId == sourceId)
                                        .Where(x => x.LastTransationId != null &&
                                        x.LastTransation.OperationTypeId == NomenclatureConstants.MainTransactionTypes.Finish)
                                        .AnyAsync().ConfigureAwait(false);
            return isFinished;
        }

        public async Task<SaveResultVM> ContinueOperation(int sourceType)
        {
            long objectId = await repo.AllReadonly<MainGroup>()
                                        .Include(x => x.LastTransation)
                                        .Where(x => x.CourtId == userContext.CourtId && x.SourceType == sourceType)
                                        .Where(x => x.LastTransationId != null &&
                                        x.LastTransation.OperationTypeId == NomenclatureConstants.MainTransactionTypes.InUser &&
                                        x.LastTransation.UserId == userContext.UserId)
                                        .OrderBy(x => x.Id)
                                        .Select(x => x.SourceId)
                                        .FirstOrDefaultAsync().ConfigureAwait(false);

            if (objectId > 0)
            {
                return new SaveResultVM()
                {
                    Result = true,
                    ObjectId = objectId
                };
            }
            else
            {
                return new SaveResultVM(false);
            }
        }

        public Task<SaveResultVM> AppendTransaction(int sourceType, long sourceId, int newOperationType, string message = null, long? newSourceId = null)
        {
            return appendTransactionInternal(userContext.UserId, sourceType, sourceId, newOperationType, message, newSourceId);
        }
        private async Task<SaveResultVM> appendTransactionInternal(string userId, int sourceType, long sourceId, int newOperationType, string message = null, long? newSourceId = null)
        {
            try
            {

                var mainGroup = await repo.All<MainGroup>()
                                            .Include(x => x.LastTransation)
                                            .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
                                            .OrderBy(x => x.Id)
                                            .FirstOrDefaultAsync();

                if (mainGroup.LastTransation != null && mainGroup.LastTransation.OperationTypeId == NomenclatureConstants.MainTransactionTypes.Finish)
                {
                    return new SaveResultVM(true);
                }

                var newOper = new MainTransaction()
                {
                    MainGroupId = mainGroup.Id,
                    DateWrt = DateTime.Now,
                    OperationTypeId = newOperationType,
                    UserId = userId,
                    Message = message,
                    PriorOperationTypeId = (mainGroup.LastTransation != null) ? mainGroup.LastTransation.OperationTypeId : NomenclatureConstants.MainTransactionTypes.Waiting
                };
                repo.Add(newOper);
                await repo.SaveChangesAsync();
                mainGroup.LastTransationId = newOper.Id;
                if (newSourceId > 0)
                {
                    mainGroup.SourceId = newSourceId.Value;
                }
                await repo.SaveChangesAsync();
                return new SaveResultVM(true);

            }
            catch (Exception ex)
            {
                return new SaveResultVM(false);
            }
        }

        public async Task<SaveResultVM> TakeForOperation(int sourceType)
        {
            var continueResult = await ContinueOperation(sourceType).ConfigureAwait(false);
            if (continueResult.Result)
            {
                return continueResult;
            }
            long objectId = 0;
            using (var ts = repo.BeginTransaction())
            {
                var firstWaiting = await repo.All<MainGroup>()
                                            .Include(x => x.LastTransation)
                                            .Where(x => x.CourtId == userContext.CourtId && x.SourceType == sourceType)
                                            .Where(x => (x.LastTransationId == null) || (x.LastTransationId != null && x.LastTransation.OperationTypeId == NomenclatureConstants.MainTransactionTypes.Waiting))
                                            .OrderBy(x => x.Id)
                                            .FirstOrDefaultAsync().ConfigureAwait(false);

                if (firstWaiting == null)
                {
                    return new SaveResultVM(false);
                }

                var newOper = new MainTransaction()
                {
                    MainGroupId = firstWaiting.Id,
                    DateWrt = DateTime.Now,
                    OperationTypeId = NomenclatureConstants.MainTransactionTypes.InUser,
                    UserId = userContext.UserId,
                    PriorOperationTypeId = (firstWaiting.LastTransation != null) ? firstWaiting.LastTransation.OperationTypeId : NomenclatureConstants.MainTransactionTypes.InUser
                };
                repo.Add(newOper);
                await repo.SaveChangesAsync().ConfigureAwait(false);
                firstWaiting.LastTransationId = newOper.Id;
                await repo.SaveChangesAsync().ConfigureAwait(false);

                ts.Commit();
                objectId = firstWaiting.SourceId;
            }
            if (objectId > 0)
            {
                return new SaveResultVM()
                {
                    Result = true,
                    ObjectId = objectId
                };
            }
            else
            {
                return new SaveResultVM(false);
            }
        }

        public async Task ReturnToWaiting(int fetchCount, int waitMinutes)
        {
            var dtNow = DateTime.Now.AddMinutes(-waitMinutes);
            var waitingTransactions = await repo.AllReadonly<MainGroup>()
                                                .Include(x => x.LastTransation)
                                                .Where(x => x.LastTransationId != null)
                                                .Where(x => x.LastTransation.OperationTypeId == NomenclatureConstants.MainTransactionTypes.InUser)
                                                .Where(x => x.LastTransation.DateWrt < dtNow)
                                                .OrderBy(x => x.LastTransation.DateWrt)
                                                .Select(x => new
                                                {
                                                    x.Id,
                                                    x.SourceType,
                                                    x.SourceId
                                                }).Take(fetchCount)
                                                .ToListAsync().ConfigureAwait(false);

            foreach (var item in waitingTransactions)
            {
                bool forReset = true;
                switch (item.SourceType)
                {
                    case SourceTypeSelectVM.ElectronicDocument:
                        forReset = !(await repo.AllReadonly<Document>().Where(x => x.ElectronicDocumentId == item.SourceId).AnyAsync().ConfigureAwait(false));
                        break;
                    default:
                        break;
                }

                if (forReset)
                {
                    var res = await appendTransactionInternal(null, item.SourceType, item.SourceId, NomenclatureConstants.MainTransactionTypes.Waiting, $"Автоматично върнато в опашката").ConfigureAwait(false);
                }
            }
        }
    }
}
