// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Data.DW;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Contracts
{
  public interface IDWSessionService
  {
    void SessionTransfer(DWCourt court);
    bool SessionInsertUpdate(DWCaseSession currentSession, DWCourt court);
    IEnumerable<DWCaseSession> SelectCasesSessionForTransfer(int selectedRowCount, DWCourt court);

    bool CaseSessionResultInsertUpdate(DWCaseSessionResult current);
    IEnumerable<DWCaseSessionResult> SelectCaseSessionResultTransfer(long sessionId, DWCourt court);
    void CaseSessionResultTransfer(DWCourt court, long SessionId);

    bool CaseSessionLawUnitInsertUpdate(DWCaseSessionLawUnit current);
    IEnumerable<DWCaseSessionLawUnit> SelectCaseSessionLawUnitTransfer(DWCourt court,long sessionId);
    DWCaseSession CaseSessionLawUnitTransfer(DWCourt court, DWCaseSession session);
  }
}
