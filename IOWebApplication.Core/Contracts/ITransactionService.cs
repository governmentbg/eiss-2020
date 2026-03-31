using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ITransactionService : IBaseService
    {
        Task<SaveResultVM> AppendTransaction(int sourceType, long sourceId, int newOperationType, string message = null, long? newSourceId = null);
        Task<SaveResultVM> ContinueOperation(int sourceType);
        string GetOperationFailUrl(int sourceType);
        string GetOperationUrl(int sourceType, long sourceId);
        Task ReturnToWaiting(int fetchCount, int waitMinutes);
        Task<int> SelectWaiting(int sourceType);
        Task<SaveResultVM> TakeForOperation(int sourceType);
    }
}
