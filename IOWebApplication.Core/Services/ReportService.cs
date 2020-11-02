// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using AutoMapper.Configuration.Conventions;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Resources;

namespace IOWebApplication.Core.Services
{
    public class ReportService : BaseService, IReportService
    {
        public ReportService(
            ILogger<ReportService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        private IQueryable<DocumentOutReportVM> DocumentOutGoingReport_Select(int courtId, DocumentOutFilterReportVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Document, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.DocumentDate.Date >= dateFromSearch.Date && x.DocumentDate.Date <= dateToSearch.Date;

            Expression<Func<Document, bool>> documentNumberWhere = x => true;
            if (model.FromNumber != null || model.ToNumber != null)
            {
                documentNumberWhere = x => (x.DocumentNumberValue ?? 0) >= (model.FromNumber ?? 0) && (x.DocumentNumberValue ?? 0) <= (model.ToNumber ?? int.MaxValue);
            }

            Expression<Func<Document, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.DocumentCaseInfo.Where(a => a.Case.CaseGroupId == model.CaseGroupId).Any();

            Expression<Func<Document, bool>> caseTypeWhere = x => true;
            if (string.IsNullOrEmpty(model.CaseTypeIds) == false)
            {
                var types = model.CaseTypeIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                caseTypeWhere = x => x.DocumentCaseInfo.Where(a => types.Contains(a.Case.CaseTypeId.ToString())).Any();
            }


            return repo.AllReadonly<Document>()
                                .Include(x => x.DocumentPersons)
                                .Include(x => x.DocumentGroup)
                                .Include(x => x.DeliveryGroup)
                                .Where(x => x.CourtId == courtId && x.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                                .Where(x => x.DateExpired == null)
                                .Where(dateSearch)
                                .Where(documentNumberWhere)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Select(x => new DocumentOutReportVM
                                {
                                    DocumentNumber = x.DocumentNumber,
                                    DocumentDate = x.DocumentDate,
                                    Description = x.DocumentType.Label + Environment.NewLine +
                                                    String.Join(Environment.NewLine, x.DocumentPersons.Select(p => p.FullName)) +
                                                    (string.IsNullOrEmpty(x.Description) ? "" : Environment.NewLine + x.Description) +
                                           x.DocumentCaseInfo.Select(a => Environment.NewLine + a.Case.CaseType.Code + "; № " + a.Case.RegNumber)
                                                                      .FirstOrDefault(),
                                    DeliveryGroupName = x.DeliveryGroup.Label,
                                    DocumentNumberValue = (x.DocumentNumberValue ?? 0)
                                }).AsQueryable();
        }

        public byte[] DocumentOutGoingReportToExcelOne(DocumentOutFilterReportVM model)
        {
            var dataRows = DocumentOutGoingReport_Select(userContext.CourtId, model).OrderBy(x => x.DocumentNumberValue).ToList();

            NPoiExcelService excelService = GetExcelHtmlTemplate("DocumentOut", model.CaseGroupId);
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<DocumentOutReportVM, object>>>()
                {
                    x => x.DocumentNumber,
                    x => x.DocumentDate,
                    x => x.Description,
                    x => x.DeliveryGroupName,
                }
            );
            return excelService.ToArray();
        }

        private IQueryable<DocumentInReportVM> DocumentInComingReport_Select(int courtId, DocumentInFilterReportVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Document, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.DocumentDate.Date >= dateFromSearch.Date && x.DocumentDate.Date <= dateToSearch.Date;

            Expression<Func<Document, bool>> documentWhere = x => true;
            if (model.DocumentKindId > 0)
            {
                documentWhere = x => x.DocumentGroup.DocumentKindId == model.DocumentKindId;
            }

            Expression<Func<Document, bool>> documentNumberWhere = x => true;
            if (model.FromNumber != null || model.ToNumber != null)
            {
                documentNumberWhere = x => (x.DocumentNumberValue ?? 0) >= (model.FromNumber ?? 0) && (x.DocumentNumberValue ?? 0) <= (model.ToNumber ?? int.MaxValue);
            }

            Expression<Func<Document, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => (x.DocumentCaseInfo.Where(a => a.Case.CaseGroupId == model.CaseGroupId).Any() ||
                                      x.Cases.Where(a => a.CaseGroupId == model.CaseGroupId).Any());

            Expression<Func<Document, bool>> caseTypeWhere = x => true;
            if (string.IsNullOrEmpty(model.CaseTypeIds) == false)
            {
                var types = model.CaseTypeIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                caseTypeWhere = x => x.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument ?
                                       x.Cases.Where(a => types.Contains(a.CaseTypeId.ToString())).Any() :
                                       x.DocumentCaseInfo.Where(a => types.Contains(a.Case.CaseTypeId.ToString())).Any()
                                      ;
            }

            return repo.AllReadonly<Document>()
                                .Include(x => x.DocumentPersons)
                                .Include(x => x.DocumentType)
                                .Include(x => x.Cases)
                                .ThenInclude(x => x.CaseGroup)
                                .Where(x => x.CourtId == courtId && x.DocumentDirectionId == DocumentConstants.DocumentDirection.Incoming)
                                .Where(x => x.DateExpired == null)
                                .Where(dateSearch)
                                .Where(documentWhere)
                                .Where(documentNumberWhere)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Select(x => new DocumentInReportVM
                                {
                                    DocumentNumber = x.DocumentNumber,
                                    DocumentDate = x.DocumentDate,
                                    Description = x.DocumentType.Label + (x.Description == null ? "" : " - " + x.Description) +
                                                  x.Cases.Select(p => Environment.NewLine + p.CaseGroup.Label).DefaultIfEmpty("").FirstOrDefault() +
                                                  x.DocumentCaseInfo.Select(a => Environment.NewLine +
                                                    (a.CaseId != null ? a.Case.RegNumber : a.CaseRegNumber)).DefaultIfEmpty("").FirstOrDefault(),
                                    DocumentPersonName = String.Join(Environment.NewLine, x.DocumentPersons
                                                                              .Select(a => a.FullName + "(" + a.PersonRole.Label + ")")),
                                    TaskName = String.Join("; ", repo.AllReadonly<WorkTask>().Include(t => t.TaskType).Include(t => t.User).Include(t => t.User.LawUnit)
                                                                         .Where(t => t.SourceType == SourceTypeSelectVM.Document &&
                                                                           t.SourceId == x.Id).OrderBy(t => t.Id).Select(t => t.TaskType.Label + " - " + t.User.LawUnit.FullName)
                                                                         .DefaultIfEmpty("").FirstOrDefault()),
                                    DocumentNumberValue = (x.DocumentNumberValue ?? 0)
                                }).AsQueryable();
        }

        public byte[] DocumentInGoingReportToExcelOne(DocumentInFilterReportVM model)
        {
            var dataRows = DocumentInComingReport_Select(userContext.CourtId, model).OrderBy(x => x.DocumentNumberValue).ToList();

            NPoiExcelService excelService = GetExcelHtmlTemplate("DocumentIn");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<DocumentInReportVM, object>>>()
                {
                    x => x.DocumentNumber,
                    x => x.DocumentDate,
                    x => x.Description,
                    x => x.DocumentPersonName,
                    x => x.TaskName,
                }
            );
            return excelService.ToArray();
        }

        private CaseAlphabeticalVM fillAlphabeticalVM(Case caseCase, CasePerson casePerson)
        {
            CaseAlphabeticalVM alphabeticalVM = new CaseAlphabeticalVM()
            {
                CaseGroupId = caseCase.CaseGroupId,
                CaseGroupLabel = caseCase.CaseGroup.Label,
                CaseTypeId = caseCase.CaseTypeId,
                CaseTypeLabel = caseCase.CaseType.Code,
                ReportGroupe = caseCase.CaseType.ReportGroupAzbuchnik,
                ShortNumber = caseCase.ShortNumber,
                RegDate = caseCase.RegDate,
                RegDateString = caseCase.RegDate.ToString("dd.MM.yyyy"),
                Name = (casePerson.FullName ?? string.Empty),
                FirstLetter = ((casePerson.FullName != null) ? casePerson.FullName[0] : '\0'),
                IdentityNumber = casePerson.Uic ?? string.Empty,
                CaseNumberString = caseCase.RegNumber,
                UicTypeId = casePerson.UicTypeId
            };

            return alphabeticalVM;
        }

        private IQueryable<CaseAlphabeticalVM> CaseAlphabetical_Select(int courtId, CaseAlphabeticalFilterVM model)
        {
            DateTime dateFrom = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateTo = model.DateTo ?? DateTime.Now.AddYears(100);
            var cases = repo.AllReadonly<Case>()
                .Include(x => x.Court)
                .Include(x => x.CaseGroup)
                .Include(x => x.CaseType)
                .Include(x => x.CasePersons)
                .ThenInclude(x => x.PersonRole)
                .Include(x => x.CasePersons)
                .Where(x => x.CourtId == courtId &&
                            ((model.CaseGroupId > 0) ? (x.CaseGroupId == model.CaseGroupId) : true) &&
                            ((x.RegDate.Date >= dateFrom.Date) && (x.RegDate.Date <= dateTo.Date)))
                .ToList();

            List<CaseAlphabeticalVM> caseAlphabeticalVMs = new List<CaseAlphabeticalVM>();

            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            string[] alphabets = (model.Alphabet ?? string.Empty).Split(delimiterChars);

            foreach (var caseCase in cases)
            {
                List<CasePerson> casePersons = null;
                if (caseCase.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                {
                    casePersons = caseCase.CasePersons.Where(x => x.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide ||
                             x.PersonRoleId == NomenclatureConstants.PersonRole.Petitioner ||
                             x.PersonRoleId == NomenclatureConstants.PersonRole.Offender)
                        .ToList();
                }
                else
                {
                    casePersons = caseCase.CasePersons.Where(x => x.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide ||
                             x.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.LeftSide)
                        .ToList();
                }

                foreach (var casePerson in casePersons.Where(x => (x.CaseSessionId == null) && x.DateExpired == null && ((x.FullName ?? string.Empty) != string.Empty)))
                {
                    if ((model.Alphabet ?? string.Empty) != string.Empty)
                    {
                        if (alphabets.Any(x => x.ToUpper() == casePerson.FullName[0].ToString().ToUpper()))
                            caseAlphabeticalVMs.Add(fillAlphabeticalVM(caseCase, casePerson));
                    }
                    else
                        caseAlphabeticalVMs.Add(fillAlphabeticalVM(caseCase, casePerson));
                }
            }

            return caseAlphabeticalVMs.OrderBy(x => x.CaseGroupId).ThenBy(x => x.Name).AsQueryable();
        }

        private int SetRowAndColumn(NPoiExcelService excelService, int colIndex, int rowIndex, int rowMax)
        {
            excelService.colIndex = colIndex;
            excelService.rowIndex = rowIndex;

            return (rowMax > rowIndex) ? rowMax : rowIndex;
        }

        private int FillHeaderTable(NPoiExcelService excelService, XSSFCellStyle style, int rowIndex, string title1, string title2, string title3, string title4)
        {
            SetRowAndColumn(excelService, 0, rowIndex, rowIndex);
            excelService.AddRange(title1, 8, excelService.CreateTitleStyle());
            excelService.AddRow();
            excelService.AddRange(title2, 8, excelService.CreateTitleStyle());
            excelService.AddRow();
            excelService.AddRange(title3, 8, excelService.CreateTitleStyle());
            excelService.AddRow();
            excelService.AddRange(title4, 8, excelService.CreateTitleStyle());
            excelService.AddRow();

            rowIndex = rowIndex + 5;
            int rowMax = rowIndex;

            excelService.SetColumnWidth(0, 8000);
            excelService.SetColumnWidth(1, 8000);
            excelService.SetColumnWidth(2, 8000);
            excelService.SetColumnWidth(3, 8000);
            excelService.SetColumnWidth(4, 8000);
            excelService.SetColumnWidth(5, 8000);
            excelService.SetColumnWidth(6, 8000);
            excelService.SetColumnWidth(7, 5000);

            rowMax = SetRowAndColumn(excelService, 0, rowIndex, rowMax);
            excelService.AddRange("Номер на дело по опис", 6, 1, style);

            rowMax = SetRowAndColumn(excelService, 0, rowIndex + 1, rowMax);
            excelService.AddRange("Гражданско", 1, 2, style);

            rowMax = SetRowAndColumn(excelService, 1, rowIndex + 1, rowMax);
            excelService.AddRange("Наказателно", 3, 1, style);

            rowMax = SetRowAndColumn(excelService, 4, rowIndex + 1, rowMax);
            excelService.AddRange("Частно производство", 2, 1, style);

            rowMax = SetRowAndColumn(excelService, 1, rowIndex + 2, rowMax);
            excelService.AddRange("Общ характер", 1, 1, style);

            rowMax = SetRowAndColumn(excelService, 2, rowIndex + 2, rowMax);
            excelService.AddRange("Частен характер", 1, 1, style);

            rowMax = SetRowAndColumn(excelService, 3, rowIndex + 2, rowMax);
            excelService.AddRange("Административно", 1, 1, style);

            rowMax = SetRowAndColumn(excelService, 4, rowIndex + 2, rowMax);
            excelService.AddRange("Гражданско", 1, 1, style);

            rowMax = SetRowAndColumn(excelService, 5, rowIndex + 2, rowMax);
            excelService.AddRange("Наказателно", 1, 1, style);

            rowMax = SetRowAndColumn(excelService, 6, rowIndex, rowMax);
            excelService.AddRange("Собствено, бащино и фамилно име на страната (ищец, тъжител, подсъдим или въззивник)", 1, 3, style);

            rowMax = SetRowAndColumn(excelService, 7, rowIndex, rowMax);
            excelService.AddRange("ЕГН/БУЛСТАТ", 1, 3, style);

            return rowMax;
        }

        private void FillHeaderMonth(NPoiExcelService excelService, XSSFCellStyle style, DateTime dateTime)
        {
            excelService.AddRange("Месец: " + dateTime.ToString("MMMM").ToUpper() + " " + dateTime.ToString("yyyy"), 8, 1, style);
        }

        private XSSFCellStyle SetStyle(NPoiExcelService excelService, short color)
        {
            var style = excelService.CreateTitleStyle();
            excelService.SetStyleBorderMedium(style);
            excelService.SetColor(style, color);
            return style;
        }

        private void FillRow(CaseAlphabeticalVM caseAlphabetical, NPoiExcelService excelService, XSSFCellStyle style, int rowIndex, bool replaceEgn)
        {
            var delo = caseAlphabetical.CaseTypeLabel + " " + caseAlphabetical.CaseNumberString + " / " + caseAlphabetical.RegDateString;

            SetRowAndColumn(excelService, 0, rowIndex, rowIndex);
            excelService.AddRange((caseAlphabetical.ReportGroupe == 1) ? delo : string.Empty, 1, 1, style);
            SetRowAndColumn(excelService, 1, rowIndex, rowIndex);
            excelService.AddRange((caseAlphabetical.ReportGroupe == 2) ? delo : string.Empty, 1, 1, style);
            SetRowAndColumn(excelService, 2, rowIndex, rowIndex);
            excelService.AddRange((caseAlphabetical.ReportGroupe == 3) ? delo : string.Empty, 1, 1, style);
            SetRowAndColumn(excelService, 3, rowIndex, rowIndex);
            excelService.AddRange((caseAlphabetical.ReportGroupe == 4) ? delo : string.Empty, 1, 1, style);
            SetRowAndColumn(excelService, 4, rowIndex, rowIndex);
            excelService.AddRange((caseAlphabetical.ReportGroupe == 5) ? delo : string.Empty, 1, 1, style);
            SetRowAndColumn(excelService, 5, rowIndex, rowIndex);
            excelService.AddRange((caseAlphabetical.ReportGroupe == 6) ? delo : string.Empty, 1, 1, style);
            SetRowAndColumn(excelService, 6, rowIndex, rowIndex);
            excelService.AddRange(caseAlphabetical.Name, 1, 1, style);

            var idenNumber = string.Empty;
            if ((caseAlphabetical.UicTypeId == NomenclatureConstants.UicTypes.EGN) ||
                (caseAlphabetical.UicTypeId == NomenclatureConstants.UicTypes.LNCh) ||
                (caseAlphabetical.UicTypeId == NomenclatureConstants.UicTypes.BirthDate))
            {
                idenNumber = caseAlphabetical.IdentityNumber;
                if (replaceEgn && string.IsNullOrEmpty(caseAlphabetical.IdentityNumber) == false)
                    idenNumber = "**********";
            }
            else
                idenNumber = (caseAlphabetical.IdentityNumber != string.Empty) ? caseAlphabetical.IdentityNumber : string.Empty;

            SetRowAndColumn(excelService, 7, rowIndex, rowIndex);
            excelService.AddRange(idenNumber, 1, 1, style);
        }

        private XSSFCellStyle SetStyleRow(NPoiExcelService excelService, short color)
        {
            var style = excelService.CreateDefaultStyle();
            excelService.SetStyleBorderMedium(style);
            excelService.SetColor(style, color);
            return style;
        }

        public byte[] CaseAlphabetical_ToExcel(int courtId, CaseAlphabeticalFilterVM model)
        {
            NPoiExcelService excelService = GetExcelHtmlTemplate("CaseAlphabetical");
            var caseAlphabeticals = CaseAlphabetical_Select(courtId, model).OrderBy(x => x.Name).ToList();

            var charFirstLetter = '\0';
            int monthNum = 0;

            var style = SetStyle(excelService, NPOI.HSSF.Util.HSSFColor.White.Index);
            var styleRow1 = SetStyleRow(excelService, NPOI.HSSF.Util.HSSFColor.White.Index);
            var styleRow2 = SetStyleRow(excelService, NPOI.HSSF.Util.HSSFColor.White.Index);

            var rowMax = 2;
            bool colorType = true;

            var minDelo = string.Empty;
            var maxDelo = string.Empty;
            if (caseAlphabeticals.Count > 0)
            {
                var min = caseAlphabeticals.Min(x => Int64.Parse(x.CaseNumberString));
                var max = caseAlphabeticals.Max(x => Int64.Parse(x.CaseNumberString));

                var minDeloO = caseAlphabeticals.Where(x => x.CaseNumberString == min.ToString()).FirstOrDefault();
                var maxDeloO = caseAlphabeticals.Where(x => x.CaseNumberString == max.ToString()).FirstOrDefault();
                minDelo = minDeloO.ShortNumber + "/" + minDeloO.RegDateString;
                maxDelo = maxDeloO.ShortNumber + "/" + maxDeloO.RegDateString;
            }

            int i = 0;
            foreach (var item in caseAlphabeticals.OrderBy(x => x.FirstLetter).ThenBy(x => x.RegDate).ThenBy(x => x.Name))
            {
                colorType = !colorType;

                if (charFirstLetter != Char.ToUpper(item.Name[0]))
                {
                    if (i > 0)
                    {
                        excelService.SetRowBreak();
                    }
                    if (charFirstLetter != '\0')
                        rowMax = SetRowAndColumn(excelService, 0, rowMax + 1, rowMax);

                    colorType = false;
                    monthNum = item.RegDate.Month;
                    charFirstLetter = Char.ToUpper(item.Name[0]);
                    var title1 = item.CaseGroupLabel;
                    var title2 = "Азбучник за буква: " + charFirstLetter;
                    var title3 = "От номер дело: " + minDelo;
                    var title4 = "До номер дело: " + maxDelo;
                    rowMax = FillHeaderTable(excelService, style, rowMax, title1, title2, title3, title4);
                    rowMax = SetRowAndColumn(excelService, 0, rowMax + 1, rowMax);
                    FillHeaderMonth(excelService, style, item.RegDate);
                    rowMax = SetRowAndColumn(excelService, 0, rowMax + 1, rowMax);
                }

                if (monthNum != item.RegDate.Month)
                {
                    monthNum = item.RegDate.Month;
                    FillHeaderMonth(excelService, style, item.RegDate);
                    rowMax = SetRowAndColumn(excelService, 0, rowMax + 1, rowMax);
                }

                FillRow(item, excelService, (colorType) ? styleRow1 : styleRow2, rowMax, model.ReplaceEgn);
                rowMax = SetRowAndColumn(excelService, 0, rowMax + 1, rowMax);
                i++;
            }

            return excelService.ToArray();
        }

        private IQueryable<DismisalReportVM> DismisalReport_Select(int courtId, DismisalReportFilterVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CaseLawUnitDismisal, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.DismisalDate.Date >= dateFromSearch.Date && x.DismisalDate.Date <= dateToSearch.Date;

            Expression<Func<CaseLawUnitDismisal, bool>> regNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.NumberCase) == false)
                regNumberSearch = x => x.Case.RegNumber.ToLower().Contains(model.NumberCase.ToLower());

            Expression<Func<CaseLawUnitDismisal, bool>> lawUnitSearch = x => true;
            if ((model.LawUnitId ?? 0) > 0)
                lawUnitSearch = x => x.CaseLawUnit.LawUnitId == model.LawUnitId;

            var dismisalReportVMs = repo.AllReadonly<CaseLawUnitDismisal>()
                .Where(x => x.CourtId == courtId)
                .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(x.CaseLawUnit.JudgeRoleId))
                .Where(x => NomenclatureConstants.DismisalType.DismisalList.Contains(x.DismisalTypeId))
                .Where(dateSearch)
                .Where(regNumberSearch)
                .Where(lawUnitSearch)
                .Select(x => new DismisalReportVM()
                {
                    CaseSessionDate = x.CaseSessionAct.CaseSession.DateFrom,
                    CaseRegNumber = x.Case.CaseGroup.Label + " " + x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM"),
                    LawUnitName = x.CaseLawUnit.LawUnit.FullName,
                    Description = x.Description,
                    LawUnitNewName = String.Join("; ", repo.AllReadonly<CaseSelectionProtokol>()
                                                                              .Where(t => t.CaseLawUnitDismisalId == x.Id)
                                                                              .Select(t => t.SelectedLawUnit.FullName))
                })
                .AsQueryable();

