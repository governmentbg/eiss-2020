using IOWebApplicationService.Infrastructure.Data.DW;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Contracts
{
  public interface IDWCaseSelectionProtocolService
  {
    void CaseSelectionProtokolTransfer(DWCourt court);
    IEnumerable<DWCaseSelectionProtocol> SelectCaseSelectionProtokol(int selectedRowCount, DWCourt court);
    bool CaseSelectionProtocolInsertUpdate(DWCaseSelectionProtocol current, DWCourt court);

    void CaseSelectionProtocolCompartmentTransfer(DWCourt court);
    IEnumerable<DWCaseSelectionProtocolCompartment> SelectCaseSelectionProtokolCompartment(int selectedRowCount, DWCourt court);
    bool CaseSelectionProtocolCompartmentInsertUpdate(DWCaseSelectionProtocolCompartment current, DWCourt court);

    void CaseSelectionProtocolLawunitTransfer(DWCourt court);
    IEnumerable<DWCaseSelectionProtocolLawunit> SelectCaseSelectionProtokolLawUnit(int selectedRowCount, DWCourt court);
    bool CaseSelectionProtocolLawUnitInsertUpdate(DWCaseSelectionProtocolLawunit current, DWCourt court);

  }
}
