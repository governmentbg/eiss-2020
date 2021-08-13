using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
  public interface ICourtLoadPeriodService : IBaseService
  {
    CourtLoadPeriod CourtLoadPeriod_GetById(long id, bool readOnly);
    bool MakeDaylyLoadPeriodLawuitRowsByGroup(CaseSelectionProtokolVM caseSelectionProtocol);
    bool MakeDaylyLoadPeriodLawuitRowsForLowUnit(CaseSelectionProtokolVM caseSelectionProtocol, int courtLoadPeriodId, int lawUnitId, bool IsDuty);

    bool MakeDaylyLoadPeriodLawuitRowsTotal(CaseSelectionProtokolVM caseSelectionProtocol, int courtLoadPeriodId, bool IsDuty);

    //bool MakeLoadPeriod(int? courtGroupid, int? courtDutyId);
    bool MakeLoadPeriodForAll();
    CourtLoadPeriod GetLoadPeriod(int? courtGroupid, int? courtDutyId);
    CaseSelectionProtokolVM CalculateAllKoef(CaseSelectionProtokolVM caseSelectionProtocol);
    void UpdateDailyLoadPeriod(int? CourtGroupId, int? CourtDutyId, int selectedLawUnit);
    void MergeCaseSelectionProtokolAndVM(CaseSelectionProtokol caseSelectionProtokol, CaseSelectionProtokolVM caseSelectionProtokolVM);
    IQueryable<CourtLoadResetPeriod> CourtLoadResetPeriod_Select(int CourtId);
    bool CourtLoadResetPeriod_SaveData(CourtLoadResetPeriod model);
    IEnumerable<CourtLoadResetPeriod> Get_CourtLoadResetPeriod_CrossPeriod( CourtLoadResetPeriod newPeriod);

    void UpdateDailyLoadPeriod_RemoveByDismisal(int case_lawunit_dismisal_id);
    CourtLoadPeriodLawUnit UpdateChangedProcentAverageCases(int lawunitId, int courtGroupId, decimal newPercent);
  }
}