            return dismisalReportVMs;
        }

        public byte[] DismisalReportToExcelOne(DismisalReportFilterVM model)
        {
            var dataRows = DismisalReport_Select(userContext.CourtId, model).OrderBy(x => x.CaseSessionDate).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Number = i + 1;
            }

            NPoiExcelService excelService = GetExcelHtmlTemplate("Dismisal");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<DismisalReportVM, object>>>()
                {
                    x => x.Number,
                    x => x.CaseSessionDate,
                    x => x.CaseRegNumber,
                    x => x.LawUnitName,
                    x => x.Description,
                    x => x.LawUnitNewName,
                }
            );
            return excelService.ToArray();
        }

        public IQueryable<PaymentPosReportVM> PaymentPosReport_Select(int courtId, PaymentPosFilterReportVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Payment, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.PaidDate.Date >= dateFromSearch.Date && x.PaidDate.Date <= dateToSearch.Date;

            Expression<Func<Payment, bool>> personSearch = x => true;
            if (string.IsNullOrEmpty(model.FullName) == false)
                personSearch = x => x.SenderName.ToLower().Contains(model.FullName.ToLower());

            Expression<Func<Payment, bool>> moneyGroupSearch = x => true;
            if (model.MoneyGroupId > 0)
                moneyGroupSearch = x => x.CourtBankAccount.MoneyGroupId == model.MoneyGroupId;

            return repo.AllReadonly<Payment>()
                .Where(x => x.CourtId == courtId && x.IsActive == true)
                .Where(x => x.PaymentTypeId == NomenclatureConstants.PaymentType.Pos)
                .Where(dateSearch)
                .Where(personSearch)
                .Where(moneyGroupSearch)
                .Select(x => new PaymentPosReportVM
                {
                    MoneyGroupId = x.CourtBankAccount.MoneyGroupId,
                    PaidDate = x.PaidDate,
                    PaidDateHour = x.PaidDate,
                    PaymentNumber = x.PaymentNumber,
                    Tid = x.PosPaymentResults.Where(p => p.Status == MoneyConstants.PosPaymentResultStatus.StatusOk)
                               .Select(p => p.Tid).FirstOrDefault() + " " + x.CourtBankAccount.MoneyGroup.Label,
                    SenderName = x.SenderName,
                    Description = string.Join("; ", x.ObligationPayments.Where(a => a.IsActive).Select(a => a.Obligation.MoneyType.Label).Distinct()) +
                                   " " + x.Description ?? "",
                    Amount = x.Amount,
                }).AsQueryable();
        }

        public byte[] PaymentPosReportToExcelOne(PaymentPosFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = PaymentPosReport_Select(userContext.CourtId, model).ToList();

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Преведени суми през ПОС-терминал по сметка Бюджет/депозит за период от " + dateFrom + " до " + dateTo, 7,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<PaymentPosReportVM, object>>>()
                {
                    x => x.PaidDate,
                    x => x.PaidDateHour,
                    x => x.PaymentNumber,
                    x => x.Tid,
                    x => x.SenderName,
                    x => x.Description,
                    x => x.Amount,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );
            excelService.AddRow();
            excelService.AddRow();

            excelService.AddRangeMoveCol("Всичко по сметка (Бюджетна) " +
                dataRows.Where(x => x.MoneyGroupId == NomenclatureConstants.MoneyGroups.Budget).Count() +
                " бр. транзакции на стойност " +
                dataRows.Where(x => x.MoneyGroupId == NomenclatureConstants.MoneyGroups.Budget).Select(x => x.Amount).Sum() +
                " лв.", 7, 1);

            excelService.AddRow();
            excelService.AddRangeMoveCol("Всичко по сметка (Депозитна) " +
                dataRows.Where(x => x.MoneyGroupId == NomenclatureConstants.MoneyGroups.Deposit).Count() +
                " бр. транзакции на стойност " +
                dataRows.Where(x => x.MoneyGroupId == NomenclatureConstants.MoneyGroups.Deposit).Select(x => x.Amount).Sum() +
                " лв.", 7, 1);

            return excelService.ToArray();
        }

        private IQueryable<CaseObligationReportVM> CaseObligationReport_Select(int courtId, CaseObligationFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Obligation, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.ObligationDate.Date >= dateFromSearch.Date && x.ObligationDate.Date <= dateToSearch.Date;

            Expression<Func<Obligation, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<Obligation, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.CaseSessionAct.CaseSession.Case.CaseLawUnits
                          .Where(a => (a.DateTo ?? dateEnd).Date >= x.ObligationDate.Date && a.CourtDepartmentId == model.DepartmentId).Any();

            return repo.AllReadonly<Obligation>()
                .Where(x => x.CourtId == courtId)
                .Where(x => (x.IsActive ?? true) == true)
                .Where(x => x.CaseSessionActId != null)
                .Where(x => x.MoneySign == NomenclatureConstants.MoneySign.SignPlus)
                .Where(x => x.ObligationReceives.Where(a => a.ExecListTypeId == NomenclatureConstants.ExecListTypes.ThirdPerson).Any() == false)
                .Where(dateSearch)
                .Where(caseGroupWhere)
                .Where(departmentWhere)
                .Select(x => new CaseObligationReportVM
                {
                    CaseData = x.Case.CaseType.Code + " " +
                                x.Case.RegNumber,
                    PersonName = x.FullName + " " + (x.Person_SourceType == SourceTypeSelectVM.CasePerson ? repo.AllReadonly<CasePerson>()
                                                  .Where(a => a.Id == x.Person_SourceId)
                                                  .Select(a => a.PersonRole.Label).DefaultIfEmpty("").FirstOrDefault() :
                                                  repo.AllReadonly<CaseLawUnit>()
                                                  .Where(a => a.Id == x.Person_SourceId)
                                                  .Select(a => a.JudgeRole.Label).DefaultIfEmpty("").FirstOrDefault()),
                    ObligationDate = x.ObligationDate,
                    ObligationDateData = x.ObligationDate.ToString("dd.MM.yyyy") + Environment.NewLine +
                                         x.CaseSessionAct.CaseSession.SessionType.Label + " от " +
                                          x.CaseSessionAct.CaseSession.DateFrom.ToString("dd.MM.yyyy") +
                                         Environment.NewLine +
                                         x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.RegNumber +
                                         (x.CaseSessionAct.ActDate != null ? ("/" + ((DateTime)x.CaseSessionAct.ActDate).ToString("dd.MM.yyyy")) : ""),
                    Description = ((x.Description ?? "") + " " + (x.MoneyFineType.Label ?? "")).Trim(),
                    Amount = x.Amount,
                    AmountPay = x.ObligationPayments.Where(a => a.IsActive && a.Payment.IsActive)
                               .Select(a => a.Amount).Sum(),
                    IsActive = x.IsActive ?? true
                }).AsQueryable();
        }

        public byte[] CaseObligationReportToExcelOne(CaseObligationFilterReportVM model)
        {
            var dataRows = CaseObligationReport_Select(userContext.CourtId, model).OrderBy(x => x.ObligationDate).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }

            NPoiExcelService excelService = GetExcelHtmlTemplate("Obligation", model.CaseGroupId);
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<CaseObligationReportVM, object>>>()
                {
                    x => x.Index,
                    x => x.CaseData,
                    x => x.PersonName,
                    x => x.ObligationDateData,
                    x => x.Amount,
                    x => x.DescriptionText,
                    x => x.Signature,
                }
            );
            return excelService.ToArray();
        }

        public IQueryable<FineReportVM> FineReport_Select(int courtId, FineFilterReportVM model)
        {
            Expression<Func<Obligation, bool>> groupWhere = x => true;
            if (model.CaseGroupId > 0)
                groupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<Obligation, bool>> typeWhere = x => true;
            if (model.CaseTypeId > 0)
                typeWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Obligation, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.ObligationDate.Date >= dateFromSearch.Date && x.ObligationDate.Date <= dateToSearch.Date;

            return repo.AllReadonly<Obligation>()
                .Where(x => x.CourtId == courtId)
                .Where(x => (x.IsActive ?? true) == true)
                .Where(x => x.MoneyTypeId == NomenclatureConstants.MoneyType.Fine)
                .Where(dateSearch)
                .Where(groupWhere)
                .Where(typeWhere)
                .Select(x => new FineReportVM
                {
                    Id = x.Id,
                    CaseId = x.Case.Id,
                    CaseGroupName = x.Case.CaseGroup.Label,
                    CaseNumber = x.Case.RegNumber,
                    SessionTypeName = x.CaseSessionAct.CaseSession.SessionType.Label,
                    ObligationDate = x.ObligationDate,
                    SenderName = x.FullName,
                    Amount = x.Amount,
                    AmountPay = x.ObligationPayments.Where(a => a.IsActive && a.Payment.IsActive)
                               .Select(a => a.Amount).Sum(),
                    PaidDate = string.Join("; ", x.ObligationPayments.Where(a => a.IsActive && a.Payment.IsActive && a.Amount > 0)
                               .Select(a => a.Payment.PaidDate.ToString("dd.MM.yyyy")).Distinct()),
                    Description = ((x.Description ?? "") + " " + (x.MoneyFineType.Label ?? "")).Trim()
                }).AsQueryable();
        }

        public byte[] FineReportToExcelOne(FineFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = FineReport_Select(userContext.CourtId, model).ToList();

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка за глоби за период от " + dateFrom + " до " + dateTo, 9,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 10000, 5000, 5000, 5000, 15000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<FineReportVM, object>>>()
                {
                    x => x.CaseGroupName,
                    x => x.CaseNumber,
                    x => x.SessionTypeName,
                    x => x.ObligationDate,
                    x => x.SenderName,
                    x => x.Amount,
                    x => x.PaidDate,
                    x => x.State,
                    x => x.Description,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );
            excelService.AddRow();
            excelService.AddRow();
            excelService.AddRangeMoveCol(dataRows.Count + " бр. записи отговарящи на зададените критерии.", 2, 1);

            return excelService.ToArray();
        }


        public IQueryable<StateFeeReportVM> StateFeeReport_Select(int courtId, StateFeeFilterReportVM model)
        {
            Expression<Func<Obligation, bool>> groupWhere = x => true;
            if (model.CaseGroupId > 0)
                groupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<Obligation, bool>> typeWhere = x => true;
            if (model.CaseTypeId > 0)
                typeWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Obligation, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.ObligationDate.Date >= dateFromSearch.Date && x.ObligationDate.Date <= dateToSearch.Date;

            Expression<Func<Obligation, bool>> documentGroupWhere = x => true;
            if (model.DocumentGroupId > 0)
                documentGroupWhere = x => x.Document.DocumentGroupId == model.DocumentGroupId;

            Expression<Func<Obligation, bool>> documentTypeWhere = x => true;
            if (model.DocumentTypeId > 0)
                documentTypeWhere = x => x.Document.DocumentTypeId == model.DocumentTypeId;

            return repo.AllReadonly<Obligation>()
                .Where(x => x.CourtId == courtId)
                .Where(x => (x.IsActive ?? true) == true)
                .Where(x => x.MoneyTypeId == NomenclatureConstants.MoneyType.StateFee)
                .Where(dateSearch)
                .Where(groupWhere)
                .Where(typeWhere)
                .Where(documentGroupWhere)
                .Where(documentTypeWhere)
                .Select(x => new StateFeeReportVM
                {
                    Id = x.Id,
                    CaseId = x.CaseId,
                    ExistCase = x.CaseId != null,
                    DocumentData = x.CaseId != null ? (x.Case.CaseGroup.Label + " " + (x.Case.RegNumber ?? "")) :
                                    (x.Document.DocumentType.Label + " " + x.Document.DocumentNumber + "/" +
                                    x.Document.DocumentDate.ToString("dd.MM.yyyy")),
                    PaymentData = string.Join("; ", x.ObligationPayments.Where(a => a.IsActive && a.Payment.IsActive)
                                  .Select(a => a.Payment.PaymentType.Label + " " + a.Payment.PaymentNumber + "/" +
                                      a.Payment.PaidDate.ToString("dd.MM.yyyy")).Distinct()),
                    CaseTypeCode = x.Case.CaseType.Code,
                    ObligationDate = x.ObligationDate,
                    SenderName = x.FullName,
                    Amount = x.Amount,
                    PaidDate = string.Join("; ", x.ObligationPayments.Where(a => a.IsActive && a.Payment.IsActive && a.Amount > 0)
                               .Select(a => a.Payment.PaidDate.ToString("dd.MM.yyyy")).Distinct()),
                    PaymentDescription = string.Join("; ", x.ObligationPayments.Where(a => a.IsActive && a.Payment.IsActive && a.Amount > 0)
                               .Select(a => a.Payment.PaymentInfo).Distinct()),
                    Description = x.MoneyType.Label + " " + (x.MoneyFeeType.Label ?? "") + "; " + (x.Description ?? ""),
                }).AsQueryable();
        }

        public byte[] StateFeeReportExportExcel(StateFeeFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = StateFeeReport_Select(userContext.CourtId, model).ToList();

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка за държавни такси за период от " + dateFrom + " до " + dateTo, 9,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 10000, 5000, 5000, 5000, 15000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<StateFeeReportVM, object>>>()
                {
                    x => x.DocumentData,
                    x => x.PaymentData,
                    x => x.CaseTypeCode,
                    x => x.ObligationDate,
                    x => x.SenderName,
                    x => x.Amount,
                    x => x.PaidDate,
                    x => x.PaymentDescription,
                    x => x.Description,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );
            excelService.AddRow();
            excelService.AddRow();
            excelService.AddRangeMoveCol(dataRows.Count + " бр. записи отговарящи на зададените критерии.", 2, 1);

            return excelService.ToArray();
        }
        public IQueryable<ObligationJuryReportVM> ObligationJuryReport_Select(int courtId, ObligationJuryFilterReportVM model)
        {
            int[] moneyType = { NomenclatureConstants.MoneyType.Transport, NomenclatureConstants.MoneyType.Earnings,
                NomenclatureConstants.MoneyType.EarningsDeposit };

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Obligation, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.ObligationDate.Date >= dateFromSearch.Date && x.ObligationDate.Date <= dateToSearch.Date;

            Expression<Func<Obligation, bool>> groupWhere = x => true;
            if (model.CaseGroupId > 0)
                groupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<Obligation, bool>> typeWhere = x => true;
            if (model.CaseTypeId > 0)
                typeWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            Expression<Func<Obligation, bool>> sessionWhere = x => true;
            if (model.SessionTypeId > 0)
                sessionWhere = x => (x.CaseSessionId != null ? x.CaseSession.SessionTypeId : x.CaseSessionAct.CaseSessionId) == model.SessionTypeId;

            Expression<Func<Obligation, bool>> moneyGroupWhere = x => true;
            if (model.MoneyGroupId > 0)
                moneyGroupWhere = x => x.MoneyType.MoneyGroupId == model.MoneyGroupId;

            Expression<Func<Obligation, bool>> onlyJuryWhere = x => true;
            if (model.OnlyJury)
                onlyJuryWhere = x => ((x.IsForMinAmount ?? false) || (x.Person_SourceType == SourceTypeSelectVM.CaseLawUnit &&
                                      repo.AllReadonly<CaseLawUnit>()
                                       .Where(a => a.Id == x.Person_SourceId)
                                       .Where(a => a.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
                                       .Any()
                                      ));

            return repo.AllReadonly<Obligation>()
                .Where(x => x.CourtId == courtId)
                .Where(x => (x.IsActive ?? true) == true)
                .Where(x => moneyType.Contains(x.MoneyTypeId))
                .Where(dateSearch)
                .Where(groupWhere)
                .Where(typeWhere)
                .Where(sessionWhere)
                .Where(moneyGroupWhere)
                .Where(onlyJuryWhere)
                .Select(x => new ObligationJuryReportVM
                {
                    CaseId = x.Case.Id,
                    CaseTypeName = x.Case.CaseType.Label,
                    CaseNumber = x.Case.RegNumber,
                    SessionTypeName = x.CaseSessionId != null ? x.CaseSession.SessionType.Label : x.CaseSessionAct.CaseSession.SessionType.Label,
                    SessionDate = (x.CaseSessionId == null && x.CaseSessionActId == null) ? (DateTime?)null :
                                   (x.CaseSessionId != null ? x.CaseSession.DateFrom.Date : x.CaseSessionAct.CaseSession.DateFrom.Date),
                    PersonName = x.FullName,
                    Uic = x.Uic,
                    ObligationDate = x.ObligationDate.Date,
                    Amount = NomenclatureConstants.MoneyType.EarningList.Contains(x.MoneyTypeId) ? x.Amount : 0,
                    AmountTransport = x.MoneyTypeId == NomenclatureConstants.MoneyType.Transport ? x.Amount : 0,
                    AmountPayment = x.ObligationPayments.Where(p => p.IsActive == true).Select(p => p.Amount).DefaultIfEmpty(0).Sum(),
                    Description = x.IsForMinAmount == true ? x.ObligationInfo : x.Description,
                    MoneyGroupName = x.MoneyType.MoneyGroup.Label,
                    ExpenseOrderDates = x.ExpenseOrderObligations.Where(a => a.ExpenseOrder.IsActive == true)
                                                                 .Select(a => a.ExpenseOrder.RegDate.ToString("dd.MM.yyyy"))
                                                                 .FirstOrDefault()
                })
                .GroupBy(x => new
                {
                    x.CaseTypeName,
                    x.CaseNumber,
                    x.SessionTypeName,
                    x.SessionDate,
                    x.ObligationDate,
                    x.PersonName,
                    x.Uic,
                    x.MoneyGroupName,
                    x.CaseId
                })
                                .Select(g => new ObligationJuryReportVM
                                {
                                    CaseId = g.Key.CaseId,
                                    ExistCase = g.Key.CaseId != null,
                                    CaseTypeName = g.Key.CaseTypeName,
                                    CaseNumber = g.Key.CaseNumber,
                                    SessionTypeName = g.Key.SessionTypeName,
                                    SessionDate = g.Key.SessionDate,
                                    PersonName = g.Key.PersonName,
                                    ObligationDate = g.Key.ObligationDate,
                                    Amount = g.Sum(x => x.Amount),
                                    AmountTransport = g.Sum(x => x.AmountTransport),
                                    AmountPayment = g.Sum(x => x.AmountPayment),
                                    Description = g.Max(x => x.Description),
                                    MoneyGroupName = g.Key.MoneyGroupName,
                                    ExpenseOrderDates = string.Join("; ",
                                     g.Where(x => string.IsNullOrEmpty(x.ExpenseOrderDates) == false)
                                     .Select(x => x.ExpenseOrderDates)
                                     .Distinct())
                                })
                .AsQueryable();
        }

        public byte[] ObligationJuryReportToExcelOne(ObligationJuryFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = ObligationJuryReport_Select(userContext.CourtId, model).ToList();

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка за възнаграждения за периода от " + dateFrom + " до " + dateTo, 12,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<ObligationJuryReportVM, object>>>()
                {
                    x => x.CaseTypeName,
                    x => x.CaseNumber,
                    x => x.SessionTypeName,
                    x => x.SessionDate,
                    x => x.PersonName,
                    x => x.ObligationDate,
                    x => x.MoneyGroupName,
                    x => x.Amount,
                    x => x.AmountTransport,
                    x => x.Status,
                    x => x.Description,
                    x => x.ExpenseOrderDates,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }
        private IQueryable<DeliveryBookVM> DeliveryBook_Select(int courtId, DeliveryBookFilterVM model)
        {
            int[] institutionTypeIds = { NomenclatureConstants.InstitutionTypes.Attourney };
            Expression<Func<Document, bool>> typePersonWhere = x => true;
            typePersonWhere = x => (x.DocumentPersons.Where(p => p.Person_SourceType == SourceTypeSelectVM.Court).Any() ||
                                     (x.DocumentPersons.Where(p => p.Person_SourceType == SourceTypeSelectVM.Instutution &&
                                                repo.AllReadonly<Institution>().Where(i => i.Id == p.Person_SourceId &&
                                                                                       institutionTypeIds.Contains(i.InstitutionTypeId)).Any()
                                                             ).Any()
                                     )
                                   );

            Expression<Func<Document, bool>> personNameWhere = x => true;
            if (string.IsNullOrEmpty(model.CasePersonName) == false)
                personNameWhere = x => x.DocumentPersons.Where(p => p.FullName.ToUpper().Contains(model.CasePersonName.ToUpper())).Any();

            Expression<Func<Document, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.DocumentCaseInfo.Where(p => p.Case.CaseGroupId == model.CaseGroupId).Any();

            Expression<Func<Document, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.DocumentCaseInfo.Where(p => p.Case.CaseTypeId == model.CaseTypeId).Any();

            var result = repo.AllReadonly<Document>()
                                .Where(x => x.CourtId == courtId && x.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                                .Where(x => x.DocumentDate.Date >= model.DateFrom.Date && x.DocumentDate.Date <= model.DateTo.Date)
                                .Where(x => x.DateExpired == null)
                                .Where(typePersonWhere)
                                .Where(personNameWhere)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Select(x => new DeliveryBookVM
                                {
                                    Id = x.Id,
                                    DocumentNumber = x.DocumentNumber,
                                    DocumentDate = x.DocumentDate,
                                    DocumentGroupName = x.DocumentGroup.Label + Environment.NewLine + x.DocumentType.Label,
                                    Description = x.Description,
                                    DocumentPersonName = String.Join(Environment.NewLine, x.DocumentPersons.Select(p => p.FullName)),
                                    DocumentNumberValue = (x.DocumentNumberValue ?? 0),
                                    DocumentLinkDate = repo.AllReadonly<DocumentLink>().Include(d => d.Document)
                                              .Where(d => d.CourtId == x.CourtId)
                                              .Where(d => d.PrevDocumentNumber == x.DocumentNumber)
                                              .Where(d => (d.PrevDocumentDate ?? DateTime.Now).Date == x.DocumentDate.Date)
                                              .Where(d => d.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                                              .Select(d => (DateTime?)d.Document.DocumentDate).FirstOrDefault(),
                                    DocumentLinkUser = repo.AllReadonly<DocumentLink>().Include(d => d.Document)
                                              .Include(d => d.Document.User)
                                              .Include(d => d.Document.User.LawUnit)
                                              .Where(d => d.CourtId == x.CourtId)
                                              .Where(d => d.PrevDocumentNumber == x.DocumentNumber)
                                              .Where(d => (d.PrevDocumentDate ?? DateTime.Now).Date == x.DocumentDate.Date)
                                              .Where(d => d.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                                              .Select(d => d.Document.User.LawUnit.FullName).FirstOrDefault(),
                                    CaseGroupName = x.DocumentCaseInfo.Select(d => d.Case.CaseGroup.Label).FirstOrDefault(),
                                    CaseNumber = x.DocumentCaseInfo.Select(d => d.Case.RegNumber).FirstOrDefault(),
                                    CaseDate = x.DocumentCaseInfo.Select(d => (DateTime?)d.Case.RegDate).FirstOrDefault()
                                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        public byte[] DeliveryBookReportToExcelOne(DeliveryBookFilterVM model)
        {
            var dataRows = DeliveryBook_Select(userContext.CourtId, model).OrderBy(x => x.DocumentNumberValue).ToList();

            NPoiExcelService excelService = GetExcelHtmlTemplate("DeliveryBook");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<DeliveryBookVM, object>>>()
                {
                    x => x.DocumentNumberDate,
                    x => x.DocumentPersonName,
                    x => x.DocumentGroupName,
                    x => x.DocumentLinkDate,
                    x => x.DocumentLinkUser,
                    x => x.CaseGroupName,
                    x => x.CaseNumber,
                    x => x.CaseDate,
                }
            );
            return excelService.ToArray();
        }
        private IQueryable<CaseSessionPrivateReportVM> CaseSessionPrivateReport_Select(int courtId, CaseSessionPrivateFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseSession, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseSession, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom && a.CourtDepartmentId == model.DepartmentId).Any();

            var result = repo.AllReadonly<CaseSession>()
                                .Where(x => x.Case.CourtId == courtId && x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PrivateSession)
                                .Where(x => x.DateFrom >= model.DateFrom && x.DateFrom <= model.DateTo)
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                                .Where(caseGroupWhere)
                                .Where(departmentWhere)
                                .Where(x => x.CaseSessionActs.Where(a => NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.ActStateId) && a.DateExpired == null).Any())
                                .Select(x => new CaseSessionPrivateReportVM
                                {
                                    CaseNumber = x.Case.RegNumber,
                                    CompartmentName = x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom && a.CourtDepartmentId != null)
                                                              .Select(a => a.CourtDepartment.Label).FirstOrDefault() + Environment.NewLine +
                                                              string.Join(Environment.NewLine, x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom &&
                                                                  NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(a.JudgeRoleId))
                                                              .Select(a => a.LawUnit.FullName)),
                                    JudgeReporterName = x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom &&
                                                                  a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                              .Select(a => a.LawUnit.FullName).FirstOrDefault(),
                                    acts = x.CaseSessionActs.Where(p => p.DateExpired == null && p.RegDate != null).Select(p => new CaseSessionPrivateAct
                                    {
                                        ActDate = (DateTime)p.ActDate,
                                        ActNumber = p.RegNumber,
                                        ActTypeName = p.ActType.Label,
                                        ActDescription = p.Description,
                                        ActDeclaredDate = p.ActDeclaredDate
                                    }),
                                    CaseSessionResultName = x.DateFrom.ToString("dd.MM.yyyy") + Environment.NewLine +
                                                           String.Join(Environment.NewLine, x.CaseSessionResults.Where(a => a.IsMain && a.IsActive).Select(p => p.SessionResult.Label)),
                                    ActDateOrder = x.CaseSessionActs.Where(p => p.RegNumber != null && p.ActDate != null && p.DateExpired == null)
                                                                    .Select(p => p.ActDate).FirstOrDefault(),
                                    CaseGroupId = x.Case.CaseGroupId,
                                    SessionStateId = x.SessionStateId,
                                    ActEnforcedFinal = x.CaseSessionActs.Where(a => a.IsFinalDoc == true &&
                                                       NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.ActStateId) && a.DateExpired == null).Any(),
                                    ActEnforcedNoFinal = x.CaseSessionActs.Where(a => a.IsFinalDoc == false &&
                                                       NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.ActStateId) && a.DateExpired == null).Any(),
                                    CaseTypeId = x.Case.CaseTypeId,
                                    CaseSessionResult = string.Join(",", x.CaseSessionResults.Where(a => a.IsMain && a.IsActive).Select(a => a.SessionResultId)),
                                    CaseCodeId = x.Case.CaseCodeId ?? 0,
                                    CaseInstanceId = x.Case.CaseType.CaseInstanceId,
                                    DocumentTypeId = x.Case.Document.DocumentTypeId
                                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        private void AddTextCountToExcel(NPoiExcelService excelService, string text, int count, int countCol)
        {
            excelService.AddRange(text, countCol); excelService.colIndex += countCol;
            excelService.AddRange(count.ToString(), 1); excelService.AddRow();
        }

        private void CaseSessionPrivateAddTextCountToExcel(NPoiExcelService excelService, string text, int count)
        {
            AddTextCountToExcel(excelService, text, count, 3);
        }

        private string[] GetComplain(List<CaseSessionActComplainResult> complainResults)
        {
            return complainResults.Select(x => x.CaseSessionAct.CaseSession.SessionType.Label + " " + x.CaseSessionAct.CaseSession.DateFrom.ToString("dd.MM.yyyy") +
                       Environment.NewLine + x.CaseSessionAct.ActType.Label + " " +
                       x.CaseSessionAct.RegNumber + "/" + ((DateTime)x.CaseSessionAct.RegDate).ToString("dd.MM.yyyy") +
                       Environment.NewLine + (x.Description ?? "")
                       )
                       .Distinct()
                       .ToArray();
        }

        private List<InsolvencyReportVM> InsolvencyReport_Select(int courtId, InsolvencyFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> yearSearch = x => true;
            if ((model.CaseYear ?? 0) > 0)
                yearSearch = x => x.RegDate.Year == model.CaseYear;

            var documentTypes = repo.AllReadonly<DocumentTypeGrouping>()
                             .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.Insolvency)
                             .Select(x => x.DocumentTypeId)
                             .ToList();

            int[] personRoles = { NomenclatureConstants.PersonRole.Debtor, NomenclatureConstants.PersonRole.Kreditor,
                                 NomenclatureConstants.PersonRole.KomitetKreditor, NomenclatureConstants.PersonRole.Sindik};

            var caseModel = repo.AllReadonly<Case>()
                       .Where(x => x.CourtId == courtId)
                       .Where(x => x.RegNumber == model.CaseNumber)
                       .Where(x => documentTypes.Contains(x.Document.DocumentTypeId))
                       .Where(caseGroupWhere)
                       .Where(yearSearch)
                       .Select(x => new InsolvencyCaseReportVM
                       {
                           Id = x.Id,
                           CaseGroupId = x.CaseGroupId,
                           DocumentId = x.DocumentId,
                           RegNumber = x.RegNumber,
                           DebtorName = string.Join(Environment.NewLine, x.CasePersons.Where(a => a.DateExpired == null &&
                                                   a.CaseSessionId == null &&
                                                   a.PersonRoleId == NomenclatureConstants.PersonRole.Debtor)
                                                    .Select(a => a.FullName + ", " + a.Addresses.Select(z => z.Address.FullAddress)
                                                    .FirstOrDefault())),
                       })
                       .FirstOrDefault();
            if (caseModel == null)
            {
                return new List<InsolvencyReportVM>();
            }

            List<InsolvencyReportVM> result = new List<InsolvencyReportVM>();

            var caseLawUnits = repo.AllReadonly<CaseLawUnit>()
                                   .Include(x => x.LawUnit)
                                   .Include(x => x.CourtDepartment)
                                   .Where(x => x.CaseId == caseModel.Id)
                                   .Where(x => !NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId))
                                   .ToList();

            var acts = repo.AllReadonly<CaseSessionAct>()
                                .Include(x => x.CaseSession)
                                .Include(x => x.ActType)
                                .Where(x => x.CaseSession.CaseId == caseModel.Id && x.DateExpired == null)
                                .Where(x => NomenclatureConstants.SessionActState.EnforcedStates.Contains(x.ActStateId))
                                .ToList();

            //Всички жаления за делото
            var actsComplainResult = repo.AllReadonly<CaseSessionActComplainResult>()
                                     .Include(x => x.CaseSessionActComplain)
                                     .Include(x => x.CaseSessionAct)
                                     .Include(x => x.ComplainCourt)
                                     .Include(x => x.CaseSessionAct.CaseSession)
                                     .Include(x => x.CaseSessionAct.CaseSession.SessionType)
                                     .Include(x => x.CaseSessionAct.ActResult)
                                     .Include(x => x.CaseSessionAct.ActType)
                                     .Where(x => x.CaseId == caseModel.Id)
                                     .ToList();

            //Четат се заседанията
            result.AddRange(repo.AllReadonly<CaseSession>()
                                .Where(x => x.CaseId == caseModel.Id)
                                .Where(x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                                .Where(x => x.DateFrom.Date >= model.DateFrom.Date && x.DateFrom.Date <= model.DateTo.Date)
                                .Select(x => new InsolvencyReportVM
                                {
                                    SessionId = x.Id,
                                    SessionDate = x.DateFrom,
                                    SessionData = x.SessionType.Label + " " + x.DateFrom.ToString("dd.MM.yyyy"),
                                    PersonNames = string.Join(Environment.NewLine, x.CasePersons.Where(a => a.DateExpired == null &&
                                                            personRoles.Contains(a.PersonRoleId)).Select(a => a.FullName + "(" + a.PersonRole.Label + ")")),
                                    SessionDocumentIds = x.CaseSessionDocs.Select(a => a.DocumentId).ToList()
                                }).ToList());

            foreach (var item in result)
            {
                item.DateOrder1 = item.SessionDate;
                item.DateOrder2 = item.SessionDate;

                var sessionActs = acts.Where(x => x.CaseSessionId == item.SessionId && x.ActDeclaredDate != null).ToList();
                item.SessionActData = string.Join("; ", sessionActs
                                                       .Select(a => a.ActType.Label + " №" +
                                                       a.RegNumber + "/" + ((DateTime)a.ActDeclaredDate).ToString("dd.MM.yyyy") +
                                                 Environment.NewLine + a.Description));
                item.Acts = item.SessionData + Environment.NewLine + item.SessionActData;
            }

            //Четат се съпровождащите документи
            var resultDocument = repo.AllReadonly<Document>()
                                .Where(x => x.DocumentCaseInfo.Where(a => a.CaseId == caseModel.Id).Any())
                                .Where(x => x.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                                .Where(x => x.DocumentDate.Date >= model.DateFrom.Date && x.DocumentDate.Date <= model.DateTo.Date)
                                .Where(x => x.DateExpired == null)
                                .Select(x => new InsolvencyReportVM
                                {
                                    DocumentId = x.Id,
                                    SessionDate = x.DocumentDate,
                                    DocumentDate = x.DocumentDate,
                                    DocumentTypeName = x.DocumentType.Label +
                                    " входящ документ №" + x.DocumentNumber + "/" + x.DocumentDate.Year,
                                    PersonNames = string.Join(Environment.NewLine, x.DocumentPersons
                                     .Select(a => a.FullName + "(" + a.PersonRole.Label + ")")),
                                    DocumentActId = x.DocumentCaseInfo.Select(a => a.SessionActId).FirstOrDefault(),
                                }).ToList();

            foreach (var item in resultDocument)
            {
                //Ако един съпровождащ документ е насочен към акт, то трябва да излезе под заседанието в който е този акт
                item.DateOrder1 = item.SessionDate;
                if (item.DocumentActId != null)
                {
                    var act = acts.Where(x => x.Id == (item.DocumentActId ?? 0)).FirstOrDefault();
                    var sessionResult = result.Where(x => x.SessionId == act.CaseSessionId).FirstOrDefault();
                    if (sessionResult != null)
                    {
                        item.DateOrder1 = sessionResult.SessionDate;
                    }

                    var actsApeal = actsComplainResult.Where(x => x.CaseSessionActComplain.CaseSessionActId == item.DocumentActId &&
                             x.CaseSessionActComplain.ComplainDocumentId == item.DocumentId &&
                             x.ComplainCourt.CourtTypeId == NomenclatureConstants.CourtType.Apeal)
                             .ToList();
                    item.ActsApeal = string.Join(Environment.NewLine,
                             GetComplain(actsApeal)
                             );

                    var actIdsForVKS = actsApeal.Select(x => x.CaseSessionActId).ToList();
                    actIdsForVKS.Add(item.DocumentActId ?? 0);
                    item.ActsVKS = string.Join(Environment.NewLine,
                             GetComplain(actsComplainResult.Where(x => actIdsForVKS.Contains(x.CaseSessionActComplain.CaseSessionActId) &&
                             x.ComplainCourt.CourtTypeId == NomenclatureConstants.CourtType.VKS)
                             .ToList())
                             );
                }
                item.DateOrder2 = item.SessionDate;
                item.Acts = string.Join(Environment.NewLine,
                                   result.Where(x => x.SessionDocumentIds.Contains(item.DocumentId ?? 0)).Select(x => x.SessionActData));
            }

            result.AddRange(resultDocument);

            //Добавяне на входящия документ
            var documentIn = repo.AllReadonly<Document>()
                 .Where(x => x.Id == caseModel.DocumentId)
                 .Select(x => new InsolvencyReportVM()
                 {
                     DateOrder1 = DateTime.Now.AddYears(-100),
                     DocumentDate = x.DocumentDate,
                     SessionDate = x.DocumentDate,
                     DocumentTypeName = x.DocumentType.Label + " №" + x.DocumentNumber + "/" + x.DocumentDate.Year,
                     PersonNames = string.Join(Environment.NewLine, x.DocumentPersons
                                     .Select(a => a.FullName + "(" + a.PersonRole.Label + ")"))
                 })
                 .FirstOrDefault();
            result.Add(documentIn);


            int i = 1;
            foreach (var item in result.OrderBy(x => x.DateOrder1).ThenBy(x => x.DateOrder2).ToList())
            {
                item.NumberAction = i; i++;
                item.CaseNumber = caseModel.RegNumber;
                item.CaseGroupId = caseModel.CaseGroupId;
                item.JudgeReporterName = caseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= item.SessionDate && (a.CaseSessionId ?? 0) == (item.SessionId ?? 0) &&
                                                                  a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                              .Select(a => a.LawUnit.FullName + " " +
                                                              (a.CourtDepartmentId == null ? "" : a.CourtDepartment.Label)).FirstOrDefault();
                item.DebtorName = caseModel.DebtorName;
            }

            return result.OrderBy(x => x.NumberAction).ToList();
        }

        public byte[] InsolvencyReportToExcelOne(InsolvencyFilterReportVM model)
        {
            var dataRows = InsolvencyReport_Select(userContext.CourtId, model);

            var htmlTemplate = GetHtmlTemplate("Insolvency");
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);

            if (dataRows.Count > 0)
            {
                excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 1;
                excelService.SetCellData("КНИГА ПО ЧЛ. 634В ОТ ТЗ " + CaseGroupCaption_Title(dataRows[0].CaseGroupId));
            }

            excelService.colIndex = 0;
            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;

            excelService.InsertList(
                dataRows,
                new List<Expression<Func<InsolvencyReportVM, object>>>()
                {
                    x => x.CaseNumber,
                    x => x.JudgeReporterName,
                    x => x.DebtorName,
                    x => x.NumberAction,
                    x => x.DocumentDate,
                    x => x.SessionDate,
                    x => x.DocumentTypeName,
                    x => x.PersonNames,
                    x => x.Acts,
                    x => x.ActsApeal,
                    x => x.ActsVKS
                }
            );
            return excelService.ToArray();
        }

        private IQueryable<ZzdnReportVM> ZzdnReport_Select(int courtId, ZzdnFilterReportVM model)
        {
            int fromNumberSearch = model.FromCaseNumber == null ? 1 : (int)model.FromCaseNumber;
            int toNumberSearch = model.ToCaseNumber == null ? int.MaxValue : (int)model.ToCaseNumber;

            Expression<Func<Document, bool>> yearSearch = x => true;
            if ((model.CaseYear ?? 0) > 0)
                yearSearch = x => x.Cases.Where(a => a.RegDate.Year == model.CaseYear).Any();

            Expression<Func<Document, bool>> numberCaseSearch = x => true;
            if (model.FromCaseNumber != null || model.ToCaseNumber != null)
                numberCaseSearch = x => x.Cases.Where(a => a.ShortNumberValue >= fromNumberSearch && a.ShortNumberValue <= toNumberSearch).Any();

            DateTime dateFromSearch = model.FromDateDocument == null ? DateTime.Now.AddYears(-100) : (DateTime)model.FromDateDocument;
            DateTime dateToSearch = model.ToDateDocument == null ? DateTime.Now.AddYears(100) : (DateTime)model.ToDateDocument;

            Expression<Func<Document, bool>> dateSearch = x => true;
            if (model.FromDateDocument != null || model.ToDateDocument != null)
                dateSearch = x => x.DocumentDate.Date >= dateFromSearch.Date && x.DocumentDate.Date <= dateToSearch.Date;

            var documentTypes = repo.AllReadonly<DocumentTypeGrouping>()
                             .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.Zzdn)
                             .Select(x => x.DocumentTypeId)
                             .ToList();

            DateTime dateEnd = DateTime.Now.AddDays(1);
            var result = repo.AllReadonly<Document>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => documentTypes.Contains(x.DocumentTypeId))
                                .Where(x => x.DateExpired == null)
                                .Where(yearSearch)
                                .Where(numberCaseSearch)
                                .Where(dateSearch)
                                .OrderBy(x => x.DocumentDate)
                                .Select(x => new ZzdnReportVM
                                {
                                    Id = x.Id,
                                    DocumentData = x.DocumentNumber + "/ " + x.DocumentDate.ToString("dd.MM.yyyy"),
                                    CaseNumber = x.Cases.Select(a => a.RegNumber +
                                                     a.CaseLawUnits.Where(b => (b.DateTo ?? dateEnd).Date >= DateTime.Now.Date &&
                                                               b.CaseSessionId == null && b.CourtDepartmentId != null)
                                                              .Select(b => Environment.NewLine + b.CourtDepartment.Label)
                                                              .DefaultIfEmpty("")
                                                              .FirstOrDefault())
                                                   .FirstOrDefault(),
                                    CasePersons = x.Cases.Select(a => string.Join(Environment.NewLine, a.CasePersons
                                                                  .Where(p => p.DateExpired == null && p.CaseSessionId == null)
                                                                  .Select(p => p.FullName + "(" + p.PersonRole.Label + ")"))).FirstOrDefault()
                                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        public byte[] ZzdnReportToExcelOne(ZzdnFilterReportVM model)
        {
            var dataRows = ZzdnReport_Select(userContext.CourtId, model).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }

            NPoiExcelService excelService = GetExcelHtmlTemplate("Zzdn");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<ZzdnReportVM, object>>>()
                {
                    x => x.Index,
                    x => x.DocumentData,
                    x => x.CaseNumber,
                    x => x.CasePersons
                }
            );
            return excelService.ToArray();
        }

        private IQueryable<EuropeanHeritageReportVM> EuropeanHeritageReport_Select(int courtId, EuropeanHeritageFilterReportVM model)
        {
            Expression<Func<Case, bool>> groupSearch = x => true;
            if (model.CaseGroupId > 0)
                groupSearch = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> regNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.RegNumber) == false)
                regNumberSearch = x => x.RegNumber.Contains(model.RegNumber);

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            var documentTypes = repo.AllReadonly<DocumentTypeGrouping>()
                             .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.EuropeanHeritage)
                             .Select(x => x.DocumentTypeId)
                             .ToList();

            var requestDocumentTypes = repo.AllReadonly<DocumentTypeGrouping>()
                             .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.RequestEuropeanHeritage)
                             .Select(x => x.DocumentTypeId)
                             .ToList();

            int[] sessionResults = { NomenclatureConstants.CaseSessionResult.OpredeleniePrikluchvane, NomenclatureConstants.CaseSessionResult.RazporejdanePrikluchvane };

            var result = repo.AllReadonly<Case>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.CaseStateId != NomenclatureConstants.CaseState.Draft)
                                .Where(x => documentTypes.Contains(x.Document.DocumentTypeId))
                                .Where(groupSearch)
                                .Where(regNumberSearch)
                                .Where(dateSearch)
                                .OrderBy(x => x.RegDate)
                                .Select(x => new EuropeanHeritageReportVM
                                {
                                    Inheritor = string.Join(Environment.NewLine,
                                               x.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                                                            a.PersonRoleId == NomenclatureConstants.PersonRole.Legator)
                                                            .Select(a => a.FullName + "(" + a.PersonRole.Label + ")")),
                                    Notifier = string.Join(Environment.NewLine,
                                               x.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                                                            a.PersonRoleId == NomenclatureConstants.PersonRole.Notifier)
                                                            .Select(a => a.FullName + "(" + a.PersonRole.Label + ")")),
                                    RegNumber = x.RegNumber,
                                    RegDate = x.RegDate,
                                    FinishDate = string.Join(Environment.NewLine, x.CaseSessions.Where(a => a.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                                       a.CaseSessionResults.Where(b => b.IsActive && b.IsMain &&
                                                                      sessionResults.Contains(b.SessionResultId)).Any())
                                                 .Select(a => a.DateFrom.ToString("dd.MM.yyyy"))),
                                    ActTypeName = string.Join(Environment.NewLine, x.CaseSessions.Where(a => a.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                                       a.CaseSessionResults.Where(b => b.IsActive && b.IsMain && sessionResults.Contains(b.SessionResultId)).Any()
                                                       ).Select(a => string.Join("; ", a.CaseSessionActs.Where(b => b.ActDate != null && b.RegNumber != null)
                                                                                       .Select(b => b.ActType.Label)))),
                                    ActNumber = string.Join(Environment.NewLine, x.CaseSessions.Where(a => a.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                                       a.CaseSessionResults.Where(b => b.IsActive && b.IsMain && sessionResults.Contains(b.SessionResultId)).Any()
                                                       ).Select(a => string.Join("; ", a.CaseSessionActs.Where(b => b.ActDate != null && b.RegNumber != null)
                                                                                       .Select(b => b.RegNumber + "/" + ((DateTime)b.RegDate).ToString("dd.MM.yyyy"))))),
                                    PersonNameReceive = string.Join(Environment.NewLine,
                                                            x.CaseSessions.Select(a => string.Join(Environment.NewLine,
                                                                      a.CaseSessionDocs.Where(b => requestDocumentTypes.Contains(b.Document.DocumentTypeId))
                                                                        .Select(b => string.Join(Environment.NewLine,
                                                                        b.Document.DocumentPersons.Where(c => c.PersonRoleId == NomenclatureConstants.PersonRole.Notifier)
                                                                        .Select(c => c.FullName))))))
                                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        public byte[] EuropeanHeritageReportToExcelOne(EuropeanHeritageFilterReportVM model)
        {
            var dataRows = EuropeanHeritageReport_Select(userContext.CourtId, model).ToList();

            NPoiExcelService excelService = GetExcelHtmlTemplate("EuropeanHeritage");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<EuropeanHeritageReportVM, object>>>()
                {
                    x => x.Inheritor,
                    x => x.Notifier,
                    x => x.RegNumber,
                    x => x.RegDate,
                    x => x.FinishDate,
                    x => x.ActTypeName,
                    x => x.ActNumber,
                    x => x.PersonNameReceive
                }
            );
            return excelService.ToArray();
        }

        private IQueryable<PublicInformationReportVM> PublicInformationReport_Select(int courtId, PublicInformationFilterReportVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Document, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.DocumentDate.Date >= dateFromSearch.Date && x.DocumentDate.Date <= dateToSearch.Date;

            int fromNumberSearch = model.NumberFrom == null ? 1 : (int)model.NumberFrom;
            int toNumberSearch = model.NumberTo == null ? int.MaxValue : (int)model.NumberTo;

            Expression<Func<Document, bool>> numberSearch = x => true;
            if (model.NumberFrom != null || model.NumberTo != null)
                numberSearch = x => x.DocumentNumberValue >= fromNumberSearch && x.DocumentNumberValue <= toNumberSearch;

            var documentTypes = repo.AllReadonly<DocumentTypeGrouping>()
                             .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.PublicInformation)
                             .Select(x => x.DocumentTypeId)
                             .ToList();

            var result = repo.AllReadonly<Document>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => documentTypes.Contains(x.DocumentTypeId))
                                .Where(x => x.DateExpired == null)
                                .Where(numberSearch)
                                .Where(dateSearch)
                                .OrderBy(x => x.DocumentDate)
                                .Select(x => new PublicInformationReportVM
                                {
                                    Notifier = string.Join(Environment.NewLine,
                                               x.DocumentPersons.Select(a => a.FullName)),
                                    DocumentData = "Вх.№ " + x.DocumentNumber + "/" + x.DocumentDate.ToString("dd.MM.yyyy"),
                                    Description = x.Description,
                                    Decision = repo.AllReadonly<DocumentDecision>().Where(a => a.DocumentId == x.Id
                                               && a.DocumentDecisionStateId == NomenclatureConstants.DocumentDecisionStates.Resolution)
                                               .Select(a => a.DecisionType.Label + Environment.NewLine + "Решение № " + a.RegNumber + "/" + ((DateTime)a.RegDate).ToString("dd.MM.yyyy") +
                                               (string.IsNullOrEmpty(a.Description) ? "" : Environment.NewLine + a.Description))
                                               .FirstOrDefault()
                                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        public byte[] PublicInformationReportToExcelOne(PublicInformationFilterReportVM model)
        {
            var dataRows = PublicInformationReport_Select(userContext.CourtId, model).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }

            NPoiExcelService excelService = GetExcelHtmlTemplate("PublicInformation");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<PublicInformationReportVM, object>>>()
                {
                    x => x.Index,
                    x => x.Notifier,
                    x => x.DocumentData,
                    x => x.Description,
                    x => x.Decision
                }
            );
            return excelService.ToArray();
        }

        public IQueryable<CaseDecisionReportVM> CaseDecisionReport_Select(int courtId, CaseDecisionFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CaseSessionAct, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => ((DateTime)x.ActDeclaredDate).Date >= dateFromSearch.Date && ((DateTime)x.ActDeclaredDate).Date <= dateToSearch.Date;

            Expression<Func<CaseSessionAct, bool>> groupSearch = x => true;
            if (model.CaseGroupId > 0)
                groupSearch = x => x.CaseSession.Case.CaseGroupId == model.CaseGroupId;


            var result = repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.CaseSession.Case.CourtId == courtId)
                                .Where(x => x.ActDeclaredDate != null)
                                .Where(x => x.ActTypeId == NomenclatureConstants.ActType.Answer)
                                .Where(x => x.CaseSession.Case.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo) // Без наказателни дела
                                .Where(groupSearch)
                                .Where(dateSearch)
                                .Select(x => new CaseDecisionReportVM
                                {
                                    CaseGroupName = x.CaseSession.Case.CaseGroup.Label,
                                    CaseRegNumber = x.CaseSession.Case.RegNumber,
                                    JudgeReporterName = x.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.CaseSession.DateFrom &&
                                                                  a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                              .Select(a => a.LawUnit.FullName).FirstOrDefault(),
                                    DepersonalizeUser = x.DepersonalizeUser.LawUnit.FullName,
                                    ActDeclaredDate = (DateTime)x.ActDeclaredDate,
                                    ActLink = x.ActType.Label + " " + x.RegNumber + (x.ActDate != null ? ("/" + ((DateTime)x.ActDate).Year.ToString()) : ""),
                                    FileAct = repo.AllReadonly<MongoFile>().Where(a => a.SourceType == SourceTypeSelectVM.CaseSessionActDepersonalized &&
                                                    a.SourceId == x.Id.ToString()).Select(a => a.FileId).FirstOrDefault(),
                                    FileMotive = repo.AllReadonly<MongoFile>().Where(a => a.SourceType == SourceTypeSelectVM.CaseSessionActMotiveDepersonalized &&
                                                    a.SourceId == x.Id.ToString()).Select(a => a.FileId).FirstOrDefault(),
                                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        public byte[] CaseDecisionReportToExcelOne(CaseDecisionFilterReportVM model, string url)
        {
            var dataRows = CaseDecisionReport_Select(userContext.CourtId, model).OrderBy(x => x.ActDeclaredDate).ToList();

            NPoiExcelService excelService = GetExcelHtmlTemplate("CaseDecision");
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }

            excelService.InsertList(
                dataRows,
                new List<Expression<Func<CaseDecisionReportVM, object>>>()
                {
                    x => x.Index,
                    x => x.CaseGroupName,
                    x => x.CaseRegNumber,
                    x => x.JudgeReporterName,
                    x => x.DepersonalizeUser,
                    x => x.ActDeclaredDate,
                    x => x.ActLink,
                }
            );

            return excelService.ToArray();
        }

        private IQueryable<HeritageReportVM> HeritageReport_Select(int courtId, HeritageFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CaseSessionAct, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => ((DateTime)x.ActDeclaredDate).Date >= dateFromSearch.Date && ((DateTime)x.ActDeclaredDate).Date <= dateToSearch.Date;

            DateTime dateInforcedFromSearch = model.FromActInforcedDate == null ? DateTime.Now.AddYears(-100) : (DateTime)model.FromActInforcedDate;
            DateTime dateInforcedToSearch = model.ToActInforcedDate == null ? DateTime.Now.AddYears(100) : (DateTime)model.ToActInforcedDate;

            Expression<Func<CaseSessionAct, bool>> actInforcedDateSearch = x => true;
            if (model.FromActInforcedDate != null || model.ToActInforcedDate != null)
                actInforcedDateSearch = x => x.ActInforcedDate != null && (x.ActInforcedDate ?? dateEnd).Date >= dateInforcedFromSearch.Date &&
                           (x.ActInforcedDate ?? dateEnd).Date <= dateInforcedToSearch.Date;

            Expression<Func<CaseSessionAct, bool>> actNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.NumberAct) == false)
                actNumberSearch = x => x.RegNumber == model.NumberAct;

            int[] documentTypesGrouping = { NomenclatureConstants.DocumentTypeGroupings.OtherHeritage, NomenclatureConstants.DocumentTypeGroupings.RefuseHeritage,
                                              NomenclatureConstants.DocumentTypeGroupings.AcceptHeritage};
            var documentTypes = repo.AllReadonly<DocumentTypeGrouping>()
                 .Where(x => documentTypesGrouping.Contains(x.DocumentTypeGroup))
                 .ToList();

            var sessionResults = repo.AllReadonly<SessionResultGrouping>()
                 .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.HeritageReport_Result)
                 .Select(x => x.SessionResultId)
                 .ToList();
            int[] personRoles = { NomenclatureConstants.PersonRole.Inheritor, NomenclatureConstants.PersonRole.Legator };

            var result = repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.CaseSession.Case.CourtId == courtId)
                                .Where(x => x.ActDeclaredDate != null)
                                .Where(x => (documentTypes.Select(a => a.DocumentTypeId)).Contains(x.CaseSession.Case.Document.DocumentTypeId))
                                .Where(x => x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                                .Where(x => x.CaseSession.CaseSessionResults.Where(a => a.IsActive && a.IsMain && sessionResults.Contains(a.SessionResultId)).Any())
                                .Where(actNumberSearch)
                                .Where(dateSearch)
                                .Where(actInforcedDateSearch)
                                .Select(x => new HeritageReportVM
                                {
                                    ActDeclaredDate = x.ActDeclaredDate ?? DateTime.Now, //влизат само тези с ActDeclaredDate != null
                                    DocumentNumber = x.CaseSession.Case.Document.DocumentNumber + "/" + x.CaseSession.Case.Document.DocumentDate.ToString("dd.MM.yyyy") +
                                                     "; " + x.CaseSession.Case.CaseType.Code + " " + x.CaseSession.Case.RegNumber + "; Съдия-докладчик " +
                                                     x.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.CaseSession.DateFrom &&
                                                                  a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                              .Select(a => a.LawUnit.FullName).FirstOrDefault(),
                                    DocumentDate = x.CaseSession.Case.Document.DocumentDate,
                                    Notifier = string.Join(Environment.NewLine,
                                               x.CaseSession.Case.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                                                            a.PersonRoleId == NomenclatureConstants.PersonRole.Petitioner)
                                                            .Select(a => a.FullName + " (" + a.PersonRole.Label + ")")),
                                    RefuseHeritage = (documentTypes.Where(a => a.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.RefuseHeritage)
                                                    .Select(a => a.DocumentTypeId)).Contains(x.CaseSession.Case.Document.DocumentTypeId) == true ?
                                                       x.CaseSession.Case.Document.DocumentType.Label : "",
                                    AcceptHeritage = (documentTypes.Where(a => a.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.AcceptHeritage)
                                                    .Select(a => a.DocumentTypeId)).Contains(x.CaseSession.Case.Document.DocumentTypeId) == true ?
                                                       x.CaseSession.Case.Document.DocumentType.Label : "",
                                    ResultData = string.Join(Environment.NewLine, x.CaseSession.Case.CasePersons
                                                                                   .Where(a => a.CaseSessionId == null &&
                                                                                   a.DateExpired == null &&
                                                                                   a.PersonRoleId == NomenclatureConstants.PersonRole.Legator)
                                                    .Select(a => a.FullName)),
                                    PersonNames = string.Join(Environment.NewLine, x.CaseSession.Case.CasePersons
                                                                                   .Where(a => a.CaseSessionId == null &&
                                                                                   a.DateExpired == null &&
                                                                                   personRoles.Contains(a.PersonRoleId))
                                                    .Select(a => a.FullName + " (" + a.PersonRole.Label + ")")),
                                    Description = x.ActType.Label +
                                                          (x.ActDate != null ? (Environment.NewLine + x.RegNumber + "/" + ((DateTime)x.ActDate).ToString("dd.MM.yyyy")) : "") +
                                                          Environment.NewLine +
                                                          string.Join(Environment.NewLine, x.CasePersonInheritances.Where(a => a.IsActive == true)
                                                          .Select(a => a.CasePerson.FullName + " - " + a.CasePersonInheritanceResult.Label)) +
                                                  (x.ActInforcedDate != null ? (Environment.NewLine +
                                                       "В законна сила от " + ((DateTime)x.ActInforcedDate).ToString("dd.MM.yyyy")) : "")
                                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        public byte[] HeritageReportToExcelOne(HeritageFilterReportVM model)
        {
            var dataRows = HeritageReport_Select(userContext.CourtId, model).OrderBy(x => x.ActDeclaredDate).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }

            NPoiExcelService excelService = GetExcelHtmlTemplate("Heritage");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<HeritageReportVM, object>>>()
                {
                    x => x.Index,
                    x => x.DocumentNumber,
                    x => x.DocumentDate,
                    x => x.Notifier,
                    x => x.RefuseHeritage,
                    x => x.AcceptHeritage,
                    x => x.ResultData,
                    x => x.PersonNames,
                    x => x.Description
                }
            );
            return excelService.ToArray();
        }

        private IQueryable<CaseFirstInstanceReportVM> CaseFirstInstanceReport_Select(int courtId, CaseFirstInstanceFilterReportVM model)
        {
            int fromNumberSearch = model.FromNumber == null ? 1 : (int)model.FromNumber;
            int toNumberSearch = model.ToNumber == null ? int.MaxValue : (int)model.ToNumber;

            Expression<Func<Case, bool>> numberCaseSearch = x => true;
            if (model.FromNumber != null || model.ToNumber != null)
                numberCaseSearch = x => x.ShortNumberValue >= fromNumberSearch && x.ShortNumberValue <= toNumberSearch;

            Expression<Func<Case, bool>> groupWhere = x => true;
            if (model.CaseGroupId > 0)
                groupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> typeWhere = x => true;
            if (model.CaseTypeId > 0)
                typeWhere = x => x.CaseTypeId == model.CaseTypeId;

            var personRoles = repo.AllReadonly<PersonRole>().ToList();
            var personNakazatelnoLeft = personRoles.Where(x => x.RoleKindId == NomenclatureConstants.RoleKind.LeftSide ||
                                      x.Id == NomenclatureConstants.PersonRole.Petitioner)
                                    .Select(x => x.Id).ToList();
            var personNakazatelnoRight = personRoles.Where(x => x.RoleKindId == NomenclatureConstants.RoleKind.RightSide ||
                                      x.Id == NomenclatureConstants.PersonRole.Offender)
                                    .Select(x => x.Id).ToList();

            var personOtherLeft = personRoles.Where(x => x.RoleKindId == NomenclatureConstants.RoleKind.LeftSide ||
                                      x.Id == NomenclatureConstants.PersonRole.Petitioner)
                                    .Select(x => x.Id).ToList();
            var personOtherRight = personRoles.Where(x => x.RoleKindId == NomenclatureConstants.RoleKind.RightSide)
                                    .Select(x => x.Id).ToList();

            DateTime dateEnd = DateTime.Now.AddDays(1);
            var caseBooksVMs = repo.AllReadonly<Case>()
                .Where(x => x.CourtId == courtId)
                .Where(x => x.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance)
                .Where(x => x.RegDate.Date >= model.DateFrom.Date && x.RegDate.Date <= model.DateTo.Date)
                .Where(numberCaseSearch)
                .Where(groupWhere)
                .Where(typeWhere)
                .Select(x => new CaseFirstInstanceReportVM()
                {
                    CaseRegNumberValue = x.ShortNumberValue ?? 0,
                    CaseLifecycleMonths = x.CaseLifecycles.Select(a => a.DurationMonths).Sum(),
                    CaseTypeId = x.CaseTypeId,
                    RegNumber = x.CaseType.Code + ", " + x.RegNumber +
                            (x.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo ? ("/" + x.RegDate.ToString("dd.MM.yyyy")) : "") +
                            (x.CaseStateId == NomenclatureConstants.CaseState.Deleted ? " - Анулирано" : "") +
                                 x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= DateTime.Now && a.CourtDepartmentId != null && a.CaseSessionId == null)
                                                              .Select(a => Environment.NewLine + a.CourtDepartment.Label)
                                                              .DefaultIfEmpty("").FirstOrDefault(),
                    IsNewNumber = (x.IsNewCaseNewNumber ?? false),
                    AcceptJurisdiction = (x.IsNewCaseNewNumber ?? false) == true ? false : repo.AllReadonly<CaseMigration>()
                                           .Where(a => a.CaseId == x.Id && a.DateExpired == null)
                                           .Where(a => a.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction)
                                           .Any(),
                    AcceptDiffJurisdiction = (x.IsNewCaseNewNumber ?? false) == true ? false : repo.AllReadonly<CaseMigration>()
                                           .Where(a => a.CaseId == x.Id && a.DateExpired == null)
                                           .Where(a => a.InitialCaseId != x.Id)
                                           .Where(a => a.CaseMigrationTypeId != NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction)
                                           .Where(a => a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                           .Any(),
                    InputDocument = x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " - " + x.Document.DocumentType.Label,
                    RegDate = x.RegDate,
                    SessionDates = model.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo ? "" :
                                  string.Join(Environment.NewLine, x.CaseSessions.Where(a => a.DateExpired == null &&
                                            a.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
                                            .OrderBy(a => a.DateFrom)
                                            .Select(a => a.DateFrom.ToString("dd.MM.yyyy HH:mm"))),
                    CaseInstitution = model.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo ? "" :
                                  string.Join(Environment.NewLine, x.Document.DocumentInstitutionCaseInfo
                                            .Select(a => a.InstitutionCaseType.Label + " " + a.CaseNumber + "/" + a.CaseYear.ToString() + " " +
                                                       a.Institution.FullName)),
                    JudgeReporterName = x.CaseLawUnits.Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter && a.DateTo == null && a.CaseSessionId == null)
                                                   .Select(a => a.LawUnit.FullName).DefaultIfEmpty("").FirstOrDefault(),
                    CaseCodeLabel = (x.CaseCode.Code ?? "") + " " + (x.CaseCode.Label ?? ""),
                    CaseCodeCode = x.CaseCode.Code,
                    CaseCodeId = x.CaseCode.Id,
                    CasePersonLeft = String.Join(Environment.NewLine, x.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                           (x.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo ? personNakazatelnoLeft.Contains(a.PersonRoleId) :
                           personOtherLeft.Contains(a.PersonRoleId)))
                                        .Select(p => p.FullName)),
                    CasePersonRight = String.Join(Environment.NewLine, x.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                           (x.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo ? personNakazatelnoRight.Contains(a.PersonRoleId) :
                           personOtherRight.Contains(a.PersonRoleId)))
                                        .Select(p => p.FullName)),
                    SlovingDate = string.Join(Environment.NewLine, x.CaseSessions.Where(a => a.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                                  a.DateExpired == null &&
                                                  a.CaseSessionResults.Where(r => r.IsActive && r.IsMain &&
                                                            r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution)
                                                            .Any())
                                                  .Select(a => a.DateFrom.ToString("dd.MM.yyyy"))),
                    FinalAct = x.CaseSessions.Where(a => a.SessionStateId == NomenclatureConstants.SessionState.Provedeno && a.DateExpired == null &&
                                                 a.CaseSessionActs.Where(b => NomenclatureConstants.SessionActState.EnforcedStates.Contains(b.ActStateId) &&
                                                        b.IsFinalDoc == true && b.DateExpired == null).Any())
                                              .Select(a => a.CaseSessionActs.Where(b => NomenclatureConstants.SessionActState.EnforcedStates.Contains(b.ActStateId) &&
                                                        b.IsFinalDoc == true && b.DateExpired == null)
                                                                  .Select(b => (DateTime?)b.ActDeclaredDate).FirstOrDefault()).FirstOrDefault(),
                    Result = x.CaseSessionActs.Where(a=>NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.ActStateId) &&
                                            a.IsFinalDoc == true && a.DateExpired == null && a.RegDate != null &&
                               a.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                               a.CaseSession.DateExpired == null)
                             .Select(a=> string.Join("; ", a.CaseSession.CaseSessionResults
                                                                .Where(c => c.IsActive && c.IsMain)
                                                                .Select(c => c.SessionResult.Label)) +
                                              Environment.NewLine + a.ActType.Label + " " +
                                              (a.RegNumber ?? "") + "/" + 
                                              ((DateTime)a.RegDate).ToString(FormattingConstant.NormalDateFormat) + 
                                              Environment.NewLine +
                                              a.CaseSession.SessionType.Label + " " + 
                                              a.CaseSession.DateFrom.ToString("dd.MM.yyyy")
                                     )
                             .FirstOrDefault(),
                    SendOtherInstance = string.Join(Environment.NewLine, repo.AllReadonly<CaseMigration>().Where(a => a.CaseId == x.Id &&
                                          a.CaseId == a.InitialCaseId && a.SendToCourtId != null && a.OutDocumentId != null)
                                        .Select(a => a.OutDocument.DocumentNumber + "/" + a.OutDocument.DocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                a.SendToCourt.Label)),
                    ReceiveOtherInstance = string.Join(Environment.NewLine, repo.AllReadonly<CaseMigration>()
                                          .Where(a => a.ReturnCaseId == x.Id &&
                                          a.SendToCourtId == x.CourtId && a.OutDocumentId != null)
                                        .Select(a => a.OutDocument.DocumentNumber + "/" + a.OutDocument.DocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                a.SendToCourt.Label)),
                    ResultOtherInstance = string.Join(Environment.NewLine,
                                        repo.AllReadonly<CaseMigration>().Where(a => a.OutCaseMigration.ReturnCaseId == x.Id &&
                                         a.Case.CourtId == x.CourtId
                                         && a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .Select(a => (a.CaseMigrationTypeId != NomenclatureConstants.CaseMigrationTypes.AcceptCase_AfterComplain ?
                                               (a.CaseMigrationType.Description ?? "") :
                                               string.Join("; ", a.Case.CaseSessionActs.Where(b => b.DateExpired == null && b.ActResultId != null)
                                                 .Select(b => b.ActResult.Label))) +
                                        (a.CaseId != x.Id ? (Environment.NewLine + a.Case.RegNumber + "/" + a.Case.RegDate.ToString("dd.MM.yyyy")) : ""))),
                    DateArch = x.CaseArchives.Select(a => (DateTime?)a.RegDate).FirstOrDefault(),
                    NumArch = x.CaseArchives.Select(a => a.RegNumber).FirstOrDefault(),
                    NumLinkArch = x.CaseArchives.Select(a => a.ArchiveLink).FirstOrDefault(),
                }).AsQueryable();

            //var sql = caseBooksVMs.ToSql();
            return caseBooksVMs;
        }

        private void CaseFirstInstanceAddTextCountToExcel(NPoiExcelService excelService, string text, int count)
        {
            AddTextCountToExcel(excelService, text, count, 3);
        }

        public byte[] CaseFirstInstanceReportToExcelOne(CaseFirstInstanceFilterReportVM model)
        {
            if (model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                return CaseFirstInstanceCriminalReportToExcelOne(model);
            }
            else
            {
                return CaseFirstInstanceDifferentCriminalReportToExcelOne(model);
            }
        }

        private byte[] CaseFirstInstanceCriminalReportToExcelOne(CaseFirstInstanceFilterReportVM model)
        {
            var dataRows = CaseFirstInstanceReport_Select(userContext.CourtId, model).OrderBy(x => x.CaseRegNumberValue).ToList();
            HtmlTemplate htmlTemplate = GetHtmlTemplate("FInstanceC");
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;

            excelService.InsertList(
                dataRows,
                new List<Expression<Func<CaseFirstInstanceReportVM, object>>>()
                {
                    x => x.RegNumber,
                    x => x.InputDocumentCriminal,
                    x => x.SessionDates,
                    x => x.CaseCodeLabel,
                    x => x.CaseCodeCode,
                    x => x.JudgeReporterName,
                    x => x.CasePersonString,
                    x => x.SlovingDate,
                    x => x.FinalAct,
                    x => x.Result,
                    x => x.SendOtherInstance,
                    x => x.ReceiveOtherInstance,
                    x => x.ResultOtherInstance,
                    x => x.Interval1M,
                    x => x.Interval3M,
                    x => x.Interval6M,
                    x => x.Interval1Y,
                    x => x.IntervalMore1Y,
                    x => x.DateArch,
                    x => x.NumArch,
                    x => x.NumLinkArch,
                }
            );

            excelService.rowIndex += (htmlTemplate.XlsRecapRow ?? 0) - (htmlTemplate.XlsDataRow ?? 0);
            excelService.colIndex = 0;

            excelService.AddRange("Рекапитулация за ПЪРВОИНСТАНЦИОННИ НАКАЗАТЕЛНИ ДЕЛА", 20);
            excelService.AddRow();
            CaseFirstInstanceAddTextCountToExcel(excelService, "Всичко дела", dataRows.Count);
            CaseFirstInstanceAddTextCountToExcel(excelService, "В т.ч. НОХД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.NOHD).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       НЧХД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.NChHD).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       ЧНД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.ChND).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       АНД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.AND).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       ВНОХД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VNOHD).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       ВНЧХД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VNChHD).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       ВЧНД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VChND).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       ВАНД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VAND).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       КАНД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.KAND).Count());

            return excelService.ToArray();
        }

        private byte[] CaseFirstInstanceDifferentCriminalReportToExcelOne(CaseFirstInstanceFilterReportVM model)
        {
            var dataRows = CaseFirstInstanceReport_Select(userContext.CourtId, model).OrderBy(x => x.CaseRegNumberValue).ToList();

            HtmlTemplate htmlTemplate = GetHtmlTemplate("FInstanceO");
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 1;
            excelService.SetCellData(CaseGroupCaption_Title(model.CaseGroupId));

            excelService.colIndex = 0;
            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;

            excelService.InsertList(
                dataRows,
                new List<Expression<Func<CaseFirstInstanceReportVM, object>>>()
                {
                    x => x.RegNumber,
                    x => x.InputDocumentText,
                    x => x.RegDate,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeLabel,
                    x => x.CaseCodeCode,
                    x => x.CasePersonString,
                    x => x.SlovingDate,
                    x => x.FinalAct,
                    x => x.Result,
                    x => x.SendOtherInstance,
                    x => x.ReceiveOtherInstance,
                    x => x.ResultOtherInstance,
                    x => x.Interval1M,
                    x => x.Interval3M,
                    x => x.Interval6M,
                    x => x.Interval1Y,
                    x => x.IntervalMore1Y,
                    x => x.DateArch,
                    x => x.NumArch,
                    x => x.NumLinkArch,
                }
            );

            excelService.rowIndex += (htmlTemplate.XlsRecapRow ?? 0) - (htmlTemplate.XlsDataRow ?? 0);
            excelService.colIndex = 0;

            if (model.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo)
            {
                excelService.AddRange("Рекапитулация за ПЪРВОИНСТАНЦИОННИ ГРАЖДАНСКИ ДЕЛА", 20);
                excelService.AddRow();
                CaseFirstInstanceAddTextCountToExcel(excelService, "Всичко дела", dataRows.Count);
                CaseFirstInstanceAddTextCountToExcel(excelService, "В т.ч. Гражданско дело", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.GD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Частно гражданско дело", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.ChGD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Гражданско дело (В)", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VGD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Частно гражданско дело (В)", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VChGD).Count());
            }

            if (model.CaseGroupId == NomenclatureConstants.CaseGroups.Trade)
            {
                excelService.AddRange("Рекапитулация за ПЪРВОИНСТАНЦИОННИ ТЪРГОВСКИ ДЕЛА", 20);
                excelService.AddRow();
                CaseFirstInstanceAddTextCountToExcel(excelService, "Всичко дела", dataRows.Count);
                CaseFirstInstanceAddTextCountToExcel(excelService, "В т.ч. Търговско дело", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.TD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Частно търговско дело", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.ChTD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Търговско дело (В)", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VTD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Частно търговско дело (В)", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VChTD).Count());
            }

            if (model.CaseGroupId == NomenclatureConstants.CaseGroups.Company)
            {
                var codes = repo.AllReadonly<CaseCodeGrouping>().Where(x => x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.RegisterAssociation ||
                                                                          x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.RegisterJSK ||
                                                                          x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.RegisterLawyer ||
                                                                          x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.RegisterPensioner
                                                                          ).ToList();
                var codeAssociation = codes.Where(x => x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.RegisterAssociation).Select(x => x.CaseCodeId).ToList();
                var codeJSK = codes.Where(x => x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.RegisterJSK).Select(x => x.CaseCodeId).ToList();
                var codeLawyer = codes.Where(x => x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.RegisterLawyer).Select(x => x.CaseCodeId).ToList();
                var codePensioner = codes.Where(x => x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.RegisterPensioner).Select(x => x.CaseCodeId).ToList();

                excelService.AddRange("Рекапитулация за ПЪРВОИНСТАНЦИОННИ ФИРМЕНИ ДЕЛА", 20);
                excelService.AddRow();
                CaseFirstInstanceAddTextCountToExcel(excelService, "Всичко дела", dataRows.Count);
                CaseFirstInstanceAddTextCountToExcel(excelService, "В т.ч. Фирмено дело", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.FD).Count());
                excelService.SetRowHeight(excelService.rowIndex, 1000);
                CaseFirstInstanceAddTextCountToExcel(excelService, "       В т.ч. Регистрация на сдружение, фондация, читалище, синдикална и работодателска организация", dataRows.Where(x => codeAssociation.Contains(x.CaseCodeId)).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Вписване промени", 0);
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Регистрация на ЖСК", dataRows.Where(x => codeJSK.Contains(x.CaseCodeId)).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Вписване промени", 0);
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Регистрации на адвокатско дружество", dataRows.Where(x => codeLawyer.Contains(x.CaseCodeId)).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Вписване промени", 0);
                excelService.SetRowHeight(excelService.rowIndex, 2000);
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Регистрация на пенсионен фонд /фондове за допълнително задължително пенсионно " +
                    "осигуряване и фондове за допълнително доброволно пенсионни осигуряване вкл. по професионални схеми/",
                    dataRows.Where(x => codePensioner.Contains(x.CaseCodeId)).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Вписване промени", 0);
            }

            return excelService.ToArray();
        }
        private IQueryable<CaseMigrationReturnReportVM> CaseMigrationReturnReport_Select(int courtId, CaseMigrationReturnFilterReportVM model)
        {
            Expression<Func<CaseMigration, bool>> groupWhere = x => true;
            if (model.CaseGroupId > 0)
                groupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseMigration, bool>> typeWhere = x => true;
            if (model.CaseTypeId > 0)
                typeWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            Expression<Func<CaseMigration, bool>> initialCourtWhere = x => true;
            if (model.InitialCourtId > 0)
                initialCourtWhere = x => x.InitialCase.CourtId == model.InitialCourtId;

            Expression<Func<CaseMigration, bool>> judgeWhere = x => true;
            if (model.JudgeReporterName > 0)
                judgeWhere = x => x.Case.CaseLawUnits.Where(a => a.DateTo == null && a.CaseSessionId == null &&
                                                  a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                  a.LawUnitId == model.JudgeReporterName).Any();

            Expression<Func<CaseMigration, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.Case.CaseLawUnits.Where(a => a.DateTo == null && a.CaseSessionId == null &&
                                                  a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                  a.CourtDepartmentId == model.DepartmentId).Any();

            int[] courtTypes = { NomenclatureConstants.CourtType.VKS, NomenclatureConstants.CourtType.Apeal, NomenclatureConstants.CourtType.DistrictCourt };

            var sessionResultFinish = repo.AllReadonly<SessionResultGrouping>()
                 .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseMigrationReturnReport_FinishCase)
                 .Select(x => x.SessionResultId)
                 .ToList();

            var sessionResultFinishBase = repo.AllReadonly<SessionResultBaseGrouping>()
                 .Where(x => x.SessionResultBaseGroup == NomenclatureConstants.SessionResultBaseGroupings.CaseMigrationReturnReport_FinishCaseBase)
                 .Select(x => x.SessionResultBaseId)
                 .ToList();

            var result = repo.AllReadonly<CaseMigration>()
                .Where(x => NomenclatureConstants.CaseMigrationTypes.ReturnCaseTypes.Contains(x.CaseMigrationTypeId))
                //.Where(x => x.SendToCourtId == x.InitialCase.CourtId)
                .Where(x => x.CourtId == courtId)
                .Where(x => x.OutDocument.DocumentDate.Date >= model.DateFrom.Date && x.OutDocument.DocumentDate.Date <= model.DateTo.Date)
                .Where(x => courtTypes.Contains(x.Case.Court.CourtTypeId))
                .Where(x => x.Case.CaseSessions.Where(a => a.DateExpired == null &&
                                        a.CaseSessionResults.Where(b => b.IsActive && b.IsMain &&
                                         sessionResultFinish.Contains(b.SessionResultId) &&
                                         sessionResultFinishBase.Contains(b.SessionResultBaseId ?? 0)
                                         ).Any()).Any())
                .Where(groupWhere)
                .Where(typeWhere)
                .Where(initialCourtWhere)
                .Where(judgeWhere)
                .Where(departmentWhere)
                .Select(x => new CaseMigrationReturnReportVM()
                {
                    OldLinkNumber = x.Case.Document.DocumentCaseInfo.Where(a => a.CaseId == null)
                                       .Select(a => a.CaseRegNumber + " - " + a.Court.Label)
                                       .FirstOrDefault(),
                    MigrationLinkNumber = x.InitialCase.RegNumber + "(" + x.InitialCase.CaseGroup.Code + ") - " + x.InitialCase.Court.Label,
                    CaseData = x.Case.Document.DocumentNumber + "/" + x.Case.Document.DocumentDate.ToString("dd.MM.yyyy") + " - " +
                               x.Case.Document.DocumentType.Label + "; " + x.Case.CaseType.Code + " " + x.Case.RegNumber,
                    ActReturnData = string.Join(Environment.NewLine,
                                     x.Case.CaseSessions.Where(a => a.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                                       a.DateExpired == null &&
                                                            a.CaseSessionResults.Where(b => b.IsActive && b.IsMain &&
                                                             sessionResultFinish.Contains(b.SessionResultId) &&
                                                             sessionResultFinishBase.Contains(b.SessionResultBaseId ?? 0)
                                                        ).Any())
                                            .Select(a => string.Join("; ",
                                                       a.CaseSessionActs.Where(b => b.ActDate != null && b.RegNumber != null && b.DateExpired == null)
                                                            .Select(b => b.ActType.Label + " " + b.RegNumber + "/" +
                                                                   ((DateTime)b.RegDate).ToString("dd.MM.yyyy"))))),
                    ActReturnDescription = string.Join(Environment.NewLine, x.Case.CaseSessions.Where(a => a.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                                       a.DateExpired == null &&
                                                            a.CaseSessionResults.Where(b => b.IsActive && b.IsMain &&
                                                             sessionResultFinish.Contains(b.SessionResultId) &&
                                                             sessionResultFinishBase.Contains(b.SessionResultBaseId ?? 0)
                                                        ).Any()
                                                       ).Select(a => string.Join("; ",
                                                       a.CaseSessionActs.Where(b => b.ActDate != null && b.RegNumber != null && b.DateExpired == null)
                                                                                       .Select(b => b.Description)))) +
                                                       Environment.NewLine +
                                         string.Join(Environment.NewLine, x.Case.CaseSessions.Where(a => a.DateExpired == null &&
                                                     a.CaseSessionActs.Where(b => b.DateExpired == null && b.IsFinalDoc).Any() &&
                                                            a.CaseSessionResults.Where(b => b.IsActive && b.IsMain &&
                                                             sessionResultFinish.Contains(b.SessionResultId) &&
                                                             sessionResultFinishBase.Contains(b.SessionResultBaseId ?? 0)
                                         ).Any())
                                         .Select(a => a.CaseSessionResults.Where(b => b.IsActive && b.IsMain &&
                                                               sessionResultFinish.Contains(b.SessionResultId) &&
                                                               sessionResultFinishBase.Contains(b.SessionResultBaseId ?? 0))
                                                      .Select(b => b.SessionResultBase.Label).FirstOrDefault()))
                                                       ,
                    JudgeReporterName = x.Case.CaseLawUnits.Where(a => a.DateTo == null && a.CaseSessionId == null &&
                                                  a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                              .Select(a => a.LawUnit.FullName).FirstOrDefault(),
                    OutDocumentData = x.OutDocument.DocumentNumber + "/" + x.OutDocument.DocumentDate.ToString("dd.MM.yyyy") + Environment.NewLine +
                                      x.Case.RegNumber + " " + x.Case.CaseGroup.Code,
                    UserName = x.OutDocument.User.LawUnit.FullName,
                    NewCaseData = string.Join(Environment.NewLine, repo.AllReadonly<CaseMigration>().Where(a => a.InitialCaseId == x.InitialCaseId &&
                                         a.CaseId != x.InitialCaseId && a.Case.CourtId == x.Case.CourtId && a.Id > x.Id)
                                        .Select(a => "Вх.№ " + a.Case.Document.DocumentNumber + "/" + a.Case.Document.DocumentDate.ToString("dd.MM.yyyy") +
                                        " - " + a.Case.Document.DocumentType.Label + ";" + Environment.NewLine +
                                        (a.Case.RegNumber ?? "") + " " + (a.Case.CaseType.Code ?? ""))
                                        .Distinct()),
                });

            return result;
        }

        public byte[] CaseMigrationReturnReportToExcelOne(CaseMigrationReturnFilterReportVM model)
        {
            var dataRows = CaseMigrationReturnReport_Select(userContext.CourtId, model).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }

            NPoiExcelService excelService = GetExcelHtmlTemplate("CaseMigration");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<CaseMigrationReturnReportVM, object>>>()
                {
                    x => x.Index,
                    x => x.InitialCaseData,
                    x => x.CaseData,
                    x => x.ActReturnData,
                    x => x.ActReturnDescription,
                    x => x.JudgeReporterName,
                    x => x.OutDocumentData,
                    x => x.UserName,
                    x => x.NewCaseData,
                }
            );
            return excelService.ToArray();
        }

        private IQueryable<CaseArchiveReportVM> CaseArchiveReport_Select(int courtId, CaseArchiveFilterReportVM model)
        {
            Expression<Func<CaseArchive, bool>> groupWhere = x => true;
            if (model.CaseGroupId > 0)
                groupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CaseArchive, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            Expression<Func<CaseArchive, bool>> yearSearch = x => true;
            if ((model.CaseYear ?? 0) > 0)
                yearSearch = x => x.Case.RegDate.Year == model.CaseYear;

            DateTime dateEnd = DateTime.Now.AddYears(100);
            var result = repo.AllReadonly<CaseArchive>()
                .Where(x => x.Case.CourtId == courtId)
                .Where(groupWhere)
                .Where(dateSearch)
                .Where(yearSearch)
                .Select(x => new CaseArchiveReportVM()
                {
                    CaseData = x.Case.RegNumber + "; " + x.Case.CaseType.Code +
                    x.CaseSessionAct.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= a.CaseSession.DateFrom &&
                                                               a.CourtDepartmentId != null)
                                                              .Select(a => Environment.NewLine + a.CourtDepartment.Label)
                                                              .DefaultIfEmpty("").FirstOrDefault(),
                    ArchiveDate = x.RegDate,
                    ArchiveLink = x.ArchiveLink,
                    ActData = (string.IsNullOrEmpty(x.ActDestroyLabel) == false ? x.ActDestroyLabel + Environment.NewLine : "") +
                              (string.IsNullOrEmpty(x.Description) == false ? x.Description + Environment.NewLine : "") +
                              (x.DescriptionInfoDestroy ?? ""),
                    Description = x.Description,
                    BookData = x.BookNumber.ToString() + "/" + x.BookYear.ToString(),
                    ArchiveIndex = (string.IsNullOrEmpty(x.CourtArchiveIndex.Code) == true ? "" : (x.CourtArchiveIndex.Code + " - ")) + x.CourtArchiveIndex.Label,
                    DescriptionInfo = x.DescriptionInfo,
                    CasePersonNames = model.WithPerson == false ? "" : String.Join(Environment.NewLine,
                                       x.Case.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null).Select(p => p.FullName)),
                });

            return result;
        }

        public byte[] CaseArchiveReportToExcelOne(CaseArchiveFilterReportVM model)
        {
            var dataRows = CaseArchiveReport_Select(userContext.CourtId, model).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }

            NPoiExcelService excelService = null;
            if (model.WithPerson == false)
            {
                excelService = GetExcelHtmlTemplate("CaseArchive");
                excelService.InsertList(
                    dataRows,
                    new List<Expression<Func<CaseArchiveReportVM, object>>>()
                    {
                    x => x.Index,
                    x => x.CaseData,
                    x => x.ArchiveDate,
                    x => x.ArchiveLink,
                    x => x.ActData,
                    x => x.Description,
                    x => x.BookData,
                    x => x.ArchiveIndex,
                    x => x.DescriptionInfo,
                    }
                );
            }
            else
            {
                excelService = GetExcelHtmlTemplate("CaseArchivePerson");
                excelService.InsertList(
                    dataRows,
                    new List<Expression<Func<CaseArchiveReportVM, object>>>()
                    {
                    x => x.Index,
                    x => x.CaseData,
                    x => x.ArchiveDate,
                    x => x.ArchiveLink,
                    x => x.ActData,
                    x => x.Description,
                    x => x.BookData,
                    x => x.ArchiveIndex,
                    x => x.DescriptionInfo,
                    x => x.CasePersonNames,
                    }
                );
            }

            return excelService.ToArray();
        }

        private IQueryable<DivorceReportVM> DivorceReport_Select(int courtId, DivorceFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

            Expression<Func<CaseSessionActDivorce, bool>> regNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.CaseNumber) == false)
                regNumberSearch = x => x.CaseSessionAct.CaseSession.Case.RegNumber.Contains(model.CaseNumber);

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CaseSessionActDivorce, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            var result = repo.AllReadonly<CaseSessionActDivorce>()
                .Where(x => x.CaseSessionAct.CaseSession.Case.CourtId == courtId)
                .Where(regNumberSearch)
                .Where(dateSearch)
                .Select(x => new DivorceReportVM()
                {
                    DivorceRegDate = x.RegDate,
                    OutDocumentData = x.OutDocument.DocumentNumber + "/" + x.OutDocument.DocumentDate.ToString("dd.MM.yyyy"),
                    CaseData = x.CaseSessionAct.CaseSession.Case.CaseType.Code + "; " + x.CaseSessionAct.CaseSession.Case.RegNumber,
                    SessionActData = x.CaseSessionAct.RegNumber +
                          (x.CaseSessionAct.RegDate != null ? ("/" + ((DateTime)x.CaseSessionAct.RegDate).ToString("dd.MM.yyyy")) : ""),
                    CaseSessionActInforcedDate = x.CaseSessionAct.ActInforcedDate
                });

            return result;
        }

        public byte[] DivorceReportToExcelOne(DivorceFilterReportVM model)
        {
            var dataRows = DivorceReport_Select(userContext.CourtId, model).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            var htmlTemplate = GetHtmlTemplate("Divorce");
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 1;
            excelService.SetCellData("за " + dateFromSearch.ToString("dd.MM.yyyy") + " - " + dateToSearch.ToString("dd.MM.yyyy"));

            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;
            excelService.colIndex = 0;
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<DivorceReportVM, object>>>()
                {
                    x => x.Index,
                    x => x.DivorceRegDate,
                    x => x.OutDocumentData,
                    x => x.CaseData,
                    x => x.SessionActData,
                    x => x.CaseSessionActInforcedDate
                }
            );
            return excelService.ToArray();
        }

        private IQueryable<CaseSecondInstanceReportVM> CaseSecondInstanceReport_Select(int courtId, CaseSecondInstanceFilterReportVM model)
        {
            int fromNumberSearch = model.FromNumber == null ? 1 : (int)model.FromNumber;
            int toNumberSearch = model.ToNumber == null ? int.MaxValue : (int)model.ToNumber;

            Expression<Func<Case, bool>> numberCaseSearch = x => true;
            if (model.FromNumber != null || model.ToNumber != null)
                numberCaseSearch = x => x.ShortNumberValue >= fromNumberSearch && x.ShortNumberValue <= toNumberSearch;

            Expression<Func<Case, bool>> groupWhere = x => true;
            if (model.CaseGroupId > 0)
                groupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> typeWhere = x => true;
            if (model.CaseTypeId > 0)
                typeWhere = x => x.CaseTypeId == model.CaseTypeId;

            var resultStop = repo.AllReadonly<SessionResultGrouping>()
                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseSecondInstanceStop)
                .Select(x => x.SessionResultId)
                .ToList();

            DateTime dateEnd = DateTime.Now.AddDays(1);
            int[] instances = { NomenclatureConstants.CaseInstanceType.SecondInstance, NomenclatureConstants.CaseInstanceType.ThirdInstance };
            var caseBooksVMs = repo.AllReadonly<Case>()
                .Where(x => x.CourtId == courtId)
                .Where(x => instances.Contains(x.CaseType.CaseInstanceId))
                .Where(x => x.RegDate.Date >= model.DateFrom.Date && x.RegDate.Date <= model.DateTo.Date)
                .Where(numberCaseSearch)
                .Where(groupWhere)
                .Where(typeWhere)
                .Select(x => new CaseSecondInstanceReportVM()
                {
                    CaseRegNumberValue = x.ShortNumberValue ?? 0,
                    CaseLifecycleMonths = x.CaseLifecycles.Select(a => a.DurationMonths).Sum(),
                    CaseTypeId = x.CaseTypeId,
                    RegNumber = x.CaseType.Code + ", " + x.RegNumber + (x.CaseStateId == NomenclatureConstants.CaseState.Deleted ? " - Анулирано" : "") +
                                 x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= DateTime.Now && a.CourtDepartmentId != null && a.CaseSessionId == null)
                                                              .Select(a => Environment.NewLine + a.CourtDepartment.Label)
                                                              .DefaultIfEmpty("").FirstOrDefault(),
                    RegDate = x.RegDate,
                    DocumentTypeId = model.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo ? 0 : x.Document.DocumentTypeId,
                    OldLinkNumber = x.Document.DocumentCaseInfo.Where(a => a.CaseId == null)
                                       .Select(a => a.CaseRegNumber + " - " + a.Court.Label)
                                       .FirstOrDefault(),
                    MigrationLinkNumber = repo.AllReadonly<CaseMigration>().Where(a => a.CaseId == x.Id)
                                        .Select(a => a.InitialCase.RegNumber + "/" + a.InitialCase.RegDate.ToString("dd.MM.yyyy") + " - " +
                                                a.InitialCase.Court.Label).FirstOrDefault(),
                    CaseCodeLabel = (x.CaseCode.Code ?? "") + " " + (x.CaseCode.Label ?? ""),
                    CaseCodeCode = x.CaseCode.Code,
                    CaseCodeId = x.CaseCode.Id,
                    SessionDates = model.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo ? "" :
                                  string.Join(Environment.NewLine, x.CaseSessions.Where(a => a.DateExpired == null &&
                                            a.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
                                            .OrderBy(a => a.DateFrom)
                                            .Select(a => a.DateFrom.ToString("dd.MM.yyyy HH:mm"))),
                    CasePersonLeft = String.Join(Environment.NewLine, x.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                                                                                a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.LeftSide)
                                                                                     .Select(p => p.FullName)),
                    CasePersonRight = String.Join(Environment.NewLine, x.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                                                                                a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide)
                                                                                     .Select(p => p.FullName)),
                    SlovingDate = string.Join(Environment.NewLine, x.CaseSessions.Where(a => a.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                                  a.DateExpired == null &&
                                                  a.CaseSessionResults.Where(r => r.IsActive && r.IsMain &&
                                                            r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution)
                                                            .Any())
                                                  .Select(a => a.DateFrom.ToString("dd.MM.yyyy"))),
                    FinalAct = x.CaseSessions.Where(a => a.SessionStateId == NomenclatureConstants.SessionState.Provedeno && a.DateExpired == null &&
                                                   a.CaseSessionActs.Where(b => NomenclatureConstants.SessionActState.EnforcedStates.Contains(b.ActStateId) &&
                                                        b.IsFinalDoc == true && b.DateExpired == null).Any())
                                              .Select(a => a.CaseSessionActs.Where(b => NomenclatureConstants.SessionActState.EnforcedStates.Contains(b.ActStateId) &&
                                                        b.IsFinalDoc == true && b.DateExpired == null)
                                                                  .Select(b => (DateTime?)b.ActDeclaredDate).FirstOrDefault()).FirstOrDefault(),
                    ActResultIds = string.Join(",", x.CaseSessions.Where(a => a.DateExpired == null)
                                        .Select(a => string.Join(",", a.CaseSessionActs.Where(b => b.DateExpired == null && b.ActResultId != null)
                                                                             .Select(b => b.ActResultId))
                                               )),
                    JudgeReporterName = x.CaseLawUnits.Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter && a.DateTo == null && a.CaseSessionId == null)
                                                   .Select(a => a.LawUnit.FullName).DefaultIfEmpty("").FirstOrDefault(),
                    SendOtherInstance = string.Join(Environment.NewLine, repo.AllReadonly<CaseMigration>().Where(a => a.CaseId == x.Id &&
                                          a.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel &&
                                          a.SendToCourtId != null && a.OutDocumentId != null)
                                        .Select(a => a.OutDocument.DocumentNumber + "/" + a.OutDocument.DocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                a.SendToCourt.Label)),
                    ReceiveOtherInstance = string.Join(Environment.NewLine, repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.ReturnCaseId == x.Id && a.OutDocumentId != null &&
                                        NomenclatureConstants.CaseMigrationTypes.ReturnCaseTypes.Contains(a.CaseMigrationTypeId))
                                        .Select(a => a.OutDocument.DocumentNumber + "/" + a.OutDocument.DocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                a.SendToCourt.Label)),
                    ActResultOtherInstanceIds = string.Join(",", x.CaseSessions.Where(a => a.DateExpired == null)
                                        .Select(a => string.Join(",", a.CaseSessionActs.Where(b => b.DateExpired == null && b.ActComplainResultId != null)
                                                                             .Select(b => b.ActComplainResultId))
                                               )),
                    CaseReturnDate = string.Join(Environment.NewLine, repo.AllReadonly<CaseMigration>().Where(a => a.CaseId == x.Id &&
                                          NomenclatureConstants.CaseMigrationTypes.ReturnCaseTypes.Contains(a.CaseMigrationTypeId) &&
                                          a.SendToCourtId != null && a.OutDocumentId != null)
                                        .Select(a => "изх. № " + a.OutDocument.DocumentNumber + "/" + a.OutDocument.DocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                a.SendToCourt.Label)),
                }).AsQueryable();

            //var sql = caseBooksVMs.ToSql();
            return caseBooksVMs;
        }

        private byte[] CaseSecondInstanceDifferentCriminalReportToExcelOne(CaseSecondInstanceFilterReportVM model)
        {
            var dataRows = CaseSecondInstanceReport_Select(userContext.CourtId, model).OrderBy(x => x.CaseRegNumberValue).ToList();

            var complainResults = repo.AllReadonly<ActComplainResultGrouping>()
                .Where(x => NomenclatureConstants.ActComplainResultGroupings.SecondInstanceNonCriminalReport.Contains(x.ActComplainResultGroup))
                .ToList();

            var actResultGroups = repo.AllReadonly<ActResultGroup>()
                .Where(x => NomenclatureConstants.ActResultGroups.SecondInstanceReportNonCriminal.Contains(x.ActResultGrouping))
                .ToList();

            foreach (var item in dataRows)
            {
                var actOtherInstanceResults = item.ActResultOtherInstanceIds.Split(",", StringSplitOptions.RemoveEmptyEntries);
                item.AcceptAll = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.AcceptAll);

                item.AcceptNotAll = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.AcceptNotAll);

                item.CancelAndNew = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.CancelAndNew);

                item.CancelAndReturn = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.CancelAndReturn);

                item.MakeNull = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.MakeNull);

                item.CaseStop = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.CaseStopNonCriminal);

                var actResults = item.ActResultIds.Split(",", StringSplitOptions.RemoveEmptyEntries);
                item.OtherInstanceAcceptAll = GetFromActResult(actResultGroups, actResults,
                                      NomenclatureConstants.ActResultGroups.AcceptAllNonCriminal);

                item.OtherInstanceAcceptNotAll = GetFromActResult(actResultGroups, actResults,
                             NomenclatureConstants.ActResultGroups.AcceptNotAllNonCriminal);

                item.OtherInstanceCancelAndNew = GetFromActResult(actResultGroups, actResults,
                             NomenclatureConstants.ActResultGroups.CancelAndNewNonCriminal);

                item.OtherInstanceCancelAndReturn = GetFromActResult(actResultGroups, actResults,
                            NomenclatureConstants.ActResultGroups.CancelAndReturnNonCriminal);
            }

            HtmlTemplate htmlTemplate = GetHtmlTemplate("SInstance");
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 2;
            excelService.colIndex = 0;
            if (userContext.CourtTypeId == NomenclatureConstants.CourtType.VKS)
                excelService.SetCellData("ОПИСНА КНИГА КАСАЦИОННИ");
            else
                excelService.SetCellData("ОПИСНА КНИГА ВЪЗЗИВНИ");

            excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 1;
            excelService.colIndex = 0;
            excelService.SetCellData(CaseGroupCaption_Title(model.CaseGroupId));

            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;
            excelService.colIndex = 0;

            excelService.InsertList(
                dataRows,
                new List<Expression<Func<CaseSecondInstanceReportVM, object>>>()
                {
                    x => x.RegNumber,
                    x => x.RegDate,
                    x => x.InitialCaseData,
                    x => x.CaseCodeLabel,
                    x => x.CaseCodeCode,
                    x => x.CasePersonLeft,
                    x => x.CasePersonRight,
                    x => x.SlovingDate,
                    x => x.FinalAct,
                    x => x.AcceptAll,
                    x => x.AcceptNotAll,
                    x => x.CancelAndNew,
                    x => x.CancelAndReturn,
                    x => x.MakeNull,
                    x => x.CaseStop,
                    x => x.JudgeReporterName,
                    x => x.SendOtherInstance,
                    x => x.ReceiveOtherInstance,
                    x => x.OtherInstanceAcceptAll,
                    x => x.OtherInstanceAcceptNotAll,
                    x => x.OtherInstanceCancelAndNew,
                    x => x.OtherInstanceCancelAndReturn,
                    x => x.Interval1M,
                    x => x.Interval3M,
                    x => x.Interval6M,
                    x => x.Interval1Y,
                    x => x.IntervalMore1Y,
                    x => x.CaseReturnDate,
                }
            );

            excelService.rowIndex += (htmlTemplate.XlsRecapRow ?? 0) - (htmlTemplate.XlsDataRow ?? 0);
            excelService.colIndex = 0;

            if (model.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo)
            {
                excelService.AddRange("Рекапитулация за ВЪЗЗИВНИ/КАСАЦИОННИ ГРАЖДАНСКИ ДЕЛА", 20);
                excelService.AddRow();
                CaseFirstInstanceAddTextCountToExcel(excelService, "Всичко дела", dataRows.Count);
                CaseFirstInstanceAddTextCountToExcel(excelService, "В т.ч. Гражданско дело", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.GD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Частно гражданско дело", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.ChGD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Гражданско дело (В)", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VGD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Частно гражданско дело (В)", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VChGD).Count());
            }

            if (model.CaseGroupId == NomenclatureConstants.CaseGroups.Trade)
            {
                excelService.AddRange("Рекапитулация за ВЪЗЗИВНИ/КАСАЦИОННИ ТЪРГОВСКИ ДЕЛА", 20);
                excelService.AddRow();
                CaseFirstInstanceAddTextCountToExcel(excelService, "Всичко дела", dataRows.Count);
                CaseFirstInstanceAddTextCountToExcel(excelService, "В т.ч. Търговско дело", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.TD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Частно търговско дело", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.ChTD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Търговско дело (В)", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VTD).Count());
                CaseFirstInstanceAddTextCountToExcel(excelService, "       Частно търговско дело (В)", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VChTD).Count());
            }

            return excelService.ToArray();
        }

        private string GetFromComplainResult(List<ActComplainResultGrouping> complainResults, string[] actOtherInstanceResults,
                                            int complainGroup)
        {
            return complainResults
                                  .Where(x => x.ActComplainResultGroup == complainGroup &&
                                           actOtherInstanceResults.Contains(x.ActComplainResultId.ToString()) == true).Any() ? "*" : "";
        }

        private string GetFromActResult(List<ActResultGroup> actResultGroups, string[] actResults,
                                            int actGroup)
        {
            return actResultGroups
                                  .Where(x => x.ActResultGrouping == actGroup &&
                                           actResults.Contains(x.ActResultId.ToString()) == true).Any() ? "*" : "";
        }

        private byte[] CaseSecondInstanceCriminalReportToExcelOne(CaseSecondInstanceFilterReportVM model)
        {
            var dataRows = CaseSecondInstanceReport_Select(userContext.CourtId, model).OrderBy(x => x.CaseRegNumberValue).ToList();

            int[] documentGroupings = { NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceComplaint,
                                        NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceProtest,
                                        NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceComplaintProtest};
            var documentTypes = repo.AllReadonly<DocumentTypeGrouping>().Where(x => documentGroupings.Contains(x.DocumentTypeGroup)).ToList();
            var documentTypesComplaint = documentTypes.Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceComplaint ||
                        x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceComplaintProtest)
                        .Select(x => x.DocumentTypeId).ToList();
            var documentTypesProtest = documentTypes.Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceProtest ||
                        x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceComplaintProtest)
                        .Select(x => x.DocumentTypeId).ToList();

            var complainResults = repo.AllReadonly<ActComplainResultGrouping>()
                .Where(x => NomenclatureConstants.ActComplainResultGroupings.SecondInstanceReport.Contains(x.ActComplainResultGroup))
                .ToList();

            var actResultGroups = repo.AllReadonly<ActResultGroup>()
                .Where(x => NomenclatureConstants.ActResultGroups.SecondInstanceReport.Contains(x.ActResultGrouping))
                .ToList();

            foreach (var item in dataRows)
            {
                var actOtherInstanceResults = item.ActResultOtherInstanceIds.Split(",", StringSplitOptions.RemoveEmptyEntries);
                item.AcceptSentence = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.AcceptSentence);

                item.Applied66 = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.Applied66);

                item.Cancel66 = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.Cancel66);

                item.SentenceDown = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.SentenceDown);

                item.SentenceUp = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.SentenceUp);

                item.ChangeCriminalPart = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.ChangeCriminalPart);

                item.ChangeCivilPart = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.ChangeCivilPart);

                item.AppliedNew = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.AppliedNew);

                item.ReturnNew = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.ReturnNew);

                item.SentenceNew = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.SentenceNew);

                item.CaseStop = GetFromComplainResult(complainResults, actOtherInstanceResults,
                                      NomenclatureConstants.ActComplainResultGroupings.CaseStop);


                var actResults = item.ActResultIds.Split(",", StringSplitOptions.RemoveEmptyEntries);
                item.OtherInstanceAcceptAll = GetFromActResult(actResultGroups, actResults, NomenclatureConstants.ActResultGroups.AcceptAll);
                item.OtherInstanceAcceptNotAll = GetFromActResult(actResultGroups, actResults, NomenclatureConstants.ActResultGroups.AcceptNotAll);
                item.OtherInstanceCancelAndNew = GetFromActResult(actResultGroups, actResults, NomenclatureConstants.ActResultGroups.CancelAndNew);
                item.OtherInstanceCancelAndReturn = GetFromActResult(actResultGroups, actResults, NomenclatureConstants.ActResultGroups.CancelAndReturn);

                item.Complaint = documentTypesComplaint.Contains(item.DocumentTypeId) ? "*" : "";
                item.Protest = documentTypesProtest.Contains(item.DocumentTypeId) ? "*" : "";
            }

            HtmlTemplate htmlTemplate = GetHtmlTemplate("SInstanceC");
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 2;
            excelService.colIndex = 0;
            if (userContext.CourtTypeId == NomenclatureConstants.CourtType.VKS)
                excelService.SetCellData("ОПИСНА КНИГА КАСАЦИОННИ");
            else
                excelService.SetCellData("ОПИСНА КНИГА ВЪЗЗИВНИ");

            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;
            excelService.colIndex = 0;

            excelService.InsertList(
                dataRows,
                new List<Expression<Func<CaseSecondInstanceReportVM, object>>>()
                {
                    x => x.RegNumber,
                    x => x.RegDate,
                    x => x.InitialCaseData,
                    x => x.CaseCodeLabel,
                    x => x.SessionDates,
                    x => x.CaseCodeCode,
                    x => x.Complaint,
                    x => x.Protest,
                    x => x.CasePersonLeft,
                    x => x.SlovingDate,
                    x => x.FinalAct,
                    x => x.AcceptSentence,
                    x => x.Applied66,
                    x => x.Cancel66,
                    x => x.SentenceDown,
                    x => x.SentenceUp,
                    x => x.ChangeCriminalPart,
                    x => x.ChangeCivilPart,
                    x => x.AppliedNew,
                    x => x.ReturnNew,
                    x => x.SentenceNew,
                    x => x.CaseStop,
                    x => x.JudgeReporterName,
                    x => x.SendOtherInstance,
                    x => x.ReceiveOtherInstance,
                    x => x.OtherInstanceAcceptAll,
                    x => x.OtherInstanceAcceptNotAll,
                    x => x.OtherInstanceCancelAndNew,
                    x => x.OtherInstanceCancelAndReturn,
                    x => x.Interval1M,
                    x => x.Interval3M,
                    x => x.Interval6M,
                    x => x.Interval1Y,
                    x => x.IntervalMore1Y,
                    x => x.CaseReturnDate,
                }
            );

            excelService.rowIndex += (htmlTemplate.XlsRecapRow ?? 0) - (htmlTemplate.XlsDataRow ?? 0);
            excelService.colIndex = 0;

            excelService.AddRange("Рекапитулация за ПЪРВОИНСТАНЦИОННИ НАКАЗАТЕЛНИ ДЕЛА", 20);
            excelService.AddRow();
            CaseFirstInstanceAddTextCountToExcel(excelService, "Всичко дела", dataRows.Count);
            CaseFirstInstanceAddTextCountToExcel(excelService, "В т.ч. НОХД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.NOHD).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       НЧХД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.NChHD).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       ЧНД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.ChND).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       АНД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.AND).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       ВНОХД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VNOHD).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       ВНЧХД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VNChHD).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       ВЧНД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VChND).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       ВАНД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.VAND).Count());
            CaseFirstInstanceAddTextCountToExcel(excelService, "       КАНД", dataRows.Where(x => x.CaseTypeId == NomenclatureConstants.CaseTypes.KAND).Count());

            return excelService.ToArray();
        }
        public byte[] CaseSecondInstanceReportToExcelOne(CaseSecondInstanceFilterReportVM model)
        {
            if (model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                return CaseSecondInstanceCriminalReportToExcelOne(model);
            }
            else
            {
                return CaseSecondInstanceDifferentCriminalReportToExcelOne(model);
            }
        }

        private IQueryable<SentenceReportVM> SentenceReport_Select(int courtId, SentenceFilterReportVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CasePersonSentence, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => ((DateTime)x.InforcedDate).Date >= dateFromSearch.Date && ((DateTime)x.InforcedDate).Date <= dateToSearch.Date;

            var result = repo.AllReadonly<CasePersonSentence>()
                .Where(x => x.Case.CourtId == courtId)
                .Where(x => x.InforcedDate != null)
                .Where(x => x.DateExpired == null)
                .Where(dateSearch)
                .Select(x => new SentenceReportVM()
                {
                    SentDate = x.SentDate,
                    PersonData = x.CasePerson.FullName + " - " + x.CasePerson.Addresses.Select(a => a.Address.FullAddress).DefaultIfEmpty("").FirstOrDefault() +
                                   Environment.NewLine + (x.Description ?? ""),
                    SentenceData = x.ChangeCaseSessionActId != null ?
                                  ((string.Join(Environment.NewLine,
                                   x.ChangeCaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.IsActive && a.IsMain).Select(a => a.SessionResult.Label)) ?? "") +
                                   Environment.NewLine +
                                   x.ChangeCaseSessionAct.ActType.Label + " " +
                                   x.ChangeCaseSessionAct.RegNumber + "/" +
                                   (x.ChangeCaseSessionAct.ActDate != null ? ((DateTime)x.ChangeCaseSessionAct.ActDate).ToString("dd.MM.yyyy") : "") + Environment.NewLine +
                                   x.ChangeCaseSessionAct.CaseSession.Case.RegNumber + " " + x.ChangeCaseSessionAct.CaseSession.Case.Court.Label + Environment.NewLine +
                                   ((DateTime)x.InforcedDate).ToString("dd.MM.yyyy")) :
                                   ((string.Join(Environment.NewLine,
                                   x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.IsActive && a.IsMain).Select(a => a.SessionResult.Label)) ?? "") +
                                   Environment.NewLine +
                                   x.CaseSessionAct.ActType.Label + " " +
                                   x.CaseSessionAct.RegNumber + "/" +
                                   (x.CaseSessionAct.ActDate != null ? ((DateTime)x.CaseSessionAct.ActDate).ToString("dd.MM.yyyy") : "") + Environment.NewLine +
                                   x.CaseSessionAct.CaseSession.Case.RegNumber + " " + x.CaseSessionAct.CaseSession.Case.Court.Label + Environment.NewLine +
                                   ((DateTime)x.InforcedDate).ToString("dd.MM.yyyy")),
                    SendData = (x.OutDocumentId != null ? (x.OutDocument.DocumentNumber + "/" + x.OutDocument.DocumentDate.ToString("dd.MM.yyyy") +
                                     Environment.NewLine +
                                     string.Join(Environment.NewLine, x.OutDocument.DocumentPersons.Select(a => a.FullName)) +
                                     Environment.NewLine) : "") +
                               (x.InforcedDate != null ? ((DateTime)x.InforcedDate).ToString("dd.MM.yyyy") : ""),
                    ExecuteData = (x.ExecDate != null ? ((DateTime)x.ExecDate).ToString("dd.MM.yyyy") : "") + " " +
                                   (x.EnforceIncomingDocument ?? "") + " " +
                                   x.InforcerInstitution.FullName,
                    SentencePlace = x.ExecInstitution.FullName,
                    AmnestyData = x.AmnestyDocumentNumber,
                    SentenceFinishData = (x.ExecIncomingDocument ?? "") + " " + x.InforcerInstitution.FullName + Environment.NewLine +
                                         (x.InforcerDocumentNumber ?? ""),
                    Description = x.ExecRemark
                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        public byte[] SentenceReportToExcelOne(SentenceFilterReportVM model)
        {
            var dataRows = SentenceReport_Select(userContext.CourtId, model).OrderBy(x => x.SentDate).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }

            NPoiExcelService excelService = GetExcelHtmlTemplate("Sentence");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<SentenceReportVM, object>>>()
                {
                    x => x.Index,
                    x => x.PersonData,
                    x => x.SentenceData,
                    x => x.SendData,
                    x => x.ExecuteData,
                    x => x.SentencePlace,
                    x => x.AmnestyData,
                    x => x.SentenceFinishData,
                    x => x.Description,
                }
            );
            return excelService.ToArray();
        }

        private IQueryable<ExecListReportVM> ExecListReport_Select(int courtId, ExecListFilterReportVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<ExecList, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            Expression<Func<ExecList, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.ExecListObligations.Where(a => a.Obligation.CaseSessionAct.CaseSession.Case.CaseGroupId == model.CaseGroupId).Any();

            var result = repo.AllReadonly<ExecList>()
                .Where(x => x.CourtId == courtId)
                .Where(x => x.IsActive == true)
                .Where(x => x.ExecListTypeId == NomenclatureConstants.ExecListTypes.Country)
                .Where(dateSearch)
                .Select(x => new ExecListReportVM()
                {
                    ExecListNumber = x.RegNumber,
                    CaseRegNumber = string.Join(Environment.NewLine, x.ExecListObligations.Select(a => a.Obligation.CaseSessionAct.CaseSession.Case.CaseGroup.Label + " " +
                                   a.Obligation.CaseSessionAct.CaseSession.Case.RegNumber).Distinct()),
                    CaseSessionActData = string.Join(Environment.NewLine, x.ExecListObligations.Select(a =>
                                    a.Obligation.CaseSessionAct.CaseSession.SessionType.Label + " " +
                                    a.Obligation.CaseSessionAct.CaseSession.DateFrom.ToString("dd.MM.yyyy") + ", " +
                                    a.Obligation.CaseSessionAct.ActType.Label + " " +
                                   (a.Obligation.CaseSessionAct.ActDate != null ? (a.Obligation.CaseSessionAct.RegNumber + "/" +
                                   ((DateTime)a.Obligation.CaseSessionAct.ActDate).ToString("dd.MM.yyyy")) : "") +
                                   Environment.NewLine +
                                   (a.Obligation.CaseSessionAct.ActInforcedDate != null ? ("В законна сила от " +
                                   ((DateTime)a.Obligation.CaseSessionAct.ActInforcedDate).ToString("dd.MM.yyyy")) : "")).Distinct()),
                    obligations = x.ExecListObligations.Select(p => new ExecListObligationReportVM
                    {
                        MoneyTypeName = (p.Obligation.MoneyType.Label + " " + (p.Obligation.MoneyFineType.Label ?? "")).Trim(),
                        Amount = p.Obligation.Amount,
                        PaymentData = string.Join(Environment.NewLine, p.Obligation.ObligationPayments.Where(a => a.IsActive == true)
                                      .Select(a => a.Amount.ToString("0.00") + " внесена на " + a.Payment.PaidDate.ToString("dd.MM.yyyy"))),
                        PersonNameReceive = p.Obligation.ObligationReceives.Select(b => b.FullName).FirstOrDefault(),
                    }),
                    PersonName = string.Join(Environment.NewLine, x.ExecListObligations.Select(a => a.Obligation.FullName + " " + (a.Obligation.Uic ?? "")).Distinct()),
                    ExecListDate = x.RegDate,
                    SendData = x.OutDocumentId != null ? x.OutDocument.DocumentNumber + "/" + x.OutDocument.DocumentDate.ToString("dd.MM.yyyy") : "",
                    Receiver = x.OutDocumentId != null ? string.Join(Environment.NewLine, x.OutDocument.DocumentPersons.Select(a => a.FullName)) :
                               x.DeliveryPersonName,
                    ExecListCaseNumber = x.CaseNumber
                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        public byte[] ExecListReportToExcelOne(ExecListFilterReportVM model)
        {
            var dataRows = ExecListReport_Select(userContext.CourtId, model).OrderBy(x => x.ExecListNumber).ToList();

            NPoiExcelService excelService = GetExcelHtmlTemplate("ExecList");

            for (int i = 0; i < dataRows.Count; i++)
            {
                var row = dataRows[i];
                var rowsRange = row.obligations.Count();
                var addRows = rowsRange;
                if (i == 0)
                    addRows--;

                excelService.InsertRow(true, addRows);
                if (i == 0)
                    excelService.rowIndex -= addRows;
                else
                    excelService.rowIndex -= addRows - 1;

                excelService.InsertRangeMoveCol(row.ExecListNumber, 1, rowsRange);
                excelService.InsertRangeMoveCol(row.CaseRegNumber, 1, rowsRange);
                excelService.InsertRangeMoveCol(row.CaseSessionActData, 1, rowsRange);
                excelService.SetCellData("");
                excelService.SetCellData("");
                excelService.InsertRangeMoveCol(row.PersonName, 1, rowsRange);
                excelService.SetCellData("");
                excelService.InsertRangeMoveCol(row.ExecListDate.ToString("dd.MM.yyyy"), 1, rowsRange);
                excelService.InsertRangeMoveCol(row.SendData, 1, rowsRange);
                excelService.InsertRangeMoveCol(row.Receiver, 1, rowsRange);
                excelService.InsertRangeMoveCol(row.ExecListCaseNumber, 1, rowsRange);
                excelService.SetCellData("");

                bool addRow = false;
                foreach (var item in row.obligations)
                {
                    if (addRow)
                        excelService.rowIndex++;

                    excelService.colIndex = 3;
                    excelService.SetCellData(item.MoneyTypeName);
                    excelService.SetCellData(item.Amount.ToString("0.00"));
                    excelService.colIndex = 6;
                    excelService.SetCellData(item.PersonNameReceive);
                    excelService.colIndex = 11;
                    excelService.SetCellData(item.PaymentData);

                    addRow = true;
                }
            }

            return excelService.ToArray();
        }

        public IQueryable<CaseArchiveListReportVM> CaseArchiveListReport_Select(int courtId, CaseArchiveListFilterReportVM model)
        {
            Expression<Func<CaseArchive, bool>> groupWhere = x => true;
            if (model.CaseGroupId > 0)
                groupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseArchive, bool>> typeWhere = x => true;
            if (model.CaseTypeId > 0)
                typeWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CaseArchive, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            Expression<Func<CaseArchive, bool>> numberSearch = x => true;
            if (string.IsNullOrEmpty(model.ArchiveNumber) == false)
                numberSearch = x => x.RegNumber == model.ArchiveNumber;

            var result = repo.AllReadonly<CaseArchive>()
                .Where(x => x.Case.CourtId == courtId)
                .Where(groupWhere)
                .Where(dateSearch)
                .Where(numberSearch)
                .Where(typeWhere)
                .Select(x => new CaseArchiveListReportVM()
                {
                    CaseId = x.CaseId,
                    CaseTypeName = x.Case.CaseType.Label,
                    CaseNumber = x.Case.RegNumber + "/" + x.Case.RegDate.Year,
                    ArchiveLink = x.ArchiveLink,
                    ArchiveNumber = x.RegNumber,
                    ArchiveYear = x.RegDate.Year,
                    ArchiveDate = x.RegDate,
                    ArchiveIndexName = x.CourtArchiveIndex.Label,
                    StorageYears = x.StorageYears
                });

            return result;
        }

        public byte[] CaseArchiveListReportToExcelOne(CaseArchiveListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CaseArchiveListReport_Select(userContext.CourtId, model).ToList();

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            excelService.AddRange("Справка за архивирани дела за периода от " + dateFrom + " до " + dateTo, 8,
                      excelService.CreateTitleStyle()); excelService.AddRow();
            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseArchiveListReportVM, object>>>()
                {
                    x => x.CaseTypeName,
                    x => x.CaseNumber,
                    x => x.ArchiveLink,
                    x => x.ArchiveNumber,
                    x => x.ArchiveYear,
                    x => x.ArchiveDate,
                    x => x.ArchiveIndexName,
                    x => x.StorageYears,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );
            excelService.AddRow();
            excelService.AddRow();
            excelService.AddRange(dataRows.Count + " бр. записа отговарят на зададените критерии", 9);
            return excelService.ToArray();
        }

        public IQueryable<DocumentOutListReportVM> DocumentOutListReport_Select(int courtId, DocumentOutListFilterReportVM model, string newLine)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<Document, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.DocumentDate.Date >= dateFromSearch.Date && x.DocumentDate.Date <= dateToSearch.Date;

            Expression<Func<Document, bool>> documentGroupWhere = x => true;
            if (model.DocumentGroupId > 0)
                documentGroupWhere = x => x.DocumentGroupId == model.DocumentGroupId;

            Expression<Func<Document, bool>> documentTypeWhere = x => true;
            if (model.DocumentTypeId > 0)
                documentTypeWhere = x => x.DocumentTypeId == model.DocumentTypeId;

            Expression<Func<Document, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.DocumentCaseInfo.Where(a => a.Case.CaseGroupId == model.CaseGroupId).Any();

            Expression<Func<Document, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.DocumentCaseInfo.Where(a => a.Case.CaseTypeId == model.CaseTypeId).Any();

            return repo.AllReadonly<Document>()
                                .Where(x => x.CourtId == courtId && x.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                                .Where(x => x.DateExpired == null)
                                .Where(dateSearch)
                                .Where(documentGroupWhere)
                                .Where(documentTypeWhere)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Select(x => new DocumentOutListReportVM
                                {
                                    DocumentNumber = x.DocumentNumber,
                                    DocumentYear = x.DocumentDate.Year,
                                    DocumentDate = x.DocumentDate,
                                    DocumentData = x.DocumentGroup.Label + " - " + x.DocumentType.Label,
                                    CaseData = x.DocumentCaseInfo.Select(a => a.Case.CaseGroup.Label + " - " + a.Case.CaseType.Label)
                                                .FirstOrDefault(),
                                    DocumentPersons = string.Join(newLine, x.DocumentPersons.Select(a => a.FullName)),
                                    Description = x.Description,
                                    DeliveryGroupName = x.DeliveryGroup.Label,
                                    DocumentNumberValue = x.DocumentNumberValue ?? 0
                                }).AsQueryable();
        }

        public byte[] DocumentOutListReportToExcelOne(DocumentOutListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = DocumentOutListReport_Select(userContext.CourtId, model, Environment.NewLine).OrderBy(x => x.DocumentNumberValue).ToList();

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            excelService.AddRange("Справка за документи от изходящ регистър за периода от " + dateFrom + " до " + dateTo, 8,
                      excelService.CreateTitleStyle()); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<DocumentOutListReportVM, object>>>()
                {
                    x => x.DocumentNumber,
                    x => x.DocumentYear,
                    x => x.DocumentDate,
                    x => x.DocumentData,
                    x => x.CaseData,
                    x => x.DocumentPersons,
                    x => x.Description,
                    x => x.DeliveryGroupName,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );
            return excelService.ToArray();
        }

        public IQueryable<PosDeviceReportVM> PosDeviceReport_Select(int courtId, PosDeviceFilterReportVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<PosPaymentResult, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.PaidDate.Date >= dateFromSearch.Date && x.PaidDate.Date <= dateToSearch.Date;

            Expression<Func<PosPaymentResult, bool>> posWhere = x => true;
            if (string.IsNullOrEmpty(model.PosDeviceTid) == false && model.PosDeviceTid != "-1" && model.PosDeviceTid != "-2")
                posWhere = x => x.Tid == model.PosDeviceTid;

            return repo.AllReadonly<PosPaymentResult>()
                                .Where(x => x.CourtId == courtId && x.Status == MoneyConstants.PosPaymentResultStatus.StatusOk)
                                .Where(x => x.Payment.IsActive)
                                .Where(dateSearch)
                                .Where(posWhere)
                                .Select(x => new
                                {
                                    CourtName = x.Court.Label,
                                    Tid = x.Tid,
                                    Amount = x.Amount,
                                    BankData = repo.AllReadonly<CourtPosDevice>()
                                               .Where(a => a.CourtId == x.CourtId)
                                               .Where(a => a.Tid == x.Tid)
                                               .OrderByDescending(a => a.Id)
                                               .Select(a => a.BIC + " " + a.BankName)
                                               .FirstOrDefault(),
                                })
                                .GroupBy(x => new { x.CourtName, x.BankData })
                                .Select(g => new PosDeviceReportVM
                                {
                                    CourtName = g.Key.CourtName,
                                    PosDeviceCount = g.Select(x => x.Tid).Distinct().Count(),
                                    PaymentCount = g.Count(),
                                    PaymentSum = g.Sum(x => x.Amount),
                                    BankData = g.Key.BankData,
                                })
                                .AsQueryable();
        }

        public byte[] PosDeviceReportToExcelOne(PosDeviceFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = PosDeviceReport_Select(userContext.CourtId, model).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Number = i + 1;
            }

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            excelService.AddRange("СПРАВКА", 6, excelService.CreateTitleStyle()); excelService.AddRow();
            excelService.AddRange("за използваните терминални устройства ПОС и извършени транзакции по чл. 4, ал. 1 " +
                              "от Закона за ограничаване на плащанията в брой", 6, excelService.CreateTitleStyle()); excelService.AddRow();
            excelService.AddRange("от " + dateFrom + " до " + dateTo, 6,
                      excelService.CreateTitleStyle()); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<PosDeviceReportVM, object>>>()
                {
                    x => x.Number,
                    x => x.CourtName,
                    x => x.PosDeviceCount,
                    x => x.PaymentCount,
                    x => x.PaymentSum,
                    x => x.BankData,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );
            return excelService.ToArray();
        }

        private IQueryable<CaseSessionPublicReportVM> CaseSessionPublicReport_Select(int courtId, CaseSessionPublicFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseSession, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseSession, bool>> caseInstanceWhere = x => true;
            if (model.InstanceId > 0)
                caseInstanceWhere = x => x.Case.CaseType.CaseInstanceId == model.InstanceId;

            Expression<Func<CaseSession, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom && a.CourtDepartmentId == model.DepartmentId).Any();

            var result = repo.AllReadonly<CaseSession>()
                             .Where(x => x.Case.CourtId == courtId && x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
                             .Where(x => x.DateFrom >= model.DateFrom && x.DateFrom <= model.DateTo)
                             .Where(x => x.DateExpired == null)
                             .Where(x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                             .Where(caseGroupWhere)
                             .Where(departmentWhere)
                             .Where(caseInstanceWhere)
                             .Select(x => new CaseSessionPublicReportVM
                             {
                                 SessionDate = x.DateFrom,
                                 CaseData = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                 CaseLawUnits = string.Join(", ", x.CaseLawUnits.Where(a => ((a.DateTo ?? dateEnd) >= x.DateFrom) &&
                                                                                            (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(a.JudgeRoleId)))
                                                                                .Select(a => a.LawUnit.FullName)),
                                 CourtDepartmentName = x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom && a.CourtDepartmentId != null)
                                                                     .Select(a => a.CourtDepartment.Label).FirstOrDefault(),
                                 JudgeReporterName = x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom &&
                                                                                a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                   .Select(a => a.LawUnit.FullName)
                                                                   .FirstOrDefault(),
                                 SecretaryName = string.Join(", ", x.CaseSessionMeetings
                                                                    .Select(a => string.Join(", ", a.CaseSessionMeetingUsers
                                                                                 .Select(b => b.SecretaryUser.LawUnit.FullName)))
                                                                    ),
                                 LeftSide = String.Join(Environment.NewLine, x.CasePersons.Where(a => a.DateExpired == null &&
                                                                                                      a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.LeftSide)
                                                                                          .Select(p => p.FullName + " (" + p.PersonRole.Label + ")")),
                                 LeftSideGrajdansko = String.Join(Environment.NewLine, x.CasePersons.Where(a => a.DateExpired == null &&
                                                                                                                a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.LeftSide &&
                                                                                                                (a.PersonRoleId == NomenclatureConstants.PersonRole.Petitioner ||
                                                                                                                 a.PersonRoleId == NomenclatureConstants.PersonRole.Plaintiff ||
                                                                                                                 a.PersonRoleId == NomenclatureConstants.PersonRole.Kreditor))
                                                                                                    .Select(p => p.FullName + " (" + p.PersonRole.Label + ")")),
                                 RightSideGrajdansko = String.Join(Environment.NewLine, x.CasePersons.Where(a => a.DateExpired == null &&
                                                                                                       a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide &&
                                                                                                       (a.PersonRoleId == NomenclatureConstants.PersonRole.libellee ||
                                                                                                        a.PersonRoleId == NomenclatureConstants.PersonRole.Debtor))
                                                                                           .Select(p => p.FullName + " (" + p.PersonRole.Label + ")")),
                                 RightSide = String.Join(Environment.NewLine, x.CasePersons.Where(a => a.DateExpired == null &&
                                                                                                       a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide)
                                                                                           .Select(p => p.FullName + " (" + p.PersonRole.Label + ")")),
                                 SessionResult = String.Join(Environment.NewLine, x.CaseSessionResults.Where(a => a.IsMain && a.IsActive).Select(p => p.SessionResult.Label + ((p.SessionResultBaseId != null) ? (" - " + p.SessionResultBase.Label) : string.Empty))),
                                 SessionAct = string.Join("; ", x.CaseSessionActs.Where(d => d.ActDate != null && d.DateExpired == null)
                                                                                 .Select(d => d.ActType.Label + " №" + d.RegNumber + "/" + ((DateTime)d.ActDate).ToString("dd.MM.yyyy"))),
                                 SessionActDecision = (x.CaseSessionActs.Any(act => act.IsFinalDoc && act.DateExpired == null)) ? x.CaseSessionActs.Where(act => act.IsFinalDoc && act.DateExpired == null)
                                                                                                                                                                                              .Select(act => act.ActType.Label + " №" + act.RegNumber + "/" + (act.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") + (act.Description != null ? " " + act.Description : string.Empty)).FirstOrDefault() : string.Empty,
                                 SessionAdjourn = string.Join(",", x.CaseSessionResults.Where(sr => sr.SessionResult.SessionResultGroupId != null).Select(sr => sr.SessionResult.Label + ((sr.SessionResultBaseId != null) ? (" - " + sr.SessionResultBase.Label) : string.Empty))) + (x.CaseSessionResults.Any(csr => csr.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Procrastination) ? (x.Case.CaseSessions.Where(css => css.DateFrom > x.DateFrom && css.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno).OrderBy(css => css.DateFrom).Select(css => " Насрочено за: " + css.DateFrom.ToString("dd.MM.yyyy")).FirstOrDefault() + " " + string.Join("; ", x.CaseSessionActs.Where(d => d.ActDate != null && d.DateExpired == null)
                                                                                 .Select(d => d.Description))) : string.Empty),
                                 CaseTypeCode = x.Case.CaseType.Code,
                                 CaseCodeName = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label,
                                 ProsecutorName = string.Join(",", x.CasePersons.Where(p => p.PersonRoleId == NomenclatureConstants.PersonRole.Prosecutor &&
                                                                                            (p.DateTo ?? dateEnd) >= x.DateFrom)
                                                                                .Select(p => p.FullName)),
                                 DateCaseOffice = x.CaseSessionActs.Where(actActDate => actActDate.ActStateId != NomenclatureConstants.SessionActState.Project && actActDate.ActStateId != NomenclatureConstants.SessionActState.Registered && actActDate.ActDeclaredDate != null && actActDate.DateExpired == null).Select(actActDate => actActDate.ActDeclaredDate).FirstOrDefault()
                             }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        private IQueryable<CaseSessionPublicReportVM> CaseSessionPublicCompanyFirstInstanceReport_Select(int courtId, CaseSessionPublicFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseSession, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseSession, bool>> caseInstanceWhere = x => true;
            if (model.InstanceId > 0)
                caseInstanceWhere = x => x.Case.CaseType.CaseInstanceId == model.InstanceId;

            Expression<Func<CaseSession, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom && a.CourtDepartmentId == model.DepartmentId).Any();

            var result = repo.AllReadonly<CaseSession>()
                                .Where(x => x.Case.CourtId == courtId && x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
                                .Where(x => x.DateFrom >= model.DateFrom && x.DateFrom <= model.DateTo)
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                                .Where(caseGroupWhere)
                                .Where(departmentWhere)
                                .Where(caseInstanceWhere)
                                .Select(x => new CaseSessionPublicReportVM
                                {
                                    CaseLifecycleMonths = x.Case.CaseLifecycles.Select(a => a.DurationMonths).Sum(),
                                    SessionDate = x.DateFrom,
                                    CaseData = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                    JudgeReporterName = x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom &&
                                                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                   .Select(a => a.LawUnit.FullName)
                                                   .FirstOrDefault(),
                                    SecretaryName = string.Join(", ", x.CaseSessionMeetings
                                                 .Select(a => string.Join(", ", a.CaseSessionMeetingUsers
                                                              .Select(b => b.SecretaryUser.LawUnit.FullName)))
                                                 ),
                                    CaseCodeName = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label,
                                    CasePersons = String.Join(Environment.NewLine, x.CasePersons.Where(a => a.DateExpired == null &&
                                                                      a.CaseSessionId == null)
                                                                      .Select(p => p.FullName)),
                                    SessionResult = String.Join(Environment.NewLine,
                                             x.CaseSessionResults.Where(a => a.IsMain && a.IsActive).Select(p => p.SessionResult.Label)),
                                    SessionAdjourn = x.SessionStateId == NomenclatureConstants.SessionState.Cancel ? x.Description : "",
                                    ActResultName = string.Join(",", x.CaseSessionActs.Where(b => b.DateExpired == null && b.ActResultId != null)
                                                                             .Select(b => b.ActResult.Label)),
                                    IsNewNumberCase = repo.AllReadonly<CaseMigration>()
                                                      .Where(a => a.CaseId == x.CaseId && a.CaseId != a.InitialCaseId && a.DateExpired == null)
                                                      .Any(),
                                    IsStop = x.CaseSessionResults.Where(a => a.IsMain && a.IsActive && a.DateExpired == null &&
                                                     a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Stop)
                                                        .Any()
                                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        private IQueryable<CaseSessionPublicReportVM> CaseSessionPublicNakazatelnoRegionalReport_Select(int courtId, CaseSessionPublicFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CaseSession, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseSession, bool>> caseInstanceWhere = x => true;
            if (model.InstanceId > 0)
                caseInstanceWhere = x => x.Case.CaseType.CaseInstanceId == model.InstanceId;

            Expression<Func<CaseSession, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom && a.CourtDepartmentId == model.DepartmentId).Any();

            var roles = new int[] {NomenclatureConstants.PersonRole.Offender, NomenclatureConstants.PersonRole.Defendant,
                                NomenclatureConstants.PersonRole.Prisoner};

            var result = repo.AllReadonly<CaseSession>()
                                .Where(x => x.Case.CourtId == courtId && x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
                                .Where(x => x.DateFrom >= model.DateFrom && x.DateFrom <= model.DateTo)
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                                .Where(caseGroupWhere)
                                .Where(departmentWhere)
                                .Where(caseInstanceWhere)
                                .Select(x => new CaseSessionPublicReportVM
                                {
                                    SessionDate = x.DateFrom,
                                    CaseData = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                    CaseLawUnits = string.Join(", ", x.CaseLawUnits.Where(a => ((a.DateTo ?? dateEnd) >= x.DateFrom) &&
                                                                                               (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(a.JudgeRoleId)))
                                                                                .Select(a => a.LawUnit.FullName)),
                                    CourtDepartmentName = x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom && a.CourtDepartmentId != null)
                                                                     .Select(a => a.CourtDepartment.Label).FirstOrDefault(),
                                    JudgeReporterName = x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom &&
                                                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                   .Select(a => a.LawUnit.FullName)
                                                   .FirstOrDefault(),
                                    SecretaryName = string.Join(", ", x.CaseSessionMeetings
                                                 .Select(a => string.Join(", ", a.CaseSessionMeetingUsers
                                                              .Select(b => b.SecretaryUser.LawUnit.FullName)))
                                                 ),
                                    CaseTypeId = x.Case.CaseTypeId,
                                    CaseCodeName = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label,
                                    SessionActDecision = string.Join("; ", x.CaseSessionActs.Where(d => d.ActDate != null &&
                                                              d.DateExpired == null && d.IsFinalDoc)
                                                             .Select(d => d.ActType.Label + " " + d.RegNumber + "/" + ((DateTime)d.ActDate).ToString("dd.MM.yyyy") +
                                                             Environment.NewLine + d.Description)
                                                             ),
                                    SessionAdjourn = x.SessionStateId == NomenclatureConstants.SessionState.Cancel ? x.Description : "",
                                    DateCaseOffice = x.CaseSessionActs.Where(d => d.ActDeclaredDate != null && d.DateExpired == null)
                                                             .Select(d => d.ActDeclaredDate)
                                                             .FirstOrDefault(),
                                    RightSide = String.Join(Environment.NewLine, x.CasePersons.Where(a => a.DateExpired == null &&
                                                                                                          a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide)
                                                                                           .Select(p => p.FullName + " (" + p.PersonRole.Label + ")")),
                                    ProsecutorName = string.Join(",", x.CasePersons.Where(p => p.PersonRoleId == NomenclatureConstants.PersonRole.Prosecutor &&
                                                                                               (p.DateTo ?? dateEnd) >= x.DateFrom)
                                                                                .Select(p => p.FullName)),
                                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }
        private string CaseSessionPublicReportCaption(int groupId, int instanceId)
        {
            string result = "";

            string instance = "";
            switch (instanceId)
            {
                case NomenclatureConstants.CaseInstanceType.FirstInstance:
                    instance = "ПЪРВОИНСТАНЦИОННИ";
                    break;
                case NomenclatureConstants.CaseInstanceType.SecondInstance:
                    instance = "ВЪЗЗИВНИ";
                    break;
                default:
                    break;
            }

            string group = "";
            switch (groupId)
            {
                case NomenclatureConstants.CaseGroups.Company:
                    group = "Фирмени дела";
                    break;
                case NomenclatureConstants.CaseGroups.GrajdanskoDelo:
                    group = "Граждански дела";
                    break;
                case NomenclatureConstants.CaseGroups.NakazatelnoDelo:
                    group = "Наказателни дела";
                    break;
                case NomenclatureConstants.CaseGroups.Trade:
                    group = "Търговски дела";
                    break;
                default:
                    break;
            }

            result = "СРОЧНА КНИГА " + instance + " " + group;

            return result;
        }

        private byte[] CaseSessionPublicReportToExcelOneSecondInstance(CaseSessionPublicFilterReportVM model)
        {
            var dataRows = CaseSessionPublicReport_Select(userContext.CourtId, model).OrderBy(x => x.SessionDate).ToList();

            string caption = CaseSessionPublicReportCaption(model.CaseGroupId, model.InstanceId);
            var htmlTemplate = GetHtmlTemplate("SessionPublic1");
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 1;
            excelService.SetCellData(caption);

            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;
            excelService.colIndex = 0;
            for (int i = 0; i < dataRows.Count; i++)
            {
                bool addDate = false;

                var row = dataRows[i];
                row.Index = i + 1;
                if (i == 0 || row.SessionDate.Date != dataRows[i - 1].SessionDate.Date)
                    addDate = true;

                if (addDate)
                {
                    if (i != 0)
                        excelService.InsertRow(true);
                    excelService.InsertRangeMoveCol(row.SessionDate.ToString("dd.MM.yyyy"), 15, 1);
                }

                excelService.InsertRow(true);
                excelService.SetCellData(row.SessionDate.ToString("dd.MM.yyyy"));
                excelService.SetCellData(row.Index.ToString());
                excelService.SetCellData(row.CaseData);
                excelService.SetCellData(string.IsNullOrEmpty(row.CourtDepartmentName) == true ? row.CaseLawUnits :
                                     (row.CourtDepartmentName + Environment.NewLine + row.CaseLawUnits));
                excelService.SetCellData(row.JudgeReporterName);
                excelService.SetCellData(row.ProsecutorName);
                excelService.SetCellData(row.SecretaryName);
                excelService.SetCellData(row.LeftSide);
                excelService.SetCellData(row.RightSide);
                excelService.SetCellData(row.SessionResult);
                excelService.SetCellData(row.SessionAct);
                excelService.SetCellData(row.SessionActDecision);
                excelService.SetCellData(row.SessionAdjourn);
                excelService.SetCellData(row.DateCaseOffice != null ? ((DateTime)row.DateCaseOffice).ToString("dd.MM.yyyy") : "");
                excelService.SetCellData(row.Signature);
            }

            return excelService.ToArray();
        }

        private byte[] CaseSessionPublicReportToExcelOneFirstInstanceGrajdansko(CaseSessionPublicFilterReportVM model)
        {
            var dataRows = CaseSessionPublicReport_Select(userContext.CourtId, model).OrderBy(x => x.SessionDate).ToList();

            string caption = CaseSessionPublicReportCaption(model.CaseGroupId, model.InstanceId);
            var htmlTemplate = GetHtmlTemplate("SessionPublic2");
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 1;
            excelService.SetCellData(caption);

            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;
            excelService.colIndex = 0;
            for (int i = 0; i < dataRows.Count; i++)
            {
                bool addDate = false;

                var row = dataRows[i];
                row.Index = i + 1;
                if (i == 0 || row.SessionDate.Date != dataRows[i - 1].SessionDate.Date)
                    addDate = true;

                if (addDate)
                {
                    if (i != 0)
                        excelService.InsertRow(true);
                    excelService.InsertRangeMoveCol(row.SessionDate.ToString("dd.MM.yyyy"), 14, 1);
                }
                excelService.InsertRow(true);
                excelService.SetCellData(row.SessionDate.ToString("dd.MM.yyyy"));
                excelService.SetCellData(row.Index.ToString());
                excelService.SetCellData(row.CaseData);
                excelService.SetCellData(row.CaseTypeCode);
                excelService.SetCellData(row.LeftSideGrajdansko);
                excelService.SetCellData(row.RightSideGrajdansko);
                excelService.SetCellData(string.IsNullOrEmpty(row.CourtDepartmentName) == true ? row.CaseLawUnits :
                                     (row.CourtDepartmentName + Environment.NewLine + row.CaseLawUnits));
                excelService.SetCellData(row.JudgeReporterName);
                excelService.SetCellData(row.ProsecutorName);
                excelService.SetCellData(row.SecretaryName);
                excelService.SetCellData(row.SessionActDecision);
                excelService.SetCellData(row.SessionAdjourn);
                excelService.SetCellData(row.DateCaseOffice != null ? ((DateTime)row.DateCaseOffice).ToString("dd.MM.yyyy") : "");
                excelService.SetCellData(row.Signature);
            }

            return excelService.ToArray();
        }

        private byte[] CaseSessionPublicReportToExcelOneFirstInstanceNakazatelnoDistrictCourt(CaseSessionPublicFilterReportVM model)
        {
            var dataRows = CaseSessionPublicReport_Select(userContext.CourtId, model).OrderBy(x => x.SessionDate).ToList();

            NPoiExcelService excelService = GetExcelHtmlTemplate("SessionPublic3");
            for (int i = 0; i < dataRows.Count; i++)
            {
                bool addDate = false;

                var row = dataRows[i];
                row.Index = i + 1;
                if (i == 0 || row.SessionDate.Date != dataRows[i - 1].SessionDate.Date)
                    addDate = true;

                if (addDate)
                {
                    if (i != 0)
                        excelService.InsertRow(true);
                    excelService.InsertRangeMoveCol(row.SessionDate.ToString("dd.MM.yyyy"), 13, 1);
                }
                excelService.InsertRow(true);
                excelService.SetCellData(row.SessionDate.ToString("dd.MM.yyyy"));
                excelService.SetCellData(row.Index.ToString());
                excelService.SetCellData(row.CaseData);
                excelService.SetCellData(row.CaseCodeName);
                excelService.SetCellData(string.IsNullOrEmpty(row.CourtDepartmentName) == true ? row.CaseLawUnits :
                                     (row.CourtDepartmentName + Environment.NewLine + row.CaseLawUnits));
                excelService.SetCellData(row.JudgeReporterName);
                excelService.SetCellData(row.ProsecutorName);
                excelService.SetCellData(row.SecretaryName);
                excelService.SetCellData(row.RightSide);
                excelService.SetCellData(row.SessionActDecision);
                excelService.SetCellData(row.SessionAdjourn);
                excelService.SetCellData(row.DateCaseOffice != null ? ((DateTime)row.DateCaseOffice).ToString("dd.MM.yyyy") : "");
                excelService.SetCellData(row.Signature);
            }

            return excelService.ToArray();
        }

        private byte[] CaseSessionPublicReportToExcelOneCompanyFirstInstance(CaseSessionPublicFilterReportVM model)
        {
            var dataRows = CaseSessionPublicCompanyFirstInstanceReport_Select(userContext.CourtId, model).OrderBy(x => x.SessionDate).ToList();
            NPoiExcelService excelService = GetExcelHtmlTemplate("SessionPublic5");

            for (int i = 0; i < dataRows.Count; i++)
            {
                bool addDate = false;

                var row = dataRows[i];
                row.Index = i + 1;
                if (i == 0 || row.SessionDate.Date != dataRows[i - 1].SessionDate.Date)
                    addDate = true;

                if (addDate)
                {
                    if (i != 0)
                        excelService.InsertRow(true);
                    excelService.InsertRangeMoveCol(row.SessionDate.ToString("dd.MM.yyyy"), 18, 1);
                }
                excelService.InsertRow(true);
                excelService.SetCellData(row.SessionDate.ToString("dd.MM.yyyy"));
                excelService.SetCellData(row.JudgeReporterName);
                excelService.SetCellData(row.JudgeReporterName);
                excelService.SetCellData(row.ProsecutorName);
                excelService.SetCellData(row.SecretaryName);
                excelService.SetCellData(row.Index.ToString());
                excelService.SetCellData(row.CaseData);
                excelService.SetCellData(row.CaseCodeName);
                excelService.SetCellData(row.IsNewNumberCase == true ? "*" : "");
                excelService.SetCellData(row.ActResultName);
                excelService.SetCellData(row.CasePersons);
                excelService.SetCellData(row.SessionResult);
                excelService.SetCellData(row.SessionAdjourn);
                excelService.SetCellData(row.IsStop == true ? "*" : "");
                excelService.SetCellData(row.CaseLifecycleMonths == 1 ? "*" : "");
                excelService.SetCellData((row.CaseLifecycleMonths > 1 && row.CaseLifecycleMonths <= 3) ? "*" : "");
                excelService.SetCellData(row.CaseLifecycleMonths > 3 ? "*" : "");
                excelService.SetCellData(row.Signature);
            }

            return excelService.ToArray();
        }
        private byte[] CaseSessionPublicReportToExcelOneNakazatelnoRegional(CaseSessionPublicFilterReportVM model)
        {
            var dataRows = CaseSessionPublicNakazatelnoRegionalReport_Select(userContext.CourtId, model).OrderBy(x => x.SessionDate).ToList();

            // Добавен е шаблона от окръжен съд в районен и коментираното е предният вариант ако искат да го върнем. Несъответствие #29136
            NPoiExcelService excelService = GetExcelHtmlTemplate("SessionPublic3");
            for (int i = 0; i < dataRows.Count; i++)
            {
                bool addDate = false;

                var row = dataRows[i];
                row.Index = i + 1;
                if (i == 0 || row.SessionDate.Date != dataRows[i - 1].SessionDate.Date)
                    addDate = true;

                if (addDate)
                {
                    if (i != 0)
                        excelService.InsertRow(true);
                    excelService.InsertRangeMoveCol(row.SessionDate.ToString("dd.MM.yyyy"), 13, 1);
                }
                excelService.InsertRow(true);
                excelService.SetCellData(row.SessionDate.ToString("dd.MM.yyyy"));
                excelService.SetCellData(row.Index.ToString());
                excelService.SetCellData(row.CaseData);
                excelService.SetCellData(row.CaseCodeName);
                excelService.SetCellData(string.IsNullOrEmpty(row.CourtDepartmentName) == true ? row.CaseLawUnits :
                                     (row.CourtDepartmentName + Environment.NewLine + row.CaseLawUnits));
                excelService.SetCellData(row.JudgeReporterName);
                excelService.SetCellData(row.ProsecutorName);
                excelService.SetCellData(row.SecretaryName);
                excelService.SetCellData(row.RightSide);
                excelService.SetCellData(row.SessionActDecision);
                excelService.SetCellData(row.SessionAdjourn);
                excelService.SetCellData(row.DateCaseOffice != null ? ((DateTime)row.DateCaseOffice).ToString("dd.MM.yyyy") : "");
                excelService.SetCellData(row.Signature);
            }

            //NPoiExcelService excelService = GetExcelHtmlTemplate("SessionPublic4");
            //for (int i = 0; i < dataRows.Count; i++)
            //{
            //    bool addDate = false;

            //    var row = dataRows[i];
            //    row.Index = i + 1;
            //    if (i == 0 || row.SessionDate.Date != dataRows[i - 1].SessionDate.Date)
            //        addDate = true;

            //    if (addDate)
            //    {
            //        if (i != 0)
            //            excelService.InsertRow(true);
            //        excelService.InsertRangeMoveCol(row.SessionDate.ToString("dd.MM.yyyy"), 27, 1);
            //    }

            //    excelService.InsertRow(true);
            //    excelService.SetCellData(row.SessionDate.ToString("dd.MM.yyyy"));
            //    excelService.SetCellData(row.JudgeReporterName);
            //    excelService.SetCellData(row.JudgeReporterName);
            //    excelService.SetCellData(row.ProsecutorName);
            //    excelService.SetCellData(row.LawUnitJury);
            //    excelService.SetCellData(row.SecretaryName);
            //    excelService.SetCellData(row.Index.ToString());
            //    excelService.SetCellData(row.CaseTypeId == NomenclatureConstants.CaseTypes.NOHD ? row.CaseData : "");
            //    excelService.SetCellData(row.CaseTypeId == NomenclatureConstants.CaseTypes.NChHD ? row.CaseData : "");
            //    excelService.SetCellData(row.CaseTypeId == NomenclatureConstants.CaseTypes.AND ? row.CaseData : "");
            //    excelService.SetCellData(row.CaseTypeId == NomenclatureConstants.CaseTypes.ChND ? row.CaseData : "");
            //    excelService.SetCellData(row.IsNewNumberCase == true ? "*" : "");
            //    excelService.SetCellData(row.IsOldNumberCase == true ? "*" : "");
            //    excelService.SetCellData(row.CaseCodeName);
            //    excelService.SetCellData(row.CasePersonCount.ToString());
            //    excelService.SetCellData(row.CasePersonUnder18Count.ToString());
            //    excelService.SetCellData(row.SessionAdjourn);
            //    excelService.SetCellData(row.IsStop == true ? "*" : "");
            //    excelService.SetCellData(row.CaseLifecycleMonths == 1 ? "*" : "");
            //    excelService.SetCellData((row.CaseLifecycleMonths > 1 && row.CaseLifecycleMonths <= 3) ? "*" : "");
            //    excelService.SetCellData(row.CaseLifecycleMonths > 3 ? "*" : "");
            //    excelService.SetCellData(row.CasePersons);
            //    excelService.SetCellData(row.SessionActDecision);
            //    excelService.SetCellData(row.RecidiveGeneral);
            //    excelService.SetCellData(row.RecidiveSpecial);
            //    excelService.SetCellData(row.RecidiveDanger);
            //    excelService.SetCellData(row.ActInforcedDate?.ToString("dd.MM.yyyy"));
            //}

            return excelService.ToArray();
        }

        public byte[] CaseSessionPublicReportToExcelOne(CaseSessionPublicFilterReportVM model)
        {
            if (model.InstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance)
            {
                return CaseSessionPublicReportToExcelOneSecondInstance(model);
            }
            else if (model.InstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance && model.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo)
            {
                return CaseSessionPublicReportToExcelOneFirstInstanceGrajdansko(model);
            }
            else if (model.InstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance && model.CaseGroupId == NomenclatureConstants.CaseGroups.Trade)
            {
                return CaseSessionPublicReportToExcelOneFirstInstanceGrajdansko(model);
            }
            else if (model.InstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance &&
                 model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo &&
                 userContext.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
            {
                return CaseSessionPublicReportToExcelOneFirstInstanceNakazatelnoDistrictCourt(model);
            }
            else if (model.InstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance &&
                model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo &&
                userContext.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
            {
                return CaseSessionPublicReportToExcelOneNakazatelnoRegional(model);
            }
            else if (model.InstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance &&
                model.CaseGroupId == NomenclatureConstants.CaseGroups.Company)
            {
                return CaseSessionPublicReportToExcelOneCompanyFirstInstance(model);
            }
            else
            {
                NPoiExcelService excelService = new NPoiExcelService("Sheet1");
                return excelService.ToArray();
            }
        }

        public byte[] CaseSessionPrivateReportToExcelOneTemplate(CaseSessionPrivateFilterReportVM model)
        {
            var dataRows = CaseSessionPrivateReport_Select(userContext.CourtId, model).OrderBy(x => x.ActDateOrder).ToList();

            var htmlTemplate = GetHtmlTemplate("SessionPrivate");
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);

            if (model.CaseGroupId > 0)
            {
                excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 1;
                excelService.SetCellData(CaseGroupCaption_Title(model.CaseGroupId));
            }

            excelService.colIndex = 0;
            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;

                var row = dataRows[i];
                var rowsRange = row.acts.Count() + 1;

                var addRows = rowsRange;
                if (i == 0)
                    addRows--;

                excelService.InsertRow(true, addRows);
                if (i == 0)
                    excelService.rowIndex -= addRows;
                else
                    excelService.rowIndex -= addRows - 1;

                excelService.InsertRangeMoveCol(row.Index.ToString(), 1, rowsRange);
                excelService.InsertRangeMoveCol(row.CaseNumber, 1, rowsRange);
                excelService.InsertRangeMoveCol(row.CompartmentName, 1, rowsRange);
                excelService.InsertRangeMoveCol(row.JudgeReporterName, 1, rowsRange);
                excelService.SetCellData("");
                excelService.SetCellData(row.CaseSessionResultName);
                excelService.SetCellData("");

                foreach (var item in row.acts)
                {
                    excelService.rowIndex++;
                    excelService.colIndex = 4;
                    excelService.SetCellData(item.ActTypeName + " № " + item.ActNumber + "/" + item.ActDate.ToString("dd.MM.yyyy"));
                    excelService.SetCellData(item.ActDescription);
                    excelService.SetCellData(item.ActDeclaredDate?.ToString("dd.MM.yyyy"));
                }
            }

            excelService.rowIndex += (htmlTemplate.XlsRecapRow ?? 0) - (htmlTemplate.XlsDataRow ?? 0);
            excelService.colIndex = 0;
            var space = "   ";
            var caseGroups = repo.AllReadonly<CaseGroup>().ToList();

            if (model.CaseGroupId <= 0 || model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                var dataRowsNakazatelno = dataRows.Where(x => x.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo).ToList();
                var code3000 = repo.AllReadonly<CaseCodeGrouping>()
                    .Where(x => x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.CaseSessionPrivateReportFirstInstanceCriminal)
                    .Select(x => x.CaseCodeId)
                    .ToList();
                var resultNakazatelno = repo.AllReadonly<SessionResultGrouping>()
                    .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseSessionReportFirstInstanceCriminal)
                    .Select(x => x.SessionResultId)
                    .ToList();
                excelService.AddRange("НАКАЗАТЕЛНИ ДЕЛА", 4); excelService.AddRow();
                excelService.AddRange("Рекапитулация за " + model.DateFrom.ToString("dd.MM.yyyy") + " - " + model.DateTo.ToString("dd.MM.yyyy"), 4); excelService.AddRow();
                CaseSessionPrivateAddTextCountToExcel(excelService, "Всички заседания", dataRowsNakazatelno.Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "Свършени I инстанция", dataRowsNakazatelno.Where(x => x.ActEnforcedFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "решени - без досъдебни", dataRowsNakazatelno
                                          .Where(x => x.ActEnforcedFinal == true &&
                                                 (x.CaseTypeId == NomenclatureConstants.CaseTypes.ChND && code3000.Contains(x.CaseCodeId)) == false &&
                                                 resultNakazatelno.Where(a => (x.CaseSessionResult.Split(",").ToList()).Contains(a.ToString())).Any() == false &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "прекратени - без досъдебни", dataRowsNakazatelno
                                          .Where(x => x.ActEnforcedFinal == true &&
                                                 (x.CaseTypeId == NomenclatureConstants.CaseTypes.ChND && code3000.Contains(x.CaseCodeId)) == false &&
                                                 resultNakazatelno.Where(a => (x.CaseSessionResult.Split(",").ToList()).Contains(a.ToString())).Any() == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "решени - досъдебни", dataRowsNakazatelno
                                          .Where(x => x.ActEnforcedFinal == true &&
                                                 (x.CaseTypeId == NomenclatureConstants.CaseTypes.ChND && code3000.Contains(x.CaseCodeId)) == true &&
                                                 resultNakazatelno.Where(a => (x.CaseSessionResult.Split(",").ToList()).Contains(a.ToString())).Any() == false &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "прекратени - досъдебни", dataRowsNakazatelno
                                          .Where(x => x.ActEnforcedFinal == true &&
                                                 (x.CaseTypeId == NomenclatureConstants.CaseTypes.ChND && code3000.Contains(x.CaseCodeId)) == true &&
                                                 resultNakazatelno.Where(a => (x.CaseSessionResult.Split(",").ToList()).Contains(a.ToString())).Any() == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "С определение, разпореждане и допълнителни решения I инстанция", dataRowsNakazatelno.Where(x => x.ActEnforcedNoFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());

                var documentTypeSecondInstance = repo.AllReadonly<DocumentTypeGrouping>()
                     .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.PrivateSessionSecondInstanceCriminal)
                     .Select(x => x.DocumentTypeId)
                     .ToList();
                var code8000 = repo.AllReadonly<CaseCodeGrouping>()
                    .Where(x => x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.CaseSessionPrivateReportSecondInstanceCriminal)
                    .Select(x => x.CaseCodeId)
                    .ToList();
                CaseSessionPrivateAddTextCountToExcel(excelService, "Свършени II касационна инстанция", dataRowsNakazatelno.Where(x => x.ActEnforcedFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "по жалби и протести - без досъдебни", dataRowsNakazatelno.
                    Where(x => x.ActEnforcedFinal == true && documentTypeSecondInstance.Contains(x.DocumentTypeId) == false &&
                    (x.CaseTypeId == NomenclatureConstants.CaseTypes.VChND && code8000.Contains(x.CaseCodeId)) == false &&
                    x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "по частни жалби и протести - без досъдебни", dataRowsNakazatelno.
                    Where(x => x.ActEnforcedFinal == true && documentTypeSecondInstance.Contains(x.DocumentTypeId) == true &&
                    (x.CaseTypeId == NomenclatureConstants.CaseTypes.VChND && code8000.Contains(x.CaseCodeId)) == false &&
                    x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "по жалби и протести - досъдебни", dataRowsNakazatelno.
                    Where(x => x.ActEnforcedFinal == true && documentTypeSecondInstance.Contains(x.DocumentTypeId) == false &&
                    (x.CaseTypeId == NomenclatureConstants.CaseTypes.VChND && code8000.Contains(x.CaseCodeId)) == true &&
                    x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "по частни жалби и протести - досъдебни", dataRowsNakazatelno.
                    Where(x => x.ActEnforcedFinal == true && documentTypeSecondInstance.Contains(x.DocumentTypeId) == true &&
                    (x.CaseTypeId == NomenclatureConstants.CaseTypes.VChND && code8000.Contains(x.CaseCodeId)) == true &&
                    x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "С определение, разпореждане и допълнителни решения II инстанция", dataRowsNakazatelno.
                    Where(x => x.ActEnforcedNoFinal == true &&
                    //documentTypeSecondInstance.Contains(x.DocumentTypeId) == true &&
                    //(x.CaseTypeId == NomenclatureConstants.CaseType.VChND && code8000.Contains(x.CaseCodeId)) == true &&
                    x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());

                excelService.AddRow();
                excelService.AddRow();
            }

            var resultGrajdansko = repo.AllReadonly<SessionResultGrouping>()
                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseSessionReportFirstInstanceCriminal)
                .Select(x => x.SessionResultId)
                .ToList();

            var documentTypeSecondInstanceCivil = repo.AllReadonly<DocumentTypeGrouping>()
                 .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.PrivateSessionSecondInstanceCivil)
                 .Select(x => x.DocumentTypeId)
                 .ToList();
            var documentTypeSlowSecondInstanceCivil = repo.AllReadonly<DocumentTypeGrouping>()
                 .Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.PrivateSessionSlowSecondInstanceCivil)
                 .Select(x => x.DocumentTypeId)
                 .ToList();

            if (model.CaseGroupId <= 0 || model.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo)
            {
                var dataRowsGrajdansko = dataRows.Where(x => x.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo).ToList();

                excelService.AddRange("ГРАЖДАНСКИ ДЕЛА", 4); excelService.AddRow();
                excelService.AddRange("Рекапитулация за " + model.DateFrom.ToString("dd.MM.yyyy") + " - " + model.DateTo.ToString("dd.MM.yyyy"), 4); excelService.AddRow();
                CaseSessionPrivateAddTextCountToExcel(excelService, "Всички заседания", dataRowsGrajdansko.Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "Свършени I инстанция", dataRowsGrajdansko.Where(x => x.ActEnforcedFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "решени", dataRowsGrajdansko
                                          .Where(x => x.ActEnforcedFinal == true &&
                                                 resultGrajdansko.Where(a => (x.CaseSessionResult.Split(",").ToList()).Contains(a.ToString())).Any() == false &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "прекратени", dataRowsGrajdansko
                                          .Where(x => x.ActEnforcedFinal == true &&
                                                 resultGrajdansko.Where(a => (x.CaseSessionResult.Split(",").ToList()).Contains(a.ToString())).Any() == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "С определение, разпореждане и допълнителни решения I инстанция", dataRowsGrajdansko.Where(x => x.ActEnforcedNoFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());

                CaseSessionPrivateAddTextCountToExcel(excelService, "Свършени II касационна инстанция", dataRowsGrajdansko.Where(x => x.ActEnforcedFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "по жалби и протести", dataRowsGrajdansko.Where(x => x.ActEnforcedFinal == true &&
                                                      documentTypeSecondInstanceCivil.Contains(x.DocumentTypeId) == false &&
                                                      documentTypeSlowSecondInstanceCivil.Contains(x.DocumentTypeId) == false &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "по частни жалби и протести", dataRowsGrajdansko.Where(x => x.ActEnforcedFinal == true &&
                                                      documentTypeSecondInstanceCivil.Contains(x.DocumentTypeId) == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "по молба за срок за бавност", dataRowsGrajdansko.Where(x => x.ActEnforcedFinal == true &&
                                                      documentTypeSlowSecondInstanceCivil.Contains(x.DocumentTypeId) == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "С определение, разпореждане и допълнителни решения II инстанция", dataRowsGrajdansko.Where(x => x.ActEnforcedNoFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());

                excelService.AddRow();
                excelService.AddRow();
            }

            if (model.CaseGroupId <= 0 || model.CaseGroupId == NomenclatureConstants.CaseGroups.Trade)
            {
                var dataRowsTrade = dataRows.Where(x => x.CaseGroupId == NomenclatureConstants.CaseGroups.Trade).ToList();
                excelService.AddRange("ТЪРГОВСКИ ДЕЛА", 4); excelService.AddRow();
                excelService.AddRange("Рекапитулация за " + model.DateFrom.ToString("dd.MM.yyyy") + " - " + model.DateTo.ToString("dd.MM.yyyy"), 4); excelService.AddRow();
                CaseSessionPrivateAddTextCountToExcel(excelService, "Всички заседания", dataRowsTrade.Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "Свършени I инстанция", dataRowsTrade.Where(x => x.ActEnforcedFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "решени", dataRowsTrade
                                          .Where(x => x.ActEnforcedFinal == true &&
                                                 resultGrajdansko.Where(a => (x.CaseSessionResult.Split(",").ToList()).Contains(a.ToString())).Any() == false &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "прекратени", dataRowsTrade
                                          .Where(x => x.ActEnforcedFinal == true &&
                                                 resultGrajdansko.Where(a => (x.CaseSessionResult.Split(",").ToList()).Contains(a.ToString())).Any() == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "С определение, разпореждане и допълнителни решения I инстанция", dataRowsTrade.Where(x => x.ActEnforcedNoFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());

                CaseSessionPrivateAddTextCountToExcel(excelService, "Свършени II касационна инстанция", dataRowsTrade.Where(x => x.ActEnforcedFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "по жалби и протести", dataRowsTrade.Where(x => x.ActEnforcedFinal == true &&
                                                      documentTypeSecondInstanceCivil.Contains(x.DocumentTypeId) == false &&
                                                      documentTypeSlowSecondInstanceCivil.Contains(x.DocumentTypeId) == false &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "по частни жалби и протести", dataRowsTrade.Where(x => x.ActEnforcedFinal == true &&
                                                      documentTypeSecondInstanceCivil.Contains(x.DocumentTypeId) == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "по молба за срок за бавност", dataRowsTrade.Where(x => x.ActEnforcedFinal == true &&
                                                      documentTypeSlowSecondInstanceCivil.Contains(x.DocumentTypeId) == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "С определение, разпореждане и допълнителни решения II инстанция", dataRowsTrade.Where(x => x.ActEnforcedNoFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance).Count());

                excelService.AddRow();
                excelService.AddRow();
            }

            if (model.CaseGroupId <= 0 || model.CaseGroupId == NomenclatureConstants.CaseGroups.Administrative)
            {
                var dataRowsAdministrative = dataRows.Where(x => x.CaseGroupId == NomenclatureConstants.CaseGroups.Administrative).ToList();
                excelService.AddRange("АДМИНИСТРАТИВНИ ДЕЛА", 4); excelService.AddRow();
                excelService.AddRange("Рекапитулация за " + model.DateFrom.ToString("dd.MM.yyyy") + " - " + model.DateTo.ToString("dd.MM.yyyy"), 4); excelService.AddRow();
                CaseSessionPrivateAddTextCountToExcel(excelService, "Всички заседания", dataRowsAdministrative.Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "Свършени I инстанция", dataRowsAdministrative.Where(x => x.ActEnforcedFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "решени", dataRowsAdministrative
                                          .Where(x => x.ActEnforcedFinal == true &&
                                                 resultGrajdansko.Where(a => (x.CaseSessionResult.Split(",").ToList()).Contains(a.ToString())).Any() == false &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, space + "прекратени", dataRowsAdministrative
                                          .Where(x => x.ActEnforcedFinal == true &&
                                                 resultGrajdansko.Where(a => (x.CaseSessionResult.Split(",").ToList()).Contains(a.ToString())).Any() == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
                CaseSessionPrivateAddTextCountToExcel(excelService, "С определение, разпореждане и допълнителни решения I инстанция", dataRowsAdministrative.Where(x => x.ActEnforcedNoFinal == true &&
                                                      x.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance).Count());
            }

            return excelService.ToArray();
        }

        private HtmlTemplate GetHtmlTemplate(string alias)
        {
            return repo.AllReadonly<HtmlTemplate>()
                        .Where(x => x.Alias.ToUpper() == alias.ToUpper())
                        .FirstOrDefault();
        }

        private NPoiExcelService GetExcelHtmlTemplate(string alias, int caseGroupId = 0)
        {
            var htmlTemplate = GetHtmlTemplate(alias);
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);

            if (caseGroupId > 0)
            {
                excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 1;
                excelService.SetCellData(CaseGroupCaption_Title(caseGroupId));
            }

            if ((htmlTemplate.XlsDataRow ?? 0) > 0)
            {
                excelService.colIndex = 0;
                excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;
            }
            return excelService;
        }

        private string CaseGroupCaption_Title(int groupId)
        {
            string result = "";

            switch (groupId)
            {
                case NomenclatureConstants.CaseGroups.Company:
                    result = "/фирмени дела/";
                    break;
                case NomenclatureConstants.CaseGroups.GrajdanskoDelo:
                    result = "/граждански дела/";
                    break;
                case NomenclatureConstants.CaseGroups.NakazatelnoDelo:
                    result = "/наказателни дела/";
                    break;
                case NomenclatureConstants.CaseGroups.Trade:
                    result = "/търговски дела/";
                    break;
                case NomenclatureConstants.CaseGroups.Administrative:
                    result = "/административни дела/";
                    break;
                default:
                    break;
            }

            return result;
        }

        private IQueryable<CaseLinkReportVM> CaseLinkMigration_Select(int courtId, CaseLinkFilterReportVM model, string newLine)
        {
            DateTime dateFromSearch = model.DateFromCase ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateToCase ?? DateTime.Now.AddYears(100);

            Expression<Func<CaseMigration, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseMigration, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            Expression<Func<CaseMigration, bool>> caseNumberWhere = x => true;
            if (model.FromCaseNumber != null || model.ToCaseNumber != null)
            {
                caseNumberWhere = x => (x.Case.ShortNumberValue ?? 0) >= (model.FromCaseNumber ?? 0) &&
                         (x.Case.ShortNumberValue ?? 0) <= (model.ToCaseNumber ?? int.MaxValue);
            }

            Expression<Func<CaseMigration, bool>> dateSearch = x => true;
            if (model.DateFromCase != null || model.DateToCase != null)
                dateSearch = x => x.Case.RegDate.Date >= dateFromSearch.Date && x.Case.RegDate.Date <= dateToSearch.Date;

            Expression<Func<CaseMigration, bool>> migrationTypeSearch = x => true;
            if (model.CaseMigrationTypeId > 0)
                migrationTypeSearch = x => x.CaseMigrationTypeId == model.CaseMigrationTypeId;

            Expression<Func<CaseMigration, bool>> fromCourtSearch = x => true;
            if (model.FromCourtId > 0)
                fromCourtSearch = x => x.OutCaseMigration.Case.CourtId == model.FromCourtId;

            Expression<Func<CaseMigration, bool>> linkNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.CaseLinkNumber) == false)
                linkNumberSearch = x => x.OutCaseMigration.Case.RegNumber == model.CaseLinkNumber;

            Expression<Func<CaseMigration, bool>> linkYearSearch = x => true;
            if ((model.CaseLinkYear ?? 0) > 0)
                linkYearSearch = x => x.OutCaseMigration.Case.RegDate.Year == model.CaseLinkYear;

            Expression<Func<CaseMigration, bool>> documentGroupWhere = x => true;
            if (model.DocumentGroupId > 0)
                documentGroupWhere = x => x.Case.Document.DocumentGroupId == model.DocumentGroupId;

            Expression<Func<CaseMigration, bool>> documentTypeWhere = x => true;
            if (model.DocumentTypeId > 0)
                documentTypeWhere = x => x.Case.Document.DocumentTypeId == model.DocumentTypeId;

            DateTime dateActFromSearch = model.DateFromAct ?? DateTime.Now.AddYears(-100);
            DateTime dateActToSearch = model.DateToAct ?? DateTime.Now.AddYears(100);

            Expression<Func<CaseMigration, bool>> dateActSearch = x => true;
            if (model.DateFromAct != null || model.DateToAct != null)
                dateActSearch = x => x.Case.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null)
                                      .Where(a => a.ActInforcedDate != null)
                                      .Where(a => ((DateTime)a.ActDate).Date >= dateActFromSearch.Date && ((DateTime)a.ActDate).Date <= dateActToSearch.Date)
                                      .Any();

            return repo.AllReadonly<CaseMigration>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                .Where(x => x.OutCaseMigrationId != null)
                                .Where(x => x.DateExpired == null)
                                .Where(dateSearch)
                                .Where(caseNumberWhere)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Where(migrationTypeSearch)
                                .Where(fromCourtSearch)
                                .Where(linkNumberSearch)
                                .Where(linkYearSearch)
                                .Where(documentGroupWhere)
                                .Where(documentTypeWhere)
                                .Where(dateActSearch)
                                .Select(x => new CaseLinkReportVM
                                {
                                    CaseId = x.Case.Id,
                                    CaseData = x.Case.CaseGroup.Code + " " + x.Case.RegNumber,
                                    OutDocument = x.OutCaseMigration.OutDocument.DocumentNumber + "/" +
                                                  x.OutCaseMigration.OutDocument.DocumentDate.ToString("dd.MM.yyyy"),
                                    ActData = string.Join(newLine, x.Case.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null &&
                                                  a.ActInforcedDate != null)
                                              .Select(a => a.ActType.Label)),
                                    SessionResult = string.Join(newLine, x.Case.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null &&
                                                  a.ActInforcedDate != null)
                                          .Select(a => a.CaseSession.CaseSessionResults
                                                     .Where(b => b.IsActive && b.IsMain && b.DateExpired == null)
                                                     .Select(b => b.SessionResult.Label)
                                                     .FirstOrDefault())
                                                     ),
                                    ActDate = string.Join(newLine, x.Case.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null &&
                                                  a.ActInforcedDate != null)
                                              .Select(a => ((DateTime)a.ActDate).ToString("dd.MM.yyyy"))),
                                    PersonName = x.OutCaseMigration.Case.Court.Label,
                                    CaseInstitutionType = x.CaseMigrationType.Description ?? x.CaseMigrationType.Label,
                                    CaseLinkType = x.OutCaseMigration.Case.CaseGroup.Label,
                                    CaseLinkNumber = x.OutCaseMigration.Case.RegNumber,
                                }).AsQueryable();
        }

        private IQueryable<CaseLinkReportVM> CaseLinkInstitution_Select(int courtId, CaseLinkFilterReportVM model, string newLine)
        {
            DateTime dateFromSearch = model.DateFromCase ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateToCase ?? DateTime.Now.AddYears(100);

            Expression<Func<DocumentInstitutionCaseInfo, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.Document.Cases.FirstOrDefault().CaseGroupId == model.CaseGroupId;

            Expression<Func<DocumentInstitutionCaseInfo, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.Document.Cases.FirstOrDefault().CaseTypeId == model.CaseTypeId;

            Expression<Func<DocumentInstitutionCaseInfo, bool>> caseNumberWhere = x => true;
            if (model.FromCaseNumber != null || model.ToCaseNumber != null)
            {
                caseNumberWhere = x => (x.Document.Cases.FirstOrDefault().ShortNumberValue ?? 0) >= (model.FromCaseNumber ?? 0) &&
                         (x.Document.Cases.FirstOrDefault().ShortNumberValue ?? 0) <= (model.ToCaseNumber ?? int.MaxValue);
            }

            Expression<Func<DocumentInstitutionCaseInfo, bool>> dateSearch = x => true;
            if (model.DateFromCase != null || model.DateToCase != null)
                dateSearch = x => x.Document.Cases.FirstOrDefault().RegDate.Date >= dateFromSearch.Date &&
                             x.Document.Cases.FirstOrDefault().RegDate.Date <= dateToSearch.Date;

            Expression<Func<DocumentInstitutionCaseInfo, bool>> caseInstitutionTypeSearch = x => true;
            if (model.InstitutionCaseTypeId > 0)
                caseInstitutionTypeSearch = x => x.InstitutionCaseTypeId == model.InstitutionCaseTypeId;

            Expression<Func<DocumentInstitutionCaseInfo, bool>> fromInstitutionTypeSearch = x => true;
            if (model.InstitutionTypeId > 0)
                fromInstitutionTypeSearch = x => x.Institution.InstitutionTypeId == model.InstitutionTypeId;

            Expression<Func<DocumentInstitutionCaseInfo, bool>> fromInstitutionSearch = x => true;
            if (model.InstitutionId > 0)
                fromInstitutionSearch = x => x.InstitutionId == model.InstitutionId;

            Expression<Func<DocumentInstitutionCaseInfo, bool>> linkNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.CaseLinkNumber) == false)
                linkNumberSearch = x => x.CaseNumber == model.CaseLinkNumber;

            Expression<Func<DocumentInstitutionCaseInfo, bool>> linkYearSearch = x => true;
            if ((model.CaseLinkYear ?? 0) > 0)
                linkYearSearch = x => x.CaseYear == model.CaseLinkYear;

            Expression<Func<DocumentInstitutionCaseInfo, bool>> documentGroupWhere = x => true;
            if (model.DocumentGroupId > 0)
                documentGroupWhere = x => x.Document.DocumentGroupId == model.DocumentGroupId;

            Expression<Func<DocumentInstitutionCaseInfo, bool>> documentTypeWhere = x => true;
            if (model.DocumentTypeId > 0)
                documentTypeWhere = x => x.Document.DocumentTypeId == model.DocumentTypeId;

            DateTime dateActFromSearch = model.DateFromAct ?? DateTime.Now.AddYears(-100);
            DateTime dateActToSearch = model.DateToAct ?? DateTime.Now.AddYears(100);

            Expression<Func<DocumentInstitutionCaseInfo, bool>> dateActSearch = x => true;
            if (model.DateFromAct != null || model.DateToAct != null)
                dateActSearch = x => x.Document.Cases.FirstOrDefault().CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null)
                                      .Where(a => a.ActInforcedDate != null)
                                      .Where(a => ((DateTime)a.ActDate).Date >= dateActFromSearch.Date && ((DateTime)a.ActDate).Date <= dateActToSearch.Date)
                                      .Any();

            return repo.AllReadonly<DocumentInstitutionCaseInfo>()
                                .Where(x => x.Document.CourtId == courtId)
                                .Where(x => x.Document.Cases.Any())
                                .Where(x => x.Document.DateExpired == null)
                                .Where(dateSearch)
                                .Where(caseNumberWhere)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Where(caseInstitutionTypeSearch)
                                .Where(fromInstitutionTypeSearch)
                                .Where(fromInstitutionSearch)
                                .Where(linkNumberSearch)
                                .Where(linkYearSearch)
                                .Where(documentGroupWhere)
                                .Where(documentTypeWhere)
                                .Where(dateActSearch)
                                .Select(x => new CaseLinkReportVM
                                {
                                    CaseId = x.Document.Cases.FirstOrDefault().Id,
                                    CaseData = x.Document.Cases.FirstOrDefault().CaseGroup.Code + " " + x.Document.Cases.FirstOrDefault().RegNumber,
                                    ActData = string.Join(newLine, x.Document.Cases.FirstOrDefault().CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null &&
                                                  a.ActInforcedDate != null)
                                              .Select(a => a.ActType.Label)),
                                    SessionResult = string.Join(newLine, x.Document.Cases.FirstOrDefault().CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null &&
                                                  a.ActInforcedDate != null)
                                          .Select(a => a.CaseSession.CaseSessionResults
                                                     .Where(b => b.IsActive && b.IsMain && b.DateExpired == null)
                                                     .Select(b => b.SessionResult.Label)
                                                     .FirstOrDefault())
                                                     ),
                                    ActDate = string.Join(newLine, x.Document.Cases.FirstOrDefault().CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null &&
                                                  a.ActInforcedDate != null)
                                              .Select(a => ((DateTime)a.ActDate).ToString("dd.MM.yyyy"))),
                                    PersonName = x.Institution.FullName,
                                    CaseInstitutionType = x.InstitutionCaseType.Label,
                                    CaseLinkNumber = x.CaseNumber + "/" + x.CaseYear,
                                }).AsQueryable();
        }

        private IQueryable<CaseLinkReportVM> CaseLinkInstitutionNew_Select(int courtId, CaseLinkFilterReportVM model, string newLine)
        {
            DateTime dateFromSearch = model.DateFromCase ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateToCase ?? DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> caseNumberWhere = x => true;
            if (model.FromCaseNumber != null || model.ToCaseNumber != null)
            {
                caseNumberWhere = x => (x.ShortNumberValue ?? 0) >= (model.FromCaseNumber ?? 0) &&
                         (x.ShortNumberValue ?? 0) <= (model.ToCaseNumber ?? int.MaxValue);
            }

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFromCase != null || model.DateToCase != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date &&
                             x.RegDate.Date <= dateToSearch.Date;

            Expression<Func<Case, bool>> caseInstitutionTypeSearch = x => true;
            if (model.InstitutionCaseTypeId > 0)
                caseInstitutionTypeSearch = x => x.Document.DocumentInstitutionCaseInfo
                               .Where(a => a.InstitutionCaseTypeId == model.InstitutionCaseTypeId).Any();


            Expression<Func<Case, bool>> fromInstitutionTypeSearch = x => true;
            if (model.InstitutionTypeId > 0)
                fromInstitutionTypeSearch = x => x.Document.DocumentInstitutionCaseInfo
                               .Where(a => a.Institution.InstitutionTypeId == model.InstitutionTypeId).Any();


            Expression<Func<Case, bool>> fromInstitutionSearch = x => true;
            if (model.InstitutionId > 0)
                fromInstitutionSearch = x => x.Document.DocumentInstitutionCaseInfo
                               .Where(a => a.InstitutionId == model.InstitutionId).Any();


            Expression<Func<Case, bool>> linkNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.CaseLinkNumber) == false)
                linkNumberSearch = x => x.Document.DocumentInstitutionCaseInfo
                               .Where(a => a.CaseNumber == model.CaseLinkNumber).Any();

            Expression<Func<DocumentInstitutionCaseInfo, bool>> linkNumberSearchInst = x => true;
            if (string.IsNullOrEmpty(model.CaseLinkNumber) == false)
                linkNumberSearchInst = x => x.CaseNumber == model.CaseLinkNumber;


            Expression<Func<Case, bool>> linkYearSearch = x => true;
            if ((model.CaseLinkYear ?? 0) > 0)
                linkYearSearch = x => x.Document.DocumentInstitutionCaseInfo
                               .Where(a => a.CaseYear == model.CaseLinkYear).Any();

            Expression<Func<DocumentInstitutionCaseInfo, bool>> linkYearSearchInst = x => true;
            if ((model.CaseLinkYear ?? 0) > 0)
                linkYearSearchInst = x => x.CaseYear == model.CaseLinkYear;

            Expression<Func<Case, bool>> documentGroupWhere = x => true;
            if (model.DocumentGroupId > 0)
                documentGroupWhere = x => x.Document.DocumentGroupId == model.DocumentGroupId;

            Expression<Func<Case, bool>> documentTypeWhere = x => true;
            if (model.DocumentTypeId > 0)
                documentTypeWhere = x => x.Document.DocumentTypeId == model.DocumentTypeId;

            DateTime dateActFromSearch = model.DateFromAct ?? DateTime.Now.AddYears(-100);
            DateTime dateActToSearch = model.DateToAct ?? DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> dateActSearch = x => true;
            if (model.DateFromAct != null || model.DateToAct != null)
                dateActSearch = x => x.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null)
                                      .Where(a => a.ActInforcedDate != null)
                                      .Where(a => ((DateTime)a.ActDate).Date >= dateActFromSearch.Date && ((DateTime)a.ActDate).Date <= dateActToSearch.Date)
                                      .Any();

            return repo.AllReadonly<Case>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.Document.DocumentInstitutionCaseInfo.Any())
                                .Where(dateSearch)
                                .Where(caseNumberWhere)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Where(caseInstitutionTypeSearch)
                                .Where(fromInstitutionTypeSearch)
                                .Where(fromInstitutionSearch)
                                .Where(linkNumberSearch)
                                .Where(linkYearSearch)
                                .Where(documentGroupWhere)
                                .Where(documentTypeWhere)
                                .Where(dateActSearch)
                                .SelectMany(x => x.Document.DocumentInstitutionCaseInfo, (objCase, objInst) => new { objCase, objInst })
                                .Where(x => model.InstitutionCaseTypeId > 0 ? x.objInst.InstitutionCaseTypeId == model.InstitutionCaseTypeId : true)
                                .Where(x => model.InstitutionTypeId > 0 ? x.objInst.Institution.InstitutionTypeId == model.InstitutionTypeId : true)
                                .Where(x => model.InstitutionId > 0 ? x.objInst.InstitutionId == model.InstitutionId : true)
                                .Where(x => string.IsNullOrEmpty(model.CaseLinkNumber) == false ? x.objInst.CaseNumber == model.CaseLinkNumber : true)
                                .Where(x => (model.CaseLinkYear ?? 0) > 0 ? x.objInst.CaseYear == model.CaseLinkYear : true)
                                .Select(x => new CaseLinkReportVM
                                {
                                    CaseId = x.objCase.Id,
                                    CaseData = x.objCase.CaseGroup.Code + " " + x.objCase.RegNumber,
                                    ActData = string.Join(newLine, x.objCase.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null &&
                                                  a.ActInforcedDate != null)
                                              .Select(a => a.ActType.Label)),
                                    SessionResult = string.Join(newLine, x.objCase.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null &&
                                                  a.ActInforcedDate != null)
                                          .Select(a => a.CaseSession.CaseSessionResults
                                                     .Where(b => b.IsActive && b.IsMain && b.DateExpired == null)
                                                     .Select(b => b.SessionResult.Label)
                                                     .FirstOrDefault())
                                                     ),
                                    ActDate = string.Join(newLine, x.objCase.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null &&
                                                  a.ActInforcedDate != null)
                                              .Select(a => ((DateTime)a.ActDate).ToString("dd.MM.yyyy"))),
                                    PersonName = x.objInst.Institution.FullName,
                                    CaseInstitutionType = x.objInst.InstitutionCaseType.Label,
                                    CaseLinkNumber = x.objInst.CaseNumber + "/" + x.objInst.CaseYear,
                                }).AsQueryable();
        }
        public IQueryable<CaseLinkReportVM> CaseLinkReport_Select(int courtId, CaseLinkFilterReportVM model, string newLine)
        {
            IQueryable<CaseLinkReportVM> result = null;
            if ((model.InstitutionId <= 0 && model.InstitutionCaseTypeId <= 0 && model.InstitutionTypeId <= 0) || model.FromCourtId > 0 || model.CaseMigrationTypeId > 0)
            {
                result = CaseLinkMigration_Select(courtId, model, newLine);
            }

            if ((model.FromCourtId <= 0 && model.CaseMigrationTypeId <= 0) || model.InstitutionTypeId > 0 || model.InstitutionId > 0 || model.InstitutionCaseTypeId > 0)
            {
                var institutions = CaseLinkInstitutionNew_Select(courtId, model, newLine);
                if (result != null)
                    result = result.Union(institutions);
                else
                    result = institutions;
            }

            if (result == null)
                result = Enumerable.Empty<CaseLinkReportVM>().AsQueryable();

            return result;
        }

        public byte[] CaseLinkReportExportExcel(CaseLinkFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CaseLinkReport_Select(userContext.CourtId, model, Environment.NewLine).OrderBy(x => x.CaseId).ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка за дела на други институции/инстанции", 9,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseLinkReportVM, object>>>()
                {
                    x => x.CaseData,
                    x => x.OutDocument,
                    x => x.ActData,
                    x => x.SessionResult,
                    x => x.ActDate,
                    x => x.PersonName,
                    x => x.CaseInstitutionType,
                    x => x.CaseLinkType,
                    x => x.CaseLinkNumber,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        public List<TableDescription> TableDescription_Select()
        {
            return repo.AllReadonly<TableDescription>()
                            .OrderBy(x => x.TableName)
                            .ThenBy(x => x.OrdinalPosition)
                            .ToList();
        }

        /// <summary>
        /// Справка влезли в сила присъди
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        public IQueryable<SentenceListReportVM> SentenceListReport_Select(int courtId, SentenceListFilterReportVM model, string newLine)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);

            Expression<Func<CasePersonSentence, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => ((DateTime)x.InforcedDate).Date >= dateFromSearch.Date &&
                             ((DateTime)x.InforcedDate).Date <= dateToSearch.Date;

            Expression<Func<CasePersonSentence, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CasePersonSentence, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            Expression<Func<CasePersonSentence, bool>> caseCodeWhere = x => true;
            if (model.CaseCodeId > 0)
                caseCodeWhere = x => x.Case.CaseCodeId == model.CaseCodeId;

            Expression<Func<CasePersonSentence, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<CasePersonSentence, bool>> sessionResultWhere = x => true;
            if (model.SessionResultId > 0)
                sessionResultWhere = x => x.Case.CaseSessionResults.Where(a => a.DateExpired == null &&
                                          a.IsActive && a.IsMain &&
                                          a.SessionResultId == model.SessionResultId &&
                                          a.CaseSession.CaseSessionActs.Where(b => b.DateExpired == null &&
                                                          b.IsFinalDoc
                                                          ).Any()
                                          ).Any();

            Expression<Func<CasePersonSentence, bool>> sentenceResultWhere = x => true;
            if (model.SentenceResultTypeId > 0)
                sentenceResultWhere = x => x.SentenceResultTypeId == model.SentenceResultTypeId;

            return repo.AllReadonly<CasePersonSentence>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.InforcedDate != null)
                                .Where(x => (x.IsActive ?? true) == true)
                                .Where(dateSearch)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Where(caseCodeWhere)
                                .Where(sessionResultWhere)
                                .Where(sentenceResultWhere)
                                .Where(judgeReporterSearch)
                                .Select(x => new SentenceListReportVM
                                {
                                    CaseTypeName = x.Case.CaseType.Label,
                                    CaseRegNumber = x.Case.RegNumber,
                                    InforcedDate = (DateTime)x.InforcedDate,
                                    JudgeReporterName = x.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                            .Select(a => a.LawUnit.FullName)
                                                            .FirstOrDefault(),
                                    CaseCodeName = (x.Case.CaseCode.Code ?? "") + " " + x.Case.CaseCode.Label,
                                    SessionResultName = string.Join(newLine,
                                                      x.Case.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                        a.IsActive && a.IsMain &&
                                                        a.CaseSession.CaseSessionActs.Where(b => b.DateExpired == null &&
                                                          b.IsFinalDoc
                                                                                      ).Any()
                                          ).Select(a => a.SessionResult.Label)),
                                    SentenceResultTypeName = x.SentenceResultType.Label
                                }).AsQueryable();
        }

        /// <summary>
        /// Справка влезли в сила присъди Excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public byte[] SentenceListReportExportExcel(SentenceListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = SentenceListReport_Select(userContext.CourtId, model, Environment.NewLine).OrderBy(x => x.InforcedDate).ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка влезли в сила присъди", 7,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<SentenceListReportVM, object>>>()
                {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.InforcedDate,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeName,
                    x => x.SessionResultName,
                    x => x.SentenceResultTypeName,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Актове за обезличаване
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<SessionActForDepersonalizeReportVM> SessionActForDepersonalizeReport_Select(int courtId, SessionActForDepersonalizeFilterReportVM model)
        {
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);

            Expression<Func<CaseSessionAct, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => ((DateTime)x.ActDeclaredDate).Date >= dateFromSearch.Date &&
                             ((DateTime)x.ActDeclaredDate).Date <= dateToSearch.Date;

            Expression<Func<CaseSessionAct, bool>> isFinalAct = x => true;
            if (!string.IsNullOrEmpty(model.IsFinalAct))
            {
                switch (model.IsFinalAct)
                {
                    case "Y":
                        isFinalAct = x => x.IsFinalDoc;
                        break;
                    case "N":
                        isFinalAct = x => !x.IsFinalDoc;
                        break;
                }
            }

            return repo.AllReadonly<CaseSessionAct>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.ActDeclaredDate != null)
                                .Where(x => x.DateExpired == null)
                                .Where(x => repo.AllReadonly<MongoFile>()
                                       .Where(a => a.SourceType == SourceTypeSelectVM.CaseSessionActDepersonalizedBlank &&
                                              a.SourceId == x.Id.ToString()).Any())
                                .Where(x => repo.AllReadonly<MongoFile>()
                                       .Where(a => a.SourceType == SourceTypeSelectVM.CaseSessionActDepersonalized &&
                                              a.SourceId == x.Id.ToString()).Any() == false)
                                .Where(dateSearch)
                                .Where(isFinalAct)
                                .Select(x => new SessionActForDepersonalizeReportVM
                                {
                                    CaseTypeName = x.Case.CaseType.Label,
                                    CaseRegNumber = x.Case.RegNumber,
                                    CaseId = x.CaseId ?? 0,
                                    SessionActId = x.Id,
                                    SessionActTypeName = x.ActType.Label,
                                    SessionActNumber = x.RegNumber,
                                    SessionActDate = (DateTime)x.ActDate,
                                    SessionTypeName = x.CaseSession.SessionType.Label,
                                }).AsQueryable();
        }

        /// <summary>
        /// Справка Съдени и осъдени лица
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        public IQueryable<CasePersonDefendantListReportVM> CasePersonDefendantListReport_Select(int courtId, CasePersonDefendantListFilterReportVM model, string newLine)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = NomenclatureExtensions.ForceStartDate(model.DateFrom ?? DateTime.Now.AddYears(-100));
            DateTime dateToSearch = NomenclatureExtensions.ForceEndDate(model.DateTo ?? DateTime.Now.AddYears(100));

            Expression<Func<CasePerson, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.Case.CaseSessionActs.Where(a => a.DateExpired == null &&
                              a.IsFinalDoc && a.ActDeclaredDate != null &&
                              a.ActDeclaredDate >= dateFromSearch &&
                             a.ActDeclaredDate <= dateToSearch).Any();

            Expression<Func<CasePerson, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CasePerson, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            Expression<Func<CasePerson, bool>> caseCodeWhere = x => true;
            if (model.CaseCodeId > 0)
                caseCodeWhere = x => x.Case.CaseCodeId == model.CaseCodeId;

            Expression<Func<CasePerson, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<CasePerson, bool>> sessionResultWhere = x => true;
            if (model.SessionResultId > 0)
                sessionResultWhere = x => x.Case.CaseSessionResults.Where(a => a.DateExpired == null &&
                                          a.IsActive && a.IsMain &&
                                          a.SessionResultId == model.SessionResultId &&
                                          a.CaseSession.CaseSessionActs.Where(b => b.DateExpired == null &&
                                                          b.IsFinalDoc
                                                          ).Any()
                                          ).Any();

            Expression<Func<CasePerson, bool>> maturityWhere = x => true;
            if (model.PersonMaturityId > 0)
                maturityWhere = x => x.PersonMaturityId == model.PersonMaturityId;

            Expression<Func<CasePerson, bool>> sentenceTypeWhere = x => true;
            if (model.SentenceTypeId > 0)
                sentenceTypeWhere = x => x.CasePersonSentences
                     .Where(a => a.CasePersonSentencePunishments.Where(b => b.SentenceTypeId == model.SentenceTypeId).Any())
                     .Any();

            Expression<Func<CasePerson, bool>> uicSearch = x => true;
            if (!string.IsNullOrEmpty(model.PersonUicSearch))
                uicSearch = x => x.Uic.ToLower() == model.PersonUicSearch.ToLower();

            Expression<Func<CasePerson, bool>> nameSearch = x => true;
            if (!string.IsNullOrEmpty(model.PersonNameSearch))
                nameSearch = x => x.FullName.ToLower().Contains(model.PersonNameSearch.ToLower());

            var roles = repo.AllReadonly<PersonRoleGrouping>()
                           .Where(x => x.PersonRoleGroup == NomenclatureConstants.PersonRoleGroupings.RoleCasePersonDedendantList)
                           .Select(x => x.PersonRoleId)
                           .ToArray();

            return repo.AllReadonly<CasePerson>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.CaseSessionId == null)
                                .Where(x => x.DateExpired == null)
                                .Where(x => roles.Contains(x.PersonRoleId))
                                .Where(dateSearch)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Where(caseCodeWhere)
                                .Where(sessionResultWhere)
                                .Where(sentenceTypeWhere)
                                .Where(judgeReporterSearch)
                                .Where(maturityWhere)
                                .Where(uicSearch)
                                .Where(nameSearch)
                                .Select(x => new CasePersonDefendantListReportVM
                                {
                                    CaseTypeName = x.Case.CaseType.Label,
                                    CaseRegNumber = x.Case.RegNumber,
                                    JudgeReporterName = x.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                            .Select(a => a.LawUnit.FullName)
                                                            .FirstOrDefault(),
                                    CaseCodeName = (x.Case.CaseCode.Code ?? "") + " " + x.Case.CaseCode.Label,
                                    SessionResultName = string.Join(newLine,
                                                      x.Case.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                        a.IsActive && a.IsMain &&
                                                        a.CaseSession.CaseSessionActs.Where(b => b.DateExpired == null &&
                                                          b.IsFinalDoc
                                                                                      ).Any()
                                          ).Select(a => a.SessionResult.Label)),
                                    CaseEndDate = x.Case.CaseSessionActs
                                                  .Where(a => a.DateExpired == null && a.IsFinalDoc && a.ActDeclaredDate != null)
                                                  .OrderByDescending(a => a.ActDeclaredDate)
                                                  .Select(a => a.ActDeclaredDate)
                                                  .FirstOrDefault(),
                                    CasePersonName = x.FullName + "(" + x.PersonRole.Label + ")" + " " + (x.Uic ?? ""),
                                    PersonMaturityName = x.PersonMaturity.Label,
                                    SentenceTypeName = x.CasePersonSentences.Where(a => (a.IsActive ?? true) && a.DateExpired == null)
                                                       .Select(a => string.Join(newLine, a.CasePersonSentencePunishments
                                                                   .Where(b => b.DateExpired == null)
                                                                   .Select(b => b.SentenceType.Label)))
                                                       .FirstOrDefault(),
                                    SentenceTime = x.CasePersonSentences.Where(a => (a.IsActive ?? true) && a.DateExpired == null)
                                                       .Select(a => string.Join(newLine, a.CasePersonSentencePunishments
                                                                   .Where(b => b.DateExpired == null)
                                                                   .Select(b => b.SentenseDays + " дни " + b.SentenseWeeks + " седмици " +
                                                                                b.SentenseMonths + " месеци " + b.SentenseYears + " години")))
                                                       .FirstOrDefault()
                                }).AsQueryable();
        }

        /// <summary>
        /// Справка Съдени и осъдени лица
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public byte[] CasePersonDefendantListReportExportExcel(CasePersonDefendantListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CasePersonDefendantListReport_Select(userContext.CourtId, model, Environment.NewLine).ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка съдени и осъдени лица", 10,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CasePersonDefendantListReportVM, object>>>()
                {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeName,
                    x => x.SessionResultName,
                    x => x.CaseEndDate,
                    x => x.CasePersonName,
                    x => x.PersonMaturityName,
                    x => x.SentenceTypeName,
                    x => x.SentenceTime,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Справка Постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseFirstInstanceListReportVM> CaseFirstInstanceListReport_Select(int courtId, CaseFirstInstanceListFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeWhere = x => true;
            if (model.CaseCodeId > 0)
                caseCodeWhere = x => x.CaseCodeId == model.CaseCodeId;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<Case, bool>> createFromWhere = x => true;
            if (model.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.New)
            {
                createFromWhere = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                    a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .Any() == false;
            }
            else if (model.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.Jurisdiction)
            {
                createFromWhere = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                    a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .OrderByDescending(a => a.Id)
                                        .Select(a => a.CaseMigrationTypeId)
                                        .FirstOrDefault() == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction;
            }
            else if (model.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.NewNumber)
            {
                createFromWhere = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                    a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .OrderByDescending(a => a.Id)
                                        .Select(a => a.OutCaseMigration.ReturnCaseId)
                                        .FirstOrDefault() != x.Id;
            }
            else if (model.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.OldNumber)
            {
                createFromWhere = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                    a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .OrderByDescending(a => a.Id)
                                        .Select(a => a.OutCaseMigration.ReturnCaseId)
                                        .FirstOrDefault() == x.Id;
            }

            return repo.AllReadonly<Case>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance)
                                .Where(dateSearch)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Where(caseCodeWhere)
                                .Where(judgeReporterSearch)
                                .Where(createFromWhere)
                                .Select(x => new CaseFirstInstanceListReportVM
                                {
                                    CaseId = x.Id,
                                    CaseTypeName = x.CaseType.Label,
                                    CaseRegNumber = x.RegNumber,
                                    CaseRegDate = x.RegDate,
                                    JudgeReporterName = x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                            .Select(a => a.LawUnit.FullName)
                                                            .FirstOrDefault(),
                                    CaseCodeName = (x.CaseCode.Code ?? "") + " " + x.CaseCode.Label,
                                    migration = repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                        a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .OrderByDescending(a => a.Id)
                                        .Select(a => new CaseMigrationDataReportVM
                                        {
                                            MigrationId = a.Id,
                                            CaseMigrationTypeId = a.CaseMigrationTypeId,
                                            ReturnCaseId = a.OutCaseMigration.ReturnCaseId ?? 0
                                        })
                                        .FirstOrDefault()
                                }).AsQueryable();
        }

        /// <summary>
        /// Експорт Справка Постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public byte[] CaseFirstInstanceListReportExportExcel(CaseFirstInstanceListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CaseFirstInstanceListReport_Select(userContext.CourtId, model).ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка постъпили дела за период – първоинстанционни дела", 6,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseFirstInstanceListReportVM, object>>>()
                {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.CaseRegDate,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeName,
                    x => x.CaseCreateFromName,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSecondInstanceListReportVM> CaseSecondInstanceListReport_Select(int courtId, CaseSecondInstanceListFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeWhere = x => true;
            if (model.CaseCodeId > 0)
                caseCodeWhere = x => x.CaseCodeId == model.CaseCodeId;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<Case, bool>> fromCourtSearch = x => true;
            if (model.FromCourtId > 0)
                fromCourtSearch = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null)
                                        .Select(a => a.InitialCase.CourtId)
                                        .FirstOrDefault() == model.FromCourtId ||
                                        x.Document.DocumentCaseInfo.Where(a => a.CaseId == null)
                                       .Select(a => a.CourtId)
                                       .FirstOrDefault() == model.FromCourtId;

            Expression<Func<Case, bool>> createFromWhere = x => true;
            if (model.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.New)
            {
                createFromWhere = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                    a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .Any() == false;
            }
            else if (model.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.Jurisdiction)
            {
                createFromWhere = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                    a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .OrderByDescending(a => a.Id)
                                        .Select(a => a.CaseMigrationTypeId)
                                        .FirstOrDefault() == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction;
            }
            else if (model.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.Prosecutors)
            {
                createFromWhere = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                    a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .OrderByDescending(a => a.Id)
                                        .Select(a => a.CaseMigrationTypeId)
                                        .FirstOrDefault() == NomenclatureConstants.CaseMigrationTypes.AcceptProsecutors;
            }
            else if (model.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.NewNumber)
            {
                createFromWhere = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                    a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .OrderByDescending(a => a.Id)
                                        .Select(a => a.OutCaseMigration.ReturnCaseId)
                                        .FirstOrDefault() != x.Id;
            }
            else if (model.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.OldNumber)
            {
                createFromWhere = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                    a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .OrderByDescending(a => a.Id)
                                        .Select(a => a.OutCaseMigration.ReturnCaseId)
                                        .FirstOrDefault() == x.Id;
            }

            return repo.AllReadonly<Case>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance)
                                .Where(dateSearch)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Where(caseCodeWhere)
                                .Where(judgeReporterSearch)
                                .Where(createFromWhere)
                                .Where(fromCourtSearch)
                                .Select(x => new CaseSecondInstanceListReportVM
                                {
                                    CaseId = x.Id,
                                    CaseTypeName = x.CaseType.Label,
                                    CaseRegNumber = x.RegNumber,
                                    CaseRegDate = x.RegDate,
                                    JudgeReporterName = x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                            .Select(a => a.LawUnit.FullName)
                                                            .FirstOrDefault(),
                                    CaseCodeName = (x.CaseCode.Code ?? "") + " " + x.CaseCode.Label,
                                    migration = repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null &&
                                        a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                        .OrderByDescending(a => a.Id)
                                        .Select(a => new CaseMigrationDataReportVM
                                        {
                                            MigrationId = a.Id,
                                            CaseMigrationTypeId = a.CaseMigrationTypeId,
                                            ReturnCaseId = a.OutCaseMigration.ReturnCaseId ?? 0
                                        })
                                        .FirstOrDefault(),
                                    OldLinkNumber = x.Document.DocumentCaseInfo.Where(a => a.CaseId == null)
                                       .Select(a => a.Court.Label)
                                       .FirstOrDefault(),
                                    NewLinkNumber = repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null)
                                        .Select(a => a.InitialCase.Court.Label)
                                        .FirstOrDefault(),
                                    DocumentTypeName = x.Document.DocumentType.Label
                                }).AsQueryable();
        }

        /// <summary>
        /// Експорт Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public byte[] CaseSecondInstanceListReportExportExcel(CaseSecondInstanceListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CaseSecondInstanceListReport_Select(userContext.CourtId, model).ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка постъпили дела за период – въззивни дела", 8,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseSecondInstanceListReportVM, object>>>()
                {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.CaseRegDate,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeName,
                    x => x.FromCourtName,
                    x => x.CaseCreateFromName,
                    x => x.DocumentTypeName,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Справка Свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        public IQueryable<CaseFinishListReportVM> CaseFinishFirstInstanceListReport_Select(int courtId, CaseFinishListFilterReportVM model, string newLine)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);

            int[] caseGroupsGR_TR = { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade };

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.CaseSessionActs.Where(a => a.DateExpired == null && a.IsFinalDoc && a.ActDeclaredDate != null &&
                           ((DateTime)a.ActDeclaredDate).Date >= dateFromSearch.Date && ((DateTime)a.ActDeclaredDate).Date <= dateToSearch.Date)
                .Any();

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeWhere = x => true;
            if (model.CaseCodeId > 0)
                caseCodeWhere = x => x.CaseCodeId == model.CaseCodeId;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();


            Expression<Func<Case, bool>> complainSearch = x => true;
            if (model.ActComplainResultId > 0 && caseGroupsGR_TR.Contains(model.CaseGroupId))
            {
                complainSearch = x => x.CaseSessionActs.Where(a => a.DateExpired == null && a.IsFinalDoc &&
                            a.ActComplainResultId == model.ActComplainResultId)
                            .Any();
            }

            Expression<Func<Case, bool>> sessionResultSearch = x => true;
            if (model.SessionResultId > 0 && model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                sessionResultSearch = x => x.CaseSessionActs.Where(a => a.DateExpired == null && a.IsFinalDoc &&
                            a.CaseSession.CaseSessionResults
                                       .Where(b => b.DateExpired == null && b.SessionResultId == model.SessionResultId).Any())
                            .Any();
            }

            var resultFinish = repo.AllReadonly<SessionResultGrouping>()
                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                .Select(x => x.SessionResultId)
                .ToList();


            return repo.AllReadonly<Case>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance)
                                .Where(x => x.CaseSessionActs
                                             .Where(a => a.DateExpired == null && a.IsFinalDoc && 
                                             a.ActDeclaredDate != null && a.CaseSession.DateExpired == null &&
                                             a.CaseSession.CaseSessionResults.Where(b => b.DateExpired == null && resultFinish.Contains(b.SessionResultId)).Any())
                                             .Any())
                                .Where(dateSearch)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Where(caseCodeWhere)
                                .Where(judgeReporterSearch)
                                .Where(complainSearch)
                                .Where(sessionResultSearch)
                                .Select(x => new CaseFinishListReportVM
                                {
                                    CaseTypeName = x.CaseType.Label,
                                    CaseRegNumber = x.RegNumber,
                                    JudgeReporterName = x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                            .Select(a => a.LawUnit.FullName)
                                                            .FirstOrDefault(),
                                    CaseCodeName = (x.CaseCode.Code ?? "") + " " + x.CaseCode.Label,
                                    ActComplainResultName = caseGroupsGR_TR.Contains(x.CaseGroupId) ?
                                                         string.Join(newLine, x.CaseSessionActs
                                                                              .Where(a => a.DateExpired == null && a.IsFinalDoc &&
                                                                    a.ActComplainResultId != null)
                                                             .Select(a => a.ActComplainResult.Label)) : "",
                                    SessionResultName = x.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo ?
                                                 string.Join(newLine, x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                    resultFinish.Contains(a.SessionResultId) && 
                                                                    a.CaseSession.CaseSessionActs.Where(b => b.DateExpired == null &&
                                                                          b.IsFinalDoc).Any())
                                                             .Select(a => a.SessionResult.Label)) : "",
                                    SessionResultStopBaseName =
                                                 string.Join(newLine, x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                    a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                                             .Select(a => a.SessionResult.Label)),

                                    CaseDateFinish = string.Join(newLine, x.CaseSessionActs.Where(a => a.DateExpired == null &&
                                                        a.IsFinalDoc && a.ActDeclaredDate != null && a.CaseSession.DateExpired == null &&
                                                        a.CaseSession.CaseSessionResults.Where(b => b.DateExpired == null && resultFinish.Contains(b.SessionResultId)).Any()
                                                        )
                                              .Select(a => ((DateTime)a.ActDeclaredDate).ToString(FormattingConstant.NormalDateFormat))),
                                    CaseLifecycleMonths = x.CaseLifecycles.Select(a => a.DurationMonths).Sum(),
                                }).AsQueryable();
        }

        /// <summary>
        /// Експорт Справка Свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public byte[] CaseFinishFirstInstanceListReportExportExcel(CaseFinishListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CaseFinishFirstInstanceListReport_Select(userContext.CourtId, model, Environment.NewLine).ToList();

            int[] caseGroupsGR_TR = { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade };

            int colCount = 9;
            if (model.CaseGroupId > 0)
            {
                if (caseGroupsGR_TR.Contains(model.CaseGroupId))
                    colCount = 8;
                else if (model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                    colCount = 8;
            }
            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка свършени дела за период – първоинстанционни дела", colCount,
                      styleTitle); excelService.AddRow();

            if (caseGroupsGR_TR.Contains(model.CaseGroupId))
            {
                excelService.AddList(
                    dataRows,
                    new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                    new List<Expression<Func<CaseFinishListReportVM, object>>>()
                    {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeName,
                    x => x.ActComplainResultName,
                    x => x.SessionResultStopBaseName,
                    x => x.CaseDateFinish,
                    x => x.CaseLifecycleMonths,
                    },
                    NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index
                );
            }
            else if (model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                excelService.AddList(
                    dataRows,
                    new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                    new List<Expression<Func<CaseFinishListReportVM, object>>>()
                    {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeName,
                    x => x.SessionResultName,
                    x => x.SessionResultStopBaseName,
                    x => x.CaseDateFinish,
                    x => x.CaseLifecycleMonths,
                    },
                    NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index
                );
            }
            else
            {
                excelService.AddList(
                    dataRows,
                    new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                    new List<Expression<Func<CaseFinishListReportVM, object>>>()
                    {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeName,
                    x => x.ActComplainResultName,
                    x => x.SessionResultName,
                    x => x.SessionResultStopBaseName,
                    x => x.CaseDateFinish,
                    x => x.CaseLifecycleMonths,
                    },
                    NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index
                );
            }


            return excelService.ToArray();
        }

        /// <summary>
        /// Справка Свършени дела за период – въззивни дела
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        public IQueryable<CaseFinishListReportVM> CaseFinishSecondInstanceListReport_Select(int courtId, CaseFinishListFilterReportVM model, string newLine)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.CaseSessionActs.Where(a => a.DateExpired == null && a.IsFinalDoc && a.ActDeclaredDate != null &&
                           ((DateTime)a.ActDeclaredDate).Date >= dateFromSearch.Date && ((DateTime)a.ActDeclaredDate).Date <= dateToSearch.Date)
                .Any();

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> fromCourtSearch = x => true;
            if (model.InitialCourtId > 0)
                fromCourtSearch = x => repo.AllReadonly<CaseMigration>()
                                        .Where(a => a.CaseId == x.Id && a.DateExpired == null)
                                        .Select(a => a.InitialCase.CourtId)
                                        .FirstOrDefault() == model.InitialCourtId;

            Expression<Func<Case, bool>> caseCodeWhere = x => true;
            if (model.CaseCodeId > 0)
                caseCodeWhere = x => x.CaseCodeId == model.CaseCodeId;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();


            Expression<Func<Case, bool>> sessionResultSearch = x => true;
            if (model.SessionResultId > 0)
            {
                sessionResultSearch = x => x.CaseSessionActs.Where(a => a.DateExpired == null && a.IsFinalDoc &&
                            a.CaseSession.CaseSessionResults
                                       .Where(b => b.DateExpired == null && b.SessionResultId == model.SessionResultId).Any())
                            .Any();
            }

            var resultFinish = repo.AllReadonly<SessionResultGrouping>()
                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                .Select(x => x.SessionResultId)
                .ToList();

            return repo.AllReadonly<Case>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance)
                                .Where(x => x.CaseSessionActs
                                             .Where(a => a.DateExpired == null && a.IsFinalDoc &&
                                             a.ActDeclaredDate != null && a.CaseSession.DateExpired == null &&
                                             a.CaseSession.CaseSessionResults.Where(b => b.DateExpired == null && resultFinish.Contains(b.SessionResultId)).Any())
                                             .Any())
                                .Where(dateSearch)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Where(caseCodeWhere)
                                .Where(judgeReporterSearch)
                                .Where(fromCourtSearch)
                                .Where(sessionResultSearch)
                                .Select(x => new CaseFinishListReportVM
                                {
                                    CaseTypeName = x.CaseType.Label,
                                    CaseRegNumber = x.RegNumber,
                                    JudgeReporterName = x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                            .Select(a => a.LawUnit.FullName)
                                                            .FirstOrDefault(),
                                    CaseCodeName = (x.CaseCode.Code ?? "") + " " + x.CaseCode.Label,
                                    InitialCourtName = repo.AllReadonly<CaseMigration>()
                                                            .Where(a => a.CaseId == x.Id && a.DateExpired == null)
                                                            .Select(a => a.InitialCase.Court.Label)
                                                            .FirstOrDefault(),
                                    ActComplainResultName = string.Join(newLine, x.CaseSessionActs
                                                                              .Where(a => a.DateExpired == null && a.IsFinalDoc &&
                                                                    a.ActComplainResultId != null)
                                                             .Select(a => a.ActComplainResult.Label)),
                                    SessionResultName = string.Join(newLine, x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                    resultFinish.Contains(a.SessionResultId) &&
                                                                   a.CaseSession.CaseSessionActs.Where(b => b.DateExpired == null &&
                                                                         b.IsFinalDoc).Any())
                                                             .Select(a => a.SessionResult.Label)),
                                    SessionResultStopBaseName =
                                                 string.Join(newLine, x.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                    a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                                             .Select(a => a.SessionResult.Label)),

                                    CaseDateFinish = string.Join(newLine, x.CaseSessionActs.Where(a => a.DateExpired == null &&
                                                        a.IsFinalDoc && a.ActDeclaredDate != null && a.CaseSession.DateExpired == null &&
                                                        a.CaseSession.CaseSessionResults.Where(b => b.DateExpired == null && resultFinish.Contains(b.SessionResultId)).Any()
                                                        )
                                              .Select(a => ((DateTime)a.ActDeclaredDate).ToString(FormattingConstant.NormalDateFormat))),
                                    CaseLifecycleMonths = x.CaseLifecycles.Select(a => a.DurationMonths).Sum(),
                                }).AsQueryable();
        }

        /// <summary>
        /// Справка Свършени дела за период – въззивни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public byte[] CaseFinishSecondInstanceListReportExportExcel(CaseFinishListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CaseFinishSecondInstanceListReport_Select(userContext.CourtId, model, Environment.NewLine).ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка свършени дела за период – въззивни дела", 9,
                      styleTitle); excelService.AddRow();


            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseFinishListReportVM, object>>>()
                {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.JudgeReporterName,
                    x => x.InitialCourtName,
                    x => x.CaseCodeName,
                    x => x.ActComplainResultName,
                    x => x.SessionResultName,
                    x => x.SessionResultStopBaseName,
                    x => x.CaseDateFinish,
                    x => x.CaseLifecycleMonths,
                },
                NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        public byte[] CourtStatsReport()
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");


            var dataRows = repo.ExecuteProc<ReportCourtStatsVM>("public.report_court_stats()").ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange($"Справка дейности по съд към {DateTime.Now:dd.MM.yyyy HH:mm:ss}", 6,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 15000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<ReportCourtStatsVM, object>>>()
                {
                    x => x.CourtName,
                    x => x.DocCount,
                    x => x.CaseCount,
                    x => x.CaseSessionCount,
                    x => x.ActCount,
                    x => x.ActSignedCount
                },
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }
    }
}
