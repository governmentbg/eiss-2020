using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Integrations.Sisma;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Core.Services
{
    public class StatisticsReportService : BaseService, IStatisticsReportService
    {
        public StatisticsReportService(
            IRepository _repo,
            IUserContext _userContext
            )
        {
            repo = _repo;
            userContext = _userContext;
        }

        private async Task<int[]> SessionResultGrouping_Select(int groupId)
        {
            return await repo.AllReadonly<SessionResultGrouping>()
                                      .Where(x => x.SessionResultGroup == groupId)
                                      .Select(x => x.SessionResultId)
                                      .ToArrayAsync().ConfigureAwait(false);
        }

        private int GetFromCourtType(int courtTypeId)
        {
            int result = 0;
            switch (courtTypeId)
            {
                case NomenclatureConstants.CourtType.DistrictCourt:
                    result = NomenclatureConstants.CourtType.RegionalCourt;
                    break;
                case NomenclatureConstants.CourtType.Apeal:
                    result = NomenclatureConstants.CourtType.DistrictCourt;
                    break;
                case NomenclatureConstants.CourtType.MillitaryApeal:
                    result = NomenclatureConstants.CourtType.Millitary;
                    break;
                default:
                    result = 0;
                    break;
            }

            return result;
        }

        private async Task<int[]> DocumentTypeGrouping_Select(int groupId)
        {
            return await repo.AllReadonly<DocumentTypeGrouping>()
                                      .Where(x => x.DocumentTypeGroup == groupId)
                                      .Select(x => x.DocumentTypeId)
                                      .ToArrayAsync().ConfigureAwait(false);
        }

        private async Task<int[]> ActComplainResultGrouping_Select(int groupId)
        {
            return await repo.AllReadonly<ActComplainResultGrouping>()
                                      .Where(x => x.ActComplainResultGroup == groupId)
                                      .Select(x => x.ActComplainResultId)
                                      .ToArrayAsync().ConfigureAwait(false);
        }

        private async Task<int[]> SessionResultBaseGrouping_Select(int groupId)
        {
            return await repo.AllReadonly<SessionResultBaseGrouping>()
                                      .Where(x => x.SessionResultBaseGroup == groupId)
                                      .Select(x => x.SessionResultBaseId)
                                      .ToArrayAsync().ConfigureAwait(false);
        }

        private async Task<List<ExcelReportData>> RS_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData,
            int searchCourtId, string paramValue)
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

            result.AddRange(await SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                 courtTypeId, excelReportCaseTypeCols, 4, 8).ConfigureAwait(false));
            result.AddRange(await SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                   new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo },
                   courtTypeId, excelReportCaseTypeCols, 6, 9).ConfigureAwait(false));

            result.AddRange(await SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                 courtTypeId, excelReportIndexCols.Where(x => x.SheetIndex == 5).ToList(), 5, 8).ConfigureAwait(false));
            result.AddRange(await SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade,
                             NomenclatureConstants.CaseGroups.Company},
                   courtTypeId, excelReportIndexCols.Where(x => x.SheetIndex == 7).ToList(), 7, 7).ConfigureAwait(false));

            result.AddRange(await SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                   courtTypeId, excelReportIndexCols.Where(x => x.SheetIndex == 8).ToList(), 8, 7).ConfigureAwait(false));

            //Sheet 2 Приложение ГД
            if (IsSystemInFeature(paramValue, NomenclatureConstants.SystemFeatures.Request9Stats))
            {
                result.AddRange(await RSSheet2Request9Stats(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 102).ToList(),
                excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList()).ConfigureAwait(false));
            }
            else
            {
                result.AddRange(await RSSheet2(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList(),
                excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList()).ConfigureAwait(false));
            }

            //Sheet 3 Приложение НД

            if (IsSystemInFeature(paramValue, NomenclatureConstants.SystemFeatures.Request9Stats))
            {
                result.AddRange(await RSSheet3Request9Stats(fromDate, toDate, searchCourtId, templateId,
               excelReportCaseCodeRows.Where(x => x.SheetIndex == 130 || x.SheetIndex == 1003).ToList(),
                excelReportComplainResults.Where(x => x.SheetIndex == 103).ToList()).ConfigureAwait(false));
            }
            else
            {
                result.AddRange(await RSSheet3Request7(fromDate, toDate, searchCourtId, templateId,
               excelReportCaseCodeRows.Where(x => x.SheetIndex == 3 || x.SheetIndex == 103).ToList(),
                excelReportComplainResults.Where(x => x.SheetIndex == 103).ToList()).ConfigureAwait(false));
            }


            //Sheet 1 Приложение 1
            result.AddRange(await RSSheet1(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows.Where(x => x.SheetIndex == 0).ToList()).ConfigureAwait(false));

            result.AddRange(await RSExcelTitle(toDate, searchCourtId, templateId).ConfigureAwait(false));

            return result;
        }

        private async Task<List<ExcelReportData>> OS_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData,
            int searchCourtId, string paramValue)
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

            List<StatisticsExcelReportIspnReasonVM> excelReportIspnReasons = nomData.excelReportIspnReasons
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            result.AddRange(await SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                 courtTypeId, excelReportCaseTypeCols, 6, 8).ConfigureAwait(false));
            result.AddRange(await SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade,
                             NomenclatureConstants.CaseGroups.Company},
                   courtTypeId, excelReportCaseTypeCols, 8, 8).ConfigureAwait(false));

            result.AddRange(await SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                 new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                 courtTypeId, excelReportIndexCols.Where(x => x.SheetIndex == 7).ToList(), 7, 7).ConfigureAwait(false));
            result.AddRange(await SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade,
                             NomenclatureConstants.CaseGroups.Company},
                   courtTypeId, excelReportIndexCols.Where(x => x.SheetIndex == 9).ToList(), 9, 7).ConfigureAwait(false));

            //Sheet 2 Приложение ГД, ТД, ФД
            result.AddRange(await OSSheet2(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList(),
                excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList(), paramValue).ConfigureAwait(false));

            //Sheet 3 Приложение ГД
            if (IsSystemInFeature(paramValue, NomenclatureConstants.SystemFeatures.Request9Stats))
            {
                result.AddRange(await OSSheet3Request9Stats(fromDate, toDate, searchCourtId, templateId, excelReportComplainResults.Where(x => x.SheetIndex == 103).ToList()).ConfigureAwait(false));
            }
            else
            {
                result.AddRange(await OSSheet3(fromDate, toDate, searchCourtId, templateId, excelReportComplainResults.Where(x => x.SheetIndex == 3).ToList()).ConfigureAwait(false));
            }

            //Sheet 4 Приложение НД
            if (IsSystemInFeature(paramValue, NomenclatureConstants.SystemFeatures.Request9Stats))
            {
                result.AddRange(await OSSheet4Request9Stats(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 104).ToList()).ConfigureAwait(false));
            }
            else
            {
                result.AddRange(await OSSheet4Request7(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 4).ToList()).ConfigureAwait(false));
            }

            //Sheet 5 Приложение НД
            if (IsSystemInFeature(paramValue, NomenclatureConstants.SystemFeatures.Request9Stats))
            {
                result.AddRange(await OSSheet5Request9Stats(fromDate, toDate, searchCourtId, templateId,
                   excelReportComplainResults.Where(x => x.SheetIndex == 105).ToList()).ConfigureAwait(false));
            }
            else
            {
                result.AddRange(await OSSheet5(fromDate, toDate, searchCourtId, templateId,
                   excelReportComplainResults.Where(x => x.SheetIndex == 5).ToList()).ConfigureAwait(false));
            }

            //Sheet 1 Приложение 1
            if (IsSystemInFeature(paramValue, NomenclatureConstants.SystemFeatures.Request9Stats))
            {
                result.AddRange(await DistrictSheet1Request9Stats(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows.Where(x => x.SheetIndex == 1).ToList(),
                            excelReportIspnReasons.Where(x => x.SheetIndex == 101).ToList()).ConfigureAwait(false));
            }
            else
            {
                result.AddRange(await DistrictSheet1(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows.Where(x => x.SheetIndex == 0).ToList(),
                    excelReportIspnReasons.Where(x => x.SheetIndex == 1).ToList()).ConfigureAwait(false));
            }

            result.AddRange(await OSExcelTitle(toDate, searchCourtId, templateId).ConfigureAwait(false));

            return result;
        }

        private async Task<List<ExcelReportData>> AP_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData,
            int searchCourtId, string paramValue)
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

            result.AddRange(await SheetCaseCount(fromDate, toDate, searchCourtId, templateId, new int[]
              { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, courtTypeId,
              excelReportCaseTypeCols, 4, 8).ConfigureAwait(false));
            result.AddRange(await SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                    new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade },
                    courtTypeId, excelReportCaseTypeCols, 6, 8).ConfigureAwait(false));

            result.AddRange(await SheetActIndex(fromDate, toDate, searchCourtId, templateId, new int[]
              { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, courtTypeId,
              excelReportIndexCols.Where(x => x.SheetIndex == 5).ToList(), 5, 7).ConfigureAwait(false));
            result.AddRange(await SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade,
                             NomenclatureConstants.CaseGroups.Company},
                    courtTypeId, excelReportIndexCols.Where(x => x.SheetIndex == 7).ToList(), 7, 7).ConfigureAwait(false));

            //Sheet 2 Приложение ГД/ТД
            if (IsSystemInFeature(paramValue, NomenclatureConstants.SystemFeatures.Request9Stats))
            {
                result.AddRange(await ApealSheet2Request9Stats(fromDate, toDate, searchCourtId, templateId, excelReportComplainResults.Where(x => x.SheetIndex == 102).ToList()).ConfigureAwait(false));
            }
            else
            {
                result.AddRange(await ApealSheet2(fromDate, toDate, searchCourtId, templateId, excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList()).ConfigureAwait(false));
            }

            //Sheet 3 Приложение НД
            if (IsSystemInFeature(paramValue, NomenclatureConstants.SystemFeatures.Request9Stats))
            {
                result.AddRange(await ApealSheet3Request9Stats(fromDate, toDate, searchCourtId, templateId,
                   excelReportComplainResults.Where(x => x.SheetIndex == 30 || x.SheetIndex == 130).ToList(),
                   excelReportCaseCodeRows.Where(x => x.SheetIndex == 3).ToList()).ConfigureAwait(false));
            }
            else
            {
                result.AddRange(await ApealSheet3(fromDate, toDate, searchCourtId, templateId,
                   excelReportComplainResults.Where(x => x.SheetIndex == 3 || x.SheetIndex == 103).ToList(),
                   excelReportCaseCodeRows.Where(x => x.SheetIndex == 3).ToList()).ConfigureAwait(false));
            }

            //Sheet 1 Приложение 1
            if (IsSystemInFeature(paramValue, NomenclatureConstants.SystemFeatures.Request9Stats))
            {
                result.AddRange(await ApealSheet1Request9Stats(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows.Where(x => x.SheetIndex == 1).ToList(), courtTypeId).ConfigureAwait(false));
            }
            else
            {
                result.AddRange(await ApealSheet1(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows.Where(x => x.SheetIndex == 0).ToList(), courtTypeId).ConfigureAwait(false));
            }

            result.AddRange(await ApealExcelTitle(toDate, searchCourtId, templateId).ConfigureAwait(false));

            return result;
        }

        private async Task<List<ExcelReportData>> Millitary_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData,
            int searchCourtId, string paramValue)
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

            result.AddRange(await SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                courtTypeId, excelReportCaseTypeCols, 3, 8).ConfigureAwait(false));

            result.AddRange(await SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                courtTypeId, excelReportIndexCols.Where(x => x.SheetIndex == 4).ToList(), 4, 7).ConfigureAwait(false));

            //Sheet 2 Приложение НД
            result.AddRange(await MillitarySheet2(fromDate, toDate, searchCourtId, templateId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList()).ConfigureAwait(false));

            //Sheet 1 Приложение 1
            result.AddRange(await MillitarySheet1(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows.Where(x => x.SheetIndex == 0).ToList()).ConfigureAwait(false));

            result.AddRange(await MillitaryExcelTitle(toDate, searchCourtId, templateId).ConfigureAwait(false));

            return result;
        }

        private async Task<List<ExcelReportData>> MillitaryAP_Sheets(DateTime fromDate, DateTime toDate, int templateId, StatisticsNomDataVM nomData,
            int searchCourtId, string paramValue)
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

            result.AddRange(await SheetCaseCount(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                courtTypeId, excelReportCaseTypeCols, 3, 8).ConfigureAwait(false));

            result.AddRange(await SheetActIndex(fromDate, toDate, searchCourtId, templateId,
                new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                courtTypeId, excelReportIndexCols.Where(x => x.SheetIndex == 4).ToList(), 4, 7).ConfigureAwait(false));

            //Sheet 3 Приложение НД
            result.AddRange(await MillitaryAPSheet2(fromDate, toDate, searchCourtId, templateId,
                   excelReportComplainResults.Where(x => x.SheetIndex == 2 || x.SheetIndex == 102).ToList(),
                   excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList()).ConfigureAwait(false));

            //Sheet 1 Приложение 1
            result.AddRange(await ApealSheet1(fromDate, toDate, searchCourtId, templateId, excelReportCaseTypeRows.Where(x => x.SheetIndex == 0).ToList(), courtTypeId).ConfigureAwait(false));

            result.AddRange(await MillitaryApealExcelTitle(toDate, searchCourtId, templateId).ConfigureAwait(false));

            return result;
        }

        private async Task<StatisticsNomDataVM> GetStatisticsNomData()
        {
            StatisticsNomDataVM nomData = new StatisticsNomDataVM();

            nomData.excelReportIndexCols = await GetExcelReportIndex().ConfigureAwait(false);
            nomData.excelReportCaseTypeRows = await GetExcelReportCaseTypeRow().ConfigureAwait(false);
            nomData.excelReportCaseTypeCols = await GetExcelReportCaseTypeCol().ConfigureAwait(false);
            nomData.excelReportComplainResults = await GetExcelReportComplainResult().ConfigureAwait(false);
            nomData.excelReportCaseCodeRows = await GetExcelReportCaseCodeRow().ConfigureAwait(false);
            nomData.excelReportIspnReasons = await GetExcelReportIspnReason().ConfigureAwait(false);

            return nomData;
        }

        public async Task<List<ExcelReportData>> FillExcelData(DateTime fromDate, DateTime toDate, int courtId)
        {
            var paramValue = await repo.AllReadonly<SystemParam>().Where(x => x.ParamName == "system_features").Select(x => x.ParamValue).FirstOrDefaultAsync().ConfigureAwait(false);

            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = 0;
            if (courtId > 0)
            {
                courtTypeId = await repo.AllReadonly<Court>()
                               .Where(x => x.Id == courtId)
                               .Select(x => x.CourtTypeId)
                               .FirstOrDefaultAsync().ConfigureAwait(false);
            }

            StatisticsNomDataVM nomData = await GetStatisticsNomData().ConfigureAwait(false);

            var courtTypeCaseTypes = await repo.AllReadonly<CourtTypeCaseType>().ToListAsync().ConfigureAwait(false);

            var reportTemplates = await repo.AllReadonly<ExcelReportTemplate>()
                     .Where(x => x.DateFrom <= toDate && (x.DateTo ?? DateTime.MaxValue).Date >= toDate.Date && x.ReportTypeId == NomenclatureConstants.ExcelReportTemplateReportTypes.Normal)
                     .ToListAsync().ConfigureAwait(false);

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
            {
                result.AddRange(await RS_Sheets(fromDate, toDate,
                    reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                    .Select(x => x.Id).FirstOrDefault(), nomData, courtId, paramValue).ConfigureAwait(false));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
            {
                result.AddRange(await OS_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
                .Select(x => x.Id).FirstOrDefault(), nomData, courtId, paramValue).ConfigureAwait(false));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.Apeal)
            {
                result.AddRange(await AP_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal)
                .Select(x => x.Id).FirstOrDefault(), nomData, courtId, paramValue).ConfigureAwait(false));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.Millitary)
            {
                result.AddRange(await Millitary_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Millitary)
                .Select(x => x.Id).FirstOrDefault(), nomData, courtId, paramValue).ConfigureAwait(false));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.MillitaryApeal)
            {
                result.AddRange(await MillitaryAP_Sheets(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.MillitaryApeal)
                .Select(x => x.Id).FirstOrDefault(), nomData, courtId, paramValue).ConfigureAwait(false));
            }

            return result;
        }

        public async Task<bool> Statistics_SaveData(DateTime fromDate, DateTime toDate, int courtId)
        {
            try
            {
                //Ако е за определен съд да извика метода за delete и save
                if (courtId > 0)
                {
                    return await Statistics_DeleteSaveData(fromDate, toDate, courtId).ConfigureAwait(false);
                }

                //Ако има данните за месец/година нищо да не прави сървиза
                var hasData = await repo.AllReadonly<ExcelReportData>()
                                 .Where(x => x.ReportMonth == toDate.Month &&
                                 x.ReportYear == toDate.Year)
                                 .AnyAsync().ConfigureAwait(false);
                if (hasData == true)
                    return true;

                List<ExcelReportData> result = await FillExcelData(fromDate, toDate, courtId).ConfigureAwait(false);

                repo.AddRange(result);
                await repo.SaveChangesAsync().ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Statistics_DeleteSaveData(DateTime fromDate, DateTime toDate, int courtId)
        {
            try
            {
                List<ExcelReportData> result = await FillExcelData(fromDate, toDate, courtId).ConfigureAwait(false);

                //Изтриване на данните за месец/година
                repo.DeleteRange<ExcelReportData>(x => x.ReportMonth == toDate.Month && x.ReportYear == toDate.Year &&
                                  (courtId > 0 ? courtId : x.CourtId) == x.CourtId);

                repo.AddRange(result);
                await repo.SaveChangesAsync().ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Statistics_DeleteSaveDataMediation(DateTime fromDate, DateTime toDate, int courtId)
        {
            try
            {
                List<ExcelReportData> result = await FillExcelData_Mediation(fromDate, toDate, courtId).ConfigureAwait(false);

                //Изтриване на данните за месец/година
                repo.DeleteRange<ExcelReportData>(x => x.ReportMonth == toDate.Month && x.ReportYear == toDate.Year &&
                                  (courtId > 0 ? courtId : x.CourtId) == x.CourtId);

                repo.AddRange(result);
                await repo.SaveChangesAsync().ConfigureAwait(false);

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

        private ExcelReportData InsertExcelReportDataInterval(int court_id, int reportTemplateId, int reportYear, int reportMonth, int sheetIndex, int rowIndex, int colIndex, TimeSpan cellValue)
        {
            ExcelReportData exd = ExcelReportSetMainData(court_id, reportTemplateId, reportYear, reportMonth, sheetIndex, rowIndex, colIndex);
            exd.CellValueInterval = cellValue;
            exd.CellValueType = NomenclatureConstants.ExcelReportCellValueTypes.IntervalValue;
            return exd;
        }

        private Expression<Func<CaseLifecycle, bool>> UnfinishedLifecycle(DateTime fromDate)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => (x.Iteration == 1 ? x.Case.RegDate.Date : x.DateFrom.Date) < fromDate.Date && (x.DateTo ?? dateEnd).Date >= fromDate.Date;
            return reportTypeWhere;
        }

        private Expression<Func<CaseLifecycle, bool>> IncomingLifecycle(DateTime fromDate, DateTime toDate)
        {
            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => (x.Iteration == 1 ? x.Case.RegDate.Date : x.DateFrom.Date) >= fromDate.Date &&
                                                                         (x.Iteration == 1 ? x.Case.RegDate.Date : x.DateFrom.Date) <= toDate.Date;
            return reportTypeWhere;
        }

        private Expression<Func<CaseLifecycle, bool>> IncomingLifecycleWithoutCh80(DateTime fromDate, DateTime toDate)
        {
            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => (x.Iteration == 1 ? x.Case.RegDate.Date : x.DateFrom.Date) >= fromDate.Date &&
                                                                         (x.Iteration == 1 ? x.Case.RegDate.Date : x.DateFrom.Date) <= toDate.Date &&
                                                                         x.CaseMigration.CaseMigrationTypeId != NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS;
            return reportTypeWhere;
        }

        private Expression<Func<CaseLifecycle, bool>> IncomingLifecycleCh80(DateTime fromDate, DateTime toDate)
        {
            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date &&
                                                                         x.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS;
            return reportTypeWhere;
        }

        private Expression<Func<Case, bool>> GetDocumentTypeWhere(int[] documentTypeIds)
        {
            Expression<Func<Case, bool>> documentTypeWhere = x => true;
            if (documentTypeIds != null && documentTypeIds.Length > 0)
                documentTypeWhere = x => documentTypeIds.Contains(x.Document.DocumentTypeId);

            return documentTypeWhere;
        }

        private Expression<Func<CaseLifecycle, bool>> GetDocumentTypeCaseLifecycleWhere(int[] documentTypeIds)
        {
            Expression<Func<CaseLifecycle, bool>> documentTypeWhere = x => true;
            if (documentTypeIds != null && documentTypeIds.Length > 0)
                documentTypeWhere = x => documentTypeIds.Contains(x.Case.Document.DocumentTypeId);

            return documentTypeWhere;
        }

        private Expression<Func<CaseLifecycle, bool>> GeneralLifecycle()
        {
            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => x.DateExpired == null && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                         x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false;
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
            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                         x.Iteration > 1 && x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date;

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
                                       (a.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel ||
                                           (a.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendCase_ToOtherSystem &&
                                            a.OutDocument.DocumentTypeId == NomenclatureConstants.DocumentType.LetterOfTransmittalForAppeal)))
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
        private async Task<Expression<Func<CaseLifecycle, bool>>> FinishedLifecycleByType(DateTime fromDate, DateTime toDate, bool onlyStop)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

            var actComplainResults = await repo.AllReadonly<ActComplainResultGrouping>()
           .Where(x => x.ActComplainResultGroup == NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop)
           .Select(x => x.ActComplainResultId)
           .ToArrayAsync().ConfigureAwait(false);

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    (actComplainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) ||
                                     x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                    a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                     .Any()) == onlyStop;
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

        private string GetSismaIndexFromReportComplainResults(List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults,
                int complainResultId)
        {
            return excelReportComplainResults
                   .Where(a => a.ActComplainResult.Contains(complainResultId))
                   .Select(a => a.SismaIndex)
                   .FirstOrDefault();
        }

        private int GetRowFromCaseCodeRows(List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, int caseCodeId, int colIndex)
        {
            return excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(caseCodeId) && a.ExcludeCol.Contains(colIndex) == false)
                                                           .Select(a => a.RowIndex)
                                                           .FirstOrDefault();
        }

        private string GetCaseCodeLabelFromCaseCodeRows(List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, int caseCodeId)
        {
            return excelReportCaseCodeRows.Where(a => a.CaseCode.Contains(caseCodeId))
                                                           .Select(a => a.CaseCodeLabel)
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
        private async Task<List<CaseStatisticsVM>> CaseLawUnitCaseType_Select(int courtTypeId, int courtId, int[] caseGroupIds,
            DateTime fromDate, DateTime toDate, int reportType, List<StatisticsExcelReportCaseTypeColVM> courtTypeCaseTypesCols)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime toDateEnd = toDate.ForceEndDate();

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
                reportTypeWhere = IncomingLifecycle(fromDate, toDate);
            else if (reportType == NomenclatureConstants.StatisticReportTypes.Finished3months)
                reportTypeWhere = FinishedLifecycleMonths(fromDate, toDate, 0, 3);
            else if (reportType == NomenclatureConstants.StatisticReportTypes.FinishedStop || reportType == NomenclatureConstants.StatisticReportTypes.FinishedNoStop)
            {
                if (reportType == NomenclatureConstants.StatisticReportTypes.FinishedStop)
                {
                    reportTypeWhere = await FinishedLifecycleByType(fromDate, toDate, true).ConfigureAwait(false);
                }
                else if (reportType == NomenclatureConstants.StatisticReportTypes.FinishedNoStop)
                {
                    reportTypeWhere = await FinishedLifecycleByType(fromDate, toDate, false).ConfigureAwait(false);
                }
            }

            var result = await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycle())
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    CaseTypeId = x.Case.CaseTypeId,
                                    DocumentTypeId = x.Case.Document.DocumentTypeId,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    ProcessPriorityId = x.Case.ProcessPriorityId ?? 0,
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
                                              .Where(a => a.DateFrom <= toDateEnd)
                                              .OrderByDescending(a => a.DateFrom)
                                              .Select(a => a.LawUnitId + ",," + a.LawUnit.FullName)
                                              .FirstOrDefault()
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelCol = courtTypeCaseTypesCols.Where(a => a.CaseType.Contains(x.CaseTypeId))
                .Where(a => a.ReportTypeId == reportType)
                .Where(a => (a.ProcessPriority.Count == 0 || a.ProcessPriority.Contains(x.ProcessPriorityId)))
                .Where(a => ((a.DocumentType.Count == 0 || a.DocumentType.Contains(x.DocumentTypeId)) &&
                            (a.CaseCode.Count == 0 || a.CaseCode.Contains(x.CaseCodeId))) == a.IsTrue)
                .Select(a => a.ColIndex)
                .FirstOrDefault());

            result = result.GroupBy(x => new
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

        private async Task<List<ExcelReportData>> SheetCaseCount(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            int[] caseGroupIds, int courtTypeId, List<StatisticsExcelReportCaseTypeColVM> courtTypeCaseTypesCols,
            int sheetIndex, int startRowIndex)
        {
            var allData = await CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.Unfinished, courtTypeCaseTypesCols).ConfigureAwait(false);

            allData.AddRange(await CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.Incoming, courtTypeCaseTypesCols).ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.Finished3months, courtTypeCaseTypesCols).ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.FinishedStop, courtTypeCaseTypesCols).ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.FinishedNoStop, courtTypeCaseTypesCols).ConfigureAwait(false));

            return SaveExcelByJudge(toDate, startRowIndex, allData, templateId, sheetIndex);
        }

        private async Task<List<CaseStatisticsVM>> Index_Select(int courtTypeId, int courtId, int[] caseGroupIds,
            DateTime fromDate, DateTime toDate, List<StatisticsExcelReportIndexVM> excelReportIndexCols)
        {
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

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


            var result = await repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(x => x.IsFinalDoc && x.ActDeclaredDate != null)
                                .Where(x => x.CaseSessionActComplains
                                             .Where(a => a.ComplainResults.
                                                         Where(b => b.DateResult >= fromDateStart &&
                                                              b.DateResult <= toDateEnd).Any())
                                             .Any())
                                .Where(x => x.ActComplainIndexId != null)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    CaseTypeId = x.Case.CaseTypeId,
                                    ActComplainIndexId = x.ActComplainIndexId ?? 0,
                                    ActTypeId = x.ActTypeId,
                                    LawUnitData = x.CaseSession.CaseLawUnits
                                              .Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                              .Where(a => (a.DateTo ?? dateEnd) >= x.CaseSession.DateFrom)
                                              .OrderByDescending(a => a.DateFrom)
                                              .Select(a => a.LawUnitId + ",," + a.LawUnit.FullName)
                                              .FirstOrDefault()
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelCol = excelReportIndexCols.Where(a => a.CaseTypes.Contains(x.CaseTypeId) &&
                                               a.ActTypes.Contains(x.ActTypeId) &&
                                               a.ActComplainIndex.Contains(x.ActComplainIndexId))
                                               .Select(a => a.Col)
                                               .FirstOrDefault());

            result = result.GroupBy(x => new
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

        private async Task<List<ExcelReportData>> SheetActIndex(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            int[] caseGroupIds, int courtTypeId, List<StatisticsExcelReportIndexVM> excelReportIndexCols,
            int sheetIndex, int startRowIndex)
        {
            var allData = await Index_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate, excelReportIndexCols).ConfigureAwait(false);

            return SaveExcelByJudge(toDate, startRowIndex, allData, templateId, sheetIndex);
        }

        private Expression<Func<Case, bool>> GetCaseCaseCodeWhere(int reportType, DateTime fromDate, DateTime toDate)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.CaseLifecycles
                        .Where(a => a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                        .Where(a => (a.Iteration == 1 ? a.Case.RegDate.Date : a.DateFrom.Date) < fromDate.Date && (a.DateTo ?? dateEnd).Date >= fromDate.Date)
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
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date && (x.IsRenewCase ?? false) == true;
            }

            return reportTypeWhere;
        }

        private Expression<Func<Case, bool>> GeneralCaseWhere()
        {
            Expression<Func<Case, bool>> reportTypeWhere = x => x.CaseStateId != NomenclatureConstants.CaseState.Draft &&
                                                                x.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false;

            return reportTypeWhere;
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
        private async Task<List<CaseStatisticsVM>> CaseCaseCode_Select(int courtTypeId, int courtId, int[] caseGroupIds,
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

            Expression<Func<Case, bool>> reportTypeWhere = GetCaseCaseCodeWhere(reportType, fromDate, toDate);

            int[] caseCodes = excelReportCaseCodeRows.SelectMany(x => x.CaseCode).ToArray();
            var result = await repo.AllReadonly<Case>()
                                .Where(GeneralCaseWhere())
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(x => caseCodes.Contains(x.CaseCodeId ?? 0))
                                .Where(reportTypeWhere)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    CaseCodeId = x.CaseCodeId ?? 0,
                                    ExcelCol = colIndex,
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelRow = GetRowFromCaseCodeRows(excelReportCaseCodeRows,
                                                 x.CaseCodeId, colIndex));

            result = result.GroupBy(x => new
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
        /// <param name="sismaIndex"></param>
        /// <param name="reportType">1 - Несвършели към началото на периода, 2 - новообразувани без получените по подсъдност, 3 - получени по подсъдност,
        /// 4 - образувани под нов номер, 5 - обжалвани, 6 - Общо постъпили, 7 - новообразувани, 8 - постъпили бързи произвоства,
        /// 9 - Възобновени дела</param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        private async Task<List<SismaCaseStatisticsVM>> SismaCaseCaseCode_Select(int courtTypeId, int courtId, int[] caseGroupIds,
            DateTime fromDate, DateTime toDate, string sismaIndex, int reportType, int instanceId, int[] caseTypeIds, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
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

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (caseTypeIds != null && caseTypeIds.Length > 0)
                caseTypeWhere = x => caseTypeIds.Contains(x.CaseTypeId);

            Expression<Func<Case, bool>> reportTypeWhere = GetCaseCaseCodeWhere(reportType, fromDate, toDate);

            int[] caseCodes = excelReportCaseCodeRows.SelectMany(x => x.CaseCode).ToArray();

            var result = await repo.AllReadonly<Case>()
                                .Where(GeneralCaseWhere())
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(x => caseCodes.Contains(x.CaseCodeId ?? 0))
                                .Where(reportTypeWhere)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(caseTypeWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    CaseCodeId = x.CaseCodeId ?? 0,
                                    SismaIndex = sismaIndex,
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.CodeData = GetCaseCodeLabelFromCaseCodeRows(excelReportCaseCodeRows, x.CaseCodeId));

            result = result.GroupBy(x => new
            {
                x.CourtCode,
                x.CodeData,
                x.SismaIndex,
            })
            .Select(x => new SismaCaseStatisticsVM
            {
                CourtCode = x.Key.CourtCode,
                CodeData = x.Key.CodeData,
                SismaIndex = x.Key.SismaIndex,
                Count = x.Count(),
            })
            .ToList();

            return result;
        }


        private async Task<Expression<Func<CaseLifecycle, bool>>> GetCaseLifecycleCaseCodeWhere(int reportType, DateTime fromDate, DateTime toDate)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

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
                var sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsStopCaseAgreement).ConfigureAwait(false);
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null && sessionResults.Contains(a.SessionResultId))
                                      .Any();
            }
            else if (reportType == 5)
            {
                var sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsStopCaseAgreement).ConfigureAwait(false);

                var complainResults = await repo.AllReadonly<ActComplainResultGrouping>()
                                     .Where(x => x.ActComplainResultGroup == NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStopGD)
                                     .Select(x => x.ActComplainResultId)
                                     .ToListAsync().ConfigureAwait(false);
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    (complainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) ||
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null &&
                              a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                      .Any()) &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null && sessionResults.Contains(a.SessionResultId))
                                      .Any() == false
                                      ;
            }
            else if (reportType == 6)
            {
                var actTypes = new int[] {NomenclatureConstants.ActType.Answer, NomenclatureConstants.ActType.Definition,
                             NomenclatureConstants.ActType.Sentence, NomenclatureConstants.ActType.Protokol,
                NomenclatureConstants.ActType.ProtokolOpredelenie, NomenclatureConstants.ActType.Injunction};
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && actTypes.Contains(x.CaseSessionAct.ActTypeId) &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null &&
                                  a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                      .Any() == false;
            }
            else if (reportType == 7)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null &&
                                  a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                      .Any();
            }
            else if (reportType == 8)
            {
                var sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseStop381ND).ConfigureAwait(false);

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
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null &&
                                      a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                      .Any() == false &&
                                    x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick;
            }
            else if (reportType == 10)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null &&
                                      a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                      .Any() &&
                                    x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick;
            }
            else if (reportType == 11)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.Case.CaseTypeId == NomenclatureConstants.CaseTypes.VChND;
            }

            return reportTypeWhere;
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
        /// 4 - прекратено по спогодба, 5 - прекратено по други причини Всички прекратени без по спогодба, 
        /// 6 - решени по същество с присъда,
        /// 7 - прекр. и спор. – Общо, 8 - в т.ч. свърш.споразум.- чл.381-384, 9 - От св.дела б.п.по чл. 356 НПК – решени,
        /// 10 - От св.дела б.п.по чл. 356 НПК – прекратени и споразумения, 11 - Свършени ВЧНД</param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CaseLifecycleCaseCode_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, int colIndex, int reportType,
    int instanceId)
        {
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = await GetCaseLifecycleCaseCodeWhere(reportType, fromDate, toDate).ConfigureAwait(false);

            int[] caseCodes = excelReportCaseCodeRows.SelectMany(x => x.CaseCode).ToArray();

            var result = await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycle())
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => caseCodes.Contains(x.Case.CaseCodeId ?? 0))
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    ExcelCol = colIndex,
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelRow = GetRowFromCaseCodeRows(excelReportCaseCodeRows, x.CaseCodeId, colIndex));

            result = result.GroupBy(x => new
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
        /// <param name="sismaIndex"></param>
        /// <param name="reportType">1 - продължаващи под същия номер, 2 - свършени до 3 месеца вкл., 3 - свършени от 3 до 6 месеца,
        /// 4 - прекратено по спогодба, 5 - прекратено по други причини Всички прекратени без по спогодба, 
        /// 6 - решени по същество с присъда,
        /// 7 - прекр. и спор. – Общо, 8 - в т.ч. свърш.споразум.- чл.381-384, 9 - От св.дела б.п.по чл. 356 НПК – решени,
        /// 10 - От св.дела б.п.по чл. 356 НПК – прекратени и споразумения, 11 - Свършени ВЧНД</param>
        /// <returns></returns>
        private async Task<List<SismaCaseStatisticsVM>> SismaCaseLifecycleCaseCode_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, string sismaIndex, int reportType,
    int instanceId, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = await GetCaseLifecycleCaseCodeWhere(reportType, fromDate, toDate).ConfigureAwait(false);

            int[] caseCodes = excelReportCaseCodeRows.SelectMany(x => x.CaseCode).ToArray();

            var result = await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycle())
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => caseCodes.Contains(x.Case.CaseCodeId ?? 0))
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    SismaIndex = sismaIndex,
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.CodeData = GetCaseCodeLabelFromCaseCodeRows(excelReportCaseCodeRows, x.CaseCodeId));

            result = result.GroupBy(x => new
            {
                x.CourtCode,
                x.CodeData,
                x.SismaIndex,
            })
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Key.CourtCode,
                                    CodeData = x.Key.CodeData,
                                    SismaIndex = x.Key.SismaIndex,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        private async Task<List<CaseStatisticsVM>> CaseLifecycleCaseCodeComplainResult_Select(int courtTypeId, int courtId, int[] caseGroupIds,
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

            int[] caseCodes = excelReportCaseCodeRows.SelectMany(x => x.CaseCode).ToArray();

            var result = await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycle())
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date)
                                .Where(x => caseCodes.Contains(x.Case.CaseCodeId ?? 0))
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    ActComplainResultId = x.CaseSessionAct.ActComplainResultId ?? 0,
                                })
                                .ToListAsync().ConfigureAwait(false);

            foreach (var item in result)
            {
                item.ExcelRow = GetRowFromCaseCodeRows(excelReportCaseCodeRows, item.CaseCodeId, -1);
                item.ExcelCol = GetColFromReportComplainResults(excelReportComplainResults, item.ActComplainResultId);
            }

            result = result.GroupBy(x => new
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

        private async Task<List<SismaCaseStatisticsVM>> SismaCaseLifecycleCaseCodeComplainResult_Select(int courtTypeId, int courtId, int[] caseGroupIds,
            DateTime fromDate, DateTime toDate, List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults, int instanceId, int[] caseTypeIds,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseLifecycle, bool>> caseTypeWhere = x => true;
            if (caseTypeIds != null && caseTypeIds.Length > 0)
                caseTypeWhere = x => caseTypeIds.Contains(x.Case.CaseTypeId);

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            int[] caseCodes = excelReportCaseCodeRows.SelectMany(x => x.CaseCode).ToArray();
            int[] actComplainResults = excelReportComplainResults.SelectMany(a => a.ActComplainResult).ToArray();

            var result = await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycle())
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date)
                                .Where(x => actComplainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0))
                                .Where(x => caseCodes.Contains(x.Case.CaseCodeId ?? 0))
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(caseTypeWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    ActComplainResultId = x.CaseSessionAct.ActComplainResultId ?? 0,
                                })
                                .ToListAsync().ConfigureAwait(false);

            foreach (var item in result)
            {
                item.CodeData = GetCaseCodeLabelFromCaseCodeRows(excelReportCaseCodeRows, item.CaseCodeId);
                item.SismaIndex = GetSismaIndexFromReportComplainResults(excelReportComplainResults, item.ActComplainResultId);
            }

            result = result.GroupBy(x => new
            {
                x.CourtCode,
                x.CodeData,
                x.SismaIndex,
            })
            .Select(x => new SismaCaseStatisticsVM
            {
                CourtCode = x.Key.CourtCode,
                CodeData = x.Key.CodeData,
                SismaIndex = x.Key.SismaIndex,
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
        private async Task<List<CaseStatisticsVM>> CaseSession_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int[] caseTypeIds, int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime toDateEnd = toDate.ForceEndDate();

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
                                              NomenclatureConstants.CaseSessionResult.ScheduledFirstSessionList.Contains(a.SessionResultId)).Any() &&
                                       x.SessionTypeId == NomenclatureConstants.SessionType.ClosedSession &&
                                       x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.GeneralOrder;
            }
            else if (reportType >= 8 && reportType <= 11)
            {
                reportTypeSpravka3Where = x => x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              NomenclatureConstants.CaseSessionResult.ScheduledFirstSessionList.Contains(a.SessionResultId)).Any() &&
                                       x.SessionTypeId == NomenclatureConstants.SessionType.ClosedSession &&
                                       x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick;
            }
            else if (reportType >= 15 && reportType <= 18)
            {
                reportTypeSpravka3Where = x => x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              NomenclatureConstants.CaseSessionResult.ScheduledFirstSessionList.Contains(a.SessionResultId)).Any() &&
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
                    sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseDelayGD).ConfigureAwait(false);
                else
                    sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseDelayND).ConfigureAwait(false);

                reportTypeWhere = x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                       x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession &&
                                       x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                              sessionResults.Contains(a.SessionResultId)).Any();
            }
            else if (reportType == 3)
            {
                var sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsCaseDelayFirstSessionGD).ConfigureAwait(false);
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
                var sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsInvestigateND).ConfigureAwait(false);
                var sessionResultBases = await SessionResultBaseGrouping_Select(NomenclatureConstants.SessionResultBaseGroupings.StatisticsInvestigateND).ConfigureAwait(false);
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
                                                            a.ActInforcedDate <= toDateEnd).Any() &&
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

            var result = (await repo.AllReadonly<CaseSession>()
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
                                .ToListAsync().ConfigureAwait(false))
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
        private async Task<List<CaseStatisticsVM>> CaseLifecycleComplain_Select(int courtTypeId, int courtId, int[] caseGroupIds,
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
                                       (x.Iteration == 1 ? x.Case.RegDate.AddYears(1).Date : x.DateFrom.AddYears(1).Date) <= toDate.Date &&
                                       (x.Iteration == 1 ? x.Case.RegDate.AddYears(3).Date : x.DateFrom.AddYears(3).Date) >= toDate.Date;
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                       (x.Iteration == 1 ? x.Case.RegDate.AddYears(3).Date : x.DateFrom.AddYears(3).Date) < toDate.Date &&
                                       (x.Iteration == 1 ? x.Case.RegDate.AddYears(5).Date : x.DateFrom.AddYears(5).Date) >= toDate.Date;
            }
            else if (reportType == 3)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                       (x.Iteration == 1 ? x.Case.RegDate.AddYears(5).Date : x.DateFrom.AddYears(5).Date) < toDate.Date;
            }
            else if (reportType == 4)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date && x.Iteration == 1 &&
                                       x.Case.RegDate.AddMonths(3).Date >= toDate.Date && x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.AND;
            }
            else if (reportType == 5)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date && x.Iteration == 1 &&
                                       x.Case.RegDate.AddMonths(3).Date < toDate.Date && x.Case.RegDate.AddMonths(6).Date >= toDate.Date && x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.AND;
            }
            else if (reportType == 6)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date && x.Iteration == 1 &&
                                       x.Case.RegDate.AddMonths(6).Date < toDate.Date && x.Case.RegDate.AddMonths(12).Date >= toDate.Date && x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.AND;
            }
            else if (reportType == 7)
            {
                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date && x.Iteration == 1 &&
                                       x.Case.RegDate.AddMonths(12).Date < toDate.Date && x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.AND;
            }
            else if (reportType == 8 || reportType == 12)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && x.CaseSessionAct.ActDeclaredDate != null &&
                                    x.CaseSessionAct.ActTypeId == NomenclatureConstants.ActType.Sentence &&
                                    (
              (x.CaseSessionAct.ActMotivesDeclaredDate ?? dateEnd).Date > ((DateTime)x.CaseSessionAct.ActDeclaredDate).AddDays(60).Date ||
              (x.CaseSessionAct.ActMotivesDeclaredDate == null && dateEnd > ((DateTime)x.CaseSessionAct.ActDeclaredDate).AddDays(60))
               ) && x.Case.CaseTypeId != NomenclatureConstants.CaseTypes.AND;
            }
            else if (reportType == 9)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                      x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick;
            }
            else if (reportType == 10)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                            x.Case.CaseTypeId == NomenclatureConstants.CaseTypes.ChND &&
                            x.Case.Document.DocumentTypeId == NomenclatureConstants.DocumentType.Request368;
            }
            else if (reportType == 11)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Summary;
            }
            else if (reportType == 13)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.Case.CaseTypeId == NomenclatureConstants.CaseTypes.VChND &&
                                    x.Case.CaseCode.Code == "8030"; //за трепане съм
            }
            else if (reportType == 14)
            {
                var documentResume = await DocumentTypeGrouping_Select(NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND).ConfigureAwait(false);

                reportTypeWhere = x => (x.DateTo ?? dateEnd).Date > toDate.Date &&
                                 documentResume.Contains(x.Case.Document.DocumentTypeId);
            }
            else if (reportType == 15)
            {
                var documentResume = await DocumentTypeGrouping_Select(NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND).ConfigureAwait(false);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                 documentResume.Contains(x.Case.Document.DocumentTypeId);
            }
            else if (reportType == 16)
            {
                var documentResume = await DocumentTypeGrouping_Select(NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND).ConfigureAwait(false);
                var complains = await ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop).ConfigureAwait(false);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                 documentResume.Contains(x.Case.Document.DocumentTypeId) &&
                                 complains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) == false;
            }
            else if (reportType == 17)
            {
                var documentResume = await DocumentTypeGrouping_Select(NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND).ConfigureAwait(false);
                var complains = await ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseSuccessful).ConfigureAwait(false);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                 documentResume.Contains(x.Case.Document.DocumentTypeId) &&
                                 complains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0);
            }

            var result = await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycle())
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => (x.Iteration == 1 ? x.Case.RegDate.Date : x.DateFrom.Date) <= toDate.Date) // Изключва всички започнали интервали след датата на справката
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ActComplainResultId = x.CaseSessionAct.ActComplainResultId ?? 0,
                                    ExcelRow = rowIndex,

                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelCol = colIndex > 0 ? colIndex :
                                               GetColFromReportComplainResults(excelReportComplainResults, x.ActComplainResultId));

            result = result.GroupBy(x => new
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
        private async Task<List<CaseStatisticsVM>> CaseLifecycle_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId)
        {
            return await CaseLifecycleComplain_Select(courtTypeId, courtId, caseGroupIds,
                    fromDate, toDate, colIndex, rowIndex, reportType, instanceId, null).ConfigureAwait(false);
        }

        private Expression<Func<CaseSessionAct, bool>> GeneralCaseSessionActWhere()
        {
            Expression<Func<CaseSessionAct, bool>> reportTypeWhere = x => x.DateExpired == null &&
                                                                     x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false;

            return reportTypeWhere;
        }

        private async Task<Expression<Func<CaseSessionAct, bool>>> GetCaseSessionActWhere(int reportType, DateTime fromDate, DateTime toDate)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

            Expression<Func<CaseSessionAct, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd &&
                                    x.CaseSession.DateFrom.AddMonths(3).Date < (x.ActDeclaredDate ?? dateEnd).Date &&
                                    x.CaseSession.DateExpired == null &&
                                    x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession &&
                                    x.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                 a.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution).Any();
            }
            else if (reportType == 2)
            {
                var complainResults = await repo.AllReadonly<ActComplainResultGrouping>()
                                     .Where(x => x.ActComplainResultGroup == NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStopGD)
                                     .Select(x => x.ActComplainResultId)
                                     .ToListAsync().ConfigureAwait(false);
                int[] typeAct = new int[] {NomenclatureConstants.ActType.Answer,
                                        NomenclatureConstants.ActType.Definition,
                                        NomenclatureConstants.ActType.Injunction};

                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart &&
                                    x.ActDeclaredDate <= toDateEnd &&
                                    typeAct.Contains(x.ActTypeId) &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession &&
                                    (complainResults.Contains(x.ActComplainResultId ?? 0) ||
                                    x.CaseSession.CaseSessionResults
                                      .Where(a => a.DateExpired == null &&
                                      a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                      .Any()) == false;
            }
            else if (reportType == 3)
            {
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart &&
                                    x.ActDeclaredDate <= toDateEnd &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Sentence &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession;
            }
            else if (reportType == 4)
            {
                var complainResults = await ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop).ConfigureAwait(false);

                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart &&
                                    x.ActDeclaredDate <= toDateEnd &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Answer &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession &&
                                    x.Case.CaseTypeId == NomenclatureConstants.CaseTypes.VAND &&
                                    complainResults.Contains(x.ActComplainResultId ?? 0) == false;
            }
            else if (reportType == 5)
            {
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart &&
                                    x.ActDeclaredDate <= toDateEnd &&
                                    x.IsFinalDoc == true &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Answer &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession;
            }
            else if (reportType == 6)
            {
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart &&
                                    x.ActDeclaredDate <= toDateEnd &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Sentence;
            }
            else if (reportType == 7)
            {
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart &&
                                    x.ActDeclaredDate <= toDateEnd &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Answer &&
                                    x.Case.CaseTypeId == NomenclatureConstants.CaseTypes.AND;
            }
            else if (reportType == 8)
            {
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart &&
                                    x.ActDeclaredDate <= toDateEnd &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Answer &&
                                    x.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession;
            }
            else if (reportType == 9)
            {
                int[] sessionTypes = new int[] { NomenclatureConstants.SessionType.FirstSession, NomenclatureConstants.SessionType.SecondSession, NomenclatureConstants.SessionType.ClosedSession,
                                                 NomenclatureConstants.SessionType.OpenSession, NomenclatureConstants.SessionType.OpenSessionOpenDoors };
                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart &&
                                    x.ActDeclaredDate <= toDateEnd &&
                                    x.ActTypeId == NomenclatureConstants.ActType.Answer && sessionTypes.Contains(x.CaseSession.SessionTypeId);
            }

            return reportTypeWhere;
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
        /// 7 - Постановени Решения за АНД, 8 - Постановени решения по чл. 235, ал. 5 от ГПК, след проведено открито съдебно заседание,
        /// 9 - Справка за постановени решения за промени по фирмени дела</param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CaseSessionAct_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId)
        {
            Expression<Func<CaseSessionAct, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseSessionAct, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseSessionAct, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseSessionAct, bool>> reportTypeWhere = await GetCaseSessionActWhere(reportType, fromDate, toDate).ConfigureAwait(false);

            var result = (await repo.AllReadonly<CaseSessionAct>()
                                .Where(GeneralCaseSessionActWhere())
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
                                .ToListAsync().ConfigureAwait(false))
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
        /// <param name="sismaIndex"></param>
        /// <param name="instanceId"></param>
        /// <param name="reportType">RSSheet2 - 1 -От решените дела /кол.9+10+11/“ с необявени решения с изтекъл срок над 3м.,
        /// 2 - Постановени решения по чл. 235, ал. 5 от ГПК, след проведено открито съдебно заседание,
        /// 3 - Постановени присъди, 4 - Постановени решения по НАХД, 
        /// 5 - Финализиращи решения постановени в открити заседания, 6 - Постановени присъди,
        /// 7 - Постановени Решения за АНД, 8 - Постановени решения по чл. 235, ал. 5 от ГПК, след проведено открито съдебно заседание</param>
        /// <returns></returns>
        private async Task<List<SismaCaseStatisticsVM>> SismaCaseSessionAct_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, string sismaIndex, int reportType, int instanceId)
        {
            Expression<Func<CaseSessionAct, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseSessionAct, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseSessionAct, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseSessionAct, bool>> reportTypeWhere = await GetCaseSessionActWhere(reportType, fromDate, toDate).ConfigureAwait(false);

            var result = (await repo.AllReadonly<CaseSessionAct>()
                                .Where(GeneralCaseSessionActWhere())
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    SismaIndex = sismaIndex,
                                })
                                .ToListAsync().ConfigureAwait(false))
                                .GroupBy(x => new
                                {
                                    x.CourtCode,
                                    x.SismaIndex,
                                })
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Key.CourtCode,
                                    SismaIndex = x.Key.SismaIndex,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }
        private Expression<Func<CasePerson, bool>> GeneralCasePersonWhere()
        {
            Expression<Func<CasePerson, bool>> reportTypeWhere = x => x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft &&
                                                                      x.DateExpired == null && x.CaseSessionId == null &&
                                                                      x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false &&
                                                                      x.Case.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo;

            return reportTypeWhere;
        }

        private Expression<Func<CasePerson, bool>> GetCasePersonCaseCodeWhere(int reportType, DateTime fromDate, DateTime toDate)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

            Expression<Func<CasePerson, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.Case.CaseLifecycles.Where(a => a.DateExpired == null &&
                                   a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                   a.DateTo != null && a.DateTo >= fromDateStart &&
                                    a.DateTo <= toDateEnd).Any() &&
                                   x.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide;
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.CasePersonSentences
                                       .Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
                                        a.SentenceResultTypeId == NomenclatureConstants.SentenceResultTypes.Justified &&
                                              a.DateExpired == null && (a.IsActive ?? false))
                                   .Any();
            }
            else if (reportType == 3)
            {
                reportTypeWhere = x => x.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge &&
                                        x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
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

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
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

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
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

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null && (a.IsActive ?? false) &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null && b.IsMainPunishment &&
                                                                   sentenceTypes.Contains(b.SentenceTypeId ?? 0))
                                                                      .Any())
                                         .Any();
            }
            else if (reportType == 10)
            {
                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
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

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null && (a.IsActive ?? false) &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null && b.IsMainPunishment &&
                                                                   sentenceTypes.Contains(b.SentenceTypeId ?? 0) == false)
                                                                      .Any())
                                         .Any();
            }
            else if (reportType == 16)
            {
                reportTypeWhere = x => x.CasePersonSentences
                                       .Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
                                        a.SentenceResultTypeId == NomenclatureConstants.SentenceResultTypes.Justified78 &&
                                              a.DateExpired == null && (a.IsActive ?? false))
                                   .Any();
            }
            else if (reportType == 17)
            {
                reportTypeWhere = x => x.CasePersonSentences
                                       .Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
                                        a.SentenceResultTypeId == NomenclatureConstants.SentenceResultTypes.PropertySanction83 &&
                                              a.DateExpired == null && (a.IsActive ?? false))
                                   .Any();
            }

            return reportTypeWhere;
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
        /// 13 - Доживотен затвор, 14 - Доживотен затвор без право на замяна, 15 - Осъдени лица други наказания ОС и Военен,
        /// 16 - освободени от наказателна отговорност (чл. 78А НК), 17 - с наложена имуществена санкция по чл. 83а от ЗАНН</param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CasePersonCaseCode_Select(int courtTypeId, int courtId,
    DateTime fromDate, DateTime toDate, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, int colIndex, int reportType,
    int instanceId)
        {
            Expression<Func<CasePerson, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CasePerson, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CasePerson, bool>> reportTypeWhere = GetCasePersonCaseCodeWhere(reportType, fromDate, toDate);

            int[] caseCodes = excelReportCaseCodeRows.SelectMany(x => x.CaseCode).ToArray();

            var result = await repo.AllReadonly<CasePerson>()
                                .Where(GeneralCasePersonWhere())
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => caseCodes.Contains(x.Case.CaseCodeId ?? 0))
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    ExcelCol = colIndex,
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelRow = GetRowFromCaseCodeRows(excelReportCaseCodeRows, x.CaseCodeId, colIndex));

            result = result.GroupBy(x => new
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
        /// <param name="sismaIndex"></param>
        /// <param name="reportType">1 - Съдени лица общо, 2 - Оправдани от Съдени лица, 3 - Осъдени лица непълнолетни,
        /// 4 - Лишаване от свобода – до 3г. – Общо, 5 - Лишаване от свобода – до 3г. – условно,
        /// 6 - Лишаване от свобода над 3 до 15 г, 7 - Осъден глоба, 8 - Осъден Пробация, 9 - Осъдени лица – Други наказания,
        /// 10 - брой наказани лица по споразум. - чл.381-384 НПК,
        /// 11 - Лишаване от свобода над 3 до 10 г, 12 - Лишаване от свобода над 10 до 30 г,
        /// 13 - Доживотен затвор, 14 - Доживотен затвор без право на замяна, 15 - Осъдени лица други наказания ОС и Военен,
        /// 16 - освободени от наказателна отговорност (чл. 78А НК), 17 - с наложена имуществена санкция по чл. 83а от ЗАНН</param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        private async Task<List<SismaCaseStatisticsVM>> SismaCasePersonCaseCode_Select(int courtTypeId, int courtId,
    DateTime fromDate, DateTime toDate, string sismaIndex, int reportType,
    int instanceId, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            Expression<Func<CasePerson, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CasePerson, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CasePerson, bool>> reportTypeWhere = GetCasePersonCaseCodeWhere(reportType, fromDate, toDate);

            int[] caseCodes = excelReportCaseCodeRows.SelectMany(x => x.CaseCode).ToArray();

            var result = await repo.AllReadonly<CasePerson>()
                                .Where(GeneralCasePersonWhere())
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => caseCodes.Contains(x.Case.CaseCodeId ?? 0))
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    SismaIndex = sismaIndex,
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.CodeData = GetCaseCodeLabelFromCaseCodeRows(excelReportCaseCodeRows, x.CaseCodeId));
            result = result.GroupBy(x => new
            {
                x.CourtCode,
                x.CodeData,
                x.SismaIndex,
            })
            .Select(x => new SismaCaseStatisticsVM
            {
                CourtCode = x.Key.CourtCode,
                CodeData = x.Key.CodeData,
                SismaIndex = x.Key.SismaIndex,
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
        private async Task<List<CaseStatisticsVM>> CasePerson_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

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
                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
                                        a.DateExpired == null && (a.IsActive ?? false) &&
                                        a.CasePersonSentenceLawbases.Any())
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

                reportTypeWhere = x => x.CasePersonSentences.Where(a => a.CaseSessionAct.ActDeclaredDate >= fromDateStart &&
                                        a.CaseSessionAct.ActDeclaredDate <= toDateEnd &&
                                        a.SentenceResultTypeId != NomenclatureConstants.SentenceResultTypes.Justified &&
                                        a.DateExpired == null && (a.IsActive ?? false) &&
                                        a.CasePersonSentencePunishments.Where(b => b.DateExpired == null && b.IsMainPunishment &&
                                                                   sentenceTypes.Contains(b.SentenceTypeId ?? 0))
                                                                      .Any())
                                         .Any();
            }

            var result = (await repo.AllReadonly<CasePerson>()
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
                                .ToListAsync().ConfigureAwait(false))
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

        private async Task<Expression<Func<Case, bool>>> GetCaseByFromCourtWhere(int reportType, DateTime fromDate, DateTime toDate)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.CaseLifecycles.Where(a => a.LifecycleTypeId == NomenclatureConstants.LifecycleType.Stop &&
                                            a.DateExpired == null &&
                                            a.DateFrom.Date <= toDate.Date &&
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
                var documentResume = await DocumentTypeGrouping_Select(NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND).ConfigureAwait(false);

                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                                                (x.IsNewCaseNewNumber ?? false) == false &&
                                 documentResume.Contains(x.Document.DocumentTypeId);
            }

            return reportTypeWhere;
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
        private async Task<List<CaseStatisticsVM>> CaseByFromCourt_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId,
    int[] documentTypeIds, bool groupByFromCourt)
        {
            Expression<Func<Case, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.CaseGroupId);

            Expression<Func<Case, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<Case, bool>> courtGroupWhere = x => true;
            if (groupByFromCourt == true)
            {
                courtGroupWhere = x => (x.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId)).Any() ||
                                     x.Document.DocumentInstitutionCaseInfo
                                      .Where(a => NomenclatureConstants.InstitutionTypes.StatisticsFromInstitution.Contains(a.Institution.InstitutionTypeId)).Any());
            }

            Expression<Func<Case, bool>> reportTypeWhere = await GetCaseByFromCourtWhere(reportType, fromDate, toDate).ConfigureAwait(false);

            var result = (await repo.AllReadonly<Case>()
                                .Where(GeneralCaseWhere())
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Where(GetDocumentTypeWhere(documentTypeIds))
                                .Where(courtGroupWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    FromCourtData = groupByFromCourt == true ?
                                            (x.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId)).Any() ?
                                            x.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId))
                                            .Select(a => a.CourtId + ",," + a.Court.Label + ",," +
                                              (a.Court.ParentCourtId == x.CourtId ? 0 : 1))
                                            .FirstOrDefault() :
                                            x.Document.DocumentInstitutionCaseInfo
                                            .Where(a => NomenclatureConstants.InstitutionTypes.StatisticsFromInstitution.Contains(a.Institution.InstitutionTypeId))
                                            .Select(a => "I" + a.Institution.InstitutionTypeId + ",," + a.Institution.InstitutionType.Label + ",,999")
                                            .FirstOrDefault()) : "",
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                })
                                .ToListAsync().ConfigureAwait(false))
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
        /// <param name="sismaIndex"></param>
        /// <param name="reportType">1 - Спрени дела, 2 - Възовновени дела, 3 - по дата на делото, 4 - новообразувани по възобновяване</param>
        /// <param name="instanceId"></param>
        /// <param name="documentTypeIds"></param>
        /// <param name="groupByFromCourt"></param>
        /// <returns></returns>
        private async Task<List<SismaCaseStatisticsVM>> SismaCaseByFromCourt_Select(int courtTypeId, int courtId, int[] caseGroupIds,
            DateTime fromDate, DateTime toDate, string sismaIndex, int reportType, int instanceId,
            int[] documentTypeIds, bool groupByFromCourt)
        {
            Expression<Func<Case, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.CaseGroupId);

            Expression<Func<Case, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<Case, bool>> courtGroupWhere = x => true;
            if (groupByFromCourt == true)
            {
                courtGroupWhere = x => (x.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId)).Any() ||
                                     x.Document.DocumentInstitutionCaseInfo
                                      .Where(a => NomenclatureConstants.InstitutionTypes.StatisticsFromInstitution.Contains(a.Institution.InstitutionTypeId)).Any());
            }

            Expression<Func<Case, bool>> reportTypeWhere = await GetCaseByFromCourtWhere(reportType, fromDate, toDate).ConfigureAwait(false);

            var result = (await repo.AllReadonly<Case>()
                                .Where(GeneralCaseWhere())
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Where(GetDocumentTypeWhere(documentTypeIds))
                                .Where(courtGroupWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    CodeData = x.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId)).Any() ?
                                            x.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId))
                                            .Select(a => a.Court.Code + ",," + a.Court.Label)
                                            .FirstOrDefault() :
                                            x.Document.DocumentInstitutionCaseInfo
                                            .Where(a => NomenclatureConstants.InstitutionTypes.StatisticsFromInstitution.Contains(a.Institution.InstitutionTypeId))
                                            .Select(a => a.Institution.Code + ",," + a.Institution.InstitutionType.Label)
                                            .FirstOrDefault(),
                                    SismaIndex = sismaIndex,
                                })
                                .ToListAsync().ConfigureAwait(false))
                                .GroupBy(x => new
                                {
                                    x.CourtCode,
                                    x.CodeData,
                                    x.SismaIndex,
                                })
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Key.CourtCode,
                                    CodeData = x.Key.CodeData,
                                    SismaIndex = x.Key.SismaIndex,
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
        private async Task<List<CaseStatisticsVM>> Case_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId)
        {
            return await CaseByFromCourt_Select(courtTypeId, courtId, caseGroupIds,
                    fromDate, toDate, colIndex, rowIndex, reportType, instanceId, null, false).ConfigureAwait(false);
        }

        private Expression<Func<CasePersonSentence, bool>> GeneralCasePersonSentenceWhere()
        {
            Expression<Func<CasePersonSentence, bool>> reportTypeWhere = x => x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft &&
                                                                    x.DateExpired == null && (x.IsActive ?? false) == true &&
                                                                    x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false &&
                                                                    x.Case.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo;

            return reportTypeWhere;
        }

        private Expression<Func<CasePersonSentence, bool>> GetCasePersonSentenceCaseCodeWhere(int reportType, DateTime fromDate, DateTime toDate)
        {
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

            Expression<Func<CasePersonSentence, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = x => x.InforcedDate >= fromDateStart && x.InforcedDate <= toDateEnd;
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.InforcedDate >= fromDateStart && x.InforcedDate <= toDateEnd &&
                                  x.SentenceResultTypeId == NomenclatureConstants.SentenceResultTypes.Justified;
            }

            return reportTypeWhere;
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
        private async Task<List<CaseStatisticsVM>> CasePersonSentenceCaseCode_Select(int courtTypeId, int courtId,
    DateTime fromDate, DateTime toDate, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, int colIndex, int reportType,
    int instanceId)
        {
            Expression<Func<CasePersonSentence, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CasePersonSentence, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CasePersonSentence, bool>> reportTypeWhere = GetCasePersonSentenceCaseCodeWhere(reportType, fromDate, toDate);

            int[] caseCodes = excelReportCaseCodeRows.SelectMany(x => x.CaseCode).ToArray();

            var result = await repo.AllReadonly<CasePersonSentence>()
                                .Where(GeneralCasePersonSentenceWhere())
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => caseCodes.Contains(x.Case.CaseCodeId ?? 0))
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    ExcelCol = colIndex,
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelRow = GetRowFromCaseCodeRows(excelReportCaseCodeRows, x.CaseCodeId, colIndex));
            result = result.GroupBy(x => new
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
        /// <param name="sismaIndex"></param>
        /// <param name="reportType">1 - Влезли в сила присъди, 2 - Влезли в сила оправдателни присъди</param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        private async Task<List<SismaCaseStatisticsVM>> SismaCasePersonSentenceCaseCode_Select(int courtTypeId, int courtId,
    DateTime fromDate, DateTime toDate, string sismaIndex, int reportType, int instanceId, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            Expression<Func<CasePersonSentence, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CasePersonSentence, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CasePersonSentence, bool>> reportTypeWhere = GetCasePersonSentenceCaseCodeWhere(reportType, fromDate, toDate);

            int[] caseCodes = excelReportCaseCodeRows.SelectMany(x => x.CaseCode).ToArray();

            var result = await repo.AllReadonly<CasePersonSentence>()
                                .Where(GeneralCasePersonSentenceWhere())
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => caseCodes.Contains(x.Case.CaseCodeId ?? 0))
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    SismaIndex = sismaIndex,
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.CodeData = GetCaseCodeLabelFromCaseCodeRows(excelReportCaseCodeRows, x.CaseCodeId));
            result = result.GroupBy(x => new
            {
                x.CourtCode,
                x.CodeData,
                x.SismaIndex,
            })
            .Select(x => new SismaCaseStatisticsVM
            {
                CourtCode = x.Key.CourtCode,
                CodeData = x.Key.CodeData,
                SismaIndex = x.Key.SismaIndex,
                Count = x.Count(),
            })
            .ToList();

            return result;
        }

        private async Task<List<ExcelReportData>> RSSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 6, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 16, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 14, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes,
                      excelReportComplainResults, instanceId).ConfigureAwait(false));

                //Обжалвани
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 18, 5, instanceId).ConfigureAwait(false));
            }


            //Справка 1
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 51, 1, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 52, 2, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 53, 3, null, instanceId).ConfigureAwait(false));

            //Справка 2
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 57, 1, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 58, 2, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 59, 3, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 60, 1, instanceId).ConfigureAwait(false));

            //Справка 3
            int colIndex = 5;
            for (int i = 4; i <= 11; i++)
            {
                allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, colIndex, 52, i, null, instanceId).ConfigureAwait(false));
                colIndex++;
            }

            //Справка 4
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 64, 5, instanceId).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> RSSheet3(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 6, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 7, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 12, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 14, 9, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 10, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 16, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 17, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 19, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 20, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 22, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 23, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 25, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 26, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 27, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 28, 9, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 29, 10, instanceId).ConfigureAwait(false));
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId).ConfigureAwait(false));

                allData.AddRange(await CaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes,
                      excelReportComplainResults, instanceId).ConfigureAwait(false));
            }

            //Справка 3
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 134, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 135, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 136, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 137, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 138, 12, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 139, 13, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 140, 14, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 141, 1, instanceId).ConfigureAwait(false));

            //Справка 4
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 146, 4, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 147, 5, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 148, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 149, 7, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 150, 8, instanceId).ConfigureAwait(false));

            //Справка 6
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 160, 9, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 162, 10, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 163, 11, instanceId).ConfigureAwait(false));

            //Справка 7
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 168, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 169, 7, instanceId).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> MillitarySheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 8, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 9, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 10, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 14, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 15, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 17, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 18, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 19, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 20, 11, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 21, 12, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 22, 13, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 23, 14, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 15, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 25, 10, instanceId).ConfigureAwait(false));
            }

            //Справка 1
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 174, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 175, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 176, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 177, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 178, 12, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 179, 13, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 180, 14, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 181, 1, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 182, 2, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 183, 3, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 184, 1, instanceId).ConfigureAwait(false));

            //Справка 2
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 191, 4, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 192, 5, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 193, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 194, 7, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 195, 12, instanceId).ConfigureAwait(false));

            //Справка 3
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 202, 9, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 204, 10, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 205, 11, instanceId).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> OSSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows, List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults, string paramValue)
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 6, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 10, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes,
                      excelReportComplainResults, instanceId).ConfigureAwait(false));

                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 16, 5, instanceId).ConfigureAwait(false));

                //Обжалвани
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 18, 5, instanceId).ConfigureAwait(false));
            }


            //Справка 1
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 79, 1, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 80, 2, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 81, 3, null, instanceId).ConfigureAwait(false));

            //Справка 2
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 85, 1, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 86, 2, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 87, 3, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 88, 1, instanceId).ConfigureAwait(false));

            //Справка 3
            int colIndex = 5;
            for (int i = 4; i <= 11; i++)
            {
                allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo },
                                  fromDate, toDate, colIndex, 86, i, null, instanceId).ConfigureAwait(false));
                colIndex++;
            }
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.Trade }, fromDate, toDate, 13, 86, 15, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.Trade }, fromDate, toDate, 14, 86, 16, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.Trade }, fromDate, toDate, 15, 86, 17, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.Trade }, fromDate, toDate, 16, 86, 18, null, instanceId).ConfigureAwait(false));

            //Справка 4
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 92, 8, instanceId).ConfigureAwait(false));

            if (IsSystemInFeature(paramValue, NomenclatureConstants.SystemFeatures.StatisticsIspn))
            {
                //Справка 5

                //Подадени молби
                allData.AddRange(await CaseDocumentIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 9, 101, 1, instanceId, "21110-1").ConfigureAwait(false));
                allData.AddRange(await CaseDocumentIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 9, 102, 1, instanceId, "21111-1").ConfigureAwait(false));
                allData.AddRange(await CaseDocumentIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 9, 104, 1, instanceId, "24100-1").ConfigureAwait(false));
                allData.AddRange(await CaseDocumentIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 9, 105, 1, instanceId, "24111-1").ConfigureAwait(false));

                //Открити производства
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 10, 101, 1, instanceId, "21110-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 10, 102, 1, instanceId, "21111-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 10, 104, 1, instanceId, "24100-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 10, 105, 1, instanceId, "24111-1").ConfigureAwait(false));

                //Висящи производства
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 11, 101, 2, instanceId, "21110-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 11, 102, 2, instanceId, "21111-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 11, 104, 2, instanceId, "24100-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 11, 105, 2, instanceId, "24111-1").ConfigureAwait(false));

                //Приключени производства
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 12, 101, 3, instanceId, "21110-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 12, 102, 3, instanceId, "21111-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 12, 104, 3, instanceId, "24100-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 12, 105, 3, instanceId, "24111-1").ConfigureAwait(false));

                //Средна продължителност
                allData.AddRange(await CalcCaseIspnDurationFinishAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 13, 101, instanceId, "21110-1").ConfigureAwait(false));
                allData.AddRange(await CalcCaseIspnDurationFinishAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 13, 102, instanceId, "21111-1").ConfigureAwait(false));
                allData.AddRange(await CalcCaseIspnDurationFinishAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 13, 104, instanceId, "24100-1").ConfigureAwait(false));
                allData.AddRange(await CalcCaseIspnDurationFinishAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 13, 105, instanceId, "24111-1").ConfigureAwait(false));
                

                //Отхвърлени с акт по същество  	
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 14, 101, 4, instanceId, "21110-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 14, 102, 4, instanceId, "21111-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 14, 104, 4, instanceId, "24100-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 14, 105, 4, instanceId, "24111-1").ConfigureAwait(false));

                //Прекратени	
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 16, 101, 5, instanceId, "21110-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 16, 102, 5, instanceId, "21111-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 16, 104, 5, instanceId, "24100-1").ConfigureAwait(false));
                allData.AddRange(await CaseIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 16, 105, 5, instanceId, "24111-1").ConfigureAwait(false));

                //Брой длъжници
                allData.AddRange(await CaseDebtorIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 18, 101, instanceId, "21110-1").ConfigureAwait(false));
                allData.AddRange(await CaseDebtorIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 18, 102, instanceId, "21111-1").ConfigureAwait(false));
                allData.AddRange(await CaseDebtorIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 18, 104, instanceId, "24100-1").ConfigureAwait(false));
                allData.AddRange(await CaseDebtorIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 18, 105, instanceId, "24111-1").ConfigureAwait(false));

                //Длъжници Юридически лица	
                allData.AddRange(await CasePersonIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 21, 105, 1, instanceId, "21111-1").ConfigureAwait(false));
                allData.AddRange(await CasePersonIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 21, 105, 1, instanceId, "24111-1").ConfigureAwait(false));

                //Длъжници Физически лица	
                allData.AddRange(await CasePersonIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 22, 101, 1, instanceId, "21110-1").ConfigureAwait(false));
                allData.AddRange(await CasePersonIspn_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 22, 104, 1, instanceId, "24100-1").ConfigureAwait(false));

                //Справка 6

                //Подадени
                allData.AddRange(await DocumentRequest760_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 4, 115, 1, instanceId, NomenclatureConstants.DocumentType.Request760Entrepreneur).ConfigureAwait(false));
                allData.AddRange(await DocumentRequest760_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 4, 116, 1, instanceId, NomenclatureConstants.DocumentType.Request760ET).ConfigureAwait(false));

                //Висящи
                allData.AddRange(await CaseRequest760_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 5, 115, 1, instanceId, NomenclatureConstants.DocumentType.Request760Entrepreneur).ConfigureAwait(false));
                allData.AddRange(await CaseRequest760_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 5, 116, 1, instanceId, NomenclatureConstants.DocumentType.Request760ET).ConfigureAwait(false));

                //Прекратени
                allData.AddRange(await CaseRequest760_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 6, 115, 2, instanceId, NomenclatureConstants.DocumentType.Request760Entrepreneur).ConfigureAwait(false));
                allData.AddRange(await CaseRequest760_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 6, 116, 2, instanceId, NomenclatureConstants.DocumentType.Request760ET).ConfigureAwait(false));

                //Средна продължителност
                allData.AddRange(await CalcCaseRequest760Duration_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 7, 115, instanceId, NomenclatureConstants.DocumentType.Request760Entrepreneur).ConfigureAwait(false));
                allData.AddRange(await CalcCaseRequest760Duration_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 7, 116, instanceId, NomenclatureConstants.DocumentType.Request760ET).ConfigureAwait(false));


                //Брой решения за опрощаване 
                allData.AddRange(await CaseRequest760_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 9, 115, 3, instanceId, NomenclatureConstants.DocumentType.Request760Entrepreneur).ConfigureAwait(false));
                allData.AddRange(await CaseRequest760_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 9, 116, 3, instanceId, NomenclatureConstants.DocumentType.Request760ET).ConfigureAwait(false));

                //Брой решения за отхвърляне
                allData.AddRange(await CaseRequest760_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 10, 115, 4, instanceId, NomenclatureConstants.DocumentType.Request760Entrepreneur).ConfigureAwait(false));
                allData.AddRange(await CaseRequest760_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 10, 116, 4, instanceId, NomenclatureConstants.DocumentType.Request760ET).ConfigureAwait(false));
            }            

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> OSSheet4(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 8, 9, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 10, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 12, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonSentenceCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 16, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonSentenceCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 17, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 18, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 19, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 21, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 22, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 23, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 11, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 25, 12, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 26, 13, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 27, 14, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 28, 15, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 29, 10, instanceId).ConfigureAwait(false));
            }

            allData.AddRange(await Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 125, 2, instanceId).ConfigureAwait(false));

            //Справка 1
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 130, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 131, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 132, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 133, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 134, 12, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 135, 13, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 136, 14, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 137, 1, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 138, 3, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 139, 1, instanceId).ConfigureAwait(false));

            //Справка 2
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 145, 4, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 146, 5, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 147, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 148, 7, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 149, 12, instanceId).ConfigureAwait(false));

            //Справка 3
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 155, 9, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 157, 10, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 158, 11, instanceId).ConfigureAwait(false));

            //Справка 4
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 163, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 164, 7, instanceId).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    4, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }


        private async Task<Expression<Func<CaseLifecycle, bool>>> GetCaseLifecycleByFromCourtWhere(int reportType, DateTime fromDate, DateTime toDate)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = UnfinishedLifecycle(fromDate);
            }
            else if (reportType == 2)
            {
                reportTypeWhere = IncomingLifecycle(fromDate, toDate);
            }
            else if (reportType == 3)
            {
                reportTypeWhere = await FinishedLifecycleByType(fromDate, toDate, true).ConfigureAwait(false);
            }
            else if (reportType == 4)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date;
            }
            else if (reportType == 5)
            {
                reportTypeWhere = IncomingLifecycleWithoutCh80(fromDate, toDate);
            }
            else if (reportType == 6)
            {
                reportTypeWhere = IncomingLifecycleCh80(fromDate, toDate);
            }

            return reportTypeWhere;
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
        /// 4 - Свършени дела в периода, 5 - Постъпили дела с изключени Постъпили дела по чл. 80, ал. 10 ПАС, 6 - Постъпили дела по чл. 80, ал. 10 ПАС</param>
        /// <param name="documentTypeIds"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CaseLifecycleByFromCourt_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int reportType, int[] documentTypeIds,
    List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseLifecycle, bool>> complainWhere = x => true;
            if (colIndex <= 0)
            {
                int[] actComplainResults = excelReportComplainResults.SelectMany(a => a.ActComplainResult).ToArray();
                complainWhere = x => actComplainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0);
            }

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = await GetCaseLifecycleByFromCourtWhere(reportType, fromDate, toDate).ConfigureAwait(false);

            var result = await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycle())
                                .Where(x => x.Case.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance)
                                .Where(x => (x.Case.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId)).Any() || x.Case.Document.DocumentInstitutionCaseInfo
                                  .Where(a => NomenclatureConstants.InstitutionTypes.StatisticsFromInstitution.Contains(a.Institution.InstitutionTypeId)).Any()))
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(GetDocumentTypeCaseLifecycleWhere(documentTypeIds))
                                .Where(reportTypeWhere)
                                .Where(complainWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ActComplainResultId = x.CaseSessionAct.ActComplainResultId ?? 0,
                                    FromCourtData = x.Case.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId)).Any() ?
                                            x.Case.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId))
                                            .Select(a => a.CourtId + ",," + a.Court.Label + ",," +
                                              (a.Court.ParentCourtId == x.CourtId ? 0 : 1))
                                            .FirstOrDefault() :
                                            x.Case.Document.DocumentInstitutionCaseInfo
                                            .Where(a => NomenclatureConstants.InstitutionTypes.StatisticsFromInstitution.Contains(a.Institution.InstitutionTypeId))
                                            .Select(a => "I" + a.Institution.InstitutionTypeId + ",," + a.Institution.InstitutionType.Label + ",,999")
                                            .FirstOrDefault(),
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelCol = colIndex > 0 ? colIndex : GetColFromReportComplainResults(excelReportComplainResults, x.ActComplainResultId));
            result = result.GroupBy(x => new
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sismaIndex"></param>
        /// <param name="reportType"> 1 - несвършени дела в началото на отчетния период,
        /// 2 - Постъпили дела, 3 - прекратяване на делото,
        /// 4 - Свършени дела в периода, 5 - Постъпили дела с изключени Постъпили дела по чл. 80, ал. 10 ПАС, 6 - Постъпили дела по чл. 80, ал. 10 ПАС</param>
        /// <param name="documentTypeIds"></param>
        /// <returns></returns>
        private async Task<List<SismaCaseStatisticsVM>> SismaCaseLifecycleByFromCourt_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, string sismaIndex, int reportType, int[] documentTypeIds,
    List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.Case.CaseGroupId);

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseLifecycle, bool>> complainWhere = x => true;
            if (string.IsNullOrEmpty(sismaIndex) == true)
            {
                int[] actComplainResults = excelReportComplainResults.SelectMany(a => a.ActComplainResult).ToArray();
                complainWhere = x => actComplainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0);
            }

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = await GetCaseLifecycleByFromCourtWhere(reportType, fromDate, toDate).ConfigureAwait(false);

            var result = await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycle())
                                .Where(x => x.Case.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance)
                                .Where(x => (x.Case.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId)).Any() || x.Case.Document.DocumentInstitutionCaseInfo
                                  .Where(a => NomenclatureConstants.InstitutionTypes.StatisticsFromInstitution.Contains(a.Institution.InstitutionTypeId)).Any()))
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(GetDocumentTypeCaseLifecycleWhere(documentTypeIds))
                                .Where(reportTypeWhere)
                                .Where(complainWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    ActComplainResultId = x.CaseSessionAct.ActComplainResultId ?? 0,
                                    CodeData = x.Case.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId)).Any() ?
                                            x.Case.Document.DocumentCaseInfo.Where(a => a.Court.CourtTypeId == GetFromCourtType(courtTypeId))
                                            .Select(a => a.Court.Code + ",," + a.Court.Label)
                                            .FirstOrDefault() :
                                            x.Case.Document.DocumentInstitutionCaseInfo
                                            .Where(a => NomenclatureConstants.InstitutionTypes.StatisticsFromInstitution.Contains(a.Institution.InstitutionTypeId))
                                            .Select(a => a.Institution.Code + ",," + a.Institution.InstitutionType.Label)
                                            .FirstOrDefault(),
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.SismaIndex = string.IsNullOrEmpty(sismaIndex) == false ? sismaIndex :
                                          GetSismaIndexFromReportComplainResults(excelReportComplainResults, x.ActComplainResultId));

            result = result.GroupBy(x => new
            {
                x.CourtCode,
                x.CodeData,
                x.SismaIndex,
            })
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Key.CourtCode,
                                    CodeData = x.Key.CodeData,
                                    SismaIndex = x.Key.SismaIndex,
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

        private async Task<List<ExcelReportData>> ApealSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 2, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 11, 3, complains, null).ConfigureAwait(false));

            //Колони за Частни Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 13, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 14, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 15, 4, complains, null).ConfigureAwait(false));

            //Колони за Жалби по бавност
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsRequestSlownessTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 17, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults).ConfigureAwait(false));

            //Колони за Жалби по 274
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaint274TDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 20, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 21, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 22, 4, complains, null).ConfigureAwait(false));


            result.AddRange(SaveExcelByFromCourt(toDate, 14, allData, templateId, 2));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 40, 1, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 41, 2, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 42, 19, null, instanceId).ConfigureAwait(false));

            //Справка 2
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 47, 2, instanceId).ConfigureAwait(false));

            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }
            return result;
        }

        private async Task<List<ExcelReportData>> ApealSheet3(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби и протести
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND ||
                    x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 2, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults.Where(x => x.SheetIndex == 3).ToList()).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 18, 3, complains, null).ConfigureAwait(false));

            //Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 4, 2, complains, null).ConfigureAwait(false));

            //Протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 5, 2, complains, null).ConfigureAwait(false));


            //Колони за Частни Жалби и протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 20, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 21, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 22, 4, complains, null).ConfigureAwait(false));

            //Колони за Възобновяване
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 24, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 25, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 26, 4, complains, null).ConfigureAwait(false));

            result.AddRange(SaveExcelByFromCourt(toDate, 12, allData, templateId, 3));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 38, 1, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 39, 2, null, instanceId).ConfigureAwait(false));

            //Справка 3
            allDataGroup.AddRange(await CaseLifecycleComplain_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 0, 48, 13, instanceId,
                               excelReportComplainResults.Where(x => x.SheetIndex == 103).ToList()).ConfigureAwait(false));

            //Справка 4
            allDataGroup.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, excelReportCaseCodeRows,
                             13, 13, instanceId).ConfigureAwait(false));

            //Справка 5
            allDataGroup.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 23,
                             46, 14, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 23,
                             47, 4, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 23,
                             48, 15, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 23,
                             49, 16, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 23,
                             50, 17, instanceId).ConfigureAwait(false));

            //Справка 6
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             54, 3, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             55, 4, instanceId).ConfigureAwait(false));


            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count));
            }
            return result;
        }

        private async Task<List<ExcelReportData>> MillitaryAPSheet2(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби и протести
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND ||
                    x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 2, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList()).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 18, 3, complains, null).ConfigureAwait(false));

            //Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 4, 2, complains, null).ConfigureAwait(false));

            //Протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 5, 2, complains, null).ConfigureAwait(false));


            //Колони за Частни Жалби и протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 20, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 21, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 22, 4, complains, null).ConfigureAwait(false));

            result.AddRange(SaveExcelByFromCourt(toDate, 12, allData, templateId, 2));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 26, 1, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 27, 2, null, instanceId).ConfigureAwait(false));

            //Справка 3
            allDataGroup.AddRange(await CaseLifecycleComplain_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 0, 36, 13, instanceId,
                               excelReportComplainResults.Where(x => x.SheetIndex == 102).ToList()).ConfigureAwait(false));

            //Справка 4
            allDataGroup.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, excelReportCaseCodeRows,
                             16, 13, instanceId).ConfigureAwait(false));

            //Справка 5
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             42, 3, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             43, 4, instanceId).ConfigureAwait(false));


            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> OSSheet3(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD,
                    NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD};

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 2, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 10, 3, complains, null).ConfigureAwait(false));

            //Колони за Частни Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 12, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 13, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 14, 4, complains, null).ConfigureAwait(false));

            result.AddRange(SaveExcelByFromCourt(toDate, 13, allData, templateId, 3));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 30, 1, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 31, 2, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 32, 19, null, instanceId).ConfigureAwait(false));

            //Справка 2
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 39, 2, instanceId).ConfigureAwait(false));

            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> OSSheet5(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND,
                           NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND,
            NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND};

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби и протести
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND ||
                    x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 2, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults.Where(x => x.SheetIndex == 5).ToList()).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 18, 3, complains, null).ConfigureAwait(false));

            //Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 4, 2, complains, null).ConfigureAwait(false));

            //Протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 5, 2, complains, null).ConfigureAwait(false));


            //Колони за Частни Жалби и протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 20, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 21, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 22, 4, complains, null).ConfigureAwait(false));

            result.AddRange(SaveExcelByFromCourt(toDate, 13, allData, templateId, 5));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 30, 1, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 31, 2, null, instanceId).ConfigureAwait(false));

            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             36, 3, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             37, 4, instanceId).ConfigureAwait(false));


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
            int documentTypeId, int caseCodeId, int processPriorityId)
        {
            return caseTypesRows.Where(x => x.CaseType.Contains(caseTypeId))
                .Where(x => x.ForColumns.Contains(colIndex))
                .Where(a => (a.ProcessPriority.Count == 0 || a.ProcessPriority.Contains(processPriorityId)))
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
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="reportType">1 - несвършени дела в началото на отчетния период, 2 - Свършени до 3 месеца вкл.,
        /// 3 - Свършели през прекратени, 4 - Свършили само прекратени, 5 - Продължаващи под същия номер, 
        /// 6 - Споразу- мения по чл.382 НПК, 7 - Споразум. по чл.384 НПК , спог. по чл.24 ал. 3 НПК или чл.234 ГПК,
        /// 8 - Върнати за доразследване, 9 - Прекратени по други причини без 6,7,8, 10 - Постъпили дела по чл. 80 ал. 10 ПАС</param>
        /// <param name="colIndex"></param>
        /// <param name="caseTypesRows"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CaseTypeLifecycle_Select(int courtTypeId, int courtId,
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
                reportTypeWhere = await FinishedLifecycleByType(fromDate, toDate, false).ConfigureAwait(false);
            else if (reportType == 4)
                reportTypeWhere = await FinishedLifecycleByType(fromDate, toDate, true).ConfigureAwait(false);
            else if (reportType == 5)
                reportTypeWhere = ContinueCase(fromDate, toDate);
            else if (reportType == 6)
            {
                var actComplains = await ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop382).ConfigureAwait(false);
                reportTypeWhere = reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.IsFinalDoc &&
                                    actComplains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                    a.SessionResultId == NomenclatureConstants.CaseSessionResult.WithAgreement).Any();
            }
            else if (reportType == 7)
            {
                var actComplains = await ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop384).ConfigureAwait(false);
                reportTypeWhere = reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.IsFinalDoc &&
                                    actComplains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0);
            }
            else if (reportType == 8)
            {
                var sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsInvestigate).ConfigureAwait(false);
                reportTypeWhere = reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.IsFinalDoc &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                    sessionResults.Contains(a.SessionResultId)).Any();
            }
            else if (reportType == 9)
            {
                var actComplains = await ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop382).ConfigureAwait(false);
                var actComplains384 = await ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop384).ConfigureAwait(false);
                var sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsInvestigate).ConfigureAwait(false);

                var actComplainResults = await repo.AllReadonly<ActComplainResultGrouping>()
               .Where(x => x.ActComplainResultGroup == NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop)
               .Select(x => x.ActComplainResultId)
               .ToArrayAsync().ConfigureAwait(false);

                reportTypeWhere = reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date &&
                                    x.CaseSessionAct.IsFinalDoc &&
                                    (actComplains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                    a.SessionResultId == NomenclatureConstants.CaseSessionResult.WithAgreement).Any()) == false &&
                                    actComplains384.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) == false &&
                                    x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                    sessionResults.Contains(a.SessionResultId)).Any() == false &&
                                    (actComplainResults.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) ||
                                         x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                          a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                         .Any()) == true;
            }
            else if (reportType == 10)
            {
                reportTypeWhere = reportTypeWhere = x => x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress && x.DateFrom.Date >= fromDate.Date &&
                                    x.DateFrom.Date <= toDate.Date && x.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS;
            }

            var result = await repo.AllReadonly<CaseLifecycle>()
                            .Where(GeneralLifecycle())
                            .Where(courtWhere)
                            .Where(courtTypeWhere)
                            .Where(reportTypeWhere)
                            .Select(x => new CaseStatisticsVM
                            {
                                CourtId = x.CourtId ?? 0,
                                CaseTypeId = x.Case.CaseTypeId,
                                DocumentTypeId = x.Case.Document.DocumentTypeId,
                                CaseCodeId = x.Case.CaseCodeId ?? 0,
                                ExcelCol = colIndex,
                                ProcessPriorityId = x.Case.ProcessPriorityId ?? 0
                            })
                            .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelRow = GetRowFromCaseTypeRows(caseTypesRows, x.CaseTypeId, colIndex, x.DocumentTypeId, x.CaseCodeId, x.ProcessPriorityId));
            result = result.GroupBy(x => new
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
        private async Task<List<CaseStatisticsVM>> CaseTypeCase_Select(int courtTypeId, int courtId,
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
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                                        repo.AllReadonly<CaseMigration>().Any(a => a.CaseId == x.Id &&
                                                   a.DateExpired == null &&
                                                   a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                   a.OutCaseMigrationId != null &&
                                                   a.PriorCase.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                     s.CaseSessionResults.Any(r => (r.SessionResultId == NomenclatureConstants.CaseSessionResult.StopProduction &&
                                                                                                             NomenclatureConstants.CaseSessionResultBase.FiledCasesFirstInstance.Contains(r.SessionResultBaseId ?? 0)) ||
                                                                                                             NomenclatureConstants.CaseSessionResult.FiledCasesFirstInstance.Contains(r.SessionResultId))));
            }
            else if (reportType == 5)
            {
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date &&
                             repo.AllReadonly<CaseMigration>().Where(a => a.DateExpired == null).Any(a => a.CaseId == x.Id &&
                                                         a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                              repo.AllReadonly<CaseMigration>().Where(a => a.DateExpired == null)
                                                                               .Where(m => m.InitialCaseId == a.InitialCaseId &&
                                                                                  m.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                                  m.Id < a.Id)
                                                                               .OrderByDescending(m => m.Id)
                                                                               .Select(m => m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ForAdministration)
                                                                               .FirstOrDefault());
            }

            var result = await repo.AllReadonly<Case>()
                                .Where(GeneralCaseWhere())
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    CaseTypeId = x.CaseTypeId,
                                    DocumentTypeId = x.Document.DocumentTypeId,
                                    CaseCodeId = x.CaseCodeId ?? 0,
                                    ExcelCol = colIndex,
                                    ProcessPriorityId = x.ProcessPriorityId ?? 0
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelRow = GetRowFromCaseTypeRows(caseTypesRows, x.CaseTypeId, colIndex, x.DocumentTypeId, x.CaseCodeId, x.ProcessPriorityId));
            result = result.GroupBy(x => new
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
        private async Task<List<CaseStatisticsVM>> CaseTypeCaseSession_Select(int courtTypeId, int courtId,
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

            var result = await repo.AllReadonly<CaseSession>()
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(x => x.DateFrom.Date >= fromDate.Date && x.DateFrom.Date <= toDate.Date)
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.Case.CourtId,
                                    CaseTypeId = x.Case.CaseTypeId,
                                    DocumentTypeId = x.Case.Document.DocumentTypeId,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    ExcelCol = colIndex,
                                    ProcessPriorityId = x.Case.ProcessPriorityId ?? 0
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelRow = GetRowFromCaseTypeRows(caseTypesRows, x.CaseTypeId, colIndex, x.DocumentTypeId, x.CaseCodeId, x.ProcessPriorityId));
            result = result.GroupBy(x => new
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

        private async Task<List<ExcelReportData>> ApealSheet1(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows, int courtTypeId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            var allData = await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 3, excelReportCaseTypeRows).ConfigureAwait(false);

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 4, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 5, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 8, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 10, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 11, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCaseSession_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 12, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 14, excelReportCaseTypeRows).ConfigureAwait(false));


            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    1, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> DistrictSheet1(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows,
            List<StatisticsExcelReportIspnReasonVM> excelReportIspnReasons)
        {
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;

            List<ExcelReportData> result = new List<ExcelReportData>();

            var allData = await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 3, excelReportCaseTypeRows).ConfigureAwait(false);

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 4, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 5, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 6, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 7, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 8, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 12, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 14, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 6, 16, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 7, 17, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 8, 18, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 9, 19, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCaseSession_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 20, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 22, excelReportCaseTypeRows).ConfigureAwait(false));

            //Решения по дела за несъстоятелност
            allData.AddRange(await ActIspnReason_Select(courtTypeId, searchCourtId, fromDate, toDate, 63, excelReportIspnReasons).ConfigureAwait(false));

            //Справка за постановени решения за промени по фирмени дела
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.Company }, fromDate, toDate, 14, 64, 9, NomenclatureConstants.CaseInstanceType.FirstInstance).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    1, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> MillitarySheet1(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows)
        {
            int courtTypeId = NomenclatureConstants.CourtType.Millitary;

            List<ExcelReportData> result = new List<ExcelReportData>();

            var allData = await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 3, excelReportCaseTypeRows).ConfigureAwait(false);

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 4, excelReportCaseTypeRows).ConfigureAwait(false));


            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 5, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 6, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 10, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 12, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 6, 14, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 7, 15, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 8, 16, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 9, 17, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCaseSession_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 18, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 20, excelReportCaseTypeRows).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    1, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> RSSheet1(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows)
        {
            int courtTypeId = NomenclatureConstants.CourtType.RegionalCourt;

            List<ExcelReportData> result = new List<ExcelReportData>();

            var allData = await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 3, excelReportCaseTypeRows).ConfigureAwait(false);

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 4, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 5, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 6, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 7, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 11, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 13, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 6, 15, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 7, 16, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 8, 17, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 9, 18, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCaseSession_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 19, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 21, excelReportCaseTypeRows).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    1, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> RSExcelTitle(DateTime toDate, int searchCourtId, int templateId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = await repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToListAsync().ConfigureAwait(false);
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

        private async Task<List<ExcelReportData>> OSExcelTitle(DateTime toDate, int searchCourtId, int templateId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = await repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToListAsync().ConfigureAwait(false);
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

        private async Task<List<ExcelReportData>> ApealExcelTitle(DateTime toDate, int searchCourtId, int templateId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = await repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToListAsync().ConfigureAwait(false);
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

        private async Task<List<ExcelReportData>> MillitaryExcelTitle(DateTime toDate, int searchCourtId, int templateId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = await repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Millitary)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToListAsync().ConfigureAwait(false);
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

        private async Task<List<ExcelReportData>> MillitaryApealExcelTitle(DateTime toDate, int searchCourtId, int templateId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = await repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.MillitaryApeal)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToListAsync().ConfigureAwait(false);
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

        private async Task<List<CaseStatisticsVM>> ActIspnReason_Select(int courtTypeId, int courtId,
            DateTime fromDate, DateTime toDate, int rowIndex,
            List<StatisticsExcelReportIspnReasonVM> excelReportIspnReasons)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

            Expression<Func<CaseSessionAct, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseSessionAct, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;


            var result = await repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
                                .Where(x => x.ActTypeId == NomenclatureConstants.ActType.Answer)
                                .Where(x => x.ActDeclaredDate != null)
                                .Where(x => x.ActDeclaredDate >= fromDateStart &&
                                            x.ActDeclaredDate <= toDateEnd)
                                .Where(x => x.ActISPNReasonId != null)
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ActISPNReasonId = x.ActISPNReasonId ?? 0,
                                    ExcelRow = rowIndex,
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.ExcelCol = excelReportIspnReasons.Where(a => a.ActIspnReason.Contains(x.ActISPNReasonId))
                                               .Select(a => a.Col)
                                               .FirstOrDefault());
            result = result.GroupBy(x => new
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

        private async Task<List<SismaCaseStatisticsVM>> SismaCaseLawUnitCaseType_Select(int courtTypeId, int courtId, int[] caseGroupIds,
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
                reportTypeWhere = IncomingLifecycle(fromDate, toDate);
            else if (reportType == NomenclatureConstants.StatisticReportTypes.Finished3months)
                reportTypeWhere = FinishedLifecycleMonths(fromDate, toDate, 0, 3);
            else if (reportType == NomenclatureConstants.StatisticReportTypes.FinishedStop || reportType == NomenclatureConstants.StatisticReportTypes.FinishedNoStop)
            {
                if (reportType == NomenclatureConstants.StatisticReportTypes.FinishedStop)
                {
                    reportTypeWhere = await FinishedLifecycleByType(fromDate, toDate, true).ConfigureAwait(false);
                }
                else if (reportType == NomenclatureConstants.StatisticReportTypes.FinishedNoStop)
                {
                    reportTypeWhere = await FinishedLifecycleByType(fromDate, toDate, false).ConfigureAwait(false);
                }
            }

            var result = await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycle())
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    CaseTypeId = x.Case.CaseTypeId,
                                    DocumentTypeId = x.Case.Document.DocumentTypeId,
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    ProcessPriorityId = x.Case.ProcessPriorityId ?? 0,
                                    CodeData = x.CaseSessionActId != null ?
                                               x.CaseSessionAct.CaseSession.CaseLawUnits
                                              .Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                              .Where(a => (a.DateTo ?? dateEnd) >= x.CaseSessionAct.CaseSession.DateFrom)
                                              .OrderByDescending(a => a.DateFrom)
                                              .Select(a => a.LawUnit.Uic + ",," + a.LawUnit.FullName)
                                              .FirstOrDefault()
                                               :
                                               x.Case.CaseLawUnits
                                              .Where(a => a.CaseSessionId == null)
                                              .Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                              .Where(a => (a.DateTo ?? dateEnd) >= (reportType == NomenclatureConstants.StatisticReportTypes.Unfinished ? fromDate : toDate))
                                              .OrderByDescending(a => a.DateFrom)
                                              .Select(a => a.LawUnit.Uic + ",," + a.LawUnit.FullName)
                                              .FirstOrDefault()
                                })
                                .ToListAsync().ConfigureAwait(false);

            result.ForEach(x => x.SismaIndex = courtTypeCaseTypesCols.Where(a => a.CaseType.Contains(x.CaseTypeId))
                .Where(a => a.ReportTypeId == reportType)
                .Where(a => (a.ProcessPriority.Count == 0 || a.ProcessPriority.Contains(x.ProcessPriorityId)))
                .Where(a => ((a.DocumentType.Count == 0 || a.DocumentType.Contains(x.DocumentTypeId)) &&
                            (a.CaseCode.Count == 0 || a.CaseCode.Contains(x.CaseCodeId))) == a.IsTrue)
                .Select(a => a.SismaIndex)
                .FirstOrDefault());

            result = result.GroupBy(x => new
                                {
                                    x.CourtCode,
                                    x.SismaIndex,
                                    x.CodeData,
                                })
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Key.CourtCode,
                                    SismaIndex = x.Key.SismaIndex,
                                    CodeData = x.Key.CodeData,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        private void SismaFillDataRecap(List<SismaCaseStatisticsVM> allData, List<SismaIndexRecap> sismaIndexRecaps)
        {
            foreach (var sisma in sismaIndexRecaps.OrderBy(x => x.OrderNumber))
            {
                var sismaContains = sisma.RecapContains.Select(a => a.SismaIndexContains).ToArray();
                var recaps = allData.Where(x => sismaContains.Contains(x.SismaIndex))
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.CourtCode,
                                    CodeData = x.CodeData ?? String.Empty,
                                    CountRecap = x.Count * sisma.RecapContains.Where(a => a.SismaIndexContains == x.SismaIndex).Select(a => a.Sign).FirstOrDefault(),
                                })
                                .GroupBy(x => new
                                {
                                    x.CourtCode,
                                    x.CodeData,
                                })
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Key.CourtCode,
                                    CodeData = x.Key.CodeData,
                                    SismaIndex = sisma.SismaIndex,
                                    Count = x.Select(a => a.CountRecap).Sum(),
                                })
                                .ToList();

                allData.AddRange(recaps);
            }
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaSheetCaseCount(DateTime fromDate, DateTime toDate, int searchCourtId,
            int[] caseGroupIds, int courtTypeId, List<StatisticsExcelReportCaseTypeColVM> courtTypeCaseTypesCols)
        {
            var allData = await SismaCaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.Unfinished, courtTypeCaseTypesCols).ConfigureAwait(false);

            allData.AddRange(await SismaCaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.Incoming, courtTypeCaseTypesCols).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.Finished3months, courtTypeCaseTypesCols).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.FinishedStop, courtTypeCaseTypesCols).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLawUnitCaseType_Select(courtTypeId, searchCourtId,
                  caseGroupIds, fromDate, toDate,
                  NomenclatureConstants.StatisticReportTypes.FinishedNoStop, courtTypeCaseTypesCols).ConfigureAwait(false));

            return allData;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaRS_Sheets(DateTime fromDate, DateTime toDate, StatisticsNomDataVM nomData, int searchCourtId, int sheetIndex)
        {
            List<SismaCaseStatisticsVM> result = new List<SismaCaseStatisticsVM>();

            int courtTypeId = NomenclatureConstants.CourtType.RegionalCourt;

            List<StatisticsExcelReportCaseTypeColVM> excelReportCaseTypeCols = nomData.excelReportCaseTypeCols
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults = nomData.excelReportComplainResults
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            if (sheetIndex == 12 || sheetIndex == 0)
                result.AddRange(await SismaSheetCaseCount(fromDate, toDate, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo },
                       courtTypeId, excelReportCaseTypeCols).ConfigureAwait(false));

            if (sheetIndex == 13 || sheetIndex == 0)
                result.AddRange(await SismaSheetCaseCount(fromDate, toDate, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                 courtTypeId, excelReportCaseTypeCols).ConfigureAwait(false));

            //Sheet 6
            if (sheetIndex == 6 || sheetIndex == 0)
                result.AddRange(await SismaRSSheet6(fromDate, toDate, searchCourtId, excelReportComplainResults.Where(x => x.SheetIndex == 103).ToList(),
                                                                                     excelReportCaseCodeRows.Where(x => x.SheetIndex == 103).ToList()).ConfigureAwait(false));

            //Sheet 8
            if (sheetIndex == 8 || sheetIndex == 0)
                result.AddRange(await SismaRSSheet8(fromDate, toDate, searchCourtId).ConfigureAwait(false));

            //Sheet 9
            if (sheetIndex == 9 || sheetIndex == 0)
                result.AddRange(await SismaRSSheet9(fromDate, toDate, searchCourtId).ConfigureAwait(false));

            //Sheet 10
            if (sheetIndex == 10 || sheetIndex == 0)
                result.AddRange(await SismaRSSheet10(fromDate, toDate, searchCourtId, excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList(),
                                                                                      excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList()).ConfigureAwait(false));

            //Sheet 11
            if (sheetIndex == 11 || sheetIndex == 0)
                result.AddRange(await SismaRSSheet11(fromDate, toDate, searchCourtId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 3).ToList()).ConfigureAwait(false));

            return result;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaOS_Sheets(DateTime fromDate, DateTime toDate, StatisticsNomDataVM nomData, int searchCourtId, int sheetIndex)
        {
            List<SismaCaseStatisticsVM> result = new List<SismaCaseStatisticsVM>();

            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;

            List<StatisticsExcelReportCaseTypeColVM> excelReportCaseTypeCols = nomData.excelReportCaseTypeCols
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults = nomData.excelReportComplainResults
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            if (sheetIndex == 14 || sheetIndex == 0)
                result.AddRange(await SismaSheetCaseCount(fromDate, toDate, searchCourtId,
                   new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade, NomenclatureConstants.CaseGroups.Company },
                   courtTypeId, excelReportCaseTypeCols).ConfigureAwait(false));

            if (sheetIndex == 15 || sheetIndex == 0)
                result.AddRange(await SismaSheetCaseCount(fromDate, toDate, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                 courtTypeId, excelReportCaseTypeCols).ConfigureAwait(false));

            //Sheet 1
            if (sheetIndex == 1 || sheetIndex == 0)
                result.AddRange(await SismaOSSheet1(fromDate, toDate, searchCourtId, excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList(),
                                                      excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList()).ConfigureAwait(false));

            //Sheet 2
            if (sheetIndex == 2 || sheetIndex == 0)
                result.AddRange(await SismaOSSheet2(fromDate, toDate, searchCourtId, excelReportComplainResults.Where(x => x.SheetIndex == 3).ToList()).ConfigureAwait(false));

            //Sheet 3
            if (sheetIndex == 3 || sheetIndex == 0)
                result.AddRange(await SismaOSSheet3(fromDate, toDate, searchCourtId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 4).ToList()).ConfigureAwait(false));

            //Sheet 4
            if (sheetIndex == 4 || sheetIndex == 0)
                result.AddRange(await SismaOSSheet4(fromDate, toDate, searchCourtId,
                   excelReportComplainResults.Where(x => x.SheetIndex == 5).ToList()).ConfigureAwait(false));

            //Sheet 7
            if (sheetIndex == 7 || sheetIndex == 0)
                result.AddRange(await SismaOSSheet7(fromDate, toDate, searchCourtId).ConfigureAwait(false));

            return result;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaFillData(DateTime fromDate, DateTime toDate, int courtId, int sheetIndex)
        {
            List<SismaCaseStatisticsVM> result = new List<SismaCaseStatisticsVM>();

            int courtTypeId = 0;
            if (courtId > 0)
            {
                courtTypeId = await repo.AllReadonly<Court>()
                               .Where(x => x.Id == courtId)
                               .Select(x => x.CourtTypeId)
                               .FirstOrDefaultAsync().ConfigureAwait(false);
            }

            StatisticsNomDataVM nomData = await GetStatisticsNomData().ConfigureAwait(false);
            nomData.sismaIndexRecaps = await repo.AllReadonly<SismaIndexRecap>()
                                    .Include(x => x.RecapContains)
                                    .ToListAsync().ConfigureAwait(false);

            nomData.excelSismaMappings = await repo.AllReadonly<ExcelSismaMapping>()
                                    .ToListAsync().ConfigureAwait(false);

            nomData.courts = await repo.AllReadonly<Court>()
                                    .ToListAsync().ConfigureAwait(false);

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
            {
                result.AddRange(await SismaRS_Sheets(fromDate, toDate, nomData, courtId, sheetIndex).ConfigureAwait(false));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
            {
                result.AddRange(await SismaOS_Sheets(fromDate, toDate, nomData, courtId, sheetIndex).ConfigureAwait(false));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.Apeal)
            {
                result.AddRange(await SismaAP_Sheets(fromDate, toDate, nomData, courtId, sheetIndex).ConfigureAwait(false));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.Millitary)
            {
                result.AddRange(await SismaMillitary_Sheets(fromDate, toDate, nomData, courtId, sheetIndex).ConfigureAwait(false));
            }

            if (sheetIndex == 16)
                result.AddRange(await SismaSheet16(fromDate, toDate, nomData, 0).ConfigureAwait(false));

            SismaFillDataRecap(result, nomData.sismaIndexRecaps);

            return result;
        }

        private async Task<List<StatisticsExcelReportCaseTypeColVM>> GetExcelReportCaseTypeCol()
        {
            return await repo.AllReadonly<ExcelReportCaseTypeCol>()
                                    .Select(x => new StatisticsExcelReportCaseTypeColVM()
                                    {
                                        CourtTypeId = x.CourtTypeId,
                                        ReportTypeId = x.ReportTypeId,
                                        CaseTypeIds = x.CaseTypeId,
                                        DocumentTypeIds = x.DocumentTypeId ?? "",
                                        CaseCodeIds = x.CaseCodeId ?? "",
                                        ColIndex = x.ColIndex,
                                        IsTrue = x.IsTrue,
                                        SismaIndex = x.SismaIndex,
                                        ProcessPriorityIds = x.ProcessPriorityId ?? "",
                                    }).ToListAsync().ConfigureAwait(false);
        }

        private async Task<List<StatisticsExcelReportComplainIndexVM>> GetExcelReportComplainResult()
        {
            return await repo.AllReadonly<ExcelReportComplainResult>()
                                    .Select(x => new StatisticsExcelReportComplainIndexVM()
                                    {
                                        CourtTypeId = x.CourtTypeId,
                                        SheetIndex = x.SheetIndex,
                                        ActComplainResultIds = x.ActComplainResult,
                                        Col = x.ColIndex,
                                        SismaIndex = x.SismaIndex,
                                    }).ToListAsync().ConfigureAwait(false);
        }

        private async Task<List<StatisticsExcelReportCaseCodeRowVM>> GetExcelReportCaseCodeRow()
        {
            return await repo.AllReadonly<ExcelReportCaseCodeRow>()
                                    .Select(x => new StatisticsExcelReportCaseCodeRowVM()
                                    {
                                        CourtTypeId = x.CourtTypeId ?? 0,
                                        SheetIndex = x.SheetIndex,
                                        CaseCodeIds = x.CaseCodeId,
                                        RowIndex = x.RowIndex,
                                        CaseCodeLabel = x.CaseCodeLabel,
                                        ExcludeColIds = x.ExcludeCol ?? "",
                                    }).ToListAsync().ConfigureAwait(false);

        }

        private async Task<List<StatisticsExcelReportIspnReasonVM>> GetExcelReportIspnReason()
        {
            return await repo.AllReadonly<ExcelReportActIspnReason>()
                                    .Select(x => new StatisticsExcelReportIspnReasonVM()
                                    {
                                        CourtTypeId = x.CourtTypeId,
                                        SheetIndex = x.SheetIndex,
                                        ActIspnReasonIds = x.ActIspnReason,
                                        Col = x.ColIndex
                                    }).ToListAsync().ConfigureAwait(false);


        }

        private async Task<List<StatisticsExcelReportIndexVM>> GetExcelReportIndex()
        {
            var caseGroups = await repo.AllReadonly<CaseGroup>()
                               .Include(x => x.CaseTypes)
                               .ToListAsync().ConfigureAwait(false);


            var result = await repo.AllReadonly<ExcelReportIndex>()
                                    .Select(x => new StatisticsExcelReportIndexVM()
                                    {
                                        CourtTypeId = x.CourtTypeId,
                                        CaseGroupId = x.CaseGroupId,
                                        CaseTypeIds = x.CaseTypeId,
                                        ActTypeIds = x.ActTypeId,
                                        ActComplainIndexIds = x.ActComplainIndex,
                                        Col = x.ColIndex,
                                        SheetIndex = x.SheetIndex,
                                    }).ToListAsync().ConfigureAwait(false);

            foreach (var item in result)
            {
                if (string.IsNullOrEmpty(item.CaseGroupId) == false)
                {
                    var caseGroupIds = item.CaseGroupId.Split(",");
                    item.CaseGroupCaseTypeIds = caseGroups.Where(x => caseGroupIds.Contains(x.Id.ToString())).SelectMany(x => x.CaseTypes.Select(a => a.Id)).ToList();
                }
            }

            return result;
        }

        private async Task<List<StatisticsExcelReportCaseTypeRowVM>> GetExcelReportCaseTypeRow()
        {
            return await repo.AllReadonly<ExcelReportCaseTypeRow>()
                                    .Select(x => new StatisticsExcelReportCaseTypeRowVM()
                                    {
                                        CourtTypeId = x.CourtTypeId,
                                        CaseTypeIds = x.CaseTypeId,
                                        DocumentTypeIds = x.DocumentTypeId ?? "",
                                        CaseCodeIds = x.CaseCodeId ?? "",
                                        ForColumnIds = x.ForColumns,
                                        RowIndex = x.RowIndex,
                                        IsTrue = x.IsTrue,
                                        ProcessPriorityIds = x.ProcessPriorityId ?? "",
                                        SheetIndex = x.SheetIndex,
                                    }).ToListAsync().ConfigureAwait(false);
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaOSSheet1(DateTime fromDate, DateTime toDate, int searchCourtId,
                    List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<SismaCaseStatisticsVM> allData = new List<SismaCaseStatisticsVM>();

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

                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "101002", 1, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "101003", 2, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "101004", 3, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "101005", 4, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "101006", 1, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "101008", 2, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "101009", 3, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate,
                      excelReportComplainResults, instanceId, null, caseCodes).ConfigureAwait(false));

                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "101014", 4, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "101015", 5, instanceId, caseCodes).ConfigureAwait(false));

                //Обжалвани
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "101016", 5, instanceId, null, caseCodes).ConfigureAwait(false));
            }

            return allData;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaOSSheet3(DateTime fromDate, DateTime toDate, int searchCourtId, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<SismaCaseStatisticsVM> allData = new List<SismaCaseStatisticsVM>();

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

                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "103001", 1, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "103002", 6, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "103003", 4, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "103004", 1, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "103007", 9, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "103008", 6, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "103009", 7, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "103010", 8, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "103011", 2, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "103014", 5, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonSentenceCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103015", 1, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonSentenceCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103016", 2, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103017", 1, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103018", 2, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103019", 3, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103030", 16, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103031", 17, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103028", 4, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103026", 5, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103020", 11, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103021", 12, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103027", 13, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103022", 14, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103023", 15, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "103024", 10, instanceId, caseCodes).ConfigureAwait(false));
            }

            return allData;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaOSSheet2(DateTime fromDate, DateTime toDate, int searchCourtId,
                    List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD,
                    NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD};

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "102002", 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "102003", 2, complains, null).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "", 4, complains, excelReportComplainResults).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "102009", 3, complains, null).ConfigureAwait(false));

            //Колони за Частни Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "102012", 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await SismaCaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "102013", 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "102014", 4, complains, null).ConfigureAwait(false));


            return allData.Where(x => string.IsNullOrEmpty(x.CodeData) == false).ToList();
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaOSSheet4(DateTime fromDate, DateTime toDate, int searchCourtId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND,
                           NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND,
            NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND};

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби и протести
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND ||
                    x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "104002", 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "104003", 2, complains, null).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "", 4, complains, excelReportComplainResults).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "104011", 3, complains, null).ConfigureAwait(false));

            //Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "104004", 2, complains, null).ConfigureAwait(false));

            //Протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "104005", 2, complains, null).ConfigureAwait(false));


            //Колони за Частни Жалби и протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "104014", 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await SismaCaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "104015", 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "104016", 4, complains, null).ConfigureAwait(false));

            return allData.Where(x => string.IsNullOrEmpty(x.CodeData) == false).ToList();
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaRSSheet6(DateTime fromDate, DateTime toDate, int searchCourtId,
                    List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<SismaCaseStatisticsVM> allData = new List<SismaCaseStatisticsVM>();

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

                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "106002", 1, instanceId,
                                        new int[] { NomenclatureConstants.CaseTypes.AND }, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "106003", 6, instanceId,
                                        new int[] { NomenclatureConstants.CaseTypes.AND }, caseCodes).ConfigureAwait(false));

                allData.AddRange(await SismaCaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate,
                                        excelReportComplainResults, instanceId,
                                        new int[] { NomenclatureConstants.CaseTypes.AND }, caseCodes).ConfigureAwait(false));
            }

            return allData;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaRSSheet8(DateTime fromDate, DateTime toDate, int searchCourtId)
        {
            List<SismaCaseStatisticsVM> allData = new List<SismaCaseStatisticsVM>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.RegionalCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.FirstInstance;

            allData.AddRange(await SismaCaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "108001", 5, instanceId).ConfigureAwait(false));

            return allData;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaRSSheet9(DateTime fromDate, DateTime toDate, int searchCourtId)
        {
            List<SismaCaseStatisticsVM> allData = new List<SismaCaseStatisticsVM>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.RegionalCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.FirstInstance;

            //Справка 7
            allData.AddRange(await SismaCaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "109001", 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await SismaCaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "109002", 7, instanceId).ConfigureAwait(false));

            return allData;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaRSSheet10(DateTime fromDate, DateTime toDate, int searchCourtId,
                    List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<SismaCaseStatisticsVM> allData = new List<SismaCaseStatisticsVM>();

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

                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "110002", 1, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "110003", 2, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "110004", 3, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "110005", 4, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "110006", 1, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "110014", 2, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "110015", 3, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "110012", 4, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "110013", 5, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate,
                      excelReportComplainResults, instanceId, null, caseCodes).ConfigureAwait(false));

                //Обжалвани
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "110017", 5, instanceId, null, caseCodes).ConfigureAwait(false));
            }

            return allData;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaRSSheet11(DateTime fromDate, DateTime toDate, int searchCourtId, List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<SismaCaseStatisticsVM> allData = new List<SismaCaseStatisticsVM>();

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

                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111002", 1, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111006", 6, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111003", 4, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111004", 7, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111005", 8, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111007", 1, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111010", 6, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111011", 7, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111012", 8, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111013", 9, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111014", 10, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111015", 2, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "111016", 5, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111019", 1, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111020", 2, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111030", 16, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111021", 3, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111022", 4, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111023", 5, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111024", 6, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111025", 7, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111026", 8, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111027", 9, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "111028", 10, instanceId, caseCodes).ConfigureAwait(false));
            }

            return allData;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaOSSheet7(DateTime fromDate, DateTime toDate, int searchCourtId)
        {
            List<SismaCaseStatisticsVM> allData = new List<SismaCaseStatisticsVM>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;

            allData.AddRange(await SismaCaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "107001", 1).ConfigureAwait(false));
            allData.AddRange(await SismaCase_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "107002", 1).ConfigureAwait(false));
            allData.AddRange(await SismaCaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "107003", 2).ConfigureAwait(false));
            allData.AddRange(await SismaCaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "107004", 3).ConfigureAwait(false));
            allData.AddRange(await SismaCaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "107005", 4).ConfigureAwait(false));

            return allData;
        }

        public async Task<byte[]> TestPrintSisma(DateTime fromDate, DateTime toDate, int courtId, int sheetIndex)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var data = await SismaFillData(fromDate, toDate, courtId, sheetIndex).ConfigureAwait(false);

            excelService.AddList(
                data,
                new int[] { 5000, 5000, 5000, 5000 },
                new List<Expression<Func<SismaCaseStatisticsVM, object>>>()
                {
                    x => x.CourtCode,
                    x => x.SismaIndex,
                    x => x.CodeData,
                    x => x.Count,
                },
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="caseGroupIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sismaIndex"></param>
        /// <param name="reportType">1 - Несвършили за възобновяване,
        /// 2 - Свършени по възобновяване, 3 - Решени възобновяване, 4 - Решени уважени възобновяване</param>
        /// <returns></returns>
        private async Task<List<SismaCaseStatisticsVM>> SismaCaseLifecycle_Select(int courtTypeId, int courtId, int[] caseGroupIds,
                    DateTime fromDate, DateTime toDate, string sismaIndex, int reportType)
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
                reportTypeWhere = x => (x.Iteration == 1 ? x.Case.RegDate.Date : x.DateFrom.Date) <= toDate.Date && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                       (x.Case.IsRenewCase ?? false) == true;
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && (x.Case.IsRenewCase ?? false) == true;
            }
            else if (reportType == 3)
            {
                var complains = await ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseStop).ConfigureAwait(false);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && (x.Case.IsRenewCase ?? false) == true &&
                                 complains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0) == false;
            }
            else if (reportType == 4)
            {
                var complains = await ActComplainResultGrouping_Select(NomenclatureConstants.ActComplainResultGroupings.StatisticsCaseSuccessful).ConfigureAwait(false);

                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd).Date >= fromDate.Date &&
                                    (x.DateTo ?? dateEnd).Date <= toDate.Date && (x.Case.IsRenewCase ?? false) == true &&
                                 complains.Contains(x.CaseSessionAct.ActComplainResultId ?? 0);
            }

            var result = (await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycle())
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    SismaIndex = sismaIndex,
                                })
                                .ToListAsync().ConfigureAwait(false))
                                .GroupBy(x => new
                                {
                                    x.CourtCode,
                                    x.SismaIndex,
                                })
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Key.CourtCode,
                                    SismaIndex = x.Key.SismaIndex,
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
        /// <param name="sismaIndex"></param>
        /// <param name="reportType">1 - Новопостъпили по възобновяване</param>
        /// <returns></returns>
        private async Task<List<SismaCaseStatisticsVM>> SismaCase_Select(int courtTypeId, int courtId, int[] caseGroupIds,
                    DateTime fromDate, DateTime toDate, string sismaIndex, int reportType)
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
                reportTypeWhere = x => x.RegDate.Date >= fromDate.Date && x.RegDate.Date <= toDate.Date && (x.IsRenewCase ?? false) == true;
            }

            var result = (await repo.AllReadonly<Case>()
                                .Where(GeneralCaseWhere())
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Court.Code,
                                    SismaIndex = sismaIndex,
                                })
                                .ToListAsync().ConfigureAwait(false))
                                .GroupBy(x => new
                                {
                                    x.CourtCode,
                                    x.SismaIndex,
                                })
                                .Select(x => new SismaCaseStatisticsVM
                                {
                                    CourtCode = x.Key.CourtCode,
                                    SismaIndex = x.Key.SismaIndex,
                                    Count = x.Count(),
                                })
                                .ToList();

            return result;
        }

        public async Task<SismaModel> GetSismaData(int reportMonth, int reportYear, int sheetIndex, string reportType)
        {
            //К.Борисов - данните се вземат до края на текущия месец
            DateTime toDate = new DateTime(reportYear, reportMonth, 1).AddMonths(1).AddDays(-1);
            DateTime fromDate = new DateTime(toDate.Year, 1, 1);

            var data = await SismaFillData(fromDate, toDate, 0, sheetIndex).ConfigureAwait(false);
            SismaModel model = new SismaModel()
            {
                Context = new SismaContextModel()
                {
                    IntegrationType = "1",
                    MethodName = EpepConstants.Methods.Add,
                    PeriodNumber = toDate.Month,
                    PeriodYear = toDate.Year,
                    ReportType = reportType,
                },
                Codes = data.Where(x => string.IsNullOrEmpty(x.SismaIndex) == false).GroupBy(x => x.SismaIndex)
                              .Select(x => new SismaCodeModel()
                              {
                                  IbdCode = x.Key,
                                  Details = x.Select(a => new SismaCodeDetailModel()
                                  {
                                      EntityCode = a.CourtCode,
                                      Count = a.Count,
                                      SubjectCode = a.SubjectCode,
                                      SubjectName = a.SubjectName
                                  }).ToArray(),
                              })
                              .ToArray()
            };

            return model;
        }

        private List<SismaCaseStatisticsVM> MappingExcelSisma(List<ExcelReportData> excelData, List<ExcelSismaMapping> excelSismaMappings, List<Court> courts)
        {
            return excelData.Where(x => excelSismaMappings.Where(m => m.RowIndex == x.RowIndex && m.ColIndex == x.ColIndex && m.SheetIndex == x.SheetIndex).Any())
                          .Select(x => new SismaCaseStatisticsVM()
                          {
                              CourtCode = courts.Where(c => c.Id == x.CourtId).Select(c => c.Code).FirstOrDefault(),
                              SismaIndex = excelSismaMappings.Where(m => m.RowIndex == x.RowIndex && m.ColIndex == x.ColIndex && m.SheetIndex == x.SheetIndex).Select(m => m.SismaIndex).FirstOrDefault(),
                              Count = x.CellValueInt ?? 0,
                          }).ToList();
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaSheet16(DateTime fromDate, DateTime toDate, StatisticsNomDataVM nomData, int searchCourtId)
        {
            List<SismaCaseStatisticsVM> result = new List<SismaCaseStatisticsVM>();

            //Sheet 1 Приложение 1
            var dataRS = await RSSheet1(fromDate, toDate, searchCourtId, 1,
                                nomData.excelReportCaseTypeRows.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt && x.SheetIndex == 0).ToList()).ConfigureAwait(false);

            result.AddRange(MappingExcelSisma(dataRS, nomData.excelSismaMappings.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt).ToList(), nomData.courts));

            //Sheet 1 Приложение 1
            var dataOs = await DistrictSheet1(fromDate, toDate, searchCourtId, 1,
                nomData.excelReportCaseTypeRows.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt && x.SheetIndex == 0).ToList(),
                nomData.excelReportIspnReasons.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt && x.SheetIndex == 1).ToList()).ConfigureAwait(false);

            result.AddRange(MappingExcelSisma(dataOs, nomData.excelSismaMappings.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt).ToList(), nomData.courts));

            //Sheet 1 Приложение 1
            var dataApeal = await ApealSheet1(fromDate, toDate, searchCourtId, 1,
                          nomData.excelReportCaseTypeRows.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal && x.SheetIndex == 0).ToList(), NomenclatureConstants.CourtType.Apeal).ConfigureAwait(false);

            result.AddRange(MappingExcelSisma(dataApeal, nomData.excelSismaMappings.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal).ToList(), nomData.courts));

            //Sheet 1 Приложение 1
            var dataMillitary = await MillitarySheet1(fromDate, toDate, searchCourtId, 1,
                          nomData.excelReportCaseTypeRows.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Millitary && x.SheetIndex == 0).ToList()).ConfigureAwait(false);

            result.AddRange(MappingExcelSisma(dataMillitary, nomData.excelSismaMappings.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Millitary).ToList(), nomData.courts));

            //Sheet 1 Приложение 1
            var dataMillitaryApeal = await ApealSheet1(fromDate, toDate, searchCourtId, 1,
                          nomData.excelReportCaseTypeRows.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.MillitaryApeal && x.SheetIndex == 0).ToList(), NomenclatureConstants.CourtType.MillitaryApeal).ConfigureAwait(false);

            result.AddRange(MappingExcelSisma(dataMillitaryApeal, nomData.excelSismaMappings.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.MillitaryApeal).ToList(), nomData.courts));

            return result;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaAP_Sheets(DateTime fromDate, DateTime toDate, StatisticsNomDataVM nomData, int searchCourtId, int sheetIndex)
        {
            List<SismaCaseStatisticsVM> result = new List<SismaCaseStatisticsVM>();

            int courtTypeId = NomenclatureConstants.CourtType.Apeal;

            List<StatisticsExcelReportIndexVM> excelReportIndexCols = nomData.excelReportIndexCols
                          .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults = nomData.excelReportComplainResults
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeColVM> excelReportCaseTypeCols = nomData.excelReportCaseTypeCols
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            if (sheetIndex == 18 || sheetIndex == 0)
                result.AddRange(await SismaSheetCaseCount(fromDate, toDate, searchCourtId, new int[]
              { NomenclatureConstants.CaseGroups.NakazatelnoDelo }, courtTypeId, excelReportCaseTypeCols).ConfigureAwait(false));

            if (sheetIndex == 19 || sheetIndex == 0)
                result.AddRange(await SismaSheetCaseCount(fromDate, toDate, searchCourtId,
                    new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade }, courtTypeId, excelReportCaseTypeCols).ConfigureAwait(false));

            if (sheetIndex == 17 || sheetIndex == 0)
                result.AddRange(await SismaApealSheet17(fromDate, toDate, searchCourtId, excelReportComplainResults.Where(x => x.SheetIndex == 2).ToList()).ConfigureAwait(false));

            if (sheetIndex == 20 || sheetIndex == 0)
                result.AddRange(await SismaApealSheet20(fromDate, toDate, searchCourtId, excelReportComplainResults.Where(x => x.SheetIndex == 3).ToList()).ConfigureAwait(false));

            return result;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaApealSheet17(DateTime fromDate, DateTime toDate, int searchCourtId,
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

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "117002", 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "117003", 2, complains, null).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "", 4, complains, excelReportComplainResults).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "117010", 3, complains, null).ConfigureAwait(false));

            //Колони за Частни Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "117013", 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await SismaCaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "117014", 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "117015", 4, complains, null).ConfigureAwait(false));

            //Колони за Жалби по бавност
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsRequestSlownessTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await SismaCaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "117017", 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "", 4, complains, excelReportComplainResults).ConfigureAwait(false));

            //Колони за Жалби по 274
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaint274TDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "117020", 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await SismaCaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "117021", 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "117022", 4, complains, null).ConfigureAwait(false));

            return allData.Where(x => string.IsNullOrEmpty(x.CodeData) == false).ToList();
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaApealSheet20(DateTime fromDate, DateTime toDate, int searchCourtId,
            List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.Apeal;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND,
                           NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND,
            NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND,
            NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND};

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби и протести
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND ||
                    x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120002", 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120005", 2, complains, null).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "", 4, complains, excelReportComplainResults).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120017", 3, complains, null).ConfigureAwait(false));

            //Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120003", 2, complains, null).ConfigureAwait(false));

            //Протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120004", 2, complains, null).ConfigureAwait(false));


            //Колони за Частни Жалби и протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120020", 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await SismaCaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120021", 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120022", 4, complains, null).ConfigureAwait(false));

            //Колони за Възобновяване
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120024", 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await SismaCaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120025", 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await SismaCaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, "120026", 4, complains, null).ConfigureAwait(false));


            return allData.Where(x => string.IsNullOrEmpty(x.CodeData) == false).ToList();
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaMillitary_Sheets(DateTime fromDate, DateTime toDate, StatisticsNomDataVM nomData, int searchCourtId, int sheetIndex)
        {
            List<SismaCaseStatisticsVM> result = new List<SismaCaseStatisticsVM>();

            int courtTypeId = NomenclatureConstants.CourtType.Millitary;

            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows = nomData.excelReportCaseCodeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows = nomData.excelReportCaseTypeRows
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            List<StatisticsExcelReportCaseTypeColVM> excelReportCaseTypeCols = nomData.excelReportCaseTypeCols
                                              .Where(x => x.CourtTypeId == courtTypeId).ToList();

            if (sheetIndex == 21 || sheetIndex == 0)
                result.AddRange(await SismaSheetCaseCount(fromDate, toDate, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo },
                courtTypeId, excelReportCaseTypeCols).ConfigureAwait(false));


            if (sheetIndex == 22 || sheetIndex == 0)
                result.AddRange(await SismaMillitarySheet22(fromDate, toDate, searchCourtId, excelReportCaseCodeRows.Where(x => x.SheetIndex == 2).ToList()).ConfigureAwait(false));


            return result;
        }

        private async Task<List<SismaCaseStatisticsVM>> SismaMillitarySheet22(DateTime fromDate, DateTime toDate, int searchCourtId,
                            List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows)
        {
            List<SismaCaseStatisticsVM> allData = new List<SismaCaseStatisticsVM>();

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

                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "122002", 1, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "122003", 6, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "122004", 7, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "122005", 1, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "122007", 6, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "122008", 7, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "122009", 8, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "122011", 2, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, "122013", 5, instanceId, null, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122014", 1, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122015", 2, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122016", 3, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122017", 4, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122018", 5, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122019", 11, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122020", 12, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122021", 13, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122022", 14, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122023", 15, instanceId, caseCodes).ConfigureAwait(false));
                allData.AddRange(await SismaCasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, "122024", 10, instanceId, caseCodes).ConfigureAwait(false));
            }

            return allData;
        }

        private async Task<List<ExcelReportData>> OSSheet4Request7(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 8, 9, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 10, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 12, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonSentenceCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 16, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonSentenceCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 17, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 18, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 19, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 20, 16, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 21, 17, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 23, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 25, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 26, 11, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 27, 12, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 28, 13, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 29, 14, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 30, 15, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 31, 10, instanceId).ConfigureAwait(false));
            }

            allData.AddRange(await Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 127, 2, instanceId).ConfigureAwait(false));

            //Справка 1
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 132, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 133, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 134, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 135, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 136, 12, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 137, 13, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 138, 14, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 139, 1, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 140, 3, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 141, 1, instanceId).ConfigureAwait(false));

            //Справка 2
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 147, 4, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 148, 5, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 149, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 150, 7, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 151, 12, instanceId).ConfigureAwait(false));

            //Справка 3
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 157, 9, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 159, 10, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 160, 11, instanceId).ConfigureAwait(false));

            //Справка 4
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 165, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 166, 7, instanceId).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    4, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> RSSheet3Request7(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 6, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 7, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 12, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 14, 9, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 10, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 16, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 17, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 19, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 20, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 21, 16, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 23, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 25, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 26, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 27, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 28, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 29, 9, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 30, 10, instanceId).ConfigureAwait(false));
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId).ConfigureAwait(false));

                allData.AddRange(await CaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes,
                      excelReportComplainResults, instanceId).ConfigureAwait(false));
            }

            //Справка 3
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 135, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 136, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 137, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 138, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 139, 12, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 140, 13, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 141, 14, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 142, 1, instanceId).ConfigureAwait(false));

            //Справка 4
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 147, 4, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 148, 5, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 149, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 150, 7, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 151, 8, instanceId).ConfigureAwait(false));

            //Справка 6
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 161, 9, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 163, 10, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 164, 11, instanceId).ConfigureAwait(false));

            //Справка 7
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 169, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 170, 7, instanceId).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count));
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
        /// <param name="rowIndex"></param>
        /// <param name="reportType">1 - Открити производства, 2 - Висящи производства, 3 - Приключени производства, 4 - отхвърлени с акт по същество, 5 - Прекратени,
        /// </param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CaseIspn_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId, string caseCode)
        {
            string[] caseCode21 = new string[] { "21110-1", "21111-1" };
            string[] caseCode24 = new string[] { "24100-1", "24111-1" };
            int[] deginitions = new int[] { NomenclatureConstants.ActType.Definition, NomenclatureConstants.ActType.Protokol,
                NomenclatureConstants.ActType.ProtokolOpredelenie };
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

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
            if (reportType == 1 && caseCode21.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspn21).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd && 
                              x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(x.ActISPNReasonId)).Any();
            }
            else if (reportType == 1 && caseCode24.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspn24).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd &&
                              deginitions.Contains(x.ActTypeId) && ispnReasons.Contains(x.ActISPNReasonId)).Any();
            }
            else if (reportType == 2 && caseCode21.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspn21).ConfigureAwait(false);
                var ispnFinishReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspnFinish21).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate <= toDateEnd && 
                              x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(x.ActISPNReasonId)).Any() &&
                              a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate <= toDateEnd &&
                              x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnFinishReasons.Contains(x.ActISPNReasonId)).Any() == false;
            }
            else if (reportType == 2 && caseCode24.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspn24).ConfigureAwait(false);
                var ispnFinishReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspnFinish24).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate <= toDateEnd &&
                              deginitions.Contains(x.ActTypeId) && ispnReasons.Contains(x.ActISPNReasonId)).Any() &&
                              a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate <= toDateEnd &&
                              deginitions.Contains(x.ActTypeId) && ispnFinishReasons.Contains(x.ActISPNReasonId)).Any() == false;
            }
            else if (reportType == 3 && caseCode21.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspn21).ConfigureAwait(false);
                var ispnFinishReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspnFinish21).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate <= toDateEnd && 
                              x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(x.ActISPNReasonId)).Any() &&
                              a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd &&
                              x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnFinishReasons.Contains(x.ActISPNReasonId)).Any();
            }
            else if (reportType == 3 && caseCode24.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspn24).ConfigureAwait(false);
                var ispnFinishReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspnFinish24).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate <= toDateEnd &&
                              deginitions.Contains(x.ActTypeId) && ispnReasons.Contains(x.ActISPNReasonId)).Any() &&
                              a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd &&
                              deginitions.Contains(x.ActTypeId) && ispnFinishReasons.Contains(x.ActISPNReasonId)).Any();
            }
            else if (reportType == 4 && caseCode21.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspnRejected21).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd && 
                              ispnReasons.Contains(x.ActISPNReasonId)).Any();
            }
            else if (reportType == 4 && caseCode24.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspnRejected24).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd && 
                              ispnReasons.Contains(x.ActISPNReasonId)).Any();
            }
            else if (reportType == 5 && caseCode21.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspn21).ConfigureAwait(false);
                var sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsIspnTerminate).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate <= toDateEnd && 
                              x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(x.ActISPNReasonId)).Any() == false &&
                              a.CaseSessionResults.Where(x => x.IsActive == true && x.DateExpired == null && x.SessionResult.IsActive == true && sessionResults.Contains(x.SessionResultId)).Any();
            }
            else if (reportType == 5 && caseCode24.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspn24).ConfigureAwait(false);
                var sessionResults = await SessionResultGrouping_Select(NomenclatureConstants.SessionResultGroupings.StatisticsIspnTerminate).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate <= toDateEnd &&
                              deginitions.Contains(x.ActTypeId) && ispnReasons.Contains(x.ActISPNReasonId)).Any() == false &&
                              a.CaseSessionResults.Where(x => x.IsActive == true && x.DateExpired == null && x.SessionResult.IsActive == true && sessionResults.Contains(x.SessionResultId)).Any();
            }

            var result = (await repo.AllReadonly<Case>()
                                .Where(GeneralCaseWhere())
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.CaseCode.Code == caseCode)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                })
                                .ToListAsync().ConfigureAwait(false))
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
        /// <param name="reportType">1 - Подадени молби,
        /// </param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CaseDocumentIspn_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId, string caseCode)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

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
                reportTypeWhere = a => a.Document.DocumentDate >= fromDateStart && a.Document.DocumentDate <= toDateEnd && a.Document.DateExpired == null;
            }

            var result = (await repo.AllReadonly<Case>()
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.CaseCode.Code == caseCode)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                })
                                .ToListAsync().ConfigureAwait(false))
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
        /// <param name="reportType">1 - Брой длъжници,
        /// </param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CasePersonIspn_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId, string caseCode)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

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
                var personRoles = await repo.AllReadonly<PersonRoleGrouping>()
                                      .Where(x => x.PersonRoleGroup == NomenclatureConstants.PersonRoleGroupings.StatisticsIspnDebtor)
                                      .Select(x => x.PersonRoleId)
                                      .ToArrayAsync().ConfigureAwait(false);

                reportTypeWhere = a => a.DateFrom >= fromDateStart && a.DateFrom <= toDateEnd && personRoles.Contains(a.PersonRoleId);
            }

            var result = (await repo.AllReadonly<CasePerson>()
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.Case.CaseCode.Code == caseCode)
                                .Where(x => x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft &&
                                                                      x.DateExpired == null && x.CaseSessionId == null &&
                                                                      x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false)
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
                                .ToListAsync().ConfigureAwait(false))
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
        /// <param name="reportType">1 - Подадени молби</param>
        /// <param name="instanceId"></param>
        /// <param name="documentTypeId"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> DocumentRequest760_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId, int documentTypeId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

            Expression<Func<Document, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<Document, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<Document, bool>> reportTypeWhere = x => true;
            if (reportType == 1)
            {
                reportTypeWhere = a => a.DocumentDate >= fromDateStart && a.DocumentDate <= toDateEnd;
            }

            var result = (await repo.AllReadonly<Document>()
                                .Where(x => x.DocumentTypeId == documentTypeId)
                                .Where(x => x.DateExpired == null)
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                })
                                .ToListAsync().ConfigureAwait(false))
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
        /// <param name="reportType">1 - Висящи, 2 - Приключени, 3 - Брой решения за опрощаване, 4 - Брой решения за отхвърляне </param>
        /// <param name="instanceId"></param>
        /// <param name="documentTypeId"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CaseRequest760_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int reportType, int instanceId, int documentTypeId)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

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
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsRequest760Finish).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate <= toDateEnd &&
                              x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(x.ActISPNReasonId)).Any() == false;
            }
            else if (reportType == 2)
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsRequest760Finish).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd &&
                              x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(x.ActISPNReasonId)).Any();
            }
            else if (reportType == 3)
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsRequest760Forgive).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd &&
                              x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(x.ActISPNReasonId)).Any();
            }
            else if (reportType == 4)
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsRequest760Reject).ConfigureAwait(false);

                reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd &&
                              x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(x.ActISPNReasonId)).Any();
            }

            var result = (await repo.AllReadonly<Case>()
                                .Where(GeneralCaseWhere())
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.DocumentCaseInfos.Where(a => a.Document.DocumentTypeId == documentTypeId).Any())
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                })
                                .ToListAsync().ConfigureAwait(false))
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
        /// <param name="instanceId"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CaseDebtorIspn_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int instanceId, string caseCode)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

            Expression<Func<Case, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (caseGroupIds.Length > 0)
                caseGroupWhere = x => caseGroupIds.Contains(x.CaseGroupId);

            Expression<Func<Case, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;            

            var result = (await repo.AllReadonly<Case>()
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.CaseCode.Code == caseCode)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(x => x.RegDate >= fromDateStart && x.RegDate <= toDateEnd)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId,
                                    ExcelRow = rowIndex,
                                    ExcelCol = colIndex,
                                    Count = x.DebtorsCount ?? 0,
                                })
                                .ToListAsync().ConfigureAwait(false))
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
                                    Count = x.Select(a => a.Count).Sum(),
                                })
                                .ToList();

            return result;
        }

        private async Task<int?[]> ActIspnReasonGrouping_Select(int groupId)
        {
            return await repo.AllReadonly<ActISPNReasonGrouping>()
                                      .Where(x => x.ActISPNReasonGroup == groupId)
                                      .Select(x => (int?)x.ActISPNReasonId)
                                      .ToArrayAsync().ConfigureAwait(false);
        }


        private async Task<List<CaseStatisticsVM>> CaseIspnDurationFinishAct_Select(int courtTypeId, int courtId, int[] caseGroupIds,
                        DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int instanceId, string caseCode, int startRow, int getRow)
        {
            List<CaseStatisticsVM> result = new List<CaseStatisticsVM>();

            string[] caseCode21 = new string[] { "21110-1", "21111-1" };
            string[] caseCode24 = new string[] { "24100-1", "24111-1" };
            int[] deginitions = new int[] { NomenclatureConstants.ActType.Definition, NomenclatureConstants.ActType.Protokol,
                NomenclatureConstants.ActType.ProtokolOpredelenie };

            DateTime dateNow = DateTime.Now;
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

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
            if (caseCode21.Contains(caseCode))
            {
                var ispnFinishReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspnFinish21).ConfigureAwait(false);

                reportTypeWhere = x => x.ActDeclaredDate != null && x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnFinishReasons.Contains(x.ActISPNReasonId);
            }
            else if (caseCode24.Contains(caseCode))
            {
                var ispnFinishReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspnFinish24).ConfigureAwait(false);

                reportTypeWhere = x => deginitions.Contains(x.ActTypeId) && ispnFinishReasons.Contains(x.ActISPNReasonId);
            }

            //Твърди се, че има един акт за старт и един за край
            var finish = await repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.Case.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.Case.CaseCode.Code == caseCode)
                                .Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd)
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseIspnDuration()
                                {
                                    CaseSessionActId = x.Id,
                                    CourtId = x.CourtId ?? 0,
                                    CaseId = x.CaseId,
                                    ActDeclaredDate = x.ActDeclaredDate ?? dateNow,
                                })
                               .OrderBy(x => x.CaseId).ThenBy(x => x.CaseSessionActId)
                               .Skip(startRow)
                               .Take(getRow)
                               .ToListAsync();

            if (finish.Count > 0)
            {
                var caseIds = finish.Select(x => x.CaseId).ToArray();
                var start = await CaseIspnDurationStartAct_Select(caseIds, caseCode);

                foreach (var itemFinish in finish)
                {
                    var itemStart = start.Where(x => x.CaseId == itemFinish.CaseId).FirstOrDefault();
                    if (itemStart != null && itemStart.ActDeclaredDate.Date < itemFinish.ActDeclaredDate.Date)
                    {
                        result.Add(new CaseStatisticsVM()
                        {
                            CourtId = itemFinish.CourtId,
                            ExcelRow = rowIndex,
                            ExcelCol = colIndex,
                            Count = GetMonthsBetweenTwoDates(itemStart.ActDeclaredDate, itemFinish.ActDeclaredDate),
                        });
                    }
                }
            }

            return result;
        }

        private async Task<List<CaseIspnDuration>> CaseIspnDurationStartAct_Select(int?[] caseIds, string caseCode)
        {
            string[] caseCode21 = new string[] { "21110-1", "21111-1" };
            string[] caseCode24 = new string[] { "24100-1", "24111-1" };
            int[] deginitions = new int[] { NomenclatureConstants.ActType.Definition, NomenclatureConstants.ActType.Protokol,
                NomenclatureConstants.ActType.ProtokolOpredelenie };

            DateTime dateNow = DateTime.Now;

            Expression<Func<CaseSessionAct, bool>> reportTypeWhere = x => true;
            if (caseCode21.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspn21).ConfigureAwait(false);

                reportTypeWhere = x => x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(x.ActISPNReasonId);
            }
            else if (caseCode24.Contains(caseCode))
            {
                var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsIspn24).ConfigureAwait(false);

                reportTypeWhere = x => deginitions.Contains(x.ActTypeId) && ispnReasons.Contains(x.ActISPNReasonId);
            }

            var result = await repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.DateExpired == null && x.ActDeclaredDate != null && caseIds.Contains(x.CaseId))
                                .Where(reportTypeWhere)
                                .Select(x => new CaseIspnDuration()
                                {
                                    CourtId = x.CourtId ?? 0,
                                    CaseId = x.CaseId ?? 0,
                                    ActDeclaredDate = x.ActDeclaredDate ?? dateNow,
                                })
                                .ToListAsync();

            return result;
        }

        private async Task<List<CaseStatisticsVM>> CalcCaseIspnDurationFinishAct_Select(int courtTypeId, int courtId, int[] caseGroupIds,
                        DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int instanceId, string caseCode)
        {
            List<CaseStatisticsVM> dataRows = new List<CaseStatisticsVM>();
            List<CaseStatisticsVM> getRows = new List<CaseStatisticsVM>();

            int start = 0;
            int take = 100;
            getRows = await CaseIspnDurationFinishAct_Select(courtTypeId, courtId, caseGroupIds, fromDate, toDate, colIndex, rowIndex, instanceId, caseCode, start, take).ConfigureAwait(false);
            dataRows.AddRange(getRows);

            while (getRows.Any())
            {
                start = start + take;
                getRows = await CaseIspnDurationFinishAct_Select(courtTypeId, courtId, caseGroupIds, fromDate, toDate, colIndex, rowIndex, instanceId, caseCode, start, take).ConfigureAwait(false);
                dataRows.AddRange(getRows);
            }

            return dataRows.GroupBy(x => new
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
                Count = x.Sum(a => a.Count),
            })
            .ToList();
        }

        /// <summary>
        /// Метод връщащ месеци между 2 дати за статистиката. Тук е за да мога да си наглася точно както ми трябва за статистиката и да не се пипа за други неща
        /// </summary>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дата</param>
        /// <returns></returns>
        private int GetMonthsBetweenTwoDates(DateTime dateFrom, DateTime dateTo)
        {
            int monthsApart = (12 * (dateTo.Year - dateFrom.Year) + dateTo.Month - dateFrom.Month) + (dateTo.Day > dateFrom.Day ? 1 : 0);
            monthsApart = monthsApart == 0 ? 1 : monthsApart;
            return Math.Abs(monthsApart);
        }

        private async Task<List<CaseStatisticsVM>> CaseRequest760Duration_Select(int courtTypeId, int courtId, int[] caseGroupIds,
    DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int instanceId, int documentTypeId, int startRow, int getRow)
        {
            List<CaseStatisticsVM> result = new List<CaseStatisticsVM>();

            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

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
            var ispnReasons = await ActIspnReasonGrouping_Select(NomenclatureConstants.ActISPNReasonGroupings.StatisticsRequest760Finish).ConfigureAwait(false);

            reportTypeWhere = a => a.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActDeclaredDate >= fromDateStart && x.ActDeclaredDate <= toDateEnd &&
                          x.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(x.ActISPNReasonId)).Any();


            var cases = await repo.AllReadonly<Case>()
                                .Where(GeneralCaseWhere())
                                .Where(x => x.CaseType.CaseInstanceId == instanceId)
                                .Where(x => x.DocumentCaseInfos.Where(a => a.Document.DocumentTypeId == documentTypeId).Any())
                                .Where(courtWhere)
                                .Where(caseGroupWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Select(x => new CaseRequest760Duration
                                {
                                    CourtId = x.CourtId,
                                    CaseId = x.Id,
                                    StartDate = x.DocumentCaseInfos.Where(a => a.Document.DocumentTypeId == documentTypeId).Select(a => a.Document.DocumentDate).Min(),
                                    EndDate = x.CaseSessionActs.Where(a => a.DateExpired == null && a.ActDeclaredDate != null && a.ActDeclaredDate >= fromDateStart && a.ActDeclaredDate <= toDateEnd &&
                          a.ActTypeId == NomenclatureConstants.ActType.Answer && ispnReasons.Contains(a.ActISPNReasonId)).Select(a => (DateTime)a.ActDeclaredDate).Max(),
                                })
                               .OrderBy(x => x.CaseId)
                               .Skip(startRow)
                               .Take(getRow)
                               .ToListAsync();

            if (cases.Count() > 0)
            {
                foreach (var item in cases)
                {
                    if (item.StartDate.Date <= item.EndDate.Date)
                    {
                        result.Add(new CaseStatisticsVM()
                        {
                            CourtId = item.CourtId,
                            ExcelRow = rowIndex,
                            ExcelCol = colIndex,
                            Count = GetMonthsBetweenTwoDates(item.StartDate, item.EndDate),
                        });
                    }
                }
            }

            return result;
        }

        private async Task<List<CaseStatisticsVM>> CalcCaseRequest760Duration_Select(int courtTypeId, int courtId, int[] caseGroupIds,
                        DateTime fromDate, DateTime toDate, int colIndex, int rowIndex, int instanceId, int documentTypeId)
        {
            List<CaseStatisticsVM> dataRows = new List<CaseStatisticsVM>();
            List<CaseStatisticsVM> getRows = new List<CaseStatisticsVM>();

            int start = 0;
            int take = 100;
            getRows = await CaseRequest760Duration_Select(courtTypeId, courtId, caseGroupIds, fromDate, toDate, colIndex, rowIndex, instanceId, documentTypeId, start, take).ConfigureAwait(false);
            dataRows.AddRange(getRows);

            while (getRows.Any())
            {
                start = start + take;
                getRows = await CaseRequest760Duration_Select(courtTypeId, courtId, caseGroupIds, fromDate, toDate, colIndex, rowIndex, instanceId, documentTypeId, start, take).ConfigureAwait(false);
                dataRows.AddRange(getRows);
            }

            return dataRows.GroupBy(x => new
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
                Count = x.Sum(a => a.Count),
            })
            .ToList();
        }

        private async Task<List<ExcelReportData>> ApealExcelTitle_Mediation(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId, List<EkEkatte> ekattes)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = await repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToListAsync().ConfigureAwait(false);
            string month = toDate.Month.ToString();
            string year = toDate.Year.ToString();
            int intMonth = toDate.Month;
            int intYear = toDate.Year;
            foreach (var item in courts)
            {
                var city = string.IsNullOrEmpty(item.CityName) == false ? item.CityName : ekattes.Where(x => x.Ekatte == item.CityCode).Select(x => x.Name).DefaultIfEmpty("").FirstOrDefault();

                //Sheet1
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    0, 3, 12, "за периода от " + fromDate.ToString("dd.MM.yyyy")));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    0, 3, 15, "до " + toDate.ToString("dd.MM.yyyy")));

                //Sheet2
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 10, city));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                   1, 1, 22, city));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 30, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 33, "месеца на " + year + "г."));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> OSExcelTitle_Mediation(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId, List<EkEkatte> ekattes)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = await repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToListAsync().ConfigureAwait(false);
            string month = toDate.Month.ToString();
            string year = toDate.Year.ToString();
            int intMonth = toDate.Month;
            int intYear = toDate.Year;
            foreach (var item in courts)
            {
                var city = string.IsNullOrEmpty(item.CityName) == false ? item.CityName : ekattes.Where(x => x.Ekatte == item.CityCode).Select(x => x.Name).DefaultIfEmpty("").FirstOrDefault();

                //Sheet1
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    0, 1, 1, "Справка по съдии от Окръжен съд град " + city + " за препратените дела за процедура по медиация"));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    0, 3, 12, "за периода от " + fromDate.ToString("dd.MM.yyyy")));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    0, 3, 15, "до " + toDate.ToString("dd.MM.yyyy")));

                //Sheet2
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                   1, 1, 9, city));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                   1, 1, 25, city));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 29, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 32, "месеца на " + year + "г."));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> RSExcelTitle_Mediation(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId, List<EkEkatte> ekattes)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();
            var courts = await repo.AllReadonly<Court>()
                          .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                          .Where(x => x.IsActive)
                          .Where(x => (searchCourtId == 0 || x.Id == searchCourtId))
                          .ToListAsync().ConfigureAwait(false);
            string month = toDate.Month.ToString();
            string year = toDate.Year.ToString();
            int intMonth = toDate.Month;
            int intYear = toDate.Year;
            foreach (var item in courts)
            {
                var city = string.IsNullOrEmpty(item.CityName) == false ? item.CityName : ekattes.Where(x => x.Ekatte == item.CityCode).Select(x => x.Name).DefaultIfEmpty("").FirstOrDefault();

                //Sheet1
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    0, 0, 1, "Справка по съдии от Районен съд град " + city + " за препратените дела за процедура по медиация"));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                   0, 2, 12, "за периода от " + fromDate.ToString("dd.MM.yyyy")));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    0, 2, 15, "до " + toDate.ToString("dd.MM.yyyy")));

                //Sheet2
                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                   1, 1, 11, city));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 31, month));

                result.Add(InsertExcelReportDataString(item.Id, templateId, intYear, intMonth,
                    1, 1, 34, "месеца на " + year + "г."));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> RS_Sheets_Mediation(DateTime fromDate, DateTime toDate, int templateId, int searchCourtId, List<EkEkatte> ekattes)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = NomenclatureConstants.CourtType.RegionalCourt;

            result.AddRange(await RSExcelTitle_Mediation(fromDate, toDate, searchCourtId, templateId, ekattes).ConfigureAwait(false));

            //Sheet0
            result.AddRange(await RSSheet0Mediation(courtTypeId, fromDate, toDate, searchCourtId, templateId).ConfigureAwait(false));

            //Sheet1
            result.AddRange(await RSSheet1Mediation(courtTypeId, fromDate, toDate, searchCourtId, templateId).ConfigureAwait(false));

            return result;
        }

        private async Task<List<ExcelReportData>> OS_Sheets_Mediation(DateTime fromDate, DateTime toDate, int templateId, int searchCourtId, List<EkEkatte> ekattes)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;

            result.AddRange(await OSExcelTitle_Mediation(fromDate, toDate, searchCourtId, templateId, ekattes).ConfigureAwait(false));

            //Sheet0
            result.AddRange(await OSSheet0Mediation(courtTypeId, fromDate, toDate, searchCourtId, templateId).ConfigureAwait(false));

            //Sheet1
            result.AddRange(await RSSheet1Mediation(courtTypeId, fromDate, toDate, searchCourtId, templateId).ConfigureAwait(false));

            return result;
        }

        private async Task<List<ExcelReportData>> AP_Sheets_Mediation(DateTime fromDate, DateTime toDate, int templateId, int searchCourtId, List<EkEkatte> ekattes)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = NomenclatureConstants.CourtType.Apeal;
           
            result.AddRange(await ApealExcelTitle_Mediation(fromDate, toDate, searchCourtId, templateId, ekattes).ConfigureAwait(false));

            //Sheet0
            result.AddRange(await ApealSheet0Mediation(courtTypeId, fromDate, toDate, searchCourtId, templateId).ConfigureAwait(false));

            //Sheet1
            result.AddRange(await RSSheet1Mediation(courtTypeId, fromDate, toDate, searchCourtId, templateId).ConfigureAwait(false));

            return result;
        }

        public async Task<List<ExcelReportData>> FillExcelData_Mediation(DateTime fromDate, DateTime toDate, int courtId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int courtTypeId = 0;
            if (courtId > 0)
            {
                courtTypeId = await repo.AllReadonly<Court>()
                               .Where(x => x.Id == courtId)
                               .Select(x => x.CourtTypeId)
                               .FirstOrDefaultAsync().ConfigureAwait(false);
            }

            var reportTemplates = await repo.AllReadonly<ExcelReportTemplate>()
                     .Where(x => x.DateFrom <= toDate && (x.DateTo ?? DateTime.MaxValue).Date >= toDate.Date && x.ReportTypeId == NomenclatureConstants.ExcelReportTemplateReportTypes.Mediation)
                     .ToListAsync().ConfigureAwait(false);

            List<EkEkatte> ekattes = await repo.AllReadonly<EkEkatte>().ToListAsync().ConfigureAwait(false);

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
            {
                result.AddRange(await RS_Sheets_Mediation(fromDate, toDate,
                    reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                    .Select(x => x.Id).FirstOrDefault(), courtId, ekattes).ConfigureAwait(false));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
            {
                result.AddRange(await OS_Sheets_Mediation(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
                .Select(x => x.Id).FirstOrDefault(), courtId, ekattes).ConfigureAwait(false));
            }

            if (courtTypeId == 0 || courtTypeId == NomenclatureConstants.CourtType.Apeal)
            {
                result.AddRange(await AP_Sheets_Mediation(fromDate, toDate,
                reportTemplates.Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.Apeal)
                .Select(x => x.Id).FirstOrDefault(), courtId, ekattes).ConfigureAwait(false));
            }            

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="reportType">1 - Брой препратени дела за процедура по медиация, 2 - постигнати споразумения или прекратени дела след процедури по медиация при препращане от всеки съдия</param>
        /// <param name="colIndex"></param>
        /// <param name="caseCodeSub"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> CaseLawUnitMediation_Select(int courtTypeId, int courtId, DateTime fromDate, DateTime toDate, int reportType, int colIndex, string caseCodeSub)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

            Expression<Func<CaseLifecycle, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<CaseLifecycle, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<CaseLifecycle, bool>> caseCodeSubWhere = x => true;
            if (string.IsNullOrEmpty(caseCodeSub) == false)
                caseCodeSubWhere = x => x.Case.CaseCodeSub.Code == caseCodeSub;

            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => true;

            if (reportType == 1)
            {
                reportTypeWhere = x => x.DateFrom <= toDateEnd && (x.DateTo ?? dateEnd) >= fromDateStart;
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.DateTo != null && (x.DateTo ?? dateEnd) >= fromDateStart && (x.DateTo ?? dateEnd) <= toDateEnd;
            }

            var result = await repo.AllReadonly<CaseLifecycle>()
                                .Where(GeneralLifecycleMediation())
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Where(caseCodeSubWhere)
                                .Select(x => new CaseStatisticsVM
                                {
                                    CourtId = x.CourtId ?? 0,
                                    ExcelCol = colIndex,
                                    LawUnitData = x.Case.CaseLawUnits
                                              .Where(a => a.CaseSessionId == null)
                                              .Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                              .Where(a => (a.DateTo ?? dateEnd) >= fromDate)
                                              .Where(a => a.DateFrom <= toDateEnd)
                                              .OrderByDescending(a => a.DateFrom)
                                              .Select(a => a.LawUnitId + ",," + a.LawUnit.FullName)
                                              .FirstOrDefault()
                                })
                                .ToListAsync().ConfigureAwait(false);

            result = result.GroupBy(x => new
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

        private Expression<Func<CaseLifecycle, bool>> GeneralLifecycleMediation()
        {
            Expression<Func<CaseLifecycle, bool>> reportTypeWhere = x => x.DateExpired == null && x.LifecycleTypeId == NomenclatureConstants.LifecycleType.Mediation &&
                                                                         x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false;
            return reportTypeWhere;
        }

        private async Task<List<ExcelReportData>> RSSheet0Mediation(int courtTypeId, DateTime fromDate, DateTime toDate, int searchCourtId, int templateId)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 4, "М140-1").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 5, "М140-1").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 6, "М140-2").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 7, "М140-2").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 8, "М140-3").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 9, "М140-3").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 10, "М140-4-1").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 11, "М140-4-1").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 12, "М140-4-2").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 13, "М140-4-2").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 14, "М140-5").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 15, "М140-5").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 16, "М140-6").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 17, "М140-6").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 18, "М140-7").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 19, "М140-7").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 20, "М140-8").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 21, "М140-8").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 22, "М140-9").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 23, "М140-9").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 24, "М140-10").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 25, "М140-10").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 26, "М140-11").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 27, "М140-11").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 28, "М140-12").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 29, "М140-12").ConfigureAwait(false));

            return SaveExcelByJudgeMediation(toDate, 8, allData, templateId, 0);
        }

        private async Task<List<ExcelReportData>> OSSheet0Mediation(int courtTypeId, DateTime fromDate, DateTime toDate, int searchCourtId, int templateId)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 4, "М140-1").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 5, "М140-1").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 6, "М140-2").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 7, "М140-2").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 8, "М140-3").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 9, "М140-3").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 10, "М140-4-1").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 11, "М140-4-1").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 12, "М140-4-2").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 13, "М140-4-2").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 14, "М140-5").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 15, "М140-5").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 16, "М140-6").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 17, "М140-6").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 18, "М140-7").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 19, "М140-7").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 20, "М140-8").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 21, "М140-8").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 22, "М140-9").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 23, "М140-9").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 24, "М140-10").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 25, "М140-10").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 26, "М140-11").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 27, "М140-11").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 28, "М140-12").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 29, "М140-12").ConfigureAwait(false));

            return SaveExcelByJudgeMediation(toDate, 8, allData, templateId, 0);
        }

        private async Task<List<ExcelReportData>> ApealSheet0Mediation(int courtTypeId, DateTime fromDate, DateTime toDate, int searchCourtId, int templateId)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 4, "М140-1").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 5, "М140-1").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 6, "М140-2").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 7, "М140-2").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 8, "М140-3").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 9, "М140-3").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 10, "М140-4-1").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 11, "М140-4-1").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 12, "М140-4-2").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 13, "М140-4-2").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 14, "М140-5").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 15, "М140-5").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 16, "М140-6").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 17, "М140-6").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 18, "М140-7").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 19, "М140-7").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 20, "М140-8").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 21, "М140-8").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 22, "М140-9").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 23, "М140-9").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 24, "М140-10").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 25, "М140-10").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 26, "М140-11").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 27, "М140-11").ConfigureAwait(false));

            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 28, "М140-12").ConfigureAwait(false));
            allData.AddRange(await CaseLawUnitMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 29, "М140-12").ConfigureAwait(false));

            return SaveExcelByJudgeMediation(toDate, 8, allData, templateId, 0);
        }

        private Expression<Func<MediationCaseSession, bool>> GeneralCaseSessionMediation()
        {
            Expression<Func<MediationCaseSession, bool>> reportTypeWhere = x => x.DateExpired == null && x.Case.CaseDeactivations.Where(d => d.DateExpired == null).Any() == false;
            return reportTypeWhere;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="reportType"> 1 - Информационни срещи брой, 2 - Информационни срещи с поле Избор на медиатор, 3 - Процедури брой, 4 - Процедури с поле Избор на медиатор, 5 - Брой проведени срещи</param>
        /// <param name="colIndex"></param>
        /// <param name="caseCodeSub"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> MediatorMediation_Select(int courtTypeId, int courtId, DateTime fromDate, DateTime toDate, int reportType, int colIndex, string caseCodeSub)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

            Expression<Func<MediationCaseSession, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<MediationCaseSession, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<MediationCaseSession, bool>> caseCodeSubWhere = x => true;
            if (string.IsNullOrEmpty(caseCodeSub) == false)
                caseCodeSubWhere = x => x.Case.CaseCodeSub.Code == caseCodeSub;

            Expression<Func<MediationCaseSession, bool>> reportTypeWhere = x => true;

            if (reportType == 1 || reportType == 2)
            {
                reportTypeWhere = x => x.DateFrom >= fromDateStart && x.DateFrom <= toDateEnd && NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId) &&
                                        x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held;
            }
            else if (reportType == 3 || reportType == 4)
            {
                reportTypeWhere = x => x.DateFrom >= fromDateStart && x.DateFrom <= toDateEnd && NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) &&
                                        x.Results.Where(r => r.DateExpired == null && r.MediationResultId == NomenclatureConstants.MediationResultGroupConstants.Termination).Any();
            }
            else if (reportType == 5)
            {
                reportTypeWhere = x => x.DateFrom >= fromDateStart && x.DateFrom <= toDateEnd && NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) &&
                                        x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held &&
                                        x.Results.Where(r => r.DateExpired == null && r.MediationResultId == NomenclatureConstants.MediationResultGroupConstants.Termination).Any();
            }

            var result = await repo.AllReadonly<MediationCaseSession>()
                                .Where(GeneralCaseSessionMediation())
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Where(caseCodeSubWhere)
                                .SelectMany(x => x.Case.MediationCaseMediators
                                                    .Where(m => m.DateFrom <= (x.DateTo ?? dateEnd) && (m.DateTo ?? dateEnd) >= x.DateFrom)
                                                    .Where(m => ((reportType == 2 && m.MediationTypeChoiceMediatorId == NomenclatureConstants.MediationTypeChoiceMediatorConstants.ElectedPartyCase) || reportType != 2))
                                                    .Select(m => new CaseStatisticsVM
                                                    {
                                                        CourtId = x.CourtId ?? 0,
                                                        ExcelCol = colIndex,
                                                        LawUnitData = m.MediationMediatorId + ",," + m.Mediator.Name
                                                    }))
                                .ToListAsync().ConfigureAwait(false);

            result = result.GroupBy(x => new
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

        private async Task<List<ExcelReportData>> RSSheet1Mediation(int courtTypeId, DateTime fromDate, DateTime toDate, int searchCourtId, int templateId)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            //продължителност
            allData.AddRange(await MediatorMediationDuration_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 4).ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 5, "М140-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 6, "М140-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 7, "М140-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 8, "М140-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 9, "М140-3").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 10, "М140-3").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 11, "М140-4-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 12, "М140-4-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 13, "М140-4-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 14, "М140-4-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 15, "М140-5").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 16, "М140-5").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 17, "М140-6").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 18, "М140-6").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 19, "М140-7").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 20, "М140-7").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 21, "М140-8").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 22, "М140-8").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 23, "М140-9").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 24, "М140-9").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 25, "М140-10").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 26, "М140-10").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 27, "М140-11").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 28, "М140-11").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 29, "М140-12").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 30, "М140-12").ConfigureAwait(false));


            //Процедури

            //продължителност
            allData.AddRange(await MediatorMediationDuration_Select(courtTypeId, searchCourtId, fromDate, toDate, 5, 32).ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 5, 33, "").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 34, "М140-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 35, "М140-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 36, "М140-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 37, "М140-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 38, "М140-3").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 39, "М140-3").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 40, "М140-4-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 41, "М140-4-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 42, "М140-4-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 43, "М140-4-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 44, "М140-5").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 45, "М140-5").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 46, "М140-6").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 47, "М140-6").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 48, "М140-7").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 49, "М140-7").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 50, "М140-8").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 51, "М140-8").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 52, "М140-9").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 53, "М140-9").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 54, "М140-10").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 55, "М140-10").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 56, "М140-11").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 57, "М140-11").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 58, "М140-12").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 59, "М140-12").ConfigureAwait(false));

            return SaveExcelByJudgeMediation(toDate, 10, allData, templateId, 1);
        }

        private async Task<List<ExcelReportData>> OSSheet1Mediation(int courtTypeId, DateTime fromDate, DateTime toDate, int searchCourtId, int templateId)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            //продължителност
            allData.AddRange(await MediatorMediationDuration_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 4).ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 5, "М140-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 6, "М140-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 7, "М140-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 8, "М140-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 9, "М140-3").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 10, "М140-3").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 11, "М140-4-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 12, "М140-4-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 13, "М140-4-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 14, "М140-4-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 15, "М140-5").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 16, "М140-5").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 17, "М140-6").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 18, "М140-6").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 19, "М140-7").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 20, "М140-7").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 21, "М140-8").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 22, "М140-8").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 23, "М140-9").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 24, "М140-9").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 25, "М140-10").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 26, "М140-10").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 27, "М140-11").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 28, "М140-11").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 29, "М140-12").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 30, "М140-12").ConfigureAwait(false));

            //Процедури

            //продължителност
            allData.AddRange(await MediatorMediationDuration_Select(courtTypeId, searchCourtId, fromDate, toDate, 5, 32).ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 5, 33, "").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 34, "М140-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 35, "М140-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 36, "М140-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 37, "М140-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 38, "М140-3").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 39, "М140-3").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 40, "М140-4-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 41, "М140-4-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 42, "М140-4-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 43, "М140-4-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 44, "М140-5").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 45, "М140-5").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 46, "М140-6").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 47, "М140-6").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 48, "М140-7").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 49, "М140-7").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 50, "М140-8").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 51, "М140-8").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 52, "М140-9").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 53, "М140-9").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 54, "М140-10").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 55, "М140-10").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 56, "М140-11").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 57, "М140-11").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 58, "М140-12").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 59, "М140-12").ConfigureAwait(false));

            return SaveExcelByJudgeMediation(toDate, 10, allData, templateId, 1);
        }

        private async Task<List<ExcelReportData>> ApealSheet1Mediation(int courtTypeId, DateTime fromDate, DateTime toDate, int searchCourtId, int templateId)
        {
            List<CaseStatisticsVM> allData = new List<CaseStatisticsVM>();

            //продължителност
            allData.AddRange(await MediatorMediationDuration_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 4).ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 5, "М140-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 6, "М140-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 7, "М140-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 8, "М140-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 9, "М140-3").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 10, "М140-3").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 11, "М140-4-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 12, "М140-4-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 13, "М140-4-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 14, "М140-4-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 15, "М140-5").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 16, "М140-5").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 17, "М140-6").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 18, "М140-6").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 19, "М140-7").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 20, "М140-7").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 21, "М140-8").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 22, "М140-8").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 23, "М140-9").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 24, "М140-9").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 25, "М140-10").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 26, "М140-10").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 27, "М140-11").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 28, "М140-11").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 1, 29, "М140-12").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 2, 30, "М140-12").ConfigureAwait(false));

            //Процедури

            //продължителност
            allData.AddRange(await MediatorMediationDuration_Select(courtTypeId, searchCourtId, fromDate, toDate, 5, 32).ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 5, 33, "").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 34, "М140-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 35, "М140-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 36, "М140-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 37, "М140-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 38, "М140-3").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 39, "М140-3").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 40, "М140-4-1").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 41, "М140-4-1").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 42, "М140-4-2").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 43, "М140-4-2").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 44, "М140-5").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 45, "М140-5").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 46, "М140-6").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 47, "М140-6").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 48, "М140-7").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 49, "М140-7").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 50, "М140-8").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 51, "М140-8").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 52, "М140-9").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 53, "М140-9").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 54, "М140-10").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 55, "М140-10").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 56, "М140-11").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 57, "М140-11").ConfigureAwait(false));

            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 3, 58, "М140-12").ConfigureAwait(false));
            allData.AddRange(await MediatorMediation_Select(courtTypeId, searchCourtId, fromDate, toDate, 4, 59, "М140-12").ConfigureAwait(false));

            return SaveExcelByJudgeMediation(toDate, 10, allData, templateId, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courtTypeId"></param>
        /// <param name="courtId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="reportType"> 1 - Информационни срещи брой, 2 - Брой проведени срещи</param>
        /// <param name="colIndex"></param>
        /// <returns></returns>
        private async Task<List<CaseStatisticsVM>> MediatorMediationDuration_Select(int courtTypeId, int courtId, DateTime fromDate, DateTime toDate, int reportType, int colIndex)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime fromDateStart = fromDate.ForceStartDate();
            DateTime toDateEnd = toDate.ForceEndDate();

            Expression<Func<MediationCaseSession, bool>> courtWhere = x => true;
            if (courtId > 0)
                courtWhere = x => x.CourtId == courtId;

            Expression<Func<MediationCaseSession, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.Court.CourtTypeId == courtTypeId;

            Expression<Func<MediationCaseSession, bool>> reportTypeWhere = x => true;

            if (reportType == 1)
            {
                reportTypeWhere = x => x.DateFrom >= fromDateStart && x.DateFrom <= toDateEnd && NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId) &&
                                        x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held;
            }
            else if (reportType == 2)
            {
                reportTypeWhere = x => x.DateFrom >= fromDateStart && x.DateFrom <= toDateEnd && NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) &&
                                        x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held &&
                                        x.Results.Where(r => r.DateExpired == null && r.MediationResultId == NomenclatureConstants.MediationResultGroupConstants.Termination).Any();
            }

            var result = await repo.AllReadonly<MediationCaseSession>()
                                .Where(GeneralCaseSessionMediation())
                                .Where(courtWhere)
                                .Where(courtTypeWhere)
                                .Where(reportTypeWhere)
                                .Where(x => x.DateTo != null)
                                .SelectMany(x => x.Case.MediationCaseMediators
                                                    .Where(m => m.DateFrom <= (x.DateTo ?? dateEnd) && (m.DateTo ?? dateEnd) >= x.DateFrom)
                                                    .Select(m => new
                                                    {
                                                        CourtId = x.CourtId ?? 0,
                                                        ExcelCol = colIndex,
                                                        LawUnitData = m.MediationMediatorId + ",," + m.Mediator.Name,
                                                        DateFrom = x.DateFrom,
                                                        DateTo = x.DateTo
                                                    }))
                                .ToListAsync().ConfigureAwait(false);

            var resultObj = result.GroupBy(x => new
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
                IsDuration = true,
                Duration = TimeSpan.FromSeconds(x.Sum(a => ((a.DateTo ?? DateTime.Now) - a.DateFrom).TotalSeconds)), //НЯма с null. Филтрират се в селекта
            })
            .ToList();

            return resultObj;
        }

        private List<ExcelReportData> SaveExcelByJudgeMediation(DateTime toDate, int rowIndex, List<CaseStatisticsVM> allData, int templateId,
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
                        if (item.IsDuration == false)
                        {
                            result.Add(InsertExcelReportData(courtId, templateId, toDate.Year, toDate.Month,
                                sheetIndex, startRowIndex, item.ExcelCol, item.Count));
                        }
                        else
                        {
                            result.Add(InsertExcelReportDataInterval(courtId, templateId, toDate.Year, toDate.Month,
                                sheetIndex, startRowIndex, item.ExcelCol,item.Duration));
                        }
                    }

                    startRowIndex++;
                }
            }

            return result;
        }

        private async Task<List<ExcelReportData>> DistrictSheet1Request9Stats(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
    List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows,
    List<StatisticsExcelReportIspnReasonVM> excelReportIspnReasons)
        {
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;

            List<ExcelReportData> result = new List<ExcelReportData>();

            var allData = await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 3, excelReportCaseTypeRows).ConfigureAwait(false);

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 4, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 5, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 6, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 7, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 8, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 10, 9, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 13, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 15, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 6, 17, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 7, 18, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 8, 19, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 9, 20, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCaseSession_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 21, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 23, excelReportCaseTypeRows).ConfigureAwait(false));

            //Решения по дела за несъстоятелност
            allData.AddRange(await ActIspnReason_Select(courtTypeId, searchCourtId, fromDate, toDate, 63, excelReportIspnReasons).ConfigureAwait(false));

            //Справка за постановени решения за промени по фирмени дела
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, new int[] { NomenclatureConstants.CaseGroups.Company }, fromDate, toDate, 15, 64, 9, NomenclatureConstants.CaseInstanceType.FirstInstance).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    1, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> OSSheet3Request9Stats(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
    List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.GrajdanskoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD,
                    NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD};

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 5, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 4, 6, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 11, 3, complains, null).ConfigureAwait(false));

            //Колони за Частни Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 13, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 14, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 15, 4, complains, null).ConfigureAwait(false));

            result.AddRange(SaveExcelByFromCourt(toDate, 13, allData, templateId, 3));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 30, 1, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 31, 2, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 32, 19, null, instanceId).ConfigureAwait(false));

            //Справка 2
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 39, 2, instanceId).ConfigureAwait(false));

            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> OSSheet5Request9Stats(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
    List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            int[] caseGroups = new int[] { NomenclatureConstants.CaseGroups.NakazatelnoDelo };
            int courtTypeId = NomenclatureConstants.CourtType.DistrictCourt;
            int instanceId = NomenclatureConstants.CaseInstanceType.SecondInstance;

            int[] documentTypeGrouping = new int[] { NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND,
                           NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND,
            NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND};

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби и протести
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND ||
                    x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 5, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 6, 6, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults.Where(x => x.SheetIndex == 5).ToList()).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 19, 3, complains, null).ConfigureAwait(false));

            //Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 4, 5, complains, null).ConfigureAwait(false));

            //Протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 5, 5, complains, null).ConfigureAwait(false));


            //Колони за Частни Жалби и протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 21, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 22, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 23, 4, complains, null).ConfigureAwait(false));

            result.AddRange(SaveExcelByFromCourt(toDate, 13, allData, templateId, 5));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 30, 1, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 31, 2, null, instanceId).ConfigureAwait(false));

            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             36, 3, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             37, 4, instanceId).ConfigureAwait(false));


            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    5, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> OSSheet4Request9Stats(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 8, 9, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 10, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 12, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonSentenceCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 16, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonSentenceCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 17, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 18, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 19, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 20, 16, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 21, 17, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 23, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 25, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 26, 11, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 27, 12, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 28, 13, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 29, 14, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 30, 15, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 31, 10, instanceId).ConfigureAwait(false));
            }

            allData.AddRange(await Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 129, 2, instanceId).ConfigureAwait(false));

            //Справка 1
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 134, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 135, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 136, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 137, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 138, 12, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 139, 13, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 140, 14, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 141, 1, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 142, 3, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 143, 1, instanceId).ConfigureAwait(false));

            //Справка 2
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 149, 4, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 150, 5, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 151, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 152, 7, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 153, 12, instanceId).ConfigureAwait(false));

            //Справка 3
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 159, 9, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 161, 10, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 162, 11, instanceId).ConfigureAwait(false));

            //Справка 4
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 167, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 1, 168, 7, instanceId).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    4, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> ApealSheet1Request9Stats(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
    List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows, int courtTypeId)
        {
            List<ExcelReportData> result = new List<ExcelReportData>();

            var allData = await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 3, excelReportCaseTypeRows).ConfigureAwait(false);

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 4, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 5, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 6, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 5, 7, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 10, 8, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 2, 12, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 14, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeLifecycle_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 4, 15, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCaseSession_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 1, 16, excelReportCaseTypeRows).ConfigureAwait(false));

            allData.AddRange(await CaseTypeCase_Select(courtTypeId, searchCourtId,
                  fromDate, toDate, 3, 18, excelReportCaseTypeRows).ConfigureAwait(false));


            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    1, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> ApealSheet2Request9Stats(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 5, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 4, 6, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 12, 3, complains, null).ConfigureAwait(false));

            //Колони за Частни Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaintTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 14, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 15, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 16, 4, complains, null).ConfigureAwait(false));

            //Колони за Жалби по бавност
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsRequestSlownessTDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 18, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults).ConfigureAwait(false));

            //Колони за Жалби по 274
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateComplaint274TDGD)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 21, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 22, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 23, 4, complains, null).ConfigureAwait(false));


            result.AddRange(SaveExcelByFromCourt(toDate, 14, allData, templateId, 2));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 40, 1, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 41, 2, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 42, 19, null, instanceId).ConfigureAwait(false));

            //Справка 2
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 47, 2, instanceId).ConfigureAwait(false));

            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }
            return result;
        }

        private async Task<List<ExcelReportData>> ApealSheet3Request9Stats(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

            var documentTypes = await repo.AllReadonly<DocumentTypeGrouping>()
                               .Where(x => documentTypeGrouping.Contains(x.DocumentTypeGroup))
                               .ToListAsync().ConfigureAwait(false);

            //Колони за Жалби и протести
            var complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND ||
                    x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            var allData = await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 2, 1, complains, null).ConfigureAwait(false);

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 3, 5, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 6, 6, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 0, 4, complains, excelReportComplainResults.Where(x => x.SheetIndex == 30).ToList()).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 19, 3, complains, null).ConfigureAwait(false));

            //Жалби
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 4, 5, complains, null).ConfigureAwait(false));

            //Протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsProtestND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 5, 5, complains, null).ConfigureAwait(false));


            //Колони за Частни Жалби и протести
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsPrivateProtestComplainND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();
            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 21, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 22, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 23, 4, complains, null).ConfigureAwait(false));

            //Колони за Възобновяване
            complains = documentTypes
                    .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.StatisticsResumeND)
                    .Select(x => x.DocumentTypeId)
                    .ToArray();

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 25, 1, complains, null).ConfigureAwait(false));

            allData.AddRange(await CaseByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 26, 0, 3, instanceId, complains, true).ConfigureAwait(false));

            allData.AddRange(await CaseLifecycleByFromCourt_Select(courtTypeId, searchCourtId,
                  caseGroups, fromDate, toDate, 27, 4, complains, null).ConfigureAwait(false));

            result.AddRange(SaveExcelByFromCourt(toDate, 12, allData, templateId, 3));

            List<CaseStatisticsVM> allDataGroup = new List<CaseStatisticsVM>();

            //Справка 1
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 38, 1, null, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 3, 39, 2, null, instanceId).ConfigureAwait(false));

            //Справка 3
            allDataGroup.AddRange(await CaseLifecycleComplain_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 0, 48, 13, instanceId,
                               excelReportComplainResults.Where(x => x.SheetIndex == 130).ToList()).ConfigureAwait(false));

            //Справка 4
            allDataGroup.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, excelReportCaseCodeRows,
                             14, 13, instanceId).ConfigureAwait(false));

            //Справка 5
            allDataGroup.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 24,
                             46, 14, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await Case_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 24,
                             47, 4, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 24,
                             48, 15, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 24,
                             49, 16, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 24,
                             50, 17, instanceId).ConfigureAwait(false));

            //Справка 6
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             54, 3, instanceId).ConfigureAwait(false));
            allDataGroup.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2,
                             55, 4, instanceId).ConfigureAwait(false));


            foreach (var item in allDataGroup)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count));
            }
            return result;
        }

        private async Task<List<ExcelReportData>> RSSheet2Request9Stats(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 6, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 16, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 14, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes,
                      excelReportComplainResults, instanceId).ConfigureAwait(false));

                //Обжалвани
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 18, 5, instanceId).ConfigureAwait(false));
            }


            //Справка 1
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 53, 1, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 54, 2, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 55, 3, null, instanceId).ConfigureAwait(false));

            //Справка 2
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 59, 1, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 60, 2, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 61, 3, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 62, 1, instanceId).ConfigureAwait(false));

            //Справка 3
            int colIndex = 5;
            for (int i = 4; i <= 11; i++)
            {
                allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, colIndex, 54, i, null, instanceId).ConfigureAwait(false));
                colIndex++;
            }

            //Справка 4
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 66, 5, instanceId).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    2, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private async Task<List<ExcelReportData>> RSSheet3Request9Stats(DateTime fromDate, DateTime toDate, int searchCourtId, int templateId,
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
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count == 1 && x.SheetIndex == 130).ToList();
                else
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count > 1 && x.SheetIndex == 130).ToList();

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 4, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 5, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 6, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 7, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 11, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 12, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 13, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 14, 9, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 15, 10, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseLifecycleCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 16, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 17, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 19, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 20, 2, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 21, 16, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 23, 3, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 24, 4, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 25, 5, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 26, 6, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 27, 7, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 28, 8, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 29, 9, instanceId).ConfigureAwait(false));
                allData.AddRange(await CasePersonCaseCode_Select(courtTypeId, searchCourtId, fromDate, toDate, caseCodes, 30, 10, instanceId).ConfigureAwait(false));
            }

            //Справка 2
            //Един път се пуска за кодовете, които са по един и един път за тези които са сумарни
            for (int i = 0; i < 2; i++)
            {
                List<StatisticsExcelReportCaseCodeRowVM> caseCodes = null;
                if (i == 0)
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count == 1 && x.SheetIndex == 1003).ToList();
                else
                    caseCodes = excelReportCaseCodeRows.Where(x => x.CaseCode.Count > 1 && x.SheetIndex == 1003).ToList();

                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 2, 1, instanceId).ConfigureAwait(false));
                allData.AddRange(await CaseCaseCode_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes, 3, 6, instanceId).ConfigureAwait(false));

                allData.AddRange(await CaseLifecycleCaseCodeComplainResult_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, caseCodes,
                      excelReportComplainResults, instanceId).ConfigureAwait(false));
            }

            //Справка 3
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 143, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 144, 1,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 145, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD, NomenclatureConstants.CaseTypes.NChHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 146, 2,
                new int[] { NomenclatureConstants.CaseTypes.NOHD }, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 147, 12, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 148, 13, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSession_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 149, 14, null, instanceId).ConfigureAwait(false));
            allData.AddRange(await CasePerson_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 150, 1, instanceId).ConfigureAwait(false));

            //Справка 4
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 154, 4, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 155, 5, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 156, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 157, 7, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 158, 8, instanceId).ConfigureAwait(false));

            //Справка 6
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 169, 9, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 171, 10, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseLifecycle_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 172, 11, instanceId).ConfigureAwait(false));

            //Справка 7
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 177, 6, instanceId).ConfigureAwait(false));
            allData.AddRange(await CaseSessionAct_Select(courtTypeId, searchCourtId, caseGroups, fromDate, toDate, 2, 178, 7, instanceId).ConfigureAwait(false));

            foreach (var item in allData)
            {
                if (item.ExcelRow <= 0) continue;
                if (item.ExcelCol <= 0) continue;

                result.Add(InsertExcelReportData(item.CourtId, templateId, toDate.Year, toDate.Month,
                    3, item.ExcelRow, item.ExcelCol, item.Count));
            }

            return result;
        }

        private bool IsSystemInFeature(string paramValue, string feature)
        {
            return paramValue.Contains("#" + feature + "$");
        }

    }
}
