using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IEpepService : IBaseMQService
    {
        Task<bool> Correction();
    }
}
