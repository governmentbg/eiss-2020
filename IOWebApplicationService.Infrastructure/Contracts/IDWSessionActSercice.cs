using IOWebApplicationService.Infrastructure.Data.DW;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Contracts
{
  public interface IDWSessionActService
  {
    void SessionActTransfer(DWCourt court);
    bool SessionActInsertUpdate(DWCaseSessionAct current);
    IEnumerable<DWCaseSessionAct> SelectCasesSessionActForTransfer(int selectedRowCount, DWCourt court);
    void SessionActComplainTransfer(DWCourt court);
    bool SessionActComplainInsertUpdate(DWCaseSessionActComplain current);
    IEnumerable<DWCaseSessionActComplain> SelectCasesSessionActComplainForTransfer(int selectedRowCount, DWCourt court);
    void SessionActComplainResultTransfer(DWCourt court);
    bool SessionActComplainResultInsertUpdate(DWCaseSessionActComplainResult current);
    IEnumerable<DWCaseSessionActComplainResult> SelectCasesSessionActComplainResultForTransfer(int selectedRowCount, DWCourt court);


    void SessionActComplainPersonTransfer(DWCourt court);
    bool SessionActComplainPersonInsertUpdate(DWCaseSessionActComplainPerson current);

    IEnumerable<DWCaseSessionActComplainPerson> SelectCasesSessionActComplainPersonForTransfer(int selectedRowCount, DWCourt court);

    void SessionActCoordinationTransfer(DWCourt court);
    bool SessionActCoordinationInsertUpdate(DWCaseSessionActCoordination current);
    IEnumerable<DWCaseSessionActCoordination> SelectCasesSessionActCoordinationTransfer(int selectedRowCount, DWCourt court);

    void SessionActDivorceTransfer(DWCourt court);
    bool SessionActDivorceInsertUpdate(DWCaseSessionActDivorce current);
    IEnumerable<DWCaseSessionActDivorce> SelectCasesSessionActDivorceForTransfer(int selectedRowCount, DWCourt court);

  }
}
