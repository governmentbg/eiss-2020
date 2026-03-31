using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.FastProcess;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IFastProcessSelectionCourtService : IBaseService
    {
        Task<bool> CreateSelectionForCourtByDate(int courtId, DateTime selection_date, string simulation = "0");
        Task<int> RandomizeCourtByDate(DateTime selection_date, int[] AvailableCourts, string simulation = "0");
        Task<int> RandomizeCourtByDateNoAvailableCourts(DateTime selection_date, string simulation = "0");

        Task<int[]> GetRegionalCourts();
        Task<bool> CheckIsCreatedSelectionDay(DateTime selection_date, string simulation = "0");
        Task<bool> UpdateCourtForSelectionDate(int courtId, DateTime selection_date, string simulation = "0");
        //Task< List<int>> CheckHasAvailabeJudgesInFastSelectionGroup(int courtID);
        Task<int> GetSelectedCourtForSelectionDate(DateTime selection_date, int[] available_courts, string simulation = "0");
        Task<bool> AddAfterZeroCourtForSelectionDat(DateTime selection_date, int[] available_courts, string simulation = "0");

        Task<FastProcessSelectionProtokolPreviewVM> FastProcessSelectionProtokol_Preview(int id);
        Task<CourtLoadPeriod> GetLoadPeriod(int courtGroupid);
        Task<int> MakeDaylyLoadPeriodLawuitRowsByGroup(CaseSelectionProtokol caseSelectionProtocol);
        Task<bool> FSMakeDaylyLoadPeriodLawuitRowsForLowUnit(CaseSelectionProtokol caseSelectionProtocol, int courtLoadPeriodId, int lawUnitId);
        Task<bool> FSMakeDaylyLoadPeriodLawuitRowsTotal(CaseSelectionProtokol caseSelectionProtocol, int courtLoadPeriodId);
        Task<IEnumerable<FastProcessSelectionProtokolLawUnitVM>> FastProcessLawUnit_LoadJudge(int caseId, int groupKind);
        Task<IEnumerable<FastProcessSelectionProtokolLawUnitVM>> FastProcessSetDataExcludeLawUnit(FastProcessSelectionProtokolLawUnitVM[] model, int caseId);
        Task<FastProcessProtocolInsertResultVM> FastProcessCreateSelectionProtocol(int CaseId);
        Task<int> GetSelectedAvailableCourtForFastSelection(int courtID, int groupKindId = 0, string string_date = "");
        Task<CaseSelectionProtokol> Fill_Case_Count(CaseSelectionProtokol protocol);
        Task<bool> Update_CourtLoadPeriodLawunit(int periodId, int selectedLawunitId, DateTime data);
        void FastProcessSetSelectedLawUnit_SaveCaseLawUnit(CaseSelectionProtokol model, int periodID, List<FastProcessSelectionProtokolLawUnitVM> lawunits_data);
        Task<bool> CreateInitializationSelectionForCourtByDate(DateTime selection_date, string simulation = "0");
        List<SelectListItem> SelectionMonths_ForDropDownList(int year);
        Task<List<SelectListItem>> SelectionYears_ForDropDownList();
        Task<FastProcessSelectionMonthsVM> GetFastSelectioForToday();
        Task<FastProcessSelectionCourtReportVM> FastProcessSelectionCorut_SelectForReport(FastProcessSelectionCourtReportFilterVM model);
        Task<bool> GenerateRandomForDateAndCount(string strindDate, int count);
    }
}