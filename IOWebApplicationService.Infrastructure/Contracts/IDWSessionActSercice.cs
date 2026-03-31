using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IDWSessionActService
    {
        Task SessionActTransfer(DWCourt court);
        //bool SessionActInsertUpdate(DWCaseSessionAct current);
        //IEnumerable<DWCaseSessionAct> SelectCasesSessionActForTransfer(int selectedRowCount, DWCourt court);
        Task SessionActComplainTransfer(DWCourt court);
        //bool SessionActComplainInsertUpdate(DWCaseSessionActComplain current);
        //IEnumerable<DWCaseSessionActComplain> SelectCasesSessionActComplainForTransfer(int selectedRowCount, DWCourt court);
        Task SessionActComplainResultTransfer(DWCourt court);
        //bool SessionActComplainResultInsertUpdate(DWCaseSessionActComplainResult current);
        //IEnumerable<DWCaseSessionActComplainResult> SelectCasesSessionActComplainResultForTransfer(int selectedRowCount, DWCourt court);


        Task SessionActComplainPersonTransfer(DWCourt court);

        Task SessionActCoordinationTransfer(DWCourt court);

        Task SessionActDivorceTransfer(DWCourt court);

    }
}
