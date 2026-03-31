using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IEesppService : IBaseMQService
    {
        Task<bool> FetchResult(int fetchCount);
    }
}
