using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class StatisticsReportService : BaseService, IStatisticsReportService
    {
        public StatisticsReportService(
            IRepository _repo
            )
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

        private int[] DocumentTypeGrouping_Select(int groupId)
        {
            return repo.AllReadonly<DocumentTypeGrouping>()
                                      .Where(x => x.DocumentTypeGroup == groupId)
                                      .Select(x => x.DocumentTypeId)
                                      .ToArray();
        }

        private int[] ActComplainResultGrouping_Select(int groupId)
        {
            return repo.AllReadonly<ActComplainResultGrouping>()
                                      .Where(x => x.ActComplainResultGroup == groupId)
                                      .Select(x => x.ActComplainResultId)
                                      .ToArray();
        }

        private int[] SessionResultBaseGrouping_Select(int groupId)
        {
            return repo.AllReadonly<SessionResultBaseGrouping>()
                                      .Where(x => x.SessionResultBaseGroup == groupId)
                                      .Select(x => x.SessionResultBaseId)
                                      .ToArray();
        }

        private List<ExcelReportData> RS_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData,
            int searchCourtId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = NomenclatureConstants.CourtType.RegionalCourt;

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults = nomData.excelReportComplainResults
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows = nomData.excelReportCaseTypeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeColVM> excelReportCaseTypeCols = nomData.excelReportCaseTypeCols
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            result.AddRange(SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                 courtTypeId, excelReportCaseTypeCols, 4, 8));
            result.AddRange(SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                   new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo },
                   courtTypeId, excelReportCaseTypeCols, 6, 9));

            result.AddRange(SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                 courtTypeId, excelReportIndexCols, 5, 8));
            result.AddRange(SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                   new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo },
                   courtTypeId, excelReportIndexCols, 7, 7));

            //Sheet 2 Приложение ГД
            result.AddRange(RSSheet2(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList(),
                excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList()));

            //Sheet 3 Приложение НД
            result.AddRange(RSSheet3(fromDate, toDate, searchCourtId, templateId, 
                excelReportCaseCodeRows.Where(x => x.SheetIndex == 3 || x.SheetIndex == 103).ToList(),
                excelReportComplainResults.Where(x => x.SheetIndex == 103).ToList()));

            //Sheet 1 Приложение 1
            result.AddRange(RSSheet1(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows));

            result.AddRange(RSExcelTitle(toDate, searchCourtId, templateId));

            return result;
        }

        private List<ExcelReportData> OS_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData,
            int searchCourtId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults = nomData.excelReportComplainResults
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows = nomData.excelReportCaseTypeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeColVM> excelReportCaseTypeCols = nomData.excelReportCaseTypeCols
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            result.AddRange(SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                 courtTypeId, excelReportCaseTypeCols, 6, 8));
            result.AddRange(SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade,
                             NomenclatureConstants.CaseGroups.Company},
                   courtTypeId, excelReportCaseTypeCols, 8, 8));

            result.AddRange(SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                 courtTypeId, excelReportIndexCols, 7, 7));
            result.AddRange(SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade,
                             NomenclatureConstants.CaseGroups.Company},
                   courtTypeId, excelReportIndexCols, 9, 7));

            //Sheet 2 Приложение ГД, ТД, ФД
            result.AddRange(OSSheet2(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList(),
                excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList()));

            //Sheet 3 Приложение ГД
            result.AddRange(OSSheet3(fromDate, toDate, searchCourtId, templateId, excelReportComplainResults.Where(x => x.SheetIndex == 3).ToList()));

            //Sheet 4 Приложение НД
            result.AddRange(OSSheet4(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 4).ToList()));

            //Sheet 5 Приложение НД
            result.AddRange(OSSheet5(fromDate, toDate, searchCourtId, templateId,
                   excelReportComplainResults.Where(x => x.SheetIndex == 5).ToList()));

            //Sheet 1 Приложение 1
            result.AddRange(DistrictSheet1(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows));

            result.AddRange(OSExcelTitle(toDate, searchCourtId, templateId));

            return result;
        }

        private List<ExcelReportData> AP_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData,
            int searchCourtId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = NomenclatureConstants.CourtType.Apeal;

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults = nomData.excelReportComplainResults
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows = nomData.excelReportCaseTypeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeColVM> excelReportCaseTypeCols = nomData.excelReportCaseTypeCols
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            result.AddRange(SheetCaseCount(fromDate, toDate, searchCourtId, templateId, new int[]
              { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, courtTypeId,
              excelReportCaseTypeCols, 4, 8));
            result.AddRange(SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                    new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade },
                    courtTypeId, excelReportCaseTypeCols, 6, 8));

            result.AddRange(SheetActIndex(fromDate, toDate, searchCourtId, templateId, new int[]
              { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, courtTypeId,
              excelReportIndexCols, 5, 7));
            result.AddRange(SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                    new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade },
                    courtTypeId, excelReportIndexCols, 7, 7));

            //Sheet 2 Приложение ГД/ТД
            result.AddRange(ApealSheet2(fromDate, toDate, searchCourtId, templateId, excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList()));

            //Sheet 3 Приложение НД
            result.AddRange(ApealSheet3(fromDate, toDate, searchCourtId, templateId,
                   excelReportComplainResults.Where(x => x.SheetIndex == 3 || x.SheetIndex == 103).ToList(),
                   excelReportCaseCodeRows.Where(x => x.SheetIndex == 3).ToList()));

            //Sheet 1 Приложение 1
            result.AddRange(ApealSheet1(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows, courtTypeId));

            result.AddRange(ApealExcelTitle(toDate, searchCourtId, templateId));

            return result;
        }

        private List<ExcelReportData> Millitary_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData,
            int searchCourtId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = NomenclatureConstants.CourtType.Millitary;

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows = nomData.excelReportCaseTypeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeColVM> excelReportCaseTypeCols = nomData.excelReportCaseTypeCols
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            result.AddRange(SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                courtTypeId, excelReportCaseTypeCols, 3, 8));

            result.AddRange(SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                courtTypeId, excelReportIndexCols, 4, 7));

            //Sheet 2 Приложение НД
            result.AddRange(MillitarySheet2(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList()));

            //Sheet 1 Приложение 1
            result.AddRange(MillitarySheet1(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows));

            result.AddRange(MillitaryExcelTitle(toDate, searchCourtId, templateId));

            return result;
        }

        private List<ExcelReportData> MillitaryAP_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData,
            int searchCourtId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = NomenclatureConstants.CourtType.MillitaryApeal;

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults = nomData.excelReportComplainResults
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows = nomData.excelReportCaseTypeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeColVM> excelReportCaseTypeCols = nomData.excelReportCaseTypeCols
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            result.AddRange(SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                courtTypeId, excelReportCaseTypeCols, 3, 8));

            result.AddRange(SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                courtTypeId, excelReportIndexCols, 4, 7));

            //Sheet 3 Приложение НД
            result.AddRange(MillitaryAPSheet2(fromDate, toDate, searchCourtId, templateId,
                   excelReportComplainResults.Where(x => x.SheetIndex == 2 || x.SheetIndex == 102).ToList(),
                   excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList()));

            //Sheet 1 Приложение 1
            result.AddRange(ApealSheet1(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows, courtTypeId));

            result.AddRange(MillitaryApealExcelTitle(toDate, searchCourtId, templateId));

            return result;
        }

        public List<ExcelReportData> FillExcelData(DateTime fromDate, DateTime toDate, int courtId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = 0;
            if (courtId > 0)
            {
                courtTypeId = repo.AllReadonly<Court>()
                               .Where(x => x.Id == courtId)
                               .Select(x => x.CourtTypeId)
                               .FirstOrDefault();
            }

            StatisticsNomDataVM nomData = new StatisticsNomDataVM();

            var courtTypeCaseTypes = repo.AllReadonly<CourtTypeCaseType>().ToList();

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

            nomData.excelReportCaseTypeRows = repo.AllReadonly<ExcelReportCaseTypeRow>()
                                    .Select(x => new StatisticsExcelReportCaseTypeRowVM()
                                    {
                                        CourtTypeId = x.CourtTypeId,
                                        CaseTypeIds = x.CaseTypeId,
                                        DocumentTypeIds = x.DocumentTypeId ?? "",
                                        CaseCodeIds = x.CaseCodeId ?? "",
                                        ForColumnIds = x.ForColumns,
                                        RowIndex = x.RowIndex,
                                        IsTrue = x.IsTrue,
                                    }).ToList();

            nomData.excelReportCaseTypeCols = repo.AllReadonly<ExcelReportCaseTypeCol>()
                                    .Select(x => new StatisticsExcelReportCaseTypeColVM()
                                    {
                                        CourtTypeId = x.CourtTypeId,
                                        ReportTypeId = x.ReportTypeId,
                                        CaseTypeIds = x.CaseTypeId,
                                        DocumentTypeIds = x.DocumentTypeId ?? "",
                                        CaseCodeIds = x.CaseCodeId ?? "",
                                        ColIndex = x.ColIndex,
                                        IsTrue = x.IsTrue,
                                    }).ToList();

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
            {
                result.AddRange(RS_Sheets(fromDate, toDate,
                    reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                    .Select(x => x.Id).FirstOrDefault(), nomData, courtId));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
            {
                result.AddRange(OS_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
                .Select(x => x.Id).FirstOrDefault(), nomData, courtId));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.Apeal)
            {
                result.AddRange(AP_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal)
                .Select(x => x.Id).FirstOrDefault(), nomData, courtId));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.Millitary)
            {
                result.AddRange(Millitary_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Millitary)
                .Select(x => x.Id).FirstOrDefault(), nomData, courtId));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.MillitaryApeal)
            {
                result.AddRange(MillitaryAP_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.MillitaryApeal)
                .Select(x => x.Id).FirstOrDefault(), nomData, courtId));
            }

            return result;
        }

        public bool Statistics_SaveData(DateTime fromDate, DateTime toDate, int courtId)
        {
            try
            {
                //Ако е за определен съд да извика метода за delete и save
                if (courtId > 0)
                {
                    return Statistics_DeleteSaveData(fromDate, toDate, courtId);
                }

                //Ако има данните за месец/година нищо да не прави сървиза
                var hasData = repo.AllReadonly<ExcelReportData>()
                                 .Where(x => x.ReportMonth == toDate.Month &&
                                 x.ReportYear == toDate.Year)
                                 .Any();
                if (hasData == true) 
                    return true;

                List<ExcelReportData> result = FillExcelData(fromDate, toDate, courtId);

                repo.AddRange(result);
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Statistics_DeleteSaveData(DateTime fromDate, DateTime toDate, int courtId)
        {
            try
            {
                List<ExcelReportData> result = FillExcelData(fromDate, toDate, courtId);

                //Изтриване на данните за месец/година
                repo.DeleteRange<ExcelReportData>(x => x.ReportMonth == toDate.Month && x.ReportYear == toDate.Year &&
                                  (courtId > 0 ? courtId : x.CourtId) == x.CourtId);

                repo.AddRange(result);
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private ExcelReportData ExcelReportSetMainData(int court_id, int reportTemplateId, int reportYear, int reportMonth, int sheetIndex, int rowIndex, int colIndex)
        {
            ExcelReportData exd = new ExcelReportData();
            exd.CourtId = court_id;
            exd.DateWrt = DateTime.Now;
            exd.ExcelReportTemplateId = reportTemplateId;
            exd.ReportYear = reportYear;
            exd.ReportMonth = reportMonth;
            exd.SheetIndex = sheetIndex;
            exd.RowIndex = rowIndex;
            exd.ColIndex = colIndex;
            return exd;
        }

        private ExcelReportData InsertExcelReportData(int court_id, int reportTemplateId, int reportYear, int reportMonth, int sheetIndex, int rowIndex, int colIndex, int cellValue)
        {
            ExcelReportData exd = ExcelReportSetMainData(court_id, reportTemplateId, reportYear, reportMonth, sheetIndex, rowIndex, colIndex);
            exd.CellValueInt = cellValue;
            exd.CellValueType = NomenclatureConstants.ExcelReportCellValueTypes.IntValue;
            return exd;
        }

        private ExcelReportData InsertExcelReportDataString(int court_id, int reportTemplateId, int reportYear, int reportMonth, int sheetIndex, int rowIndex, int colIndex, string cellValue)
        {
            ExcelReportData exd = ExcelReportSetMainData(court_id, reportTemplateId, reportYear, reportMonth, sheetIndex, rowIndex, colIndex);
            exd.CellValue = cellValue;
            exd.CellValueType = NomenclatureConstants.ExcelReportCellValueTypes.StringValue;
            return exd;
        }

        private Expression<Func<CaseLifecycle, bool>> UnfinishedLifecycle(DateTime fromDate)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => x.DateFrom.Date < fromDate.Date && (x.DateTo ?? dateEnd).Date >= fromDate.Date;
            return reportTypeWhere;
        }

        private Expression<Func<CaseLifecycle, bool>> FinishedLifecycleMonths(DateTime fromDate, DateTime toDate,
             int fromMonths, int toMonths)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.DurationMonths >= fromMonths && x.DurationMonths <= toMonths;
            return reportTypeWhere;
        }

        private Expression<Func<CaseLifecycle, bool>> ContinueCase(DateTime fromDate, DateTime toDate)
        {
            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => x.Case.CaseLifecycles.Where(a => a.Id < x.Id &&
                                 a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                 .Any() &&
                x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date;
            return reportTypeWhere;
        }

        /// <summary>
        /// Обжалвани
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        private Expression<Func<Case, bool>> CaseComplain(DateTime fromDate, DateTime toDate)
        {
            Expression<Func<Case, bool>> reportTypeWhere = x => true;
            reportTypeWhere = x => repo.AllReadonly<CaseMigration>()
                                       .Where(a => a.DateExpired == null &&
                                       a.OutDocument.DocumentDate.Date >= fromDate.Date &&
                                       a.OutDocument.DocumentDate.Date <= toDate.Date &&
                                       a.CaseId == x.Id &&
                                       a.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel)
                                       .Any();
            return reportTypeWhere;
        }

        /// <summary>
        /// Свършили по същество или прекратени
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="onlyStop">true - само прекратени, false - без прекратените</param>
        /// <returns></returns>
        private Expression<Func<CaseLifecycle, bool>> FinishedLifecycleByType(DateTime fromDate, DateTime toDate, bool onlyStop)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

            var actComplainResults = repo.AllReadonly<ActComplainResultGrouping>()
           .Where(x => x.ActComplainResultGroup == NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop)
           .Select(x => x.ActComplainResultId)
           .ToArray();

            var actSessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsStopCase);

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    (actComplainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) ||
                                     x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                          a.IsMain && a.IsActive &&
                                          actSessionResults.Contains(a.SessionResultId)).Any()) == onlyStop;
            return reportTypeWhere;
        }

        private Expression<Func<Case, bool>> AllCaseByRegDate(DateTime fromDate, DateTime toDate)
        {
            Expression<Func<Case, bool>> reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date;
            return reportTypeWhere;
        }

        private Expression<Func<Case, bool>> CaseByRegDateNewNumber(DateTime fromDate, DateTime toDate)
        {
            Expression<Func<Case, bool>> reportTypeWhere = x => x.RegDate.Date >= fromDate.Date &&
             x.RegDate.Date <= toDate.Date && (x.IsNewCaseNewNumber ?? false) == true;
            return reportTypeWhere;
        }

        private int GetColFromReportComplainResults(List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults,
                int complainResultId)
        {
            return excelReportComplainResults
                   .Where(a => a.ActComplainResult.Contains(complainResultId))
                   .Select(a => a.Col)
                   .FirstOrDefault();
        }

        private int GetRowFromCaseCodeRows(List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, int caseCodeId)
        {
            return excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(caseCodeId))
                                                           .Select(a => a.RowIndex)
                                                           .FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="reportType"></param>
        /// <param name="courtTypeCaseTypesCols"></param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseLawUnitCaseType_Select(int courtTypeId, int courtId, int[] caseGroupIds,
            DateTime fromDate, DateTime toDate, int reportType, List<StatisticsExcelReportCaseTypeColVM> courtTypeCaseTypesCols)
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
            if (reportType == NomenclatureConstants.StatisticReportTypes.Unfinished)
                reportTypeWhere = UnfinishedLifecycle(fromDate);
            else if (reportType == NomenclatureConstants.StatisticReportTypes.Incoming)
                reportTypeWhere = x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date;
            else if (reportType == NomenclatureConstants.StatisticReportTypes.Finished3months)
                reportTypeWhere = FinishedLifecycleMonths(fromDate, toDate, 0, 3);
            else if (reportType == NomenclatureConstants.StatisticReportTypes.FinishedStop || reportType == NomenclatureConstants.StatisticReportTypes.FinishedNoStop)
            {
                if (reportType == NomenclatureConstants.StatisticReportTypes.FinishedStop)
                {
                    reportTypeWhere = FinishedLifecycleByType(fromDate, toDate, true);
                }
                else if (reportType == NomenclatureConstants.StatisticReportTypes.FinishedNoStop)
                {
                    reportTypeWhere = FinishedLifecycleByType(fromDate, toDate, false);
                }
            }

            var result = repo.AllReadonly<CaseLifecycle>()
                                .Where(x => x.DateExpired == null && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelCol = courtTypeCaseTypesCols.Where(a => a.CaseType.Contains(x.Case.CaseTypeId))
                .Where(a => a.ReportTypeId == reportType)
                .Where(a => ((a.DocumentType.Count == 0 || a.DocumentType.Contains(x.Case.Document.DocumentTypeId)) &&
                            (a.CaseCode.Count == 0 || a.CaseCode.Contains(x.Case.CaseCodeId ?? 0))) == a.IsTrue)
                .Select(a => a.ColIndex)
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
                                              .Where(a => (a.DateTo ?? dateEnd) >= (reportType == NomenclatureConstants.StatisticReportTypes.Unfinished ? fromDate : toDate))
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

        private List<ExcelReportData> SaveExcelByJudge(DateTime toDate, int rowIndex, List<CaseStatisticsVM> allData, int templateId,
                               int sheetIndex)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            var allCorts = allData.Select(x => x.CourtId)
                           .Distinct()
                           .ToList();

            int startRowIndex = rowIndex;
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
                startRowIndex = rowIndex;
                for (int j = 0; j < allJudge.Count; j++)
                {
                    result.Add(InsertExcelReportDataString(courtId, templateId, toDate.Year, toDate.Month,
                        sheetIndex, startRowIndex, 1, allJudge[j].name));

                    foreach (var item in allDataByCourt.Where(x => x.LawUnitId == allJudge[j].id))
                    {
                        result.Add(InsertExcelReportData(courtId, templateId, toDate.Year, toDate.Month,
                            sheetIndex, startRowIndex, item.ExcelCol, item.Count));
                    }

                    startRowIndex++;
                }
            }

            return result;
        }

        private List<ExcelReportData> SheetCaseCount(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            int[] caseGroupIds, int courtTypeId, List<StatisticsExcelReportCaseTypeColVM> courtTypeCaseTypesCols,
            int sheetIndex, int startRowIndex)
        {
            var allData = CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.Unfinished, courtTypeCaseTypesCols);

            allData.AddRange(CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.Incoming, courtTypeCaseTypesCols));

            allData.AddRange(CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.Finished3months, courtTypeCaseTypesCols));

            allData.AddRange(CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.FinishedStop, courtTypeCaseTypesCols));

            allData.AddRange(CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.FinishedNoStop, courtTypeCaseTypesCols));

            return SaveExcelByJudge(toDate, startRowIndex, allData, templateId, sheetIndex);
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
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
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

        private List<ExcelReportData> SheetActIndex(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            int[] caseGroupIds, int courtTypeId, List<StatisticsExcelReportIndexVM> excelReportIndexCols,
            int sheetIndex, int startRowIndex)
        {
            var allData = Index_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate, excelReportIndexCols);

            return SaveExcelByJudge(toDate, startRowIndex, allData, templateId, sheetIndex);
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
                reportTypeWhere = CaseByRegDateNewNumber(fromDate, toDate);
            }
            else if (reportType == 5)
            {
                reportTypeWhere = CaseComplain(fromDate, toDate);
            }
            else if (reportType == 6)
            {
                reportTypeWhere = AllCaseByRegDate(fromDate, toDate);
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
                                .Where(x => x.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(x => excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.CaseCodeId ?? 0)).Any())
                                .Where(reportTypeWhere)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    ExcelRow = GetRowFromCaseCodeRows(excelReportCaseCodeRows,
                                                 x.CaseCodeId ?? 0),
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
        /// 10 - От св.дела б.п.по чл. 356 НПК – прекратени и споразумения, 11 - Свършени ВЧНД</param>
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
                reportTypeWhere = ContinueCase(fromDate, toDate);

            }
            else if (reportType == 2)
            {
                reportTypeWhere = FinishedLifecycleMonths(fromDate, toDate, 0, 3);
            }
            else if (reportType == 3)
            {
                reportTypeWhere = FinishedLifecycleMonths(fromDate, toDate, 4, 6);
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
                var sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseStopND);

                var actTypes = new int[] {NomenclatureConstants.ActType.Answer, NomenclatureConstants.ActType.Definition,
                             NomenclatureConstants.ActType.Sentence };
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && actTypes.Contains(x.CaseSessionAct.ActTypeId) &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null && sessionResults.Contains(a.SessionResultId))
                                      .Any() == false;
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
                var sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseStopND);

                var actTypes = new int[] {NomenclatureConstants.ActType.Answer, NomenclatureConstants.ActType.Definition,
                             NomenclatureConstants.ActType.Sentence };
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && actTypes.Contains(x.CaseSessionAct.ActTypeId) &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null && sessionResults.Contains(a.SessionResultId))
                                      .Any() == false &&
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
            else if (reportType == 11)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.Case.CaseTypeId == NomenclatureConstants.CaseTypes.VChND;
            }

            var result = repo.AllReadonly<CaseLifecycle>()
                                .Where(x => x.DateExpired == null && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0)).Any())
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelRow = GetRowFromCaseCodeRows(excelReportCaseCodeRows,
                                                 x.Case.CaseCodeId ?? 0),
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
                                .Where(x => x.DateExpired == null && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
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
                                    ExcelRow = GetRowFromCaseCodeRows(excelReportCaseCodeRows,
                                                 x.Case.CaseCodeId ?? 0),
                                    ExcelCol = GetColFromReportComplainResults(excelReportComplainResults,
                                              x.CaseSessionAct.ActComplainResultId ?? 0),
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
                                       x.SessionTypeId == NomenclatureConstants.SessionType.ClosedSession &&  
                                       x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.GeneralOrder;
            }
            else if (reportType >= 8 && reportType <= 11)
            {
                reportTypeSpravka3Where = x => x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSession).Any() &&
                                       x.SessionTypeId == NomenclatureConstants.SessionType.ClosedSession &&
                                       x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick;
            }
            else if (reportType >= 15 && reportType <= 18)
            {
                reportTypeSpravka3Where = x => x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSession).Any() &&
                                              x.SessionTypeId == NomenclatureConstants.SessionType.ClosedSession;
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
            else if (reportType == 12 || reportType == 13)
            {
                var sessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsInvestigateND);
                var sessionResultBases = SessionResultBaseGrouping_Select(NomenclatureConstants.SessionResultBaseGroupings.StatisticsInvestigateND);
                int sessionTypeGroup = NomenclatureConstants.CaseSessionTypeGroup.PrivateSession;
                if (reportType == 13)
                    sessionTypeGroup = NomenclatureConstants.CaseSessionTypeGroup.PublicSession;

                reportTypeWhere = x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                    x.SessionType.SessionTypeGroup == sessionTypeGroup &&
                                       x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              sessionResults.Contains(a.SessionResultId) &&
                                              sessionResultBases.Contains(a.SessionResultBaseId ?? 0)).Any();
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
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
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
        /// 12 - От решените дела /кол. 10/ с ненаписани мотиви към присъдата с изтекъл  60-дневен срок,
        /// 13 - Дела относно основания за прекратяване на НП, 14 - Несвършили за възобновяване,
        /// 15 - Свършени по възобновяване, 16 - Решени възобновяване, 17 - Решени уважени възобновяване</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseLifecycleComplain_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId,
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
            else if (reportType == 13)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.Case.CaseTypeId == NomenclatureConstants.CaseTypes.VChND &&
                                    x.Case.CaseCode.Code == "8030";
            }
            else if (reportType == 14)
            {
                var documentResume = DocumentTypeGrouping_Select(NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND);

                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                 documentResume.Contains(x.Case.Document.DocumentTypeId);
            }
            else if (reportType == 15)
            {
                var documentResume = DocumentTypeGrouping_Select(NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                 documentResume.Contains(x.Case.Document.DocumentTypeId);
            }
            else if (reportType == 16)
            {
                var documentResume = DocumentTypeGrouping_Select(NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND);
                var complains = ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                 documentResume.Contains(x.Case.Document.DocumentTypeId) &&
                                 complains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) == false;
            }
            else if (reportType == 17)
            {
                var documentResume = DocumentTypeGrouping_Select(NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND);
                var complains = ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseSuccessful);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                 documentResume.Contains(x.Case.Document.DocumentTypeId) &&
                                 complains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0);
            }

            var result = repo.AllReadonly<CaseLifecycle>()
                                .Where(x => x.DateExpired == null && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex > 0 ? colIndex :
                                               GetColFromReportComplainResults(excelReportComplainResults,
                                                        x.CaseSessionAct.ActComplainResultId ?? 0),

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
        /// 12 - От решените дела /кол. 10/ с ненаписани мотиви към присъдата с изтекъл  60-дневен срок,
        /// 13 - Дела относно основания за прекратяване на НП, 14 - Несвършили за възобновяване,
        /// 15 - Свършени по възобновяване, 16 - Решени възобновяване,
        /// 17 - Решени уважени възобновяване</param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseLifecycle_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId)
        {
            return CaseLifecycleComplain_Select(courtTypeId, courtId, caseGroupIds,
                    fromDate, toDate, colIndex, rowIndex, reportType, instanceId, null);
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
        /// <param name="instanceId"></param>
        /// <param name="reportType">RSSheet2 - 1 -От решените дела /кол.9+10+11/“ с необявени решения с изтекъл срок над 3м.,
        /// 2 - Постановени решения по чл. 235, ал. 5 от ГПК, след проведено открито съдебно заседание,
        /// 3 - Постановени присъди, 4 - Постановени решения по НАХД, 
        /// 5 - Финализиращи решения постановени в открити заседания, 6 - Постановени присъди,
        /// 7 - Постановени Решения за АНД</param>
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
            else if (reportType == 3)
            {
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                    x.ActDeclaredDate <= toDate.ForceEndDate() &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Sentence &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession;
            }
            else if (reportType == 4)
            {
                var complainResults = ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop);

                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                    x.ActDeclaredDate <= toDate.ForceEndDate() &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Answer &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession &&
                                    x.Case.CaseTypeId == NomenclatureConstants.CaseTypes.VAND &&
                                    complainResults.Contains(x.ActComplainResultId ?? 0) == false;
            }
            else if (reportType == 5)
            {
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                    x.ActDeclaredDate <= toDate.ForceEndDate() &&
                                    x.IsFinalDoc == true &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Answer &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession;
            }
            else if (reportType == 6)
            {
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                    x.ActDeclaredDate <= toDate.ForceEndDate() &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Sentence;
            }
            else if (reportType == 7)
            {
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                    x.ActDeclaredDate <= toDate.ForceEndDate() &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Answer &&
                                    x.Case.CaseTypeId == NomenclatureConstants.CaseTypes.AND;
            }

            var result = repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
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
        /// 13 - Доживотен затвор, 14 - Доживотен затвор без право на замяна, 15 - Осъдени лица други наказания ОС и Военен</param>
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
                                              a.DateExpired == null && (a.IsActive ?? false))
                                   .Any();
            }
            else if (reportType == 3)
            {
                reportTypeWhere = x => x.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge &&
                                        x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null && (a.IsActive ?? false) &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null && b.IsMainPunishment).Any())
                                         .Any();
            }
            else if (reportType == 4 || reportType == 5)
            {
                int[] sentenceTypes;
                if (reportType == 4)
                    sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.ImprisonmentConditional ,
                                                NomenclatureConstants.SentenceTypes.ImprisonmentEffectively};
                else
                    sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.ImprisonmentConditional };

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null && (a.IsActive ?? false) &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null && b.IsMainPunishment &&
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
                                        a.DateExpired == null && (a.IsActive ?? false) &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null && b.IsMainPunishment &&
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
                                        a.DateExpired == null && (a.IsActive ?? false) &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null && b.IsMainPunishment &&
                                                                   sentenceTypes.Contains(b.SentenceTypeId ?? 0))
                                                                      .Any())
                                         .Any();
            }
            else if (reportType == 10)
            {
                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.SentenceResultTypeId == NomenclatureConstants.SentenceResultTypes.ConvictAgreement &&
                                        a.DateExpired == null && (a.IsActive ?? false))
                                         .Any();
            }
            else if (reportType == 15)
            {
                int[] sentenceTypes = new int[] { NomenclatureConstants.SentenceTypes.ImprisonmentConditional ,
                                                 NomenclatureConstants.SentenceTypes.ImprisonmentEffectively ,
                                                NomenclatureConstants.SentenceTypes.LifeSentence,
                                                NomenclatureConstants.SentenceTypes.LifeSentenceNoChange };

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDate.ForceStartDate() &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDate.ForceEndDate() &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null && (a.IsActive ?? false) &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null && b.IsMainPunishment &&
                                                                   sentenceTypes.Contains(b.SentenceTypeId ?? 0) == false)
                                                                      .Any())
                                         .Any();
            }

            var result = repo.AllReadonly<CasePerson>()
                                .Where(x => x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft)
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(x => x.CaseSessionId == null)
                                .Where(x => x.Case.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                                .Where(x => excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0)).Any())
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelRow = GetRowFromCaseCodeRows(excelReportCaseCodeRows,
                                                 x.Case.CaseCodeId ?? 0),
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
                                        a.DateExpired == null && (a.IsActive ?? false) &&
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
                                        a.DateExpired == null && (a.IsActive ?? false) &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null && b.IsMainPunishment &&
                                                                   sentenceTypes.Contains(b.SentenceTypeId ?? 0))
                                                                      .Any())
                                         .Any();
            }

            var result = repo.AllReadonly<CasePerson>()
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(x => x.CaseSessionId == null)
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
        /// <param name="reportType">1 - Спрени дела, 2 - Възовновени дела, 3 - по дата на делото, 4 - новообразувани по възобновяване</param>
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
                reportTypeWhere = AllCaseByRegDate(fromDate, toDate);
            }
            else if (reportType == 4)
            {
                var documentResume = DocumentTypeGrouping_Select(NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND);

                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                                                (x.IsNewCaseNewNumber ?? false) == false &&
                                 documentResume.Contains(x.Document.DocumentTypeId);
            }

            var result = repo.AllReadonly<Case>()
                                .Where(x => x.CaseStateId != NomenclatureConstants.CaseState.Draft)
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Where(documentTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    FromCourtData = groupByFromCourt == true ? x.Document.DocumentCaseInfo
                                            .Select(a => a.CourtId + ",," + a.Court.Label + ",," +
                                              (a.Court.ParentCourtId == x.CourtId ? 0 : 1))
                                            .FirstOrDefault() : "",
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
        /// <param name="reportType">1 - Спрени дела, 2 - Възовновени дела, 3 - по дата на делото, 4 - новообразувани по възобновяване</param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
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
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(x => (x.IsActive ?? false) == true)
                                .Where(x => x.Case.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                                .Where(x => excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(x.Case.CaseCodeId ?? 0)).Any())
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    ExcelRow = GetRowFromCaseCodeRows(excelReportCaseCodeRows,
                                                 x.Case.CaseCodeId ?? 0),
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
        private List<ExcelReportData> RSSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

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

            //Справка 4
            allData.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 64, 5, instanceId));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private List<ExcelReportData> RSSheet3(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, 
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.RegionalCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.FirstInstance;

            //Един път се пуска за кодовете, които са по един и един път за тези които са сумарни
            for (int i = 0; i < 2; i++)
            {
                List<StatisticsExcelReportCaseCodeRowVM> caseCodes = null;
                if (i == 0)
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count == 1 && x.SheetIndex == 3).ToList();
                else
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count > 1 && x.SheetIndex == 3).ToList();

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

            //Справка 2
            //Един път се пуска за кодовете, които са по един и един път за тези които са сумарни
            for (int i = 0; i < 2; i++)
            {
                List<StatisticsExcelReportCaseCodeRowVM> caseCodes = null;
                if (i == 0)
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count == 1 && x.SheetIndex == 103).ToList();
                else
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count > 1 && x.SheetIndex == 103).ToList();

                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId));
                allData.AddRange(CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId));

                allData.AddRange(CaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes,
                      excelReportComplainResults, instanceId));
            }

            //Справка 3
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 134, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId));
            allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 135, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId));
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

            //Справка 7
            allData.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 168, 6, instanceId));
            allData.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 169, 7, instanceId));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private List<ExcelReportData> MillitarySheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

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
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 15, instanceId));
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

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private List<ExcelReportData> OSSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

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
                allData.AddRange(CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo },
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

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private List<ExcelReportData> OSSheet4(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

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
                allData.AddRange(CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 28, 15, instanceId));
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

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    4, item.ExcelRow, item.ExcelCol, item.Count));
            }

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

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = UnfinishedLifecycle(fromDate);
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date;
            }
            else if (reportType == 3)
            {
                reportTypeWhere = FinishedLifecycleByType(fromDate, toDate, true);
            }
            else if (reportType == 4)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date;
            }


            var result = repo.AllReadonly<CaseLifecycle>()
                                .Where(x => x.DateExpired == null && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
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
                                          GetColFromReportComplainResults(excelReportComplainResults,
                                              x.CaseSessionAct.ActComplainResultId ?? 0),
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

        private List<ExcelReportData> SaveExcelByFromCourt(DateTime toDate, int rowIndex, List<CaseStatisticsVM> allData, int templateId,
                               int sheetIndex)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int startRowIndex = rowIndex;
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
                startRowIndex = rowIndex;
                for (int j = 0; j < allFromCourt.Count; j++)
                {
                    result.Add(InsertExcelReportDataString(courtId, templateId, toDate.Year, toDate.Month,
                        sheetIndex, startRowIndex, 0, allFromCourt[j].fromCourtName));

                    foreach (var item in allDataByCourt.Where(x => x.FromCourtId == allFromCourt[j].fromCourtId))
                    {
                        result.Add(InsertExcelReportData(courtId, templateId, toDate.Year, toDate.Month,
                            sheetIndex, startRowIndex, item.ExcelCol, item.Count));
                    }

                    startRowIndex++;
                }
            }

            return result;
        }

        private List<ExcelReportData> ApealSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

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


            result.AddRange(SaveExcelByFromCourt(toDate, 14, allData, templateId, 2));

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

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }
            return result;
        }
        private List<ExcelReportData> ApealSheet3(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
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
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults.Where(x => x.SheetIndex == 3).ToList()));

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

            result.AddRange(SaveExcelByFromCourt(toDate, 12, allData, templateId, 3));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 40, 1, null, instanceId));
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 41, 2, null, instanceId));

            //Справка 3
            allDataGroup.AddRange(CaseLifecycleComplain_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 0, 48, 13, instanceId,
                               excelReportComplainResults.Where(x => x.SheetIndex == 103).ToList()));

            //Справка 4
            allDataGroup.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, excelReportCaseCodeRows,
                             13, 13, instanceId));

            //Справка 5
            allDataGroup.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 23,
                             46, 14, instanceId));
            allDataGroup.AddRange(Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 23,
                             47, 4, instanceId));
            allDataGroup.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 23,
                             48, 15, instanceId));
            allDataGroup.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 23,
                             49, 16, instanceId));
            allDataGroup.AddRange(CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 23,
                             50, 17, instanceId));
            allDataGroup.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             54, 3, instanceId));
            allDataGroup.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             55, 4, instanceId));


            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count));
            }
            return result;
        }

        private List<ExcelReportData> MillitaryAPSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.MillitaryApeal;
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
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList()));

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

            result.AddRange(SaveExcelByFromCourt(toDate, 12, allData, templateId, 2));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 26, 1, null, instanceId));
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 27, 2, null, instanceId));

            //Справка 3
            allDataGroup.AddRange(CaseLifecycleComplain_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 0, 36, 13, instanceId,
                               excelReportComplainResults.Where(x => x.SheetIndex == 102).ToList()));

            //Справка 4
            allDataGroup.AddRange(CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, excelReportCaseCodeRows,
                             16, 13, instanceId));

            //Справка 5
            allDataGroup.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             42, 3, instanceId));
            allDataGroup.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             43, 4, instanceId));


            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private List<ExcelReportData> OSSheet3(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD,
                    NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD};

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
                  caseGroups, fromDate, toDate, 10, 3, complains, null));

            //Колони за Частни Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 12, 1, complains, null));

            allData.AddRange(CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 13, 0, 3, instanceId, complains, true));

            allData.AddRange(CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 14, 4, complains, null));

            result.AddRange(SaveExcelByFromCourt(toDate, 13, allData, templateId, 3));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 30, 1, null, instanceId));
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 31, 2, null, instanceId));
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 32, 19, null, instanceId));

            //Справка 2
            allDataGroup.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 39, 2, instanceId));

            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }
        private List<ExcelReportData> OSSheet5(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND,
                           NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND,
            NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND};

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
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults.Where(x => x.SheetIndex == 5).ToList()));

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

            result.AddRange(SaveExcelByFromCourt(toDate, 13, allData, templateId, 5));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 30, 1, null, instanceId));
            allDataGroup.AddRange(CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 31, 2, null, instanceId));

            allDataGroup.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             36, 3, instanceId));
            allDataGroup.AddRange(CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             37, 4, instanceId));


            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    5, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private int GetRowFromCaseTypeRows(List<StatisticsExcelReportCaseTypeRowVM> caseTypesRows, int caseTypeId, int colIndex,
            int documentTypeId, int caseCodeId)
        {
            return caseTypesRows.Where(x => x.CaseType.Contains(caseTypeId))
                .Where(x => x.ForColumns.Contains(colIndex))
                .Where(a => ((a.DocumentType.Count == 0 || a.DocumentType.Contains(documentTypeId)) &&
                            (a.CaseCode.Count == 0 || a.CaseCode.Contains(caseCodeId))) == a.IsTrue)
                .Select(x => x.RowIndex)
                .FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="reportType">1 - несвършени дела в началото на отчетния период, 2 - Свършени до 3 месеца вкл.,
        /// 3 - Свършели през прекратени, 4 - Свършили само прекратени, 5 - Продължаващи под същия номер, 
        /// 6 - Споразу- мения по чл.382 НПК, 7 - Споразум. по чл.384 НПК , спог. по чл.24 ал. 3 НПК или чл.234 ГПК,
        /// 8 - Върнати за доразследване, 9 - Прекратени по други причини без 6,7,8</param>
        /// <param name="colIndex"></param>
        /// <param name="caseTypesRows"></param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseTypeLifecycle_Select(int courtTypeId, int courtId,
            DateTime fromDate, DateTime toDate, int reportType, int colIndex,
            List<StatisticsExcelReportCaseTypeRowVM> caseTypesRows)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
                reportTypeWhere = UnfinishedLifecycle(fromDate);
            else if (reportType == 2)
                reportTypeWhere = FinishedLifecycleMonths(fromDate, toDate, 0, 3);
            else if (reportType == 3)
                reportTypeWhere = FinishedLifecycleByType(fromDate, toDate, false);
            else if (reportType == 4)
                reportTypeWhere = FinishedLifecycleByType(fromDate, toDate, true);
            else if (reportType == 5)
                reportTypeWhere = ContinueCase(fromDate, toDate);
            else if (reportType == 6)
            {
                var actComplains = ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop382);
                reportTypeWhere = reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.IsFinalDoc &&
                                    actComplains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                    a.SessionResultId == NomenclatureConstants.CaseSessionResult.WithAgreement).Any();
            }
            else if (reportType == 7)
            {
                var actComplains = ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop384);
                reportTypeWhere = reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.IsFinalDoc &&
                                    actComplains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0);
            }
            else if (reportType == 8)
            {
                reportTypeWhere = reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.IsFinalDoc &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                    a.SessionResultId == NomenclatureConstants.CaseSessionResult.SuspendedInvestigation).Any();
            }
            else if (reportType == 9)
            {
                var actComplains = ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop382);
                var actComplains384 = ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop384);

                var actComplainResults = repo.AllReadonly<ActComplainResultGrouping>()
               .Where(x => x.ActComplainResultGroup == NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop)
               .Select(x => x.ActComplainResultId)
               .ToArray();

                var actSessionResults = SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsStopCase);

                reportTypeWhere = reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.IsFinalDoc &&
                                    (actComplains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                    a.SessionResultId == NomenclatureConstants.CaseSessionResult.WithAgreement).Any()) == false &&
                                    actComplains384.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) == false &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                    a.SessionResultId == NomenclatureConstants.CaseSessionResult.SuspendedInvestigation).Any() == false &&
                                    (actComplainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) ||
                                         x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              a.IsMain && a.IsActive &&
                                              actSessionResults.Contains(a.SessionResultId)).Any()) == true;
            }

            var result = repo.AllReadonly<CaseLifecycle>()
                                .Where(x => x.DateExpired == null && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelCol = colIndex,
                                    ExcelRow = GetRowFromCaseTypeRows(caseTypesRows, x.Case.CaseTypeId, colIndex,
                                                  x.Case.Document.DocumentTypeId, x.Case.CaseCodeId ?? 0),

                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelCol,
                                    x.ExcelRow,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelCol = x.Key.ExcelCol,
                                    ExcelRow = x.Key.ExcelRow,
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
        /// <param name="reportType">1 - всички новообразувани, 2 - образувани под нов номер,
        /// 3 - Обжалвани, 4 - Под нов номер след прекратяване, 5 - Обр. под нов No дела при повторно пост. въззивни жалби - ПАС,
        ///</param>
        /// <param name="colIndex"></param>
        /// <param name="caseTypesRows"></param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseTypeCase_Select(int courtTypeId, int courtId,
            DateTime fromDate, DateTime toDate, int reportType, int colIndex,
            List<StatisticsExcelReportCaseTypeRowVM> caseTypesRows)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<Case, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<Case, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<Case, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
                reportTypeWhere = AllCaseByRegDate(fromDate, toDate);
            else if (reportType == 2)
                reportTypeWhere = CaseByRegDateNewNumber(fromDate, toDate);
            else if (reportType == 3)
                reportTypeWhere = CaseComplain(fromDate, toDate);
            else if (reportType == 4)
            {
                int[] migrations = { NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction };
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                                      (x.IsNewCaseNewNumber ?? false)  == true &&
                                      repo.AllReadonly<CaseMigration>()
                                       .Where(a => a.DateExpired == null &&
                                       a.CaseId == x.Id &&
                                       migrations.Contains(a.CaseMigrationTypeId))
                                       .Any();
            }
            else if (reportType == 5)
            {
                var sessionResultFinish = repo.AllReadonly<SessionResultGrouping>()
                     .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseMigrationReturnReport_FinishCase)
                     .Select(x => x.SessionResultId)
                     .ToList();

                var sessionResultFinishBase = repo.AllReadonly<SessionResultBaseGrouping>()
                     .Where(x => x.SessionResultBaseGroup == NomenclatureConstants.SessionResultBaseGroupings.CaseMigrationReturnReport_FinishCaseBase)
                     .Select(x => x.SessionResultBaseId)
                     .ToList();

                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                                      (x.IsNewCaseNewNumber ?? false) == true &&
                                      repo.AllReadonly<CaseMigration>()
                .Where(a => a.CaseId == x.Id)
                .Where(a => NomenclatureConstants.CaseMigrationTypes.ReturnCaseTypes.Contains(a.OutCaseMigration.OutCaseMigration.OutCaseMigration.CaseMigrationTypeId))
                .Where(a => a.ReturnCase.CaseSessions.Where(b => b.DateExpired == null &&
                                        b.CaseSessionResults.Where(c => c.IsActive && c.IsMain &&
                                         sessionResultFinish.Contains(c.SessionResultId) &&
                                         sessionResultFinishBase.Contains(c.SessionResultBaseId ?? 0)
                                         ).Any()).Any()).Any();
            }

            var result = repo.AllReadonly<Case>()
                                .Where(x => x.CaseStateId != NomenclatureConstants.CaseState.Draft)
                                .Where(x => x.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    ExcelCol = colIndex,
                                    ExcelRow = GetRowFromCaseTypeRows(caseTypesRows, x.CaseTypeId, colIndex,
                                                  x.Document.DocumentTypeId, x.CaseCodeId ?? 0),

                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelCol,
                                    x.ExcelRow,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelCol = x.Key.ExcelCol,
                                    ExcelRow = x.Key.ExcelRow,
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
        /// <param name="reportType">1 - всички Проведени заседания</param>
        /// <param name="colIndex"></param>
        /// <param name="caseTypesRows"></param>
        /// <returns></returns>
        private List<CaseStatisticsVM> CaseTypeCaseSession_Select(int courtTypeId, int courtId,
            DateTime fromDate, DateTime toDate, int reportType, int colIndex,
            List<StatisticsExcelReportCaseTypeRowVM> caseTypesRows)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseSession, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseSession, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseSession, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
                reportTypeWhere = x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                       x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession;

            var result = repo.AllReadonly<CaseSession>()
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date)
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Case.CourtId,
                                    ExcelCol = colIndex,
                                    ExcelRow = GetRowFromCaseTypeRows(caseTypesRows, x.Case.CaseTypeId, colIndex,
                                                  x.Case.Document.DocumentTypeId, x.Case.CaseCodeId ?? 0),

                                })
                                .GroupBy(x => new
                                {
                                    x.CourtId,
                                    x.ExcelCol,
                                    x.ExcelRow,
                                })
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Key.CourtId,
                                    ExcelCol = x.Key.ExcelCol,
                                    ExcelRow = x.Key.ExcelRow,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        private List<ExcelReportData> ApealSheet1(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows, int courtTypeId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            var allData = CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 3, excelReportCaseTypeRows);

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 4, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 5, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 8, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 10, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 11, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCaseSession_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 12, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 14, excelReportCaseTypeRows));


            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    1, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private List<ExcelReportData> DistrictSheet1(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows)
        {
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;

            List<ExcelReportData> result = new List<ExcelReportData>();

            var allData = CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 3, excelReportCaseTypeRows);

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 4, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 5, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 6, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 7, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 8, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 12, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 14, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 6, 16, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 7, 17, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 8, 18, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 9, 19, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCaseSession_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 20, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 22, excelReportCaseTypeRows));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    1, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private List<ExcelReportData> MillitarySheet1(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows)
        {
            int courtTypeId = NomenclatureConstants.CourtType.Millitary;

            List<ExcelReportData> result = new List<ExcelReportData>();

            var allData = CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 3, excelReportCaseTypeRows);

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 4, excelReportCaseTypeRows));


            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 5, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 6, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 10, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 12, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 6, 14, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 7, 15, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 8, 16, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 9, 17, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCaseSession_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 18, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 20, excelReportCaseTypeRows));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    1, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private List<ExcelReportData> RSSheet1(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows)
        {
            int courtTypeId = NomenclatureConstants.CourtType.RegionalCourt;

            List<ExcelReportData> result = new List<ExcelReportData>();

            var allData = CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 3, excelReportCaseTypeRows);

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 4, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 5, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 6, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 7, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 11, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 13, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 6, 15, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 7, 16, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 8, 17, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 9, 18, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCaseSession_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 19, excelReportCaseTypeRows));

            allData.AddRange(CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 21, excelReportCaseTypeRows));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    1, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private List<ExcelReportData> RSExcelTitle(DateTime toDate, int searchCourtId, int templateId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToList();
            string month = toDate.Month.ToString();
            string year = toDate.Year.ToString();
            int intMonth = toDate.Month;
            int intYear = toDate.Year;
            foreach (var item in courts)
            {
                //Sheet1
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 0, 1, "Отчет за работата на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 0, 12, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 0, 13, "месеца на " + year + " г."));

                //Sheet2
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 0, "  О Т Ч Е Т по гражданските дела на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 12, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 13, "месеца на " + year + " г."));

                //Sheet3
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 0, 0, "  О Т Ч Е Т по наказателните дела на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 0, 12, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 0, 13, "месеца на " + year + " г."));

                //Sheet4
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    4, 0, 2, "Справка за дейността на съдиите в " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    4, 1, 2, "за " + month + " месеца на " + year + " г. (НАКАЗАТЕЛНИ ДЕЛА)"));

                //Sheet5
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    5, 1, 2, "Справка за резултатите от върнати обжалвани и протестирани НАКАЗАТЕЛНИТЕ дела на съдиите " +
                        "от " + item.Label + " през " + month + " месеца на " + year + " г."));

                //Sheet6
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    6, 1, 2, "Справка за дейността на съдиите в " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    6, 2, 2, "за  " + month + " месеца на " + year + " г. (ГРАЖДАНСКИ  ДЕЛА)"));

                //Sheet7
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    7, 1, 1, "Справка за резултатите от върнати обжалвани и протестирани ГРАЖДАНСКИ и ТЪРГОВСКИ дела на съдиите от " +
                    item.Label + " през " + month + " месеца на " + year + " г."));
            }

            return result;
        }

        private List<ExcelReportData> OSExcelTitle(DateTime toDate, int searchCourtId, int templateId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToList();
            string month = toDate.Month.ToString();
            string year = toDate.Year.ToString();
            int intMonth = toDate.Month;
            int intYear = toDate.Year;
            foreach (var item in courts)
            {
                //Sheet1
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 2, "Отчет за работата на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 12, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 13, "месеца на " + year + " г."));

                //Sheet2
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 0, "  ОТЧЕТ по граждански, търговски и фирмени дела І инст.  на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 13, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 14, "месеца на " + year + " г."));

                //Sheet3
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 1, 0, " О Т Ч Е Т   по гражданските дела ІІ инст. на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 1, 12, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 1, 13, "месеца на " + year + " г."));

                //Sheet4
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    4, 0, 0, "О Т Ч Е Т по наказателните дела І инстанция  на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    4, 0, 12, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    4, 0, 13, "месеца на " + year + " г."));

                //Sheet5
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    5, 1, 0, "  ОТЧЕТ  по наказателните дела  ІІ инст.  на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    5, 1, 12, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    5, 1, 13, "месеца на " + year + " г."));

                //Sheet6
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    6, 1, 2, "Справка за дейността на съдиите в " + item.Label + " през " + month + " " +
                    year + " г. (НАКАЗАТЕЛНИ ДЕЛА)"));


                //Sheet7
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    7, 1, 2, "Справка за резултатите от върнати обжалвани и протестирани НАКАЗАТЕЛНИ дела на съдиите от " +
                    item.Label + " през " + month + " месеца на " + year + " г."));

                //Sheet8
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    8, 1, 2, "Справка за дейността на съдиите в " + item.Label + " през " + month +
                    " " + year + " г. (ГРАЖДАНСКИ  И ТЪРГОВСКИ ДЕЛА)"));

                //Sheet9
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    9, 1, 2, "Справка за резултатите от върнати обжалвани и протестирани ГРАЖДАНСКИ и ТЪРГОВСКИ дела на съдиите от " +
                    item.Label + " през " + month + " " + year + " г."));
            }

            return result;
        }

        private List<ExcelReportData> ApealExcelTitle(DateTime toDate, int searchCourtId, int templateId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToList();
            string month = toDate.Month.ToString();
            string year = toDate.Year.ToString();
            int intMonth = toDate.Month;
            int intYear = toDate.Year;
            foreach (var item in courts)
            {
                //Sheet1
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 1, "Отчет за работата на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 11, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 12, "месеца на " + year + " г."));

                //Sheet2
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 1, 0, "0 Т Ч Е Т  по граждански и търговски  дела на  " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 1, 12, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 1, 13, "месеца на " + year + " г."));

                //Sheet3
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 0, 0, "0 Т Ч Е Т  по наказателните дела на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 0, 11, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 0, 12, "месеца на " + year + " г."));

                //Sheet4
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    4, 1, 2, "Справка за дейността на съдиите в " + item.Label + " през " + month + " " + 
                    year + " г. (НАКАЗАТЕЛНИ  ДЕЛА)"));

                //Sheet5
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    5, 1, 2, "Справка за резултатите от върнати обжалвани и протестирани НАКАЗАТЕЛНИ дела на съдиите от " +
                    item.Label + " през " + month + " " + year + " г."));


                //Sheet6
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    6, 1, 3, "Справка за дейността на съдиите в " + item.Label + " през " + month + " " +
                    year + " г. (ГРАЖДАНСКИ  И ТЪРГОВСКИ ДЕЛА)"));


                //Sheet7
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    7, 1, 2, "Справка за резултатите от върнати обжалвани и протестирани ГРАЖДАНСКИ И ТЪРГОВСКИ дела на съдиите от " +
                    item.Label + " през " + month + " месеца на " + year + " г."));

            }

            return result;
        }

        private List<ExcelReportData> MillitaryExcelTitle(DateTime toDate, int searchCourtId, int templateId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Millitary)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToList();
            string month = toDate.Month.ToString();
            string year = toDate.Year.ToString();
            int intMonth = toDate.Month;
            int intYear = toDate.Year;
            foreach (var item in courts)
            {
                //Sheet1
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 0, "Отчет за работата на " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 12, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 13, "месеца на " + year + " г."));

                //Sheet2
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 0, "  О Т Ч Е Т   по наказателните дела  І инстанция на  " + item.Label));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 12, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 13, "месеца на " + year + " г."));

                //Sheet3
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 1, 2, "Справка за дейността на съдиите във " + item.Label + " през " + month + " " + year + " г."));

                //Sheet4
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    4, 1, 2, "Справка за резултатите от върнати обжалвани и протестирани НАКАЗАТЕЛНИ дела на съдиите от " +
                   item.Label + " през " + month + " " + year + " г."));
            }

            return result;
        }

        private List<ExcelReportData> MillitaryApealExcelTitle(DateTime toDate, int searchCourtId, int templateId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.MillitaryApeal)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToList();
            string month = toDate.Month.ToString();
            string year = toDate.Year.ToString();
            int intMonth = toDate.Month;
            int intYear = toDate.Year;
            foreach (var item in courts)
            {
                //Sheet1
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 11, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 12, "месеца на " + year + " г."));

                //Sheet2
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 11, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    2, 0, 12, "месеца на " + year + " г."));

                //Sheet3
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    3, 1, 2, "Справка за дейността на съдиите във Военно-апелативния съд през " + month + " " + year + " г."));

                //Sheet4
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    4, 1, 2, "Справка за резултатите от върнати обжалвани и протестирани НАКАЗАТЕЛНИТЕ дела на съдиите от ВОЕННО - АПЕЛАТИВЕН СЪД гр.София през " + 
                    month + " " + year + " г."));
            }

            return result;
        }
    }
}
