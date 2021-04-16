using IOWebApplicationService.Infrastructure.Data.Models.Base;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IDWService
    {
        void MigrateCases();
        void MigrateAllForCourt(int? courtId);
        DWCourt GetCourtData(int? courtId);
    }
}
