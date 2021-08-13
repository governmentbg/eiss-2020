using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Rewrite.Internal.PatternSegments;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IReportService service;
        private readonly INomenclatureService nomService;
        private readonly ICourtDepartmentService departmentService;
        private readonly ICommonService commonService;
        private readonly IExcelReportService excelReportService;
        private readonly ICourtDepartmentService courtDepartmentService;
        private readonly IElasticService elasticService;

        /// <summary>
        /// Справки
        /// </summary>
        /// <param name="_service"></param>
        /// <param name="_nomService"></param>
        /// <param name="_departmentService"></param>
        /// <param name="_commonService"></param>
        /// <param name="_courtDepartmentService"></param>
        /// <param name="_excelReportService"></param>
        /// <param name="_elasticService"></param>
        public ReportController(IReportService _service, INomenclatureService _nomService,
                 ICourtDepartmentService _departmentService,
                 ICommonService _commonService,
                 ICourtDepartmentService _courtDepartmentService,
                 IExcelReportService _excelReportService,
                 IElasticService _elasticService)
        {
            service = _service;
            nomService = _nomService;
            departmentService = _departmentService;
            commonService = _commonService;
            excelReportService = _excelReportService;
            courtDepartmentService = _courtDepartmentService;
            elasticService = _elasticService;
        }

        public async Task<IActionResult> Excel()
        {

            return File(await excelReportService.GetReport(userContext.CourtId, 2020, 6), "application/octet-stream", $"Отчет{userContext.CourtName}.xlsx");
        }
        public IActionResult FillCourt()
        {
            excelReportService.FillAllCourts(2020, 6);
            //var obj=excelReportService.GetReportCases(83,1,2020, 6);
            excelReportService.FillAll_RS_CourtsDataSheets(2020, 6);
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Изходящ дневник
        /// </summary>
        /// <returns></returns>
        public IActionResult DocumentOutGoingReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DepartmentId_ddl = departmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

            var model = new DocumentOutFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;

            SetHelpFile(HelpFileValues.Register2);
            return View(model);
        }

        /// <summary>
        /// Експорт ексел наизходящ дневник
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DocumentOutGoingReport(DocumentOutFilterReportVM model)
        {
            var xlsBytes = service.DocumentOutGoingReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Експорт ексел наизходящ дневник
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DocumentOutGoingReportPrev(DocumentOutFilterReportVM model)
        {
            var xlsBytes = service.DocumentOutGoingReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Входящ дневник
        /// </summary>
        /// <returns></returns>
        public IActionResult DocumentInComingReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DepartmentId_ddl = departmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

            var model = new DocumentInFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            ViewBag.DocumentKindId_ddl = nomService.GetDDL_DocumentKind(DocumentConstants.DocumentDirection.Incoming, false, true);

            SetHelpFile(HelpFileValues.Register1);

            return View(model);
        }

        /// <summary>
        /// Експорт в ексел на входящ дневник
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DocumentInComingReportPrev(DocumentInFilterReportVM model)
        {
            var xlsBytes = service.DocumentInGoingReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Експорт в ексел на входящ дневник
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DocumentInComingReport(DocumentInFilterReportVM model)
        {
            var xlsBytes = service.DocumentInGoingReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Азбучник
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseAlphabeticalReport()
        {
            var model = new CaseAlphabeticalFilterVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            model.ReplaceEgn = true;
            ViewBag.CaseGroupId_ddl = ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>(false);
            ViewBag.DepartmentId_ddl = departmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            SetHelpFile(HelpFileValues.Register3);

            return View(model);
        }

        /// <summary>
        /// Експорт в ексел на азбучник
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseAlphabeticalReport(CaseAlphabeticalFilterVM model)
        {
            var xlsBytes = service.CaseAlphabetical_ToExcel(userContext.CourtId, model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Отвод/Самоотвод
        /// </summary>
        /// <returns></returns>
        public IActionResult DismisalReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>(false);
            ViewBag.DepartmentId_ddl = departmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

            var model = new DismisalReportFilterVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = new DateTime(DateTime.Now.Year, 12, 31);
            SetHelpFile(HelpFileValues.Register4);

            return View(model);
        }

        /// <summary>
        /// Експорт в ексел на отвод/самоотвод
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DismisalReport(DismisalReportFilterVM model)
        {
            var xlsBytes = service.DismisalReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Преведени суми през ПОС терминал
        /// </summary>
        /// <returns></returns>
        public IActionResult PaymentPosReport()
        {
            ViewBag.MoneyGroupId_ddl = nomService.GetDropDownList<MoneyGroup>();

            var model = new PaymentPosFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report32);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за Преведени суми през ПОС терминал
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataPaymentPosReport(IDataTablesRequest request, PaymentPosFilterReportVM model)
        {
            var data = service.PaymentPosReport_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт в ексел за Преведени суми през ПОС терминал
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PaymentPosReportExportExcel(PaymentPosFilterReportVM model)
        {
            var xlsBytes = service.PaymentPosReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Книга глоби и присъдени на държавата суми
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseObligationReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DepartmentId_ddl = departmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            var model = new CaseObligationFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register6);

            return View(model);
        }

        /// <summary>
        /// Експорт в ексел за Книга глоби и присъдени на държавата суми
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseObligationReport(CaseObligationFilterReportVM model)
        {
            var xlsBytes = service.CaseObligationReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Справка Глоби
        /// </summary>
        /// <returns></returns>
        public IActionResult FineReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();

            var model = new FineFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report33);

            return View(model);
        }

        /// <summary>
        /// Извличане на данните за Справка Глоби
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataFineReport(IDataTablesRequest request, FineFilterReportVM model)
        {
            var data = service.FineReport_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт в ексел за Справка Глоби
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult FineReportExportExcel(FineFilterReportVM model)
        {
            var xlsBytes = service.FineReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Държавни такси
        /// </summary>
        /// <returns></returns>
        public IActionResult StateFeeReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DocumentGroupId_ddl = nomService.GetDDL_DocumentGroupByDirection(DocumentConstants.DocumentDirection.Incoming);

            var model = new StateFeeFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report34);

            return View(model);
        }

        /// <summary>
        /// Извличане на Държавни такси
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataStateFeeReport(IDataTablesRequest request, StateFeeFilterReportVM model)
        {
            var data = service.StateFeeReport_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт на Държавни такси
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult StateFeeReportExportExcel(StateFeeFilterReportVM model)
        {
            var xlsBytes = service.StateFeeReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Възнаграждения
        /// </summary>
        /// <returns></returns>
        public IActionResult ObligationJuryReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.SessionTypeId_ddl = nomService.GetDropDownList<SessionType>();
            ViewBag.MoneyGroupId_ddl = nomService.GetDropDownList<MoneyGroup>();
            ViewBag.PersonType_ddl = nomService.GetDDL_ObligationJuryReportPersonType();

            var model = new ObligationJuryFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report35);

            return View(model);
        }

        /// <summary>
        /// Извличане на Възнаграждения
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataObligationJuryReport(IDataTablesRequest request, ObligationJuryFilterReportVM model)
        {
            var data = service.ObligationJuryReport_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт на Възнаграждения
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ObligationJuryReportExportExcel(ObligationJuryFilterReportVM model)
        {
            var xlsBytes = service.ObligationJuryReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Разносна книга
        /// </summary>
        /// <returns></returns>
        public IActionResult DeliveryBookReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();

            var model = new DeliveryBookFilterVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register7);

            return View(model);
        }

        /// <summary>
        /// Експорт на Разносна книга
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeliveryBookReport(DeliveryBookFilterVM model)
        {
            var xlsBytes = service.DeliveryBookReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Книга за закрити заседания
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseSessionPrivateReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DepartmentId_ddl = departmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            var model = new CaseSessionPrivateFilterReportVM();
            model.DateFrom = NomenclatureExtensions.ForceStartDate(new DateTime(DateTime.Now.Year, 1, 1));
            model.DateTo = NomenclatureExtensions.ForceEndDate(DateTime.Now);
            SetHelpFile(HelpFileValues.Register8);

            return View(model);
        }

        /// <summary>
        /// Експорт на Книга за закрити заседания
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseSessionPrivateReport(CaseSessionPrivateFilterReportVM model)
        {
            var xlsBytes = service.CaseSessionPrivateReportToExcelOneTemplate(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Книга по чл. 634в от ТЗ
        /// </summary>
        /// <returns></returns>
        public IActionResult InsolvencyReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>().Where(x => x.Value == NomenclatureConstants.CaseGroups.GrajdanskoDelo.ToString() ||
                                                                                     x.Value == NomenclatureConstants.CaseGroups.Trade.ToString() ||
                                                                                     x.Value == "-1" || x.Value == "-2").ToList();
            var model = new InsolvencyFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register19);

            return View(model);
        }

        /// <summary>
        /// Експорт на Книга по чл. 634в от ТЗ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult InsolvencyReport(InsolvencyFilterReportVM model)
        {
            var xlsBytes = service.InsolvencyReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Регистър по чл. 10, ал. 2 от ЗЗДН
        /// </summary>
        /// <returns></returns>
        public IActionResult ZzdnReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            var model = new ZzdnFilterReportVM();
            model.FromDateDocument = new DateTime(DateTime.Now.Year, 1, 1);
            model.ToDateDocument = DateTime.Now;
            SetHelpFile(HelpFileValues.Register15);

            return View(model);
        }

        /// <summary>
        /// Експорт на Регистър по чл. 10, ал. 2 от ЗЗДН
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ZzdnReport(ZzdnFilterReportVM model)
        {
            var xlsBytes = service.ZzdnReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Регистър на издадените европейски удостоверения за наследство
        /// </summary>
        /// <returns></returns>
        public IActionResult EuropeanHeritageReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            var model = new EuropeanHeritageFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register17);

            return View(model);
        }

        /// <summary>
        /// Експорт на Регистър на издадените европейски удостоверения за наследство
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EuropeanHeritageReport(EuropeanHeritageFilterReportVM model)
        {
            var xlsBytes = service.EuropeanHeritageReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Регистър на заявленията за достъп до обществена информация 
        /// </summary>
        /// <returns></returns>
        public IActionResult PublicInformationReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            var model = new PublicInformationFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register21);

            return View(model);
        }

        /// <summary>
        /// Регистър на заявленията за достъп до обществена информация 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PublicInformationReport(PublicInformationFilterReportVM model)
        {
            var xlsBytes = service.PublicInformationReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Регистър на заявленията за достъп до обществена информация 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PublicInformationReportPrev(PublicInformationFilterReportVM model)
        {
            var xlsBytes = service.PublicInformationReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Регистър на съдебните решения по чл. 235, ал. 5 ГПК
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseDecisionReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>().Where(x => x.Value != NomenclatureConstants.CaseGroups.NakazatelnoDelo.ToString()).ToList();
            var model = new CaseDecisionFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            model.WithActDescription = true;
            model.WithoutActDescriptionCaseRestriction = true;
            SetHelpFile(HelpFileValues.Register13);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни Регистър на съдебните решения по чл. 235, ал. 5 ГПК
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseDecisionReport(IDataTablesRequest request, CaseDecisionFilterReportVM model)
        {
            var data = service.CaseDecisionReport_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт Регистър на съдебните решения по чл. 235, ал. 5 ГПК
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseDecisionReport(CaseDecisionFilterReportVM model)
        {
            var url = Url.Action("Download", "Files", null, Request.Scheme);
            var xlsBytes = service.CaseDecisionReportToExcelOne(model, url);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Книга за приемане и отказ от наследство
        /// </summary>
        /// <returns></returns>
        public IActionResult HeritageReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            var model = new HeritageFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register14);

            return View(model);
        }

        /// <summary>
        /// Експорт Книга за приемане и отказ от наследство
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult HeritageReport(HeritageFilterReportVM model)
        {
            var xlsBytes = service.HeritageReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Описна книга първоинстанционни
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseFirstInstanceReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>(false);
            ViewBag.DepartmentId_ddl = departmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

            var model = new CaseFirstInstanceFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register11);

            return View(model);
        }

        /// <summary>
        /// Експорт Описна книга първоинстанционни
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseFirstInstanceReport(CaseFirstInstanceFilterReportVM model)
        {
            var xlsBytes = service.CaseFirstInstanceReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Регистър по чл. 39, т. 13 от ПАС
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseMigrationReturnReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DepartmentId_ddl = departmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

            var model = new CaseMigrationReturnFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register20);

            return View(model);
        }

        /// <summary>
        /// Експорт Регистър по чл. 39, т. 13 от ПАС
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseMigrationReturnReport(CaseMigrationReturnFilterReportVM model)
        {
            var xlsBytes = service.CaseMigrationReturnReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Експорт Регистър по чл. 39, т. 13 от ПАС
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseMigrationReturnReportPrev(CaseMigrationReturnFilterReportVM model)
        {
            var xlsBytes = service.CaseMigrationReturnReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Архивна книга
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseArchiveReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();

            var model = new CaseArchiveFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            model.WithPerson = false;
            SetHelpFile(HelpFileValues.Register18);

            return View(model);
        }

        /// <summary>
        /// Експорт Архивна книга
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseArchiveReport(CaseArchiveFilterReportVM model)
        {
            var xlsBytes = service.CaseArchiveReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Експорт Архивна книга
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseArchiveReportPrev(CaseArchiveFilterReportVM model)
        {
            var xlsBytes = service.CaseArchiveReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Регистър на изготвените съобщения за прекратен граждански брак
        /// </summary>
        /// <returns></returns>
        public IActionResult DivorceReport()
        {
            var model = new DivorceFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register16);

            return View(model);
        }

        /// <summary>
        /// Експорт Регистър на изготвените съобщения за прекратен граждански брак
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DivorceReport(DivorceFilterReportVM model)
        {
            var xlsBytes = service.DivorceReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Описна книга въззивни/касационни
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseSecondInstanceReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>(false);
            ViewBag.DepartmentId_ddl = departmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

            var model = new CaseSecondInstanceFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register12);

            return View(model);
        }

        /// <summary>
        /// Експорт Описна книга въззивни/касационни
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseSecondInstanceReport(CaseSecondInstanceFilterReportVM model)
        {
            var xlsBytes = service.CaseSecondInstanceReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Книга за изпълнение на присъдите
        /// </summary>
        /// <returns></returns>
        public IActionResult SentenceReport()
        {
            var model = new SentenceFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register10);

            return View(model);
        }

        /// <summary>
        /// Експорт Книга за изпълнение на присъдите
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SentenceReport(SentenceFilterReportVM model)
        {
            var xlsBytes = service.SentenceReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Регистър на изпълнителните листове
        /// </summary>
        /// <returns></returns>
        public IActionResult ExecListReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();

            var model = new ExecListFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register22);

            return View(model);
        }

        /// <summary>
        /// Експорт Регистър на изпълнителните листове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExecListReport(ExecListFilterReportVM model)
        {
            var xlsBytes = service.ExecListReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Експорт Регистър на изпълнителните листове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExecListReportPrev(ExecListFilterReportVM model)
        {
            var xlsBytes = service.ExecListReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Архивирани дела
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseArchiveListReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();

            var model = new CaseArchiveListFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report12);

            return View(model);
        }

        /// <summary>
        /// Извличане на данните Архивирани дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseArchiveListReport(IDataTablesRequest request, CaseArchiveListFilterReportVM model)
        {
            var data = service.CaseArchiveListReport_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт Архивирани дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseArchiveListReportExportExcel(CaseArchiveListFilterReportVM model)
        {
            var xlsBytes = service.CaseArchiveListReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Документи от изходящ регистър
        /// </summary>
        /// <returns></returns>
        public IActionResult DocumentOutListReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DocumentGroupId_ddl = nomService.GetDDL_DocumentGroupByDirection(DocumentConstants.DocumentDirection.OutGoing);

            var model = new DocumentOutListFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report28);

            return View(model);
        }

        /// <summary>
        /// Извличане на Документи от изходящ регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataDocumentOutListReport(IDataTablesRequest request, DocumentOutListFilterReportVM model)
        {
            var data = service.DocumentOutListReport_Select(userContext.CourtId, model, "<br>");

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт Документи от изходящ регистър
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DocumentOutListReportExportExcel(DocumentOutListFilterReportVM model)
        {
            var xlsBytes = service.DocumentOutListReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Справка ПОС терминал
        /// </summary>
        /// <returns></returns>
        public IActionResult PosDeviceReport()
        {
            ViewBag.PosDeviceTid_ddl = commonService.CourtPosDevice_SelectDDL(userContext.CourtId, false, true);

            var model = new PosDeviceFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report31);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни ПОС терминал
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataPosDeviceReport(IDataTablesRequest request, PosDeviceFilterReportVM model)
        {
            var data = service.PosDeviceReport_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт на ПОС терминал
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PosDeviceReportExportExcel(PosDeviceFilterReportVM model)
        {
            var xlsBytes = service.PosDeviceReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Срочна книга
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseSessionPublicReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>(false, false);
            ViewBag.DepartmentId_ddl = departmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            ViewBag.InstanceId_ddl = nomService.GetDDL_ByCourtTypeInstanceList(userContext.CourtInstances, false, false);
            var model = new CaseSessionPublicFilterReportVM();
            model.DateFrom = NomenclatureExtensions.ForceStartDate(new DateTime(DateTime.Now.Year, 1, 1));
            model.DateTo = NomenclatureExtensions.ForceEndDate(DateTime.Now);
            SetHelpFile(HelpFileValues.Register9);

            return View(model);
        }

        /// <summary>
        /// Експорт Срочна книга
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseSessionPublicReport(CaseSessionPublicFilterReportVM model)
        {
            var xlsBytes = service.CaseSessionPublicReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Справка за дела на други институции/инстанции
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseLinkReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DocumentGroupId_ddl = nomService.GetDDL_DocumentGroup(DocumentConstants.DocumentKind.InitialDocument);
            ViewBag.InstitutionTypeId_ddl = nomService.GetDropDownList<InstitutionType>();
            ViewBag.InstitutionCaseTypeId_ddl = nomService.GetDropDownList<InstitutionCaseType>();
            ViewBag.CaseMigrationTypeId_ddl = nomService.GetDDL_CaseMigrationType(NomenclatureConstants.CaseMigrationDirections.Incoming);

            var model = new CaseLinkFilterReportVM();
            model.DateFromCase = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateToCase = DateTime.Now;
            SetHelpFile(HelpFileValues.Report8);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка за дела на други институции/инстанции
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseLinkReport(IDataTablesRequest request, CaseLinkFilterReportVM model)
        {
            var data = service.CaseLinkReport_Select(userContext.CourtId, model, "<br>");

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт Справка за дела на други институции/инстанции
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseLinkReportExportExcel(CaseLinkFilterReportVM model)
        {
            var xlsBytes = service.CaseLinkReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Създаване на документация на базата данни
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GenerateTableDescription()
        {
            var data = service.TableDescription_Select();
            var html = await this.RenderPartialViewAsync("~/Views/Report/", "TableDescription.cshtml", data, true);
            var htmlBytes = System.Text.Encoding.UTF8.GetBytes(html);
            return File(htmlBytes, System.Net.Mime.MediaTypeNames.Text.Html, "TableDescription.html");
        }

        /// <summary>
        /// Справка влезли в сила присъди
        /// </summary>
        /// <returns></returns>
        public IActionResult SentenceListReport()
        {
            //За момента само наказателни да се показва
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>()
                                      .Where(x => x.Value == NomenclatureConstants.CaseGroups.NakazatelnoDelo.ToString())
                                      .ToList();
            ViewBag.CaseCodeId_ddl = nomService.GetDropDownList<CaseCode>();
            ViewBag.SentenceResultTypeId_ddl = nomService.GetDropDownList<SentenceResultType>();
            ViewBag.SessionResultId_ddl = nomService.GetDDL_SessionResultGrouping(NomenclatureConstants.SessionResultGroupings.SentenceListReport_Result);

            var model = new SentenceListFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report13);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка влезли в сила присъди
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSentenceListReport(IDataTablesRequest request, SentenceListFilterReportVM model)
        {
            var data = service.SentenceListReport_Select(userContext.CourtId, model, "<br>");

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт Справка влезли в сила присъди
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SentenceListReportExportExcel(SentenceListFilterReportVM model)
        {
            var xlsBytes = service.SentenceListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Справка актове за обезличаване
        /// </summary>
        /// <returns></returns>
        public IActionResult SessionActForDepersonalizeReport()
        {
            var model = new SessionActForDepersonalizeFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            ViewBag.IsFinalAct_ddl = nomService.GetDDL_IsFinalAct();
            ViewBag.CaseGroupIds_ddl = nomService.GetDropDownList<CaseGroup>(false);
            ViewBag.ActTypeIds_ddl = nomService.GetDropDownList<ActType>(false, false);
            ViewBag.CourtDepartmentId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            ViewBag.CourtDepartmentOtdelenieId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Otdelenie);
            SetHelpFile(HelpFileValues.ActsFordePersonalization);
            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка актове за обезличаване
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSessionActForDepersonalizeReport(IDataTablesRequest request, SessionActForDepersonalizeFilterReportVM model)
        {
            var data = service.SessionActForDepersonalizeReport_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка Съдени и осъдени лица
        /// </summary>
        /// <returns></returns>
        public IActionResult CasePersonDefendantListReport()
        {
            //За момента само наказателни да се показва
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>()
                                      .Where(x => x.Value == NomenclatureConstants.CaseGroups.NakazatelnoDelo.ToString())
                                      .ToList();
            ViewBag.CaseCodeId_ddl = nomService.GetDropDownList<CaseCode>();
            ViewBag.PersonMaturityId_ddl = nomService.GetDropDownList<PersonMaturity>();
            ViewBag.SentenceTypeId_ddl = nomService.GetDropDownList<SentenceType>();
            ViewBag.SentenceLawbaseId_ddl = nomService.GetDropDownList<SentenceLawbase>();
            ViewBag.SessionResultId_ddl = nomService.GetDDL_SessionResultGrouping(NomenclatureConstants.SessionResultGroupings.SentenceListReport_Result);

            var model = new CasePersonDefendantListFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report14);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка Съдени и осъдени лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCasePersonDefendantListReport(IDataTablesRequest request, CasePersonDefendantListFilterReportVM model)
        {
            var data = service.CasePersonDefendantListReport_Select(userContext.CourtId, model, "<br>");

            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт Справка Съдени и осъдени лица
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CasePersonDefendantListReportExportExcel(CasePersonDefendantListFilterReportVM model)
        {
            var xlsBytes = service.CasePersonDefendantListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Справка Постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseFirstInstanceListReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CaseCodeId_ddl = nomService.GetDropDownList<CaseCode>();
            ViewBag.CaseCreateFromId_ddl = nomService.GetDDL_CaseCreateFroms(NomenclatureConstants.CaseInstanceType.FirstInstance);

            var model = new CaseFirstInstanceListFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report15);

            return View(model);
        }

        /// <summary>
        /// Справка Постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseFirstInstanceListReport(IDataTablesRequest request, CaseFirstInstanceListFilterReportVM model)
        {
            var data = service.CaseFirstInstanceListReport_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка Постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseFirstInstanceListReportExportExcel(CaseFirstInstanceListFilterReportVM model)
        {
            var xlsBytes = service.CaseFirstInstanceListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseSecondInstanceListReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CaseCodeId_ddl = nomService.GetDropDownList<CaseCode>();
            ViewBag.CaseCreateFromId_ddl = nomService.GetDDL_CaseCreateFroms(NomenclatureConstants.CaseInstanceType.SecondInstance);

            var model = new CaseSecondInstanceListFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report16);

            return View(model);
        }

        /// <summary>
        /// Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseSecondInstanceListReport(IDataTablesRequest request, CaseSecondInstanceListFilterReportVM model)
        {
            var data = service.CaseSecondInstanceListReport_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseSecondInstanceListReportExportExcel(CaseSecondInstanceListFilterReportVM model)
        {
            var xlsBytes = service.CaseSecondInstanceListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        ///Справка Свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseFinishFirstInstanceListReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CaseCodeId_ddl = nomService.GetDropDownList<CaseCode>();
            ViewBag.SessionResultId_ddl = nomService.GetDDL_SessionResultFromRulesByFilter(0, userContext.CourtTypeId, true);

            var model = new CaseFinishListFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report17);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка Свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseFinishFirstInstanceListReport(IDataTablesRequest request, CaseFinishListFilterReportVM model)
        {
            var data = service.CaseFinishFirstInstanceListReport_Select(userContext.CourtId, model, "<br>");

            return request.GetResponse(data);
        }

        /// <summary>
        /// Excel Справка Свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseFinishFirstInstanceListReportExportExcel(CaseFinishListFilterReportVM model)
        {
            var xlsBytes = service.CaseFinishFirstInstanceListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        ///Справка Свършени дела за период – въззивни  дела
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseFinishSecondInstanceListReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CaseCodeId_ddl = nomService.GetDropDownList<CaseCode>();
            ViewBag.SessionResultId_ddl = nomService.GetDDL_SessionResultFromRulesByFilter(0, userContext.CourtTypeId, true);

            var model = new CaseFinishListFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report18);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка Свършени дела за период – въззивни  дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseFinishSecondInstanceListReport(IDataTablesRequest request, CaseFinishListFilterReportVM model)
        {
            var data = service.CaseFinishSecondInstanceListReport_Select(userContext.CourtId, model, "<br>");

            return request.GetResponse(data);
        }

        /// <summary>
        /// Excel Справка Свършени дела за период – въззивни  дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseFinishSecondInstanceListReportExportExcel(CaseFinishListFilterReportVM model)
        {
            var xlsBytes = service.CaseFinishSecondInstanceListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        public IActionResult CourtStatsReport(DateTime? date = null)
        {
            if (date.HasValue)
            {
                date = date.MakeEndDate();
            }
            else
            {
                date = DateTime.Now;
            }
            var xlsBytes = service.CourtStatsReport(date);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, $"CourtStatsReport {date:dd.MM.yyyy}.xlsx");
        }

        public IActionResult CourtGenericReport()
        {
            var xlsBytes = service.CourtReportGeneric();
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, $"CourtReportGeneric {DateTime.Now:dd.MM.yyyy}.xlsx");
        }

        /// <summary>
        /// Статистика за период
        /// </summary>
        /// <returns></returns>
        public IActionResult ExcelReportPeriod()
        {
            var model = new ExcelReportDataFilterVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;

            return View(model);
        }

        /// <summary>
        /// Статистика експорт
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExcelReportPeriod(ExcelReportDataFilterVM model)
        {
            return File(await excelReportService.GetReport_Test(userContext.CourtId, model.DateFrom, model.DateTo), "application/octet-stream", $"Отчет{userContext.CourtName}.xlsx");
        }

        /// <summary>
        /// Статистика към месец/година от вече готови данни
        /// </summary>
        /// <returns></returns>
        public IActionResult ExcelReport()
        {
            var model = new ExcelReportDataFilterVM();
            model.DateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);

            return View(model);
        }

        /// <summary>
        /// Експорт в ексел Статистика към месец/година от вече готови данни
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExcelReport(ExcelReportDataFilterVM model)
        {
            return File(await excelReportService.GetReport(userContext.CourtId, model.DateTo.Year, model.DateTo.Month), "application/octet-stream", $"Отчет{userContext.CourtName}.xlsx");
        }

        /// <summary>
        /// Генериране на статистика
        /// </summary>
        /// <returns></returns>
        public IActionResult StatisticsGenerate()
        {
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            ViewBag.messageMonth = "Ще бъде генерирана статистиката към месец " + date.Month + " година " + date.Year;
            return View();
        }

        [HttpPost]
        public IActionResult StatisticsSave()
        {
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            excelReportService.StatisticsGenerate(date);
            ViewBag.messageMonth = "Статистиката е генерирана успешно към месец " + date.Month + " година " + date.Year;

            return View("StatisticsGenerate");
        }

        public IActionResult SearchAct(string term)
        {
            return Json(elasticService.Search(userContext.CourtId, term));
        }
    }
}