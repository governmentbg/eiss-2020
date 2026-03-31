using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IDWService
    {
        Task MigrateAllForCourt(int[] excludeCourtIds = null);
        DWCourt GetCourtData(int? courtId);
    }
}
