using IOWebApplicationService.Infrastructure.Data.DW;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IDWCaseService
    {
        Task CaseTransfer(DWCourt court);
        //bool CaseInsertUpdate(DWCourt court,DWCase currentCase);

        ////List<DWCaseLawUnit> DWCaseLowUnitInsertUpdate(List<DWCaseLawUnit> saved, List<DWCaseLawUnit> current);
        ////IEnumerable<DWCase> SelectCasesForTransfer(int selectedRowCount, DWCourt court);

        ////bool DWCaseLowUnitInsertUpdate(List<DWCaseLawUnit> savedList, List<DWCaseLawUnit> currentList);
        ////List<DWCaseLawUnit> SelectCaseLawUnitsTransfer(int caseId, int? caseSessionId);
        //// List<DWCaseLawUnit> SelectCaseLawUnitsSaved(int caseId, int? caseSessionId);

        ////bool MergeTransferedlawUnits(int ccaseId, int? caseSessionId);

        Task CaseLawUnitTransfer(DWCourt court, long CaseId);
        //IEnumerable<DWCaseLawUnit> SelectCaseLawUnitTransfer(DWCourt court,long caseId);
        //bool CaseLawUnitInsertUpdate(DWCaseLawUnit current);
        Task CaseLifecycleTransfer(DWCourt court);
        //bool CaseLifecycleInsertUpdate(DWCaseLifecycle current);
        //IEnumerable<DWCaseLifecycle> SelectCaseLifecycleTransfer(int selectedRowCount, DWCourt court);

        Task CasePersonTransfer(DWCourt court);
        //IEnumerable<DWCasePerson> SelectCasePersonTransfer(int selectedRowCount, DWCourt court);
        //bool CasePersonInsertUpdate(DWCourt court, DWCasePerson current);
        //string LinkRelationsString_Select(int personId);
    }
}
