// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
  public interface IExcelReportService
  {
    byte[] MakeFile(string fileName);

    Task<byte[]> GetReport(int courtId, int reportYear, int reportMonth);
    #region FillNames
    bool Fill_AS_CourtNamesAndDate(Court court, int reportYear, int reportMonth);
    bool Fill_RS_CourtNamesAndDate(Court court, int reportYear, int reportMonth);
    ExcelReportData InsertExcelReportData(int court_id, int reportTemplateId, int reportYear, int reportMonth, int sheetIndex, int rowIndex, int colIndex, string cellValue, int RowDataColIndex = 0, string rowData = null);
    bool FillAllCourts(int reportYear, int reportMonth);
    bool Fill_OS_CourtNamesAndDate(Court court, int reportYear, int reportMonth);
   bool Fill_VS_CourtNamesAndDate(Court court, int reportYear, int reportMonth);
    bool Fill_VAPS_CourtNamesAndDate(Court court, int reportYear, int reportMonth);
    #endregion
    IEnumerable<CaseReportVss> GetReportCases(int courtId, int caseGroupid, int reportYear, int reportMonth);
    bool FillAll_RS_CourtsDataSheets(int reportYear, int reportMonth);
    bool RS_FillSheets_Sheet4(int courtId, int reportTemplateId, int reportYear, int reportMonth);
  }
}
