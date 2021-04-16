using Integration.Eispp;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IEisppConnectionService
    {
        Task<EisppServiceClient> Connect();
    }
}
