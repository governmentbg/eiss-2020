// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplicationService.Infrastructure.Constants;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Data.StatisticsReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ZXing;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class StatisticsReportService : IStatisticsReportService
    {
        protected IRepository repo;

        public StatisticsReportService(IRepository _repo)
        {
            repo = _repo;
        }

        private int[] SessionResultGrouping_Select(int groupId)
        {
            return repo.AllReadonly<SessionResultGrouping>()
                                      .Where(x => x.SessionResultGroup == groupId)
                                      .Select(x => x.SessionResultId)
                                      .ToArray();
        }

        private void RS_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData)
        {
            int searchCourtId = 0;
            List<StatisticsCourtTypeCaseTypeVM> courtTypeCaseTypesCols = nomData.courtTypeCaseTypesCols
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt).ToList();

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt).ToList();

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt).ToList();

            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults = nomData.excelReportComplainResults
                                              .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt).ToList();

            SheetCaseCount(fromDate, toDate, searchCourtId, templateId, 
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, 
                 NomenclatureConstants.CourtType.RegionalCourt, courtTypeCaseTypesCols, 4, 8);
            SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                   new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo }, 
                   NomenclatureConstants.CourtType.RegionalCourt, courtTypeCaseTypesCols, 6, 9);

            SheetActIndex(fromDate, toDate, searchCourtId, templateId, 
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, 
                 NomenclatureConstants.CourtType.RegionalCourt, excelReportIndexCols, 5, 8);
            SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                   new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo }, 
                   NomenclatureConstants.CourtType.RegionalCourt, excelReportIndexCols, 7, 7);

            //Sheet 2 Приложение ГД
            RSSheet2(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList(),
                excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList());

            //Sheet 3 Приложение НД
            RSSheet3(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 3).ToList());
        }

        private void OS_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData)
        {
            int searchCourtId = 0;

            List<StatisticsCourtTypeCaseTypeVM> courtTypeCaseTypesCols = nomData.courtTypeCaseTypesCols
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt).ToList();

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt).ToList();

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt).ToList();

            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults = nomData.excelReportComplainResults
                                              .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt).ToList();

            SheetCaseCount(fromDate, toDate, searchCourtId, templateId, 
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, 
                 NomenclatureConstants.CourtType.DistrictCourt, courtTypeCaseTypesCols, 6, 8);
            SheetCaseCount(fromDate, toDate, searchCourtId, templateId, 
                new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade,
                             NomenclatureConstants.CaseGroups.Company}, 
                   NomenclatureConstants.CourtType.DistrictCourt, courtTypeCaseTypesCols, 8, 8);

            SheetActIndex(fromDate, toDate, searchCourtId, templateId, 
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, 
                 NomenclatureConstants.CourtType.DistrictCourt, excelReportIndexCols, 7, 7);
            SheetActIndex(fromDate, toDate, searchCourtId, templateId, 
                new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade,
                             NomenclatureConstants.CaseGroups.Company}, 
                   NomenclatureConstants.CourtType.DistrictCourt, excelReportIndexCols, 9, 7);

            //Sheet 2 Приложение ГД, ТД, ФД
            OSSheet2(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList(),
                excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList());

            //Sheet 4 Приложение НД
            OSSheet4(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 4).ToList());
        }

        private void AP_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData)
        {
            int searchCourtId = 0;

            List<StatisticsCourtTypeCaseTypeVM> courtTypeCaseTypesCols = nomData.courtTypeCaseTypesCols
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal).ToList();

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal).ToList();

            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults = nomData.excelReportComplainResults
                                              .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal).ToList();

            SheetCaseCount(fromDate, toDate, searchCourtId, templateId, new int[] 
              { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, NomenclatureConstants.CourtType.Apeal,
              courtTypeCaseTypesCols, 4, 8);
            SheetCaseCount(fromDate, toDate, searchCourtId, templateId, 
                    new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade}, 
                    NomenclatureConstants.CourtType.Apeal, courtTypeCaseTypesCols, 6, 8);

            SheetActIndex(fromDate, toDate, searchCourtId, templateId, new int[] 
              { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, NomenclatureConstants.CourtType.Apeal,
              excelReportIndexCols, 5, 7);
            SheetActIndex(fromDate, toDate, searchCourtId, templateId, 
                    new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade}, 
                    NomenclatureConstants.CourtType.Apeal, excelReportIndexCols, 7, 7);

            //Sheet 2 Приложение ГД/ТД
            ApealSheet2(fromDate, toDate, searchCourtId, templateId, excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList());

            //Sheet 3 Приложение ГД/ТД
            ApealSheet3(fromDate, toDate, searchCourtId, templateId, excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList());
        }

        private void Millitary_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData)
        {
            int searchCourtId = 0;

            List<StatisticsCourtTypeCaseTypeVM> courtTypeCaseTypesCols = nomData.courtTypeCaseTypesCols
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Millitary).ToList();

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Millitary).ToList();

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Millitary).ToList();

            SheetCaseCount(fromDate, toDate, searchCourtId, templateId, 
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, 
                NomenclatureConstants.CourtType.Millitary,
                courtTypeCaseTypesCols, 3, 8);

            SheetActIndex(fromDate, toDate, searchCourtId, templateId, 
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, 
                NomenclatureConstants.CourtType.Millitary,
                excelReportIndexCols, 4, 7);

            //Sheet 2 Приложение НД
            MillitarySheet2(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList());
        }

        private void MillitaryAP_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData)
        {
            int searchCourtId = 0;

            List<StatisticsCourtTypeCaseTypeVM> courtTypeCaseTypesCols = nomData.courtTypeCaseTypesCols
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.MillitaryApeal).ToList();

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.MillitaryApeal).ToList();

            SheetCaseCount(fromDate, toDate, searchCourtId, templateId, 
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, 
                NomenclatureConstants.CourtType.MillitaryApeal, courtTypeCaseTypesCols, 3, 8);

            SheetActIndex(fromDate, toDate, searchCourtId, templateId, 
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, 
                NomenclatureConstants.CourtType.MillitaryApeal, excelReportIndexCols, 4, 7);
        }

        public void StatisticsTest()
        {
            DateTime fromDate = new DateTime(2020, 1, 1);
            DateTime toDate = new DateTime(2020, 6, 30);

            //Изтриване на данните за месец/година
            repo.DeleteRange<ExcelReportData>(x => x.ReportMonth == toDate.Month && x.ReportYear == toDate.Year);
            repo.SaveChanges();

            StatisticsNomDataVM nomData = new StatisticsNomDataVM();

            var courtTypeCaseTypes = repo.AllReadonly<CourtTypeCaseType>().ToList();
            nomData.courtTypeCaseTypesCols = FillCourtTypeCaseTypeCols(courtTypeCaseTypes);

            var reportTemplates = repo.AllReadonly<ExcelReportTemplate>()
                     .Where(x => x.DateFrom <= toDate && (x.DateTo ?? DateTime.MaxValue) >= toDate)
                     .ToList();

            nomData.excelReportIndexCols = repo.AllReadonly<ExcelReportIndex>()
                                    .Select(x => new StatisticsExcelReportIndexVM()
                                    {
                                        CourtTypeId = x.CourtTypeId,
                                        CaseGroupId = x.CaseGroupId,
                                        CaseGroupCaseTypeIds = x.CaseGroup.CaseTypes.Select(a => a.Id).ToList(),
                                        CaseTypeIds = x.CaseTypeId,
                                        ActTypeIds = x.ActTypeId,
                                        ActComplainIndexIds = x.ActComplainIndex,
                                        Col = x.ColIndex
                                    }).ToList();

            nomData.excelReportComplainResults = repo.AllReadonly<ExcelReportComplainResult>()
                                    .Select(x => new StatisticsExcelReportComplainIndexVM()
                                    {
                                        CourtTypeId = x.CourtTypeId,
                                        SheetIndex = x.SheetIndex,
                                        ActComplainResultIds = x.ActComplainResult,
                                        Col = x.ColIndex
                                    }).ToList();

            nomData.excelReportCaseCodeRows = repo.AllReadonly<ExcelReportCaseCodeRow>()
                                    .Select(x => new StatisticsExcelReportCaseCodeRowVM()
                                    {
                                        CourtTypeId = x.CourtTypeId ?? 0,
                                        SheetIndex = x.SheetIndex,
                                        CaseCodeIds = x.CaseCodeId,
                                        RowIndex = x.RowIndex
                                    }).ToList();

            RS_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                .Select(x => x.Id).FirstOrDefault(), nomData);

            OS_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
                .Select(x => x.Id).FirstOrDefault(), nomData);

            AP_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal)
                .Select(x => x.Id).FirstOrDefault(), nomData);

            Millitary_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Millitary)
                .Select(x => x.Id).FirstOrDefault(), nomData);

            MillitaryAP_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.MillitaryApeal)
                .Select(x => x.Id).FirstOrDefault(), nomData);

            repo.SaveChanges();
        }

        public ExcelReportData InsertExcelReportData(int court_id, int reportTemplateId, int reportYear, int reportMonth, int sheetIndex, int rowIndex, int colIndex, string cellValue)
        {
            ExcelReportData exd = new ExcelReportData();
            try
            {
                exd.CourtId = court_id;
                exd.DateWrt = DateTime.Now;
                exd.ExcelReportTemplateId = reportTemplateId;
                exd.ReportYear = reportYear;
                exd.ReportMonth = reportMonth;
                exd.SheetIndex = sheetIndex;
                exd.RowIndex = rowIndex;
                exd.ColIndex = colIndex;
                exd.CellValue = cellValue;
                //exd.RowDataColIndex = RowDataColIndex;
                //exd.RowData = rowData;
                repo.Add<ExcelReportData>(exd);
            }
            catch (Exception)
            {


            }
            return exd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypes"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="reportType"></param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseLawUnitCaseType_Select(int courtTypeId, int courtId, int[] caseGroupIds, 
            DateTime fromDate, DateTime toDate, int reportType, List<StatisticsCourtTypeCaseTypeVM> courtTypeCaseTypesCols)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => true;
            if (reportType == StatisticsConstants.ReportTypes.Unfinished)
                reportTypeWhere = x => x.DateFrom.Date < fromDate.Date && (x.DateTo ?? dateEnd).Date >= fromDate.Date;
            else if (reportType == StatisticsConstants.ReportTypes.Incoming)
                reportTypeWhere = x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date;
            else if (reportType == StatisticsConstants.ReportTypes.Finished3months)
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date && 
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && x.DurationMonths <= 3;
            else if (reportType == StatisticsConstants.ReportTypes.FinishedStop || reportType == StatisticsConstants.ReportTypes.FinishedNoStop)
            {
                var actComplainResults = repo.AllReadonly<ActComplainResultGrouping>()
               .Where(x => x.ActComplainResultGroup == NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop)
               .Select(x => x.ActComplainResultId)
               .ToArray();

                var actSessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsStopCase);

                if (reportType == StatisticsConstants.ReportTypes.FinishedStop)
                {
                    reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                        (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                        (actComplainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) ||
                                         x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.IsMain && a.IsActive &&
                                              actSessionResults.Contains(a.SessionResultId)).Any());
                }
                else if (reportType == StatisticsConstants.ReportTypes.FinishedNoStop)
                {
                    reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                        (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                        !(actComplainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) ||
                                         x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.IsMain && a.IsActive &&
                                              actSessionResults.Contains(a.SessionResultId)).Any());
                }
            }

            var result = repo.AllReadonly<CaseLifecycle>()
                                .Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelCol = courtTypeCaseTypesCols.Where(a => a.CaseTypeId == x.Case.CaseTypeId &&
                                               a.ReportTypeId == reportType)
                                               .Select(a => a.Col)
                                               .FirstOrDefault(),
                                    LawUnitData = x.CaseSessionActId != null ?
                                               x.CaseSessionAct.CaseSession.CaseLawUnits
                                              .Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                              .Where(a => (a.DateTo ?? dateEnd) >= x.CaseSessionAct.CaseSession.DateFrom)
                                              .OrderByDescending(a => a.DateFrom)
                                              .Select(a => a.LawUnitId + ",," + a.LawUnit.FullName)
                                              .FirstOrDefault()
                                               :
                                               x.Case.CaseLawUnits
                                              .Where(a => a.CaseSessionId == null)
                                              .Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                              .Where(a => (a.DateTo ?? dateEnd) >= (reportType == StatisticsConstants.ReportTypes.Unfinished ? fromDate : toDate))
                                              .OrderByDescending(a => a.DateFrom)
                                              .Select(a => a.LawUnitId + ",," + a.LawUnit.FullName)
                                              .FirstOrDefault()
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelCol,
                                    x.LawUnitData,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelCol = x.Key.ExcelCol,
                                    LawUnitData = x.Key.LawUnitData,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        private void SaveExcelByJudge(DateTime toDate, int rowIndex, List<CaseStatisticsVM> allData, int templateId,
                               int sheetIndex)
        {
            var allCorts = allData.Select(x => x.CourtId)
                           .Distinct()
                           .ToList();

            for (int i = 0; i < allCorts.Count; i++)
            {
                int courtId = allCorts[i];
                var allJudge = allData
                               .Where(x => x.CourtId == courtId)
                               .GroupBy(x => new
                               {
                                   x.LawUnitId,
                                   x.LawUnitName
                               })
                               .Select(x => new
                               {
                                   id = x.Key.LawUnitId,
                                   name = x.Key.LawUnitName
                               })
                               .OrderBy(x => x.name)
                               .ToList();

                var allDataByCourt = allData.Where(x => x.CourtId == courtId && x.ExcelCol > 0);
                for (int j = 0; j < allJudge.Count; j++)
                {
                    InsertExcelReportData(courtId, templateId, toDate.Year, toDate.Month,
                        sheetIndex, rowIndex, 1, allJudge[j].name);

                    foreach (var item in allDataByCourt.Where(x => x.LawUnitId == allJudge[j].id))
                    {
                        InsertExcelReportData(courtId, templateId, toDate.Year, toDate.Month,
                            sheetIndex, rowIndex, item.ExcelCol, item.Count.ToString());
                    }

                    rowIndex++;
                }
            }
        }

        private void SheetCaseCount(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId, 
            int[] caseGroupIds, int courtTypeId, List<StatisticsCourtTypeCaseTypeVM> courtTypeCaseTypesCols, 
            int sheetIndex, int startRowIndex)
        {
            var allData = CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  StatisticsConstants.ReportTypes.Unfinished, courtTypeCaseTypesCols);

            allData.AddRange(CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  StatisticsConstants.ReportTypes.Incoming, courtTypeCaseTypesCols));

            allData.AddRange(CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  StatisticsConstants.ReportTypes.Finished3months, courtTypeCaseTypesCols));

            allData.AddRange(CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  StatisticsConstants.ReportTypes.FinishedStop, courtTypeCaseTypesCols));

            allData.AddRange(CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  StatisticsConstants.ReportTypes.FinishedNoStop, courtTypeCaseTypesCols));

            SaveExcelByJudge(toDate, startRowIndex, allData, templateId, sheetIndex);
        }

        private List<StatisticsCourtTypeCaseTypeVM> FillCourtTypeCaseTypeCols(List<CourtTypeCaseType> courtTypeCaseTypes)
        {
            List<StatisticsCourtTypeCaseTypeVM> result = new List<StatisticsCourtTypeCaseTypeVM>();

            foreach (var item in courtTypeCaseTypes)
            {
                if (string.IsNullOrEmpty(item.ExcelReportCol)) continue;
                var cols = item.ExcelReportCol.Split(",", StringSplitOptions.RemoveEmptyEntries);

                //Разчитам, че са подредени в същия ред като константите за ReportTypes
                for (int i = 0; i < cols.Length; i++)
                {
                    StatisticsCourtTypeCaseTypeVM row = new StatisticsCourtTypeCaseTypeVM();
                    row.CourtTypeId = item.CourtTypeId;
                    row.CaseTypeId = item.CaseTypeId;
                    row.ReportTypeId = i + 1;
                    row.Col = int.Parse(cols[i]);
                    result.Add(row);
                }
            }

            return result;
        }

        private List<CaseStatisticsVM> Index_Select(int courtTypeId, int courtId, int[] caseGroupIds,
            DateTime fromDate, DateTime toDate, List<StatisticsExcelReportIndexVM> excelReportIndexCols)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseSessionAct, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseSessionAct, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseSessionAct, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;


            var result = repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.IsFinalDoc && x.ActDeclaredDate != null)
                                .Where(x => x.CaseSessionActComplains
                                             .Where(a => a.ComplainResults.
                                                         Where(b => b.DateResult >= fromDate.ForceStartDate() &&
                                                              b.DateResult <= toDate.ForceEndDate()).Any())
                                             .Any())
                                .Where(x => x.ActComplainIndexId != null)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelCol = excelReportIndexCols.Where(a => a.CaseTypes.Contains(x.Case.CaseTypeId) &&
                                               a.ActTypes.Contains(x.ActTypeId) &&
                                               a.ActComplainIndex.Contains(x.ActComplainIndexId ?? 0))
                                               .Select(a => a.Col)
                                               .FirstOrDefault(),
                                    LawUnitData = x.CaseSession.CaseLawUnits
                                              .Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                              .Where(a => (a.DateTo ?? dateEnd) >= x.CaseSession.DateFrom)
                                              .OrderByDescending(a => a.DateFrom)
                                              .Select(a => a.LawUnitId + ",," + a.LawUnit.FullName)
                                              .FirstOrDefault()
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelCol,
                                    x.LawUnitData,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelCol = x.Key.ExcelCol,
                                    LawUnitData = x.Key.LawUnitData,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        private void SheetActIndex(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            int[] caseGroupIds, int courtTypeId, List<StatisticsExcelReportIndexVM> excelReportIndexCols,
            int sheetIndex, int startRowIndex)
        {
            var allData = Index_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate, excelReportIndexCols);

            SaveExcelByJudge(toDate, startRowIndex, allData, templateId, sheetIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="excelReportCaseCodeRows"></param>
        /// <param name="colIndex"></param>
        /// <param name="reportType">1 - Несвършели към началото на периода, 2 - новообразувани без получените по подсъдност, 3 - получени по подсъдност,
        /// 4 - образувани под нов номер, 5 - обжалвани, 6 - Общо постъпили, 7 - новообразувани, 8 - постъпили бързи произвоства,
        /// 9 - Възобновени дела</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseCaseCode_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, int colIndex, int reportType,
         int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<Case, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.CaseGroupId);

            Expression<Func<Case, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<Case, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.CaseLifecycles
                        .Where(a => a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                        .Where(a => a.DateFrom.Date < fromDate.Date && (a.DateTo ?? dateEnd).Date >= fromDate.Date)
                        .Any();
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                                                (x.IsNewCaseNewNumber ?? false) == false &&
                                                repo.AllReadonly<CaseMigration>()
                                               .Where(a => a.CaseId == x.Id &&
                                                     a.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction)
                                               .Any() == false;
            }
            else if (reportType == 3)
            {
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                                        repo.AllReadonly<CaseMigration>()
                                               .Where(a => a.CaseId == x.Id &&
                                                     a.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction)
                                               .Any() == true;
            }
            else if (reportType == 4)
            {
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                                                (x.IsNewCaseNewNumber ?? false) == true;
            }
            else if (reportType == 5)
            {
                reportTypeWhere = x => x.CaseSessionActComplains.Where(a => a.DateExpired == null &&
                                   a.ComplainDocument.DocumentDate.Date >= fromDate.Date &&
                                   a.ComplainDocument.DocumentDate.Date <= toDate.Date &&
                                   a.ComplainDocument.DocumentTypeId != NomenclatureConstants.DocumentType.RequestForRenewing)
                                    .Any();
            }
            else if (reportType == 6)
            {
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date;
            }
            else if (reportType == 7)
            {
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                                                (x.IsNewCaseNewNumber ?? false) == false;
            }
            else if (reportType == 8)
            {
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                                               x.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick;
            }
            else if (reportType == 9)
            {
                reportTypeWhere = x => x.CaseSessionActComplains.Where(a => a.DateExpired == null &&
                                   a.ComplainDocument.DocumentDate.Date >= fromDate.Date &&
                                   a.ComplainDocument.DocumentDate.Date <= toDate.Date &&
                                   a.ComplainDocument.DocumentTypeId == NomenclatureConstants.DocumentType.RequestForRenewing &&
                                   repo.AllReadonly<CaseMigration>()
                                               .Where(b => b.CaseId == x.Id &&
                                                     b.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Outgoing &&
                                                     b.CaseSessionActId == a.CaseSessionActId &&
                                                     b.OutDocument.DocumentDate.Date >= fromDate.Date &&
                                                     b.OutDocument.DocumentDate.Date <= toDate.Date)
                                               .Any())
                                    .Any();
            }

            var result = repo.AllReadonly<Case>()
                                .Where(x => x.CaseStateId != NomenclatureConstants.CaseState.Draft)
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(x => excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.CaseCodeId ?? 0)).Any())
                                .Where(reportTypeWhere)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    ExcelRow = excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.CaseCodeId ?? 0))
                                               .Select(a => a.RowIndex)
                                               .FirstOrDefault(),
                                    ExcelCol = colIndex,
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelRow,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelRow = x.Key.ExcelRow,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="excelReportCaseCodeRows"></param>
        /// <param name="colIndex"></param>
        /// <param name="reportType">1 - продължаващи под същия номер, 2 - свършени до 3 месеца вкл., 3 - свършени от 3 до 6 месеца,
        /// 4 - прекратено по спогодба, 5 - прекратено по други причини, 6 - решени по същество с присъда,
        /// 7 - прекр. и спор. – Общо, 8 - в т.ч. свърш.споразум.- чл.381-384, 9 - От св.дела б.п.по чл. 356 НПК – решени,
        /// 10 - От св.дела б.п.по чл. 356 НПК – прекратени и споразумения</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseLifecycleCaseCode_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, int colIndex, int reportType,
    int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.Case.CaseLifecycles.Where(a => a.Id < x.Id &&
                                                 a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                 .Any() &&
                                x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date;

            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && x.DurationMonths <= 3;
            }
            else if (reportType == 3)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && x.DurationMonths > 3 && x.DurationMonths <= 6;
            }
            else if (reportType == 4)
            {
                var sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsStopCaseAgreement);
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null && sessionResults.Contains(a.SessionResultId))
                                      .Any();
            }
            else if (reportType == 5)
            {
                var sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsStopCaseOtherReason);

                var complainResults = repo.AllReadonly<ActComplainResultGrouping>()
                                     .Where(x => x.ActComplainResultGroup == NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStopGD)
                                     .Select(x => x.ActComplainResultId);
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    (complainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) ||
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null && sessionResults.Contains(a.SessionResultId))
                                      .Any());
            }
            else if (reportType == 6)
            {
                var actTypes = new int[] {NomenclatureConstants.ActType.Answer, NomenclatureConstants.ActType.Definition,
                             NomenclatureConstants.ActType.Sentence };
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && actTypes.Contains(x.CaseSessionAct.ActTypeId);
            }
            else if (reportType == 7)
            {
                var sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseStopND);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&                                    
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null && sessionResults.Contains(a.SessionResultId))
                                      .Any();
            }
            else if (reportType == 8)
            {
                var sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseStop381ND);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&                                    
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null && sessionResults.Contains(a.SessionResultId))
                                      .Any();
            }
            else if (reportType == 9)
            {
                var actTypes = new int[] {NomenclatureConstants.ActType.Answer, NomenclatureConstants.ActType.Definition,
                             NomenclatureConstants.ActType.Sentence };
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && actTypes.Contains(x.CaseSessionAct.ActTypeId) &&
                                    x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick;
            }
            else if (reportType == 10)
            {
                var sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseStopND);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null && sessionResults.Contains(a.SessionResultId))
                                      .Any() &&
                                    x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick;
            }

            var result = repo.AllReadonly<CaseLifecycle>()
                                .Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0)).Any())
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelRow = excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0))
                                               .Select(a => a.RowIndex)
                                               .FirstOrDefault(),
                                    ExcelCol = colIndex,
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelRow,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelRow = x.Key.ExcelRow,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        private List<CaseStatisticsVM> CaseLifecycleCaseCodeComplainResult_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, 
    List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults, int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            var result = repo.AllReadonly<CaseLifecycle>()
                                .Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date)
                                .Where(x => excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0)).Any())
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelRow = excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0))
                                               .Select(a => a.RowIndex)
                                               .FirstOrDefault(),
                                    ExcelCol = excelReportComplainResults
                                               .Where(a => a.ActComplainResult.Contains(x.CaseSessionAct.ActComplainResultId ?? 0))
                                               .Select(a => a.Col)
                                               .FirstOrDefault(),
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelRow,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelRow = x.Key.ExcelRow,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="colIndex"></param>
        /// <param name="rowIndex"></param>
        /// <param name="reportType"> RSSheet2 - 1 - Брой насрочвания на дела в открито заседание, 2 - Отлагания на дела в открити заседания,
        /// 3 - В т.ч. в I-во по делото заседание и помирително, 4 - 1.6.	Изготвяне на Справка III - Общ ред 1 месец, 
        /// 5 - Общ ред 2 месеца, 6 - Общ ред 3 месеца, 7 - Общ ред над 3 месеца,
        /// 8 - 1.6.	Изготвяне на Справка III - Бързи 1 месец, 
        /// 9 - Бързи 2 месеца, 10 - Бързи 3 месеца, 11 - Бързи над 3 месеца, 12 - Изпратени за доразследване от съдия-докладчик,
        /// 13 - Изпратени за доразследване в открито заседание, 14 - От влезли в сила решени,брой  дела, изпратени за доразследване,
        /// 15 - 1.6.	Изготвяне на Справка III - Без значение реда 1 месец, 
        /// 16 - Без значение реда 2 месеца, 17 - Без значение реда 3 месеца, 18 - Без значение реда над 3 месеца, 
        /// 19 - поради отмяна на решението и даване ход по същество</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseSession_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int[] caseTypeIds, int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseSession, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseSession, bool>> caseGroupWhere = x => true;
            if (caseGroupIds != null && caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseSession, bool>> caseTypeWhere = x => true;
            if (caseTypeIds != null && caseTypeIds.Length > 0)
                caseTypeWhere = x => caseTypeIds.Contains(x.Case.CaseTypeId);

            Expression<Func<CaseSession, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseSession, bool>> reportTypeSpravka3Where = x => true;
            if (reportType >= 4 && reportType <= 7)
            {
                reportTypeSpravka3Where = x => x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSession).Any() &&
                                       x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.GeneralOrder;
            }
            else if (reportType >= 8 && reportType <= 11)
            {
                reportTypeSpravka3Where = x => x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSession).Any() &&
                                       x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick;
            }
            else if (reportType >= 15 && reportType <= 18)
            {
                reportTypeSpravka3Where = x => x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSession).Any();
            }

            Expression<Func<CaseSession, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                       x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession;
            }
            else if (reportType == 2)
            {
                int[] sessionResults;
                if (caseGroupIds.Contains(NomenclatureConstants.CaseGroups.GrajdanskoDelo))
                    sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseDelayGD);
                else
                    sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseDelayND);

                reportTypeWhere = x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                       x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession &&
                                       x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              sessionResults.Contains(a.SessionResultId)).Any();
            }
            else if (reportType == 3)
            {
                var sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseDelayFirstSessionGD);
                reportTypeWhere = x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                       x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession &&
                                       x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              sessionResults.Contains(a.SessionResultId)).Any();
            }
            else if (reportType == 4 || reportType == 8 || reportType == 15)
            {
                reportTypeWhere = x => x.Case.RegDate.AddMonths(0).Date <= x.DateFrom.Date && x.Case.RegDate.AddMonths(1).Date >= x.DateFrom.Date;
            }
            else if (reportType == 5 || reportType == 9 || reportType == 16)
            {
                reportTypeWhere = x => x.Case.RegDate.AddMonths(1).Date < x.DateFrom.Date && x.Case.RegDate.AddMonths(2).Date >= x.DateFrom.Date;
            }
            else if (reportType == 6 || reportType == 10 || reportType == 17)
            {
                reportTypeWhere = x => x.Case.RegDate.AddMonths(2).Date < x.DateFrom.Date && x.Case.RegDate.AddMonths(3).Date >= x.DateFrom.Date;
            }
            else if (reportType == 7 || reportType == 11 || reportType == 18)
            {
                reportTypeWhere = x => x.Case.RegDate.AddMonths(3).Date < x.DateFrom.Date;
            }
            else if (reportType == 12)
            {
                reportTypeWhere = x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                       x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.SessionResultId == NomenclatureConstants.CaseSessionResult.Investigation).Any();
            }
            else if (reportType == 13)
            {
                reportTypeWhere = x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                       x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession &&
                                       x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.SessionResultId == NomenclatureConstants.CaseSessionResult.Investigation).Any();
            }
            else if (reportType == 14)
            {
                reportTypeWhere = x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                       x.CaseSessionActs.Where(a => a.DateExpired == null &&
                                                            a.ActInforcedDate <= toDate.ForceEndDate()).Any() &&
                                       x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.SessionResultId == NomenclatureConstants.CaseSessionResult.Investigation).Any();
            }
            else if (reportType == 19)
            {
                reportTypeWhere = x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                       x.CaseSessionResults.Where(a => a.DateExpired == null &&
                             a.SessionResultId == NomenclatureConstants.CaseSessionResult.StopedMoveWithSubstantialReason)
                                       .Any();
            }

            var result = repo.AllReadonly<CaseSession>()
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Where(reportTypeSpravka3Where)
                                .Where(caseTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelRow,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelRow = x.Key.ExcelRow,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="colIndex"></param>
        /// <param name="rowIndex"></param>
        /// <param name="reportType">RSSheet2 1 - Несвършени дела  от 1 до 3г., 2 - Несвършени дела  от 3 до 5г.,
        /// 3 - Несвършени дела  над  5г.“, 4 - 3 Несвършили до 3 месеца от датата на делото,
        /// 5 - 3 Несвършили от 3 до 6 месеца от датата на делото, 6 - 3 Несвършили от 6 до 12 месеца от датата на делото,
        /// 7 Несвършили над 12 месеца от датата на делото, 
        /// 8 - От решените дела /кол. 10/ с ненаписани мотиви към присъдата с изтекъл  30-дневен срок,
        /// 9 - Свършили бързо производство, 10 - Свършили по искане на обвиняемия, 11 - Свършили съкратено производство,
        /// 12 - От решените дела /кол. 10/ с ненаписани мотиви към присъдата с изтекъл  60-дневен срок</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseLifecycle_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                       x.DateFrom.AddYears(1).Date <= toDate.Date && x.DateFrom.AddYears(3).Date >= toDate.Date;
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                       x.DateFrom.AddYears(3).Date < toDate.Date && x.DateFrom.AddYears(5).Date >= toDate.Date;
            }
            else if (reportType == 3)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                       x.DateFrom.AddYears(5).Date < toDate.Date;
            }
            else if (reportType == 4)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                       x.Case.RegDate.AddMonths(3).Date >= toDate.Date;
            }
            else if (reportType == 5)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                       x.Case.RegDate.AddMonths(3).Date < toDate.Date && x.Case.RegDate.AddMonths(6).Date >= toDate.Date;
            }
            else if (reportType == 6)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                       x.Case.RegDate.AddMonths(6).Date < toDate.Date && x.Case.RegDate.AddMonths(12).Date >= toDate.Date;
            }
            else if (reportType == 7)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                       x.Case.RegDate.AddMonths(12).Date < toDate.Date;
            }
            else if (reportType == 8 || reportType == 12)
            {
                int days = reportType == 8 ? 30 : 60;
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
              x.CaseSessionAct.ActMotivesDeclaredDate.ForceStartDate() > (x.CaseSessionAct.ActDeclaredDate != null ? 
               ((DateTime)x.CaseSessionAct.ActDeclaredDate).AddDays(days).ForceStartDate() : dateEnd);
            }
            else if (reportType == 9)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                      x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick;
            }
            else if (reportType == 10)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                            x.Case.CaseTypeId == NomenclatureConstants.CaseTypes.ChND &&
                            x.Case.Document.DocumentTypeId == NomenclatureConstants.DocumentType.Request368;
            }
            else if (reportType == 11)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                    x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Short;
            }

            var result = repo.AllReadonly<CaseLifecycle>()
                                .Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelRow,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelRow = x.Key.ExcelRow,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="colIndex"></param>
        /// <param name="rowIndex"></param>
        /// <param name="reportType">RSSheet2 - 1 -От решените дела /кол.9+10+11/“ с необявени решения с изтекъл срок над 3м.,
        /// 2 - Постановени решения по чл. 235, ал. 5 от ГПК, след проведено открито съдебно заседание</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseSessionAct_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseSessionAct, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseSessionAct, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseSessionAct, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseSessionAct, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.ActDeclaredDate >= fromDate.ForceStartDate() && x.ActDeclaredDate <= toDate.ForceEndDate() &&
                                    x.CaseSession.DateFrom.AddMonths(3).ForceStartDate() < x.ActDeclaredDate.ForceStartDate() &&
                                    x.CaseSession.DateExpired == null &&
                                    x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession &&
                                    x.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                 a.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution).Any();
            }
            else if (reportType == 2)
            {
                var sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsStopCaseOtherReason);

                var complainResults = repo.AllReadonly<ActComplainResultGrouping>()
                                     .Where(x => x.ActComplainResultGroup == NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStopGD)
                                     .Select(x => x.ActComplainResultId);
                int[] typeAct = new int[] {NomenclatureConstants.ActType.Answer,
                                        NomenclatureConstants.ActType.Definition,
                                        NomenclatureConstants.ActType.Injunction};

                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                    x.ActDeclaredDate <= toDate.ForceEndDate() &&
                                    typeAct.Contains(x.ActTypeId) &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession &&
                                    (complainResults.Contains(x.ActComplainResultId ?? 0) ||
                                    x.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null && sessionResults.Contains(a.SessionResultId))
                                      .Any()) == false;
            }

            var result = repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelRow,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelRow = x.Key.ExcelRow,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="excelReportCaseCodeRows"></param>
        /// <param name="colIndex"></param>
        /// <param name="reportType">1 - Съдени лица общо, 2 - Оправдани от Съдени лица, 3 - Осъдени лица непълнолетни,
        /// 4 - Лишаване от свобода – до 3г. – Общо, 5 - Лишаване от свобода – до 3г. – условно,
        /// 6 - Лишаване от свобода над 3 до 15 г, 7 - Осъден глоба, 8 - Осъден Пробация, 9 - Осъдени лица – Други наказания,
        /// 10 - брой наказани лица по споразум. - чл.381-384 НПК,
        /// 11 - Лишаване от свобода над 3 до 10 г, 12 - Лишаване от свобода над 10 до 30 г,
        /// 13 - Доживотен затвор, 14 - Доживотен затвор без право на замяна</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CasePersonCaseCode_Select(int courtTypeId, int courtId,
    DateTime fromDate, DateTime toDate, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, int colIndex, int reportType,
    int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CasePerson, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CasePerson, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CasePerson, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date &&
                                   x.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide;
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date &&
                                   x.CasePersonSentences
                                       .Where(a => a.SentenceResultTypeId == NomenclatureConstants.SentenceResultTypes.Justified &&
                                              a.DateExpired == null)
                                   .Any();
            }
            else if (reportType == 3)
            {
                reportTypeWhere = x => x.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge &&
                                        x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null).Any())
                                         .Any();
            }
            else if (reportType == 4 || reportType == 5)
            {
                int[] sentenceTypes;
                if (reportType == 4)
                    sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.ImprisonmentConditional ,
                                                NomenclatureConstants.SentenceTypes.ImprisonmentEffectively};
                else
                    sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.ImprisonmentConditional};

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null &&
                                                          sentenceTypes.Contains(b.SentenceTypeId ?? 0) &&
                                                          (b.SentenseDays + b.SentenseWeeks * 7) / 365 + b.SentenseMonths / 12 +
                                                          b.SentenseYears <= 3)
                                                                      .Any())
                                         .Any();
            }
            else if (reportType == 6 || reportType == 11 || reportType == 12)
            {
                var sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.ImprisonmentConditional ,
                                                NomenclatureConstants.SentenceTypes.ImprisonmentEffectively};

                int fromYear = 0;
                int toYear = 0;
                if (reportType == 6)
                {
                    fromYear = 3;
                    toYear = 15;
                }
                else if (reportType == 11)
                {
                    fromYear = 3;
                    toYear = 10;
                }
                else if (reportType == 12)
                {
                    fromYear = 10;
                    toYear = 30;
                }

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null &&
                                                          sentenceTypes.Contains(b.SentenceTypeId ?? 0) &&
                                                          (b.SentenseDays + b.SentenseWeeks * 7) / 365 + b.SentenseMonths / 12 +
                                                          b.SentenseYears > fromYear &&
                                                          (b.SentenseDays + b.SentenseWeeks * 7) / 365 + b.SentenseMonths / 12 +
                                                          b.SentenseYears <= toYear)
                                                                      .Any())
                                         .Any();
            }
            else if (reportType == 7 || reportType == 8 || reportType == 9 || reportType == 13 || reportType == 14)
            {
                int[] sentenceTypes;
                if (reportType == 7)
                    sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.Fine };
                else if (reportType == 8)
                    sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.Probation ,
                                                NomenclatureConstants.SentenceTypes.CorrectiveWork,
                                                NomenclatureConstants.SentenceTypes.Settlement };
                else if (reportType == 9)
                    sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.Reprimand ,
                                                NomenclatureConstants.SentenceTypes.DeprivationOfRights,
                                                NomenclatureConstants.SentenceTypes.Other,
                                                NomenclatureConstants.SentenceTypes.OtherConditional,
                                                NomenclatureConstants.SentenceTypes.TVU,
                                                NomenclatureConstants.SentenceTypes.NotPunished
                                               };
                else if (reportType == 13)
                    sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.LifeSentence
                                               };
                else 
                    sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.LifeSentenceNoChange
                                               };

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null &&
                                                                   sentenceTypes.Contains(b.SentenceTypeId ?? 0))
                                                                      .Any())
                                         .Any();
            }            
            else if (reportType == 10)
            {
                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.SentenceResultTypeId == NomenclatureConstants.SentenceResultTypes.ConvictAgreement &&
                                        a.DateExpired == null)
                                         .Any();
            }

            var result = repo.AllReadonly<CasePerson>()
                                .Where(x => x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                                .Where(x => excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0)).Any())
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelRow = excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0))
                                               .Select(a => a.RowIndex)
                                               .FirstOrDefault(),
                                    ExcelCol = colIndex,
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelRow,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelRow = x.Key.ExcelRow,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="colIndex"></param>
        /// <param name="rowIndex"></param>
        /// <param name="reportType">1 - Кумулации, 2 - Рецидивисти, 3 - Пробация</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CasePerson_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CasePerson, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CasePerson, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CasePerson, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CasePerson, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.DateExpired == null &&
                                        a.CasePersonSentenceLawbases.Where(b => b.SentenceLawbaseId == NomenclatureConstants.SentenceLawbases.LawBase25)
                                                                      .Any())
                                         .Any();
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date &&
                                        x.CasePersonCrimes.Where(a => a.DateExpired == null &&
                                        NomenclatureConstants.RecidiveTypes.Recidives.Contains(a.RecidiveTypeId))
                                         .Any();
            }
            else if (reportType == 3)
            {
                var sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.Probation ,
                                                NomenclatureConstants.SentenceTypes.CorrectiveWork,
                                                NomenclatureConstants.SentenceTypes.Settlement };

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null &&
                                                                   sentenceTypes.Contains(b.SentenceTypeId ?? 0))
                                                                      .Any())
                                         .Any();
            }

            var result = repo.AllReadonly<CasePerson>()
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelRow,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelRow = x.Key.ExcelRow,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="colIndex"></param>
        /// <param name="rowIndex"></param>
        /// <param name="reportType">1 - Спрени дела, 2 - Възовновени дела, 3 - по дата на делото</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseByFromCourt_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId,
    int[] documentTypeIds, bool groupByFromCourt)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<Case, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.CaseGroupId);

            Expression<Func<Case, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<Case, bool>> documentTypeWhere = x => true;
            if (documentTypeIds != null && documentTypeIds.Length > 0)
                documentTypeWhere = x => documentTypeIds.Contains(x.Document.DocumentTypeId);

            Expression<Func<Case, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.CaseLifecycles.Where(a => a.LifecycleTypeId == NomenclatureConstants.LifecycleType.Stop &&
                                           (a.DateTo ?? dateEnd).Date > toDate.Date).Any();
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.CaseSessionActComplains.Where(a => a.DateExpired == null &&
                                   a.ComplainDocument.DocumentTypeId == NomenclatureConstants.DocumentType.RequestForRenewing &&
                                   repo.AllReadonly<CaseMigration>()
                                               .Where(b => b.CaseId == x.Id &&
                                                     b.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Outgoing &&
                                                     b.CaseSessionActId == a.CaseSessionActId &&
                                                     b.OutDocument.DocumentDate.Date >= fromDate.Date &&
                                                     b.OutDocument.DocumentDate.Date <= toDate.Date
                                                     )
                                               .Any())
                                    .Any();
            }
            else if (reportType == 3)
            {
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date;
            }

            var result = repo.AllReadonly<Case>()
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Where(documentTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    FromCourtData = x.Document.DocumentCaseInfo
                                            .Select(a => a.CourtId + ",," + a.Court.Label + ",," +
                                              (a.Court.ParentCourtId == x.CourtId ? 0 : 1))
                                            .FirstOrDefault(),
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.FromCourtData,
                                    x.ExcelRow,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    FromCourtData = x.Key.FromCourtData,
                                    ExcelRow = x.Key.ExcelRow,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }


        private List<CaseStatisticsVM> Case_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId)
        {
            return CaseByFromCourt_Select(courtTypeId, courtId, caseGroupIds,
    fromDate, toDate, colIndex, rowIndex, reportType, instanceId, null, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="excelReportCaseCodeRows"></param>
        /// <param name="colIndex"></param>
        /// <param name="reportType"></param>
        /// <param name="instanceId">1 - Влезли в сила присъди, 2 - Влезли в сила оправдателни присъди</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CasePersonSentenceCaseCode_Select(int courtTypeId, int courtId,
    DateTime fromDate, DateTime toDate, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, int colIndex, int reportType,
    int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CasePersonSentence, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CasePersonSentence, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CasePersonSentence, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.InforcedDate >= fromDate.ForceStartDate() && x.InforcedDate <= toDate.ForceEndDate();
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.InforcedDate >= fromDate.ForceStartDate() && x.InforcedDate <= toDate.ForceEndDate() &&
                                  x.SentenceResultTypeId == NomenclatureConstants.SentenceResultTypes.Justified; 
            }


            var result = repo.AllReadonly<CasePersonSentence>()
                                .Where(x => x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.DateExpired == null)
                                .Where(x => (x.IsActive ?? false) == true)
                                .Where(x => x.Case.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                                .Where(x => excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0)).Any())
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    ExcelRow = excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0))
                                               .Select(a => a.RowIndex)
                                               .FirstOrDefault(),
                                    ExcelCol = colIndex,
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelRow,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelRow = x.Key.ExcelRow,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }
        private void RSSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.RegionalCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.FirstInstance;

            //Един път се пуска за кодовете, които са по един и един път за тези които са сумарни
            for (int i = 0; i < 2; i++)
            {
                List<StatisticsExcelReportCaseCodeRowVM> caseCodes = null;
                if (i == 0)
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count == 1).ToList();
                else
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count > 1).ToList();

                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 2, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 3, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 4, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 6, 1, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 2, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 16, 3, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 4, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 14, 5, instanceId));
                allData.AddRange(CaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes,
                      excelReportComplainResults, instanceId));

                //Обжалвани
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 18, 5, instanceId));
            }


            //Справка 1
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 51, 1, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 52, 2, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 53, 3, null, instanceId));

            //Справка 2
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 57, 1, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 58, 2, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 59, 3, instanceId));
            allData.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 60, 1, instanceId));

            //Справка 3
            int colIndex = 5;
            for (int i = 4; i <= 11; i++)
            {
                allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, colIndex, 52, i, null, instanceId));
                colIndex++;
            }

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count.ToString());
            }
        }

        private void RSSheet3(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.RegionalCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.FirstInstance;

            //Един път се пуска за кодовете, които са по един и един път за тези които са сумарни
            for (int i = 0; i < 2; i++)
            {
                List<StatisticsExcelReportCaseCodeRowVM> caseCodes = null;
                if (i == 0)
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count == 1).ToList();
                else
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count > 1).ToList();

                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 4, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 7, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 6, 8, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 7, 1, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 6, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 12, 7, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 8, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 14, 9, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 10, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 16, 2, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 17, 5, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 19, 1, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 20, 2, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 22, 3, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 23, 4, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 5, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 25, 6, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 26, 7, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 27, 8, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 28, 9, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 29, 10, instanceId));
            }

            //Справка 3
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 134, 1, 
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD}, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 135, 1, 
                new int[] { NomenclatureConstants.CaseTypes.NOHD}, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 136, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 137, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 138, 12, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 139, 13, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 140, 14, null, instanceId));
            allData.AddRange(CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 141, 1, instanceId));

            //Справка 4
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 146, 4, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 147, 5, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 148, 6, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 149, 7, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 150, 8, instanceId));

            //Справка 6
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 160, 9, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 162, 10, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 163, 11, instanceId));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count.ToString());
            }
        }

        private void MillitarySheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.Millitary;
            int instanceId = NomenclatureConstants.CaseInstanceType.FirstInstance;

            //Един път се пуска за кодовете, които са по един и един път за тези които са сумарни
            for (int i = 0; i < 2; i++)
            {
                List<StatisticsExcelReportCaseCodeRowVM> caseCodes = null;
                if (i == 0)
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count == 1).ToList();
                else
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count > 1).ToList();

                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 7, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 1, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 8, 6, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 9, 7, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 10, 8, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 2, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 5, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 14, 1, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 15, 2, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 17, 3, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 18, 4, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 19, 5, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 20, 11, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 21, 12, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 22, 13, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 23, 14, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 9, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 25, 10, instanceId));
            }

            //Справка 1
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 174, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 175, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 176, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 177, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 178, 12, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 179, 13, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 180, 14, null, instanceId));
            allData.AddRange(Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 181, 1, instanceId));
            allData.AddRange(CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 182, 2, instanceId));
            allData.AddRange(CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 183, 3, instanceId));
            allData.AddRange(CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 184, 1, instanceId));

            //Справка 2
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 191, 4, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 192, 5, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 193, 6, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 194, 7, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 195, 12, instanceId));

            //Справка 3
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 202, 9, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 204, 10, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 205, 11, instanceId));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count.ToString());
            }
        }

        private void OSSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo,
                                           NomenclatureConstants.CaseGroups.Trade,
                                           NomenclatureConstants.CaseGroups.Company};
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.FirstInstance;

            //Един път се пуска за кодовете, които са по един и един път за тези които са сумарни
            for (int i = 0; i < 2; i++)
            {
                List<StatisticsExcelReportCaseCodeRowVM> caseCodes = null;
                if (i == 0)
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count == 1).ToList();
                else
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count > 1).ToList();

                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 2, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 3, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 4, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 6, 1, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 10, 2, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 3, instanceId));
                allData.AddRange(CaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes,
                      excelReportComplainResults, instanceId));

                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 4, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 16, 5, instanceId));

                //Обжалвани
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 18, 5, instanceId));
            }


            //Справка 1
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 76, 1, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 77, 2, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 78, 3, null, instanceId));

            //Справка 2
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 82, 1, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 83, 2, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 84, 3, instanceId));
            allData.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 85, 1, instanceId));

            //Справка 3
            int colIndex = 5;
            for (int i = 4; i <= 11; i++)
            {
                allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo}, 
                                  fromDate, toDate, colIndex, 83, i, null, instanceId));
                colIndex++;
            }
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.Trade }, fromDate, toDate, 13, 83, 15, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.Trade }, fromDate, toDate, 14, 83, 16, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.Trade }, fromDate, toDate, 15, 83, 17, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.Trade }, fromDate, toDate, 16, 83, 18, null, instanceId));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count.ToString());
            }
        }

        private void OSSheet4(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.FirstInstance;

            //Един път се пуска за кодовете, които са по един и един път за тези които са сумарни
            for (int i = 0; i < 2; i++)
            {
                List<StatisticsExcelReportCaseCodeRowVM> caseCodes = null;
                if (i == 0)
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count == 1).ToList();
                else
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count > 1).ToList();

                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 4, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 1, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 8, 9, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 10, 6, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 7, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 12, 8, instanceId));
                allData.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 2, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 5, instanceId));
                allData.AddRange(CasePersonSentenceCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 16, 1, instanceId));
                allData.AddRange(CasePersonSentenceCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 17, 2, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 18, 1, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 19, 2, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 21, 3, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 22, 4, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 23, 5, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 11, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 25, 12, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 26, 13, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 27, 14, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 28, 9, instanceId));
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 29, 10, instanceId));
            }

            allData.AddRange(Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 125, 2, instanceId));

            //Справка 1
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 130, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 131, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 132, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 133, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 134, 12, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 135, 13, null, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 136, 14, null, instanceId));
            allData.AddRange(Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 137, 1, instanceId));
            allData.AddRange(CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 138, 3, instanceId));
            allData.AddRange(CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 139, 1, instanceId));

            //Справка 2
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 145, 4, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 146, 5, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 147, 6, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 148, 7, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 149, 12, instanceId));

            //Справка 3
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 155, 9, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 157, 10, instanceId));
            allData.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 158, 11, instanceId));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="colIndex"></param>
        /// <param name="reportType"> 1 - несвършени дела в началото на отчетния период,
        /// 2 - Постъпили дела, 3 - прекратяване на делото,
        /// 4 - Свършени дела в периода</param>
        /// <param name="documentTypeIds"></param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseLifecycleByFromCourt_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int reportType, int[] documentTypeIds,
    List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseLifecycle, bool>> documentTypeWhere = x => true;
            if (documentTypeIds != null && documentTypeIds.Length > 0)
                documentTypeWhere = x => documentTypeIds.Contains(x.Case.Document.DocumentTypeId);

            Expression<Func<CaseLifecycle, bool>> complainWhere = x => true;
            if (colIndex <= 0)
                complainWhere = x => excelReportComplainResults
                    .Where(a => a.ActComplainResult.Contains(x.CaseSessionAct.ActComplainResultId ?? 0))
                    .Any();

            Expression <Func<CaseLifecycle, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.DateFrom.Date < fromDate.Date && (x.DateTo ?? dateEnd).Date >= fromDate.Date;
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date;
            }
            else if (reportType == 3)
            {
                var actComplainResults = repo.AllReadonly<ActComplainResultGrouping>()
               .Where(x => x.ActComplainResultGroup == NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop)
               .Select(x => x.ActComplainResultId)
               .ToArray();

                var actSessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsStopCase);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    (actComplainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) ||
                                     x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                          a.IsMain && a.IsActive &&
                                          actSessionResults.Contains(a.SessionResultId)).Any());
            }
            else if (reportType == 4)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date;
            }


            var result = repo.AllReadonly<CaseLifecycle>()
                                .Where(x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(x => x.Case.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance)
                                .Where(x => x.Case.Document.DocumentCaseInfo.Any())
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(documentTypeWhere)
                                .Where(reportTypeWhere)
                                .Where(complainWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    FromCourtData = x.Case.Document.DocumentCaseInfo
                                            .Select(a => a.CourtId + ",," + a.Court.Label + ",," + 
                                              (a.Court.ParentCourtId == x.CourtId ? 0 : 1))
                                            .FirstOrDefault(),
                                    ExcelCol = colIndex > 0 ? colIndex :
                                    excelReportComplainResults
                                               .Where(a => a.ActComplainResult.Contains(x.CaseSessionAct.ActComplainResultId ?? 0))
                                               .Select(a => a.Col)
                                               .FirstOrDefault(),
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.FromCourtData,
                                    x.ExcelCol,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    FromCourtData = x.Key.FromCourtData,
                                    ExcelCol = x.Key.ExcelCol,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        private void SaveExcelByFromCourt(DateTime toDate, int rowIndex, List<CaseStatisticsVM> allData, int templateId,
                               int sheetIndex)
        {
            var allCorts = allData.Select(x => x.CourtId)
                           .Distinct()
                           .ToList();

            for (int i = 0; i < allCorts.Count; i++)
            {
                int courtId = allCorts[i];
                var allFromCourt = allData
                               .Where(x => x.CourtId == courtId)
                               .GroupBy(x => new
                               {
                                   x.FromCourtId,
                                   x.FromCourtIsParent,
                                   x.FromCourtName,
                               })
                               .Select(x => new
                               {
                                   fromCourtId = x.Key.FromCourtId,
                                   fromCourtIsParent = x.Key.FromCourtIsParent,
                                   fromCourtName = x.Key.FromCourtName,
                               })
                               .OrderBy(x => x.fromCourtIsParent)
                               .ThenBy(x => x.fromCourtName)
                               .ToList();

                var allDataByCourt = allData.Where(x => x.CourtId == courtId && x.ExcelCol > 0);
                for (int j = 0; j < allFromCourt.Count; j++)
                {
                    InsertExcelReportData(courtId, templateId, toDate.Year, toDate.Month,
                        sheetIndex, rowIndex, 0, allFromCourt[j].fromCourtName);

                    foreach (var item in allDataByCourt.Where(x => x.FromCourtId == allFromCourt[j].fromCourtId))
                    {
                        InsertExcelReportData(courtId, templateId, toDate.Year, toDate.Month,
                            sheetIndex, rowIndex, item.ExcelCol, item.Count.ToString());
                    }

                    rowIndex++;
                }
            }
        }

        private void ApealSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo,
                                    NomenclatureConstants.CaseGroups.Trade};
            int courtTypeId = NomenclatureConstants.CourtType.Apeal;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD,
                           NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaint274TDGD,
            NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD,
            NomenclatureConstants.DocumentTypeGroupings.StatisticsRequestSlownessTDGD};

            var documentTypes = repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToList();

            //Колони за Жалби
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null);

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 2, complains, null));

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults));

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 11, 3, complains, null));

            //Колони за Частни Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 13, 1, complains, null));

            allData.AddRange(CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 14, 0, 3, instanceId, complains, true));

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 15, 4, complains, null));

            //Колони за Жалби по бавност
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsRequestSlownessTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 17, 0, 3, instanceId, complains, true));

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults));

            //Колони за Жалби по 274
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaint274TDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 20, 1, complains, null));

            allData.AddRange(CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 21, 0, 3, instanceId, complains, true));

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 22, 4, complains, null));

            
            SaveExcelByFromCourt(toDate, 14, allData, templateId, 2);

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 40, 1, null, instanceId));
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 41, 2, null, instanceId));
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 42, 19, null, instanceId));

            //Справка 2
            allDataGroup.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 47, 2, instanceId));

            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count.ToString());
            }

        }
        private void ApealSheet3(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo};
            int courtTypeId = NomenclatureConstants.CourtType.Apeal;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND,
                           NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND,
            NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND,
            NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND};

            var documentTypes = repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToList();

            //Колони за Жалби и протести
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND ||
                    x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null);

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 2, complains, null));

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults));

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 18, 3, complains, null));

            //Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 4, 2, complains, null));

            //Протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 5, 2, complains, null));


            //Колони за Частни Жалби и протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 20, 1, complains, null));

            allData.AddRange(CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 21, 0, 3, instanceId, complains, true));

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 22, 4, complains, null));

            //Колони за Възобновяване
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 24, 1, complains, null));

            allData.AddRange(CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 25, 0, 3, instanceId, complains, true));

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 26, 4, complains, null));

            SaveExcelByFromCourt(toDate, 14, allData, templateId, 3);

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 40, 1, null, instanceId));
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 41, 2, null, instanceId));

            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count.ToString());
            }

        }
    }
}
