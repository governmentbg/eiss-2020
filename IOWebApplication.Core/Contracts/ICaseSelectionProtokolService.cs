using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.Integrations.CSRD;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
  public interface ICaseSelectionProtokolService : IBaseService
  {
    IQueryable<CaseSelectionProtokolListVM> CaseSelectionProtokol_Select(int caseId);

    bool CaseSelectionProtokol_SaveData(CaseSelectionProtokolVM model, ref string errorMessage);

    int CaseSelectionProtokolLawUnit_SelectCount(int caseId);

    IEnumerable<CaseSelectionProtokolLawUnitVM> LawUnit_LoadJudge(int courtGroupId, int caseId, int courtId, int judgeRoleId);
    IEnumerable<CaseSelectionProtokolLawUnitVM> LawUnit_LoadJury(int courtId,int caseId, int? specialityId);
    IEnumerable<CaseSelectionProtokolLawUnitVM> LawUnit_LoadByCourtDutyId(int courtDutyId, int caseId, int courtId, int judgeRoleId);

    CaseSelectionProtokolPreviewVM CaseSelectionProtokol_Preview(int id);

    IQueryable<CaseSelectionProtokolReportVM> CaseSelectionProtokol_SelectForReport(int courtId, CaseSelectionProtokolFilterVM model);


    IEnumerable<CaseSelectionProtokolLawUnitVM> LawUnit_LoadJudgeByCaseGroup(int courtId, CheckListVM[] CaseGroups, string lawUnitsIds, int caseId,int judgeRole);
    int[] CaseGroup_WithLawUnits(int courtId, string idStr);
    List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Return_Available_CaseGroup_forAdditionalSelect(string idStr, string groups, int caseId);

    List<SelectListItem> AvailableJudgeRolesForFelect_SelectForDropDownList(int caseId);
    int Select_Create_AddJuryProtocol(CaseSelectionProtokolVM model, ref string errorMessage);
    //IEnumerable<CaseSelectionProtokolLawUnitPreviewVM> GetComparentmentJudges(int case_id, int judge_id);
    bool CaseSelectionProtokol_UpdateBeforeDocForSign(CaseSelectionProtokolPreviewVM model);
    bool CaseSelectionProtokol_UpdateBeforeAfterSign(int id);
    bool CaseLowUnitsFroSelectionProtocom_insert(int id);
    bool HsaUnsignedProtocol(int id);
    List<SelectListItem> GetJudgeComprentmetList(int lawunitId, int courtId, int case_id);

    int[] LawUnitsFromConectedCases(int caseId);
    IQueryable<JuryYearDays> JuryYearDays_Select(int courtId, int year, DateTime? DateFrom, DateTime? DateTo, int? LawUnitId);

    int TakeCaseSelectionProtocolLockNumber(int? courtGroupid, int? courtDutyId);
    int TakeCaseSelectionProtocolMinLockNumber(int? courtGroupid, int? courtDutyId);
    bool FinishLockNumber(int lockId);
    int[] GetActiveLawUnits(int caseId);
    AssignmentRequestModel GetAssignmentRequestModel(int protocolId);
    IQueryable<CaseSelectionProtokolLawUnitPreviewVM> LawUnitReportByGroup(int courtId, int courtGropId, int? LawUnitId);
    List<SelectListItem> GetCourtGroups(int courtId);
    bool HasJudgeChoiceWithoutDismisal(int courtId, int caseID);
    List<SelectListItem> SelectSelectionMode_ForDropDownList();
    List<SelectListItem> SelectJudgeRole_ForDropDownList();
     Int32 IfJuryReturnCaseIdToRedirect(int id);
    CaseSessionMeeting NextCaseOpenSessionMeeting(int case_id, DateTime dateAfterSearch);
    IQueryable<CaseSelectionProtokolCaseInGroupsVM> CaseSelectionProtokolCaseInGroups(int courtId);
  }
}
