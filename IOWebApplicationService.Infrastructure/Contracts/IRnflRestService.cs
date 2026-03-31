using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IRnflRestService : IBaseMQService
    {
        Task TestClient();
    }
}
