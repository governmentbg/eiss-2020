using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IBankFileService : IBaseService
    {
        Task ReadBankFiles();
    }
}
