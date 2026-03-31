using IOWebApplicationService.Infrastructure.Data.DW;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
  public interface IDWSessionService
  {
    Task SessionTransfer(DWCourt court);
    //bool SessionInsertUpdate(DWCaseSession currentSession, DWCourt court);
    //IEnumerable<DWCaseSession> SelectCasesSessionForTransfer(int selectedRowCount, DWCourt court);

    //bool CaseSessionResultInsertUpdate(DWCaseSessionResult current);
    //IEnumerable<DWCaseSessionResult> SelectCaseSessionResultTransfer(long sessionId, DWCourt court);
    //void CaseSessionResultTransfer(DWCourt court, long SessionId);

    //bool CaseSessionLawUnitInsertUpdate(DWCaseSessionLawUnit current);
    //IEnumerable<DWCaseSessionLawUnit> SelectCaseSessionLawUnitTransfer(DWCourt court,long sessionId);
    //DWCaseSession CaseSessionLawUnitTransfer(DWCourt court, DWCaseSession session);
  }
}
