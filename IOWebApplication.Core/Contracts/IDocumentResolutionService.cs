using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using System.Linq;

namespace IOWebApplication.Core.Contracts
{
    public interface IDocumentResolutionService : IBaseService
    {
        IQueryable<DocumentResolutionVM> Select(DocumentResolutionFilterVM filter);
        IQueryable<DocumentResolutionVM> Select(long documentId, long? id = null);
        SaveResultVM SaveData(DocumentResolution model);
        SaveResultVM Register(DocumentResolution model);
        SaveResultVM UpdateAfterSign(long id);
        SaveResultVM ResolutionExpire(ExpiredInfoVM model);
        (bool canAccess, string lawunitName) CheckActBlankAccess(bool forBlank, long id, DocumentResolution model = null);
        SaveResultVM UpdateActCreator(long id);
        //------------------------------------------------------
        IQueryable<DocumentResolutionCaseVM> SelectCasesByResolution(long documentResolutionId);
        bool AppendCaseToResolution(long documentResolutionId, int caseId);
        bool RemoveCaseToResolution(long documentResolutionCaseId);
    }
}
