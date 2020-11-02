// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace IOWebApplication.Core.Services
{
  public class ExcelReportService : BaseService, IExcelReportService
  {
    private readonly ICdnService cdnService;
    public ExcelReportService(IRepository _repo, ICdnService _cdnService)
    {
      this.repo = _repo;
      cdnService = _cdnService;
    }
    public async Task<byte[]> GetReport(int courtId, int reportYear, int reportMonth)
    {
      var _reportDate = new DateTime(reportYear, reportMonth, 1);
      _reportDate = _reportDate.AddMonths(1).AddSeconds(-1);
      var reportCourt = repo.AllReadonly<Court>()
                            .Include(x => x.CourtType)
                            .Where(x => x.Id == courtId)
                            .FirstOrDefault();
      var _template = repo.AllReadonly<ExcelReportTemplate>()
                  .Where(x => x.CourtTypeId == reportCourt.CourtType.MainCourtTypeId)
                  .Where(x => x.DateFrom <= _reportDate && (x.DateTo ?? DateTime.MaxValue) >= _reportDate)
                  .FirstOrDefault();
      if (_template == null)
      {
        return null;
      }

      var blankFile = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.ExcelReportTemplate, SourceId = _template.Id.ToString() }).ConfigureAwait(false);
      if (blankFile == null)
      {
        return null;
      }
      XSSFWorkbook workBook = FromArray(Convert.FromBase64String(blankFile.FileContentBase64));

      var reportData = repo.AllReadonly<ExcelReportData>()
                                  .Where(x => x.ExcelReportTemplateId == _template.Id && x.CourtId == courtId
                                  && x.ReportYear == reportYear && x.ReportMonth == reportMonth).ToList();

      foreach (var item in reportData)
      {
        var _sheet = workBook.GetSheetAt(item.SheetIndex);
        if (_sheet != null)
        {
          var _row = _sheet.GetRow(item.RowIndex);
          if (_row != null)
          {
            ICell _cell = _row.GetCell(item.ColIndex);
            if (_cell != null)
            {
              _cell.SetCellValue(item.CellValue);
            }
          }
        }
      }

      return ToArray(workBook);
    }

    public byte[] MakeFile(string fileName)
    {
      XSSFWorkbook workBook = null;
      using (FileStream file = new FileStream(Path.Combine("wwwroot", "reports", fileName), FileMode.Open, FileAccess.Read))
      {
        workBook = new XSSFWorkbook(file);
      }
      var sheetPril1 = workBook.GetSheetAt(1);
      int offset = 8;

      for (int i = 0; i < 5; i++)
      {
        var _row = sheetPril1.GetRow(offset + i * 3);
        var colIndex = 3;
        ICell cell = _row.GetCell(colIndex);
        cell.SetCellValue(i + 1);
      }

      return ToArray(workBook);
    }

    public byte[] ToArray(XSSFWorkbook workBook)
    {
      using (var stream = new MemoryStream())
      {
        workBook.Write(stream);
        return stream.ToArray();
      }
    }
    #region FillNames

    public XSSFWorkbook FromArray(byte[] fileBytes)
    {
      using (var stream = new MemoryStream(fileBytes))
      {
        return new XSSFWorkbook(stream);
      }
    }
    public bool Fill_AS_CourtNamesAndDate(Court court, int reportYear, int reportMonth)
    {

      var _reportDate = new DateTime(reportYear, reportMonth, 1);
      var _template = repo.AllReadonly<ExcelReportTemplate>()
               .Where(x => x.CourtTypeId == court.CourtType.MainCourtTypeId)
               .Where(x => x.DateFrom <= _reportDate && (x.DateTo ?? DateTime.MaxValue) >= _reportDate)
               .FirstOrDefault();

      bool result = false;
      try
      {
        string polugodie = (reportMonth == 6) ? "I полугодие на" : "цялата";
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 9, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 11, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 12, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 1, 9, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 1, 11, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 1, 12, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 0, 9, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 0, 11, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 0, 12, $"месеца на {reportYear}г."); ;
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 4, 1, 2, $"Справка за дейността на съдиите в {court.Label} през {polugodie} {reportYear}г. (НАКАЗАТЕЛНИ  ДЕЛА).");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 5, 1, 2, $"Справка за резултатите от върнати обжалвани и протестирани НАКАЗАТЕЛНИ дела на съдиите от {court.CourtType.Label} гр.{(court.CityName ?? GetCourtCity(court.Label))} през {polugodie}  {reportYear}г. ");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 6, 1, 3, $"Справка за дейността на съдиите в {court.Label} през {polugodie} {reportYear}г. (ГРАЖДАНСКИ  И ТЪРГОВСКИ ДЕЛА).");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 7, 1, 2, $"Справка за резултатите от върнати обжалвани и протестирани ГРАЖДАНСКИ И ТЪРГОВСКИ дела на съдиите от {court.CourtType.Label} гр.{(court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г. ");

        result = true;
      }
      catch (Exception)
      {


      }
      return result;

    }

    public bool Fill_RS_CourtNamesAndDate(Court court, int reportYear, int reportMonth)
    {

      var _reportDate = new DateTime(reportYear, reportMonth, 1);
      var _template = repo.AllReadonly<ExcelReportTemplate>()
               .Where(x => x.CourtTypeId == court.CourtType.MainCourtTypeId)
               .Where(x => x.DateFrom <= _reportDate && (x.DateTo ?? DateTime.MaxValue) >= _reportDate)
               .FirstOrDefault();

      bool result = false;
      try
      {
        string polugodie = (reportMonth == 6) ? "I полугодие на" : "цялата";
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 0, 10, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 0, 12, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 0, 13, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 10, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 12, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 13, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 0, 10, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 0, 12, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 0, 13, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 4, 0, 2, $"Справка за дейността на съдиите в {court.CourtType.Label} гр.{(court.CityName ?? GetCourtCity(court.Label))}");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 4, 1, 2, $"за {polugodie} {reportYear}г. (НАКАЗАТЕЛНИ ДЕЛА)");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 5, 1, 2, $"Справка за резултатите от върнати обжалвани и протестирани НАКАЗАТЕЛНИТЕ дела на съдиите от {court.CourtType.Label} гр.{ (court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г. ");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 6, 1, 2, $"Справка за дейността на съдиите в {court.CourtType.Label} гр.{(court.CityName ?? GetCourtCity(court.Label))}");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 6, 2, 2, $"за {polugodie} {reportYear}г. (ГРАЖДАНСКИ  ДЕЛА)");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 7, 1, 1, $"Справка за резултатите от върнати обжалвани и протестирани ГРАЖДАНСКИ и ТЪРГОВСКИ дела на съдиите от {court.CourtType.Label} гр.{ (court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г. ");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 8, 1, 2, $"Справка за резултатите от върнати обжалвани и протестирани АДМИНИСТРАТИВНИ дела на съдиите от {court.CourtType.Label} гр.{ (court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г. ");




        result = true;
      }
      catch (Exception)
      {


      }
      return result;

    }
    public bool Fill_OS_CourtNamesAndDate(Court court, int reportYear, int reportMonth)
    {

      var _reportDate = new DateTime(reportYear, reportMonth, 1);
      var _template = repo.AllReadonly<ExcelReportTemplate>()
               .Where(x => x.CourtTypeId == court.CourtType.MainCourtTypeId)
               .Where(x => x.DateFrom <= _reportDate && (x.DateTo ?? DateTime.MaxValue) >= _reportDate)
               .FirstOrDefault();

      bool result = false;
      try
      {
        string polugodie = (reportMonth == 6) ? "I полугодие на" : "цялата";
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 10, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 12, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 13, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 10, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 13, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 14, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 1, 10, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 1, 12, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 1, 13, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 4, 0, 10, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 4, 0, 12, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 4, 0, 13, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 5, 1, 10, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 5, 1, 12, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 5, 1, 13, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 6, 1, 2, $"Справка за дейността на съдиите в {court.CourtType.Code} гр.{(court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г. (НАКАЗАТЕЛНИ ДЕЛА)");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 7, 1, 2, $"Справка за резултатите от върнати обжалвани и протестирани НАКАЗАТЕЛНИ дела на съдиите от { court.CourtType.Label} гр.{(court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 8, 1, 2, $"Справка за дейността на съдиите в {court.CourtType.Code} гр.{(court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г. (ГРАЖДАНСКИ  И ТЪРГОВСКИ ДЕЛА)");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 9, 1, 2, $"Справка за резултатите от върнати обжалвани и протестирани ГРАЖДАНСКИ и ТЪРГОВСКИ дела на съдиите от { court.CourtType.Label} гр.{(court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г.");





        result = true;
      }
      catch (Exception)
      {


      }
      return result;

    }
    public bool Fill_VS_CourtNamesAndDate(Court court, int reportYear, int reportMonth)
    {

      var _reportDate = new DateTime(reportYear, reportMonth, 1);
      var _template = repo.AllReadonly<ExcelReportTemplate>()
               .Where(x => x.CourtTypeId == court.CourtType.MainCourtTypeId)
               .Where(x => x.DateFrom <= _reportDate && (x.DateTo ?? DateTime.MaxValue) >= _reportDate)
               .FirstOrDefault();

      bool result = false;
      try
      {
        string polugodie = (reportMonth == 6) ? "I полугодие на" : "цялата";
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 10, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 12, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 13, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 10, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 12, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 13, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 1, 2, $"Справка за дейността на съдиите във { court.CourtType.Label} гр.{(court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 4, 1, 2, $"Справка за резултатите от върнати обжалвани и протестирани НАКАЗАТЕЛНИ дела на съдиите от { court.CourtType.Label} гр.{(court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г.");

        result = true;
      }
      catch (Exception)
      {


      }
      return result;

    }
    public bool Fill_VAPS_CourtNamesAndDate(Court court, int reportYear, int reportMonth)
    {

      var _reportDate = new DateTime(reportYear, reportMonth, 1);
      var _template = repo.AllReadonly<ExcelReportTemplate>()
               .Where(x => x.CourtTypeId == court.CourtType.MainCourtTypeId)
               .Where(x => x.DateFrom <= _reportDate && (x.DateTo ?? DateTime.MaxValue) >= _reportDate)
               .FirstOrDefault();

      bool result = false;
      try
      {
        string polugodie = (reportMonth == 6) ? "I полугодие на" : "цялата";
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 9, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 11, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 1, 1, 12, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 9, (court.CityName ?? GetCourtCity(court.Label)));
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 11, reportMonth.ToString());
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 2, 0, 12, $"месеца на {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 3, 1, 2, $"Справка за дейността на съдиите във Военно-апелативния съд през {polugodie} {reportYear}г.");
        InsertExcelReportData(court.Id, _template.Id, reportYear, reportMonth, 4, 1, 2, $"Справка за резултатите от върнати обжалвани и протестирани НАКАЗАТЕЛНИТЕ дела на съдиите от { court.CourtType.Label} гр.{(court.CityName ?? GetCourtCity(court.Label))} през {polugodie} {reportYear}г.");

        result = true;
      }
      catch (Exception)
      {


      }
      return result;

    }
    public ExcelReportData InsertExcelReportData(int court_id, int reportTemplateId, int reportYear, int reportMonth, int sheetIndex, int rowIndex, int colIndex, string cellValue, int RowDataColIndex = 0, string rowData = null)
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
        exd.RowDataColIndex = RowDataColIndex;
        exd.RowData = rowData;
        //exd.UserId = userContext.UserId;
        repo.Add<ExcelReportData>(exd);
        //repo.SaveChanges();



      }
      catch (Exception)
      {


      }
      return exd;

    }

    private string GetCourtCity(string courtName)
    {
      string city = "";

      try
      {
        city = courtName.Split('–').Last();
      }
      catch (Exception)
      {

      }
      return city;
    }

    public bool FillAllCourts(int reportYear, int reportMonth)
    {
      bool result = false;
      var courts = repo.AllReadonly<Court>()
                         .Include(x => x.CourtType)
                      // .Where(x=>x.Id==178)
                      .ToList();
      int insertCount = 0;
      foreach (var item in courts)
      {

        if (item.CourtType.MainCourtTypeId == IOWebApplication.Infrastructure.Constants.NomenclatureConstants.CourtType.Apeal)
        {
          Fill_AS_CourtNamesAndDate(item, reportYear, reportMonth);
          insertCount += 13;
        }
        if (item.CourtType.MainCourtTypeId == IOWebApplication.Infrastructure.Constants.NomenclatureConstants.CourtType.RegionalCourt)
        {
          Fill_RS_CourtNamesAndDate(item, reportYear, reportMonth);
          insertCount += 16;
        }
        if (item.CourtType.MainCourtTypeId == IOWebApplication.Infrastructure.Constants.NomenclatureConstants.CourtType.DistrictCourt)
        {
          Fill_OS_CourtNamesAndDate(item, reportYear, reportMonth);
          insertCount += 19;
        }
        if (item.CourtType.MainCourtTypeId == IOWebApplication.Infrastructure.Constants.NomenclatureConstants.CourtType.Millitary)
        {
          Fill_VS_CourtNamesAndDate(item, reportYear, reportMonth);
          insertCount += 8;
        }
        if (item.CourtType.MainCourtTypeId == IOWebApplication.Infrastructure.Constants.NomenclatureConstants.CourtType.MillitaryApeal)
        {
          Fill_VAPS_CourtNamesAndDate(item, reportYear, reportMonth);
          insertCount += 8;
        }
        if (insertCount > 200)
        {
          insertCount = 0;
          repo.SaveChanges();
        }
      }
      repo.SaveChanges();


      return result;
    }
    #endregion

    public IEnumerable<CaseReportVss> GetReportCases(int courtId, int caseGroupid, int reportYear, int reportMonth)
    {
      var _reportStartDate = new DateTime(reportYear, 1, 1);
      var _reportDate = new DateTime(reportYear, reportMonth, 1);
      _reportDate = _reportDate.AddMonths(1).AddSeconds(-1);
      var result = repo.AllReadonly<Case>()
                      .Where(x => x.RegNumber != null)
                      .Where(x => x.CourtId == courtId)
                      .Where(x => x.CaseGroupId == caseGroupid)
                      .Select(x => new CaseReportVss()
                      {
                        Id = x.Id,
                        CaseGroupId = x.CaseGroupId,
                        CaseCharacterId = x.CaseCharacterId,
                        CaseTypeId = x.CaseTypeId,
                        CaseCodeId = x.CaseCodeId,
                        CourtGroupId = x.CourtGroupId,
                        CaseInforcedDate = x.CaseInforcedDate,
                        CaseReasonId = x.CaseReasonId,
                        CaseDurationMonths = repo.AllReadonly<CaseLifecycle>().Where(a => a.CaseId == x.Id).Sum(a => a.DurationMonths),
                        CaseStateId = x.CaseStateId,
                        CaseTypeUnitId = x.CaseTypeUnitId,
                        ComplexIndex = x.ComplexIndex,
                        CorrectionLoadIndex = x.CorrectionLoadIndex,
                        Description = x.Description,
                        DocumentId = x.DocumentId,
                        DocumentTypeId = x.Document.DocumentTypeId,
                        EISSPNumber = x.EISSPNumber,
                        IsOldNumber = x.IsOldNumber,
                        IsRestictedAccess = x.IsRestictedAccess,
                        LoadGroupLinkId = x.LoadGroupLinkId,
                        LoadGrouId = x.LoadGroupLink.LoadGroupId,
                        LoadIndex = x.LoadIndex,
                        ProcessPriorityId = x.ProcessPriorityId,
                        RegNumber = x.RegNumber,
                        RegDate = x.RegDate,
                        ShortNumber = x.ShortNumber,
                        ShortNumberValue = x.ShortNumberValue,
                        CourtId = x.CourtId,
                        JudgeReporterId = repo.AllReadonly<CaseLawUnit>().Where(a => a.CaseId == x.Id)
                                                                         .Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                         .Where(a => a.DateFrom < _reportDate && (a.DateTo ?? _reportDate) >= _reportDate)
                                                                       .Select(a => a.LawUnitId).FirstOrDefault(),

                        NotFinishedPreviousPeriod = (x.RegDate < _reportStartDate),
                        StartedInPeriod = (x.RegDate > _reportStartDate && x.RegDate < _reportDate),
                        FinishedByCanceling = (x.CaseStateId == NomenclatureConstants.CaseState.Suspend),
                        FinishedByDecision = (x.CaseStateId == NomenclatureConstants.CaseState.Resolution),
                        FinishedInThreeMonths = (repo.AllReadonly<CaseLifecycle>().Where(a => a.CaseId == x.Id).Sum(a => a.DurationMonths) < 3),
                        CaseStateDescription = x.CaseStateDescription,
                        IsISPNcase = x.IsISPNcase,
                        IsNewCaseNewNumber = x.IsNewCaseNewNumber,
                        DateWrt = x.DateWrt,
                        UserId = x.UserId,
                        User = x.User



                      }).ToList();
      return result;

    }
    #region RS_FillSheets


    public bool FillAll_RS_CourtsDataSheets(int reportYear, int reportMonth)
    {
      bool result = false;
      var _reportDate = new DateTime(reportYear, reportMonth, 1);
      _reportDate = _reportDate.AddMonths(1).AddSeconds(-1);
      var _template = repo.AllReadonly<ExcelReportTemplate>()
               .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
               .Where(x => x.DateFrom <= _reportDate && (x.DateTo ?? DateTime.MaxValue) >= _reportDate)
               .FirstOrDefault();

      var courts = repo.AllReadonly<Court>()
                         .Include(x => x.CourtType)
                         .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                       //.Where(x => x.Id == 83)
                      .ToList();
      int insertCount = 0;
      foreach (var item in courts)
      {
        RS_FillSheets_Sheet4(item.Id, _template.Id, reportYear, reportMonth);

        if (insertCount > 200)
        {
          insertCount = 0;
          repo.SaveChanges();
        }
      }
      repo.SaveChanges();


      return result;
    }
    public bool RS_FillSheets_Sheet4(int courtId, int reportTemplateId, int reportYear, int reportMonth)

    {
      var current_sheet = 4;
      bool result = false;
      var _currentCases = GetReportCases(courtId, 2, reportYear, reportMonth);
      var judgeReporters = _currentCases.Where(x => x.JudgeReporterId > 0)
                                         .Select(x => x.JudgeReporterId)
                                         .Distinct().ToArray();
      var _current_judges = repo.AllReadonly<LawUnit>()

        .Where(x => judgeReporters.Contains(x.Id)).OrderBy(x => x.FullName).ToList();


      int row_offset = 7;
      int current_number = 0;
      foreach (var judge in _current_judges)
      {
        current_number += 1;
        int current_row = row_offset + current_number;
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 0, current_number.ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 1, judge.FullName);
        var _judgeCases = _currentCases.Where(x => x.JudgeReporterId == judge.Id);
        var nohd1 = _judgeCases.Where(x => x.CaseTypeId == 1);
        var nchd2 = _judgeCases.Where(x => x.CaseTypeId == 2);
        var cnd3 = _judgeCases.Where(x => x.CaseTypeId == 3);
        var anhd4 = _judgeCases.Where(x => x.CaseTypeId == 4);
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 4, nohd1.Where(x => x.NotFinishedPreviousPeriod == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 5, nchd2.Where(x => x.NotFinishedPreviousPeriod == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 6, "0");
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 7, cnd3.Where(x => x.NotFinishedPreviousPeriod == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 8, anhd4.Where(x => x.NotFinishedPreviousPeriod == true).Count().ToString());


        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 10, nohd1.Where(x => x.StartedInPeriod == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 11, nchd2.Where(x => x.StartedInPeriod == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 12, "0");
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 13, cnd3.Where(x => x.StartedInPeriod == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 14, anhd4.Where(x => x.StartedInPeriod == true).Count().ToString());

        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 28, nohd1.Where(x => x.FinishedByDecision == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 29, nchd2.Where(x => x.FinishedByDecision == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 30, "0");
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 31, cnd3.Where(x => x.FinishedByDecision == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 32, anhd4.Where(x => x.FinishedByDecision == true).Count().ToString());

        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 34, nohd1.Where(x => x.FinishedByCanceling == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 35, nchd2.Where(x => x.FinishedByCanceling == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 36, "0");
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 37, cnd3.Where(x => x.FinishedByCanceling == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 38, anhd4.Where(x => x.FinishedByCanceling == true).Count().ToString());
       
        
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 40, nohd1.Where(x => x.FinishedInThreeMonths == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 41, nchd2.Where(x => x.FinishedInThreeMonths == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 42,"0");
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 43, cnd3.Where(x => x.FinishedInThreeMonths == true).Count().ToString());
        InsertExcelReportData(courtId, reportTemplateId, reportYear, reportMonth, current_sheet, current_row, 44, anhd4.Where(x => x.FinishedInThreeMonths == true).Count().ToString());

      }



      return result;
    }
    #endregion


  }
}
