using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Extensions.HTML;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using IOWebApplication.Infrastructure.Models.ViewModels.Report.ReportWorkJudicialMediationCenters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;


namespace IOWebApplication.Core.Services
{
    public class ReportService : BaseService, IReportService
    {
        private readonly IMediationCommonService mediationCommonService;

        public ReportService(ILogger<ReportService> _logger,
                             IRepository _repo,
                             IUserContext _userContext,
                             IReadonlyRepository _readonlyrepo,
                             IMediationCommonService _mediationCommonService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            readonlyrepo = _readonlyrepo;
            mediationCommonService = _mediationCommonService;
        }

        private IQueryable<DocumentOutReportVM> DocumentOutGoingReport_Select(int courtId, DocumentOutFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
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
                var types = model.CaseTypeIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                caseTypeWhere = x => x.DocumentCaseInfo.Where(a => types.Contains(a.Case.CaseTypeId)).Any();
            }

            Expression<Func<Document, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.DocumentCaseInfo.Where(b => b.Case.CaseLawUnits
                          .Where(a => (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.CaseSessionId == null &&
                          a.CourtDepartmentId == model.DepartmentId).Any())
                .Any();

            Expression<Func<Document, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.DocumentCaseInfo.Where(b => b.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any())
                      .Any();


            return readonlyrepo.AllReadonly<Document>()
                               .Include(x => x.DocumentPersons)
                               .Include(x => x.DocumentGroup)
                               .Include(x => x.DeliveryGroup)
                               .Where(x => x.CourtId == courtId && x.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                               .Where(dateSearch)
                               .Where(documentNumberWhere)
                               .Where(caseGroupWhere)
                               .Where(caseTypeWhere)
                               .Where(departmentWhere)
                               .Where(judgeReporterSearch)
                               .Select(x => new DocumentOutReportVM
                               {
                                   DocumentNumber = x.DocumentNumber,
                                   DocumentDateHour = x.DocumentDate,
                                   Description = (x.DateExpired != null ?
                                              ("Изтриване на документ - " + (x.DescriptionExpired ?? "") + Environment.NewLine) : "") +
                                              x.DocumentType.Label +
                                              (string.IsNullOrEmpty(x.Description) ? "" : Environment.NewLine + x.Description) +
                                          (x.DocumentCaseInfo.Select(a => Environment.NewLine + a.Case.CaseType.Code + "; № " + a.Case.RegNumber)
                                                                     .FirstOrDefault() ?? ""),
                                   DeliveryGroupName = x.DeliveryGroup.Label + (x.DeliveryTypeId != null ? (" - " + x.DeliveryType.Label) : ""),
                                   DocumentNumberValue = (x.DocumentNumberValue ?? 0),
                                   PersonNames = string.Join(Environment.NewLine, x.DocumentPersons.Select(p => p.FullName)),
                               }).AsQueryable();
        }

        private IQueryable<DocumentOutReportVM> DocumentOutGoingReportPrev_Select(int courtId, DocumentOutFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
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
                var types = model.CaseTypeIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                caseTypeWhere = x => x.DocumentCaseInfo.Where(a => types.Contains(a.Case.CaseTypeId)).Any();
            }

            Expression<Func<Document, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.DocumentCaseInfo.Where(b => b.Case.CaseLawUnits
                          .Where(a => (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.CaseSessionId == null &&
                          a.CourtDepartmentId == model.DepartmentId).Any())
                .Any();

            Expression<Func<Document, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.DocumentCaseInfo.Where(b => b.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any())
                      .Any();


            return readonlyrepo.AllReadonly<Document>()
                               .Include(x => x.DocumentPersons)
                               .Include(x => x.DocumentGroup)
                               .Include(x => x.DeliveryGroup)
                               .Where(x => x.CourtId == courtId && x.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                               .Where(dateSearch)
                               .Where(documentNumberWhere)
                               .Where(caseGroupWhere)
                               .Where(caseTypeWhere)
                               .Where(departmentWhere)
                               .Where(judgeReporterSearch)
                               .Select(x => new DocumentOutReportVM
                               {
                                   DocumentNumber = x.DocumentNumber,
                                   DocumentDate = x.DocumentDate,
                                   Description = (x.DateExpired != null ?
                                              ("Изтриване на документ - " + (x.DescriptionExpired ?? "") + Environment.NewLine) : "") +
                                              x.DocumentType.Label + Environment.NewLine +
                                                   string.Join(Environment.NewLine, x.DocumentPersons.Select(p => p.FullName)) +
                                                   (string.IsNullOrEmpty(x.Description) ? "" : Environment.NewLine + x.Description) +
                                          (x.DocumentCaseInfo.Select(a => Environment.NewLine + a.Case.CaseType.Code + "; № " + a.Case.RegNumber)
                                                             .FirstOrDefault() ?? ""),
                                   DeliveryGroupName = x.DeliveryGroup.Label + (x.DeliveryTypeId != null ? (" - " + x.DeliveryType.Label) : ""),
                                   DocumentNumberValue = (x.DocumentNumberValue ?? 0)
                               }).AsQueryable();
        }

        public async Task<byte[]> DocumentOutGoingReportToExcelOnePrev(DocumentOutFilterReportVM model)
        {
            var dataRows = await DocumentOutGoingReportPrev_Select(userContext.CourtId, model).OrderBy(x => x.DocumentNumberValue).ToListAsync();

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

        public async Task<byte[]> DocumentOutGoingReportToExcelOne(DocumentOutFilterReportVM model)
        {
            var dataRows = await DocumentOutGoingReport_Select(userContext.CourtId, model).OrderBy(x => x.DocumentNumberValue)
                                                                                          .ToListAsync();

            NPoiExcelService excelService = GetExcelHtmlTemplate("DocumentOutNew", model.CaseGroupId);
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<DocumentOutReportVM, object>>>()
                {
                    x => x.DocumentNumber,
                    x => x.DocumentDateHour,
                    x => x.PersonNames,
                    x => x.Description,
                    x => x.DeliveryGroupName,
                }
            );
            return excelService.ToArray();
        }

        private IQueryable<DocumentInReportVM> DocumentInComingReport_Select(int courtId, DocumentInFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
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
                var types = model.CaseTypeIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                caseTypeWhere = x => x.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument ?
                                       x.Cases.Where(a => types.Contains(a.CaseTypeId)).Any() :
                                       x.DocumentCaseInfo.Where(a => types.Contains(a.Case.CaseTypeId)).Any()
                                      ;
            }

            Expression<Func<Document, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument ?
                           x.Cases.Where(b => b.CaseLawUnits
                          .Where(a => (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.CaseSessionId == null &&
                          a.CourtDepartmentId == model.DepartmentId).Any())
                           .Any() :
                            x.DocumentCaseInfo.Where(b => b.Case.CaseLawUnits
                          .Where(a => (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.CaseSessionId == null &&
                          a.CourtDepartmentId == model.DepartmentId).Any())
                            .Any();

            Expression<Func<Document, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument ?
                     x.Cases.Where(b => b.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any())
                      .Any() :
            x.DocumentCaseInfo.Where(b => b.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any())
                      .Any();

            return readonlyrepo.AllReadonly<Document>()
                               .Where(x => x.CourtId == courtId && x.DocumentDirectionId == DocumentConstants.DocumentDirection.Incoming)
                               .Where(dateSearch)
                               .Where(documentWhere)
                               .Where(documentNumberWhere)
                               .Where(caseGroupWhere)
                               .Where(caseTypeWhere)
                               .Where(departmentWhere)
                               .Where(judgeReporterSearch)
                               .Select(x => new DocumentInReportVM
                               {
                                   DocumentNumber = x.DocumentNumber,
                                   DocumentDate = x.DocumentDate,
                                   Description = x.DocumentType.Label + (x.Description == null ? "" : " - " + x.Description) +
                                                 (x.Cases.Select(p => Environment.NewLine + p.CaseGroup.Label).FirstOrDefault() ?? "") +
                                                 (x.DocumentCaseInfo.Select(a => Environment.NewLine + (a.CaseId != null ? a.Case.RegNumber : a.CaseRegNumber))
                                                                    .FirstOrDefault() ?? ""),
                                   DocumentPersonName = string.Join(Environment.NewLine, x.DocumentPersons.Select(a => a.FullName + "(" + a.PersonRole.Label + ")")),
                                   TaskName = (x.DateExpired != null ?
                                              ("Изтриване на документ - " + (x.DescriptionExpired ?? "") + Environment.NewLine) : "") +
                                              (readonlyrepo.AllReadonly<WorkTask>()
                                                   .Where(t => t.SourceType == SourceTypeSelectVM.Document &&
                                                          t.TaskStateId != NomenclatureConstants.TaskStates.Cancel &&
                                                          t.SourceId == x.Id).OrderBy(t => t.Id).Select(t => t.TaskType.Label + " - " + t.User.LawUnit.FullName)
                                                   .FirstOrDefault() ?? ""),
                                   DocumentNumberValue = x.DocumentNumberValue ?? 0,
                                   DeliveryTypeGroup = x.DeliveryGroup.Label + (x.PostOfficeDate != null ? " Дата на пощенско клеймо " + x.PostOfficeDate.DateToStr(FormattingConstant.NormalDateFormat) : "") +
                                                       (x.DeliveryTypeId != null ? Environment.NewLine + x.DeliveryType.Label : ""),
                               }).AsQueryable();
        }

        public async Task<byte[]> DocumentInGoingReportToExcelOnePrev(DocumentInFilterReportVM model)
        {
            var dataRows = await DocumentInComingReport_Select(userContext.CourtId, model).OrderBy(x => x.DocumentNumberValue).ToListAsync();

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

        public async Task<byte[]> DocumentInGoingReportToExcelOne(DocumentInFilterReportVM model)
        {
            var dataRows = await DocumentInComingReport_Select(userContext.CourtId, model).OrderBy(x => x.DocumentNumberValue).ToListAsync();

            NPoiExcelService excelService = GetExcelHtmlTemplate("DocumentInNew");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<DocumentInReportVM, object>>>()
                {
                    x => x.DocumentNumber,
                    x => x.DocumentDate,
                    x => x.DocumentPersonName,
                    x => x.Description,
                    x => x.TaskName,
                    x => x.DeliveryTypeGroup,
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

        /// <summary>
        /// Метод извличащ данни за азбучник
        /// </summary>
        /// <param name="courtId">Съд</param>
        /// <param name="model">Филтър попълнен ит потребител</param>
        /// <returns></returns>
        private async Task<List<CaseAlphabeticalVM>> CaseAlphabetical_Select(int courtId, CaseAlphabeticalFilterVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateFrom = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateTo = model.DateTo ?? DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.CaseLawUnits
                          .Where(a => (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.CaseSessionId == null &&
                          a.CourtDepartmentId == model.DepartmentId).Any();

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<Case, bool>> numberSearch = x => true;
            if ((model.NumberFrom ?? 0) > 0)
                numberSearch = x => x.ShortNumberValue >= model.NumberFrom;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == model.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeWhere = x => true;
            if (model.CaseCodeId > 0)
                caseCodeWhere = x => x.CaseCodeId == model.CaseCodeId;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            var cases = await readonlyrepo.AllReadonly<Case>()
                                          .Include(x => x.Court)
                                          .Include(x => x.CaseGroup)
                                          .Include(x => x.CaseType)
                                          .Include(x => x.CasePersons)
                                          .ThenInclude(x => x.PersonRole)
                                          .Include(x => x.CasePersons)
                                          .Where(x => x.CourtId == courtId &&
                                                      ((x.RegDate.Date >= dateFrom.Date) && (x.RegDate.Date <= dateTo.Date)))
                                          .Where(departmentWhere)
                                          .Where(judgeReporterSearch)
                                          .Where(numberSearch)
                                          .Where(caseTypeWhere)
                                          .Where(caseCodeWhere)
                                          .Where(caseGroupWhere)
                                          .ToListAsync();

            List<CaseAlphabeticalVM> caseAlphabeticalVMs = new List<CaseAlphabeticalVM>();

            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            string[] alphabets = (model.Alphabet ?? string.Empty).Split(delimiterChars);

            List<int> personRoleNakazatelno = null;
            if (model.CaseGroupId == 0 || model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                personRoleNakazatelno = await readonlyrepo.AllReadonly<PersonRoleGrouping>()
                                                          .Where(x => x.PersonRoleGroup == NomenclatureConstants.PersonRoleGroupings.CaseAlphabeticalNakazatelnoDelo)
                                                          .Select(x => x.PersonRoleId)
                                                          .ToListAsync();
            }
            else
            {
                personRoleNakazatelno = new List<int>();
            }


            foreach (var caseCase in cases)
            {
                //Маха " ако е първи символ защото излизат на нов ред
                foreach (var item in caseCase.CasePersons)
                {
                    if (!string.IsNullOrEmpty(item.FullName))
                    {
                        item.FullName = item.FullName.Decode();
                        if (item.FullName.StartsWith("\""))
                            item.FullName = item.FullName.Replace("\"", "");
                    }
                }

                List<CasePerson> casePersons = null;
                if (caseCase.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                {
                    casePersons = caseCase.CasePersons
                                          .Where(x => x.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide ||
                                                      personRoleNakazatelno.Contains(x.PersonRoleId))
                                          .ToList();
                }
                else
                {
                    casePersons = caseCase.CasePersons
                                          .Where(x => x.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide ||
                                                      x.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.LeftSide)
                                          .ToList();
                }

                foreach (CasePerson casePerson in casePersons.Where(x => x.CaseSessionId == null &&
                                                                         x.DateExpired == null &&
                                                                         !string.IsNullOrEmpty(x.FullName)))
                {
                    if (!string.IsNullOrEmpty(model.Alphabet))
                    {
                        if (alphabets.Any(x => x.ToUpper() == casePerson.FullName[0].ToString().ToUpper()))
                            caseAlphabeticalVMs.Add(fillAlphabeticalVM(caseCase, casePerson));
                    }
                    else
                        caseAlphabeticalVMs.Add(fillAlphabeticalVM(caseCase, casePerson));
                }
            }

            return caseAlphabeticalVMs.OrderBy(x => x.CaseGroupId).ThenBy(x => x.Name).ToList();
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
                idenNumber = !string.IsNullOrEmpty(caseAlphabetical.IdentityNumber) ? caseAlphabetical.IdentityNumber : string.Empty;

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

        /// <summary>
        /// Метод извличащ ексел за азбучника
        /// </summary>
        /// <param name="courtId">Съд</param>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public async Task<byte[]> CaseAlphabetical_ToExcel(int courtId, CaseAlphabeticalFilterVM model)
        {
            NPoiExcelService excelService = GetExcelHtmlTemplate("CaseAlphabetical");
            var caseAlphabeticals = await CaseAlphabetical_Select(courtId, model);

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
                var min = caseAlphabeticals.Min(x => Int64.Parse(x.ShortNumber));
                var max = caseAlphabeticals.Max(x => Int64.Parse(x.ShortNumber));

                var minDeloO = caseAlphabeticals.Where(x => x.ShortNumber == min.ToString()).OrderBy(x => x.RegDate).FirstOrDefault();
                var maxDeloO = caseAlphabeticals.Where(x => x.ShortNumber == max.ToString()).OrderByDescending(x => x.RegDate).FirstOrDefault();
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
                regNumberSearch = x => EF.Functions.ILike(x.Case.RegNumber, model.NumberCase.ToCasePaternSearch());

            Expression<Func<CaseLawUnitDismisal, bool>> lawUnitSearch = x => true;
            if ((model.LawUnitId ?? 0) > 0)
                lawUnitSearch = x => x.CaseLawUnit.LawUnitId == model.LawUnitId;

            Expression<Func<CaseLawUnitDismisal, bool>> caseGroupWhere = x => true;
            if (string.IsNullOrEmpty(model.CaseGroupIds) == false)
            {
                var groups = model.CaseGroupIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                caseGroupWhere = x => groups.Contains(x.Case.CaseGroupId);
            }

            Expression<Func<CaseLawUnitDismisal, bool>> departmentWhere = x => true;
            if (string.IsNullOrEmpty(model.DepartmentIds) == false)
            {
                var departments = model.DepartmentIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                departmentWhere = x => departments.Contains(x.CaseLawUnit.CourtDepartmentId ?? 0);
            }

            return readonlyrepo.AllReadonly<CaseLawUnitDismisal>()
                               .Where(x => x.CourtId == courtId)
                               .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(x.CaseLawUnit.JudgeRoleId))
                               .Where(x => NomenclatureConstants.DismisalType.DismisalList.Contains(x.DismisalTypeId))
                               .Where(dateSearch)
                               .Where(regNumberSearch)
                               .Where(lawUnitSearch)
                               .Where(caseGroupWhere)
                               .Where(departmentWhere)
                               .Where(CaseExtensions.ConfirmedDismissalsOnly())
                               .Select(x => new DismisalReportVM()
                               {
                                   CaseSessionDate = x.CaseSessionAct.CaseSession.DateFrom,
                                   CaseRegNumber = x.Case.CaseGroup.Label + " " + x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM"),
                                   LawUnitName = x.CaseLawUnit.LawUnit.FullName,
                                   Description = x.Description,
                                   LawUnitNewName = string.Join("; ", readonlyrepo.AllReadonly<CaseSelectionProtokol>()
                                                                                             .Where(t => t.CaseLawUnitDismisalId == x.Id)
                                                                                             .Select(t => t.SelectedLawUnit.FullName))
                               })
                               .AsQueryable();
        }

        public async Task<byte[]> DismisalReportToExcelOne(DismisalReportFilterVM model)
        {
            var dataRows = await DismisalReport_Select(userContext.CourtId, model).OrderBy(x => x.CaseSessionDate).ToListAsync();
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
                personSearch = x => EF.Functions.ILike(x.SenderName, model.FullName.ToPaternSearch());

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
                           AmountBGN = x.AmountBGN ?? 0,
                       })
                       .AsQueryable();
        }

        public async Task<byte[]> PaymentPosReportToExcelOne(PaymentPosFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await PaymentPosReport_Select(userContext.CourtId, model).ToListAsync();

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Преведени суми през ПОС-терминал по сметка Бюджет/депозит за период от " + dateFrom + " до " + dateTo, 7,
                      styleTitle); excelService.AddRow();

            if (userContext.IsInterimPeriodEuro == false)
            {
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
                    //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                    //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index
                );
            }
            else
            {
                excelService.AddList(
                    dataRows,
                    new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                    new List<Expression<Func<PaymentPosReportVM, object>>>()
                    {
                    x => x.PaidDate,
                    x => x.PaidDateHour,
                    x => x.PaymentNumber,
                    x => x.Tid,
                    x => x.SenderName,
                    x => x.Description,
                    x => x.Amount,
                    x => x.AmountBGN,
                    },
                    //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                    //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index
                );
            }

            excelService.AddRow();
            excelService.AddRow();

            string currencyStr = userContext.IsPeriodEuro == true ? " евро" : " лв.";

            excelService.AddRangeMoveCol("Всичко по сметка (Бюджетна) " +
                dataRows.Where(x => x.MoneyGroupId == NomenclatureConstants.MoneyGroups.Budget).Count() +
                " бр. транзакции на стойност " +
                dataRows.Where(x => x.MoneyGroupId == NomenclatureConstants.MoneyGroups.Budget).Select(x => x.Amount).Sum() +
                currencyStr, 7, 1);

            excelService.AddRow();
            excelService.AddRangeMoveCol("Всичко по сметка (Депозитна) " +
                dataRows.Where(x => x.MoneyGroupId == NomenclatureConstants.MoneyGroups.Deposit).Count() +
                " бр. транзакции на стойност " +
                dataRows.Where(x => x.MoneyGroupId == NomenclatureConstants.MoneyGroups.Deposit).Select(x => x.Amount).Sum() +
                currencyStr, 7, 1);

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
                departmentWhere = x => x.CaseSessionAct.CaseSession.CaseLawUnits
                          .Where(a => (a.DateTo ?? dateEnd) >= x.CaseSessionAct.CaseSession.DateFrom &&
                          a.CourtDepartmentId == model.DepartmentId).Any();

            return readonlyrepo.AllReadonly<Obligation>()
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
                    PersonName = x.FullName + " " + (x.Person_SourceType == SourceTypeSelectVM.CasePerson ? readonlyrepo.AllReadonly<CasePerson>()
                                                  .Where(a => a.Id == (int)x.Person_SourceId)//Насилствено е - ще се мисли. Проблема е DocumentPerson.Id
                                                  .Select(a => a.PersonRole.Label).FirstOrDefault() :
                                                  readonlyrepo.AllReadonly<CaseLawUnit>()
                                                  .Where(a => a.Id == (int)x.Person_SourceId)//Насилствено е - ще се мисли. Проблема е DocumentPerson.Id
                                                  .Select(a => a.JudgeRole.Label).FirstOrDefault()),
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
                           AmountBGN = x.AmountBGN ?? 0,
                           AmountPay = x.ObligationPayments.Where(a => a.IsActive && a.Payment.IsActive)
                                      .Select(a => a.Amount).Sum(),
                           PaidDate = string.Join("; ", x.ObligationPayments
                                                         .Where(a => a.IsActive &&
                                                                     a.Payment.IsActive &&
                                                                     a.Amount > 0)
                                                         .Select(a => a.Payment.PaidDate.ToString("dd.MM.yyyy"))
                                                         .Distinct()),
                           PaidDateFirst = x.ObligationPayments
                                            .Where(a => a.IsActive &&
                                                        a.Payment.IsActive &&
                                                        a.Amount > 0)
                                            .Select(a => a.Payment.PaidDate)
                                            .FirstOrDefault(),
                           Description = ((x.Description ?? "") + " " + (x.MoneyFineType.Label ?? "")).Trim()
                       })
                       .AsQueryable();
        }

        public async Task<byte[]> FineReportToExcelOne(FineFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await FineReport_Select(userContext.CourtId, model).ToListAsync();

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка за глоби за период от " + dateFrom + " до " + dateTo, 9,
                      styleTitle); excelService.AddRow();

            if (userContext.IsInterimPeriodEuro == false)
            {
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
                    //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                    //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index
                );
            }
            else
            {
                excelService.AddList(
                    dataRows,
                    new int[] { 10000, 5000, 5000, 5000, 15000, 5000, 5000, 5000, 5000, 5000 },
                    new List<Expression<Func<FineReportVM, object>>>()
                    {
                    x => x.CaseGroupName,
                    x => x.CaseNumber,
                    x => x.SessionTypeName,
                    x => x.ObligationDate,
                    x => x.SenderName,
                    x => x.Amount,
                    x => x.AmountBGN,
                    x => x.PaidDate,
                    x => x.State,
                    x => x.Description,
                    },
                    //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                    //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index
                );
            }
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
                    DocumentDataSort = x.CaseId != null ? x.Case.RegDate : x.Document.DocumentDate,
                    DocumentData = x.CaseId != null ? (x.Case.CaseGroup.Label + " " + (x.Case.RegNumber ?? "")) :
                                    (x.Document.DocumentType.Label + " " + x.Document.DocumentNumber + "/" +
                                    x.Document.DocumentDate.ToString("dd.MM.yyyy")),
                    PaymentDataAll = x.ObligationPayments.Where(a => a.IsActive && a.Payment.IsActive)
                                  .Select(a => a.Payment.PaymentType.Label + " " + a.Payment.PaymentNumber + "/" +
                                      a.Payment.PaidDate.ToString("dd.MM.yyyy")).ToArray(),
                    CaseTypeCode = x.Case.CaseType.Code,
                    ObligationDate = x.ObligationDate,
                    SenderName = x.FullName,
                    Amount = x.Amount,
                    AmountBGN = x.AmountBGN ?? 0,
                    PaidDate = string.Join("; ", x.ObligationPayments
                                                  .Where(a => a.IsActive &&
                                                              a.Payment.IsActive &&
                                                              a.Amount > 0)
                                                  .Select(a => a.Payment.PaidDate.ToString("dd.MM.yyyy"))
                                                  .Distinct()),
                    PaidDateDate = x.ObligationPayments
                                    .Where(a => a.IsActive &&
                                                a.Payment.IsActive &&
                                                a.Amount > 0)
                                    .Select(a => a.Payment.PaidDate)
                                    .FirstOrDefault(),
                    PaymentDescription = string.Join("; ", x.ObligationPayments
                                                            .Where(a => a.IsActive &&
                                                                        a.Payment.IsActive &&
                                                                        a.Amount > 0)
                                                            .Select(a => a.Payment.PaymentInfo)
                                                            .Distinct()),
                    PaymentDescriptionFirst = x.ObligationPayments
                                               .Where(a => a.IsActive &&
                                                           a.Payment.IsActive &&
                                                           a.Amount > 0)
                                               .Select(a => a.Payment.PaymentInfo)
                                               .FirstOrDefault(),
                    Description = x.MoneyType.Label + " " + (x.MoneyFeeType.Label ?? "") + "; " + (x.Description ?? ""),
                })
                .AsQueryable();
        }

        public async Task<byte[]> StateFeeReportExportExcel(StateFeeFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await StateFeeReport_Select(userContext.CourtId, model).ToListAsync();

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";
            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка за държавни такси за период от " + dateFrom + " до " + dateTo, 9,
                      styleTitle); excelService.AddRow();

            if (userContext.IsInterimPeriodEuro == false)
            {
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
                    //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                    //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index
                );
            }
            else
            {
                excelService.AddList(
                    dataRows,
                    new int[] { 10000, 5000, 5000, 5000, 15000, 5000, 5000, 5000, 5000, 5000 },
                    new List<Expression<Func<StateFeeReportVM, object>>>()
                    {
                    x => x.DocumentData,
                    x => x.PaymentData,
                    x => x.CaseTypeCode,
                    x => x.ObligationDate,
                    x => x.SenderName,
                    x => x.Amount,
                    x => x.AmountBGN,
                    x => x.PaidDate,
                    x => x.PaymentDescription,
                    x => x.Description,
                    },
                    //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                    //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index
                );
            }

            excelService.AddRow();
            excelService.AddRow();
            excelService.AddRangeMoveCol(dataRows.Count + " бр. записи отговарящи на зададените критерии.", 2, 1);

            return excelService.ToArray();
        }
        public IQueryable<ObligationJuryReportVM> ObligationJuryReport_Select(int courtId, ObligationJuryFilterReportVM model)
        {
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

            Expression<Func<Obligation, bool>> personNameWhere = x => true;
            if (string.IsNullOrEmpty(model.PersonName) == false)
                personNameWhere = x => EF.Functions.ILike(x.FullName, model.PersonName.ToPaternSearch());

            Expression<Func<Obligation, bool>> personTypeWhere = x => true;
            if (string.IsNullOrEmpty(model.PersonType) == false && model.PersonType != "-1")
            {
                (int sourceType, long sourceId) = SourceTypeSelectVM.GetSourceTypeSourceId(model.PersonType);
                if (sourceType == SourceTypeSelectVM.LawUnit)
                {
                    personTypeWhere = x => ((x.IsForMinAmount ?? false) || (x.Person_SourceType == SourceTypeSelectVM.CaseLawUnit &&
                                          readonlyrepo.AllReadonly<CaseLawUnit>()
                                           .Where(a => a.Id == (int)x.Person_SourceId) // Насилствено е - ще се мисли. Проблема е DocumentPerson.Id
                                           .Where(a => a.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
                                           .Any()
                                          ));
                }
                else if (sourceType == SourceTypeSelectVM.CasePerson)
                {
                    int personRoleId = (int)sourceId;
                    personTypeWhere = x => x.Person_SourceType == sourceType &&
                                          readonlyrepo.AllReadonly<CasePerson>()
                                           .Where(a => a.Id == (int)x.Person_SourceId) // Насилствено е - ще се мисли. Проблема е DocumentPerson.Id
                                           .Where(a => a.PersonRoleId == personRoleId)
                                           .Any();

                }
            }

            return readonlyrepo.AllReadonly<Obligation>()
                .Where(x => x.CourtId == courtId)
                .Where(x => (x.IsActive ?? true) == true)
                .Where(x => x.MoneySign == NomenclatureConstants.MoneySign.SignMinus)
                .Where(x => ((x.MoneyType.IsEarning ?? false) == true || (x.MoneyType.IsTransport ?? false) == true))
                .Where(dateSearch)
                .Where(groupWhere)
                .Where(typeWhere)
                .Where(sessionWhere)
                .Where(moneyGroupWhere)
                .Where(personNameWhere)
                .Where(personTypeWhere)
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
                    Amount = (x.MoneyType.IsEarning ?? false) == true ? x.Amount : 0,
                    AmountBGN = (x.MoneyType.IsEarning ?? false) == true ? (x.AmountBGN ?? 0) : 0,
                    AmountTransport = (x.MoneyType.IsTransport ?? false) == true ? x.Amount : 0,
                    AmountTransportBGN = (x.MoneyType.IsTransport ?? false) == true ? (x.AmountBGN ?? 0) : 0,
                    AmountPayment = x.ObligationPayments.Where(p => p.IsActive == true).Select(p => p.Amount).Sum(),
                    Description = x.IsForMinAmount == true ? x.ObligationInfo : x.Description,
                    MoneyGroupName = x.MoneyType.MoneyGroup.Label,
                    ExpenseOrderDates = x.ExpenseOrderObligations.Where(a => a.ExpenseOrder.IsActive == true)
                                                                 .Select(a => a.ExpenseOrder.RegDate.ToString("dd.MM.yyyy"))
                                                                 .FirstOrDefault(),
                    ExpenseOrderUsers = x.ExpenseOrderObligations.Where(a => a.ExpenseOrder.IsActive == true)
                                                                 .Select(a => a.ExpenseOrder.User.LawUnit.FullName)
                                                                 .FirstOrDefault(),
                    ExpenseOrderJudge = x.ExpenseOrderObligations.Where(a => a.ExpenseOrder.IsActive == true)
                                                                 .Select(a => a.ExpenseOrder.LawUnitSign.FullName)
                                                                 .FirstOrDefault(),
                    SessionTime = (x.CaseSessionId == null && x.CaseSessionActId == null && x.CaseSessionMeetingId == null) ? ""
                                   :
                                   x.CaseSessionMeetingId != null ?
                                   (x.CaseSessionMeeting.DateFrom.ToString(FormattingConstant.NormalDateFormatHHMM) + " - " +
                                              x.CaseSessionMeeting.DateTo.ToString(FormattingConstant.NormalDateFormatHHMM))
                                   :
                                   x.CaseSessionId != null ?
                                   string.Join(", ", x.CaseSession.CaseSessionMeetings.Where(a => a.DateExpired == null)
                                                .Select(a => a.DateFrom.ToString(FormattingConstant.NormalDateFormatHHMM) + " - " +
                                                        a.DateTo.ToString(FormattingConstant.NormalDateFormatHHMM)))
                                   :
                                   string.Join(", ", x.CaseSessionAct.CaseSession.CaseSessionMeetings.Where(a => a.DateExpired == null)
                                                .Select(a => a.DateFrom.ToString(FormattingConstant.NormalDateFormatHHMM) + " - " +
                                                        a.DateTo.ToString(FormattingConstant.NormalDateFormatHHMM)))
                                   ,
                    SessionResult = (x.CaseSessionId == null && x.CaseSessionActId == null) ? "" :
                                   (x.CaseSessionId != null ? string.Join(", ", x.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null).Select(a => a.SessionResult.Label)) :
                                   string.Join(", ", x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null).Select(a => a.SessionResult.Label))
                                   ),

                })
                .ToList() //не може да го преведе заради stringJoin. Ще видя колко е бавно, защото друго не успях
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
                    x.CaseId,
                    x.SessionTime,
                    x.SessionResult,
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
                                    AmountBGN = g.Sum(x => x.AmountBGN),
                                    AmountTransport = g.Sum(x => x.AmountTransport),
                                    AmountTransportBGN = g.Sum(x => x.AmountTransportBGN),
                                    AmountPayment = g.Sum(x => x.AmountPayment),
                                    Description = g.Max(x => x.Description),
                                    MoneyGroupName = g.Key.MoneyGroupName,
                                    ExpenseOrderDates = string.Join("; ",
                                     g.Where(x => string.IsNullOrEmpty(x.ExpenseOrderDates) == false)
                                     .Select(x => x.ExpenseOrderDates)
                                     .Distinct()),
                                    ExpenseOrderUsers = string.Join("; ",
                                     g.Where(x => string.IsNullOrEmpty(x.ExpenseOrderUsers) == false)
                                     .Select(x => x.ExpenseOrderUsers)
                                     .Distinct()),
                                    ExpenseOrderJudge = string.Join("; ",
                                     g.Where(x => string.IsNullOrEmpty(x.ExpenseOrderJudge) == false)
                                     .Select(x => x.ExpenseOrderJudge)
                                     .Distinct()),
                                    SessionTime = g.Key.SessionTime,
                                    SessionResult = g.Key.SessionResult,
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
            string additionalTitle = "";
            if (string.IsNullOrEmpty(model.PersonName) == false)
                additionalTitle = " за " + model.PersonName;
            else if (model.PersonType != "-1")
                additionalTitle = " за " + model.PersonTypeLabel;

            if (userContext.IsInterimPeriodEuro == false)
                additionalTitle += " - " + dataRows.Sum(x => x.Amount + x.AmountTransport).ToString("0.00") + " евро";
            else
                additionalTitle += " - " + dataRows.Sum(x => x.Amount + x.AmountTransport).ToString("0.00") + " евро, левова равностойност " + dataRows.Sum(x => x.AmountBGN + x.AmountTransportBGN).ToString("0.00");

            var columns = GetAllVisibleColumns(new ObligationJuryReportVM(), model.ColumnVisibility);
            excelService.AddRange("Справка за възнаграждения за периода от " + dateFrom + " до " + dateTo + additionalTitle, columns.Count,
                      styleTitle); excelService.AddRow();

            if (userContext.IsInterimPeriodEuro == false)
            {
                excelService.AddList(
                    dataRows,
                    new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                    new List<Expression<Func<ObligationJuryReportVM, object>>>()
                    {
                    x => x.CaseTypeName,
                    x => x.CaseNumber,
                    x => x.SessionTypeName,
                    x => x.SessionDate,
                    x => x.SessionTime,
                    x => x.SessionResult,
                    x => x.PersonName,
                    x => x.MoneyGroupName,
                    x => x.ObligationDate,
                    x => x.Amount,
                    x => x.AmountTransport,
                    x => x.Status,
                    x => x.Description,
                    x => x.ExpenseOrderDates,
                    x => x.ExpenseOrderUsers,
                    x => x.ExpenseOrderJudge,
                    },
                    //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                    //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    NPOI.HSSF.Util.HSSFColor.White.Index,
                    true,
                    columns
                );
            }
            else
            {
                excelService.AddList(
                                    dataRows,
                                    new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                                    new List<Expression<Func<ObligationJuryReportVM, object>>>()
                                    {
                    x => x.CaseTypeName,
                    x => x.CaseNumber,
                    x => x.SessionTypeName,
                    x => x.SessionDate,
                    x => x.SessionTime,
                    x => x.SessionResult,
                    x => x.PersonName,
                    x => x.MoneyGroupName,
                    x => x.ObligationDate,
                    x => x.Amount,
                    x => x.AmountBGN,
                    x => x.AmountTransport,
                    x => x.AmountTransportBGN,
                    x => x.Status,
                    x => x.Description,
                    x => x.ExpenseOrderDates,
                    x => x.ExpenseOrderUsers,
                    x => x.ExpenseOrderJudge,
                                    },
                                    //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                                    //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                                    NPOI.HSSF.Util.HSSFColor.White.Index,
                                    NPOI.HSSF.Util.HSSFColor.White.Index,
                                    NPOI.HSSF.Util.HSSFColor.White.Index,
                                    true,
                                    columns
                                );
            }

            return excelService.ToArray();
        }
        private async Task<List<DeliveryBookVM>> DeliveryBook_Select(int courtId, DeliveryBookFilterVM model)
        {
            int[] institutionTypeIds = { NomenclatureConstants.InstitutionTypes.Attourney };
            DateTime startDate = model.DateFrom.ForceStartDate();
            DateTime endDate = model.DateTo.ForceEndDate();
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
                personNameWhere = x => x.DocumentPersons.Where(p => EF.Functions.ILike(p.FullName, model.CasePersonName.ToPaternSearch())).Any();

            Expression<Func<Document, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.DocumentCaseInfo.Where(p => p.Case.CaseGroupId == model.CaseGroupId).Any();

            Expression<Func<Document, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.DocumentCaseInfo.Where(p => p.Case.CaseTypeId == model.CaseTypeId).Any();

            return await repo.AllReadonly<Document>()
                                .Where(x => x.CourtId == courtId && x.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                                .Where(x => x.DocumentDate >= startDate && x.DocumentDate <= endDate)
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
                                    DocumentPersonName = string.Join(Environment.NewLine, x.DocumentPersons.Select(p => p.FullName)),
                                    DocumentNumberValue = (x.DocumentNumberValue ?? 0),
                                    DocumentLinkDate = x.DocumentLinks.Where(d => d.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                                                       .Select(d => (DateTime?)d.PrevDocument.DocumentDate)
                                                       .FirstOrDefault(),
                                    DocumentLinkUser = x.DocumentLinks.Where(d => d.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                                                                      .Select(d => d.Document.User.LawUnit.FullName)
                                                                      .FirstOrDefault(),
                                    CaseGroupName = x.DocumentCaseInfo.Select(d => d.Case.CaseGroup.Label).FirstOrDefault(),
                                    CaseNumber = x.DocumentCaseInfo.Select(d => d.Case.RegNumber).FirstOrDefault(),
                                    CaseDate = x.DocumentCaseInfo.Select(d => (DateTime?)d.Case.RegDate).FirstOrDefault()
                                })
                                .OrderBy(x => x.DocumentNumberValue)
                                .ToListAsync();
        }

        public async Task<byte[]> DeliveryBookReportToExcelOne(DeliveryBookFilterVM model)
        {
            var dataRows = await DeliveryBook_Select(userContext.CourtId, model);

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

            Expression<Func<CaseSession, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom &&
                      a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            var result = repo.AllReadonly<CaseSession>()
                             .Where(x => x.Case.CourtId == courtId && x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PrivateSession)
                             .Where(x => x.DateFrom >= model.DateFrom && x.DateFrom <= model.DateTo)
                             .Where(x => x.DateExpired == null)
                             .Where(x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                             .Where(caseGroupWhere)
                             .Where(departmentWhere)
                             .Where(x => x.CaseSessionActs.Where(a => NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.ActStateId) && a.DateExpired == null).Any())
                             .Where(judgeReporterSearch)
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
                                 CaseSessionResultName = string.Join(Environment.NewLine, x.CaseSessionResults
                                                        .Where(a => a.IsMain && a.IsActive && a.DateExpired == null)
                                                        .Select(p => p.SessionResult.Label)),
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
                             })
                             .AsQueryable();

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
            var result = complainResults.Where(x => x.CaseSessionActId != null).Select(x => x.CaseSessionAct.CaseSession.SessionType.Label + " " + x.CaseSessionAct.CaseSession.DateFrom.ToString("dd.MM.yyyy") +
                       Environment.NewLine + x.CaseSessionAct.ActType.Label + " " +
                       x.CaseSessionAct.RegNumber + "/" + ((DateTime)x.CaseSessionAct.RegDate).ToString("dd.MM.yyyy") +
                       Environment.NewLine + (x.Description ?? "")
                       )
                       .Distinct()
                       .ToList();

            var resultWithoutAct = complainResults.Where(x => x.CaseSessionActId == null).Select(x => x.CaseSessionActOtherSystem +
                       Environment.NewLine + (x.Description ?? "")
                       )
                       .Distinct()
                       .ToList();

            result.AddRange(resultWithoutAct);
            return result.ToArray();
        }

        private async Task<List<InsolvencyReportVM>> InsolvencyReport_Select(int courtId, InsolvencyFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

            Expression<Func<Case, bool>> regNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.CaseNumber) == false)
                regNumberSearch = x => EF.Functions.ILike(x.RegNumber, model.CaseNumber.ToEndingPaternSearch());

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> yearSearch = x => true;
            if ((model.CaseYear ?? 0) > 0)
                yearSearch = x => x.RegDate >= NomenclatureExtensions.GetPastDate() && x.RegDate.Year == model.CaseYear;

            var caseCodes = await repo.AllReadonly<CaseCodeGrouping>()
                                      .Where(x => x.CaseCodeGroup == NomenclatureConstants.CaseCodeGroupings.InsolvencyReport)
                                      .Select(x => x.CaseCodeId)
                                      .ToListAsync();

            int[] personRoles = { NomenclatureConstants.PersonRole.Debtor, NomenclatureConstants.PersonRole.Kreditor,
                                 NomenclatureConstants.PersonRole.KomitetKreditor, NomenclatureConstants.PersonRole.Sindik};

            //Това беше за едно дело. Сега се преправя за много дела, а се четат страшно много обекти. Ако видим, че става бавно може да се 
            //изнесат още някой неща на отделно четене
            var caseModel = await readonlyrepo.AllReadonly<Case>()
                                              .Include(x => x.CasePersons.Where(a => a.DateExpired == null && a.CaseSessionId == null && a.PersonRoleId == NomenclatureConstants.PersonRole.Debtor)) //Само този ни трябва
                                              .ThenInclude(x => x.Addresses)
                                              .ThenInclude(x => x.Address)
                                              .Include(x => x.CaseLawUnits.Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)) //Само този ни трябва
                                              .ThenInclude(x => x.LawUnit)
                                              .Include(x => x.CaseLawUnits.Where(a => a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter))
                                              .ThenInclude(x => x.CourtDepartment)
                                              .Include(x => x.CaseSessionActs.Where(a => a.DateExpired == null && NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.ActStateId)))
                                              .ThenInclude(x => x.ActType)
                                              .Include(x => x.CaseSessionActs.Where(a => a.DateExpired == null && NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.ActStateId)))
                                              .ThenInclude(x => x.CaseSession)
                                              .Include(x => x.Document)
                                              .Include(x => x.Document.DocumentType)
                                              .Include(x => x.Document.DocumentPersons)
                                              .ThenInclude(x => x.PersonRole)
                                              .Where(x => x.CourtId == courtId)
                                              .Where(x => x.RegDate.Date >= model.DateFrom.Date && x.RegDate.Date <= model.DateTo.Date)
                                              .Where(x => caseCodes.Contains(x.CaseCodeId ?? 0))
                                              .Where(caseGroupWhere)
                                              .Where(yearSearch)
                                              .Where(regNumberSearch)
                                              .ToListAsync();

            var caseIds = caseModel.Select(x => x.Id).ToList();

            //Всички жаления
            var allActsComplainResult = await readonlyrepo.AllReadonly<CaseSessionActComplainResult>()
                                                          .Include(x => x.CaseSessionActComplain)
                                                          .Include(x => x.CaseSessionAct)
                                                          .Include(x => x.ComplainCourt)
                                                          .Include(x => x.CaseSessionAct.CaseSession)
                                                          .Include(x => x.CaseSessionAct.CaseSession.SessionType)
                                                          .Include(x => x.CaseSessionAct.ActResult)
                                                          .Include(x => x.CaseSessionAct.ActType)
                                                          .Where(x => caseIds.Contains(x.CaseId))
                                                          .ToListAsync();

            //Четат се съпровождащите документи
            var allComplainDocuments = await readonlyrepo.AllReadonly<Document>()
                                                         .Where(x => x.DocumentCaseInfo.Where(a => caseIds.Contains(a.CaseId ?? 0)).Any())
                                                         .Where(x => x.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                                                         .Where(x => x.DocumentDate.Date >= model.DateFrom.Date && x.DocumentDate.Date <= model.DateTo.Date)
                                                         .Where(x => x.DateExpired == null)
                                                         .Select(x => new InsolvencyReportVM
                                                         {
                                                             CaseId = x.DocumentCaseInfo.Where(a => (a.CaseId ?? 0) > 0).Select(a => a.CaseId ?? 0).FirstOrDefault(),
                                                             DocumentId = x.Id,
                                                             SessionDate = x.DocumentDate,
                                                             DocumentDate = x.DocumentDate,
                                                             DocumentTypeName = x.DocumentType.Label +
                                                             " входящ документ №" + x.DocumentNumber + "/" + x.DocumentDate.Year,
                                                             PersonNames = string.Join(Environment.NewLine, x.DocumentPersons
                                                              .Select(a => a.FullName + "(" + a.PersonRole.Label + ")")),
                                                             DocumentActId = x.DocumentCaseInfo.Select(a => a.SessionActId).FirstOrDefault(),
                                                         })
                                                         .ToListAsync();

            //Четат се заседанията с хората в тях, но с точно определени роли, защото не ми трябва всички
            var allCaseSessions = await readonlyrepo.AllReadonly<CaseSession>()
                                                    .Include(x => x.CasePersons.Where(a => a.DateExpired == null && personRoles.Contains(a.PersonRoleId)))
                                                    .ThenInclude(x => x.PersonRole)
                                                    .Include(x => x.SessionType)
                                                    .Include(x => x.CaseSessionDocs.Where(a => a.DateExpired == null))
                                                    .Where(x => caseIds.Contains(x.CaseId))
                                                    .Where(x => x.DateExpired == null)
                                                    .Where(x => x.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                                                    .Where(x => x.DateFrom.Date >= model.DateFrom.Date && x.DateFrom.Date <= model.DateTo.Date)
                                                    .ToListAsync();

            List<InsolvencyReportVM> result = new List<InsolvencyReportVM>();

            foreach (var itemOneCase in caseModel)
            {
                List<InsolvencyReportVM> oneCase = new List<InsolvencyReportVM>();

                //Всички жаления за делото
                var actsComplainResult = allActsComplainResult
                                         .Where(x => x.CaseId == itemOneCase.Id)
                                         .ToList();

                //Всички заседания за делото
                var caseSessions = allCaseSessions.Where(x => x.CaseId == itemOneCase.Id).ToList();

                //Четат се заседанията
                oneCase.AddRange(caseSessions
                                 .Select(x => new InsolvencyReportVM
                                 {
                                     SessionId = x.Id,
                                     SessionDate = x.DateFrom,
                                     SessionData = x.SessionType.Label + " " + x.DateFrom.ToString("dd.MM.yyyy"),
                                     PersonNames = string.Join(Environment.NewLine, x.CasePersons.Select(a => a.FullName + "(" + a.PersonRole.Label + ")")),
                                     SessionDocumentIds = x.CaseSessionDocs.Select(a => a.DocumentId).ToList()
                                 }).ToList());

                foreach (var item in oneCase)
                {
                    item.DateOrder1 = item.SessionDate;
                    item.DateOrder2 = item.SessionDate;

                    var sessionActs = itemOneCase.CaseSessionActs.Where(x => x.CaseSessionId == item.SessionId && x.ActDeclaredDate != null).ToList();
                    item.SessionActData = string.Join("; ", sessionActs
                                                           .Select(a => a.ActType.Label + " №" +
                                                           a.RegNumber + "/" + ((DateTime)a.ActDeclaredDate).ToString("dd.MM.yyyy") +
                                                     Environment.NewLine + a.Description));
                    item.Acts = item.SessionData + Environment.NewLine + item.SessionActData;
                }

                //Четат се съпровождащите документи
                var resultDocument = allComplainDocuments
                                    .Where(x => x.CaseId == itemOneCase.Id)
                                    .ToList();

                foreach (var item in resultDocument)
                {
                    //Ако един съпровождащ документ е насочен към акт, то трябва да излезе под заседанието в който е този акт
                    item.DateOrder1 = item.SessionDate;
                    if (item.DocumentActId != null)
                    {
                        var act = itemOneCase.CaseSessionActs.Where(x => x.Id == (item.DocumentActId ?? 0)).FirstOrDefault();
                        if (act != null)
                        {
                            var sessionResult = oneCase.Where(x => x.SessionId == act.CaseSessionId).FirstOrDefault();
                            if (sessionResult != null)
                            {
                                item.DateOrder1 = sessionResult.SessionDate;
                            }
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
                                       oneCase.Where(x => x.SessionDocumentIds.Contains(item.DocumentId ?? 0)).Select(x => x.SessionActData));
                }

                oneCase.AddRange(resultDocument);

                //Добавяне на входящия документ
                var documentIn = new InsolvencyReportVM();
                documentIn.DateOrder1 = DateTime.Now.AddYears(-100);
                documentIn.DocumentDate = itemOneCase.Document.DocumentDate;
                documentIn.SessionDate = itemOneCase.Document.DocumentDate;
                documentIn.DocumentTypeName = itemOneCase.Document.DocumentType.Label + " №" + itemOneCase.Document.DocumentNumber +
                    "/" + itemOneCase.Document.DocumentDate.Year;
                documentIn.PersonNames = string.Join(Environment.NewLine, itemOneCase.Document.DocumentPersons
                                .Select(a => a.FullName + "(" + a.PersonRole.Label + ")"));

                oneCase.Add(documentIn);

                //Филтъра по роля е в Where горе
                string debtorNames = string.Join(Environment.NewLine, itemOneCase.CasePersons
                                                    .Select(a => a.FullName + ", " + a.Addresses.Select(z => z.Address.FullAddress)
                                                    .FirstOrDefault()));
                int i = 1;
                foreach (var item in oneCase.OrderBy(x => x.DateOrder1).ThenBy(x => x.DateOrder2).ToList())
                {
                    item.NumberAction = i; i++;
                    item.CaseId = itemOneCase.Id;
                    item.CaseNumber = itemOneCase.RegNumber;
                    item.CaseGroupId = itemOneCase.CaseGroupId;
                    item.JudgeReporterName = itemOneCase.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= item.SessionDate && (a.CaseSessionId ?? 0) == (item.SessionId ?? 0)) //Филтъра по роля е в Where горе
                                                                  .Select(a => a.LawUnit.FullName + " " +
                                                                  (a.CourtDepartmentId == null ? "" : a.CourtDepartment.Label)).FirstOrDefault();
                    item.DebtorName = debtorNames;
                }

                result.AddRange(oneCase);
            }

            return result.OrderBy(x => x.CaseId).ThenBy(x => x.NumberAction).ToList();
        }

        public async Task<byte[]> InsolvencyReportToExcelOne(InsolvencyFilterReportVM model)
        {
            var dataRows = await InsolvencyReport_Select(userContext.CourtId, model);

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

            var personRoles = repo.AllReadonly<PersonRoleGrouping>()
                                 .Where(x => x.PersonRoleGroup == NomenclatureConstants.PersonRoleGroupings.ZzdnReport)
                                 .Select(x => x.PersonRoleId)
                                 .ToArray();

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
                                                              .FirstOrDefault())
                                                   .FirstOrDefault(),
                                    CasePersons = x.Cases.Select(a => string.Join(Environment.NewLine, a.CasePersons
                                                                  .Where(p => p.DateExpired == null && p.CaseSessionId == null && personRoles.Contains(p.PersonRoleId) == true)
                                                                  .Select(p => p.FullName + "(" + p.PersonRole.Label + ")"))).FirstOrDefault()
                                }).AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        public async Task<byte[]> ZzdnReportToExcelOne(ZzdnFilterReportVM model)
        {
            var dataRows = await ZzdnReport_Select(userContext.CourtId, model).ToListAsync();
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
                regNumberSearch = x => EF.Functions.ILike(x.RegNumber, model.RegNumber.ToCasePaternSearch());

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

            return repo.AllReadonly<Case>()
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
        }

        public async Task<byte[]> EuropeanHeritageReportToExcelOne(EuropeanHeritageFilterReportVM model)
        {
            var dataRows = await EuropeanHeritageReport_Select(userContext.CourtId, model).ToListAsync();

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

            return repo.AllReadonly<Document>()
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
                                      .Select(a => a.DecisionType.Label + Environment.NewLine + "Решение № " +
                                        a.RegNumber + "/" + ((DateTime)a.RegDate).ToString("dd.MM.yyyy")
                                      )
                                      .FirstOrDefault(),
                           DecisionDescription = repo.AllReadonly<DocumentDecision>().Where(a => a.DocumentId == x.Id
                                      && a.DocumentDecisionStateId == NomenclatureConstants.DocumentDecisionStates.Resolution)
                                      .Select(a => a.Description)
                                      .FirstOrDefault()
                       }).AsQueryable();
        }

        public async Task<byte[]> PublicInformationReportToExcelOne(PublicInformationFilterReportVM model)
        {
            var dataRows = await PublicInformationReport_Select(userContext.CourtId, model).ToListAsync();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }

            NPoiExcelService excelService = GetExcelHtmlTemplate("PublicInformationNew");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<PublicInformationReportVM, object>>>()
                {
                    x => x.Index,
                    x => x.Notifier,
                    x => x.DocumentData,
                    x => x.Description,
                    x => x.Decision,
                    x => x.DecisionDescription
                }
            );
            return excelService.ToArray();
        }

        public async Task<byte[]> PublicInformationReportToExcelOnePrev(PublicInformationFilterReportVM model)
        {
            var dataRows = await PublicInformationReport_Select(userContext.CourtId, model).ToListAsync();
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
                groupSearch = x => x.Case.CaseGroupId == model.CaseGroupId;

            int? courtIdNull = (int?)courtId;

            return repo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.CourtId == courtIdNull)
                       .Where(x => x.ActDeclaredDate != null)
                       .Where(x => x.ActTypeId == NomenclatureConstants.ActType.Answer)
                       .Where(x => x.Case.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo) // Без наказателни дела
                       .Where(groupSearch)
                       .Where(dateSearch)
                       .Select(x => new CaseDecisionReportVM
                       {
                           CaseGroupName = x.Case.CaseGroup.Label,
                           CaseRegNumber = x.Case.RegNumber,
                           JudgeReporterName = x.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.CaseSession.DateFrom &&
                                                         a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                     .Select(a => a.LawUnit.FullName).FirstOrDefault(),
                           DepersonalizeUser = x.DepersonalizeUser.LawUnit.FullName,
                           ActDeclaredDate = (DateTime)x.ActDeclaredDate,
                           ActLink = x.ActType.Label + " " + x.RegNumber + (x.ActDate != null ? ("/" + ((DateTime)x.ActDate).Year.ToString()) : "") +
                                        Environment.NewLine +
                                        (model.WithActDescription == false ? "" :
                                           (model.WithoutActDescriptionCaseRestriction == true &&
                                                x.Case.CaseClassifications
                                                .Where(a => a.ClassificationId == NomenclatureConstants.CaseClassifications.Restriction &&
                                                       a.DateTo == null && a.CaseSessionId == null)
                                                .Any() ? "***" : (x.Description ?? ""))
                                        ),
                           FileAct = repo.AllReadonly<MongoFile>().Where(a => a.SourceType == SourceTypeSelectVM.CaseSessionActDepersonalized &&
                                           a.SourceId == x.Id.ToString()).Select(a => a.FileId).FirstOrDefault(),
                           FileMotive = repo.AllReadonly<MongoFile>().Where(a => a.SourceType == SourceTypeSelectVM.CaseSessionActMotiveDepersonalized &&
                                           a.SourceId == x.Id.ToString()).Select(a => a.FileId).FirstOrDefault(),
                       }).AsQueryable();
        }

        public async Task<byte[]> CaseDecisionReportToExcelOne(CaseDecisionFilterReportVM model, string url)
        {
            var dataRows = await CaseDecisionReport_Select(userContext.CourtId, model).OrderBy(x => x.ActDeclaredDate).ToListAsync();

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

            Expression<Func<CaseSessionAct, bool>> dateSearch = x => true;
            dateSearch = x => x.ActDeclaredDate >= model.DateFrom.ForceStartDate() && x.ActDeclaredDate <= model.DateTo.ForceEndDate();

            DateTime dateInforcedFromSearch = model.FromActInforcedDate ?? DateTime.Now.AddYears(-100);
            DateTime dateInforcedToSearch = model.ToActInforcedDate ?? DateTime.Now.AddYears(100);

            Expression<Func<CaseSessionAct, bool>> actInforcedDateSearch = x => true;
            if (model.FromActInforcedDate != null || model.ToActInforcedDate != null)
                actInforcedDateSearch = x => x.ActInforcedDate != null && x.ActInforcedDate >= dateInforcedFromSearch.ForceStartDate() &&
                           x.ActInforcedDate <= dateInforcedToSearch.ForceEndDate();

            Expression<Func<CaseSessionAct, bool>> actNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.NumberAct) == false)
                actNumberSearch = x => x.RegNumber == model.NumberAct;

            int[] documentTypesGrouping = { NomenclatureConstants.DocumentTypeGroupings.OtherHeritage, NomenclatureConstants.DocumentTypeGroupings.RefuseHeritage,
                                              NomenclatureConstants.DocumentTypeGroupings.AcceptHeritage};
            var documentTypes = repo.AllReadonly<DocumentTypeGrouping>()
                                    .Where(x => documentTypesGrouping.Contains(x.DocumentTypeGroup))
                                    .ToList();

            int[] personRoles = { NomenclatureConstants.PersonRole.Inheritor, NomenclatureConstants.PersonRole.Legator };

            var notifiers = repo.AllReadonly<PersonRoleGrouping>()
                                .Where(x => x.PersonRoleGroup == NomenclatureConstants.PersonRoleGroupings.HeritageReportPersonNotifier)
                                .Select(x => x.PersonRoleId)
                                .ToArray();

            return repo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.DateExpired == null)
                       .Where(x => x.Case.CourtId == courtId)
                       .Where(x => x.ActDeclaredDate != null)
                       .Where(x => (documentTypes.Select(a => a.DocumentTypeId)).Contains(x.Case.Document.DocumentTypeId))
                       .Where(x => x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                       .Where(x => x.CasePersonInheritances.Any(i => i.DateExpired == null && (i.DateCreate ?? model.DateTo).Date <= model.DateTo))
                       .Where(actNumberSearch)
                       .Where(dateSearch)
                       .Where(actInforcedDateSearch)
                       .Select(x => new HeritageReportVM
                       {
                           ActDeclaredDate = x.ActDeclaredDate ?? DateTime.Now, //влизат само тези с ActDeclaredDate != null
                           DocumentNumber = x.Case.Document.DocumentNumber + "/" + x.Case.Document.DocumentDate.ToString("dd.MM.yyyy") +
                                            "; " + x.Case.CaseType.Code + " " + x.Case.RegNumber + "; Съдия-докладчик " +
                                            x.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.CaseSession.DateFrom &&
                                                         a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                     .Select(a => a.LawUnit.FullName).FirstOrDefault(),
                           DocumentDate = x.Case.Document.DocumentDate,
                           Notifier = string.Join(Environment.NewLine,
                                      x.Case.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                                                 notifiers.Contains(a.PersonRoleId))
                                                   .Select(a => a.FullName + " (" + a.PersonRole.Label + ")")),
                           RefuseHeritage = string.Join(Environment.NewLine, x.CasePersonInheritances
                                        .Where(a => a.DateExpired == null && (a.DateCreate ?? model.DateTo).Date <= model.DateTo &&
                                               a.CasePersonInheritanceResultId != NomenclatureConstants.CasePersonInheritanceResults.Accept)
                                        .Select(a => a.CasePerson.FullName + " - " + a.CasePersonInheritanceResult.Label)
                                        ),
                           AcceptHeritage = string.Join(Environment.NewLine, x.CasePersonInheritances
                                        .Where(a => a.DateExpired == null && (a.DateCreate ?? model.DateTo).Date <= model.DateTo &&
                                               a.CasePersonInheritanceResultId == NomenclatureConstants.CasePersonInheritanceResults.Accept)
                                        .Select(a => a.CasePerson.FullName + " - " + a.CasePersonInheritanceResult.Label)
                                        ),
                           ResultData = string.Join(Environment.NewLine, x.Case.CasePersons
                                                                          .Where(a => a.CaseSessionId == null &&
                                                                          a.DateExpired == null &&
                                                                          a.PersonRoleId == NomenclatureConstants.PersonRole.Legator)
                                           .Select(a => a.FullName)),
                           PersonNames = string.Join(Environment.NewLine, x.Case.CasePersons
                                                                          .Where(a => a.CaseSessionId == null &&
                                                                          a.DateExpired == null &&
                                                                          personRoles.Contains(a.PersonRoleId))
                                           .Select(a => a.FullName + " (" + a.PersonRole.Label + ")")),
                           Description = x.ActType.Label +
                                                 (x.ActDate != null ? (Environment.NewLine + x.RegNumber + "/" + ((DateTime)x.ActDate).ToString("dd.MM.yyyy")) : "") +
                                                 Environment.NewLine +
                                                 string.Join(Environment.NewLine, x.CasePersonInheritances.Where(a => a.DateExpired == null &&
                                                             (a.DateCreate ?? model.DateTo).Date <= model.DateTo)
                                                 .Select(a => a.CasePerson.FullName + " - " + a.CasePersonInheritanceResult.Label)) +
                                         (x.ActInforcedDate != null ? (Environment.NewLine +
                                              "В законна сила от " + ((DateTime)x.ActInforcedDate).ToString("dd.MM.yyyy")) : "")
                       }).AsQueryable();
        }

        private IQueryable<HeritageReportVM> HeritageReportNew_Select(int courtId, HeritageFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);

            DateTime dateFromSearch = model.DateActFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateActTo ?? DateTime.Now.AddYears(100);

            Expression<Func<CasePersonInheritance, bool>> dateSearch = x => true;
            if (model.DateActFrom != null || model.DateActTo != null)
                dateSearch = x => x.CaseSessionAct.ActDeclaredDate >= dateFromSearch.ForceStartDate() && x.CaseSessionAct.ActDeclaredDate <= dateToSearch.ForceEndDate();

            Expression<Func<CasePersonInheritance, bool>> dateCreateSearch = x => true;
            dateCreateSearch = x => x.DateCreate >= model.DateCreateFrom.ForceStartDate() && x.DateCreate <= model.DateCreateTo.ForceEndDate();

            DateTime dateInforcedFromSearch = model.FromActInforcedDate ?? DateTime.Now.AddYears(-100);
            DateTime dateInforcedToSearch = model.ToActInforcedDate ?? DateTime.Now.AddYears(100);

            Expression<Func<CasePersonInheritance, bool>> actInforcedDateSearch = x => true;
            if (model.FromActInforcedDate != null || model.ToActInforcedDate != null)
                actInforcedDateSearch = x => x.CaseSessionAct.ActInforcedDate != null && x.CaseSessionAct.ActInforcedDate >= dateInforcedFromSearch.ForceStartDate() &&
                           x.CaseSessionAct.ActInforcedDate <= dateInforcedToSearch.ForceEndDate();

            Expression<Func<CasePersonInheritance, bool>> actNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.NumberAct) == false)
                actNumberSearch = x => x.CaseSessionAct.RegNumber == model.NumberAct;

            int[] documentTypesGrouping = { NomenclatureConstants.DocumentTypeGroupings.OtherHeritage, NomenclatureConstants.DocumentTypeGroupings.RefuseHeritage,
                                              NomenclatureConstants.DocumentTypeGroupings.AcceptHeritage};
            var documentTypes = repo.AllReadonly<DocumentTypeGrouping>()
                 .Where(x => documentTypesGrouping.Contains(x.DocumentTypeGroup))
                 .ToList();

            int[] personRoles = { NomenclatureConstants.PersonRole.Inheritor, NomenclatureConstants.PersonRole.Legator };

            var notifiers = repo.AllReadonly<PersonRoleGrouping>()
                            .Where(x => x.PersonRoleGroup == NomenclatureConstants.PersonRoleGroupings.HeritageReportPersonNotifier)
                            .Select(x => x.PersonRoleId)
                            .ToArray();

            return repo.AllReadonly<CasePersonInheritance>()
                       .Where(x => x.Case.CourtId == courtId)
                       .Where(x => x.CaseSessionAct.ActDeclaredDate != null)
                       .Where(x => (documentTypes.Select(a => a.DocumentTypeId)).Contains(x.Case.Document.DocumentTypeId))
                       .Where(x => x.CaseSessionAct.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                       .Where(actNumberSearch)
                       .Where(dateSearch)
                       .Where(actInforcedDateSearch)
                       .Where(dateCreateSearch)
                       .Select(x => new HeritageReportVM
                       {
                           Index = x.RegNumberValue ?? 0,
                           ActDeclaredDate = x.CaseSessionAct.ActDeclaredDate ?? DateTime.Now, //влизат само тези с ActDeclaredDate != null
                           DocumentNumber = x.Case.Document.DocumentNumber + "/" + x.Case.Document.DocumentDate.ToString("dd.MM.yyyy") +
                                            "; " + x.Case.CaseType.Code + " " + x.Case.RegNumber + "; Съдия-докладчик " +
                                            x.CaseSessionAct.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.CaseSessionAct.CaseSession.DateFrom &&
                                                         a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                     .Select(a => a.LawUnit.FullName).FirstOrDefault(),
                           DocumentDate = x.Case.Document.DocumentDate,
                           Notifier = string.Join(Environment.NewLine,
                                      x.Case.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                                                 notifiers.Contains(a.PersonRoleId))
                                                   .Select(a => a.FullName + " (" + a.PersonRole.Label + ")")),
                           RefuseHeritage = x.CasePersonInheritanceResultId == NomenclatureConstants.CasePersonInheritanceResults.Accept ? "" :
                                        ((x.DateExpired != null ? "Изтриване " : "") + x.CasePerson.FullName + " - " + x.CasePersonInheritanceResult.Label),
                           AcceptHeritage = x.CasePersonInheritanceResultId == NomenclatureConstants.CasePersonInheritanceResults.Accept ?
                                        ((x.DateExpired != null ? "Изтриване " : "") + x.CasePerson.FullName + " - " + x.CasePersonInheritanceResult.Label) : "",
                           ResultData = string.Join(Environment.NewLine, x.Case.CasePersons
                                                                          .Where(a => a.CaseSessionId == null &&
                                                                          a.DateExpired == null &&
                                                                          a.PersonRoleId == NomenclatureConstants.PersonRole.Legator)
                                           .Select(a => a.FullName)),
                           PersonNames = string.Join(Environment.NewLine, x.Case.CasePersons
                                                                          .Where(a => a.CaseSessionId == null &&
                                                                          a.DateExpired == null &&
                                                                          personRoles.Contains(a.PersonRoleId))
                                           .Select(a => a.FullName + " (" + a.PersonRole.Label + ")")),
                           Description = x.CaseSessionAct.ActType.Label +
                                                 (x.CaseSessionAct.ActDate != null ? (Environment.NewLine + x.CaseSessionAct.RegNumber + "/" + ((DateTime)x.CaseSessionAct.ActDate).ToString("dd.MM.yyyy")) : "") +
                                                 Environment.NewLine +
                                         (x.CaseSessionAct.ActInforcedDate != null ? (Environment.NewLine +
                                              "В законна сила от " + ((DateTime)x.CaseSessionAct.ActInforcedDate).ToString("dd.MM.yyyy")) : "")
                       }).AsQueryable();
        }

        public async Task<byte[]> HeritageReportToExcelOne(HeritageFilterReportVM model)
        {
            List<HeritageReportVM> dataRows;
            dataRows = await HeritageReport_Select(userContext.CourtId, model).OrderBy(x => x.ActDeclaredDate).ToListAsync();
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

        public async Task<byte[]> HeritageReportToExcelOneNew(HeritageFilterReportVM model)
        {
            List<HeritageReportVM> dataRows;
            dataRows = (await HeritageReportNew_Select(userContext.CourtId, model).ToListAsync()).OrderBy(x => x.Index).ToList();

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

        /// <summary>
        /// Само наказателни дела
        /// </summary>
        /// <param name="model"></param>
        /// <param name="startRow"></param>
        /// <param name="getRow"></param>
        /// <param name="personRoles"></param>
        /// <returns></returns>
        private async Task<List<CaseFirstInstanceReportVM>> CaseFirstInstanceReport_Select(CaseFirstInstanceFilterReportVM model, int startRow, int getRow, int[] personRoles)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateMin = DateTime.MinValue;
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

            Expression<Func<Case, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.CaseLawUnits
                          .Where(a => (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.CaseSessionId == null &&
                          a.CourtDepartmentId == model.DepartmentId).Any();

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();


            var cases = await readonlyrepo.AllReadonly<Case>()
                               .Where(x => x.CourtId == userContext.CourtId)
                               .Where(x => x.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance)
                               .Where(x => x.RegDate.Date >= model.DateFrom.Date && x.RegDate.Date <= model.DateTo.Date)
                               .Where(numberCaseSearch)
                               .Where(groupWhere)
                               .Where(typeWhere)
                               .Where(departmentWhere)
                               .Where(judgeReporterSearch)
                               .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                               .Select(x => new CaseFirstInstanceReportVM()
                               {
                                   CaseId = x.Id,
                                   DocumentId = x.DocumentId,
                                   CourtId = x.CourtId,
                                   CaseRegNumberValue = x.ShortNumberValue ?? 0,
                                   IsNewCaseNewNumber = x.IsNewCaseNewNumber ?? false,
                                   CaseLifecycleMonths = x.CaseLifecycles.Select(a => a.DurationMonths).Sum(),
                                   RegNumber = x.CaseType.Code + ", " + x.RegNumber + "/" + x.RegDate.ToString("dd.MM.yyyy") +
                                               (x.CaseStateId == NomenclatureConstants.CaseState.Deleted ? " - Анулирано" : ""),
                                   CaseTypeId = x.CaseTypeId,
                                   IsNewNumber = x.IsNewCaseNewNumber ?? false,
                                   InputDocument = x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " - " + x.Document.DocumentType.Label,
                                   RegDate = x.RegDate,
                                   CaseCodeLabel = (x.CaseCode.Code ?? "") + " " + (x.CaseCode.Label ?? ""),
                                   CaseCodeCode = x.CaseCode.Code,
                                   CaseCodeId = x.CaseCode.Id,
                                   CasePersons = string.Join(Environment.NewLine, x.CasePersons.Where(a => a.CaseSessionId == null &&
                                                                                                           a.DateExpired == null && personRoles.Contains(a.PersonRoleId))
                                                                                               .OrderBy(p => p.RowNumber)
                                                                                               .Select(p => p.FullName + " (" + p.PersonRole.Label + ")")),
                                   ResultSentence = (model.ActDescription == true && x.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo) ? (Environment.NewLine +
                                                                string.Join(Environment.NewLine, x.CasePersonSentencePunishments
                                                                .Where(b => b.DateExpired == null)
                                                                .Select(b => b.CasePersonSentence.CasePerson.FullName + " " +
                                                                b.SentenceType.Label +
                                                                (b.SentenseMoney > 0 ? (" " + b.SentenseMoney.ToString("0.00")) : "") +
                                                                (b.SentenseYears > 0 ? (" години: " + b.SentenseYears.ToString()) : "") +
                                                                (b.SentenseMonths > 0 ? (" месеци: " + b.SentenseMonths.ToString()) : "") +
                                                                (b.SentenseWeeks > 0 ? (" седмици: " + b.SentenseWeeks.ToString()) : "") +
                                                                (b.SentenseDays > 0 ? (" дни: " + b.SentenseDays.ToString()) : "")))) : "",
                                   AcceptedCh80 = x.CaseLifecycles.Any(a => a.CaseId == x.Id && a.DateExpired == null && a.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS),
                               })
                               .OrderBy(x => x.RegDate)
                               .ThenBy(x => x.CaseId)
                               .Skip(startRow)
                               .Take(getRow)
                               .ToListAsync();

            var caseIds = cases.Select(x => x.CaseId).Distinct().ToList();
            var caseIdsNullable = cases.Select(x => (int?)x.CaseId).Distinct().ToList();
            var documentIds = cases.Select(x => x.DocumentId).Distinct().ToList();

            //Състав по делото
            var caseLawUnits = await readonlyrepo.AllReadonly<CaseLawUnit>()
                                    .Where(x => caseIds.Contains(x.CaseId) && x.CaseSessionId == null)
                                    .Select(x => new
                                    {
                                        caseId = x.CaseId,
                                        courtDepartmentId = x.CourtDepartmentId,
                                        dateTo = x.DateTo,
                                        courtDepartmentLabel = x.CourtDepartment.Label,
                                        judgeRoleId = x.JudgeRoleId,
                                        lawUnitFullName = x.LawUnit.FullName,
                                    })
                                    .ToListAsync();

            //Архивиране
            var caseArchives = await readonlyrepo.AllReadonly<CaseArchive>()
                                    .Where(x => caseIds.Contains(x.CaseId))
                                    .Select(x => new
                                    {
                                        caseId = x.CaseId,
                                        regDate = x.RegDate,
                                        regNumber = x.RegNumber,
                                        archiveLink = x.ArchiveLink,
                                    })
                                    .ToListAsync();


            //Движения по CaseId или ReturnCaseId. or-а не му пречи. Пробвал съм го
            var caseMigrations = await readonlyrepo.AllReadonly<CaseMigration>()
                                   .Where(x => (caseIds.Contains(x.CaseId) || caseIdsNullable.Contains(x.ReturnCaseId)) && x.DateExpired == null)
                                   .Select(x => new
                                   {
                                       caseId = x.CaseId,
                                       sendToCourtId = x.SendToCourtId,
                                       outDocumentId = x.OutDocumentId,
                                       outDocumentDateExpired = x.OutDocument.DateExpired,
                                       outDocumentDocumentNumber = x.OutDocument.DocumentNumber,
                                       outDocumentDocumentDate = x.OutDocumentId == null ? dateMin : x.OutDocument.DocumentDate,
                                       sendToCourtLabel = x.SendToCourt.Label,
                                       caseMigrationTypeId = x.CaseMigrationTypeId,
                                       caseMigrationTypeMigrationDirection = x.CaseMigrationType.MigrationDirection,
                                       initialCaseId = x.InitialCaseId,
                                       returnCaseId = x.ReturnCaseId,
                                   })
                                   .ToListAsync();

            //Движенията по OutCaseMigration.ReturnCaseId
            var caseOutCaseMigration = await readonlyrepo.AllReadonly<CaseMigration>()
                                   .Where(x => caseIdsNullable.Contains(x.OutCaseMigration.ReturnCaseId) && x.DateExpired == null)
                                   .Select(x => new
                                   {
                                       caseId = x.CaseId,
                                       outCaseMigrationReturnCaseId = x.OutCaseMigration.ReturnCaseId,
                                       caseCourtId = x.Case.CourtId,
                                       caseMigrationTypeMigrationDirection = x.CaseMigrationType.MigrationDirection,
                                       caseMigrationTypeId = x.CaseMigrationTypeId,
                                       caseMigrationTypeDescription = x.CaseMigrationType.Description,
                                       acts = string.Join("; ", x.Case.CaseSessionActs.Where(b => b.DateExpired == null && b.ActResultId != null).Select(b => b.ActResult.Label)),
                                       caseRegNumber = x.Case.RegNumber,
                                       caseRegDate = x.Case.RegDate,
                                   })
                                   .ToListAsync();

            //Заседания
            var caseSessions = await readonlyrepo.AllReadonly<CaseSession>()
                                    .Where(x => caseIds.Contains(x.CaseId) && x.DateExpired == null)
                                    .Select(x => new
                                    {
                                        caseId = x.CaseId,
                                        sessionTypeSessionTypeGroup = x.SessionType.SessionTypeGroup,
                                        dateFrom = x.DateFrom,
                                        sessionStateId = x.SessionStateId,
                                        hasAnnouncedForResolution = x.CaseSessionResults.Any(r => r.IsActive && r.IsMain && r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution),
                                    })
                                    .ToListAsync();

            //Актове
            var acts = await readonlyrepo.AllReadonly<CaseSessionAct>()
                               .Where(x => caseIdsNullable.Contains(x.CaseId) && x.DateExpired == null && x.CaseSession.DateExpired == null)
                               .Select(x => new
                               {
                                   caseId = x.CaseId,
                                   actStateId = x.ActStateId,
                                   isFinalDoc = x.IsFinalDoc,
                                   caseSessionSessionStateId = x.CaseSession.SessionStateId,
                                   actDeclaredDate = x.ActDeclaredDate,
                                   regDate = x.RegDate,
                                   sessionResults = string.Join("; ", x.CaseSession.CaseSessionResults.Where(c => c.IsActive && c.IsMain).Select(c => c.SessionResult.Label)),
                                   actTypeLabel = x.ActType.Label,
                                   regNumber = x.RegNumber,
                                   caseSessionSessionTypeLabel = x.CaseSession.SessionType.Label,
                                   caseSessionDateFrom = x.CaseSession.DateFrom,
                                   actComplainResultId = x.ActComplainResultId,
                                   actComplainResultLabel = x.ActComplainResult.Label,
                                   description = x.Description,
                               })
                               .ToListAsync();

            //Документи
            var documentInstitutionCaseInfos = await readonlyrepo.AllReadonly<DocumentInstitutionCaseInfo>()
                                        .Where(x => documentIds.Contains(x.DocumentId))
                                        .Select(x => new
                                        {
                                            documentId = x.DocumentId,
                                            institutionCaseTypeLabel = x.InstitutionCaseType.Label,
                                            caseNumber = x.CaseNumber,
                                            caseYear = x.CaseYear,
                                            institutionFullName = x.Institution.FullName,
                                        }).ToListAsync();

            foreach (var item in cases)
            {
                //Състав
                var oneCaseLawUnits = caseLawUnits.Where(x => x.caseId == item.CaseId).ToList();
                item.RegNumber += oneCaseLawUnits.Any(a => (a.dateTo ?? dateEnd) >= DateTime.Now && a.courtDepartmentId != null) ?
                                    oneCaseLawUnits.Where(a => (a.dateTo ?? dateEnd) >= DateTime.Now && a.courtDepartmentId != null)
                                    .Select(a => Environment.NewLine + a.courtDepartmentLabel)
                                    .FirstOrDefault() : string.Empty;

                item.JudgeReporterName = oneCaseLawUnits.Where(a => a.judgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter && a.dateTo == null)
                                                        .Select(a => a.lawUnitFullName)
                                                        .FirstOrDefault();


                //Движения
                var oneCaseMigration = caseMigrations.Where(x => x.caseId == item.CaseId).ToList();
                item.AcceptJurisdiction = item.IsNewCaseNewNumber ? false : oneCaseMigration.Any(a => a.caseId == item.CaseId && a.caseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction);
                item.AcceptDiffJurisdiction = item.IsNewCaseNewNumber ? false : oneCaseMigration.Any(a => a.caseId == item.CaseId && a.initialCaseId != item.CaseId &&
                                                                                                     a.caseMigrationTypeId != NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction &&
                                                                                                     a.caseMigrationTypeMigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming);

                item.SendOtherInstance = string.Join(Environment.NewLine, oneCaseMigration.Where(a => a.caseId == item.CaseId &&
                                                                                                   a.sendToCourtId != null &&
                                                                                                   a.outDocumentId != null &&
                                                                                                   a.outDocumentDateExpired == null)
                                                                                       .Select(a => a.outDocumentDocumentNumber + "/" + a.outDocumentDocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                                                               a.sendToCourtLabel));
                item.ReceiveOtherInstance = string.Join(Environment.NewLine, oneCaseMigration.Where(a => a.returnCaseId == item.CaseId &&
                                                                                                                               a.sendToCourtId == item.CourtId &&
                                                                                                                               a.outDocumentId != null)
                                                                                                                   .Select(a => a.outDocumentDocumentNumber + "/" +
                                                                                                                                a.outDocumentDocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                                                                                                a.sendToCourtLabel));

                //Движения
                var oneCaseOutCaseMigration = caseOutCaseMigration.Where(x => x.caseId == item.CaseId).ToList();
                item.ResultOtherInstance = string.Join(Environment.NewLine, oneCaseOutCaseMigration.Where(a => a.outCaseMigrationReturnCaseId == item.CaseId && a.caseCourtId == item.CourtId &&
                                                                                                          a.caseMigrationTypeMigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                                                                         .Select(a => (a.caseMigrationTypeId != NomenclatureConstants.CaseMigrationTypes.AcceptCase_AfterComplain ?
                                                                                                    (a.caseMigrationTypeDescription ?? "") : a.acts) +
                                                                                                    (a.caseId != item.CaseId ? (Environment.NewLine + a.caseRegNumber + "/" + a.caseRegDate.ToString("dd.MM.yyyy")) : "")));


                //Заседания
                var oneCaseSessions = caseSessions.Where(x => x.caseId == item.CaseId).ToList();
                item.SessionDates = string.Join(Environment.NewLine, oneCaseSessions.Where(a => a.sessionTypeSessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
                         .OrderBy(a => a.dateFrom)
                         .Select(a => a.dateFrom.ToString("dd.MM.yyyy HH:mm")));

                item.SlovingDate = string.Join(Environment.NewLine, oneCaseSessions.Where(a => a.sessionStateId == NomenclatureConstants.SessionState.Provedeno && a.hasAnnouncedForResolution == true)
                                                                             .Select(a => a.dateFrom.ToString("dd.MM.yyyy")));


                //Актове
                var oneActs = acts.Where(x => x.caseId == item.CaseId).ToList();
                item.FinalAct = oneActs.Where(a => NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.actStateId) &&
                                           a.isFinalDoc == true && a.caseSessionSessionStateId == NomenclatureConstants.SessionState.Provedeno)
                                    .OrderByDescending(b => b.actDeclaredDate)
                                    .Select(b => b.actDeclaredDate)
                                    .FirstOrDefault();

                item.Result = oneActs.Where(a => NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.actStateId) &&
                                                      a.isFinalDoc == true && a.regDate != null && a.caseSessionSessionStateId == NomenclatureConstants.SessionState.Provedeno)
                                          .OrderByDescending(b => b.actDeclaredDate)
                                          .Select(a => a.sessionResults +
                                                                         Environment.NewLine + a.actTypeLabel + " " + (a.regNumber ?? "") + "/" +
                                                                         ((DateTime)a.regDate).ToString(FormattingConstant.NormalDateFormat) +
                                                                         Environment.NewLine +
                                                                         a.caseSessionSessionTypeLabel + " " +
                                                                         a.caseSessionDateFrom.ToString("dd.MM.yyyy") +
                                                                         (a.actComplainResultId != null ? (Environment.NewLine + "резултат/степен на уважаване на иска: " + a.actComplainResultLabel) : "") +
                                                                         (model.ActDescription == true ? (Environment.NewLine + a.description) : ""))
                                          .FirstOrDefault() + item.ResultSentence;

                //Архивиране
                var oneCaseArchives = caseArchives.Where(x => x.caseId == item.CaseId).ToList();
                item.DateArch = oneCaseArchives.Select(a => (DateTime?)a.regDate).FirstOrDefault();
                item.NumArch = oneCaseArchives.Select(a => a.regNumber).FirstOrDefault();
                item.NumLinkArch = oneCaseArchives.Select(a => a.archiveLink).FirstOrDefault();

                var oneCaseInstitutions = documentInstitutionCaseInfos.Where(x => x.documentId == item.DocumentId).ToList();
                item.CaseInstitution = string.Join(Environment.NewLine, oneCaseInstitutions.Select(a => a.institutionCaseTypeLabel + " " + a.caseNumber + "/" +
                                                                                     a.caseYear.ToString() + " " + a.institutionFullName));
            }

            return cases;
        }

        private async Task<List<CaseFirstInstanceReportVM>> CaseFirstInstanceReportAll_Select(CaseFirstInstanceFilterReportVM model)
        {
            List<CaseFirstInstanceReportVM> dataRows = new List<CaseFirstInstanceReportVM>();
            List<CaseFirstInstanceReportVM> getRows = new List<CaseFirstInstanceReportVM>();

            var personRoles = await readonlyrepo.AllReadonly<PersonRoleGrouping>()
                                          .Where(x => x.PersonRoleGroup == NomenclatureConstants.PersonRoleGroupings.CaseFirstInstanceReportPersonRole)
                                          .Select(x => x.PersonRoleId)
                                          .ToArrayAsync();

            int start = 0;
            int take = 100;
            getRows = await CaseFirstInstanceReport_Select(model, start, take, personRoles).ConfigureAwait(false);
            dataRows.AddRange(getRows);

            while (getRows.Any())
            {
                start = start + take;
                getRows = await CaseFirstInstanceReport_Select(model, start, take, personRoles).ConfigureAwait(false);
                dataRows.AddRange(getRows);
            }

            return dataRows;
        }


        private async Task<List<CaseFirstInstanceReportVM>> CaseFirstInstanceReportForExcel_Select(CaseFirstInstanceFilterReportVM model, int startRow, int getRow)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateMin = DateTime.MinValue;
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

            Expression<Func<Case, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.CaseLawUnits
                          .Where(a => (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.CaseSessionId == null &&
                          a.CourtDepartmentId == model.DepartmentId).Any();

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();


            var personRolesQuery = readonlyrepo.AllReadonly<PersonRoleGrouping>()
                                               .Where(x => x.PersonRoleGroup == NomenclatureConstants.PersonRoleGroupings.CaseFirstInstanceReportPersonRole)
                                               .Select(x => x.PersonRoleId);

            var cases = await readonlyrepo.AllReadonly<Case>()
                                     .Where(x => x.CourtId == userContext.CourtId)
                                     .Where(x => x.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance)
                                     .Where(x => x.RegDate.Date >= model.DateFrom.Date && x.RegDate.Date <= model.DateTo.Date)
                                     .Where(numberCaseSearch)
                                     .Where(groupWhere)
                                     .Where(typeWhere)
                                     .Where(departmentWhere)
                                     .Where(judgeReporterSearch)
                                     .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                                     .Select(x => new CaseFirstInstanceReportVM()
                                     {
                                         CaseId = x.Id,
                                         CourtId = x.CourtId,
                                         CaseRegNumberValue = x.ShortNumberValue ?? 0,
                                         IsNewCaseNewNumber = x.IsNewCaseNewNumber ?? false,
                                         CaseLifecycleMonths = x.CaseLifecycles.Select(a => a.DurationMonths).Sum(),
                                         RegNumber = x.CaseType.Code + ", " + x.RegNumber +
                                                     (x.CaseStateId == NomenclatureConstants.CaseState.Deleted ? " - Анулирано" : ""),
                                         CaseTypeId = x.CaseTypeId,
                                         IsNewNumber = x.IsNewCaseNewNumber ?? false,
                                         InputDocument = x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " - " + x.Document.DocumentType.Label,
                                         RegDate = x.RegDate,
                                         CaseCodeLabel = (x.CaseCode.Code ?? "") + " " + (x.CaseCode.Label ?? ""),
                                         CaseCodeCode = x.CaseCode.Code,
                                         CaseCodeId = x.CaseCode.Id,
                                         CasePersons = string.Join(Environment.NewLine, x.CasePersons.Where(a => a.CaseSessionId == null &&
                                                                                                                 a.DateExpired == null && personRolesQuery.Contains(a.PersonRoleId))
                                                                                                     .OrderBy(p => p.RowNumber)
                                                                                                     .Select(p => p.FullName + " (" + p.PersonRole.Label + ")")),
                                         AcceptedCh80 = x.CaseLifecycles.Any(a => a.CaseId == x.Id && a.DateExpired == null && a.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS),
                                     })
                                     .OrderBy(x => x.RegDate)
                                     .ThenBy(x => x.CaseId)
                                     .Skip(startRow)
                                     .Take(getRow)
                                     .ToListAsync();

            var caseIds = cases.Select(x => x.CaseId).Distinct().ToList();
            var caseIdsNullable = cases.Select(x => (int?)x.CaseId).Distinct().ToList();

            //Състав по делото
            var caseLawUnits = await readonlyrepo.AllReadonly<CaseLawUnit>()
                                    .Where(x => caseIds.Contains(x.CaseId) && x.CaseSessionId == null)
                                    .Select(x => new
                                    {
                                        caseId = x.CaseId,
                                        courtDepartmentId = x.CourtDepartmentId,
                                        dateTo = x.DateTo,
                                        courtDepartmentLabel = x.CourtDepartment.Label,
                                        judgeRoleId = x.JudgeRoleId,
                                        lawUnitFullName = x.LawUnit.FullName,
                                    })
                                    .ToListAsync();

            //Архивиране
            var caseArchives = await readonlyrepo.AllReadonly<CaseArchive>()
                                    .Where(x => caseIds.Contains(x.CaseId))
                                    .Select(x => new
                                    {
                                        caseId = x.CaseId,
                                        regDate = x.RegDate,
                                        regNumber = x.RegNumber,
                                        archiveLink = x.ArchiveLink,
                                    })
                                    .ToListAsync();


            //Движения по CaseId или ReturnCaseId. or-а не му пречи. Пробвал съм го
            var caseMigrations = await readonlyrepo.AllReadonly<CaseMigration>()
                                   .Where(x => (caseIds.Contains(x.CaseId) || caseIdsNullable.Contains(x.ReturnCaseId)) && x.DateExpired == null)
                                   .Select(x => new
                                   {
                                       caseId = x.CaseId,
                                       sendToCourtId = x.SendToCourtId,
                                       outDocumentId = x.OutDocumentId,
                                       outDocumentDateExpired = x.OutDocument.DateExpired,
                                       outDocumentDocumentNumber = x.OutDocument.DocumentNumber,
                                       outDocumentDocumentDate = x.OutDocumentId == null ? dateMin : x.OutDocument.DocumentDate,
                                       sendToCourtLabel = x.SendToCourt.Label,
                                       caseMigrationTypeId = x.CaseMigrationTypeId,
                                       caseMigrationTypeMigrationDirection = x.CaseMigrationType.MigrationDirection,
                                       initialCaseId = x.InitialCaseId,
                                       returnCaseId = x.ReturnCaseId,
                                   })
                                   .ToListAsync();

            //Движенията по OutCaseMigration.ReturnCaseId
            var caseOutCaseMigration = await readonlyrepo.AllReadonly<CaseMigration>()
                                   .Where(x => caseIdsNullable.Contains(x.OutCaseMigration.ReturnCaseId) && x.DateExpired == null)
                                   .Select(x => new
                                   {
                                       caseId = x.CaseId,
                                       outCaseMigrationReturnCaseId = x.OutCaseMigration.ReturnCaseId,
                                       caseCourtId = x.Case.CourtId,
                                       caseMigrationTypeMigrationDirection = x.CaseMigrationType.MigrationDirection,
                                       caseMigrationTypeId = x.CaseMigrationTypeId,
                                       caseMigrationTypeDescription = x.CaseMigrationType.Description,
                                       acts = string.Join("; ", x.Case.CaseSessionActs.Where(b => b.DateExpired == null && b.ActResultId != null).Select(b => b.ActResult.Label)),
                                       caseRegNumber = x.Case.RegNumber,
                                       caseRegDate = x.Case.RegDate,
                                   })
                                   .ToListAsync();

            //Заседания
            var caseSessions = await readonlyrepo.AllReadonly<CaseSession>()
                                    .Where(x => caseIds.Contains(x.CaseId) && x.DateExpired == null)
                                    .Select(x => new
                                    {
                                        caseId = x.CaseId,
                                        sessionTypeSessionTypeGroup = x.SessionType.SessionTypeGroup,
                                        dateFrom = x.DateFrom,
                                        sessionStateId = x.SessionStateId,
                                        hasAnnouncedForResolution = x.CaseSessionResults.Any(r => r.IsActive && r.IsMain && r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution),
                                    })
                                    .ToListAsync();

            //Актове
            var acts = await readonlyrepo.AllReadonly<CaseSessionAct>()
                               .Where(x => caseIdsNullable.Contains(x.CaseId) && x.DateExpired == null && x.CaseSession.DateExpired == null)
                               .Select(x => new
                               {
                                   caseId = x.CaseId,
                                   actStateId = x.ActStateId,
                                   isFinalDoc = x.IsFinalDoc,
                                   caseSessionSessionStateId = x.CaseSession.SessionStateId,
                                   actDeclaredDate = x.ActDeclaredDate,
                                   regDate = x.RegDate,
                                   sessionResults = string.Join("; ", x.CaseSession.CaseSessionResults.Where(c => c.IsActive && c.IsMain).Select(c => c.SessionResult.Label)),
                                   actTypeLabel = x.ActType.Label,
                                   regNumber = x.RegNumber,
                                   caseSessionSessionTypeLabel = x.CaseSession.SessionType.Label,
                                   caseSessionDateFrom = x.CaseSession.DateFrom,
                                   actComplainResultId = x.ActComplainResultId,
                                   actComplainResultLabel = x.ActComplainResult.Label,
                                   description = x.Description,
                               })
                               .ToListAsync();


            foreach (var item in cases)
            {
                //Състав
                var oneCaseLawUnits = caseLawUnits.Where(x => x.caseId == item.CaseId).ToList();
                item.RegNumber += oneCaseLawUnits.Any(a => (a.dateTo ?? dateEnd) >= DateTime.Now && a.courtDepartmentId != null) ?
                                    oneCaseLawUnits.Where(a => (a.dateTo ?? dateEnd) >= DateTime.Now && a.courtDepartmentId != null)
                                    .Select(a => Environment.NewLine + a.courtDepartmentLabel)
                                    .FirstOrDefault() : string.Empty;

                item.JudgeReporterName = oneCaseLawUnits.Where(a => a.judgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter && a.dateTo == null)
                                                        .Select(a => a.lawUnitFullName)
                                                        .FirstOrDefault();


                //Движения
                var oneCaseMigration = caseMigrations.Where(x => x.caseId == item.CaseId).ToList();
                item.AcceptJurisdiction = item.IsNewCaseNewNumber ? false : oneCaseMigration.Any(a => a.caseId == item.CaseId && a.caseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction);
                item.AcceptDiffJurisdiction = item.IsNewCaseNewNumber ? false : oneCaseMigration.Any(a => a.caseId == item.CaseId && a.initialCaseId != item.CaseId &&
                                                                                                     a.caseMigrationTypeId != NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction &&
                                                                                                     a.caseMigrationTypeMigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming);

                item.SendOtherInstance = string.Join(Environment.NewLine, oneCaseMigration.Where(a => a.caseId == item.CaseId &&
                                                                                                   a.sendToCourtId != null &&
                                                                                                   a.outDocumentId != null &&
                                                                                                   a.outDocumentDateExpired == null)
                                                                                       .Select(a => a.outDocumentDocumentNumber + "/" + a.outDocumentDocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                                                               a.sendToCourtLabel));
                item.ReceiveOtherInstance = string.Join(Environment.NewLine, oneCaseMigration.Where(a => a.returnCaseId == item.CaseId &&
                                                                                                                               a.sendToCourtId == item.CourtId &&
                                                                                                                               a.outDocumentId != null)
                                                                                                                   .Select(a => a.outDocumentDocumentNumber + "/" +
                                                                                                                                a.outDocumentDocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                                                                                                a.sendToCourtLabel));

                //Движения
                var oneCaseOutCaseMigration = caseOutCaseMigration.Where(x => x.caseId == item.CaseId).ToList();
                item.ResultOtherInstance = string.Join(Environment.NewLine, oneCaseOutCaseMigration.Where(a => a.outCaseMigrationReturnCaseId == item.CaseId && a.caseCourtId == item.CourtId &&
                                                                                                          a.caseMigrationTypeMigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                                                                         .Select(a => (a.caseMigrationTypeId != NomenclatureConstants.CaseMigrationTypes.AcceptCase_AfterComplain ?
                                                                                                    (a.caseMigrationTypeDescription ?? "") : a.acts) +
                                                                                                    (a.caseId != item.CaseId ? (Environment.NewLine + a.caseRegNumber + "/" + a.caseRegDate.ToString("dd.MM.yyyy")) : "")));


                //Заседания
                var oneCaseSessions = caseSessions.Where(x => x.caseId == item.CaseId).ToList();
                item.SlovingDate = string.Join(Environment.NewLine, oneCaseSessions.Where(a => a.sessionStateId == NomenclatureConstants.SessionState.Provedeno && a.hasAnnouncedForResolution == true)
                                                                             .Select(a => a.dateFrom.ToString("dd.MM.yyyy")));


                //Актове
                var oneActs = acts.Where(x => x.caseId == item.CaseId).ToList();
                item.FinalAct = oneActs.Where(a => NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.actStateId) &&
                                           a.isFinalDoc == true && a.caseSessionSessionStateId == NomenclatureConstants.SessionState.Provedeno)
                                    .OrderByDescending(b => b.actDeclaredDate)
                                    .Select(b => b.actDeclaredDate)
                                    .FirstOrDefault();

                item.Result = oneActs.Where(a => NomenclatureConstants.SessionActState.EnforcedStates.Contains(a.actStateId) &&
                                                      a.isFinalDoc == true && a.regDate != null && a.caseSessionSessionStateId == NomenclatureConstants.SessionState.Provedeno)
                                          .OrderByDescending(b => b.actDeclaredDate)
                                          .Select(a => a.sessionResults +
                                                                         Environment.NewLine + a.actTypeLabel + " " + (a.regNumber ?? "") + "/" +
                                                                         ((DateTime)a.regDate).ToString(FormattingConstant.NormalDateFormat) +
                                                                         Environment.NewLine +
                                                                         a.caseSessionSessionTypeLabel + " " +
                                                                         a.caseSessionDateFrom.ToString("dd.MM.yyyy") +
                                                                         (a.actComplainResultId != null ? (Environment.NewLine + "резултат/степен на уважаване на иска: " + a.actComplainResultLabel) : "") +
                                                                         (model.ActDescription == true ? (Environment.NewLine + a.description) : ""))
                                          .FirstOrDefault();

                //Архивиране
                var oneCaseArchives = caseArchives.Where(x => x.caseId == item.CaseId).ToList();
                item.DateArch = oneCaseArchives.Select(a => (DateTime?)a.regDate).FirstOrDefault();
                item.NumArch = oneCaseArchives.Select(a => a.regNumber).FirstOrDefault();
                item.NumLinkArch = oneCaseArchives.Select(a => a.archiveLink).FirstOrDefault();

            }

            return cases;
        }

        private void CaseFirstInstanceAddTextCountToExcel(NPoiExcelService excelService, string text, int count)
        {
            AddTextCountToExcel(excelService, text, count, 3);
        }

        public async Task<byte[]> CaseFirstInstanceReportToExcelOne(CaseFirstInstanceFilterReportVM model)
        {
            if (model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                return await CaseFirstInstanceCriminalReportToExcelOne(model);
            }
            else
            {
                return await CaseFirstInstanceDifferentCriminalReportToExcelOne(model).ConfigureAwait(false);
            }
        }

        private async Task<byte[]> CaseFirstInstanceCriminalReportToExcelOne(CaseFirstInstanceFilterReportVM model)
        {
            var dataRows = await CaseFirstInstanceReportAll_Select(model);
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
                    x => x.CasePersons,
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

        private async Task<byte[]> CaseFirstInstanceDifferentCriminalReportToExcelOne(CaseFirstInstanceFilterReportVM model)
        {
            //var dataRows = CaseFirstInstanceReport_Select(userContext.CourtId, model).OrderBy(x => x.CaseRegNumberValue).ToList();

            HtmlTemplate htmlTemplate = GetHtmlTemplate("FInstanceO");
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            excelService.rowIndex = (htmlTemplate.XlsTitleRow ?? 0) - 1;
            excelService.SetCellData(CaseGroupCaption_Title(model.CaseGroupId));

            excelService.colIndex = 0;
            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;

            List<CaseFirstInstanceReportVM> dataRows = new List<CaseFirstInstanceReportVM>();
            List<CaseFirstInstanceReportVM> getRows = new List<CaseFirstInstanceReportVM>();
            int start = 0;
            int take = 100;
            getRows = await CaseFirstInstanceReportForExcel_Select(model, start, take).ConfigureAwait(false);
            dataRows.AddRange(getRows);

            while (getRows.Any())
            {
                start = start + take;
                getRows = await CaseFirstInstanceReportForExcel_Select(model, start, take).ConfigureAwait(false);
                dataRows.AddRange(getRows);
            }

            excelService.InsertList(dataRows, new List<Expression<Func<CaseFirstInstanceReportVM, object>>>()
                                              {
                                                  x => x.RegNumber,
                                                  x => x.InputDocumentText,
                                                  x => x.RegDate,
                                                  x => x.JudgeReporterName,
                                                  x => x.CaseCodeLabel,
                                                  x => x.CaseCodeCode,
                                                  x => x.CasePersons,
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
                                              });

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

            return repo.AllReadonly<CaseMigration>()
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
                           CaseData = x.Case.CaseStateId == NomenclatureConstants.CaseState.Rejected ? "" :
                                 (x.Case.Document.DocumentNumber + "/" + x.Case.Document.DocumentDate.ToString("dd.MM.yyyy") + " - " +
                                      x.Case.Document.DocumentType.Label + "; " + x.Case.CaseType.Code + " " + x.Case.RegNumber),
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
                           ActReturnDescription = x.Case.CaseStateId == NomenclatureConstants.CaseState.Rejected ? x.Case.CaseStateDescription :
                                                    (string.Join(Environment.NewLine, x.Case.CaseSessions.Where(a => a.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
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
                                                    )
                                                              ,
                           OutDocumentData = x.OutDocument.DocumentGroup.Label + " " + x.OutDocument.DocumentNumber + "/" + x.OutDocument.DocumentDate.ToString("dd.MM.yyyy") + Environment.NewLine +
                                             x.Case.RegNumber + " " + x.Case.CaseGroup.Code + Environment.NewLine +
                                             string.Join(Environment.NewLine, x.OutDocument.DocumentPersons.Select(a => a.FullName)),
                       })
                       .AsQueryable();
        }

        private IQueryable<CaseMigrationReturnReportVM> CaseMigrationReturnReportPrev_Select(int courtId, CaseMigrationReturnFilterReportVM model)
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

            return repo.AllReadonly<CaseMigration>()
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
                           NewCaseDatas = repo.AllReadonly<CaseMigration>().Where(a => a.InitialCaseId == x.InitialCaseId &&
                                                a.CaseId != x.InitialCaseId && a.Case.CourtId == x.Case.CourtId && a.Id > x.Id)
                                               .Select(a => "Вх.№ " + a.Case.Document.DocumentNumber + "/" + a.Case.Document.DocumentDate.ToString("dd.MM.yyyy") +
                                               " - " + a.Case.Document.DocumentType.Label + ";" + Environment.NewLine +
                                               (a.Case.RegNumber ?? "") + " " + (a.Case.CaseType.Code ?? ""))
                                               .ToArray(),
                       })
                       .AsQueryable();
        }

        public async Task<byte[]> CaseMigrationReturnReportToExcelOne(CaseMigrationReturnFilterReportVM model)
        {
            var dataRows = await CaseMigrationReturnReport_Select(userContext.CourtId, model).ToListAsync();
            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Index = i + 1;
            }

            NPoiExcelService excelService = GetExcelHtmlTemplate("CaseMigrationNew");
            excelService.InsertList(
                dataRows,
                new List<Expression<Func<CaseMigrationReturnReportVM, object>>>()
                {
                    x => x.Index,
                    x => x.ActReturnData,
                    x => x.CaseData,
                    x => x.OutDocumentData,
                    x => x.ActReturnDescription,
                }
            );
            return excelService.ToArray();
        }

        public async Task<byte[]> CaseMigrationReturnReportToExcelOnePrev(CaseMigrationReturnFilterReportVM model)
        {
            var dataRows = await CaseMigrationReturnReportPrev_Select(userContext.CourtId, model).ToListAsync();
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
            return readonlyrepo.AllReadonly<CaseArchive>()
                       .Where(x => x.Case.CourtId == courtId)
                       .Where(groupWhere)
                       .Where(dateSearch)
                       .Where(yearSearch)
                       .Select(x => new CaseArchiveReportVM()
                       {
                           CaseData = x.Case.RegNumber + "; " + x.Case.CaseType.Code +
                           x.CaseSessionAct.CaseSession.CaseLawUnits.Where(a => (a.DateTo == null ? dateEnd : a.DateTo) >= a.CaseSession.DateFrom &&
                                                                      a.CourtDepartmentId != null)
                                                                     .Select(a => Environment.NewLine + a.CourtDepartment.Label)
                                                                     .FirstOrDefault(),
                           RegNumber = x.RegNumber,
                           ArchiveDate = x.RegDate,
                           ArchiveLink = x.ArchiveLink,
                           ActData = (string.IsNullOrEmpty(x.ActDestroyLabel) == false ? x.ActDestroyLabel + Environment.NewLine : "") +
                                     (string.IsNullOrEmpty(x.Description) == false ? x.Description + Environment.NewLine : "") +
                                     (x.DescriptionInfoDestroy ?? ""),
                           Description = x.Description,
                           BookNumber = x.BookNumber,
                           BookYear = x.BookYear,
                           ArchiveIndex = (string.IsNullOrEmpty(x.CourtArchiveIndex.Code) == true ? "" : (x.CourtArchiveIndex.Code + " - ")) + x.CourtArchiveIndex.Label,
                           DescriptionInfo = x.DescriptionInfo,
                           CasePersonNames = model.WithPerson == false ? "" : string.Join(Environment.NewLine,
                                              x.Case.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                                                              (a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.LeftSide ||
                                                              a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide)
                                                                      )
                                                                .Select(p => p.FullName)),
                       })
                       .AsQueryable();
        }

        public async Task<byte[]> CaseArchiveReportToExcelOne(CaseArchiveFilterReportVM model)
        {
            var dataRows = await CaseArchiveReport_Select(userContext.CourtId, model).OrderBy(x => x.ArchiveDate).ToListAsync();

            NPoiExcelService excelService = null;
            if (model.WithPerson == false)
            {
                excelService = GetExcelHtmlTemplate("CaseArchiveNew");
                excelService.InsertList(
                    dataRows,
                    new List<Expression<Func<CaseArchiveReportVM, object>>>()
                    {
                    x => x.RegNumber,
                    x => x.CaseData,
                    x => x.ArchiveDate,
                    x => x.ArchiveLink,
                    x => x.ActData,
                    x => x.Description,
                    x => x.BookData,
                    x => x.DescriptionInfo,
                    }
                );
            }
            else
            {
                excelService = GetExcelHtmlTemplate("CaseArchivePersonNew");
                excelService.InsertList(
                    dataRows,
                    new List<Expression<Func<CaseArchiveReportVM, object>>>()
                    {
                    x => x.RegNumber,
                    x => x.CaseData,
                    x => x.ArchiveDate,
                    x => x.ArchiveLink,
                    x => x.ActData,
                    x => x.Description,
                    x => x.BookData,
                    x => x.DescriptionInfo,
                    x => x.CasePersonNames,
                    }
                );
            }

            return excelService.ToArray();
        }

        public async Task<byte[]> CaseArchiveReportToExcelOnePrev(CaseArchiveFilterReportVM model)
        {
            var dataRows = (await CaseArchiveReport_Select(userContext.CourtId, model).ToListAsync()).OrderBy(x => x.ArchiveDate).ToList();
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
                regNumberSearch = x => x.CaseSessionAct.Case.RegNumber.Contains(model.CaseNumber);

            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CaseSessionActDivorce, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate.Date >= dateFromSearch.Date && x.RegDate.Date <= dateToSearch.Date;

            return repo.AllReadonly<CaseSessionActDivorce>()
                       .Where(x => x.CaseSessionAct.Case.CourtId == courtId)
                       .Where(regNumberSearch)
                       .Where(dateSearch)
                       .Select(x => new DivorceReportVM()
                       {
                           DivorceRegDate = x.RegDate.ToString("dd.MM.yyyy") + (x.DateExpired != null ? " - изтрит" : ""),
                           OutDocumentData = x.OutDocument.DocumentNumber + "/" + x.OutDocument.DocumentDate.ToString("dd.MM.yyyy"),
                           CaseData = x.CaseSessionAct.Case.CaseType.Code + "; " + x.CaseSessionAct.Case.RegNumber,
                           SessionActData = x.CaseSessionAct.RegNumber +
                                 (x.CaseSessionAct.RegDate != null ? ("/" + ((DateTime)x.CaseSessionAct.RegDate).ToString("dd.MM.yyyy")) : ""),
                           CaseSessionActInforcedDate = x.CaseSessionAct.ActInforcedDate
                       })
                       .AsQueryable();
        }

        public async Task<byte[]> DivorceReportToExcelOne(DivorceFilterReportVM model)
        {
            var dataRows = await DivorceReport_Select(userContext.CourtId, model).ToListAsync();
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
            DateTime dateEnd = DateTime.Now.AddYears(100);
            int fromNumberSearch = model.FromNumber == null ? 1 : (int)model.FromNumber;
            int toNumberSearch = model.ToNumber == null ? int.MaxValue : (int)model.ToNumber;
            int courtType = userContext.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt ? NomenclatureConstants.CourtType.RegionalCourt : NomenclatureConstants.CourtType.DistrictCourt;

            Expression<Func<Case, bool>> numberCaseSearch = x => true;
            if (model.FromNumber != null || model.ToNumber != null)
                numberCaseSearch = x => x.ShortNumberValue >= fromNumberSearch && x.ShortNumberValue <= toNumberSearch;

            Expression<Func<Case, bool>> groupWhere = x => true;
            if (model.CaseGroupId > 0)
                groupWhere = x => x.CaseGroupId == model.CaseGroupId;

            Expression<Func<Case, bool>> typeWhere = x => true;
            if (model.CaseTypeId > 0)
                typeWhere = x => x.CaseTypeId == model.CaseTypeId;

            var resultStop = readonlyrepo.AllReadonly<SessionResultGrouping>()
                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseSecondInstanceStop)
                .Select(x => x.SessionResultId)
                .ToList();

            Expression<Func<Case, bool>> departmentWhere = x => true;
            if (model.DepartmentId > 0)
                departmentWhere = x => x.CaseLawUnits
                          .Where(a => (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.CaseSessionId == null &&
                          a.CourtDepartmentId == model.DepartmentId).Any();

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= DateTime.Now.Date && a.LawUnitId == model.JudgeReporterId &&
                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            int[] instances = { NomenclatureConstants.CaseInstanceType.SecondInstance, NomenclatureConstants.CaseInstanceType.ThirdInstance };
            return readonlyrepo.AllReadonly<Case>()
                       .Where(x => x.CourtId == courtId)
                       .Where(x => instances.Contains(x.CaseType.CaseInstanceId))
                       .Where(x => x.RegDate.Date >= model.DateFrom.Date && x.RegDate.Date <= model.DateTo.Date)
                       .Where(numberCaseSearch)
                       .Where(groupWhere)
                       .Where(typeWhere)
                       .Where(departmentWhere)
                       .Where(judgeReporterSearch)
                       .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Select(x => new CaseSecondInstanceReportVM()
                       {
                           CaseRegNumberValue = x.ShortNumberValue ?? 0,
                           CaseLifecycleMonths = x.CaseLifecycles.Select(a => a.DurationMonths).Sum(),
                           CaseTypeId = x.CaseTypeId,
                           RegNumber = x.CaseType.Code + ", " + x.RegNumber + (x.CaseStateId == NomenclatureConstants.CaseState.Deleted ? " - Анулирано" : "") +
                                        x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= DateTime.Now && a.CourtDepartmentId != null && a.CaseSessionId == null)
                                                                     .Select(a => Environment.NewLine + a.CourtDepartment.Label)
                                                                     .FirstOrDefault(),
                           RegDate = x.RegDate,
                           DocumentTypeId = model.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo ? 0 : x.Document.DocumentTypeId,
                           OldLinkNumber = x.Document.DocumentCaseInfo.Where(a => a.CaseId == null)
                                              .Select(a => a.CaseRegNumber + " - " + a.Court.Label)
                                              .FirstOrDefault(),
                           MigrationLinkNumber = readonlyrepo.AllReadonly<CaseMigration>()
                                               .Where(a => a.CaseId == x.Id && a.DateExpired == null)
                                               .Where(a => a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                           a.PriorCase.Court.CourtTypeId == courtType)
                                               .Select(a => a.PriorCase.RegNumber + " - " + a.PriorCase.Court.Label)
                                               .FirstOrDefault(),
                           CaseCodeLabel = (x.CaseCode.Code ?? "") + " " + (x.CaseCode.Label ?? ""),
                           CaseCodeCode = x.CaseCode.Code,
                           CaseCodeId = x.CaseCode.Id,
                           SessionDates = model.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo ? "" :
                                         string.Join(Environment.NewLine, x.CaseSessions.Where(a => a.DateExpired == null &&
                                                   a.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
                                                   .OrderBy(a => a.DateFrom)
                                                   .Select(a => a.DateFrom.ToString("dd.MM.yyyy HH:mm"))),
                           CasePersonLeft = string.Join(Environment.NewLine, x.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                                                                                       a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.LeftSide)
                                                                                            .OrderBy(a => a.RowNumber)
                                                                                            .Select(p => p.FullName)),
                           CasePersonRight = string.Join(Environment.NewLine, x.CasePersons.Where(a => a.CaseSessionId == null && a.DateExpired == null &&
                                                                                       a.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide)
                                                                                            .OrderBy(a => a.RowNumber)
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
                                                          .Select(a => a.LawUnit.FullName).FirstOrDefault(),
                           SendOtherInstance = string.Join(Environment.NewLine, readonlyrepo.AllReadonly<CaseMigration>().Where(a => a.CaseId == x.Id &&
                                                 a.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel &&
                                                 a.SendToCourtId != null && a.OutDocumentId != null)
                                               .Select(a => a.OutDocument.DocumentNumber + "/" + a.OutDocument.DocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                       a.SendToCourt.Label)),
                           ReceiveOtherInstance = string.Join(Environment.NewLine, readonlyrepo.AllReadonly<CaseMigration>()
                                               .Where(a => a.ReturnCaseId == x.Id && a.OutDocumentId != null &&
                                               NomenclatureConstants.CaseMigrationTypes.ReturnCaseTypes.Contains(a.CaseMigrationTypeId))
                                               .Select(a => a.OutDocument.DocumentNumber + "/" + a.OutDocument.DocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                       a.SendToCourt.Label)),
                           ActResultOtherInstanceIds = string.Join(",", x.CaseSessions.Where(a => a.DateExpired == null)
                                               .Select(a => string.Join(",", a.CaseSessionActs.Where(b => b.DateExpired == null && b.ActComplainResultId != null)
                                                                                    .Select(b => b.ActComplainResultId))
                                                      )),
                           CaseReturnDate = string.Join(Environment.NewLine, readonlyrepo.AllReadonly<CaseMigration>().Where(a => a.CaseId == x.Id &&
                                                 NomenclatureConstants.CaseMigrationTypes.ReturnCaseTypes.Contains(a.CaseMigrationTypeId) &&
                                                 a.SendToCourtId != null && a.OutDocumentId != null)
                                               .Select(a => "изх. № " + a.OutDocument.DocumentNumber + "/" + a.OutDocument.DocumentDate.ToString("dd.MM.yyyy") + " - " +
                                                       a.SendToCourt.Label)),
                       }).AsQueryable();
        }

        private async Task<byte[]> CaseSecondInstanceDifferentCriminalReportToExcelOne(CaseSecondInstanceFilterReportVM model)
        {
            var dataRows = (await CaseSecondInstanceReport_Select(userContext.CourtId, model).ToListAsync()).OrderBy(x => x.CaseRegNumberValue).ToList();

            var complainResults = await readonlyrepo.AllReadonly<ActComplainResultGrouping>()
                                                    .Where(x => ActComplainResultGroupings.SecondInstanceNonCriminalReport.Contains(x.ActComplainResultGroup))
                                                    .ToListAsync();

            var actResultGroups = await readonlyrepo.AllReadonly<ActResultGroup>()
                                                    .Where(x => ActResultGroups.SecondInstanceReportNonCriminal.Contains(x.ActResultGrouping))
                                                    .ToListAsync();

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

        private async Task<byte[]> CaseSecondInstanceCriminalReportToExcelOne(CaseSecondInstanceFilterReportVM model)
        {
            var dataRows = (await CaseSecondInstanceReport_Select(userContext.CourtId, model).ToListAsync()).OrderBy(x => x.CaseRegNumberValue).ToList();

            int[] documentGroupings = { NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceComplaint,
                                        NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceProtest,
                                        NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceComplaintProtest};
            var documentTypes = readonlyrepo.AllReadonly<DocumentTypeGrouping>().Where(x => documentGroupings.Contains(x.DocumentTypeGroup)).ToList();
            var documentTypesComplaint = documentTypes.Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceComplaint ||
                        x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceComplaintProtest)
                        .Select(x => x.DocumentTypeId).ToList();
            var documentTypesProtest = documentTypes.Where(x => x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceProtest ||
                        x.DocumentTypeGroup == NomenclatureConstants.DocumentTypeGroupings.CaseSecondInstanceComplaintProtest)
                        .Select(x => x.DocumentTypeId).ToList();

            var complainResults = readonlyrepo.AllReadonly<ActComplainResultGrouping>()
                .Where(x => NomenclatureConstants.ActComplainResultGroupings.SecondInstanceReport.Contains(x.ActComplainResultGroup))
                .ToList();

            var actResultGroups = readonlyrepo.AllReadonly<ActResultGroup>()
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

            if (userContext.CourtTypeId == NomenclatureConstants.CourtType.VKS)
                excelService.AddRange("Рекапитулация за КАСАЦИОННИ НАКАЗАТЕЛНИ ДЕЛА", 20);
            else
                excelService.AddRange("Рекапитулация за ВЪЗЗИВНИ НАКАЗАТЕЛНИ ДЕЛА", 20);

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

        public async Task<byte[]> CaseSecondInstanceReportToExcelOne(CaseSecondInstanceFilterReportVM model)
        {
            if (model.CaseGroupId == CaseGroups.NakazatelnoDelo)
            {
                return await CaseSecondInstanceCriminalReportToExcelOne(model);
            }
            else
            {
                return await CaseSecondInstanceDifferentCriminalReportToExcelOne(model);
            }
        }

        private IQueryable<SentenceReportVM> SentenceReport_Select(int courtId, SentenceFilterReportVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CasePersonSentence, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => ((DateTime)x.InforcedDate).Date >= dateFromSearch.Date && ((DateTime)x.InforcedDate).Date <= dateToSearch.Date;

            Expression<Func<CasePersonSentence, bool>> regNumberSearch = x => true;
            if (string.IsNullOrEmpty(model.NumberCase) == false)
                regNumberSearch = x => EF.Functions.ILike(x.Case.RegNumber, model.NumberCase.ToEndingPaternSearch());


            return repo.AllReadonly<CasePersonSentence>()
                       .Where(x => x.Case.CourtId == courtId)
                       .Where(x => x.InforcedDate != null)
                       .Where(x => x.DateExpired == null)
                       .Where(dateSearch)
                       .Where(regNumberSearch)
                       .Select(x => new SentenceReportVM()
                       {
                           ActDate = x.ChangeCaseSessionActId != null ?
                                         x.ChangeCaseSessionAct.ActDate :
                                          x.CaseSessionAct.ActDate,
                           PersonData = x.CasePerson.FullName + " - " + (x.CasePerson.Addresses.Select(a => a.Address.FullAddress).FirstOrDefault() ?? "") +
                                          Environment.NewLine + (x.Description ?? ""),
                           SentenceData = x.ChangeCaseSessionActId != null ?
                                         ((string.Join(Environment.NewLine,
                                          x.ChangeCaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.IsActive && a.IsMain).Select(a => a.SessionResult.Label)) ?? "") +
                                          Environment.NewLine +
                                          x.ChangeCaseSessionAct.ActType.Label + " " +
                                          x.ChangeCaseSessionAct.RegNumber + "/" +
                                          (x.ChangeCaseSessionAct.ActDate != null ? ((DateTime)x.ChangeCaseSessionAct.ActDate).ToString("dd.MM.yyyy") : "") + Environment.NewLine +
                                          x.ChangeCaseSessionAct.Case.RegNumber + " " + x.ChangeCaseSessionAct.Case.Court.Label + Environment.NewLine +
                                          ((DateTime)x.InforcedDate).ToString("dd.MM.yyyy")) :
                                          ((string.Join(Environment.NewLine,
                                          x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.IsActive && a.IsMain).Select(a => a.SessionResult.Label)) ?? "") +
                                          Environment.NewLine +
                                          x.CaseSessionAct.ActType.Label + " " +
                                          x.CaseSessionAct.RegNumber + "/" +
                                          (x.CaseSessionAct.ActDate != null ? ((DateTime)x.CaseSessionAct.ActDate).ToString("dd.MM.yyyy") : "") + Environment.NewLine +
                                          x.CaseSessionAct.Case.RegNumber + " " + x.CaseSessionAct.Case.Court.Label + Environment.NewLine +
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
                           SentenceFinishData = string.IsNullOrEmpty(x.ExecIncomingDocument) ? "" : ((x.ExecIncomingDocument ?? "") + " " + x.InforcerInstitution.FullName),
                           Description = x.ExecRemark
                       }).AsQueryable();
        }

        public async Task<byte[]> SentenceReportToExcelOne(SentenceFilterReportVM model)
        {
            var dataRows = (await SentenceReport_Select(userContext.CourtId, model).ToListAsync()).OrderBy(x => x.SendData).ToList();
            for (int i = 0; i < dataRows.Count; i++)
            {
                var item = dataRows[i];
                item.Index = i + 1;
                //item.PersonData = item.PersonData.Decode();
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
                dateSearch = x => x.RegDate != null && x.RegDate >= dateFromSearch.ForceStartDate() && x.RegDate <= dateToSearch.ForceEndDate();

            Expression<Func<ExecList, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.ExecListObligations.Where(a => a.Obligation.CaseSessionAct.Case.CaseGroupId == model.CaseGroupId).Any();

            return repo.AllReadonly<ExecList>()
                       .Where(x => x.CourtId == courtId)
                       .Where(x => x.IsActive == true)
                       .Where(x => x.ExecListTypeId == NomenclatureConstants.ExecListTypes.Country)
                       .Where(x => x.RegDate != null)
                       .Where(dateSearch)
                       .Where(caseGroupWhere)
                       .Select(x => new ExecListReportVM()
                       {
                           ExecListStateId = x.ExecListStateId ?? 0,
                           ExecListNumber = x.RegNumber,
                           CaseRegNumbers = x.ExecListObligations.Select(a => a.Obligation.CaseSessionAct.Case.CaseGroup.Label + " " +
                                          a.Obligation.CaseSessionAct.Case.RegNumber).ToArray(),
                           obligations = x.ExecListObligations.Select(p => new ExecListObligationReportVM
                           {
                               MoneyTypeName = (p.Obligation.MoneyType.Label + " " + (p.Obligation.MoneyFineType.Label ?? "")).Trim() +
                                                (string.IsNullOrEmpty(p.Obligation.ObligationDescription) == false ? (" - " + p.Obligation.ObligationDescription) : ""),
                               Amount = p.Obligation.Amount,
                               AmountBGN = p.Obligation.AmountBGN ?? 0,
                               PaymentData = (x.CaseNumber != null ? "ИД № " + x.CaseNumber + " " : "") +
                                             string.Join(Environment.NewLine, p.Obligation.ObligationPayments.Where(a => a.IsActive == true)
                                             .Select(a => a.Amount.ToString("0.00") + " внесена на " + a.Payment.PaidDate.ToString("dd.MM.yyyy"))),
                           }),
                           PersonNames = x.ExecListObligations.Select(a => a.Obligation.FullName + " " + (a.Obligation.Uic ?? "")).ToArray(),
                           ExecListDate = x.RegDate ?? DateTime.Now, //тези без дата не излизат
                           ExecListCaseNumber = x.CaseNumber,
                           Receiver = repo.AllReadonly<DocumentTemplate>()
                                       .Where(a => a.SourceType == SourceTypeSelectVM.ExecList && a.SourceId == x.Id
                                           && a.DocumentId != null)
                                       .Select(a => a.Document.DocumentGroup.Label + " " + a.Document.DocumentNumber + "/" + a.Document.DocumentDate.ToString("dd.MM.yyyy") + Environment.NewLine +
                                               string.Join(Environment.NewLine, a.Document.DocumentPersons.Select(b => b.FullName)))
                                       .FirstOrDefault(),
                       }).AsQueryable();
        }

        private IQueryable<ExecListReportVM> ExecListReportPrev_Select(int courtId, ExecListFilterReportVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<ExecList, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate != null && x.RegDate >= dateFromSearch.ForceStartDate() && x.RegDate <= dateToSearch.ForceEndDate();

            Expression<Func<ExecList, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.ExecListObligations.Where(a => a.Obligation.CaseSessionAct.CaseSession.Case.CaseGroupId == model.CaseGroupId).Any();

            return repo.AllReadonly<ExecList>()
                       .Where(x => x.CourtId == courtId)
                       .Where(x => x.IsActive == true)
                       .Where(x => x.ExecListTypeId == NomenclatureConstants.ExecListTypes.Country)
                       .Where(x => x.RegDate != null)
                       .Where(dateSearch)
                       .Where(caseGroupWhere)
                       .Select(x => new ExecListReportVM()
                       {
                           ExecListNumber = x.RegNumber,
                           CaseRegNumbers = x.ExecListObligations.Select(a => a.Obligation.CaseSessionAct.Case.CaseGroup.Label + " " +
                                          a.Obligation.CaseSessionAct.Case.RegNumber).ToArray(),
                           CaseSessionActDatas = x.ExecListObligations.Select(a =>
                                           a.Obligation.CaseSessionAct.CaseSession.SessionType.Label + " " +
                                           a.Obligation.CaseSessionAct.CaseSession.DateFrom.ToString("dd.MM.yyyy") + ", " +
                                           a.Obligation.CaseSessionAct.ActType.Label + " " +
                                          (a.Obligation.CaseSessionAct.ActDate != null ? (a.Obligation.CaseSessionAct.RegNumber + "/" +
                                          ((DateTime)a.Obligation.CaseSessionAct.ActDate).ToString("dd.MM.yyyy")) : "") +
                                          Environment.NewLine +
                                          (a.Obligation.CaseSessionAct.ActInforcedDate != null ? ("В законна сила от " +
                                          ((DateTime)a.Obligation.CaseSessionAct.ActInforcedDate).ToString("dd.MM.yyyy")) : "")).ToArray(),
                           obligations = x.ExecListObligations.Select(p => new ExecListObligationReportVM
                           {
                               MoneyTypeName = (p.Obligation.MoneyType.Label + " " + (p.Obligation.MoneyFineType.Label ?? "")).Trim(),
                               Amount = p.Obligation.Amount,
                               AmountBGN = p.Obligation.AmountBGN ?? 0,
                               PaymentData = string.Join(Environment.NewLine, p.Obligation.ObligationPayments.Where(a => a.IsActive == true)
                                             .Select(a => a.Amount.ToString("0.00") + " внесена на " + a.Payment.PaidDate.ToString("dd.MM.yyyy"))),
                               PersonNameReceive = p.Obligation.ObligationReceives.Select(b => b.FullName).FirstOrDefault(),
                           }),
                           PersonNames = x.ExecListObligations.Select(a => a.Obligation.FullName + " " + (a.Obligation.Uic ?? "")).ToArray(),
                           ExecListDate = x.RegDate ?? DateTime.Now, //тези без дата не излизат
                           SendData = repo.AllReadonly<DocumentTemplate>()
                                       .Where(a => a.SourceType == SourceTypeSelectVM.ExecList && a.SourceId == x.Id
                                           && a.DocumentId != null)
                                       .Select(a => a.Document.DocumentNumber + "/" + a.Document.DocumentDate.ToString("dd.MM.yyyy"))
                                       .FirstOrDefault(),
                           Receiver = repo.AllReadonly<DocumentTemplate>()
                                       .Where(a => a.SourceType == SourceTypeSelectVM.ExecList && a.SourceId == x.Id
                                           && a.DocumentId != null)
                                       .Select(a => string.Join(Environment.NewLine, a.Document.DocumentPersons.Select(b => b.FullName)))
                                       .FirstOrDefault() ?? x.DeliveryPersonName,
                           ExecListCaseNumber = x.CaseNumber
                       }).AsQueryable();
        }

        public async Task<byte[]> ExecListReportToExcelOne(ExecListFilterReportVM model)
        {
            var dataRows = (await ExecListReport_Select(userContext.CourtId, model).ToListAsync()).OrderBy(x => x.ExecListDate).ToList();

            NPoiExcelService excelService = GetExcelHtmlTemplate(userContext.IsInterimPeriodEuro == false ? "ExecListNew" : "ExecListNewInterimPeriod");

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
                excelService.InsertRangeMoveCol(row.ExecListDate.ToString("dd.MM.yyyy"), 1, rowsRange);
                excelService.InsertRangeMoveCol(row.PersonName, 1, rowsRange);
                excelService.SetCellData("");
                if (userContext.IsInterimPeriodEuro == true)
                    excelService.SetCellData("");

                excelService.SetCellData(row.Receiver);

                if (row.ExecListStateId == NomenclatureConstants.ExecListStates.Cancel)
                    excelService.InsertRangeMoveCol((string.IsNullOrEmpty(row.ExecListCaseNumber) ? "" : "ИД № " + row.ExecListCaseNumber + Environment.NewLine) + "Обезсилен", 1, rowsRange);
                else
                    excelService.SetCellData("");

                string currencyStr = userContext.IsPeriodEuro == true ? " евро" : " лв.";
                bool addRow = false;
                foreach (var item in row.obligations)
                {
                    if (addRow)
                        excelService.rowIndex++;

                    excelService.colIndex = 4;
                    excelService.SetCellData(item.MoneyTypeName + " " + item.Amount.ToString("0.00") + currencyStr);

                    if (userContext.IsInterimPeriodEuro == true)
                    {
                        excelService.SetCellData(item.MoneyTypeName + " " + item.AmountBGN.ToString("0.00"));
                    }

                    excelService.colIndex++;

                    if (row.ExecListStateId != NomenclatureConstants.ExecListStates.Cancel)
                        excelService.SetCellData(item.PaymentData);

                    addRow = true;
                }
            }

            return excelService.ToArray();
        }

        public async Task<byte[]> ExecListReportToExcelOnePrev(ExecListFilterReportVM model)
        {
            var dataRows = (await ExecListReportPrev_Select(userContext.CourtId, model).ToListAsync()).OrderBy(x => x.ExecListDate).ToList();

            NPoiExcelService excelService = GetExcelHtmlTemplate(userContext.IsInterimPeriodEuro == false ? "ExecList" : "ExecListInterimPeriod");

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

                if (userContext.IsInterimPeriodEuro == true)
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

                    if (userContext.IsInterimPeriodEuro == true)
                    {
                        excelService.SetCellData(item.AmountBGN.ToString("0.00"));
                        excelService.colIndex = 7;
                        excelService.SetCellData(item.PersonNameReceive);
                        excelService.colIndex = 12;
                        excelService.SetCellData(item.PaymentData);
                    }
                    else
                    {
                        excelService.colIndex = 6;
                        excelService.SetCellData(item.PersonNameReceive);
                        excelService.colIndex = 11;
                        excelService.SetCellData(item.PaymentData);
                    }

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

            return repo.AllReadonly<CaseArchive>()
                       .Where(x => x.Case.CourtId == courtId)
                       .Where(groupWhere)
                       .Where(dateSearch)
                       .Where(numberSearch)
                       .Where(typeWhere)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Select(x => new CaseArchiveListReportVM()
                       {
                           CaseId = x.CaseId,
                           CaseTypeName = x.Case.CaseType.Label,
                           CaseNumber = x.Case.RegNumber,
                           ArchiveLink = x.ArchiveLink,
                           ArchiveNumber = x.RegNumber,
                           ArchiveYear = x.RegDate.Year,
                           ArchiveDate = x.RegDate,
                           ArchiveIndexName = x.CourtArchiveIndex.Label,
                           StorageYears = x.StorageYears
                       }).AsQueryable();
        }

        public async Task<byte[]> CaseArchiveListReportToExcelOne(CaseArchiveListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await CaseArchiveListReport_Select(userContext.CourtId, model).ToListAsync();

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
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
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
                           DocumentPersonsFirst = x.DocumentPersons.Select(a => a.FullName).FirstOrDefault(),
                           Description = x.Description,
                           DeliveryGroupName = x.DeliveryGroup.Label,
                           DocumentNumberValue = x.DocumentNumberValue ?? 0
                       }).AsQueryable();
        }

        public async Task<byte[]> DocumentOutListReportToExcelOne(DocumentOutListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = (await DocumentOutListReport_Select(userContext.CourtId, model, Environment.NewLine).ToListAsync()).OrderBy(x => x.DocumentNumberValue).ToList();

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
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
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

        public async Task<byte[]> PosDeviceReportToExcelOne(PosDeviceFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await PosDeviceReport_Select(userContext.CourtId, model).ToListAsync();
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
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );
            return excelService.ToArray();
        }

        private async Task<List<CaseSessionPublicReportVM>> CaseSessionPublicReport_Select(CaseSessionPublicFilterReportVM model, int startRow, int getRow, int[] resultDecision, List<CourtLawUnitOrder> courtLawUnits)
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

            int i = int.MinValue;
            int[] sessionStates = { NomenclatureConstants.SessionState.Nasrocheno, NomenclatureConstants.SessionState.Provedeno };
            var result = await readonlyrepo.AllReadonly<CaseSession>()
                               .Where(x => x.Case.CourtId == userContext.CourtId && x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
                               .Where(x => x.DateFrom >= model.DateFrom && x.DateFrom <= model.DateTo)
                               .Where(x => x.DateExpired == null)
                               .Where(x => sessionStates.Contains(x.SessionStateId))
                               .Where(caseGroupWhere)
                               .Where(departmentWhere)
                               .Where(caseInstanceWhere)
                               .Select(x => new CaseSessionPublicReportVM
                               {
                                   Id = x.Id,
                                   CaseId = x.CaseId,
                                   SessionDate = x.DateFrom,
                                   CaseData = x.Case.CaseType.Code + " " + x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                   SecretaryName = string.Join(", ", x.CaseSessionMeetings
                                                                      .Select(a => string.Join(", ", a.CaseSessionMeetingUsers
                                                                                   .Select(b => b.SecretaryUser.LawUnit.FullName)))
                                                                      ),
                                   CaseTypeCode = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label + Environment.NewLine + (x.Case.CaseCode.LawBaseDescription ?? ""),
                                   CaseCodeName = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label,
                                   isQuick = x.Case.ProcessPriorityId == NomenclatureConstants.ProcessPriority.Quick,
                                   SessionTypeId = x.SessionTypeId,
                               })
                               .OrderBy(x => x.SessionDate)
                               .Skip(startRow)
                               .Take(getRow)
                               .ToListAsync();

            var sessionIds = result.Select(x => x.Id).ToList();
            var sessionIdsNullable = result.Select(x => (int?)x.Id).ToList();
            var caseIds = result.Select(x => x.CaseId).ToList();

            var lawUnits = await readonlyrepo.AllReadonly<CaseLawUnit>()
                                 .Where(x => sessionIdsNullable.Contains(x.CaseSessionId))
                                 .Select(x => new
                                 {
                                     caseSessionId = x.CaseSessionId,
                                     dateTo = x.DateTo,
                                     judgeRoleId = x.JudgeRoleId,
                                     lawUnitId = x.LawUnitId,
                                     judgeDepartmentRoleId = x.JudgeDepartmentRoleId,
                                     lawUnitFullName = x.LawUnit.FullName,
                                     courtDepartmentId = x.CourtDepartmentId,
                                     courtDepartmentLabel = x.CourtDepartment.Label,
                                 })
                                 .ToListAsync();

            var casePersons = await readonlyrepo.AllReadonly<CasePerson>()
                                 .Where(x => sessionIdsNullable.Contains(x.CaseSessionId) && x.DateExpired == null)
                                 .Select(x => new
                                 {
                                     caseSessionId = x.CaseSessionId,
                                     personRoleRoleKindId = x.PersonRole.RoleKindId,
                                     fullName = x.FullName,
                                     personRoleLabel = x.PersonRole.Label,
                                     personRoleId = x.PersonRoleId,
                                     dateTo = x.DateTo,
                                 })
                                 .ToListAsync();

            var caseSessionResults = await readonlyrepo.AllReadonly<Infrastructure.Data.Models.Cases.CaseSessionResult>()
                                 .Where(x => sessionIds.Contains(x.CaseSessionId) && x.DateExpired == null)
                                 .Select(x => new
                                 {
                                     caseSessionId = x.CaseSessionId,
                                     isMain = x.IsMain,
                                     isActive = x.IsActive,
                                     sessionResultId = x.SessionResultId,
                                     sessionResultLabel = x.SessionResult.Label,
                                     sessionResultBaseId = x.SessionResultBaseId,
                                     sessionResultBaseLabel = x.SessionResultBase.Label,
                                     sessionResultSessionResultGroupId = x.SessionResult.SessionResultGroupId,
                                 })
                                 .ToListAsync();


            var caseSessions = await readonlyrepo.AllReadonly<CaseSession>()
                                       .Where(x => caseIds.Contains(x.CaseId) && x.DateExpired == null)
                                       .Select(x => new
                                       {
                                           caseId = x.CaseId,
                                           dateFrom = x.DateFrom,
                                           sessionStateId = x.SessionStateId,
                                       })
                                       .ToListAsync();

            var caseSessionActs = await readonlyrepo.AllReadonly<CaseSessionAct>()
                                         .Where(x => sessionIds.Contains(x.CaseSessionId) && x.DateExpired == null)
                                         .Select(x => new
                                         {
                                             id = x.Id,
                                             caseSessionId = x.CaseSessionId,
                                             actDate = x.ActDate,
                                             description = x.Description,
                                             actDeclaredDate = x.ActDeclaredDate,
                                             actTypeId = x.ActTypeId,
                                             actTypeLabel = x.ActType.Label,
                                             regNumber = x.RegNumber,
                                             isFinalDoc = x.IsFinalDoc,
                                             regDate = x.RegDate,
                                         })
                                         .ToListAsync();

            var caseLifecycles = await readonlyrepo.AllReadonly<CaseLifecycle>()
                                       .Where(x => caseIds.Contains(x.CaseId) && x.DateExpired == null)
                                       .Select(x => new
                                       {
                                           caseId = x.CaseId,
                                           lifecycleTypeId = x.LifecycleTypeId,
                                           dateTo = x.DateTo,
                                           id = x.Id,
                                           caseSessionActId = x.CaseSessionActId,
                                           caseSessionActActComplainResultId = x.CaseSessionAct.ActComplainResultId,
                                           durationMonths = x.DurationMonths,
                                       })
                                       .ToListAsync();

            //Това се прави само заради подредбата на LawUnit
            foreach (var item in result)
            {
                //Състав
                var oneSessionLawUnits = lawUnits.Where(x => x.caseSessionId == item.Id).ToList();

                item.CaseLawUnits = string.Join(", ", oneSessionLawUnits.Where(a => ((a.dateTo ?? dateEnd) >= item.SessionDate) && (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(a.judgeRoleId)))
                                                        .OrderBy(a => a.judgeRoleId)
                                                        .ThenBy(a => courtLawUnits.Where(b => b.LawUnitId == a.lawUnitId)
                                                        .Select(b => a.judgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel ? i : b.OrderNumber)
                                                        .FirstOrDefault())
                                                        .Select(a => a.lawUnitFullName));

                item.CourtDepartmentName = oneSessionLawUnits.Where(a => (a.dateTo ?? dateEnd) >= item.SessionDate && a.courtDepartmentId != null)
                                                    .Select(a => a.courtDepartmentLabel).FirstOrDefault();

                item.JudgeReporterName = oneSessionLawUnits.Where(a => (a.dateTo ?? dateEnd) >= item.SessionDate && a.judgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                     .Select(a => a.lawUnitFullName)
                                                                     .FirstOrDefault();

                item.JudgeReporterId = oneSessionLawUnits.Where(a => (a.dateTo ?? dateEnd) >= item.SessionDate && a.judgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                     .Select(a => a.lawUnitId)
                                                                     .FirstOrDefault();

                //Страни
                var oneSessionCasePersons = casePersons.Where(x => x.caseSessionId == item.Id).ToList();

                item.LeftSide = string.Join(Environment.NewLine, oneSessionCasePersons.Where(a => a.personRoleRoleKindId == NomenclatureConstants.RoleKind.LeftSide)
                                                         .Select(p => p.fullName + " (" + p.personRoleLabel + ")"));

                item.LeftSideGrajdansko = string.Join(Environment.NewLine, oneSessionCasePersons.Where(a => a.personRoleRoleKindId == NomenclatureConstants.RoleKind.LeftSide &&
                                                                                                                  (a.personRoleId == NomenclatureConstants.PersonRole.Petitioner ||
                                                                                                                   a.personRoleId == NomenclatureConstants.PersonRole.Plaintiff ||
                                                                                                                   a.personRoleId == NomenclatureConstants.PersonRole.Kreditor))
                                                                                                      .Select(p => p.fullName + " (" + p.personRoleLabel + ")"));

                item.RightSideGrajdansko = string.Join(Environment.NewLine, oneSessionCasePersons.Where(a => a.personRoleRoleKindId == NomenclatureConstants.RoleKind.RightSide &&
                                                                                                         (a.personRoleId == NomenclatureConstants.PersonRole.Libellee ||
                                                                                                          a.personRoleId == NomenclatureConstants.PersonRole.Debtor))
                                                                                             .Select(p => p.fullName + " (" + p.personRoleLabel + ")"));

                item.RightSide = string.Join(Environment.NewLine, oneSessionCasePersons.Where(a => a.personRoleRoleKindId == NomenclatureConstants.RoleKind.RightSide)
                                                                                             .Select(p => p.fullName + " (" + p.personRoleLabel + ")"));

                item.ProsecutorName = string.Join(",", oneSessionCasePersons.Where(p => p.personRoleId == NomenclatureConstants.PersonRole.Prosecutor && (p.dateTo ?? dateEnd) >= item.SessionDate)
                                               .Select(p => p.fullName));

                //Резултат
                var oneCaseSessionResults = caseSessionResults.Where(x => x.caseSessionId == item.Id).ToList();
                var oneCaseSessions = caseSessions.Where(x => x.caseId == item.CaseId).ToList();
                var oneCaseSessionActs = caseSessionActs.Where(x => x.caseSessionId == item.Id).ToList();

                item.SessionResult = string.Join(Environment.NewLine, oneCaseSessionResults.Where(a => a.isMain && a.isActive)
                                            .Select(p => p.sessionResultLabel + ((p.sessionResultBaseId != null) ? (" - " + p.sessionResultBaseLabel) : string.Empty)));

                item.AnnouncedForResolution = oneCaseSessionResults.Where(a => resultDecision.Contains(a.sessionResultId))
                                                                   .Select(p => p.sessionResultLabel)
                                                                   .FirstOrDefault();

                item.SessionAdjourn = string.Join(",", oneCaseSessionResults.Where(sr => sr.sessionResultSessionResultGroupId != null)
                                                                .Select(sr => sr.sessionResultLabel + ((sr.sessionResultBaseId != null) ? (" - " + sr.sessionResultBaseLabel) : string.Empty))) +
                                                                (oneCaseSessionResults.Any(csr => csr.sessionResultSessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Procrastination) ?
                                                                (oneCaseSessions.Where(css => css.dateFrom > item.SessionDate && css.sessionStateId == NomenclatureConstants.SessionState.Nasrocheno)
                                                                .OrderBy(css => css.dateFrom).Select(css => " Насрочено за: " + css.dateFrom.ToString(FormattingConstant.NormalDateFormatHHMM)).FirstOrDefault() + " " +
                                                                string.Join("; ", oneCaseSessionActs.Where(d => d.actDate != null)
                                                                .Select(d => d.description))) : string.Empty);

                item.DateCaseOffice = oneCaseSessionResults.Where(r => r.sessionResultSessionResultGroupId != null).Any() ?
                                                              oneCaseSessionActs.Where(a => a.actDeclaredDate != null &&
                                                                        a.actTypeId == NomenclatureConstants.ActType.Protokol)
                                                                        .OrderBy(a => a.actDeclaredDate)
                                                                        .Select(a => a.actDeclaredDate)
                                                                        .FirstOrDefault() :
                                                              oneCaseSessionActs.Where(a => a.actDeclaredDate != null &&
                                                                        a.actTypeId != NomenclatureConstants.ActType.Protokol)
                                                                        .OrderBy(a => a.actDeclaredDate)
                                                                        .Select(a => a.actDeclaredDate)
                                                                        .FirstOrDefault();

                item.SessionResultGroupIds = oneCaseSessionResults.Where(a => a.sessionResultSessionResultGroupId != null).Select(a => a.sessionResultSessionResultGroupId ?? 0).ToArray();

                item.SessionResultIds = oneCaseSessionResults.Select(a => a.sessionResultId).ToArray();

                //Акт
                item.SessionAct = string.Join("; ", oneCaseSessionActs.Where(d => d.actDate != null)
                                                .Select(d => d.actTypeLabel + " №" + d.regNumber + "/" + ((DateTime)d.actDate).ToString("dd.MM.yyyy")));

                item.ActDecision = oneCaseSessionActs.Where(act => act.isFinalDoc)
                                                       .Select(act => act.actTypeLabel + " №" + act.regNumber + "/" +
                                                       (act.regDate ?? DateTime.Now).ToString("dd.MM.yyyy") + " " +
                                                       (act.description ?? ""))
                                                       .FirstOrDefault();

                item.HasAct = oneCaseSessionActs.Any();

                //Интервал
                var oneCaseLifecycles = caseLifecycles.Where(x => x.caseId == item.CaseId).ToList();

                item.IsFinishSession = oneCaseLifecycles.Where(a => a.lifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                       a.dateTo != null && oneCaseSessionActs.Where(b => b.id == a.caseSessionActId && b.isFinalDoc).Any()
                       )
                       .Any();

                item.FinishActComplainResultId = oneCaseLifecycles.Where(a => a.lifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                          a.dateTo != null && oneCaseSessionActs.Where(b => b.id == a.caseSessionActId && b.isFinalDoc).Any()
                                                          ).Select(a => a.caseSessionActActComplainResultId ?? 0)
                                                          .FirstOrDefault();

                item.DurationMonths = oneCaseLifecycles.Where(a => a.lifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                          a.dateTo != null && oneCaseSessionActs.Where(b => b.id == a.caseSessionActId && b.isFinalDoc).Any()
                                                          ).Select(a => a.durationMonths)
                                                          .FirstOrDefault();

            }

            return result;
        }

        private async Task<List<CaseSessionPublicReportVM>> CaseSessionPublicReportAll_Select(CaseSessionPublicFilterReportVM model)
        {
            var resultDecision = readonlyrepo.AllReadonly<SessionResultGrouping>()
                                 .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseSessionPublicReportDecision)
                                 .Select(x => x.SessionResultId)
                                 .ToArray();

            var courtLawUnits = readonlyrepo.AllReadonly<CourtLawUnitOrder>()
                                            .Where(x => x.CourtId == userContext.CourtId)
                                            .ToList();

            List<CaseSessionPublicReportVM> dataRows = new List<CaseSessionPublicReportVM>();
            List<CaseSessionPublicReportVM> getRows = new List<CaseSessionPublicReportVM>();


            int start = 0;
            int take = 100;
            getRows = await CaseSessionPublicReport_Select(model, start, take, resultDecision, courtLawUnits);
            dataRows.AddRange(getRows);

            while (getRows.Any())
            {
                start = start + take;
                getRows = await CaseSessionPublicReport_Select(model, start, take, resultDecision, courtLawUnits);
                dataRows.AddRange(getRows);
            }

            return dataRows;

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

            return readonlyrepo.AllReadonly<CaseSession>()
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
                                   CasePersons = string.Join(Environment.NewLine, x.CasePersons.Where(a => a.DateExpired == null &&
                                                                     a.CaseSessionId == null)
                                                                     .Select(p => p.FullName)),
                                   SessionResult = string.Join(Environment.NewLine,
                                            x.CaseSessionResults.Where(a => a.IsMain && a.IsActive).Select(p => p.SessionResult.Label)),
                                   SessionAdjourn = x.SessionStateId == NomenclatureConstants.SessionState.Cancel ? x.Description : "",
                                   ActResultName = string.Join(",", x.CaseSessionActs.Where(b => b.DateExpired == null && b.ActResultId != null)
                                                                            .Select(b => b.ActResult.Label)),
                                   IsNewNumberCase = readonlyrepo.AllReadonly<CaseMigration>()
                                                                 .Where(a => a.CaseId == x.CaseId && a.CaseId != a.InitialCaseId && a.DateExpired == null)
                                                                 .Any(),
                                   IsStop = x.CaseSessionResults.Where(a => a.IsMain && a.IsActive && a.DateExpired == null &&
                                                                            a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Stop)
                                                                .Any()
                               }).AsQueryable();
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

        private async Task<byte[]> CaseSessionPublicReportToExcelOneSecondInstance(CaseSessionPublicFilterReportVM model)
        {
            var dataRows = await CaseSessionPublicReportAll_Select(model);

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

            excelService.SetRowBreak();
            var dataRowsQuick = dataRows.Where(x => x.isQuick).ToList();
            excelService.rowIndex += 5;

            excelService.colIndex = 3;
            excelService.AddRangeMoveCol("Общо", 1, 1);
            excelService.AddRangeMoveCol("В т.ч. БП", 1, 1);
            excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "Насрочени (общо)", dataRows.Count, dataRowsQuick.Count);
            excelService.AddRangeMoveCol("   в т.ч.", 3, 1); excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "      друго заседание", 0, 0);

            //Отложени
            AddTextCountToExcelSessionPublic(excelService, "Отложени (общо)",
                            dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Procrastination)).Count(),
                            dataRowsQuick.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Procrastination)).Count());

            AddTextCountToExcelSessionPublic(excelService, "Спрени",
                            dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Stop)).Count(),
                            dataRowsQuick.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Stop)).Count());

            AddTextCountToExcelSessionPublic(excelService, "Обявени за решаване (без вписан съдебен акт)",
                            dataRows.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.AnnouncedForResolution) &&
                                  x.HasAct == false).Count(),
                            dataRowsQuick.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.AnnouncedForResolution) &&
                                  x.HasAct == false).Count());

            var dataRowsFinish = dataRows.Where(Finish_Where()).ToList();
            var dataRowsQuickFinish = dataRowsFinish.Where(x => x.isQuick).ToList();
            AddTextCountToExcelSessionPublic(excelService, "Решени",
                            dataRowsFinish.Count,
                            dataRowsQuickFinish.Count);
            excelService.AddRangeMoveCol("   в т.ч.", 3, 1); excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "      в срок до 3 месеца",
                        dataRowsFinish.Where(x => x.DurationMonths < 3).Count(),
                        dataRowsQuickFinish.Where(x => x.DurationMonths < 3).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в срок над 3 месеца",
                        dataRowsFinish.Where(x => x.DurationMonths >= 3).Count(),
                        dataRowsQuickFinish.Where(x => x.DurationMonths >= 3).Count());

            var dataRowsSuspended = dataRows.Where(Suspended_Where()).ToList();
            var dataRowsQuickSuspended = dataRowsSuspended.Where(x => x.isQuick).ToList();
            AddTextCountToExcelSessionPublic(excelService, "Прекратени (общо)",
                            dataRowsSuspended.Count,
                            dataRowsQuickSuspended.Count);
            excelService.AddRangeMoveCol("   в т.ч.", 3, 1); excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "      в срок до 3 месеца",
                        dataRowsSuspended.Where(x => x.DurationMonths < 3).Count(),
                        dataRowsQuickSuspended.Where(x => x.DurationMonths < 3).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в срок над 3 месеца",
                        dataRowsSuspended.Where(x => x.DurationMonths >= 3).Count(),
                        dataRowsQuickSuspended.Where(x => x.DurationMonths >= 3).Count());

            //По съдии
            CaseSessionPublicReportRecapJudge(excelService, dataRows, model);

            return excelService.ToArray();
        }

        private async Task<byte[]> CaseSessionPublicReportToExcelOneFirstInstanceGrajdansko(CaseSessionPublicFilterReportVM model)
        {
            var dataRows = await CaseSessionPublicReportAll_Select(model);

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

            excelService.SetRowBreak();
            var dataRowsQuick = dataRows.Where(x => x.isQuick).ToList();
            excelService.rowIndex += 5;

            excelService.colIndex = 3;
            excelService.AddRangeMoveCol("Общо", 1, 1);
            excelService.AddRangeMoveCol("В т.ч. БП", 1, 1);
            excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "Насрочени (общо)", dataRows.Count, dataRowsQuick.Count);
            excelService.AddRangeMoveCol("   в т.ч.", 3, 1); excelService.rowIndex++;
            if (userContext.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                AddTextCountToExcelSessionPublic(excelService, "      заседание по привременни мерки",
                                      dataRows.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.TemporarilyAction).Count(),
                                      dataRowsQuick.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.TemporarilyAction).Count());
            if (userContext.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt)
                AddTextCountToExcelSessionPublic(excelService, "      събрание на кредиторите",
                                      dataRows.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.KreditorSession).Count(),
                                      dataRowsQuick.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.KreditorSession).Count());
            AddTextCountToExcelSessionPublic(excelService, "      друго заседание", 0, 0);

            var noMoves = repo.AllReadonly<SessionResultGrouping>().Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.NoMove).Select(x => x.SessionResultId).ToList();

            //Отложени
            var dataRowsProcrastination = dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Procrastination)).ToList();
            var dataRowsQuickProcrastination = dataRowsProcrastination.Where(x => x.isQuick).ToList();
            AddTextCountToExcelSessionPublic(excelService, "Отложени (общо)",
                            dataRowsProcrastination.Count,
                            dataRowsQuickProcrastination.Count);
            excelService.AddRangeMoveCol("   в т.ч.", 3, 1); excelService.rowIndex++;

            if (userContext.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                AddTextCountToExcelSessionPublic(excelService, "      в помирително заседание",
                            dataRowsProcrastination.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.Conciliatory &&
                                  noMoves.Where(a => x.SessionResultIds.Contains(a)).Any() == false).Count(),
                            dataRowsQuickProcrastination.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.Conciliatory &&
                                  noMoves.Where(a => x.SessionResultIds.Contains(a)).Any() == false).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в I-во заседание",
                        dataRowsProcrastination.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.FirstSession &&
                              x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.ProcrastinationFirstSession)).Count(),
                        dataRowsQuickProcrastination.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.FirstSession &&
                              x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.ProcrastinationFirstSession)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      във II-ро заседание",
                        dataRowsProcrastination.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.SecondSession &&
                              noMoves.Where(a => x.SessionResultIds.Contains(a)).Any() == false).Count(),
                        dataRowsQuickProcrastination.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.SecondSession &&
                              noMoves.Where(a => x.SessionResultIds.Contains(a)).Any() == false).Count());

            if (userContext.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                AddTextCountToExcelSessionPublic(excelService, "      в помирително заседание",
                            dataRowsProcrastination.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.TemporarilyAction &&
                                  noMoves.Where(a => x.SessionResultIds.Contains(a)).Any() == false).Count(),
                            dataRowsQuickProcrastination.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.TemporarilyAction &&
                                  noMoves.Where(a => x.SessionResultIds.Contains(a)).Any() == false).Count());

            int[] sessionTypes = {
                NomenclatureConstants.SessionType.TemporarilyAction, NomenclatureConstants.SessionType.SecondSession,
                NomenclatureConstants.SessionType.FirstSession, NomenclatureConstants.SessionType.Conciliatory };
            AddTextCountToExcelSessionPublic(excelService, "      само отложени",
                        dataRowsProcrastination.Where(x => sessionTypes.Contains(x.SessionTypeId) == false &&
                              noMoves.Where(a => x.SessionResultIds.Contains(a)).Any() == false &&
                              x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.ProcrastinationFirstSession) == false).Count(),
                        dataRowsQuickProcrastination.Where(x => sessionTypes.Contains(x.SessionTypeId) == false &&
                              noMoves.Where(a => x.SessionResultIds.Contains(a)).Any() == false &&
                              x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.ProcrastinationFirstSession) == false).Count());

            AddTextCountToExcelSessionPublic(excelService, "      без движение",
                        dataRowsProcrastination.Where(x => noMoves.Where(a => x.SessionResultIds.Contains(a)).Any()).Count(),
                        dataRowsQuickProcrastination.Where(x => noMoves.Where(a => x.SessionResultIds.Contains(a)).Any()).Count());

            AddTextCountToExcelSessionPublic(excelService, "Спрени",
                            dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Stop)).Count(),
                            dataRowsQuick.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Stop)).Count());

            AddTextCountToExcelSessionPublic(excelService, "Обявени за решаване (без вписан съдебен акт)",
                            dataRows.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.AnnouncedForResolution) &&
                                  x.HasAct == false).Count(),
                            dataRowsQuick.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.AnnouncedForResolution) &&
                                  x.HasAct == false).Count());

            var dataRowsFinish = dataRows.Where(Finish_Where()).ToList();
            var dataRowsQuickFinish = dataRowsFinish.Where(x => x.isQuick).ToList();
            AddTextCountToExcelSessionPublic(excelService, "Решени",
                            dataRowsFinish.Count,
                            dataRowsQuickFinish.Count);
            excelService.AddRangeMoveCol("   в т.ч.", 3, 1); excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "      искът уважен изцяло",
                        dataRowsFinish.Where(x => x.FinishActComplainResultId == NomenclatureConstants.ActComplainResults.AcceptAll).Count(),
                        dataRowsQuickFinish.Where(x => x.FinishActComplainResultId == NomenclatureConstants.ActComplainResults.AcceptAll).Count());

            AddTextCountToExcelSessionPublic(excelService, "      искът уважен отчасти",
                        dataRowsFinish.Where(x => x.FinishActComplainResultId == NomenclatureConstants.ActComplainResults.AcceptNotAll).Count(),
                        dataRowsQuickFinish.Where(x => x.FinishActComplainResultId == NomenclatureConstants.ActComplainResults.AcceptNotAll).Count());

            AddTextCountToExcelSessionPublic(excelService, "      искът отхвърлен",
                        dataRowsFinish.Where(x => x.FinishActComplainResultId == NomenclatureConstants.ActComplainResults.Cancel).Count(),
                        dataRowsQuickFinish.Where(x => x.FinishActComplainResultId == NomenclatureConstants.ActComplainResults.Cancel).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в срок до 3 месеца",
                        dataRowsFinish.Where(x => x.DurationMonths < 3).Count(),
                        dataRowsQuickFinish.Where(x => x.DurationMonths < 3).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в срок над 3 месеца",
                        dataRowsFinish.Where(x => x.DurationMonths >= 3).Count(),
                        dataRowsQuickFinish.Where(x => x.DurationMonths >= 3).Count());

            var dataRowsSuspended = dataRows.Where(Suspended_Where()).ToList();
            var dataRowsQuickSuspended = dataRowsSuspended.Where(x => x.isQuick).ToList();
            AddTextCountToExcelSessionPublic(excelService, "Прекратени (общо)",
                            dataRowsSuspended.Count,
                            dataRowsQuickSuspended.Count);
            excelService.AddRangeMoveCol("   в т.ч.", 3, 1); excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "      по спогодба",
                        dataRowsSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.Agreement)).Count(),
                        dataRowsQuickSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.Agreement)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      изпратени по подсъдност",
                        dataRowsSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SendJurisdiction)).Count(),
                        dataRowsQuickSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SendJurisdiction)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      прекратени по други причини",
                        dataRowsSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SendJurisdiction) == false &&
                                                     x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.Agreement) == false).Count(),
                        dataRowsQuickSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SendJurisdiction) == false &&
                                                     x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.Agreement) == false).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в срок до 3 месеца",
                        dataRowsSuspended.Where(x => x.DurationMonths < 3).Count(),
                        dataRowsQuickSuspended.Where(x => x.DurationMonths < 3).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в срок над 3 месеца",
                        dataRowsSuspended.Where(x => x.DurationMonths >= 3).Count(),
                        dataRowsQuickSuspended.Where(x => x.DurationMonths >= 3).Count());

            if (userContext.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                AddTextCountToExcelSessionPublic(excelService, "Делби по допускане",
                                dataRows.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.Partition)).Count(),
                                dataRowsQuick.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.Partition)).Count());

            if (userContext.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                AddTextCountToExcelSessionPublic(excelService, "С определение по привременни мерки",
                                dataRows.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.TemporarilyAction)).Count(),
                                dataRowsQuick.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.TemporarilyAction)).Count());

            //По съдии
            CaseSessionPublicReportRecapJudge(excelService, dataRows, model);

            return excelService.ToArray();
        }

        private async Task<byte[]> CaseSessionPublicReportToExcelOneFirstInstanceNakazatelnoDistrictCourt(CaseSessionPublicFilterReportVM model)
        {
            var dataRows = await CaseSessionPublicReportAll_Select(model);

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

            excelService.SetRowBreak();
            var dataRowsQuick = dataRows.Where(x => x.isQuick).ToList();
            excelService.rowIndex += 5;

            excelService.colIndex = 3;
            excelService.AddRangeMoveCol("Общо", 1, 1);
            excelService.AddRangeMoveCol("В т.ч. БП", 1, 1);
            excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "Насрочени (общо)", dataRows.Count, dataRowsQuick.Count);
            excelService.AddRangeMoveCol("   в т.ч.", 3, 1); excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "      предварително изслушване",
                                      dataRows.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.PreHearing).Count(),
                                      dataRowsQuick.Where(x => x.SessionTypeId == NomenclatureConstants.SessionType.PreHearing).Count());

            AddTextCountToExcelSessionPublic(excelService, "      друго заседание", 0, 0);

            //Отложени
            AddTextCountToExcelSessionPublic(excelService, "Отложени (общо)",
                            dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Procrastination)).Count(),
                            dataRowsQuick.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Procrastination)).Count());

            AddTextCountToExcelSessionPublic(excelService, "Спрени",
                            dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Stop)).Count(),
                            dataRowsQuick.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Stop)).Count());

            AddTextCountToExcelSessionPublic(excelService, "Обявени за решаване (без вписан съдебен акт)",
                            dataRows.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.AnnouncedForResolution) &&
                                  x.HasAct == false).Count(),
                            dataRowsQuick.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.AnnouncedForResolution) &&
                                  x.HasAct == false).Count());

            var dataRowsFinish = dataRows.Where(Finish_Where()).ToList();
            var dataRowsQuickFinish = dataRowsFinish.Where(x => x.isQuick).ToList();
            AddTextCountToExcelSessionPublic(excelService, "Решени",
                            dataRowsFinish.Count,
                            dataRowsQuickFinish.Count);
            excelService.AddRangeMoveCol("   в т.ч.", 3, 1); excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "      обявени за решаване (с вписан съдебен акт)",
                        dataRowsFinish.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.AnnouncedForResolution)).Count(),
                        dataRowsQuickFinish.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.AnnouncedForResolution)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      с решение",
                        dataRowsFinish.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithDecision)).Count(),
                        dataRowsQuickFinish.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithDecision)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      с присъда",
                        dataRowsFinish.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithSentence)).Count(),
                        dataRowsQuickFinish.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithSentence)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      с определение",
                        dataRowsFinish.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithDefinition)).Count(),
                        dataRowsQuickFinish.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithDefinition)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      с разпореждане",
                        dataRowsFinish.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithWrit)).Count(),
                        dataRowsQuickFinish.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithWrit)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в срок до 3 месеца",
                        dataRowsFinish.Where(x => x.DurationMonths < 3).Count(),
                        dataRowsQuickFinish.Where(x => x.DurationMonths < 3).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в срок над 3 месеца",
                        dataRowsFinish.Where(x => x.DurationMonths >= 3).Count(),
                        dataRowsQuickFinish.Where(x => x.DurationMonths >= 3).Count());

            var dataRowsSuspended = dataRows.Where(Suspended_Where()).ToList();
            var dataRowsQuickSuspended = dataRowsSuspended.Where(x => x.isQuick).ToList();
            AddTextCountToExcelSessionPublic(excelService, "Прекратени (общо)",
                            dataRowsSuspended.Count,
                            dataRowsQuickSuspended.Count);
            excelService.AddRangeMoveCol("   в т.ч.", 3, 1); excelService.rowIndex++;

            AddTextCountToExcelSessionPublic(excelService, "      върнати за доразследване",
                        dataRowsSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SuspendedInvestigation)).Count(),
                        dataRowsQuickSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SuspendedInvestigation)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      по споразумение",
                        dataRowsSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithAgreement)).Count(),
                        dataRowsQuickSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithAgreement)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      изпратени по подсъдност",
                        dataRowsSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SendJurisdiction)).Count(),
                        dataRowsQuickSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SendJurisdiction)).Count());

            AddTextCountToExcelSessionPublic(excelService, "      прекратени по други причини",
                        dataRowsSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SendJurisdiction) == false &&
                                                     x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithAgreement) == false &&
                                                     x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SuspendedInvestigation) == false).Count(),
                        dataRowsQuickSuspended.Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SendJurisdiction) == false &&
                                                     x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.WithAgreement) == false &&
                                                     x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.SuspendedInvestigation) == false).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в срок до 3 месеца",
                        dataRowsSuspended.Where(x => x.DurationMonths < 3).Count(),
                        dataRowsQuickSuspended.Where(x => x.DurationMonths < 3).Count());

            AddTextCountToExcelSessionPublic(excelService, "      в срок над 3 месеца",
                        dataRowsSuspended.Where(x => x.DurationMonths >= 3).Count(),
                        dataRowsQuickSuspended.Where(x => x.DurationMonths >= 3).Count());

            //По съдии
            CaseSessionPublicReportRecapJudge(excelService, dataRows, model);

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
        private async Task<byte[]> CaseSessionPublicReportToExcelOneNakazatelnoRegional(CaseSessionPublicFilterReportVM model)
        {
            var dataRows = await CaseSessionPublicReportAll_Select(model);

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

        public async Task<byte[]> CaseSessionPublicReportToExcelOne(CaseSessionPublicFilterReportVM model)
        {
            if (model.InstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance || model.InstanceId == NomenclatureConstants.CaseInstanceType.ThirdInstance)
            {
                return await CaseSessionPublicReportToExcelOneSecondInstance(model);
            }
            else if (model.InstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance && model.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo)
            {
                return await CaseSessionPublicReportToExcelOneFirstInstanceGrajdansko(model);
            }
            else if (model.InstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance && model.CaseGroupId == NomenclatureConstants.CaseGroups.Trade)
            {
                return await CaseSessionPublicReportToExcelOneFirstInstanceGrajdansko(model);
            }
            else if (model.InstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance &&
                 model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                return await CaseSessionPublicReportToExcelOneFirstInstanceNakazatelnoDistrictCourt(model);
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

        public async Task<byte[]> CaseSessionPrivateReportToExcelOneTemplate(CaseSessionPrivateFilterReportVM model)
        {
            var dataRows = await CaseSessionPrivateReport_Select(userContext.CourtId, model).OrderBy(x => x.ActDateOrder).ToListAsync();

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
                var rowsRange = row.acts.Count();
                rowsRange += rowsRange > 0 ? 0 : 1; // Ако няма никакви актове да направи merge на 1 ред 

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
                excelService.InsertRangeMoveCol(row.CaseSessionResultName, 1, rowsRange);
                excelService.SetCellData("");

                var acts = row.acts.ToList();
                for (int j = 0; j < acts.Count; j++)
                {
                    var item = acts[j];
                    if (j > 0)
                        excelService.rowIndex++;
                    excelService.colIndex = 4;
                    excelService.SetCellData(item.ActTypeName + " № " + item.ActNumber + "/" + item.ActDate.ToString("dd.MM.yyyy"));
                    excelService.colIndex = 6;
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
                CaseSessionPrivateAddTextCountToExcel(excelService, "Всички заседания", dataRowsNakazatelno.Count);
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
                CaseSessionPrivateAddTextCountToExcel(excelService, "Всички заседания", dataRowsGrajdansko.Count);
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
                CaseSessionPrivateAddTextCountToExcel(excelService, "Всички заседания", dataRowsTrade.Count);
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
                CaseSessionPrivateAddTextCountToExcel(excelService, "Всички заседания", dataRowsAdministrative.Count);
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
            var dateTimeNow = DateTime.Now;
            var dateTimeAddOneYear = DateTime.Now.AddYears(1);
            return repo.AllReadonly<HtmlTemplate>()
                        .Where(x => x.Alias.ToUpper() == alias.ToUpper() &&
                                    (x.DateFrom <= dateTimeNow && dateTimeNow <= (x.DateTo ?? dateTimeAddOneYear)))
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
                                .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
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
                                                                                                     a.ActInforcedDate != null && a.ActDate != null)
                                                                                         .Select(a => ((DateTime)a.ActDate).ToString("dd.MM.yyyy"))),
                                    PersonName = x.OutCaseMigration.Case.Court.Label,
                                    CaseInstitutionType = x.CaseMigrationType.Description ?? x.CaseMigrationType.Label,
                                    CaseLinkType = x.OutCaseMigration.Case.CaseGroup.Label,
                                    CaseLinkNumber = x.OutCaseMigration.Case.RegNumber,
                                })
                                .AsQueryable();
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
                                .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
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
                                    CaseLinkNumberOnly = x.objInst.CaseNumber
                                })
                                .AsQueryable();
        }
        public IQueryable<CaseLinkReportVM> CaseLinkReport_Select(int courtId, CaseLinkFilterReportVM model, string newLine)
        {
            IQueryable<CaseLinkReportVM> result = null;
            IQueryable<CaseLinkReportVM> linkMigrations = null;
            IQueryable<CaseLinkReportVM> institutions = null;
            if ((model.InstitutionId <= 0 && model.InstitutionCaseTypeId <= 0 && model.InstitutionTypeId <= 0) || model.FromCourtId > 0 || model.CaseMigrationTypeId > 0)
            {
                linkMigrations = CaseLinkMigration_Select(courtId, model, newLine);
            }

            if ((model.FromCourtId <= 0 && model.CaseMigrationTypeId <= 0) || model.InstitutionTypeId > 0 || model.InstitutionId > 0 || model.InstitutionCaseTypeId > 0)
            {
                institutions = CaseLinkInstitutionNew_Select(courtId, model, newLine);
            }

            result = Enumerable.Concat(linkMigrations ?? Enumerable.Empty<CaseLinkReportVM>(), institutions ?? Enumerable.Empty<CaseLinkReportVM>()).AsQueryable();

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
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
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
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="newLine">Знак за нов ред</param>
        /// <returns></returns>
        public IQueryable<SentenceListReportVM> SentenceListReport_Select(SentenceListFilterReportVM filter, string newLine)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);

            Expression<Func<CasePersonSentence, bool>> dateSearch = x => x.InforcedDate >= filter.DateFrom &&
                                                                         x.InforcedDate <= filter.DateTo;

            Expression<Func<CasePersonSentence, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CasePersonSentence, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CasePersonSentence, bool>> caseCodeWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeWhere = x => x.Case.CaseCodeId == filter.CaseCodeId;

            Expression<Func<CasePersonSentence, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.Case.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                        (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                        a.LawUnitId == filter.JudgeReporterId &&
                                                                        a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<CasePersonSentence, bool>> sessionResultWhere = x => true;
            if (filter.SessionResultId > 0)
                sessionResultWhere = x => x.Case.CaseSessionResults.Any(a => a.DateExpired == null &&
                                                                             a.IsActive && a.IsMain &&
                                                                             a.SessionResultId == filter.SessionResultId &&
                                                                             a.CaseSession.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                                                                    b.IsFinalDoc));

            Expression<Func<CasePersonSentence, bool>> sentenceResultWhere = x => true;
            if (filter.SentenceResultTypeId > 0)
                sentenceResultWhere = x => x.SentenceResultTypeId == filter.SentenceResultTypeId;

            Expression<Func<CasePersonSentence, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            return readonlyrepo.AllReadonly<CasePersonSentence>()
                       .Where(x => x.CourtId == userContext.CourtId)
                       .Where(x => x.InforcedDate != null)
                       .Where(x => (x.IsActive ?? true))
                       .Where(dateSearch)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseCodeWhere)
                       .Where(sessionResultWhere)
                       .Where(sentenceResultWhere)
                       .Where(judgeReporterSearch)
                       .Where(caseCodeIdsWhere)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Select(x => new SentenceListReportVM
                       {
                           CaseId = x.CaseId,
                           CaseTypeName = x.Case.CaseType.Label,
                           CaseRegNumber = x.Case.RegNumber,
                           InforcedDate = (DateTime)x.InforcedDate,
                           JudgeReporterName = x.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                             (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                             a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                  .Select(a => a.LawUnit.FullName)
                                                                  .FirstOrDefault(),
                           CaseCodeName = (x.Case.CaseCode.Code ?? "") + " " + x.Case.CaseCode.Label,
                           SessionResultName = string.Join(newLine, x.Case.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                         a.IsActive && a.IsMain &&
                                                                                                         a.CaseSession.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                                                                                                b.IsFinalDoc))
                                                                                             .Select(a => a.SessionResult.Label)),
                           SessionResultNameFirst = x.Case.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                         a.IsActive && a.IsMain &&
                                                                                         a.CaseSession.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                                                                                b.IsFinalDoc))
                                                                             .Select(a => a.SessionResult.Label)
                                                                             .FirstOrDefault(),
                           SentenceResultTypeName = x.SentenceResultType.Label
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка влезли в сила присъди Excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<byte[]> SentenceListReportExportExcel(SentenceListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = (await SentenceListReport_Select(model, Environment.NewLine).ToListAsync()).OrderBy(x => x.InforcedDate).ToList();

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
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Метод извличащ данни за актове подлежащи на обезличаване
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<SessionActForDepersonalizeReportVM> SessionActForDepersonalizeReport_Select(SessionActForDepersonalizeFilterReportVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            model.DateFrom = (model.DateFrom ?? dateNow.AddYears(-100)).ForceStartDate();
            model.DateTo = (model.DateTo ?? dateNow.AddYears(100)).ForceEndDate();


            Expression<Func<CaseSessionAct, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.ActDeclaredDate >= model.DateFrom &&
                                  x.ActDeclaredDate <= model.DateTo;

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

            Expression<Func<CaseSessionAct, bool>> caseGroupWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
            {
                var listGroupIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
                    listGroupIds = model.CaseGroupIds_text.Split(',').Select(Int32.Parse).ToList();
                caseGroupWhere = x => listGroupIds.Contains(x.Case.CaseGroupId);
            }

            Expression<Func<CaseSessionAct, bool>> caseTypeWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
            {
                var listTypeIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
                    listTypeIds = model.CaseTypeIds_text.Split(',').Select(Int32.Parse).ToList();
                caseTypeWhere = x => listTypeIds.Contains(x.Case.CaseTypeId);
            }

            Expression<Func<CaseSessionAct, bool>> caseCodeIdsWhere = x => true;
            if (model.CaseCodeIds != null && model.CaseCodeIds.Any())
            {
                int[] caseCodeIds = model.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            Expression<Func<CaseSessionAct, bool>> caseActTypeWhere = x => true;
            if (!string.IsNullOrEmpty(model.ActTypeIds_text))
            {
                var listActTypeIds = new List<int>();
                if (!string.IsNullOrEmpty(model.ActTypeIds_text))
                    listActTypeIds = model.ActTypeIds_text.Split(',').Select(Int32.Parse).ToList();
                caseActTypeWhere = x => listActTypeIds.Contains(x.ActTypeId);
            }

            Expression<Func<CaseSessionAct, bool>> courtDepartment = x => true;
            if (model.CourtDepartmentId > 0)
                courtDepartment = x => x.CaseSession.CaseLawUnits.Any(a => a.CourtDepartmentId == model.CourtDepartmentId);

            Expression<Func<CaseSessionAct, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseSession.CaseLawUnits.Any(a => (a.DateTo ?? dateEnd).Date >= x.CaseSession.DateFrom.Date &&
                                                                               a.LawUnitId == model.JudgeReporterId &&
                                                                               a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<CaseSessionAct, bool>> courtDepartmentOtdelenie = x => true;
            if (model.CourtDepartmentOtdelenieId > 0)
                courtDepartmentOtdelenie = x => x.Case.OtdelenieId == model.CourtDepartmentOtdelenieId;

            return repo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.CourtId == userContext.CourtId)
                       .Where(x => x.ActDeclaredDate != null)
                       .Where(x => x.ActDate != null)
                       .Where(x => x.DateExpired == null)
                       .Where(x => x.DepersonalizeEndDate == null)
                       .Where(dateSearch)
                       .Where(isFinalAct)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(caseActTypeWhere)
                       .Where(courtDepartment)
                       .Where(judgeReporterSearch)
                       .Where(courtDepartmentOtdelenie)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Select(x => new SessionActForDepersonalizeReportVM
                       {
                           CaseTypeName = x.Case.CaseType.Label,
                           CaseRegNumber = x.Case.RegNumber,
                           CaseRegDate = x.Case.RegDate,
                           CaseId = x.CaseId,
                           SessionActId = x.Id,
                           SessionActTypeName = x.ActType.Label,
                           SessionActNumber = x.RegNumber,
                           SessionActDate = x.ActDate,
                           SessionTypeName = x.CaseSession.SessionType.Label,
                           CaseCodeLabel = (x.Case.CaseCodeId != null) ? x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label : string.Empty,
                           ActStateLabel = x.ActState.Label + (x.ActCoordination.Any(c => c.CoordinationDeclaredDate != null) ? " (OM)" : string.Empty)
                       })
                       .Concat(repo.AllReadonly<CaseSessionAct>()
                                  .Where(x => x.CourtId == userContext.CourtId)
                                  .Where(x => x.ActMotivesDeclaredDate != null)
                                  .Where(x => x.DateExpired == null)
                                  .Where(x => x.DepersonalizeMotiveEndDate == null)
                                  .Where(dateSearch)
                                  .Where(isFinalAct)
                                  .Where(caseGroupWhere)
                                  .Where(caseTypeWhere)
                                  .Where(caseCodeIdsWhere)
                                  .Where(caseActTypeWhere)
                                  .Where(courtDepartment)
                                  .Where(judgeReporterSearch)
                                  .Where(courtDepartmentOtdelenie)
                                  .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                                  .Select(x => new SessionActForDepersonalizeReportVM
                                  {
                                      CaseTypeName = x.Case.CaseType.Label,
                                      CaseRegNumber = x.Case.RegNumber,
                                      CaseRegDate = x.Case.RegDate,
                                      CaseId = x.CaseId,
                                      SessionActId = x.Id,
                                      SessionActTypeName = "Мотив към: " + x.ActType.Label,
                                      SessionActNumber = x.RegNumber,
                                      SessionActDate = x.ActDate,
                                      SessionTypeName = x.CaseSession.SessionType.Label,
                                      CaseCodeLabel = (x.Case.CaseCodeId != null) ? x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label : string.Empty,
                                      ActStateLabel = x.ActState.Label
                                  }))
                       .AsQueryable();
        }

        /// <summary>
        /// Справка Съдени и осъдени лица
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="newLine">Знак за нов ред</param>
        /// <returns></returns>
        public IQueryable<CasePersonDefendantListReportVM> CasePersonDefendantListReport_Select(CasePersonDefendantListFilterReportVM filter, string newLine)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);

            Expression<Func<CasePerson, bool>> dateSearch = x => x.Case.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                                                                 a.IsFinalDoc &&
                                                                                                 a.ActDeclaredDate != null &&
                                                                                                 a.ActDeclaredDate >= filter.DateFrom &&
                                                                                                 a.ActDeclaredDate <= filter.DateTo);

            Expression<Func<CasePerson, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CasePerson, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CasePerson, bool>> caseCodeWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeWhere = x => x.Case.CaseCodeId == filter.CaseCodeId;

            Expression<Func<CasePerson, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.Case.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                        (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == filter.JudgeReporterId &&
                                                                        a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<CasePerson, bool>> sessionResultWhere = x => true;
            if (filter.SessionResultId > 0)
                sessionResultWhere = x => x.Case.CaseSessionResults.Any(a => a.DateExpired == null &&
                                                                             a.IsActive &&
                                                                             a.SessionResultId == filter.SessionResultId &&
                                                                             a.CaseSession.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                                                                    b.IsFinalDoc));

            Expression<Func<CasePerson, bool>> maturityWhere = x => true;
            if (filter.PersonMaturityId > 0)
                maturityWhere = x => x.PersonMaturityId == filter.PersonMaturityId;

            Expression<Func<CasePerson, bool>> sentenceTypeWhere = x => true;
            if (filter.SentenceTypeId > 0)
                sentenceTypeWhere = x => x.CasePersonSentences.Any(a => a.DateExpired == null &&
                                                                        a.CasePersonSentencePunishments
                                                                         .Any(b => b.SentenceTypeId == filter.SentenceTypeId &&
                                                                                   b.DateExpired == null));

            Expression<Func<CasePerson, bool>> uicSearch = x => true;
            if (!string.IsNullOrEmpty(filter.PersonUicSearch))
                uicSearch = x => x.Uic.ToLower() == filter.PersonUicSearch.ToLower();

            Expression<Func<CasePerson, bool>> nameSearch = x => true;
            if (!string.IsNullOrEmpty(filter.PersonNameSearch))
                nameSearch = x => EF.Functions.ILike(x.FullName, filter.PersonNameSearch.ToPaternSearch());

            Expression<Func<CasePerson, bool>> sentenceLawbaseWhere = x => true;
            if (filter.SentenceLawbaseId > 0)
                sentenceLawbaseWhere = x => x.CasePersonSentences.Any(a => a.DateExpired == null &&
                                                                           a.CasePersonSentenceLawbases
                                                                            .Any(b => b.SentenceLawbaseId == filter.SentenceLawbaseId));

            Expression<Func<CasePerson, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            var personRoleIds = repo.AllReadonly<PersonRoleGrouping>()
                                 .Where(x => x.PersonRoleGroup == NomenclatureConstants.PersonRoleGroupings.RoleCasePersonDedendantList)
                                 .Select(x => x.PersonRoleId)
                                 .ToArray();

            return repo.AllReadonly<CasePerson>()
                                .Where(x => x.CourtId == userContext.CourtId)
                                .Where(x => x.CaseSessionId == null)
                                .Where(x => x.DateExpired == null)
                                .Where(x => personRoleIds.Contains(x.PersonRoleId))
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
                                .Where(sentenceLawbaseWhere)
                                .Where(caseCodeIdsWhere)
                                .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                                .Select(x => new CasePersonDefendantListReportVM
                                {
                                    CaseId = x.CaseId,
                                    CaseTypeName = x.Case.CaseType.Label,
                                    CaseRegNumber = x.Case.RegNumber,
                                    CaseRegDate = x.Case.RegDate,
                                    JudgeReporterName = x.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                                       (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                                       a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                           .Select(a => a.LawUnit.FullName)
                                                                           .FirstOrDefault(),
                                    CaseCodeName = (x.Case.CaseCode.Code ?? "") + " " + x.Case.CaseCode.Label,
                                    SessionResultName = string.Join(newLine,
                                                                    x.Case.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                         a.IsActive &&
                                                                                                         a.IsMain &&
                                                                                                         a.CaseSession.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                                                                                                b.IsFinalDoc))
                                                                                             .Select(a => a.SessionResult.Label)),
                                    SessionResultNameFirst = x.Case.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                  a.IsActive &&
                                                                                                  a.IsMain &&
                                                                                                  a.CaseSession.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                                                                                         b.IsFinalDoc))
                                                                                      .Select(a => a.SessionResult.Label)
                                                                                      .FirstOrDefault(),
                                    CaseEndDate = x.Case.CaseSessionActs.Where(a => a.DateExpired == null &&
                                                                                    a.IsFinalDoc &&
                                                                                    a.ActDeclaredDate != null)
                                                                        .Select(a => a.ActDeclaredDate)
                                                                        .FirstOrDefault(),
                                    CasePersonName = x.FullName + "(" + x.PersonRole.Label + ")" + " " + (x.Uic ?? ""),
                                    PersonMaturityName = x.PersonMaturity.Label,
                                    SentenceTypeName = x.CasePersonSentences.Where(a => (a.IsActive ?? true) &&
                                                                                        a.DateExpired == null)
                                                                            .Select(a => string.Join(newLine, a.CasePersonSentencePunishments
                                                                                                               .Where(b => b.DateExpired == null)
                                                                                                               .Select(b => b.SentenceType.Label)))
                                                                            .FirstOrDefault(),
                                    SentenceTime = x.CasePersonSentences.Where(a => (a.IsActive ?? true) && a.DateExpired == null)
                                                                        .Select(a => string.Join(newLine, a.CasePersonSentencePunishments
                                                                                                           .Where(b => b.DateExpired == null)
                                                                                                           .Select(b => (((b.SentenceType.HasMoney ?? false) && b.SentenseMoney > (decimal)0.01) ? "Сума: " + b.SentenseMoney + "лв. " : string.Empty) +
                                                                                                                        ((b.SentenceType.HasPeriod ?? false) ? $"{b.SentenseDays} дни {b.SentenseWeeks} седмици {b.SentenseMonths} месеци {b.SentenseYears} години" : string.Empty))))
                                                                        .FirstOrDefault()
                                })
                                .AsQueryable();
        }

        /// <summary>
        /// Справка Съдени и осъдени лица
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<byte[]> CasePersonDefendantListReportExportExcel(CasePersonDefendantListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await CasePersonDefendantListReport_Select(model, Environment.NewLine).ToListAsync();

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
                    x => x.SessionResultNameFirst,
                    x => x.CaseEndDate,
                    x => x.CasePersonName,
                    x => x.PersonMaturityName,
                    x => x.SentenceTypeName,
                    x => x.SentenceTime,
                },
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Справка постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseFirstInstanceListReportVM> CaseFirstInstanceListReport_Select(CaseFirstInstanceListFilterReportVM filter)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (filter.DateFrom != null || filter.DateTo != null)
                dateSearch = x => x.RegDate >= filter.DateFrom && x.RegDate <= filter.DateTo;

            Expression<Func<Case, bool>> lifeCycleDateSearch = x => true;
            if (filter.LifeCycleDateFrom != null || filter.LifeCycleDateTo != null)
                lifeCycleDateSearch = x => x.CaseLifecycles.Any(l => l.DateExpired == null &&
                                                                     l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                     l.DateFrom >= filter.LifeCycleDateFrom &&
                                                                     l.DateFrom <= filter.LifeCycleDateTo &&
                                                                     l.Iteration > 1);

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == filter.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == filter.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeWhere = x => x.CaseCodeId == filter.CaseCodeId;

            Expression<Func<Case, bool>> documentTypeIdWhere = x => true;
            if (filter.DocumentTypeId > 0)
                documentTypeIdWhere = x => x.Document.DocumentTypeId == filter.DocumentTypeId;

            Expression<Func<Case, bool>> caseTypeUnitIdWhere = x => true;
            if (filter.CaseTypeUnitId > 0)
                caseTypeUnitIdWhere = x => x.CaseTypeUnitId == filter.CaseTypeUnitId;

            Expression<Func<Case, bool>> courtDepartmentIdWhere = x => true;
            if (filter.CourtDepartmentId > 0)
                courtDepartmentIdWhere = x => x.JudicalCompositionId == filter.CourtDepartmentId;

            Expression<Func<Case, bool>> processPriorityIdWhere = x => true;
            if (filter.ProcessPriorityId > 0)
                processPriorityIdWhere = x => x.ProcessPriorityId == filter.ProcessPriorityId;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                   (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                   a.LawUnitId == filter.JudgeReporterId &&
                                                                   a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            var caseMigrationQuery = readonlyrepo.AllReadonly<CaseMigration>();
            Expression<Func<Case, bool>> createFromWhere = x => true;
            if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.New)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && x.CaseLifecycles.Where(a => a.DateExpired == null &&
                                                                   a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                       .Count() == 1 &&
                                       caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                     a.DateExpired == null &&
                                                                     a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                         .OrderByDescending(a => a.Id)
                                                         .Select(a => a.CaseMigrationTypeId)
                                                         .FirstOrDefault() != NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction &&
                                        (caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                       a.DateExpired == null &&
                                                                       a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                       a.OutCaseMigrationId != null)
                                                           .OrderByDescending(a => a.Id)
                                                           .Select(a => a.OutCaseMigration.ReturnCaseId)
                                                           .FirstOrDefault() ?? x.Id) == x.Id;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.Jurisdiction)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                     a.DateExpired == null &&
                                                                     a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                         .OrderByDescending(a => a.Id)
                                                         .Select(a => a.CaseMigrationTypeId)
                                                         .FirstOrDefault() == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.NewNumber)
            {
                //тука става.....
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == true;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.OldNumber)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && x.CaseLifecycles.Where(a => a.DateExpired == null &&
                                                                   a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                       .Count() > 1;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.ReturnAfterFurtherInvestigation)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && caseMigrationQuery.Any(a => a.CaseId == x.Id &&
                                                                   a.DateExpired == null &&
                                                                   a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                   a.OutCaseMigrationId != null &&
                                                                   a.PriorCase.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                                     s.CaseSessionResults.Any(r => (r.SessionResultId == NomenclatureConstants.CaseSessionResult.StopProduction &&
                                                                                                                                   NomenclatureConstants.CaseSessionResultBase.FiledCasesFirstInstance.Contains(r.SessionResultBaseId ?? 0)) ||
                                                                                                                                   NomenclatureConstants.CaseSessionResult.FiledCasesFirstInstance.Contains(r.SessionResultId))));
            }

            return readonlyrepo.AllReadonly<Case>()
                       .Where(x => x.CourtId == userContext.CourtId &&
                                   x.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance &&
                                   !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Where(dateSearch)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseCodeWhere)
                       .Where(judgeReporterSearch)
                       .Where(createFromWhere)
                       .Where(lifeCycleDateSearch)
                       .Where(documentTypeIdWhere)
                       .Where(caseTypeUnitIdWhere)
                       .Where(processPriorityIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(courtDepartmentIdWhere)
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
                           LifeCycleCount = x.CaseLifecycles.Where(a => a.DateExpired == null &&
                                                                        a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                            .Count(),
                           IsNewCaseNewNumber = x.IsNewCaseNewNumber ?? false,
                           IsJurisdiction = caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                          a.DateExpired == null &&
                                                                          a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                              .OrderByDescending(a => a.Id)
                                                              .Select(a => a.CaseMigrationTypeId)
                                                              .FirstOrDefault() == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction,
                           migration = caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                     a.DateExpired == null &&
                                                                     a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                     a.OutCaseMigrationId != null)
                                                         .OrderByDescending(a => a.Id)
                                                         .Select(a => new CaseMigrationDataReportVM
                                                         {
                                                             MigrationId = a.Id,
                                                             CaseMigrationTypeId = a.CaseMigrationTypeId,
                                                             ReturnCaseId = a.OutCaseMigration.ReturnCaseId ?? 0
                                                         })
                                                         .FirstOrDefault(),
                           HaveOpenLifeCycle = x.CaseLifecycles.Any(l => l.DateExpired == null &&
                                                                         l.DateTo == null &&
                                                                         l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress),
                           DateFinishCase = x.CaseLifecycles.Where(l => l.DateExpired == null &&
                                                                        l.DateTo != null &&
                                                                        l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                            .OrderByDescending(l => l.Id)
                                                            .Select(l => l.DateTo != null ? (l.DateTo ?? DateTime.Now).Date : (DateTime?)null)
                                                            .FirstOrDefault(),
                           DateFromNewLifeCycle = x.CaseLifecycles.Where(l => l.DateExpired == null &&
                                                                              l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                              l.Iteration > 1)
                                                                  .Select(l => (DateTime?)l.DateFrom.ForceStartDate())
                                                                  .FirstOrDefault(),
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private async Task<(List<CaseFirstInstanceListReportVM> cases, int count)> CaseFirstInstanceListReportNew_Select(CaseFirstInstanceListFilterReportVM filter, bool getCount, int start, int length, List<DataTablesSortColumnVM> sortedColumns)
        {
            if (sortedColumns.Count == 0)
            {
                sortedColumns.Add(new DataTablesSortColumnVM()
                {
                    Name = "CaseId",
                    IsAscending = true
                });
            }

            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (filter.DateFrom != null || filter.DateTo != null)
                dateSearch = x => x.RegDate >= filter.DateFrom && x.RegDate <= filter.DateTo;

            Expression<Func<Case, bool>> lifeCycleDateSearch = x => true;
            if (filter.LifeCycleDateFrom != null || filter.LifeCycleDateTo != null)
                lifeCycleDateSearch = x => x.CaseLifecycles.Any(l => l.DateExpired == null &&
                                                                     l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                     l.DateFrom >= filter.LifeCycleDateFrom &&
                                                                     l.DateFrom <= filter.LifeCycleDateTo &&
                                                                     l.Iteration > 1);

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == filter.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == filter.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeWhere = x => x.CaseCodeId == filter.CaseCodeId;

            Expression<Func<Case, bool>> documentTypeIdWhere = x => true;
            if (filter.DocumentTypeId > 0)
                documentTypeIdWhere = x => x.Document.DocumentTypeId == filter.DocumentTypeId;

            Expression<Func<Case, bool>> caseTypeUnitIdWhere = x => true;
            if (filter.CaseTypeUnitId > 0)
                caseTypeUnitIdWhere = x => x.CaseTypeUnitId == filter.CaseTypeUnitId;

            Expression<Func<Case, bool>> courtDepartmentIdWhere = x => true;
            if (filter.CourtDepartmentId > 0)
                courtDepartmentIdWhere = x => x.JudicalCompositionId == filter.CourtDepartmentId;

            Expression<Func<Case, bool>> processPriorityIdWhere = x => true;
            if (filter.ProcessPriorityId > 0)
                processPriorityIdWhere = x => x.ProcessPriorityId == filter.ProcessPriorityId;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                   (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                   a.LawUnitId == filter.JudgeReporterId &&
                                                                   a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            var caseMigrationQuery = readonlyrepo.AllReadonly<CaseMigration>();
            Expression<Func<Case, bool>> createFromWhere = x => true;
            if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.New)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && x.CaseLifecycles.Where(a => a.DateExpired == null &&
                                                                   a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                       .Count() == 1 &&
                                       caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                     a.DateExpired == null &&
                                                                     a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                         .OrderByDescending(a => a.Id)
                                                         .Select(a => a.CaseMigrationTypeId)
                                                         .FirstOrDefault() != NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction &&
                                        (caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                       a.DateExpired == null &&
                                                                       a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                       a.OutCaseMigrationId != null)
                                                           .OrderByDescending(a => a.Id)
                                                           .Select(a => a.OutCaseMigration.ReturnCaseId)
                                                           .FirstOrDefault() ?? x.Id) == x.Id;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.Jurisdiction)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                     a.DateExpired == null &&
                                                                     a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                         .OrderByDescending(a => a.Id)
                                                         .Select(a => a.CaseMigrationTypeId)
                                                         .FirstOrDefault() == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.NewNumber)
            {
                //тука става.....
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == true;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.OldNumber)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && x.CaseLifecycles.Where(a => a.DateExpired == null &&
                                                                   a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                       .Count() > 1;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.ReturnAfterFurtherInvestigation)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && caseMigrationQuery.Any(a => a.CaseId == x.Id &&
                                                                   a.DateExpired == null &&
                                                                   a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                   a.OutCaseMigrationId != null &&
                                                                   a.PriorCase.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                                     s.CaseSessionResults.Any(r => (r.SessionResultId == NomenclatureConstants.CaseSessionResult.StopProduction &&
                                                                                                                                   NomenclatureConstants.CaseSessionResultBase.FiledCasesFirstInstance.Contains(r.SessionResultBaseId ?? 0)) ||
                                                                                                                                   NomenclatureConstants.CaseSessionResult.FiledCasesFirstInstance.Contains(r.SessionResultId))));
            }

            Expression<Func<Case, bool>> courtIdWhere = x => true;
            if (filter.CourtId == null)
                courtIdWhere = x => x.CourtId == userContext.CourtId;
            else
            {
                if (filter.CourtId > 0)
                    courtIdWhere = x => x.CourtId == filter.CourtId;
            }

            var query = readonlyrepo.AllReadonly<Case>()
                       .Where(x => x.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance &&
                                   !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Where(courtIdWhere)
                       .Where(dateSearch)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseCodeWhere)
                       .Where(judgeReporterSearch)
                       .Where(createFromWhere)
                       .Where(lifeCycleDateSearch)
                       .Where(documentTypeIdWhere)
                       .Where(caseTypeUnitIdWhere)
                       .Where(processPriorityIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(courtDepartmentIdWhere);

            int count = 0;
            if (getCount == true)
                count = await query.CountAsync();

            var cases = await readonlyrepo.AllReadonly<Case>()
                       .Where(x => x.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance &&
                                   !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Where(courtIdWhere)
                       .Where(dateSearch)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseCodeWhere)
                       .Where(judgeReporterSearch)
                       .Where(createFromWhere)
                       .Where(lifeCycleDateSearch)
                       .Where(documentTypeIdWhere)
                       .Where(caseTypeUnitIdWhere)
                       .Where(processPriorityIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(courtDepartmentIdWhere)
                       .Select(x => new CaseFirstInstanceListReportVM
                       {
                           CaseId = x.Id,
                           CourtLabel = x.Court.Label,
                           CaseTypeName = x.CaseType.Label,
                           CaseRegNumber = x.RegNumber,
                           CaseRegDate = x.RegDate,
                           JudgeReporterName = x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                         (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                         a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                             .Select(a => a.LawUnit.FullName)
                                                             .FirstOrDefault(),
                           CaseCodeName = (x.CaseCode.Code ?? "") + " " + x.CaseCode.Label,
                           IsNewCaseNewNumber = x.IsNewCaseNewNumber ?? false,
                       })
                       .OrderBy(sortedColumns)
                       .Skip(start)
                       .Take(length)
                       .ToListAsync();

            var caseIds = cases.Select(x => x.CaseId).ToArray();
            var lifeCycles = await readonlyrepo.AllReadonly<CaseLifecycle>()
                                   .Where(x => caseIds.Contains(x.CaseId) && x.DateExpired == null)
                                   .Select(x => new
                                   {
                                       id = x.Id,
                                       caseId = x.CaseId,
                                       lifecycleTypeId = x.LifecycleTypeId,
                                       iteration = x.Iteration,
                                       dateFrom = x.DateFrom,
                                       dateTo = x.DateTo,
                                   })
                                   .ToListAsync();

            var caseMigrations = await readonlyrepo.AllReadonly<CaseMigration>()
                                    .Where(x => caseIds.Contains(x.CaseId) && x.DateExpired == null)
                                    .Select(x => new
                                    {
                                        id = x.Id,
                                        caseId = x.CaseId,
                                        caseMigrationTypeMigrationDirection = x.CaseMigrationType.MigrationDirection,
                                        caseMigrationTypeId = x.CaseMigrationTypeId,
                                        outCaseMigrationId = x.OutCaseMigrationId,
                                        outCaseMigrationReturnCaseId = x.OutCaseMigration.ReturnCaseId,
                                    })
                                    .ToListAsync();

            foreach (var item in cases)
            {
                var oneLifeCycles = lifeCycles.Where(x => x.caseId == item.CaseId).ToList();
                item.LifeCycleCount = oneLifeCycles.Where(a => a.lifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress).Count();
                item.HaveOpenLifeCycle = oneLifeCycles.Any(l => l.dateTo == null && l.lifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress);
                item.DateFinishCase = oneLifeCycles.Where(l => l.dateTo != null && l.lifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                 .OrderByDescending(l => l.id)
                                 .Select(l => l.dateTo != null ? (l.dateTo ?? DateTime.Now).Date : (DateTime?)null)
                                 .FirstOrDefault();
                item.DateFromNewLifeCycle = oneLifeCycles.Where(l => l.lifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress && l.iteration > 1)
                                                        .Select(l => (DateTime?)l.dateFrom.ForceStartDate())
                                                        .FirstOrDefault();

                var oneCaseMigrations = caseMigrations.Where(x => x.caseId == item.CaseId).ToList();
                item.IsJurisdiction = oneCaseMigrations.Where(a => a.caseId == item.CaseId && a.caseMigrationTypeMigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                   .OrderByDescending(a => a.id)
                                                   .Select(a => a.caseMigrationTypeId)
                                                   .FirstOrDefault() == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction;

                item.migration = oneCaseMigrations.Where(a => a.caseId == item.CaseId && a.caseMigrationTypeMigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                          a.outCaseMigrationId != null)
                                              .OrderByDescending(a => a.id)
                                              .Select(a => new CaseMigrationDataReportVM
                                              {
                                                  MigrationId = a.id,
                                                  CaseMigrationTypeId = a.caseMigrationTypeId,
                                                  ReturnCaseId = a.outCaseMigrationReturnCaseId ?? 0
                                              })
                                              .FirstOrDefault();
            }

            return (cases, count);
        }

        private async Task<List<CaseFirstInstanceListReportVM>> CaseFirstInstanceListReportNewAll_Select(CaseFirstInstanceListFilterReportVM filter, List<DataTablesSortColumnVM> sortedColumns)
        {
            List<CaseFirstInstanceListReportVM> dataRows = new List<CaseFirstInstanceListReportVM>();
            List<CaseFirstInstanceListReportVM> getRows = new List<CaseFirstInstanceListReportVM>();
            int count = 0;

            int start = 0;
            int take = 100;
            (getRows, count) = await CaseFirstInstanceListReportNew_Select(filter, false, start, take, sortedColumns);
            dataRows.AddRange(getRows);

            while (getRows.Any())
            {
                start = start + take;
                (getRows, count) = await CaseFirstInstanceListReportNew_Select(filter, false, start, take, sortedColumns);
                dataRows.AddRange(getRows);
            }

            return dataRows;
        }

        public async Task<DataTableResponseVM<CaseFirstInstanceListReportVM>> CaseFirstInstanceListReportDataTable_Select(CaseFirstInstanceListFilterReportVM filter, int start, int length, List<DataTablesSortColumnVM> sortedColumns)
        {
            List<CaseFirstInstanceListReportVM> cases;
            int count = 0;
            //Всички записи
            if (length < 0)
            {
                cases = await CaseFirstInstanceListReportNewAll_Select(filter, sortedColumns);
                count = cases.Count;
            }
            else
            {
                (cases, count) = await CaseFirstInstanceListReportNew_Select(filter, true, start, length, sortedColumns);
            }

            var result = new DataTableResponseVM<CaseFirstInstanceListReportVM>
            {
                TotalCount = count,
                Records = cases,
            };

            return result;
        }

        /// <summary>
        /// Експорт Справка постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<byte[]> CaseFirstInstanceListReportExportExcel(CaseFirstInstanceListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await CaseFirstInstanceListReportNewAll_Select(model, new List<DataTablesSortColumnVM>());

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка постъпили дела за период – първоинстанционни дела", 6, styleTitle);
            excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseFirstInstanceListReportVM, object>>>()
                {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.CaseRegDate,
                    x => x.DateFromNewLifeCycleString,
                    x => x.DateFinishCaseString,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeName,
                    x => x.CaseCreateFromName,
                },
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Експорт Справка постъпили дела за период – първоинстанционни дела - със съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<byte[]> CaseFirstInstanceWithCourtListReportExportExcel(CaseFirstInstanceListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await CaseFirstInstanceListReportNewAll_Select(model, new List<DataTablesSortColumnVM>());

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка постъпили дела за период – първоинстанционни дела", 6, styleTitle);
            excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseFirstInstanceListReportVM, object>>>()
                {
                    x => x.CourtLabel,
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.CaseRegDate,
                    x => x.DateFromNewLifeCycleString,
                    x => x.DateFinishCaseString,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeName,
                    x => x.CaseCreateFromName,
                },
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSecondInstanceListReportVM> CaseSecondInstanceListReport_Select(CaseSecondInstanceListFilterReportVM filter)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);
            int courtType = userContext.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt ? NomenclatureConstants.CourtType.RegionalCourt : NomenclatureConstants.CourtType.DistrictCourt;

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (filter.DateFrom != null || filter.DateTo != null)
                dateSearch = x => x.RegDate >= filter.DateFrom && x.RegDate <= filter.DateTo;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == filter.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == filter.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeWhere = x => x.CaseCodeId == filter.CaseCodeId;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                   (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                   a.LawUnitId == filter.JudgeReporterId &&
                                                                   a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<Case, bool>> caseDepartmentWhere = x => true;
            if (filter.CourtDepartmentId > 0)
                caseDepartmentWhere = x => x.JudicalCompositionId == filter.CourtDepartmentId;

            Expression<Func<Case, bool>> fromCourtSearch = x => true;
            if (filter.FromCourtId > 0)
                fromCourtSearch = x => readonlyrepo.AllReadonly<CaseMigration>()
                                           .Where(a => a.CaseId == x.Id && a.DateExpired == null)
                                           .Where(a => a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                       a.PriorCase.Court.CourtTypeId == courtType)
                                           .Select(a => a.PriorCase.CourtId)
                                           .FirstOrDefault() == filter.FromCourtId ||
                                       x.Document.DocumentCaseInfo.Where(a => a.CaseId == null &&
                                                                              a.Court.CourtTypeId == courtType)
                                                                  .Select(a => a.CourtId)
                                                                  .FirstOrDefault() == filter.FromCourtId;

            var caseMigrationQuery = readonlyrepo.AllReadonly<CaseMigration>()
                                         .Where(m => m.DateExpired == null);

            Expression<Func<Case, bool>> createFromWhere = x => true;
            if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.New)
            {
                //Тука е....
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && x.CaseLifecycles.Where(a => a.DateExpired == null &&
                                                                   a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                       .Count() == 1 &&
                                       caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                     a.DateExpired == null &&
                                                                     a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                         .OrderByDescending(a => a.Id)
                                                         .Select(a => a.CaseMigrationTypeId)
                                                         .FirstOrDefault() != NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction &&
                                       caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                     a.DateExpired == null &&
                                                                     a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                         .OrderByDescending(a => a.Id)
                                                         .Select(a => a.CaseMigrationTypeId)
                                                         .FirstOrDefault() != NomenclatureConstants.CaseMigrationTypes.AcceptProsecutors &&
                                       (caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                      a.DateExpired == null &&
                                                                      a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                      a.OutCaseMigrationId != null)
                                                          .OrderByDescending(a => a.Id)
                                                          .Select(a => a.OutCaseMigration.ReturnCaseId)
                                                          .FirstOrDefault() ?? x.Id) == x.Id &&
                                                      x.CaseLifecycles.Any(a => a.CaseId == x.Id && a.DateExpired == null && a.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS) == false; 
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.Jurisdiction)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                     a.DateExpired == null &&
                                                                     a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                         .OrderByDescending(a => a.Id)
                                                         .Select(a => a.CaseMigrationTypeId)
                                                         .FirstOrDefault() == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction &&
                                       x.CaseLifecycles.Where(a => a.DateExpired == null &&
                                                                   a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                       .Count() == 1 &&
                                                      x.CaseLifecycles.Any(a => a.CaseId == x.Id && a.DateExpired == null && a.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS) == false;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.Prosecutors)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                     a.DateExpired == null &&
                                                                     a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                         .OrderByDescending(a => a.Id)
                                                         .Select(a => a.CaseMigrationTypeId)
                                                         .FirstOrDefault() == NomenclatureConstants.CaseMigrationTypes.AcceptProsecutors &&
                                       x.CaseLifecycles.Where(a => a.DateExpired == null &&
                                                                   a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                       .Count() == 1 &&
                                                      x.CaseLifecycles.Any(a => a.CaseId == x.Id && a.DateExpired == null && a.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS) == false;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.NewNumber)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == true &&
                                                      x.CaseLifecycles.Any(a => a.CaseId == x.Id && a.DateExpired == null && a.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS) == false;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.OldNumber)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && x.CaseLifecycles.Where(a => a.DateExpired == null &&
                                                                   a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                       .Count() > 1 &&
                                                      x.CaseLifecycles.Any(a => a.CaseId == x.Id && a.DateExpired == null && a.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS) == false;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.ReturnedAfterAdministration)
            {
                createFromWhere = x => (x.IsNewCaseNewNumber ?? false) == false && caseMigrationQuery.Any(a => a.CaseId == x.Id &&
                                                                   a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                   caseMigrationQuery.Where(m => m.InitialCaseId == a.InitialCaseId &&
                                                                                                 m.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                                                 m.Id < a.Id)
                                                                                     .OrderByDescending(m => m.Id)
                                                                                     .Select(m => m.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ForAdministration)
                                                                                     .FirstOrDefault()) &&
                                                               x.CaseLifecycles.Any(a => a.CaseId == x.Id && a.DateExpired == null && a.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS) == false;
            }
            else if (filter.CaseCreateFromId == NomenclatureConstants.CaseCreateFroms.AcceptedCh80)
            {
                createFromWhere = x => x.CaseLifecycles.Any(a => a.CaseId == x.Id && a.DateExpired == null && a.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS);
            }

            Expression<Func<Case, bool>> lifeCycleDateSearch = x => true;
            if (filter.LifeCycleDateFrom != null || filter.LifeCycleDateTo != null)
                lifeCycleDateSearch = x => x.CaseLifecycles.Any(l => l.DateExpired == null &&
                                                                     l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                     l.DateFrom >= filter.LifeCycleDateFrom &&
                                                                     l.DateFrom <= filter.LifeCycleDateTo &&
                                                                     l.Iteration > 1);

            Expression<Func<Case, bool>> caseTypeUnitIdWhere = x => true;
            if (filter.CaseTypeUnitId > 0)
                caseTypeUnitIdWhere = x => x.CaseTypeUnitId == filter.CaseTypeUnitId;

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            int[] instance = new int[] { NomenclatureConstants.CaseInstanceType.SecondInstance, NomenclatureConstants.CaseInstanceType.ThirdInstance };
            return readonlyrepo.AllReadonly<Case>()
                                .Where(x => x.CourtId == userContext.CourtId &&
                                            instance.Contains(x.CaseType.CaseInstanceId) &&
                                            !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                                .Where(dateSearch)
                                .Where(caseGroupWhere)
                                .Where(caseTypeWhere)
                                .Where(caseCodeWhere)
                                .Where(judgeReporterSearch)
                                .Where(createFromWhere)
                                .Where(fromCourtSearch)
                                .Where(lifeCycleDateSearch)
                                .Where(caseTypeUnitIdWhere)
                                .Where(caseCodeIdsWhere)
                                .Where(caseDepartmentWhere)
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
                                    LifeCycleCount = x.CaseLifecycles.Where(a => a.DateExpired == null &&
                                                                                 a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                                     .Count(),
                                    IsNewCaseNewNumber = x.IsNewCaseNewNumber ?? false,
                                    migration = caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                              a.DateExpired == null &&
                                                                             a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming)
                                                                  .OrderByDescending(a => a.Id)
                                                                  .Select(a => new CaseMigrationDataReportVM
                                                                  {
                                                                      MigrationId = a.Id,
                                                                      CaseMigrationTypeId = a.CaseMigrationTypeId,
                                                                      ReturnCaseId = a.OutCaseMigration.ReturnCaseId ?? 0
                                                                  })
                                                                  .FirstOrDefault(),
                                    OldLinkNumber = x.Document.DocumentCaseInfo.Where(a => a.CaseId == null &&
                                                                                           a.Court.CourtTypeId == courtType)
                                                                               .Select(a => a.Court.Label)
                                                                               .FirstOrDefault(),
                                    NewLinkNumber = caseMigrationQuery.Where(a => a.CaseId == x.Id &&
                                                                                  a.DateExpired == null &&
                                                                                  a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                                  a.PriorCase.Court.CourtTypeId == courtType)
                                                                      .Select(a => a.PriorCase.Court.Label)
                                                                      .FirstOrDefault(),
                                    DocumentTypeName = x.Document.DocumentType.Label,
                                    DateFinishCase = x.CaseLifecycles.Where(l => l.DateExpired == null &&
                                                                                 l.DateTo != null &&
                                                                                 l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                                                                     .OrderByDescending(l => l.Id)
                                                                     .Select(l => l.DateTo)
                                                                     .FirstOrDefault(),
                                    DateFromNewLifeCycle = x.CaseLifecycles.Any(l => l.DateExpired == null &&
                                                                                     l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                     l.Iteration > 1) ? x.CaseLifecycles.Where(l => l.DateExpired == null &&
                                                                                                                                    l.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress &&
                                                                                                                                    l.Iteration > 1)
                                                                                                                        .Select(l => l.DateFrom)
                                                                                                                        .FirstOrDefault() : (DateTime?)null,
                                    AcceptedCh80 = x.CaseLifecycles.Any(a => a.CaseId == x.Id && a.DateExpired == null && a.CaseMigration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptCase_ch80PAS),
                                })
                                .AsQueryable();
        }

        /// <summary>
        /// Експорт Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<byte[]> CaseSecondInstanceListReportExportExcel(CaseSecondInstanceListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await CaseSecondInstanceListReport_Select(model).ToListAsync();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка постъпили дела за период – въззивни/касационни дела", 8,
                      styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseSecondInstanceListReportVM, object>>>()
                {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.CaseRegDate,
                    x => x.DateFinishCaseString,
                    x => x.DateFromNewLifeCycleString,
                    x => x.JudgeReporterName,
                    x => x.CaseCodeName,
                    x => x.FromCourtName,
                    x => x.CaseCreateFromName,
                    x => x.DocumentTypeName,
                },
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        /// <summary>
        /// Справка Свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        public IQueryable<CaseFinishListReportVM> CaseFinishFirstInstanceListReport_Select(CaseFinishListFilterReportVM filter, string newLine)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);

            int[] caseGroupsGR_TR = { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade };

            Expression<Func<CaseLifecycle, bool>> dateSearch = x => true;
            if (filter.DateFrom != null || filter.DateTo != null)
                dateSearch = x => x.CaseSessionAct.ActDeclaredDate != null &&
                                  x.CaseSessionAct.ActDeclaredDate >= filter.DateFrom &&
                                  x.CaseSessionAct.ActDeclaredDate <= filter.DateTo;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CaseLifecycle, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CaseLifecycle, bool>> caseCodeWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeWhere = x => x.Case.CaseCodeId == filter.CaseCodeId;

            Expression<Func<CaseLifecycle, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.Case.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                        (a.DateTo ?? dateEnd) >= dateNow &&
                                                                        a.LawUnitId == filter.JudgeReporterId &&
                                                                        a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<CaseLifecycle, bool>> judgeReporterActSearch = x => true;
            if (filter.JudgeReporterFinalActId > 0)
                judgeReporterActSearch = x => x.CaseSessionAct.CaseSession.CaseLawUnits.Any(a => (a.DateTo ?? dateEnd) >= dateNow &&
                                                                                                 a.LawUnitId == filter.JudgeReporterFinalActId &&
                                                                                                 a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<CaseLifecycle, bool>> complainSearch = x => true;
            if (filter.ActComplainResultId > 0 && caseGroupsGR_TR.Contains(filter.CaseGroupId))
            {
                complainSearch = x => x.CaseSessionAct.ActComplainResultId == filter.ActComplainResultId;
            }

            Expression<Func<CaseLifecycle, bool>> sessionResultSearch = x => true;
            if (filter.SessionResultId > 0)
            {
                sessionResultSearch = x => x.CaseSessionAct.CaseSession.CaseSessionResults.Any(b => b.DateExpired == null &&
                                                                                                    b.SessionResultId == filter.SessionResultId);
            }

            Expression<Func<CaseLifecycle, bool>> documentTypeIdWhere = x => true;
            if (filter.DocumentTypeId > 0)
                documentTypeIdWhere = x => x.Case.Document.DocumentTypeId == filter.DocumentTypeId;

            Expression<Func<CaseLifecycle, bool>> caseTypeUnitIdWhere = x => true;
            if (filter.CaseTypeUnitId > 0)
                caseTypeUnitIdWhere = x => x.Case.CaseTypeUnitId == filter.CaseTypeUnitId;

            Expression<Func<CaseLifecycle, bool>> processPriorityIdWhere = x => true;
            if (filter.ProcessPriorityId > 0)
                processPriorityIdWhere = x => x.Case.ProcessPriorityId == filter.ProcessPriorityId;

            Expression<Func<CaseLifecycle, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            var resultFinishQuery = readonlyrepo.AllReadonly<SessionResultGrouping>()
                                                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                                                .Select(x => x.SessionResultId);

            Expression<Func<CaseLifecycle, bool>> caseDepartmentWhere = x => true;
            if (filter.CourtDepartmentId > 0)
                caseDepartmentWhere = x => x.Case.JudicalCompositionId == filter.CourtDepartmentId;

            Expression<Func<CaseLifecycle, bool>> courtIdWhere = x => true;
            if (filter.CourtId == null)
                courtIdWhere = x => x.Case.CourtId == userContext.CourtId;
            else
            {
                if (filter.CourtId > 0)
                    courtIdWhere = x => x.Case.CourtId == filter.CourtId;
            }

            return readonlyrepo.AllReadonly<CaseLifecycle>()
                               .Where(x => x.Case.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance &&
                                           x.DateExpired == null &&
                                           x.DateTo != null &&
                                           x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                               .Where(x => !x.Case.CaseLifecycles.Any(a => a.DateExpired == null &&
                                                                           a.Id > x.Id &&
                                                                           a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress))
                               .Where(x => x.Case.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                                           a.IsFinalDoc &&
                                                                           a.RegDate >= x.DateFrom))
                               .Where(x => x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                        s.DateFrom.Date >= x.DateFrom.Date &&
                                                                        s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                      resultFinishQuery.Contains(r.SessionResultId))))
                               .Where(courtIdWhere)
                               .Where(dateSearch)
                               .Where(caseGroupWhere)
                               .Where(caseTypeWhere)
                               .Where(caseCodeWhere)
                               .Where(judgeReporterSearch)
                               .Where(judgeReporterActSearch)
                               .Where(complainSearch)
                               .Where(sessionResultSearch)
                               .Where(documentTypeIdWhere)
                               .Where(caseTypeUnitIdWhere)
                               .Where(processPriorityIdWhere)
                               .Where(caseCodeIdsWhere)
                               .Where(caseDepartmentWhere)
                               .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                               .Select(x => new CaseFinishListReportVM
                               {
                                   CaseId = x.CaseId,
                                   CourtLabel = x.Case.Court.Label,
                                   CaseTypeName = x.Case.CaseType.Label,
                                   CaseRegNumber = x.Case.RegNumber,
                                   CaseRegDate = x.Case.RegDate,
                                   CaseInforcedDate = x.Case.CaseInforcedDate,
                                   JudgeReporterName = x.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                                      (a.DateTo ?? dateEnd) >= dateNow &&
                                                                                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                          .Select(a => a.LawUnit.FullName)
                                                                          .FirstOrDefault(),
                                   JudgeReporterFinalActName = x.CaseSessionAct.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= dateNow &&
                                                                                                                    a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                                                        .Select(a => a.LawUnit.FullName)
                                                                                                        .FirstOrDefault(),
                                   CaseCodeName = (x.Case.CaseCode.Code ?? "") + " " + x.Case.CaseCode.Label,
                                   ActComplainResultName = x.CaseSessionAct.ActComplainResult.Label,
                                   SessionResultName = string.Join(newLine, x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                                                       resultFinishQuery.Contains(a.SessionResultId) && a.IsMain)
                                                                                                                           .Select(a => a.SessionResult.Label)),
                                   SessionResultNameFirst = x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                                       resultFinishQuery.Contains(a.SessionResultId) && a.IsMain)
                                                                                                           .Select(a => a.SessionResult.Label)
                                                                                                           .FirstOrDefault(),
                                   SessionResultStopBaseName = string.Join(newLine, x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                                                               a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                                                                                                                   .Select(a => a.SessionResultBase.Label)),
                                   SessionResultStopBaseNameFirst = x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                                               a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                                                                                                   .Select(a => a.SessionResultBase.Label)
                                                                                                                   .FirstOrDefault(),
                                   CaseDateFinishDate = x.CaseSessionAct.ActDeclaredDate,
                                   CaseLifecycleMonths = x.Case.CaseLifecycles.Select(a => a.DurationMonths).Sum(),
                                   NumberMonths = NomenclatureConstants.CaseGroups.GrajdanskoTradeDelo.Contains(x.Case.CaseGroupId) ? ((Math.Abs(12 * ((x.DateTo ?? dateNow).Year - x.Case.RegDate.Year) + (x.DateTo ?? dateNow).Month - x.Case.RegDate.Month) +
                                                                                                                                       ((x.DateTo ?? dateNow).Day > x.Case.RegDate.Day ? 1 : 0)) == 0 ? 1 : (Math.Abs(12 * ((x.DateTo ?? dateNow).Year - x.Case.RegDate.Year) + (x.DateTo ?? dateNow).Month - x.Case.RegDate.Month) +
                                                                                                                                       ((x.DateTo ?? dateNow).Day > x.Case.RegDate.Day ? 1 : 0))) : 0,
                                   DateAnnouncedForResolution = x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                              s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                            r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution)) ?
                                                                x.Case.CaseSessions.Where(s => s.DateExpired == null &&
                                                                                               s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                             r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution))
                                                                                   .Select(s => s.DateFrom)
                                                                                   .FirstOrDefault() : (DateTime?)null
                               })
                               .AsQueryable();
        }

        /// <summary>
        /// Експорт Справка Свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<byte[]> CaseFinishFirstInstanceListReportExportExcel(CaseFinishListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await CaseFinishFirstInstanceListReport_Select(model, Environment.NewLine).ToListAsync();

            int[] caseGroupsGR_TR = { NomenclatureConstants.CaseGroups.GrajdanskoDelo, NomenclatureConstants.CaseGroups.Trade };

            int colCount = 10;
            if (model.CaseGroupId > 0)
            {
                if (caseGroupsGR_TR.Contains(model.CaseGroupId))
                    colCount = 9;
                else if (model.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                    colCount = 9;
            }
            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка свършени дела за период – първоинстанционни дела", colCount,
                      styleTitle); excelService.AddRow();

            int[] size = new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 };
            List<Expression<Func<CaseFinishListReportVM, object>>> listExpress = new List<Expression<Func<CaseFinishListReportVM, object>>>()
            {
                x => x.CaseTypeName,
                x => x.CaseRegNumber,
                x => x.CaseRegDate,
                x => x.CaseInforcedDate,
                x => x.DateAnnouncedForResolution,
                x => x.JudgeReporterName,
                x => x.JudgeReporterFinalActName,
                x => x.CaseCodeName,
                x => x.ActComplainResultName,
                x => x.SessionResultName,
                x => x.SessionResultStopBaseName,
                x => x.CaseDateFinishDateString,
                x => x.CaseLifecycleMonthsString,
                x => x.NumberMonthsText
            };

            if (dataRows.Any(x => !string.IsNullOrEmpty(x.CourtLabel)))
            {
                size = new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 };
                listExpress = new List<Expression<Func<CaseFinishListReportVM, object>>>()
                {
                    x => x.CourtLabel,
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.CaseRegDate,
                    x => x.CaseInforcedDate,
                    x => x.DateAnnouncedForResolution,
                    x => x.JudgeReporterName,
                    x => x.JudgeReporterFinalActName,
                    x => x.CaseCodeName,
                    x => x.ActComplainResultName,
                    x => x.SessionResultName,
                    x => x.SessionResultStopBaseName,
                    x => x.CaseDateFinishDateString,
                    x => x.CaseLifecycleMonthsString,
                    x => x.NumberMonthsText
                };
            }

            excelService.AddList(dataRows, size, listExpress,
                                 NPOI.HSSF.Util.HSSFColor.White.Index,
                                 NPOI.HSSF.Util.HSSFColor.White.Index,
                                 NPOI.HSSF.Util.HSSFColor.White.Index);

            return excelService.ToArray();
        }

        /// <summary>
        /// Справка свършени дела за период – въззивни/касационни дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="newLine">Символ за нов ред</param>
        /// <returns></returns>
        public IQueryable<CaseFinishListReportVM> CaseFinishSecondInstanceListReport_Select(CaseFinishListFilterReportVM filter, string newLine)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateNow = DateTime.Now;
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);
            int courtType = userContext.CourtTypeId == NomenclatureConstants.CourtType.DistrictCourt ? NomenclatureConstants.CourtType.RegionalCourt : NomenclatureConstants.CourtType.DistrictCourt;

            var caseMigrationQuery = readonlyrepo.AllReadonly<CaseMigration>();

            Expression<Func<CaseLifecycle, bool>> dateSearch = x => true;
            if (filter.DateFrom != null || filter.DateTo != null)
                dateSearch = x => x.CaseSessionAct.ActDeclaredDate != null &&
                                  x.CaseSessionAct.ActDeclaredDate >= filter.DateFrom &&
                                  x.CaseSessionAct.ActDeclaredDate <= filter.DateTo;

            Expression<Func<CaseLifecycle, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CaseLifecycle, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CaseLifecycle, bool>> fromCourtSearch = x => true;
            if (filter.InitialCourtId > 0)
                fromCourtSearch = x => caseMigrationQuery.Any(a => a.CaseId == x.CaseId && a.DateExpired == null &&
                                                                   a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                   a.PriorCase.Court.CourtTypeId == courtType &&
                                                                   a.PriorCase.CourtId == filter.InitialCourtId) ||
                                       x.Case.Document.DocumentCaseInfo.Any(a => a.CaseId == null &&
                                                                                 a.Court.CourtTypeId == courtType &&
                                                                                 a.CourtId == filter.InitialCourtId);

            Expression<Func<CaseLifecycle, bool>> caseCodeWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeWhere = x => x.Case.CaseCodeId == filter.CaseCodeId;

            Expression<Func<CaseLifecycle, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.Case.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                        (a.DateTo ?? dateEnd) >= dateNow &&
                                                                        a.LawUnitId == filter.JudgeReporterId &&
                                                                        a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<CaseLifecycle, bool>> judgeReporterActSearch = x => true;
            if (filter.JudgeReporterFinalActId > 0)
                judgeReporterActSearch = x => x.CaseSessionAct.CaseSession.CaseLawUnits.Any(a => (a.DateTo ?? dateEnd) >= dateNow &&
                                                                                                 a.LawUnitId == filter.JudgeReporterFinalActId &&
                                                                                                 a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<CaseLifecycle, bool>> sessionResultSearch = x => true;
            if (filter.SessionResultId > 0)
                sessionResultSearch = x => x.CaseSessionAct.CaseSession.CaseSessionResults.Any(b => b.DateExpired == null &&
                                                                                                    b.SessionResultId == filter.SessionResultId);

            Expression<Func<CaseLifecycle, bool>> caseTypeUnitIdWhere = x => true;
            if (filter.CaseTypeUnitId > 0)
                caseTypeUnitIdWhere = x => x.Case.CaseTypeUnitId == filter.CaseTypeUnitId;

            Expression<Func<CaseLifecycle, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            Expression<Func<CaseLifecycle, bool>> caseDepartmentWhere = x => true;
            if (filter.CourtDepartmentId > 0)
                caseDepartmentWhere = x => x.Case.JudicalCompositionId == filter.CourtDepartmentId;

            var resultFinishQuery = readonlyrepo.AllReadonly<SessionResultGrouping>()
                                                .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                                                .Select(x => x.SessionResultId);

            int[] instance = new int[] { NomenclatureConstants.CaseInstanceType.SecondInstance, NomenclatureConstants.CaseInstanceType.ThirdInstance };

            return readonlyrepo.AllReadonly<CaseLifecycle>()
                       .Where(x => x.CourtId == userContext.CourtId &&
                                   instance.Contains(x.Case.CaseType.CaseInstanceId) &&
                                   x.DateExpired == null &&
                                   x.DateTo != null &&
                                   x.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress)
                       .Where(x => !x.Case.CaseLifecycles.Any(a => a.DateExpired == null &&
                                                                   a.Id > x.Id &&
                                                                   a.LifecycleTypeId == NomenclatureConstants.LifecycleType.InProgress))
                       .Where(x => x.Case.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                                   a.IsFinalDoc &&
                                                                   a.RegDate >= x.DateFrom))
                       .Where(x => x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                s.DateFrom.Date >= x.DateFrom.Date &&
                                                                s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                              resultFinishQuery.Contains(r.SessionResultId))))
                       .Where(dateSearch)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseCodeWhere)
                       .Where(judgeReporterSearch)
                       .Where(judgeReporterActSearch)
                       .Where(fromCourtSearch)
                       .Where(sessionResultSearch)
                       .Where(caseTypeUnitIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(caseDepartmentWhere)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Select(x => new CaseFinishListReportVM
                       {
                           CaseId = x.CaseId,
                           CaseTypeName = x.Case.CaseType.Label,
                           CaseRegNumber = x.Case.RegNumber,
                           CaseRegDate = x.Case.RegDate,
                           CaseInforcedDate = x.Case.CaseInforcedDate,
                           DocumentTypeLabel = x.Case.Document.DocumentType.Label,
                           JudgeReporterName = x.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                              (a.DateTo ?? dateEnd) >= dateNow &&
                                                                              a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                  .Select(a => a.LawUnit.FullName)
                                                                  .FirstOrDefault(),
                           JudgeReporterFinalActName = x.CaseSessionAct.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= dateNow &&
                                                                                                            a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                                                .Select(a => a.LawUnit.FullName)
                                                                                                .FirstOrDefault(),
                           CaseCodeName = (x.Case.CaseCode.Code ?? "") + " " + x.Case.CaseCode.Label,
                           OldLinkNumber = x.Case.Document.DocumentCaseInfo.Where(a => a.CaseId == null &&
                                                                                       a.Court.CourtTypeId == courtType)
                                                                           .Select(a => a.Court.Label)
                                                                           .FirstOrDefault(),
                           NewLinkNumber = caseMigrationQuery.Where(a => a.CaseId == x.CaseId &&
                                                                         a.DateExpired == null &&
                                                                         a.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                                         a.PriorCase.Court.CourtTypeId == courtType)
                                                             .Select(a => a.PriorCase.Court.Label)
                                                             .FirstOrDefault(),
                           ActComplainResultName = x.CaseSessionAct.ActComplainResult.Label,
                           SessionResultName = string.Join(newLine, x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                                               resultFinishQuery.Contains(a.SessionResultId) && a.IsMain)
                                                                                                                   .Select(a => a.SessionResult.Label)),
                           SessionResultNameFirst = x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                               resultFinishQuery.Contains(a.SessionResultId) && a.IsMain)
                                                                                                   .Select(a => a.SessionResult.Label)
                                                                                                   .FirstOrDefault(),
                           SessionResultStopBaseName = string.Join(newLine, x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                                                       a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                                                                                                           .Select(a => a.SessionResultBase.Label)),
                           SessionResultStopBaseNameFirst = x.CaseSessionAct.CaseSession.CaseSessionResults.Where(a => a.DateExpired == null &&
                                                                                                                      a.SessionResult.SessionResultGroupId == NomenclatureConstants.CaseSessionResultGroups.Suspended)
                                                                                                          .Select(a => a.SessionResultBase.Label)
                                                                                                          .FirstOrDefault(),
                           CaseDateFinish = x.CaseSessionAct.ActDeclaredDate.DateToStr(FormattingConstant.NormalDateFormat),
                           CaseDateFinishDate = x.CaseSessionAct.ActDeclaredDate,
                           CaseLifecycleMonths = x.Case.CaseLifecycles.Select(a => a.DurationMonths).Sum(),
                           NumberMonths = NomenclatureConstants.CaseGroups.GrajdanskoTradeDelo.Contains(x.Case.CaseGroupId) ? ((Math.Abs(12 * ((x.DateTo ?? dateNow).Year - x.Case.RegDate.Year) + (x.DateTo ?? dateNow).Month - x.Case.RegDate.Month) +
                                                                                                                               ((x.DateTo ?? dateNow).Day > x.Case.RegDate.Day ? 1 : 0)) == 0 ? 1 : (Math.Abs(12 * ((x.DateTo ?? dateNow).Year - x.Case.RegDate.Year) + (x.DateTo ?? dateNow).Month - x.Case.RegDate.Month) +
                                                                                                                               ((x.DateTo ?? dateNow).Day > x.Case.RegDate.Day ? 1 : 0))) : 0,
                           DateAnnouncedForResolution = x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                      s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                    r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution)) ?
                                                        x.Case.CaseSessions.Where(s => s.DateExpired == null &&
                                                                                       s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                     r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution))
                                                                           .Select(s => s.DateFrom)
                                                                           .FirstOrDefault() : (DateTime?)null
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка свършени дела за период – въззивни/касационни дела в ексел
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public async Task<byte[]> CaseFinishSecondInstanceListReportExportExcel(CaseFinishListFilterReportVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = await CaseFinishSecondInstanceListReport_Select(model, Environment.NewLine).ToListAsync();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange("Справка свършени дела за период – въззивни/касационни дела", 11,
                      styleTitle); excelService.AddRow();


            excelService.AddList(
                dataRows,
                new int[] { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                new List<Expression<Func<CaseFinishListReportVM, object>>>()
                {
                    x => x.CaseTypeName,
                    x => x.CaseRegNumber,
                    x => x.CaseRegDate,
                    x => x.CaseInforcedDate,
                    x => x.DateAnnouncedForResolution,
                    x => x.DocumentTypeLabel,
                    x => x.JudgeReporterName,
                    x => x.JudgeReporterFinalActName,
                    x => x.InitialCourtName,
                    x => x.CaseCodeName,
                    x => x.ActComplainResultName,
                    x => x.SessionResultName,
                    x => x.SessionResultStopBaseName,
                    x => x.CaseDateFinish,
                    x => x.CaseLifecycleMonthsString,
                    x => x.NumberMonthsText
                },
                //NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                //NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        public byte[] CourtStatsReport(DateTime? date)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");

            var dataRows = repo.ExecuteProc<ReportCourtStatsVM>("public.report_court_stats_todate({0})", date).ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange($"Справка дейности по съд към {date:dd.MM.yyyy HH:mm:ss}", 6,
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

        public byte[] CourtReportGeneric()
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");

            var dataRows = repo.ExecuteProc<ReportCourtGenericVM>("public.report_court_generic()", 1).ToList();

            var styleTitle = excelService.CreateTitleStyle();
            excelService.AddRange($"Обобщени справки", 3, styleTitle); excelService.AddRow();

            excelService.AddList(
                dataRows,
                new int[] { 15000, 15000, 5000 },
                new List<Expression<Func<ReportCourtGenericVM, object>>>()
                {
                    x => x.Report,
                    x => x.Label,
                    x => x.Count
                },
                NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index,
                NPOI.HSSF.Util.HSSFColor.White.Index
            );

            return excelService.ToArray();
        }

        private void AddTextCountToExcelSessionPublic(NPoiExcelService excelService, string text, int count1, int count2)
        {
            excelService.colIndex = 0;
            excelService.AddRangeMoveCol(text, 3, 1);
            excelService.AddRangeMoveCol(count1.ToString(), 1, 1);
            excelService.AddRangeMoveCol(count2.ToString(), 1, 1);
            excelService.AddRow();
        }

        private void CaseSessionPublicReportRecapJudgeRow(NPoiExcelService excelService, List<CaseSessionPublicReportVM> dataRows, CaseSessionPublicFilterReportVM model, string name,
                    XSSFCellStyle styleRow)
        {
            excelService.AddRangeMoveCol(name, 3, 1, styleRow);
            excelService.AddRangeMoveCol(dataRows.Count.ToString(), 1, 1, styleRow);
            excelService.AddRangeMoveCol("0", 1, 1, styleRow);

            if (model.InstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance && model.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                //Отложени (общо)
                excelService.AddRangeMoveCol(dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Procrastination)).Count().ToString(),
                                               1, 1, styleRow);

                //отложени В т.ч. в първо с.з.
                excelService.AddRangeMoveCol(dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Procrastination) &&
                                                 x.SessionTypeId == NomenclatureConstants.SessionType.FirstSession &&
                                  x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.ProcrastinationFirstSession)).Count().ToString(),
                                               1, 1, styleRow);

                //Спрени
                excelService.AddRangeMoveCol(dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Stop)).Count().ToString(),
                                               1, 1, styleRow);

                //Общо свършени
                excelService.AddRangeMoveCol(dataRows.Where(x => x.IsFinishSession).Count().ToString(),
                                               1, 1, styleRow);

                //Решени
                excelService.AddRangeMoveCol(dataRows.Where(Finish_Where()).Count().ToString(),
                                               1, 1, styleRow);

                //Прекратени общо
                excelService.AddRangeMoveCol(dataRows.Where(Suspended_Where()).Count().ToString(),
                                               1, 1, styleRow);

                //Прекратени по спогодба
                excelService.AddRangeMoveCol(dataRows.Where(Suspended_Where())
                                 .Where(x => x.SessionResultIds.Contains(NomenclatureConstants.CaseSessionResult.Agreement)).Count().ToString(),
                                               1, 1, styleRow);

                //Общо свършени до 3 месеца
                excelService.AddRangeMoveCol(dataRows.Where(x => x.IsFinishSession && x.DurationMonths < 3).Count().ToString(),
                                               1, 1, styleRow);

                //Общо свършени над 3 месеца
                excelService.AddRangeMoveCol(dataRows.Where(x => x.IsFinishSession && x.DurationMonths >= 3).Count().ToString(),
                                               1, 1, styleRow);
            }
            else
            {
                //Отложени
                excelService.AddRangeMoveCol(dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Procrastination)).Count().ToString(),
                                               1, 1, styleRow);

                //Спрени
                excelService.AddRangeMoveCol(dataRows.Where(x => x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Stop)).Count().ToString(),
                                               1, 1, styleRow);

                //Общо свършени
                excelService.AddRangeMoveCol(dataRows.Where(x => x.IsFinishSession).Count().ToString(),
                                               1, 1, styleRow);

                //Решени
                excelService.AddRangeMoveCol(dataRows.Where(Finish_Where()).Count().ToString(),
                                               1, 1, styleRow);

                //Прекратени 
                excelService.AddRangeMoveCol(dataRows.Where(Suspended_Where()).Count().ToString(),
                                               1, 1, styleRow);

                //Общо свършени до 3 месеца
                excelService.AddRangeMoveCol(dataRows.Where(x => x.IsFinishSession && x.DurationMonths < 3).Count().ToString(),
                                               1, 1, styleRow);

                //Общо свършени над 3 месеца
                excelService.AddRangeMoveCol(dataRows.Where(x => x.IsFinishSession && x.DurationMonths >= 3).Count().ToString(),
                                               1, 1, styleRow);
            }
        }

        private void CaseSessionPublicReportRecapJudge(NPoiExcelService excelService, List<CaseSessionPublicReportVM> dataRows, CaseSessionPublicFilterReportVM model)
        {
            excelService.rowIndex += 5;
            var style = SetStyle(excelService, NPOI.HSSF.Util.HSSFColor.White.Index);
            var styleRow = SetStyleRow(excelService, NPOI.HSSF.Util.HSSFColor.White.Index);

            if (model.InstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance && model.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                excelService.colIndex = 0;
                excelService.AddRangeMoveCol("Съдия", 3, 2, style);
                excelService.AddRangeMoveCol("Насрочени дела", 2, 1, style);
                excelService.AddRangeMoveCol("Отложени дела", 2, 1, style);
                excelService.AddRangeMoveCol("Спрени дела", 1, 2, style);
                excelService.AddRangeMoveCol("Общо свършени", 1, 2, style);
                excelService.AddRangeMoveCol("Решени дела", 1, 2, style);
                excelService.AddRangeMoveCol("Прекратени дела", 2, 1, style);
                excelService.AddRangeMoveCol("Общо свършени", 2, 1, style);
                excelService.rowIndex++;
                excelService.colIndex = 3;
                excelService.AddRangeMoveCol("Общо", 1, 1, style);
                excelService.AddRangeMoveCol("В т.ч. друго заседание", 1, 1, style);
                excelService.AddRangeMoveCol("Общо", 1, 1, style);
                excelService.AddRangeMoveCol("В т.ч. в първо с.з.", 1, 1, style);
                excelService.colIndex += 3;
                excelService.AddRangeMoveCol("Общо", 1, 1, style);
                excelService.AddRangeMoveCol("В т.ч.по спогодба", 1, 1, style);
                excelService.AddRangeMoveCol("До 3 мес.", 1, 1, style);
                excelService.AddRangeMoveCol("Над 3 мес.", 1, 1, style);
                excelService.AddRow();
                excelService.colIndex = 0;
            }
            else
            {
                excelService.colIndex = 0;
                excelService.AddRangeMoveCol("Съдия", 3, 2, style);
                excelService.AddRangeMoveCol("Насрочени дела", 2, 1, style);
                excelService.AddRangeMoveCol("Отложени дела", 1, 2, style);
                excelService.AddRangeMoveCol("Спрени дела", 1, 2, style);
                excelService.AddRangeMoveCol("Свършени", 5, 1, style);
                excelService.rowIndex++;
                excelService.colIndex = 3;
                excelService.AddRangeMoveCol("Общо", 1, 1, style);
                excelService.AddRangeMoveCol("В т.ч. друго заседание", 1, 1, style);
                excelService.colIndex += 2;
                excelService.AddRangeMoveCol("Общо", 1, 1, style);
                excelService.AddRangeMoveCol("Решени", 1, 1, style);
                excelService.AddRangeMoveCol("Прекратени", 1, 1, style);
                excelService.AddRangeMoveCol("До 3 мес.", 1, 1, style);
                excelService.AddRangeMoveCol("Над 3 мес.", 1, 1, style);
                excelService.AddRow();
                excelService.colIndex = 0;
            }

            var allJudge = dataRows
                           .GroupBy(x => new
                           {
                               x.JudgeReporterId,
                               x.JudgeReporterName
                           })
                           .Select(x => new
                           {
                               id = x.Key.JudgeReporterId,
                               name = x.Key.JudgeReporterName
                           })
                           .OrderBy(x => x.name)
                           .ToList();

            for (int i = 0; i < allJudge.Count; i++)
            {
                var dataRowsJudge = dataRows.Where(x => x.JudgeReporterId == allJudge[i].id).ToList();

                CaseSessionPublicReportRecapJudgeRow(excelService, dataRowsJudge, model, allJudge[i].name, styleRow);

                excelService.AddRow();
            }

            CaseSessionPublicReportRecapJudgeRow(excelService, dataRows, model, "Общо", styleRow);

            excelService.AddRow();
        }

        private Func<CaseSessionPublicReportVM, bool> Finish_Where()
        {
            Func<CaseSessionPublicReportVM, bool> dataWhere = x => x.IsFinishSession &&
            x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Suspended) == false;

            return dataWhere;
        }

        private Func<CaseSessionPublicReportVM, bool> Suspended_Where()
        {
            Func<CaseSessionPublicReportVM, bool> dataWhere = x => x.IsFinishSession &&
            x.SessionResultGroupIds.Contains(NomenclatureConstants.CaseSessionResultGroups.Suspended);

            return dataWhere;
        }

        private List<string> GetAllVisibleColumns(object obj, string columns)
        {
            columns = columns.ToLower();
            return obj.GetType().GetProperties().Where(x => columns.Contains("|name:" + x.Name.ToLower() + "|")).Select(x => x.Name).ToList();
        }

        /// <summary>
        /// Справка за съдебни актове
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSessionActReportVM> ActReport_Select(CaseSessionActReportFilterVM filter)
        {
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);
            filter.ActInforcedDateFrom = filter.ActInforcedDateFrom.ForceStartDate();
            filter.ActInforcedDateTo = filter.ActInforcedDateTo.ForceEndDate();

            DateTime dateNow = DateTime.Now;

            Expression<Func<CaseSessionAct, bool>> whereRegDate = x => true;
            if (filter.DateFrom != null || filter.DateTo != null)
                whereRegDate = x => x.RegDate >= filter.DateFrom && x.RegDate <= (filter.DateTo);

            Expression<Func<CaseSessionAct, bool>> whereInforceDate = x => true;
            if (filter.ActInforcedDateFrom != null || filter.ActInforcedDateTo != null)
                whereInforceDate = x => x.ActInforcedDate >= filter.ActInforcedDateFrom && x.ActInforcedDate <= filter.ActInforcedDateTo;

            Expression<Func<CaseSessionAct, bool>> caseGroupIdWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupIdWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CaseSessionAct, bool>> caseTypeIdWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeIdWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CaseSessionAct, bool>> caseCodeIdWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeIdWhere = x => x.Case.CaseCodeId == filter.CaseCodeId;

            Expression<Func<CaseSessionAct, bool>> processPriorityIdWhere = x => true;
            if (filter.ProcessPriorityId > 0)
                processPriorityIdWhere = x => x.Case.ProcessPriorityId == filter.ProcessPriorityId;

            Expression<Func<CaseSessionAct, bool>> actTypeIdWhere = x => true;
            if (filter.ActTypeId > 0)
                actTypeIdWhere = x => x.ActTypeId == filter.ActTypeId;

            Expression<Func<CaseSessionAct, bool>> actStateIdWhere = x => true;
            if (filter.ActStateId > 0)
                actStateIdWhere = x => x.ActStateId == filter.ActStateId;

            Expression<Func<CaseSessionAct, bool>> documentGroupIdWhere = x => true;
            if (filter.DocumentGroupId > 0)
                documentGroupIdWhere = x => x.Case.Document.DocumentGroupId == filter.DocumentGroupId;

            Expression<Func<CaseSessionAct, bool>> documentTypeIdWhere = x => true;
            if (filter.DocumentTypeId > 0)
                documentTypeIdWhere = x => x.Case.Document.DocumentTypeId == filter.DocumentTypeId;

            Expression<Func<CaseSessionAct, bool>> actComplainResultIdWhere = x => true;
            if (filter.ActComplainResultId > 0)
                actComplainResultIdWhere = x => x.ActComplainResultId == filter.ActComplainResultId;

            Expression<Func<CaseSessionAct, bool>> sessionResultIdWhere = x => true;
            if (filter.SessionResultId > 0)
                sessionResultIdWhere = x => x.CaseSession.CaseSessionResults.Any(r => r.SessionResultId == filter.SessionResultId &&
                                                                                      r.DateExpired == null);

            Expression<Func<CaseSessionAct, bool>> judgeReporterIdWhere = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterIdWhere = x => x.CaseSession.CaseLawUnits.Any(a => (a.DateTo ?? dateNow.AddYears(100)).Date >= x.CaseSession.DateFrom.Date &&
                                                                                a.LawUnitId == filter.JudgeReporterId &&
                                                                                a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            Expression<Func<CaseSessionAct, bool>> finalActSearch = x => true;
            if (filter.IsFinalDoc == true)
                finalActSearch = x => x.IsFinalDoc == true && NomenclatureConstants.SessionActState.EnforcedStates.Contains(x.ActStateId);

            Expression<Func<CaseSessionAct, bool>> canAppealSearch = x => true;
            if (filter.CanAppeal == true)
                canAppealSearch = x => x.CanAppeal == true;

            Expression<Func<CaseSessionAct, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            return readonlyrepo.AllReadonly<CaseSessionAct>()
                       .Where(whereRegDate)
                       .Where(whereInforceDate)
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(caseCodeIdWhere)
                       .Where(processPriorityIdWhere)
                       .Where(actTypeIdWhere)
                       .Where(actStateIdWhere)
                       .Where(documentGroupIdWhere)
                       .Where(documentTypeIdWhere)
                       .Where(actComplainResultIdWhere)
                       .Where(sessionResultIdWhere)
                       .Where(judgeReporterIdWhere)
                       .Where(finalActSearch)
                       .Where(canAppealSearch)
                       .Where(caseCodeIdsWhere)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null) &&
                                   x.CourtId == userContext.CourtId &&
                                   x.DateExpired == null)
                       .Select(x => new CaseSessionActReportVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId ?? 0,
                           ActRegNumYear = x.RegNumber + "/" + (x.RegDate ?? dateNow).Year + "г.",
                           ActTypeLabel = x.ActType.Label,
                           RegDate = x.RegDate,
                           ReturnDate = x.ActDeclaredDate,
                           ActInforcedDate = x.ActInforcedDate,
                           CaseTypeLabel = x.Case.CaseType.Code,
                           CaseNumber = x.Case.RegNumber,
                           DocumentInfo = x.Case.Document.DocumentType.Label + " " + x.Case.Document.DocumentNumber + "/" + x.Case.Document.DocumentDate.ToString("dd.MM.yyyy"),
                           DocumentDate = x.Case.Document.DocumentDate,
                           ActStateName = x.ActState.Label,
                           JudgeReport = x.CaseSession.CaseLawUnits.Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                               (l.DateTo ?? dateNow.AddYears(100)) >= x.CaseSession.DateFrom)
                                                                   .OrderByDescending(l => l.DateFrom)
                                                                   .Select(l => l.LawUnit.FullName + ((l.CourtDepartment != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                                   .FirstOrDefault(),
                           ActComplainResultLabel = x.ActComplainResultId != null ? x.ActComplainResult.Label : string.Empty,
                           DateAnnouncementDecision = x.CaseSession.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution) ? x.CaseSession.DateFrom : (DateTime?)null
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка за Дела с ненаписани съдебни актове от всички съдии
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseWithoutFinalAct_Select(CaseFilterReport filter)
        {
            List<CaseSprVM> result = new List<CaseSprVM>();

            DateTime dateNow = DateTime.Now;
            filter.DateFromNew = filter.DateFromNew.ForceStartDateWithAddYear(-100);
            filter.DateToNew = filter.DateToNew.ForceEndDateWithAddYear(100);
            filter.SessionWithResultAnnouncedForResolutionDateFrom = NomenclatureExtensions.ForceStartDate(filter.SessionWithResultAnnouncedForResolutionDateFrom);
            filter.SessionWithResultAnnouncedForResolutionDateTo = NomenclatureExtensions.ForceEndDate(filter.SessionWithResultAnnouncedForResolutionDateTo);
            var dateAddYear = DateTime.Now.AddYears(100);

            var resultFinishQuery = readonlyrepo.AllReadonly<SessionResultGrouping>()
                                        .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                                        .Select(x => x.SessionResultId);

            Expression<Func<Case, bool>> caseGroupIdWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupIdWhere = x => x.CaseGroupId == filter.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeIdWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeIdWhere = x => x.CaseTypeId == filter.CaseTypeId;

            Expression<Func<Case, bool>> caseStateIdWhere = x => true;
            if (filter.CaseStateId > 0)
                caseStateIdWhere = x => x.CaseStateId == filter.CaseStateId;

            Expression<Func<Case, bool>> sessionWithResultAnnouncedForResolutionWhere = x => true;
            if (filter.SessionWithResultAnnouncedForResolutionDateFrom != null && filter.SessionWithResultAnnouncedForResolutionDateTo != null)
            {
                sessionWithResultAnnouncedForResolutionWhere = x => x.CaseSessions.Any(s => s.DateFrom >= filter.SessionWithResultAnnouncedForResolutionDateFrom &&
                                                                                            s.DateFrom <= filter.SessionWithResultAnnouncedForResolutionDateTo &&
                                                                                            s.DateExpired == null &&
                                                                                            s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                          r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution));
            }

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                   (a.DateTo ?? dateAddYear).Date >= dateNow &&
                                                                   a.LawUnitId == filter.JudgeReporterId &&
                                                                   a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            return readonlyrepo.AllReadonly<Case>()
                       .Where(x => (x.CourtId == userContext.CourtId) &&
                                   ((x.RegDate >= filter.DateFromNew) && (x.RegDate <= filter.DateToNew)) &&
                                   (x.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                   (((x.CaseSessionResults.Where(r => r.DateExpired == null && resultFinishQuery.Contains(r.SessionResultId)).Any() && (!x.CaseSessionActs.Any(a => a.DateExpired == null && a.IsFinalDoc))) ||
                                    (x.CaseSessionResults.Where(r => r.DateExpired == null && resultFinishQuery.Contains(r.SessionResultId)).Any() && (x.CaseSessionActs.Any(a => a.DateExpired == null && a.ActDeclaredDate == null && a.IsFinalDoc))) ||
                                    (!x.CaseSessionResults.Where(r => r.DateExpired == null && resultFinishQuery.Contains(r.SessionResultId)).Any() && (x.CaseSessionActs.Any(a => a.DateExpired == null && a.IsFinalDoc && a.ActDeclaredDate == null)))) &&
                                    (!x.CaseSessions.Any(s => s.CaseSessionResults.Any(r => r.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution &&
                                                                                            r.Case.CaseSessions.Any(d => d.DateFrom > s.DateFrom &&
                                                                                                                         d.CaseSessionResults.Any(g => NomenclatureConstants.CaseSessionResult.CaseWithoutFinalAct.Contains(g.SessionResultId))))))))
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(judgeReporterSearch)
                       .Where(caseStateIdWhere)
                       .Where(sessionWithResultAnnouncedForResolutionWhere)
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.Id,
                           JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == null &&
                                                                   l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                   (l.DateTo ?? dateAddYear).Date >= dateNow)
                                                       .OrderByDescending(l => l.DateFrom)
                                                       .Select(l => l.LawUnit.FullName + ((l.CourtDepartment != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                       .FirstOrDefault(),
                           CaseTypeLabel = x.CaseType.Label,
                           CaseRegNum = x.RegNumber,
                           CaseCodeLabel = x.CaseCode.Code,
                           CaseRegDate = x.RegDate,
                           CaseEndDate = null,
                           SessionDateFrom = x.CaseSessions.Where(s => s.DateExpired == null).OrderByDescending(s => s.DateFrom).Select(s => s.DateFrom).FirstOrDefault()
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за Информация за страни
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CasePersonReportVM> CasePersonInformation_Select(CasePersonFilterVM filter)
        {
            var resultFinishQuerry = repo.AllReadonly<SessionResultGrouping>()
                                         .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                                         .Select(x => x.SessionResultId);

            filter.DateFrom = NomenclatureExtensions.ForceStartDate(filter.DateFrom);
            filter.DateTo = NomenclatureExtensions.ForceEndDate(filter.DateTo);
            filter.FinalDateFrom = NomenclatureExtensions.ForceStartDate(filter.FinalDateFrom);
            filter.FinalDateTo = NomenclatureExtensions.ForceEndDate(filter.FinalDateTo);
            filter.WithoutFinalDateTo = NomenclatureExtensions.ForceEndDate(filter.WithoutFinalDateTo);

            filter.Uic = !string.IsNullOrEmpty(filter.Uic) ? filter.Uic.Replace(" ", "") : filter.Uic;

            Expression<Func<CasePerson, bool>> caseGroupIdWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupIdWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CasePerson, bool>> caseTypeIdWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeIdWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CasePerson, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int?[] caseCodeIds = filter.CaseCodeIds.Select(x => (int?)int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId);
            }

            Expression<Func<CasePerson, bool>> uicSearch = x => true;
            if (!string.IsNullOrEmpty(filter.Uic))
                uicSearch = x => x.Uic == filter.Uic;

            Expression<Func<CasePerson, bool>> nameSearch = x => true;
            if (!string.IsNullOrEmpty(filter.FullName))
                nameSearch = x => EF.Functions.ILike(x.FullName, filter.FullName.ToPaternSearch());

            Expression<Func<CasePerson, bool>> caseNumberSearch = x => true;
            if (!string.IsNullOrEmpty(filter.CaseRegNumber))
                caseNumberSearch = x => EF.Functions.ILike(x.Case.RegNumber, filter.CaseRegNumber.ToCasePaternSearch());

            Expression<Func<CasePerson, bool>> caseRegDateSearch = x => true;
            if ((filter.DateFrom != null) && (filter.DateTo != null))
                caseRegDateSearch = x => x.Case.RegDate >= filter.DateFrom && x.Case.RegDate <= filter.DateTo;

            Expression<Func<CasePerson, bool>> withFinalActSearch = x => true;
            if ((filter.FinalDateFrom != null) && (filter.FinalDateTo != null))
                withFinalActSearch = x => ((x.Case.CaseSessions.Any(a => a.DateExpired == null &&
                                                                         a.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                                                    (b.ActDeclaredDate >= filter.FinalDateFrom &&
                                                                                                    b.ActDeclaredDate <= filter.FinalDateTo) && b.IsFinalDoc &&
                                                                                                    (b.ActStateId != NomenclatureConstants.SessionActState.Project && b.ActStateId != NomenclatureConstants.SessionActState.Registered) &&
                                                                                                    b.CaseSession.CaseSessionResults.Any(r => r.DateExpired == null && resultFinishQuerry.Contains(r.SessionResultId))))));

            Expression<Func<CasePerson, bool>> withoutFinalActSearch = x => true;
            if (filter.WithoutFinalDateTo != null)
                withoutFinalActSearch = x => ((x.Case.RegDate <= filter.WithoutFinalDateTo) && ((!x.Case.CaseSessions.Any(a => a.DateExpired == null &&
                                                                                                                               a.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                                                                                                          (b.ActDeclaredDate != null &&
                                                                                                                                                          b.ActDeclaredDate <= filter.WithoutFinalDateTo) && b.IsFinalDoc &&
                                                                                                                                                          (b.ActStateId != NomenclatureConstants.SessionActState.Project && b.ActStateId != NomenclatureConstants.SessionActState.Registered) &&
                                                                                                                                                          b.CaseSession.CaseSessionResults.Any(r => r.DateExpired == null && resultFinishQuerry.Contains(r.SessionResultId)))))));

            return repo.AllReadonly<CasePerson>()
                       .Where(x => x.Case.CourtId == userContext.CourtId &&
                                   x.CaseSessionId == null &&
                                   x.DateExpired == null)
                       .Where(uicSearch)
                       .Where(nameSearch)
                       .Where(caseNumberSearch)
                       .Where(caseRegDateSearch)
                       .Where(withFinalActSearch)
                       .Where(withoutFinalActSearch)
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Select(x => new CasePersonReportVM()
                       {
                           CaseId = x.CaseId,
                           CaseNumber = x.Case.RegNumber,
                           CaseDate = x.Case.RegDate,
                           Uic = x.Uic,
                           FullName = x.FullName,
                           RoleName = x.PersonRole.Label,
                           CaseStateLabel = x.Case.CaseState.Label,
                           CaseTypeLabel = x.Case.CaseType.Code,
                           CaseCodeLabel = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label,
                           JudgeReport = x.Case.CaseLawUnits.Where(a => a.DateTo == null &&
                                                                        a.CaseSessionId == null &&
                                                                        a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                            .Select(a => a.LawUnit.FullName)
                                                            .FirstOrDefault(),
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Справка документи, постъпили чрез ЕЕСПП
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<DocumentsReceivedEESPPReportVM> DocumentsReceivedEESPP_Select(DocumentsReceivedEESPPFilterVM filter)
        {
            filter.DateReturnedFrom = filter.DateReturnedFrom.ForceStartDateWithAddYear(-100);
            filter.DateReturnedTo = filter.DateReturnedTo.ForceEndDateWithAddYear(100);

            Expression<Func<CaseLawyerHelpAssignedLawyer, bool>> whereDateReturned = x => x.DateReturned >= filter.DateReturnedFrom &&
                                                                                          x.DateReturned <= filter.DateReturnedTo;

            return repo.AllReadonly<CaseLawyerHelpAssignedLawyer>()
                       .Where(x => !x.CaseLawyerHelp.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseLawyerHelp.CaseId && d.DateExpired == null) &&
                                   x.CaseLawyerHelp.Case.CourtId == userContext.CourtId &&
                                   x.CaseLawyerHelp.DateExpired == null &&
                                   x.LawyerStateId == NomenclatureConstants.EesppLawyerState.Confirmed)
                       .Where(whereDateReturned)
                       .Select(x => new DocumentsReceivedEESPPReportVM()
                       {
                           CaseId = x.CaseLawyerHelp.CaseId,
                           CaseNumber = x.CaseLawyerHelp.Case.RegNumber,
                           DateReturned = x.DateReturned,
                           Persons = string.Join(", ", x.Persons.Select(p => p.CaseLawyerHelpPerson.CasePerson.FullName + " (" + p.CaseLawyerHelpPerson.CasePerson.PersonRole.Label + ")")),
                           Person = x.Persons
                                     .Select(p => p.CaseLawyerHelpPerson.CasePerson.FullName)
                                     .FirstOrDefault(),
                           LawyerHelpTypeLabel = x.CaseLawyerHelp.LawyerHelpType.Label,
                           LawyerInfo = x.LawyerNumber + " " + x.LawyerName
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за движение на дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<MigrationReportVM> MigrationReport_Select(MigrationReportFilterVM filter)
        {
            filter.DocumentRegDateFrom = filter.DocumentRegDateFrom.ForceStartDateWithAddYear(-100);
            filter.DocumentRegDateTo = filter.DocumentRegDateTo.ForceEndDateWithAddYear(100);
            filter.MigrationDateWrtFrom = filter.MigrationDateWrtFrom.ForceStartDate();
            filter.MigrationDateWrtTo = filter.MigrationDateWrtTo.ForceEndDate();

            Expression<Func<DocumentTemplate, bool>> whereDocumentRegDate = d => d.Document.DocumentDate >= filter.DocumentRegDateFrom &&
                                                                                 d.Document.DocumentDate <= filter.DocumentRegDateTo;

            var documentTemplateQuery = repo.AllReadonly<DocumentTemplate>()
                                            .Where(d => d.DateExpired == null &&
                                                        d.SourceType == SourceTypeSelectVM.CaseMigration &&
                                                        d.Document.DateExpired == null)
                                            .Where(whereDocumentRegDate);

            Expression<Func<CaseMigration, bool>> whereMigrationDateWrt = m => true;
            if (filter.MigrationDateWrtFrom != null && filter.MigrationDateWrtTo != null)
                whereMigrationDateWrt = m => m.DateWrt >= filter.MigrationDateWrtFrom &&
                                             m.DateWrt <= filter.MigrationDateWrtTo;

            var caseMigrationInput = repo.AllReadonly<CaseMigration>()
                                         .Where(m => m.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming &&
                                                     m.CourtId == userContext.CourtId)
                                         .Where(whereMigrationDateWrt);

            Expression<Func<CaseMigration, bool>> whereRegDate = x => documentTemplateQuery.Any(d => d.SourceId == x.Id);

            Expression<Func<CaseMigration, bool>> whereDateWrt = x => true;
            if (filter.MigrationDateWrtFrom != null && filter.MigrationDateWrtTo != null)
                whereDateWrt = x => caseMigrationInput.Any(m => m.CaseId == x.CaseId &&
                                                                m.InitialCaseId == x.InitialCaseId &&
                                                                m.Id > x.Id);

            Expression<Func<CaseMigration, bool>> whereMigrationTypeId = x => true;
            if (filter.MigrationTypeId > 0)
                whereMigrationTypeId = x => x.CaseMigrationTypeId == filter.MigrationTypeId;

            Expression<Func<CaseMigration, bool>> whereSendToCourtId = x => true;
            if (filter.SendToCourtId > 0)
                whereSendToCourtId = x => x.SendToCourtId == filter.SendToCourtId;

            return repo.AllReadonly<CaseMigration>()
                       .Where(x => x.CourtId == userContext.CourtId &&
                                   x.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Outgoing)
                       .Where(whereRegDate)
                       .Where(whereDateWrt)
                       .Where(whereMigrationTypeId)
                       .Where(whereSendToCourtId)
                       .Select(x => new MigrationReportVM()
                       {
                           CaseId = x.CaseId,
                           CaseInfo = x.Case.RegNumber,
                           CaseTypeLabel = x.Case.CaseType.Label,
                           DocumentRegDate = documentTemplateQuery.Where(d => d.SourceId == x.Id)
                                                                  .Select(d => d.Document.DocumentDate)
                                                                  .FirstOrDefault(),
                           MigrationDateWrt = caseMigrationInput.Any(m => m.CaseId == x.CaseId &&
                                                                          m.InitialCaseId == x.InitialCaseId &&
                                                                          m.Id > x.Id) ? caseMigrationInput.Where(m => m.CaseId == x.CaseId &&
                                                                                                                       m.InitialCaseId == x.InitialCaseId &&
                                                                                                                       m.Id > x.Id)
                                                                                                           .Select(m => m.DateWrt)
                                                                                                           .FirstOrDefault() : (DateTime?)null,
                           MigrationTypeLabel = x.CaseMigrationType.Label,
                           SendToCourtLabel = x.SendToCourt.Label,
                           Description = x.Description
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Специализирана справка - заявка 13
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<SpecializedReportVM> SpecializedReport_Select(SpecializedReportFilterVM filter)
        {
            DateTime dateNow = DateTime.Now;

            filter.DateFrom = filter.DateFrom.ForceStartDate();
            filter.DateTo = filter.DateTo.ForceEndDate();
            Expression<Func<Case, bool>> regDateWhere = x => x.RegDate >= filter.DateFrom && x.RegDate <= filter.DateTo;

            Expression<Func<Case, bool>> courtIdWhere = x => true;
            if ((filter.CourtId ?? 0) > 0)
                courtIdWhere = x => x.CourtId == filter.CourtId;

            Expression<Func<Case, bool>> instanceIdWhere = x => true;
            if ((filter.InstanceId ?? 0) > 0)
                instanceIdWhere = x => x.CaseType.CaseInstanceId == filter.InstanceId;

            Expression<Func<Case, bool>> caseGroupIdsWhere = x => true;
            if (filter.CaseGroupIds != null && filter.CaseGroupIds.Any())
            {
                int[] caseGroupIds = filter.CaseGroupIds.Select(i => int.Parse(i)).ToArray();
                caseGroupIdsWhere = x => caseGroupIds.Contains(x.CaseGroupId);
            }

            Expression<Func<Case, bool>> caseTypeIdsWhere = x => true;
            if (filter.CaseTypeIds != null && filter.CaseTypeIds.Any())
            {
                int[] caseTypeIds = filter.CaseTypeIds.Select(i => int.Parse(i)).ToArray();
                caseTypeIdsWhere = x => caseTypeIds.Contains(x.CaseTypeId);
            }

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(i => int.Parse(i)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            Expression<Func<Case, bool>> actWhere = x => true;
            if ((filter.ActInforcedDateFrom != null && filter.ActInforcedDateTo != null) || ((filter.ActComplainResultId ?? 0) > 0))
            {
                if (filter.ActInforcedDateFrom != null && filter.ActInforcedDateTo != null && (filter.ActComplainResultId ?? 0) > 0)
                {
                    filter.ActInforcedDateFrom = filter.ActInforcedDateFrom.ForceStartDate();
                    filter.ActInforcedDateTo = filter.ActInforcedDateTo.ForceEndDate();
                    actWhere = x => x.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                               a.IsFinalDoc &&
                                                               a.ActInforcedDate != null &&
                                                               a.ActInforcedDate >= filter.ActInforcedDateFrom &&
                                                               a.ActInforcedDate <= filter.ActInforcedDateTo &&
                                                               a.ActComplainResultId == filter.ActComplainResultId);
                }

                if (filter.ActInforcedDateFrom != null && filter.ActInforcedDateTo != null && (filter.ActComplainResultId ?? 0) < 1)
                {
                    filter.ActInforcedDateFrom = filter.ActInforcedDateFrom.ForceStartDate();
                    filter.ActInforcedDateTo = filter.ActInforcedDateTo.ForceEndDate();
                    actWhere = x => x.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                               a.IsFinalDoc &&
                                                               a.ActInforcedDate != null &&
                                                               a.ActInforcedDate >= filter.ActInforcedDateFrom &&
                                                               a.ActInforcedDate <= filter.ActInforcedDateTo);
                }

                if ((filter.ActInforcedDateFrom == null || filter.ActInforcedDateTo == null) && (filter.ActComplainResultId ?? 0) > 0)
                {
                    filter.ActInforcedDateFrom = filter.ActInforcedDateFrom.ForceStartDate();
                    filter.ActInforcedDateTo = filter.ActInforcedDateTo.ForceEndDate();
                    actWhere = x => x.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                               a.IsFinalDoc &&
                                                               a.ActComplainResultId == filter.ActComplainResultId);
                }
            }

            Expression<Func<Case, bool>> caseStateIdWhere = x => true;
            if ((filter.CaseStateId ?? 0) > 0)
                caseStateIdWhere = x => x.CaseStateId == filter.CaseStateId;

            Expression<Func<Case, bool>> caseClassificationIdsWhere = x => true;
            if (filter.CaseClassificationIds != null && filter.CaseClassificationIds.Any())
            {
                int[] caseClassificationIds = filter.CaseClassificationIds.Select(i => int.Parse(i)).ToArray();
                caseClassificationIdsWhere = x => x.CaseClassifications.Any(c => caseClassificationIds.Contains(c.ClassificationId));
            }

            var migrationQuery = readonlyrepo.AllReadonly<CaseMigration>()
                                             .Where(migration => migration.DateExpired == null &&
                                                                 migration.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Outgoing);

            return readonlyrepo.AllReadonly<Case>()
                               .Where(regDateWhere)
                               .Where(courtIdWhere)
                               .Where(instanceIdWhere)
                               .Where(caseGroupIdsWhere)
                               .Where(caseTypeIdsWhere)
                               .Where(caseCodeIdsWhere)
                               .Where(actWhere)
                               .Where(caseStateIdWhere)
                               .Where(caseClassificationIdsWhere)
                               .Select(x => new SpecializedReportVM()
                               {
                                   Id = x.Id,
                                   CourtLabel = x.Court.Label,
                                   CaseInstanceLabel = x.CaseType.CaseInstance.Label,
                                   RegNumber = x.RegNumber,
                                   RegDate = x.RegDate,
                                   EisspNumber = x.EISSPNumber,
                                   CaseGroupLabel = x.CaseGroup.Label,
                                   CaseTypeLabel = x.CaseType.Label,
                                   CaseCodeLabel = x.CaseCodeId != null ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
                                   CaseClassifications = x.CaseClassifications.Select(c => c.Classification.Label).ToArray(),
                                   CaseStateLabel = x.CaseState.Label,
                                   CaseMigrationRegNumbers = migrationQuery.Where(migration => x.CaseMigrations.Where(m => m.DateExpired == null)
                                                                                                               .Select(m => m.InitialCaseId)
                                                                                                               .Contains(migration.InitialCaseId) &&
                                                                                               migration.CaseId != x.Id)
                                                                           .Select(migration => migration.Case.RegNumber + "/" + migration.Case.RegDate.ToString("dd.MM.yyyy"))
                                                                           .ToArray(),
                                   Acts = x.CaseSessionActs.Where(a => a.DateExpired == null &&
                                                                       a.IsFinalDoc &&
                                                                       a.ActDeclaredDate != null)
                                                           .Select(a => new ActDetailsReportVM()
                                                           {
                                                               Id = a.Id,
                                                               ActInforcedDate = a.ActInforcedDate,
                                                               ActComplainResultLabel = a.ActComplainResult.Label,
                                                               RegNumber = a.RegNumber,
                                                               RegDate = a.RegDate,
                                                               ActTypeLabel = a.ActType.Label
                                                           })
                                                           .ToList()
                               })
                               .AsQueryable();
        }

        /// <summary>
        /// Дела от движенията към дело от специализирана справка  - заявка 13
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public IQueryable<SpecializedReportVM> SpecializedReportMigrationCase_Select(int caseId)
        {
            var iniCaseIdQuery = readonlyrepo.AllReadonly<CaseMigration>()
                                             .Where(m => m.CaseId == caseId)
                                             .Select(m => m.InitialCaseId)
                                             .FirstOrDefault();

            Expression<Func<CaseMigration, bool>> migrationWhere = x => x.InitialCaseId == iniCaseIdQuery;

            var migrationQuery = readonlyrepo.AllReadonly<CaseMigration>()
                                             .Where(migration => migration.DateExpired == null &&
                                                                 migration.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Outgoing);

            var result = readonlyrepo.AllReadonly<CaseMigration>()
                                     .Where(migrationWhere)
                                     .Select(x => new SpecializedReportVM()
                                     {
                                         Id = x.Case.Id,
                                         CourtLabel = x.Case.Court.Label,
                                         CaseInstanceLabel = x.Case.CaseType.CaseInstance.Label,
                                         RegNumber = x.Case.RegNumber,
                                         RegDate = x.Case.RegDate,
                                         EisspNumber = x.Case.EISSPNumber,
                                         CaseGroupLabel = x.Case.CaseGroup.Label,
                                         CaseTypeLabel = x.Case.CaseType.Label,
                                         CaseCodeLabel = x.Case.CaseCodeId != null ? x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label : string.Empty,
                                         CaseClassifications = x.Case.CaseClassifications.Select(c => c.Classification.Label).ToArray(),
                                         CaseStateLabel = x.Case.CaseState.Label,
                                         CaseMigrationRegNumbers = migrationQuery.Where(migration => x.Case.CaseMigrations.Where(m => m.DateExpired == null)
                                                                                                                          .Select(m => m.InitialCaseId)
                                                                                                                          .Contains(migration.InitialCaseId) &&
                                                                                                     migration.CaseId != x.Case.Id)
                                                                                 .Select(migration => migration.Case.RegNumber + "/" + migration.Case.RegDate.ToString("dd.MM.yyyy"))
                                                                                 .ToArray(),
                                         Acts = x.Case.CaseSessionActs.Where(a => a.DateExpired == null &&
                                                                                  a.IsFinalDoc &&
                                                                                  a.ActDeclaredDate != null)
                                                                      .Select(a => new ActDetailsReportVM()
                                                                      {
                                                                          Id = a.Id,
                                                                          ActInforcedDate = a.ActInforcedDate,
                                                                          ActComplainResultLabel = a.ActComplainResult.Label,
                                                                          RegNumber = a.RegNumber,
                                                                          RegDate = a.RegDate,
                                                                          ActTypeLabel = a.ActType.Label
                                                                      })
                                                                      .ToList()
                                     })
                                     .GroupBy(x => x.Id)
                                     .Select(g => g.FirstOrDefault())
                                     .ToList();

            return result.AsQueryable();
        }

        /// <summary>
        /// Справка разпределение на дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSelectionProtokolFastProcessVM> CaseSelectionProtokolFastProcess_Select(CaseSelectionProtokolFilterFastProcessVM filter)
        {
            Expression<Func<CaseSelectionProtokol, bool>> distributionDateFromWhere = x => true;
            if (filter.DistributionDateFrom != null)
            {
                filter.DistributionDateFrom = filter.DistributionDateFrom.ForceStartDate();
                distributionDateFromWhere = x => x.SelectionDate >= filter.DistributionDateFrom;
            }

            Expression<Func<CaseSelectionProtokol, bool>> distributionDateToWhere = x => true;
            if (filter.DistributionDateTo != null)
            {
                filter.DistributionDateTo = filter.DistributionDateTo.ForceEndDate();
                distributionDateToWhere = x => x.SelectionDate <= filter.DistributionDateTo;
            }

            Expression<Func<CaseSelectionProtokol, bool>> selectionModeIdWhere = x => true;
            if ((filter.SelectionModeId ?? 0) > 0)
                selectionModeIdWhere = x => x.SelectionModeId == filter.SelectionModeId;

            Expression<Func<CaseSelectionProtokol, bool>> judgeReporterFullNameWhere = x => true;
            if (!string.IsNullOrEmpty(filter.JudgeReporterFullName))
                judgeReporterFullNameWhere = x => EF.Functions.ILike(x.SelectedLawUnit.FullName, filter.JudgeReporterFullName.ToPaternSearch());

            Expression<Func<CaseSelectionProtokol, bool>> caseGroupWhere = x => true;
            if (!string.IsNullOrEmpty(filter.CaseGroupIds_text))
            {
                var listGroupIds = new List<int>();
                if (!string.IsNullOrEmpty(filter.CaseGroupIds_text))
                    listGroupIds = filter.CaseGroupIds_text.Split(',').Select(Int32.Parse).ToList();
                caseGroupWhere = x => listGroupIds.Contains(x.Case.CaseGroupId);
            }

            Expression<Func<CaseSelectionProtokol, bool>> caseTypeWhere = x => true;
            if (!string.IsNullOrEmpty(filter.CaseTypeIds_text))
            {
                var listTypeIds = new List<int>();
                if (!string.IsNullOrEmpty(filter.CaseTypeIds_text))
                    listTypeIds = filter.CaseTypeIds_text.Split(',').Select(Int32.Parse).ToList();
                caseTypeWhere = x => listTypeIds.Contains(x.Case.CaseTypeId);
            }

            Expression<Func<CaseSelectionProtokol, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            Expression<Func<CaseSelectionProtokol, bool>> regNumberWhere = x => true;
            if (!string.IsNullOrEmpty(filter.RegNumber))
                regNumberWhere = x => EF.Functions.ILike(x.Case.RegNumber, filter.RegNumber.ToCasePaternSearch());

            Expression<Func<CaseSelectionProtokol, bool>> fastProcessRegNumberWhere = x => true;
            if (!string.IsNullOrEmpty(filter.FastProcessRegNumber))
                fastProcessRegNumberWhere = x => x.Case.Document.AssignmentDocumentId != null && 
                                                 x.Case.Document.AssignmentDocument.DocumentNumber == filter.FastProcessRegNumber;

            Expression<Func<CaseSelectionProtokol, bool>> regNumberEpepWhere = x => true;
            if (!string.IsNullOrEmpty(filter.RegNumberEpep))
                regNumberEpepWhere = x => x.Case.Document.AssignmentDocumentId != null &&
                                          x.Case.Document.AssignmentDocument.ElectronicDocumentId != null &&
                                          x.Case.Document.AssignmentDocument.ElectronicDocument.ApplyNumber == filter.RegNumberEpep;

            Expression<Func<CaseSelectionProtokol, bool>> courtIdWhere = x => true;
            if ((filter.CourtId ?? 0) > 0)
                courtIdWhere = x => x.Case.CourtId == filter.CourtId;

            int courtId = userContext.CourtId;

            return repo.AllReadonly<CaseSelectionProtokol>()
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Where(x => x.Case.IsFastProcess ?? false)
                       .Where(distributionDateFromWhere)
                       .Where(distributionDateToWhere)
                       .Where(selectionModeIdWhere)
                       .Where(judgeReporterFullNameWhere)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(regNumberWhere)
                       .Where(fastProcessRegNumberWhere)
                       .Where(regNumberEpepWhere)
                       .Where(courtIdWhere)
                       .Select(x => new CaseSelectionProtokolFastProcessVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           IsLinkCase = x.Case.CourtId == courtId,
                           CaseCode = x.Case.CaseCode.Code,
                           DocumentDate = x.Case.Document.DocumentDate,
                           JudgeReporterFullName = x.SelectedLawUnit.FullName,
                           SelectionModeLabel = x.SelectionMode.Label,
                           CaseCourtLabel = x.Case.Court.Label,
                           CaseRegNum = x.Case.RegNumber,
                           CaseRegDate = x.Case.RegDate,
                           DocumentRegNumEpep = x.Case.Document.AssignmentDocumentId != null ? (x.Case.Document.AssignmentDocument.ElectronicDocumentId != null ? x.Case.Document.AssignmentDocument.ElectronicDocument.ApplyNumber : string.Empty) : string.Empty,
                           DocumentRegNum = x.Case.Document.AssignmentDocument.DocumentNumber,
                           CaseStateName = x.Case.CaseState.Label,
                           SelectionDate = x.SelectionDate
                       });
        }

        /// <summary>
        /// Справка на заповедните производства
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<Infrastructure.Models.ViewModels.Report.CaseFastProcessVM> CaseFastProcess_Select(CaseFilterFastProcessVM filter)
        {
            Expression<Func<Case, bool>> fastProcessRegNumberWhere = x => true;
            if (!string.IsNullOrEmpty(filter.FastProcessRegNumber))
                fastProcessRegNumberWhere = x => EF.Functions.ILike(x.Document.AssignmentDocument.DocumentNumber, "%" + filter.FastProcessRegNumber + "%");

            Expression<Func<Case, bool>> regNumberWhere = x => true;
            if (!string.IsNullOrEmpty(filter.RegNumber))
                regNumberWhere = x => EF.Functions.ILike(x.RegNumber, filter.RegNumber.ToCasePaternSearch());

            Expression<Func<Case, bool>> courtIdWhere = x => true;
            if ((filter.CourtId ?? 0) > 0)
                courtIdWhere = x => x.CourtId == filter.CourtId;

            Expression<Func<Case, bool>> caseRegDateFromWhere = x => true;
            if (filter.CaseRegDateFrom != null)
            {
                filter.CaseRegDateFrom = filter.CaseRegDateFrom.ForceStartDate();
                caseRegDateFromWhere = x => x.RegDate >= filter.CaseRegDateFrom;
            }

            Expression<Func<Case, bool>> caseRegDateToWhere = x => true;
            if (filter.CaseRegDateTo != null)
            {
                filter.CaseRegDateTo = filter.CaseRegDateTo.ForceEndDate();
                caseRegDateToWhere = x => x.RegDate <= filter.CaseRegDateTo;
            }

            Expression<Func<Case, bool>> epepDocumentRegDateFromWhere = x => true;
            if (filter.EpepDocumentRegDateFrom != null)
            {
                filter.EpepDocumentRegDateFrom = filter.EpepDocumentRegDateFrom.ForceStartDate();
                epepDocumentRegDateFromWhere = x => x.Document.AssignmentDocument.ElectronicDocument.DocumentDate >= filter.EpepDocumentRegDateFrom;
            }

            Expression<Func<Case, bool>> epepDocumentRegDateToWhere = x => true;
            if (filter.EpepDocumentRegDateTo != null)
            {
                filter.EpepDocumentRegDateTo = filter.EpepDocumentRegDateTo.ForceEndDate();
                epepDocumentRegDateToWhere = x => x.Document.AssignmentDocument.ElectronicDocument.DocumentDate <= filter.EpepDocumentRegDateTo;
            }

            Expression<Func<Case, bool>> importerPersonNameWhere = x => true;
            if (!string.IsNullOrEmpty(filter.ImporterPersonName))
            {
                importerPersonNameWhere = x => x.Document.DocumentPersons.Any(d => EF.Functions.ILike(d.FullName, filter.ImporterPersonName.ToPaternSearch()) &&
                                                                                   d.PersonRoleId == NomenclatureConstants.PersonRole.Notifier);
            }

            Expression<Func<Case, bool>> documentRegNumEpepWhere = x => true;
            if (!string.IsNullOrEmpty(filter.DocumentRegNumEpep))
                documentRegNumEpepWhere = x => x.Document.AssignmentDocument.ElectronicDocument.ApplyNumber == filter.DocumentRegNumEpep;

            Expression<Func<Case, bool>> caseStateIdWhere = x => true;
            if ((filter.CaseStateId ?? 0) > 0)
                caseStateIdWhere = x => x.CaseStateId == filter.CaseStateId;

            int courtId = userContext.CourtId;

            return repo.AllReadonly<Case>()
                       .Where(x => !x.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Where(x => x.IsFastProcess ?? false)
                       .Where(fastProcessRegNumberWhere)
                       .Where(regNumberWhere)
                       .Where(courtIdWhere)
                       .Where(caseRegDateFromWhere)
                       .Where(caseRegDateToWhere)
                       .Where(epepDocumentRegDateFromWhere)
                       .Where(epepDocumentRegDateToWhere)
                       .Where(importerPersonNameWhere)
                       .Where(documentRegNumEpepWhere)
                       .Where(caseStateIdWhere)
                       .Select(x => new Infrastructure.Models.ViewModels.Report.CaseFastProcessVM()
                       {
                           CaseId = x.Id,
                           IsLinkCase = x.CourtId == courtId,
                           DocumentRegNum = x.Document.AssignmentDocument.DocumentNumber,
                           DocumentRegNumValue = x.Document.AssignmentDocument.DocumentNumberValue,
                           CaseRegNum = x.RegNumber,
                           CaseCourtLabel = x.Court.Label,
                           CaseCode = x.CaseCode.Code,
                           ImporterPersonFullName = string.Join(", ", x.Document
                                                                       .DocumentPersons
                                                                       .Where(d => d.PersonRoleId == NomenclatureConstants.PersonRole.Notifier)
                                                                       .Select(d => d.FullName)),
                           DocumentRegNumEpep = x.Document.AssignmentDocumentId != null ? (x.Document.AssignmentDocument.ElectronicDocumentId != null ? x.Document.AssignmentDocument.ElectronicDocument.ApplyNumber : string.Empty) : string.Empty,
                           CaseStateName = x.CaseState.Label
                       });
        }

        /// <summary>
        /// Справка на списък на длъжници/заявители за дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<ListDebtorsApplicantsFastProcessVM> ListDebtorsApplicantsFilterFastProcess_Select(ListDebtorsApplicantsFilterFastProcessVM filter)
        {
            Expression<Func<CasePerson, bool>> caseRegDateFromWhere = x => true;
            if (filter.CaseRegDateFrom != null)
            {
                filter.CaseRegDateFrom = filter.CaseRegDateFrom.ForceStartDate();
                caseRegDateFromWhere = x => x.Case.RegDate >= filter.CaseRegDateFrom;
            }

            Expression<Func<CasePerson, bool>> caseRegDateToWhere = x => true;
            if (filter.CaseRegDateTo != null)
            {
                filter.CaseRegDateTo = filter.CaseRegDateTo.ForceEndDate();
                caseRegDateToWhere = x => x.Case.RegDate <= filter.CaseRegDateTo;
            }

            Expression<Func<CasePerson, bool>> namePersonWhere = x => true;
            if (!string.IsNullOrEmpty(filter.NamePerson))
                namePersonWhere = x => EF.Functions.ILike(x.FullName, filter.NamePerson.ToPaternSearch());

            Expression<Func<CasePerson, bool>> identifikatorPersonWhere = x => true;
            if (!string.IsNullOrEmpty(filter.IdentifikatorPerson))
                namePersonWhere = x => EF.Functions.ILike(x.Uic, filter.IdentifikatorPerson.ToPaternSearch());

            int courtId = userContext.CourtId;

            return repo.AllReadonly<CasePerson>()
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null))
                       .Where(x => x.Case.IsFastProcess ?? false)
                       .Where(x => NomenclatureConstants.PersonRole.PersonFastProcess.Contains(x.PersonRoleId))
                       .Where(x => x.CaseSessionId == null)
                       .Where(x => x.DateExpired == null)
                       .Where(caseRegDateFromWhere)
                       .Where(caseRegDateToWhere)
                       .Where(namePersonWhere)
                       .Where(identifikatorPersonWhere)
                       .Select(x => new ListDebtorsApplicantsFastProcessVM()
                       {
                           CaseId = x.CaseId,
                           IsLinkCase = x.CourtId == courtId,
                           PersonName = x.FullName + " (" + x.PersonRole.Label + ")",
                           CaseRegNum = x.Case.RegNumber,
                           DocumentRegNum = x.Case.Document.AssignmentDocument.DocumentNumber,
                           AssignmentDocumentId = x.Case.Document.AssignmentDocumentId,
                           CaseCourtLabel = x.Case.Court.Label,
                           DocumentRegNumEpep = x.Case.Document.AssignmentDocumentId != null ? (x.Case.Document.AssignmentDocument.ElectronicDocumentId != null ? x.Case.Document.AssignmentDocument.ElectronicDocument.ApplyNumber : string.Empty) : string.Empty,
                           CaseStateName = x.Case.CaseState.Label
                       });
        }

        /// <summary>
        /// Справка за регистрираните документи за определен период за дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<RegisteredDocumentsFastProcessVM> RegisteredDocumentsFastProcess_Select(RegisteredDocumentsFilterFastProcessVM filter)
        {
            Expression<Func<Document, bool>> documentRegNumWhere = x => true;
            if (!string.IsNullOrEmpty(filter.DocumentRegNum))
                documentRegNumWhere = x => EF.Functions.ILike(x.DocumentNumber, filter.DocumentRegNum.ToPaternSearch());

            Expression<Func<Document, bool>> documentDeliveryGroupIdWhere = x => true;
            if ((filter.DocumentDeliveryGroupId ?? 0) > 0)
                documentDeliveryGroupIdWhere = x => x.DeliveryGroupId == filter.DocumentDeliveryGroupId;

            Expression<Func<Document, bool>> documentDateFromWhere = x => true;
            if (filter.DocumentDateFrom != null)
            {
                filter.DocumentDateFrom = filter.DocumentDateFrom.ForceStartDate();
                documentDateFromWhere = x => x.DocumentDate >= filter.DocumentDateFrom;
            }

            Expression<Func<Document, bool>> documentDateToWhere = x => true;
            if (filter.DocumentDateTo != null)
            {
                filter.DocumentDateTo = filter.DocumentDateTo.ForceEndDate();
                documentDateToWhere = x => x.DocumentDate <= filter.DocumentDateTo;
            }

            Expression<Func<Document, bool>> documentRegNumEpepWhere = x => true;
            if (!string.IsNullOrEmpty(filter.DocumentRegNumEpep))
                documentRegNumEpepWhere = x => x.AssignmentDocument.ElectronicDocument.ApplyNumber == filter.DocumentRegNumEpep;

            Expression<Func<Document, bool>> documentDateEpepFromWhere = x => true;
            if (filter.DocumentDateEpepFrom != null)
            {
                filter.DocumentDateEpepFrom = filter.DocumentDateEpepFrom.ForceStartDate();
                documentDateEpepFromWhere = x => x.AssignmentDocument.ElectronicDocument.DocumentDate >= filter.DocumentDateEpepFrom;
            }

            Expression<Func<Document, bool>> documentDateEpepToWhere = x => true;
            if (filter.DocumentDateEpepTo != null)
            {
                filter.DocumentDateEpepTo = filter.DocumentDateEpepTo.ForceEndDate();
                documentDateEpepToWhere = x => x.AssignmentDocument.ElectronicDocument.DocumentDate <= filter.DocumentDateEpepTo;
            }

            Expression<Func<Document, bool>> courtIdWhere = x => true;
            if ((filter.DocumentCourtId ?? 0) > 0)
                courtIdWhere = x => x.CourtId == filter.DocumentCourtId;

            int courtId = userContext.CourtId;

            return repo.AllReadonly<Document>()
                       .Where(x => x.DateExpired == null)
                       .Where(x => x.CourtId == NomenclatureConstants.Courts.RandomAssignment)
                       .Where(documentRegNumWhere)
                       .Where(documentDeliveryGroupIdWhere)
                       .Where(documentDateFromWhere)
                       .Where(documentDateToWhere)
                       .Where(documentRegNumEpepWhere)
                       .Where(documentDateEpepFromWhere)
                       .Where(documentDateEpepToWhere)
                       .Where(courtIdWhere)
                       .Select(x => new RegisteredDocumentsFastProcessVM()
                       {
                           DocumentId = x.Id,
                           IsLinkDocument = x.CreatedCourtId == courtId,
                           DocumentRegNum = x.DocumentNumber,
                           DocumentCourtLabel = x.CreatedCourt.Label,
                           DocumentDeliveryGroupLabel = x.DeliveryGroup.Label,
                           DocumentDate = x.DocumentDate,
                           DocumentRegNumEpep = x.ElectronicDocumentId != null ? x.ElectronicDocument.ApplyNumber : string.Empty,
                           DocumentDateEpep = x.ElectronicDocumentId != null ? x.ElectronicDocument.ApplyDate : null,
                       });
        }

        /// <summary>
        /// Извличане на данни за справка необработени документ по чл 410 и 417 от ЕИСС
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<RawDocumentsFastProcessVM> GetRawDocumentsFastProcess_Select(RawDocumentsFilterFastProcessVM filter)
        {
            Expression<Func<Document, bool>> documentRegNumWhere = x => true;
            if (!string.IsNullOrEmpty(filter.DocumentRegNum))
                documentRegNumWhere = x => x.DocumentNumber == filter.DocumentRegNum;

            Expression<Func<Document, bool>> documentDateFromWhere = x => true;
            if (filter.DocumentDateFrom != null)
            {
                filter.DocumentDateFrom = filter.DocumentDateFrom.ForceStartDate();
                documentDateFromWhere = x => x.DocumentDate >= filter.DocumentDateFrom;
            }

            Expression<Func<Document, bool>> documentDateToWhere = x => true;
            if (filter.DocumentDateTo != null)
            {
                filter.DocumentDateTo = filter.DocumentDateTo.ForceEndDate();
                documentDateToWhere = x => x.DocumentDate <= filter.DocumentDateTo;
            }

            IQueryable<WorkTask> workTasksQuerry = repo.AllReadonly<WorkTask>()
                                                       .Where(w => w.SourceType == SourceTypeSelectVM.Document)
                                                       .Where(w => w.TaskTypeId == WorkTaskConstants.Types.DocumentForGlobalAssignment);

            int courtId = userContext.CourtId;

            return repo.AllReadonly<Document>()
                       .Where(x => DocumentConstants.ElectronicDocumentRequestTypes.FastProcess.Contains(x.DocumentRequestType.RequestCode))
                       .Where(x => x.DateExpired == null)
                       .Where(x => x.CourtId == NomenclatureConstants.Courts.RandomAssignment)
                       .Where(x => x.CreatedCourtId == courtId)
                       .Where(x => !workTasksQuerry.Any(w => w.SourceId == x.Id))
                       .Where(documentRegNumWhere)
                       .Where(documentDateFromWhere)
                       .Where(documentDateToWhere)
                       .Select(x => new RawDocumentsFastProcessVM()
                       {
                           DocumentId = x.Id,
                           DocumentRegNum = x.DocumentNumber,
                           DocumentTypeLabel = x.DocumentType.Label,
                           DocumentCodeLabel = x.Cases.Select(c => c.CaseCode.Label).FirstOrDefault(),
                           DocumentDate = x.DocumentDate,
                       });
        }

        /// <summary>
        /// Справка нотификации по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<WorkNotificationFatsProcessVM> WorkNotificationFatsProcess_Select(WorkNotificationFilterFatsProcessVM filter)
        {
            Expression<Func<WorkNotification, bool>> userFullNameWhere = x => true;
            if (!string.IsNullOrEmpty(filter.UserFullName))
                userFullNameWhere = x => EF.Functions.ILike(x.User.LawUnit.FullName, filter.UserFullName.ToPaternSearch());

            Expression<Func<WorkNotification, bool>> notificationTypeIdWhere = x => true;
            if ((filter.NotificationTypeId ?? 0) > 0)
                notificationTypeIdWhere = x => x.WorkNotificationTypeId == filter.NotificationTypeId;

            Expression<Func<WorkNotification, bool>> notificationDateFromWhere = x => true;
            if (filter.NotificationDateFrom != null)
            {
                filter.NotificationDateFrom = filter.NotificationDateFrom.ForceStartDate();
                notificationDateFromWhere = x => x.DateCreated >= filter.NotificationDateFrom;
            }

            Expression<Func<WorkNotification, bool>> notificationDateToWhere = x => true;
            if (filter.NotificationDateTo != null)
            {
                filter.NotificationDateTo = filter.NotificationDateTo.ForceEndDate();
                notificationDateToWhere = x => x.DateCreated <= filter.NotificationDateTo;
            }

            Expression<Func<WorkNotification, bool>> caseRegNumberWhere = x => true;
            if (!string.IsNullOrEmpty(filter.CaseRegNumber))
                caseRegNumberWhere = x => EF.Functions.ILike(x.Case.RegNumber, filter.CaseRegNumber.ToCasePaternSearch());

            Expression<Func<WorkNotification, bool>> caseIdWhere = x => true;
            if ((filter.CaseId ?? 0) > 0)
                caseIdWhere = x => x.CaseId == filter.CaseId;

            int courtId = userContext.CourtId;

            //Expression<Func<WorkNotification, bool>> createWhere = x => x.DateCreated <= DateTime.Now;

            return repo.AllReadonly<WorkNotification>()
                       .Where(x => x.DateExpired == null)
                       .Where(x => x.NotificationKind == 2)
                       .Where(x => x.CourtId == courtId)
                       .Where(userFullNameWhere)
                       .Where(notificationTypeIdWhere)
                       .Where(notificationDateFromWhere)
                       .Where(notificationDateToWhere)
                       .Where(caseRegNumberWhere)
                       .Where(caseIdWhere)
                       .Select(x => new WorkNotificationFatsProcessVM()
                       {
                           Id = x.Id,
                           UserFullName = x.User.LawUnit.FullName,
                           NotificationTypeLabel = x.WorkNotificationType.Label,
                           NotificationDate = x.DateCreated,
                           CaseId = x.CaseId ?? 0,
                           CaseRegNumber = x.Case.RegNumber,
                           IsLinkCase = x.CourtId == courtId,
                           IsUnRead = ((filter.CaseId ?? 0) > 0) ? x.DateTurnOff == null && x.UserId == userContext.UserId : x.DateTurnOff == null,
                           DateTurnOff = x.DateTurnOff,
                           DescriptionTurnOff = x.DescriptionTurnOff
                       });
        }

        /// <summary>
        /// Групова отмяна на нотификации
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<bool> WorkNotificationsSetIsRead(WorkNotificationSetIsReadVM model)
        {
            try
            {
                long[] notificationIds = model.NotificationIds.Split(',').Select(x => long.Parse(x)).ToArray();

                List<WorkNotification> notifications = await repo.All<WorkNotification>()
                                                                 .Where(x => notificationIds.Contains(x.Id))
                                                                 .ToListAsync();

                notifications.ForEach(x => { x.DateRead = DateTime.Now; x.DescriptionExpired = model.Description; });
                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при отмяна на нотигфикации с ид-та: {model.NotificationIds}");
                return false;
            }
        }

        /// <summary>
        /// Груповo гасене на нотификации
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<bool> WorkNotificationsSetTurnOff(WorkNotificationSetIsReadVM model)
        {
            try
            {
                long[] notificationIds = model.NotificationIds.Split(',').Select(x => long.Parse(x)).ToArray();

                List<WorkNotification> notifications = await repo.All<WorkNotification>()
                                                                 .Where(x => notificationIds.Contains(x.Id))
                                                                 .ToListAsync();

                notifications.ForEach(x => { x.DateTurnOff = DateTime.Now; x.DescriptionExpired = model.Description; });
                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при гасене на нотигфикации с ид-та: {model.NotificationIds}");
                return false;
            }
        }

        /// <summary>
        /// Справка изпълнителни листове по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<ExecListFatsProcessVM> ExecListFatsProcess_Select(ExecListFilterFatsProcessVM filter)
        {
            Expression<Func<CaseSessionAct, bool>> execListRegNumberAWhere = x => true;
            Expression<Func<ExecList, bool>> execListRegNumberEWhere = x => true;
            if (!string.IsNullOrEmpty(filter.ExecListRegNumber))
            {
                execListRegNumberAWhere = x => EF.Functions.ILike(x.RegNumber, "%" + filter.ExecListRegNumber);
                execListRegNumberEWhere = x => EF.Functions.ILike(x.RegNumber, "%" + filter.ExecListRegNumber);
            }

            Expression<Func<CaseSessionAct, bool>> caseRegNumberAWhere = x => true;
            Expression<Func<ExecList, bool>> caseRegNumberEWhere = x => true;
            if (!string.IsNullOrEmpty(filter.CaseRegNumber))
            {
                caseRegNumberAWhere = x => EF.Functions.ILike(x.Case.RegNumber, filter.CaseRegNumber.ToCasePaternSearch());
                caseRegNumberEWhere = x => EF.Functions.ILike(x.Case.RegNumber, filter.CaseRegNumber.ToCasePaternSearch());
            }

            Expression<Func<CaseSessionAct, bool>> courtIdAWhere = x => true;
            Expression<Func<ExecList, bool>> courtIdEWhere = x => true;
            if (filter.CourtId != null && filter.CourtId > 0)
            {
                courtIdAWhere = x => x.CourtId == filter.CourtId;
                courtIdEWhere = x => x.CourtId == filter.CourtId;
            }

            Expression<Func<CaseSessionAct, bool>> execListSignDateFromAWhere = x => true;
            Expression<Func<ExecList, bool>> execListSignDateFromEWhere = x => true;
            if (filter.ExecListSignDateFrom != null)
            {
                filter.ExecListSignDateFrom = filter.ExecListSignDateFrom.ForceStartDate();
                execListSignDateFromAWhere = x => x.ActDeclaredDate >= filter.ExecListSignDateFrom;
                execListSignDateFromEWhere = x => x.DateSigned >= filter.ExecListSignDateFrom;
            }

            Expression<Func<CaseSessionAct, bool>> execListSignDateToAWhere = x => true;
            Expression<Func<ExecList, bool>> execListSignDateToEWhere = x => true;
            if (filter.ExecListSignDateTo != null)
            {
                filter.ExecListSignDateTo = filter.ExecListSignDateTo.ForceEndDate();
                execListSignDateToAWhere = x => x.ActDeclaredDate <= filter.ExecListSignDateTo;
                execListSignDateToEWhere = x => x.DateSigned <= filter.ExecListSignDateTo;
            }

            Expression<Func<CaseSessionAct, bool>> caseDeactivationsAWhere = x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null);
            Expression<Func<ExecList, bool>> caseDeactivationsEWhere = x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null);
            Expression<Func<CaseSessionAct, bool>> isFastProcessAWhere = x => x.Case.IsFastProcess ?? false;
            Expression<Func<ExecList, bool>> isFastProcessEWhere = x => x.Case.IsFastProcess ?? false;
            Expression<Func<CaseSessionAct, bool>> generateExecProcessAWhere = x => x.GenerateExecProcess ?? false;
            Expression<Func<ExecList, bool>> generateExecProcessEWhere = x => x.GenerateExecProcess ?? false;

            return repo.AllReadonly<CaseSessionAct>()
                       .Where(caseDeactivationsAWhere)
                       .Where(isFastProcessAWhere)
                       .Where(generateExecProcessAWhere)
                       .Where(execListRegNumberAWhere)
                       .Where(caseRegNumberAWhere)
                       .Where(courtIdAWhere)
                       .Where(execListSignDateFromAWhere)
                       .Where(execListSignDateToAWhere)
                       .Where(x => x.ActDeclaredDate != null)
                       .Where(x => NomenclatureConstants.ActType.ExecListActs.Contains(x.ActTypeId))
                       .Select(x => new ExecListFatsProcessVM()
                       {
                           ExecListId = x.Id,
                           ExecListRegNumber = x.RegNumber,
                           ExecListDateSign = x.ActDeclaredDate,
                           ExecListTypeLabel = x.ActType.Label,
                           ExecListCourtLabel = x.Court.Label,
                           CaseRegNum = x.Case.RegNumber
                       })
                       .Union(repo.AllReadonly<ExecList>()
                                  .Where(caseDeactivationsEWhere)
                                  .Where(isFastProcessEWhere)
                                  .Where(generateExecProcessEWhere)
                                  .Where(execListRegNumberEWhere)
                                  .Where(caseRegNumberEWhere)
                                  .Where(courtIdEWhere)
                                  .Where(execListSignDateFromEWhere)
                                  .Where(execListSignDateToEWhere)
                                  .Where(x => x.DateSigned != null)
                                  .Select(x => new ExecListFatsProcessVM()
                                  {
                                      ExecListId = x.Id,
                                      ExecListRegNumber = x.RegNumber,
                                      ExecListDateSign = x.DateSigned,
                                      ExecListTypeLabel = x.ExecListType.Label,
                                      ExecListCourtLabel = x.Court.Label,
                                      CaseRegNum = x.Case.RegNumber
                                  }));
        }

        /// <summary>
        /// Извличане на данни за справка действителен зает щат в съд
        /// </summary>
        /// <returns></returns>
        public IQueryable<CourtJudgeCountVM> CourtJudgeCount_Select()
        {
            DateTime dateNow = DateTime.Now;

            Expression<Func<Court, bool>> dateStartWhere = x => x.DateStart <= dateNow;
            Expression<Func<Court, bool>> dateEndWhere = x => (x.DateEnd ?? dateNow) >= dateNow;
            Expression<Func<Court, bool>> courtTypeIdWhere = x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt;
            Expression<Func<Court, bool>> isActiveWhere = x => x.IsActive;

            return repo.AllReadonly<Court>()
                       .Where(dateStartWhere)
                       .Where(dateEndWhere)
                       .Where(courtTypeIdWhere)
                       .Where(isActiveWhere)
                       .OrderBy(x => x.OrderNumber)
                       .Select(x => new CourtJudgeCountVM()
                       {
                           CourtLabel = x.Label,
                           JudgeCount = x.JudgeCount ?? 0
                       });
        }

        /// <summary>
        /// Справка за актове по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSessionActFastProcessVM> CaseSessionActFastProcess_Select(CaseSessionActFastProcessFilterVM filter)
        {
            Expression<Func<CaseSessionAct, bool>> dateFromAWhere = x => true;
            Expression<Func<ExecList, bool>> dateFromEWhere = x => true;
            if (filter.ActDateFrom != null)
            {
                filter.ActDateFrom = filter.ActDateFrom.ForceStartDate();
                dateFromAWhere = x => x.RegDate >= filter.ActDateFrom;
                dateFromEWhere = x => x.RegDate >= filter.ActDateFrom;
            }

            Expression<Func<CaseSessionAct, bool>> dateToAWhere = x => true;
            Expression<Func<ExecList, bool>> dateToEWhere = x => true;
            if (filter.ActDateTo != null)
            {
                filter.ActDateTo = filter.ActDateTo.ForceEndDate();
                dateToAWhere = x => x.RegDate <= filter.ActDateTo;
                dateToEWhere = x => x.RegDate <= filter.ActDateTo;
            }

            Expression<Func<CaseSessionAct, bool>> actTypeIdAWhere = x => true;
            Expression<Func<ExecList, bool>> actTypeIdEWhere = x => true;
            if ((filter.ActTypeId ?? 0) > 0)
            {
                switch (filter.ActTypeId)
                {
                    case 1:
                        {
                            actTypeIdAWhere = x => x.ActTypeId == NomenclatureConstants.ActType.CommandmentForExec;
                            actTypeIdEWhere = x => x.ExecListTypeId == -2;
                        }
                        break;
                    case 2:
                        {
                            actTypeIdAWhere = x => NomenclatureConstants.ActType.ExecListActs.Contains(x.ActTypeId);
                            actTypeIdEWhere = x => x.ExecListTypeId == -2;
                        }
                        break;
                    case 3: actTypeIdAWhere = x => x.ActTypeId == -2;
                        break;
                }
            }

            Expression<Func<CaseSessionAct, bool>> caseDeactivationsAWhere = x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null);
            Expression<Func<ExecList, bool>> caseDeactivationsEWhere = x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.Id && d.DateExpired == null);
            Expression<Func<CaseSessionAct, bool>> isFastProcessAWhere = x => x.Case.IsFastProcess ?? false;
            Expression<Func<ExecList, bool>> isFastProcessEWhere = x => x.Case.IsFastProcess ?? false;
            Expression<Func<CaseSessionAct, bool>> generateExecProcessAWhere = x => x.GenerateExecProcess ?? false;
            Expression<Func<ExecList, bool>> generateExecProcessEWhere = x => x.GenerateExecProcess ?? false;
            Expression<Func<CaseSessionAct, bool>> actDeclaredDateAWhere = x => x.ActDeclaredDate != null;
            Expression<Func<ExecList, bool>> dateSignedEWhere = x => x.DateSigned != null;
            Expression<Func<CaseSessionAct, bool>> dateExpiredAWhere = x => x.DateExpired == null;
            Expression<Func<ExecList, bool>> dateExpiredEWhere = x => x.DateExpired == null;

            DateTime dateNow = DateTime.Now;

            return repo.AllReadonly<CaseSessionAct>()
                       .Where(dateFromAWhere)
                       .Where(dateToAWhere)
                       .Where(actTypeIdAWhere)
                       .Where(caseDeactivationsAWhere)
                       .Where(isFastProcessAWhere)
                       .Where(generateExecProcessAWhere)
                       .Where(actDeclaredDateAWhere)
                       .Where(dateExpiredAWhere)
                       .Select(x => new CaseSessionActFastProcessVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           CaseCourtLabel = x.Case.Court.Label,
                           CaseRegNum = x.Case.RegNumber,
                           CaseRegDate = x.Case.RegDate,
                           CaseCode = x.Case.CaseCode.Code,
                           ActRegNum = x.RegNumber,
                           ActRegDate = x.RegDate ?? dateNow,
                           ActDeclarDate = x.ActDeclaredDate,
                           ActTypeLabel = x.ActType.Label
                       })
                       .Union(repo.AllReadonly<ExecList>()
                                  .Where(dateFromEWhere)
                                  .Where(dateToEWhere)
                                  .Where(actTypeIdEWhere)
                                  .Where(caseDeactivationsEWhere)
                                  .Where(isFastProcessEWhere)
                                  .Where(generateExecProcessEWhere)
                                  .Where(dateSignedEWhere)
                                  .Where(dateExpiredEWhere)
                                  .Select(x => new CaseSessionActFastProcessVM()
                                  {
                                      CaseId = x.CaseId ?? 0,
                                      CaseCourtLabel = x.Case.Court.Label,
                                      CaseRegNum = x.Case.RegNumber,
                                      CaseRegDate = x.Case.RegDate,
                                      CaseCode = x.Case.CaseCode.Code,
                                      ActRegNum = x.RegNumber,
                                      ActRegDate = x.RegDate ?? dateNow,
                                      ActDeclarDate = x.DateSigned,
                                      ActTypeLabel = x.ExecListType.Label
                                  }));
        }

        /// <summary>
        /// Извличане на типове актове за падащо меню за справка за актове по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <param name="addAllElement">Дали да добави елемент "Всички"</param>
        /// <returns></returns>
        public List<SelectListItem> GetDDLActTypeForCaseSessionActFastProcess(bool addDefaultElement = false, bool addAllElement = true)
        {
            var selectListItems = new List<SelectListItem>();

            selectListItems.Add(new SelectListItem() { Text = "Заповед за изпълнение", Value = "1" });
            selectListItems.Add(new SelectListItem() { Text = "Излпълнителни листове", Value = "2" });
            selectListItems.Add(new SelectListItem() { Text = "Излпълнителни листове - финансов модул", Value = "3" });

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Справка за медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public async Task<IQueryable<MediationCaseVM>> CaseMediation_Select(MediationCaseFilterVM filter)
        {
            DateTime dateNow = DateTime.Now;

            //Expression<Func<Case, bool>> mediationWhere = x => (x.IsMediation ?? false) && x.MediationCaseSessions.Any(m => m.DateExpired == null &&
            //                                                                                                                m.MediationStateId != NomenclatureConstants.MediationStateConstants.Rescheduled &&
            //                                                                                                                m.Results.Any(r => r.DateExpired == null &&
            //                                                                                                                                   r.Result.MediationResultGroupId == NomenclatureConstants.MediationResultGroupConstants.Termination));

            Expression<Func<Case, bool>> mediationWhere = x => x.IsMediation ?? false;

            Expression<Func<Case, bool>> courtIdWhere = x => true;
            if ((filter.CourtId ?? 0) > 0)
                courtIdWhere = x => x.CourtId == filter.CourtId;
            else
            {
                if (filter.IsFindCoordinatorCourt)
                {
                    List<SelectListItem> courts = await mediationCommonService.GetDDL_MediationCoordinatorCourt(false);
                    int[] courtIds = courts.Select(x => int.Parse(x.Value)).ToArray();
                    courtIdWhere = x => courtIds.Contains(x.CourtId);
                }
            }

            Expression<Func<Case, bool>> mediatorIdWhere = x => true;
            if ((filter.MediatorId ?? 0) > 0)
                mediatorIdWhere = x => x.MediationCaseMediators.Any(m => m.MediationMediatorId == filter.MediatorId &&
                                                                         m.DateExpired == null);

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (filter.DateFrom != null || filter.DateTo != null)
                dateSearch = x => x.RegDate >= filter.DateFrom && x.RegDate <= filter.DateTo;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == filter.CaseGroupId;

            Expression<Func<Case, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.CaseTypeId == filter.CaseTypeId;

            Expression<Func<Case, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.CaseCodeId ?? 0);
            }

            Expression<Func<Case, bool>> caseCodeSubIdWhere = x => true;
            if ((filter.CaseCodeSubId ?? 0) > 0)
                caseCodeSubIdWhere = x => x.CaseCodeSubId == filter.CaseCodeSubId;

            Expression<Func<Case, bool>> mediationProcedureIdWhere = x => true;
            if (filter.MediationProcedureId > 0)
                mediationProcedureIdWhere = x => x.MediationProcedureId == filter.MediationProcedureId;

            Expression<Func<Case, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                   (a.DateTo ?? dateNow).Date >= dateNow.Date &&
                                                                   a.LawUnitId == filter.JudgeReporterId &&
                                                                   a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            return repo.AllReadonly<Case>()
                       .Where(mediationWhere)
                       .Where(courtIdWhere)
                       .Where(dateSearch)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(caseCodeSubIdWhere)
                       .Where(judgeReporterSearch)
                       .Where(mediationProcedureIdWhere)
                       .Where(mediatorIdWhere)
                       .Select(x => new MediationCaseVM
                       {
                           CaseId = x.Id,
                           CourtLabel = x.Court.Label,
                           CaseRegNumber = x.RegNumber,
                           CaseCodeName = x.CaseCode.Code,
                           CaseCodeSubName = x.CaseCodeSub.Code,
                           InformationMeetings = x.MediationCaseSessions
                                                  .Where(m => m.DateExpired == null &&
                                                              m.MediationStateId == NomenclatureConstants.MediationStateConstants.Held &&
                                                              NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(m.MediationTypeId))
                                                  .Select(m => new InformationMeeting
                                                  {
                                                      StateLabel = m.MediationState.Label,
                                                      DateFrom = m.DateFrom,
                                                      DateTo = m.DateTo,
                                                      Location = m.MediationLocation.Label
                                                  })
                                                  .ToList(),
                           InformationMeetingScheduled = string.Join("<br />", x.MediationCaseSessions
                                                               .Where(m => m.DateExpired == null &&
                                                                           m.MediationStateId != NomenclatureConstants.MediationStateConstants.Held &&
                                                                           NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(m.MediationTypeId))
                                                               .Select(m => m.MediationState.Label + " (" + m.DateFrom.ToString("dd.MM.yyyy HH:mm") + ")")
                                                               .ToList()),
                           ReasonNonImplementation = string.Join("; ", x.MediationCaseSessions
                                                                        .Where(m => m.DateExpired == null &&
                                                                                    m.MediationStateId == NomenclatureConstants.MediationStateConstants.NotHeld &&
                                                                                    NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(m.MediationTypeId) &&
                                                                                    !string.IsNullOrEmpty(m.Description))
                                                                        .Select(m => m.Description)),
                           MediationProcedureDatas = x.MediationCaseSessions
                                                      .Where(m => m.DateExpired == null &&
                                                                  m.MediationStateId == NomenclatureConstants.MediationStateConstants.Held &&
                                                                  NomenclatureConstants.MediationTypeConstants.Meetings.Contains(m.MediationTypeId))
                                                      .Select(m => new MediationProcedureVM
                                                      {
                                                          MediationProcedureLabel = m.MediationState.Label + " (" + m.DateFrom.ToString("dd.MM.yyyy HH:mm") + ")",
                                                          ReasonTermination = m.Results
                                                                               .Where(r => r.Result.MediationResultGroupId == NomenclatureConstants.MediationResultGroupConstants.Termination &&
                                                                                           NomenclatureConstants.MmediationResultBaseConstants.MediationProcedureReasonTermination.Contains(r.MediationResultBaseId ?? 0) &&
                                                                                           r.DateExpired == null)
                                                                               .Select(r => r.ResultBase.Label)
                                                                               .FirstOrDefault(),
                                                          Location = m.MediationLocation.Label,
                                                          DateFrom = m.DateFrom,
                                                          DateTo = m.DateTo
                                                      })
                                                      .ToList(),
                           MediationProcedureScheduled = string.Join("<br />", x.MediationCaseSessions
                                                                                .Where(m => m.DateExpired == null &&
                                                                                            NomenclatureConstants.MediationStateConstants.StateForEdit.Contains(m.MediationStateId ?? 0) &&
                                                                                            NomenclatureConstants.MediationTypeConstants.Meetings.Contains(m.MediationTypeId))
                                                                                .Select(m => m.MediationState.Label + " (" + m.DateFrom.ToString("dd.MM.yyyy HH:mm") + ")")
                                                                                .ToList()),
                           ConcludedAgreement = x.MediationCaseSessions
                                                 .Any(m => m.DateExpired == null &&
                                                           m.Results.Any(r => r.DateExpired == null &&
                                                                              r.MediationResultBaseId == NomenclatureConstants.MmediationResultBaseConstants.ByReachingAagreement)) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : string.Empty,
                           TerminationCaseSettlement = x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                               s.DateFrom >= x.StartMediationDate &&
                                                                               s.CaseSessionResults.Any(r => r.SessionResultId == NomenclatureConstants.CaseSessionResult.Agreement &&
                                                                                                             r.DateExpired == null)) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : string.Empty,
                           TerminationCaseTerminationWithdrawalRefusal = x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                                 s.DateFrom >= x.StartMediationDate &&
                                                                                                 s.CaseSessionResults.Any(r => r.SessionResultBaseId == NomenclatureConstants.CaseSessionResultBase.PostponeCancel &&
                                                                                                                               r.DateExpired == null)) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : string.Empty,
                           PartiallyTerminationCaseSettlement = x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                        s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                      r.SessionResultId == NomenclatureConstants.CaseSessionResult.PartiallyTerminatedSettlementAfterMediation)) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : string.Empty,
                           PartiallyTerminationCaseTerminationWithdrawalRefusal = x.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                                          s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                                        r.SessionResultId == NomenclatureConstants.CaseSessionResult.PartiallyTerminatedDuePartialWithdrawalWaiverClaimFollowingMediation)) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : string.Empty,
                           CaseMediators = string.Join(", ", x.MediationCaseMediators.Where(m => m.DateExpired == null).Select(m => m.Mediator.Name)),
                           JudgeReporterName = x.CaseSessions
                                                .Where(s => s.DateExpired == null &&
                                                            s.CaseSessionResults
                                                             .Any(r => r.DateExpired == null &&
                                                                       r.SessionResultId == NomenclatureConstants.CaseSessionResult.ReferralInformationalMeetingMediation))
                                                .Select(s => s.CaseLawUnits
                                                              .Where(l => (l.CaseSessionId ?? 0) == s.Id &&
                                                                          (l.DateTo ?? dateNow) >= dateNow &&
                                                                          l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                              .Select(l => l.LawUnit.FullName)
                                                              .FirstOrDefault())
                                                .FirstOrDefault()
                       });
        }

        /// <summary>
        /// Справка за срещи за медиация 
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public async Task<IQueryable<MediationCaseSessionVM>> CaseMediationSession_Select(MediationCaseSessionFilterVM filter)
        {
            DateTime dateNow = DateTime.Now;

            Expression<Func<MediationCaseSession, bool>> mediationWhere = x => x.Case.IsMediation ?? false;
            Expression<Func<MediationCaseSession, bool>> stateIdWhere = x => NomenclatureConstants.MediationStateConstants.ReportState.Contains(x.MediationStateId ?? 0);

            Expression<Func<MediationCaseSession, bool>> courtIdWhere = x => true;
            if ((filter.CourtId ?? 0) > 0)
                courtIdWhere = x => x.Case.CourtId == filter.CourtId;
            else
            {
                if (filter.IsFindCoordinatorCourt)
                {
                    List<SelectListItem> courts = await mediationCommonService.GetDDL_MediationCoordinatorCourt(false);
                    int[] courtIds = courts.Select(x => int.Parse(x.Value)).ToArray();
                    courtIdWhere = x => courtIds.Contains(x.Case.CourtId);
                }
            }

            Expression<Func<MediationCaseSession, bool>> mediatorIdWhere = x => true;
            if ((filter.MediatorId ?? 0) > 0)
                mediatorIdWhere = x => x.Case.MediationCaseMediators.Any(m => m.MediationMediatorId == filter.MediatorId &&
                                                                              m.DateExpired == null);

            Expression<Func<MediationCaseSession, bool>> dateSearch = x => true;
            if (filter.DateFrom != null || filter.DateTo != null)
                dateSearch = x => x.Case.RegDate >= filter.DateFrom && x.Case.RegDate <= filter.DateTo;

            Expression<Func<MediationCaseSession, bool>> dateSessionWhere = x => true;
            if (filter.SessionDateFrom != null || filter.SessionDateTo != null)
                dateSessionWhere = x => x.DateFrom >= filter.SessionDateFrom && x.DateFrom <= filter.SessionDateTo;

            Expression<Func<MediationCaseSession, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<MediationCaseSession, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<MediationCaseSession, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            Expression<Func<MediationCaseSession, bool>> caseCodeSubIdWhere = x => true;
            if ((filter.CaseCodeSubId ?? 0) > 0)
                caseCodeSubIdWhere = x => x.Case.CaseCodeSubId == filter.CaseCodeSubId;

            Expression<Func<MediationCaseSession, bool>> mediationProcedureIdWhere = x => true;
            if (filter.MediationProcedureId > 0)
                mediationProcedureIdWhere = x => x.Case.MediationProcedureId == filter.MediationProcedureId;

            Expression<Func<MediationCaseSession, bool>> judgeReporterSearch = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterSearch = x => x.Case
                                            .CaseLawUnits.Any(a => a.CaseSessionId == null &&
                                                                   (a.DateTo ?? dateNow).Date >= dateNow.Date &&
                                                                   a.LawUnitId == filter.JudgeReporterId &&
                                                                   a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);

            return repo.AllReadonly<MediationCaseSession>()
                       .Where(mediationWhere)
                       .Where(courtIdWhere)
                       .Where(dateSearch)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(caseCodeSubIdWhere)
                       .Where(judgeReporterSearch)
                       .Where(mediationProcedureIdWhere)
                       .Where(mediatorIdWhere)
                       .Where(dateSessionWhere)
                       .Where(x => x.DateExpired == null)
                       .Select(x => new MediationCaseSessionVM
                       {
                           CaseId = x.CaseId,
                           CourtLabel = x.Court.Label,
                           CaseRegNumber = x.Case.RegNumber,
                           CaseCodeName = x.Case.CaseCode.Code,
                           CaseCodeSubName = x.Case.CaseCodeSub.Code,
                           InformationMeetingDateFrom = (NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId) &&
                                                        x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held) ? x.DateFrom : null,
                           InformationMeetingDateTo = (NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId) &&
                                                      x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held) ? x.DateTo : null,
                           InformationMeetingLabel = (NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId) &&
                                                     x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held) ? x.MediationState.Label : string.Empty,
                           InformationMeetingLocation = (NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId) &&
                                                        x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held) ? x.MediationLocation.Label : string.Empty,
                           InformationMeetingScheduled = (NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId) &&
                                                         x.MediationStateId != NomenclatureConstants.MediationStateConstants.Held) ? x.MediationState.Label + " (" + x.DateFrom.ToString("dd.MM.yyyy HH:mm") + ")" : string.Empty,
                           ReasonNonImplementation = (NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId) &&
                                                     x.MediationStateId == NomenclatureConstants.MediationStateConstants.NotHeld) ? x.Description : string.Empty,
                           MediationProcedureDateFrom = x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held &&
                                                        NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) ? x.DateFrom : null,
                           MediationProcedureDateTo = x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held &&
                                                        NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) ? x.DateTo : null,
                           MediationProcedureLabel = x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held &&
                                                     NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) ? x.MediationState.Label + " (" + x.DateFrom.ToString("dd.MM.yyyy HH:mm") + ")" : string.Empty,
                           MediationProcedureLocation = x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held &&
                                                        NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) ? x.MediationLocation.Label : string.Empty,
                           MediationProcedureReasonTermination = x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held &&
                                                                 NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) ? x.Results
                                                                                                                                                      .Where(r => r.Result.MediationResultGroupId == NomenclatureConstants.MediationResultGroupConstants.Termination &&
                                                                                                                                                                  NomenclatureConstants.MmediationResultBaseConstants.MediationProcedureReasonTermination.Contains(r.MediationResultBaseId ?? 0) &&
                                                                                                                                                                  r.DateExpired == null)
                                                                                                                                                      .Select(r => r.ResultBase.Label)
                                                                                                                                                      .FirstOrDefault() : string.Empty,
                           MediationProcedureSessionCount = string.Empty,
                           MediationProcedureScheduled = NomenclatureConstants.MediationStateConstants.StateForEdit.Contains(x.MediationStateId ?? 0) &&
                                                         NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) ? x.MediationState.Label + " (" + x.DateFrom.ToString("dd.MM.yyyy HH:mm") + ")" : string.Empty,
                           ConcludedAgreement = x.Results.Any(r => r.DateExpired == null &&
                                                                   r.MediationResultBaseId == NomenclatureConstants.MmediationResultBaseConstants.ByReachingAagreement) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : string.Empty,
                           TerminationCaseSettlement = x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                    s.DateFrom >= x.Case.StartMediationDate &&
                                                                                    s.CaseSessionResults.Any(r => r.SessionResultId == NomenclatureConstants.CaseSessionResult.Agreement &&
                                                                                                                  r.DateExpired == null)) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : string.Empty,
                           TerminationCaseTerminationWithdrawalRefusal = x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                                      s.DateFrom >= x.Case.StartMediationDate &&
                                                                                                      s.CaseSessionResults.Any(r => r.SessionResultBaseId == NomenclatureConstants.CaseSessionResultBase.PostponeCancel &&
                                                                                                                                    r.DateExpired == null)) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : string.Empty,
                           PartiallyTerminationCaseSettlement = x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                             s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                           r.SessionResultId == NomenclatureConstants.CaseSessionResult.PartiallyTerminatedSettlementAfterMediation)) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : string.Empty,
                           PartiallyTerminationCaseTerminationWithdrawalRefusal = x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                                               s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                                             r.SessionResultId == NomenclatureConstants.CaseSessionResult.PartiallyTerminatedDuePartialWithdrawalWaiverClaimFollowingMediation)) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : string.Empty,
                           CaseMediators = string.Join(", ", x.Case.MediationCaseMediators.Where(m => m.DateExpired == null).Select(m => m.Mediator.Name)),
                           JudgeReporterName = x.Case.CaseSessions
                                                     .Where(s => s.DateExpired == null &&
                                                                 s.CaseSessionResults
                                                                  .Any(r => r.DateExpired == null &&
                                                                            r.SessionResultId == NomenclatureConstants.CaseSessionResult.ReferralInformationalMeetingMediation))
                                                     .Select(s => s.CaseLawUnits
                                                                   .Where(l => (l.CaseSessionId ?? 0) == s.Id &&
                                                                               (l.DateTo ?? dateNow) >= dateNow &&
                                                                               l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                   .Select(l => l.LawUnit.FullName)
                                                                   .FirstOrDefault())
                                                     .FirstOrDefault()
                       });
        }

        /// <summary>
        /// Филтър за медиатор
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private static Expression<Func<MediationMediatorCenter, bool>> GetMediatorIdWhere(ReportWorkJudicialMediationCentersFilterVM filter)
        {
            Expression<Func<MediationMediatorCenter, bool>> mediatorIdWhere = x => true;
            if ((filter.MediatorId ?? 0) > 0)
                mediatorIdWhere = x => x.MediationMediatorId == filter.MediatorId;

            return mediatorIdWhere;
        }

        /// <summary>
        /// Филтър по съд
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private async Task<Expression<Func<MediationMediatorCenter, bool>>> GetCourtIdWhere(ReportWorkJudicialMediationCentersFilterVM filter)
        {
            Expression<Func<MediationMediatorCenter, bool>> courtIdWhere = x => true;
            if ((filter.CourtId ?? 0) > 0)
                courtIdWhere = x => x.Center.Courts.Any(c => c.CourtId == filter.CourtId);
            else
            {
                if (filter.IsFindCoordinatorCourt)
                {
                    List<SelectListItem> courts = await mediationCommonService.GetDDL_MediationCoordinatorCourt(false);
                    int[] courtIds = courts.Select(x => int.Parse(x.Value)).ToArray();
                    courtIdWhere = x => x.Center.Courts.Any(c => courtIds.Contains(c.CourtId));
                }
            }

            return courtIdWhere;
        }

        /// <summary>
        /// Филтър за медиатор
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private static Expression<Func<MediationMediator, bool>> GetMediationMediatorMediatorIdWhere(ReportWorkJudicialMediationCentersFilterVM filter)
        {
            Expression<Func<MediationMediator, bool>> mediatorIdWhere = x => true;
            if ((filter.MediatorId ?? 0) > 0)
                mediatorIdWhere = x => x.Id == filter.MediatorId;

            return mediatorIdWhere;
        }

        /// <summary>
        /// Филтър по съд
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private async Task<Expression<Func<MediationMediator, bool>>> GetMediationMediatorCourtIdWhere(ReportWorkJudicialMediationCentersFilterVM filter)
        {
            Expression<Func<MediationMediator, bool>> courtIdWhere = x => true;
            if ((filter.CourtId ?? 0) > 0)
                courtIdWhere = x => x.Centers.Any(c => c.Center.Courts.Any(cc => cc.CourtId == filter.CourtId));
            else
            {
                if (filter.IsFindCoordinatorCourt)
                {
                    List<SelectListItem> courts = await mediationCommonService.GetDDL_MediationCoordinatorCourt(false);
                    int[] courtIds = courts.Select(x => int.Parse(x.Value)).ToArray();
                    courtIdWhere = x => x.Centers.Any(c => c.Center.Courts.Any(cc => courtIds.Contains(cc.CourtId)));
                }
            }

            return courtIdWhere;
        }

        /// <summary>
        /// Метод извличащ данни за медиатори и центрове
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private async Task<List<MediatorDataVM>> GetMediatorCenterDatas(ReportWorkJudicialMediationCentersFilterVM filter)
        {
            return await repo.AllReadonly<MediationMediatorCenter>()
                             .Where(GetMediatorIdWhere(filter))
                             .Where(await GetCourtIdWhere(filter))
                             .Select(x => new MediatorDataVM
                             {
                                 MediatorId = x.MediationMediatorId,
                                 MediatorName = x.Mediator.Name,
                                 MediationCenterId = x.MediationCenterId,
                                 MediationCenterName = x.Center.Name,
                                 CourtIds = x.Center.Courts.Select(c => c.CourtId).ToArray()
                             })
                             .OrderBy(x => x.MediatorName)
                             .ThenBy(x => x.MediationCenterName)
                             .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ данни за медиатори и центрове
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private async Task<List<MediatorDataVM>> GetMediatorDatas(ReportWorkJudicialMediationCentersFilterVM filter)
        {
            return await repo.AllReadonly<MediationMediator>()
                             .Where(GetMediationMediatorMediatorIdWhere(filter))
                             .Where(await GetMediationMediatorCourtIdWhere(filter))
                             .Select(x => new MediatorDataVM
                             {
                                 MediatorId = x.Id,
                                 MediatorName = x.Name,
                             })
                             .OrderBy(x => x.MediatorName)
                             .ToListAsync();
        }

        /// <summary>
        /// Метод връщащ продължителност от списък със срещи
        /// </summary>
        /// <param name="sessions">Списък със срещи</param>
        /// <returns></returns>
        private static string GetDurationText(List<MediationSessionDataVM> sessions)
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            foreach (var duration in sessions)
            {
                TimeSpan timeSpan = (duration.DateTo ?? DateTime.Now).Subtract(duration.DateFrom);
                hours += timeSpan.Hours;
                minutes += timeSpan.Minutes;
                seconds += timeSpan.Seconds;
            }

            TimeSpan returnTimeSpan = new TimeSpan(hours, minutes, seconds);
            return string.Format("{0:00}:{1:00}:{2:00}", returnTimeSpan.Hours, returnTimeSpan.Minutes, returnTimeSpan.Seconds);
        }

        /// <summary>
        /// Филтър за среща за медиация по съд
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        private static Expression<Func<MediationCaseSession, bool>> GetMediationSessionCourtIdWhere(ReportWorkJudicialMediationCentersFilterVM filter)
        {
            Expression<Func<MediationCaseSession, bool>> courtIdWhere = x => true;
            if ((filter.CourtId ?? 0) > 0)
                courtIdWhere = x => x.CourtId == filter.CourtId;

            return courtIdWhere;
        }

        /// <summary>
        /// Филтър за среща за медиация по център за медиация
        /// </summary>
        /// <param name="mediationCenterId">Идентификатор на център</param>
        /// <returns></returns>
        private static Expression<Func<MediationCaseSession, bool>> GetMediationSessionMediationCenterIdWhere(int mediationCenterId)
        {
            Expression<Func<MediationCaseSession, bool>> mediationCenterIdWhere = x => true;
            if (mediationCenterId > 0)
                mediationCenterIdWhere = x => x.MediationCenterId == mediationCenterId;

            return mediationCenterIdWhere;
        }

        /// <summary>
        /// Метод връщащ списък със срещи за медиация за медиатор
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="mediator">Медиатор</param>
        /// <returns></returns>
        private async Task<List<MediationSessionDataVM>> GetMediationSessionsForMediator(ReportWorkJudicialMediationCentersFilterVM filter, MediatorDataVM mediator)
        {
            DateTime dateNowAdd100 = DateTime.Now.AddYears(100).MakeEndDate();
            filter.DateTo = filter.DateTo.MakeEndDate();

            Expression<Func<MediationCaseSession, bool>> caseGroupWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<MediationCaseSession, bool>> caseTypeWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<MediationCaseSession, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            Expression<Func<MediationCaseSession, bool>> caseCodeSubIdWhere = x => true;
            if ((filter.CaseCodeSubId ?? 0) > 0)
                caseCodeSubIdWhere = x => x.Case.CaseCodeSubId == filter.CaseCodeSubId;

            return await repo.AllReadonly<MediationCaseSession>()
                             .Where(x => x.DateFrom >= filter.DateFrom)
                             .Where(x => x.DateFrom <= filter.DateTo)
                             .Where(x => x.Case.MediationCaseMediators.Any(m => m.MediationMediatorId == mediator.MediatorId))
                             .Where(x => x.DateFrom >= x.Case.MediationCaseMediators.Where(m => m.MediationMediatorId == mediator.MediatorId).Select(m => m.DateFrom).FirstOrDefault())
                             .Where(x => x.DateFrom <= x.Case.MediationCaseMediators.Where(m => m.MediationMediatorId == mediator.MediatorId).Select(m => m.DateTo ?? dateNowAdd100).FirstOrDefault())
                             .Where(x => x.MediationStateId == NomenclatureConstants.MediationStateConstants.Held)
                             .Where(GetMediationSessionMediationCenterIdWhere(mediator.MediationCenterId))
                             .Where(GetMediationSessionCourtIdWhere(filter))
                             .Where(caseGroupWhere)
                             .Where(caseTypeWhere)
                             .Where(caseCodeIdsWhere)
                             .Where(caseCodeSubIdWhere)
                             .Select(x => new MediationSessionDataVM
                             {
                                 CaseId = x.CaseId,
                                 MediationTypeId = x.MediationTypeId,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo,
                                 MediationTypeChoiceMediatorId = x.Case.MediationCaseMediators.Where(m => m.MediationMediatorId == mediator.MediatorId).Select(m => m.MediationTypeChoiceMediatorId).FirstOrDefault(),
                                 TerminationMediationResultGroup = x.Results.Any(r => r.Result.MediationResultGroupId == NomenclatureConstants.MediationResultGroupConstants.Termination),
                                 CaseTerminationMediationResultGroup = x.Case.MediationCaseSessions.Where(m => m.DateFrom >= filter.DateFrom)
                                                                                                   .Where(m => m.DateFrom <= filter.DateTo)
                                                                                                   .Where(m => m.DateFrom >= m.Case.MediationCaseMediators.Where(n => n.MediationMediatorId == mediator.MediatorId).Select(n => n.DateFrom).FirstOrDefault())
                                                                                                   .Where(m => m.DateFrom <= m.Case.MediationCaseMediators.Where(n => n.MediationMediatorId == mediator.MediatorId).Select(n => n.DateTo ?? dateNowAdd100).FirstOrDefault())
                                                                                                   .Where(m => m.MediationStateId == NomenclatureConstants.MediationStateConstants.Held)
                                                                                                   .Any(m => m.Results.Any(r => r.Result.MediationResultGroupId == NomenclatureConstants.MediationResultGroupConstants.Termination)),
                                 Appraisals = x.Appraisals.Where(a => a.CaseMediator.MediationMediatorId == mediator.MediatorId)
                                                            .Select(a => new AppraisalDataVM() 
                                                            { 
                                                                SumAppraisal = a.TypeAppraisal == NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Detailed ? a.Data.Sum(d => d.Rating)
                                                                                                                                                                             : a.Data.Sum(d => d.Rating * (d.PointAppraisal.MultiplicationValue ?? 0))
                                                            })
                                                            .ToList(),
                                 TerminationCaseSettlement = x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                          s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                        NomenclatureConstants.CaseSessionResult.TerminationCaseSettlement.Contains(r.SessionResultId) &&
                                                                                                                        r.SessionResultBaseId == NomenclatureConstants.CaseSessionResultBase.AgreementReachedBetweenParties)),
                                 TerminationCaseTerminationWithdrawalRefusal = x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                                            s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                                          NomenclatureConstants.CaseSessionResult.TerminationCaseTerminationWithdrawalRefusal.Contains(r.SessionResultId) &&
                                                                                                                                          r.SessionResultBaseId == NomenclatureConstants.CaseSessionResultBase.WithdrawalWaivingClaim)),
                                 PartiallyTerminationCaseSettlement = x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                                   s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                                 r.SessionResultId == NomenclatureConstants.CaseSessionResult.PartiallyTerminatedSettlementAfterMediation)),
                                 PartiallyTerminationCaseTerminationWithdrawalRefusal = x.Case.CaseSessions.Any(s => s.DateExpired == null &&
                                                                                                                     s.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                                                                   r.SessionResultId == NomenclatureConstants.CaseSessionResult.PartiallyTerminatedDuePartialWithdrawalWaiverClaimFollowingMediation)),
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Метод пълнещ обект ReportWorkJudicialMediationCentersVM за отчет за работата на съдебните центрове по медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="mediator">Медиатор</param>
        /// <returns></returns>
        private async Task<ReportWorkJudicialMediationCentersVM> FillReportWorkJudicialMediationCenters(ReportWorkJudicialMediationCentersFilterVM filter, MediatorDataVM mediator)
        {
            List<MediationSessionDataVM> mediationSessions = await GetMediationSessionsForMediator(filter, mediator);

            return new()
            {
                MediatorName = mediator.MediatorName,
                MediationCenterName = mediator.MediationCenterName,
                InformationMeetingsCount = mediationSessions.Where(x => NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId)).Count(),
                InformationMeetingDurationText = GetDurationText(mediationSessions.Where(x => NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId)).ToList()),
                InformationMeetingsElectedPartyCaseCount = mediationSessions.Where(x => NomenclatureConstants.MediationTypeConstants.InformationMeetings.Contains(x.MediationTypeId) &&
                                                                                        x.MediationTypeChoiceMediatorId == NomenclatureConstants.MediationTypeChoiceMediatorConstants.ElectedPartyCase).Count(),
                MediationProceduresERTPCount = mediationSessions.Where(x => NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) &&
                                                                            x.TerminationMediationResultGroup).Count(),
                MediationProceduresECRTPCount = mediationSessions.Where(x => NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) &&
                                                                             x.CaseTerminationMediationResultGroup).Count(),
                MediationProceduresECRTPDurationText = GetDurationText(mediationSessions.Where(x => NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) &&
                                                                                                    x.CaseTerminationMediationResultGroup)
                                                                                        .ToList()),
                MediationProceduresElectedPartyCaseCount = mediationSessions.Where(x => NomenclatureConstants.MediationTypeConstants.Meetings.Contains(x.MediationTypeId) &&
                                                                                        x.CaseTerminationMediationResultGroup &&
                                                                                        x.MediationTypeChoiceMediatorId == NomenclatureConstants.MediationTypeChoiceMediatorConstants.ElectedPartyCase).Count(),
                Appraisals = string.Join(", ", mediationSessions.SelectMany(x => x.Appraisals)
                                                                .GroupBy(x => new { x.SumAppraisalTotalText })
                                                                .Select(g => g.Key.SumAppraisalTotalText + " " + g.Count() + " бр.")),
                TerminationCaseSettlementCount = mediationSessions.Where(x => x.TerminationCaseSettlement)
                                                                  .GroupBy(x => new { x.CaseId })
                                                                  .Select(g => g.Key.CaseId)
                                                                  .Count(),
                TerminationCaseTerminationWithdrawalRefusalCount = mediationSessions.Where(x => x.TerminationCaseTerminationWithdrawalRefusal)
                                                                                    .GroupBy(x => new { x.CaseId })
                                                                                    .Select(g => g.Key.CaseId)
                                                                                    .Count(),
                PartiallyTerminationCaseSettlementCount = mediationSessions.Where(x => x.PartiallyTerminationCaseSettlement)
                                                                           .GroupBy(x => new { x.CaseId })
                                                                           .Select(g => g.Key.CaseId)
                                                                           .Count(),
                PartiallyTerminationCaseTerminationWithdrawalRefusalCount = mediationSessions.Where(x => x.PartiallyTerminationCaseTerminationWithdrawalRefusal)
                                                                                             .GroupBy(x => new { x.CaseId })
                                                                                             .Select(g => g.Key.CaseId)
                                                                                             .Count(),
            };
        }

        /// <summary>
        /// Метод връщащ попълнен модел за отчет за работата на съдебните центрове по медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="mediators">Медиатори</param>
        /// <returns></returns>
        private async Task<List<ReportWorkJudicialMediationCentersVM>> GetReportWorkJudicialMediations(ReportWorkJudicialMediationCentersFilterVM filter, List<MediatorDataVM> mediators)
        {
            List<ReportWorkJudicialMediationCentersVM> result = [];

            foreach (var mediator in mediators)
                result.Add(await FillReportWorkJudicialMediationCenters(filter, mediator));

            return result;
        }

        /// <summary>
        /// Отчет за работата на съдебните центрове по медиация
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <param name="start">От коя позиция да дръпне данните</param>
        /// <param name="length">Дължина</param>
        /// <returns></returns>
        public async Task<DataTableResponseVM<ReportWorkJudicialMediationCentersVM>> GetReportWorkJudicialMediationCenters(ReportWorkJudicialMediationCentersFilterVM filter, int start, int length)
        {
            DataTableResponseVM<ReportWorkJudicialMediationCentersVM> result = new();

            List<MediatorDataVM> mediators = [];

            switch (filter.TypeReport)
            {
                case 1:
                    mediators = await GetMediatorCenterDatas(filter);
                    break;
                case 2:
                    mediators = await GetMediatorDatas(filter);
                    break;
                default:
                    mediators = await GetMediatorCenterDatas(filter);
                    break;
            }

            result.TotalCount = mediators.Count;
            result.Records = await GetReportWorkJudicialMediations(filter, mediators.Skip(start)
                                                                                    .Take(length)
                                                                                    .ToList());

            return result;
        }
    }
}
