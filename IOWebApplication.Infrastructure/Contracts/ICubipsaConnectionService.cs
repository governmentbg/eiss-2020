using Integration.LegalActs;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface ICubipsaConnectionService
    {
        Task<LegalActsServiceClient> Connect();
    }
}
