using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using IOWebApplication.Infrastructure.Models.ViewModels.Report.ReportWorkJudicialMediationCenters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly IMediationCommonService mediationCommonService;

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
        /// <param name="_mediationCommonService"></param>
        public ReportController(IReportService _service, INomenclatureService _nomService,
                                ICourtDepartmentService _departmentService,
                                ICommonService _commonService,
                                ICourtDepartmentService _courtDepartmentService,
                                IExcelReportService _excelReportService,
                                IElasticService _elasticService,
                                IMediationCommonService _mediationCommonService)
        {
            service = _service;
            nomService = _nomService;
            departmentService = _departmentService;
            commonService = _commonService;
            excelReportService = _excelReportService;
            courtDepartmentService = _courtDepartmentService;
            elasticService = _elasticService;
            mediationCommonService = _mediationCommonService;
        }

        #region Изходящ дневник

        /// <summary>
        /// Изходящ дневник
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> DocumentOutGoingReport()
        {
            CurrentContext_SetObjectInfo("Извличане на изходящ дневник");

            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.DepartmentId_ddl = await departmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

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
        public async Task<IActionResult> DocumentOutGoingReport(DocumentOutFilterReportVM model)
        {
            var xlsBytes = await service.DocumentOutGoingReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Експорт ексел наизходящ дневник
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DocumentOutGoingReportPrev(DocumentOutFilterReportVM model)
        {
            var xlsBytes = await service.DocumentOutGoingReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Входящ дневник

        /// <summary>
        /// Входящ дневник
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> DocumentInComingReport()
        {
            CurrentContext_SetObjectInfo("Извличане на входящ дневник");

            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.DepartmentId_ddl = await departmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

            var model = new DocumentInFilterReportVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            ViewBag.DocumentKindId_ddl = await nomService.GetDDL_DocumentKindAsync(DocumentConstants.DocumentDirection.Incoming, false, true);

            SetHelpFile(HelpFileValues.Register1);

            return View(model);
        }

        /// <summary>
        /// Експорт в ексел на входящ дневник
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DocumentInComingReportPrev(DocumentInFilterReportVM model)
        {
            var xlsBytes = await service.DocumentInGoingReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Експорт в ексел на входящ дневник
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DocumentInComingReport(DocumentInFilterReportVM model)
        {
            var xlsBytes = await service.DocumentInGoingReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Азбучник

        /// <summary>
        /// Азбучник
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseAlphabeticalReport()
        {
            CurrentContext_SetObjectInfo("Извличане на азбучник");

            var model = new CaseAlphabeticalFilterVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = DateTime.Now;
            model.ReplaceEgn = true;
            ViewBag.CaseGroupId_ddl = ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.DepartmentId_ddl = await departmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

            SetHelpFile(HelpFileValues.Register3);

            return View(model);
        }

        /// <summary>
        /// Експорт в ексел на азбучник
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseAlphabeticalReport(CaseAlphabeticalFilterVM model)
        {
            var xlsBytes = await service.CaseAlphabetical_ToExcel(userContext.CourtId, model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Отвод/Самоотвод

        /// <summary>
        /// Отвод/Самоотвод
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> DismisalReport()
        {
            CurrentContext_SetObjectInfo("Извличане на отводи/самоотводи");

            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.DepartmentId_ddl = await departmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

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
        public async Task<IActionResult> DismisalReport(DismisalReportFilterVM model)
        {
            var xlsBytes = await service.DismisalReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Преведени суми през ПОС терминал

        /// <summary>
        /// Преведени суми през ПОС терминал
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> PaymentPosReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за преведени суми през ПОС терминал");

            ViewBag.MoneyGroupId_ddl = await nomService.GetDropDownListAsync<MoneyGroup>();

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
        public async Task<IActionResult> PaymentPosReportExportExcel(PaymentPosFilterReportVM model)
        {
            var xlsBytes = await service.PaymentPosReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Книга глоби и присъдени на държавата суми

        /// <summary>
        /// Книга глоби и присъдени на държавата суми
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseObligationReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за книга глоби и присъдени на държавата суми");

            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.DepartmentId_ddl = await departmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
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

        #endregion

        #region Справка Глоби

        /// <summary>
        /// Справка Глоби
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> FineReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за глоби");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
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
        public async Task<IActionResult> FineReportExportExcel(FineFilterReportVM model)
        {
            var xlsBytes = await service.FineReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Държавни такси

        /// <summary>
        /// Държавни такси
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> StateFeeReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за държавни такси");

            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.DocumentGroupId_ddl = await nomService.GetDDL_DocumentGroupByDirection(DocumentConstants.DocumentDirection.Incoming);

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
        public async Task<IActionResult> StateFeeReportExportExcel(StateFeeFilterReportVM model)
        {
            var xlsBytes = await service.StateFeeReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Възнаграждения

        /// <summary>
        /// Възнаграждения
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> ObligationJuryReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за възнаграждения");

            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.SessionTypeId_ddl = await nomService.GetDropDownListAsync<SessionType>();
            ViewBag.MoneyGroupId_ddl = await nomService.GetDropDownListAsync<MoneyGroup>();
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

        #endregion

        #region Разносна книга

        /// <summary>
        /// Разносна книга
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> DeliveryBookReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за разносна книга");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();

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
        public async Task<IActionResult> DeliveryBookReport(DeliveryBookFilterVM model)
        {
            var xlsBytes = await service.DeliveryBookReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Книга за закрити заседания

        /// <summary>
        /// Книга за закрити заседания
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseSessionPrivateReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за книга за закрити заседания");

            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.DepartmentId_ddl = await departmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
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
        public async Task<IActionResult> CaseSessionPrivateReport(CaseSessionPrivateFilterReportVM model)
        {
            var xlsBytes = await service.CaseSessionPrivateReportToExcelOneTemplate(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Книга по чл. 634в от ТЗ

        /// <summary>
        /// Книга по чл. 634в от ТЗ
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> InsolvencyReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за книга по чл. 634в от ТЗ");

            ViewBag.CaseGroupId_ddl = (await nomService.GetDropDownListAsync<CaseGroup>())
                                                       .Where(x => x.Value == NomenclatureConstants.CaseGroups.GrajdanskoDelo.ToString() ||
                                                                   x.Value == NomenclatureConstants.CaseGroups.Trade.ToString() ||
                                                                   x.Value == "-1" || x.Value == "-2")
                                                       .ToList();

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
        public async Task<IActionResult> InsolvencyReport(InsolvencyFilterReportVM model)
        {
            var xlsBytes = await service.InsolvencyReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Регистър по чл. 10, ал. 2 от ЗЗДН

        /// <summary>
        /// Регистър по чл. 10, ал. 2 от ЗЗДН
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> ZzdnReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за регистър по чл. 10, ал. 2 от ЗЗДН");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
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
        public async Task<IActionResult> ZzdnReport(ZzdnFilterReportVM model)
        {
            var xlsBytes = await service.ZzdnReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Регистър на издадените европейски удостоверения за наследство

        /// <summary>
        /// Регистър на издадените европейски удостоверения за наследство
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> EuropeanHeritageReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за регистър на издадените европейски удостоверения за наследство");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
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
        public async Task<IActionResult> EuropeanHeritageReport(EuropeanHeritageFilterReportVM model)
        {
            var xlsBytes = await service.EuropeanHeritageReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Регистър на заявленията за достъп до обществена информация

        /// <summary>
        /// Регистър на заявленията за достъп до обществена информация
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> PublicInformationReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за регистър на заявленията за достъп до обществена информация");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
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
        public async Task<IActionResult> PublicInformationReport(PublicInformationFilterReportVM model)
        {
            var xlsBytes = await service.PublicInformationReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Регистър на заявленията за достъп до обществена информация 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PublicInformationReportPrev(PublicInformationFilterReportVM model)
        {
            var xlsBytes = await service.PublicInformationReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Регистър на съдебните решения по чл. 235, ал. 5 ГПК

        /// <summary>
        /// Регистър на съдебните решения по чл. 235, ал. 5 ГПК
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseDecisionReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за регистър на съдебните решения по чл. 235, ал. 5 ГПК");
            ViewBag.CaseGroupId_ddl = (await nomService.GetDropDownListAsync<CaseGroup>()).Where(x => x.Value != NomenclatureConstants.CaseGroups.NakazatelnoDelo.ToString()).ToList();
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
        public async Task<IActionResult> CaseDecisionReport(CaseDecisionFilterReportVM model)
        {
            var url = Url.Action("Download", "Files", null, Request.Scheme);
            var xlsBytes = await service.CaseDecisionReportToExcelOne(model, url);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Книга за приемане и отказ от наследство

        /// <summary>
        /// Книга за приемане и отказ от наследство
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> HeritageReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за книга за приемане и отказ от наследство");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
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
        public async Task<IActionResult> HeritageReport(HeritageFilterReportVM model)
        {
            var xlsBytes = await service.HeritageReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Книга за приемане и отказ от наследство

        /// <summary>
        /// Книга за приемане и отказ от наследство
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> HeritageReportNew()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за книга за приемане и отказ от наследство");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            var model = new HeritageFilterReportVM();
            model.DateCreateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateCreateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Register14);

            return View(model);
        }

        /// <summary>
        /// Експорт Книга за приемане и отказ от наследство
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> HeritageReportNew(HeritageFilterReportVM model)
        {
            if (model.DateCreateFrom.Year < 2022)
            {
                ModelState.AddModelError(nameof(HeritageFilterReportVM.DateCreateFrom), "Не може да изберете дата преди 2022 година");
            }
            if (!ModelState.IsValid)
            {
                return View(nameof(HeritageReportNew), model);
            }

            var xlsBytes = await service.HeritageReportToExcelOneNew(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Описна книга първоинстанционни

        /// <summary>
        /// Описна книга първоинстанционни
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseFirstInstanceReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за описна книга първоинстанционни");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.DepartmentId_ddl = await departmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

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
        public async Task<IActionResult> CaseFirstInstanceReport(CaseFirstInstanceFilterReportVM model)
        {
            var xlsBytes = await service.CaseFirstInstanceReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Регистър по чл. 39, т. 13 от ПАС

        /// <summary>
        /// Регистър по чл. 39, т. 13 от ПАС
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseMigrationReturnReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за регистър по чл. 39, т. 13 от ПАС");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.DepartmentId_ddl = await departmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

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
        public async Task<IActionResult> CaseMigrationReturnReport(CaseMigrationReturnFilterReportVM model)
        {
            var xlsBytes = await service.CaseMigrationReturnReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Експорт Регистър по чл. 39, т. 13 от ПАС
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseMigrationReturnReportPrev(CaseMigrationReturnFilterReportVM model)
        {
            var xlsBytes = await service.CaseMigrationReturnReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Архивна книга

        /// <summary>
        /// Архивна книга
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseArchiveReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за архивна книга");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();

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
        public async Task<IActionResult> CaseArchiveReport(CaseArchiveFilterReportVM model)
        {
            var xlsBytes = await service.CaseArchiveReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Експорт Архивна книга
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseArchiveReportPrev(CaseArchiveFilterReportVM model)
        {
            var xlsBytes = await service.CaseArchiveReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Регистър на изготвените съобщения за прекратен граждански брак

        /// <summary>
        /// Регистър на изготвените съобщения за прекратен граждански брак
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult DivorceReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за регистър на изготвените съобщения за прекратен граждански брак");
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
        public async Task<IActionResult> DivorceReport(DivorceFilterReportVM model)
        {
            var xlsBytes = await service.DivorceReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Описна книга въззивни/касационни

        /// <summary>
        /// Описна книга въззивни/касационни
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseSecondInstanceReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за описна книга въззивни/касационни");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.DepartmentId_ddl = await departmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);

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
        public async Task<IActionResult> CaseSecondInstanceReport(CaseSecondInstanceFilterReportVM model)
        {
            var xlsBytes = await service.CaseSecondInstanceReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Книга за изпълнение на присъдите

        /// <summary>
        /// Книга за изпълнение на присъдите
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult SentenceReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за книга за изпълнение на присъдите");
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
        public async Task<IActionResult> SentenceReport(SentenceFilterReportVM model)
        {
            var xlsBytes = await service.SentenceReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Регистър на изпълнителните листове

        /// <summary>
        /// Регистър на изпълнителните листове
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> ExecListReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за регистър на изпълнителните листове");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();

            var model = new ExecListFilterReportVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };
            SetHelpFile(HelpFileValues.Register22);

            return View(model);
        }

        /// <summary>
        /// Експорт Регистър на изпълнителните листове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExecListReport(ExecListFilterReportVM model)
        {
            var xlsBytes = await service.ExecListReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Експорт Регистър на изпълнителните листове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExecListReportPrev(ExecListFilterReportVM model)
        {
            var xlsBytes = await service.ExecListReportToExcelOnePrev(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Архивирани дела

        /// <summary>
        /// Архивирани дела
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseArchiveListReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за архивирани дела");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();

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
        public async Task<IActionResult> CaseArchiveListReportExportExcel(CaseArchiveListFilterReportVM model)
        {
            var xlsBytes = await service.CaseArchiveListReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Документи от изходящ регистър

        /// <summary>
        /// Документи от изходящ регистър
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> DocumentOutListReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за документи от изходящ регистър");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.DocumentGroupId_ddl = await nomService.GetDDL_DocumentGroupByDirection(DocumentConstants.DocumentDirection.OutGoing);

            var model = new DocumentOutListFilterReportVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };
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
        public async Task<IActionResult> DocumentOutListReportExportExcel(DocumentOutListFilterReportVM model)
        {
            var xlsBytes = await service.DocumentOutListReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Справка ПОС терминал

        /// <summary>
        /// Справка ПОС терминал
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> PosDeviceReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка ПОС терминал");
            ViewBag.PosDeviceTid_ddl = await commonService.CourtPosDevice_SelectDDL(userContext.CourtId, false, true);

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
        public async Task<IActionResult> PosDeviceReportExportExcel(PosDeviceFilterReportVM model)
        {
            var xlsBytes = await service.PosDeviceReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Срочна книга

        /// <summary>
        /// Срочна книга
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseSessionPublicReport()
        {
            CurrentContext_SetObjectInfo("Извличане на данни за срочна книга");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false, false);
            ViewBag.DepartmentId_ddl = await departmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
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
        public async Task<IActionResult> CaseSessionPublicReport(CaseSessionPublicFilterReportVM model)
        {
            var xlsBytes = await service.CaseSessionPublicReportToExcelOne(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Справка за дела на други институции/инстанции

        /// <summary>
        /// Справка за дела на други институции/инстанции
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseLinkReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за дела на други институции/инстанции");
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.DocumentGroupId_ddl = await nomService.GetDDL_DocumentGroupAsync(DocumentConstants.DocumentKind.InitialDocument);
            ViewBag.InstitutionTypeId_ddl = await nomService.GetDropDownListAsync<InstitutionType>();
            ViewBag.InstitutionCaseTypeId_ddl = await nomService.GetDropDownListAsync<InstitutionCaseType>();
            ViewBag.CaseMigrationTypeId_ddl = await nomService.GetDDL_CaseMigrationType(NomenclatureConstants.CaseMigrationDirections.Incoming);

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

        #endregion

        #region Създаване на документация на базата данни

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

        #endregion

        #region Справка влезли в сила присъди

        /// <summary>
        /// Справка влезли в сила присъди
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> SentenceListReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка влезли в сила присъди");
            //За момента само наказателни да се показва
            var model = new SentenceListFilterReportVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };
            SetHelpFile(HelpFileValues.Report13);
            await ViewBagSentenceListReport();
            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка влезли в сила присъди
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSentenceListReport(IDataTablesRequest request, SentenceListFilterReportVM filter)
        {
            var data = service.SentenceListReport_Select(filter, "<br>");
            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт Справка влезли в сила присъди
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SentenceListReportExportExcel(SentenceListFilterReportVM model)
        {
            var xlsBytes = await service.SentenceListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Зареждане на номенклатури за справка влезли в сила присъди
        /// </summary>
        private async Task ViewBagSentenceListReport()
        {
            ViewBag.CaseGroupId_ddl = (await nomService.GetDropDownListAsync<CaseGroup>())
                                                       .Where(x => x.Value == NomenclatureConstants.CaseGroups.NakazatelnoDelo.ToString())
                                                       .ToList();
            ViewBag.CaseCodeId_ddl = await nomService.GetDropDownListAsync<CaseCode>();
            ViewBag.SentenceResultTypeId_ddl = await nomService.GetDropDownListAsync<SentenceResultType>();
            ViewBag.SessionResultId_ddl = await nomService.GetDDL_SessionResultGrouping(NomenclatureConstants.SessionResultGroupings.SentenceListReport_Result);
        }

        #endregion

        #region Актове подлежащи на обезличаване

        /// <summary>
        /// Справка актове за обезличаване
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> SessionActForDepersonalizeReport()
        {
            if (!await CheckAccessAsync(commonService, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Актове подлежащи на обезличаване");
            var model = new SessionActForDepersonalizeFilterReportVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };

            await ViewBagSessionActForDepersonalizeReport();
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
            var data = service.SessionActForDepersonalizeReport_Select(model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатурите за актове за обезличаване
        /// </summary>
        private async Task ViewBagSessionActForDepersonalizeReport()
        {
            ViewBag.IsFinalAct_ddl = nomService.GetDDL_IsFinalAct();
            ViewBag.CaseGroupIds_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.ActTypeIds_ddl = await nomService.GetDropDownListAsync<ActType>(false, false);
            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            ViewBag.CourtDepartmentOtdelenieId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Otdelenie);
        }

        #endregion

        #region Справка Съдени и осъдени лица

        /// <summary>
        /// Справка Съдени и осъдени лица
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CasePersonDefendantListReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка съдени и осъдени лица");
            var model = new CasePersonDefendantListFilterReportVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };
            SetHelpFile(HelpFileValues.Report14);
            await SetViewbagCasePersonDefendant();
            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка Съдени и осъдени лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCasePersonDefendantListReport(IDataTablesRequest request, CasePersonDefendantListFilterReportVM filter)
        {
            var data = service.CasePersonDefendantListReport_Select(filter, "<br>");
            return request.GetResponse(data);
        }

        /// <summary>
        /// Експорт Справка Съдени и осъдени лица
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CasePersonDefendantListReportExportExcel(CasePersonDefendantListFilterReportVM model)
        {
            var xlsBytes = await service.CasePersonDefendantListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        /// <summary>
        /// Метод зареждащ номенклатури за Справка Съдени и осъдени лица
        /// </summary>
        async Task SetViewbagCasePersonDefendant()
        {
            //За момента само наказателни да се показва
            ViewBag.CaseGroupId_ddl = (await nomService.GetDropDownListAsync<CaseGroup>())
                                                       .Where(x => x.Value == NomenclatureConstants.CaseGroups.NakazatelnoDelo.ToString())
                                                       .ToList();
            ViewBag.CaseCodeId_ddl = await nomService.GetDropDownListAsync<CaseCode>();
            ViewBag.PersonMaturityId_ddl = await nomService.GetDropDownListAsync<PersonMaturity>();
            ViewBag.SentenceTypeId_ddl = await nomService.GetDropDownListAsync<SentenceType>();
            ViewBag.SentenceLawbaseId_ddl = await nomService.GetDropDownListAsync<SentenceLawbase>();
            ViewBag.SessionResultId_ddl = await nomService.GetDDL_SessionResultGrouping(NomenclatureConstants.SessionResultGroupings.SentenceListReport_Result);
        }

        #endregion

        #region Справка постъпили дела за период – първоинстанционни дела

        /// <summary>
        /// Справка постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseFirstInstanceListReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка постъпили дела за период – първоинстанционни дела");
            var model = new CaseFirstInstanceListFilterReportVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };
            SetHelpFile(HelpFileValues.Report15);
            await ViewBagCaseFirstInstanceListReport();

            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка постъпили дела за период – първоинстанционни дела
        /// </summary>
        private async Task ViewBagCaseFirstInstanceListReport()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseCodeId_ddl = await nomService.GetDropDownListAsync<CaseCode>();
            ViewBag.CaseCreateFromId_ddl = nomService.GetDDL_CaseCreateFroms(NomenclatureConstants.CaseInstanceType.FirstInstance);
            ViewBag.ProcessPriorityId_ddl = await nomService.GetDropDownListAsync<ProcessPriority>();
            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav, (new List<int> { 1 }).ToArray());
        }

        /// <summary>
        /// Справка Постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataCaseFirstInstanceListReport(IDataTablesRequest request, CaseFirstInstanceListFilterReportVM filter)
        {
            var model = await service.CaseFirstInstanceListReportDataTable_Select(filter, request.Start, request.Length, request.GetSortedColumnsForOrderBy());
            return request.GetResponseServerPaging(model.Records, model.TotalCount);
        }

        /// <summary>
        /// Справка Постъпили дела за период – първоинстанционни дела - в ексел
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseFirstInstanceListReportExportExcel(CaseFirstInstanceListFilterReportVM model)
        {
            var xlsBytes = await service.CaseFirstInstanceListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Справка постъпили дела за период – първоинстанционни дела - със съд

        /// <summary>
        /// Справка постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseFirstInstanceWithCourtListReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка постъпили дела за период – първоинстанционни дела");
            var model = new CaseFirstInstanceListFilterReportVM()
            {
                DateFrom = DateTime.Now.AddMonths(-1),
                DateTo = DateTime.Now
            };
            SetHelpFile(HelpFileValues.Report15);
            await ViewBagCaseFirstInstanceWithCourtListReport();

            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка постъпили дела за период – първоинстанционни дела
        /// </summary>
        private async Task ViewBagCaseFirstInstanceWithCourtListReport()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            ViewBag.CaseCodeId_ddl = await nomService.GetDropDownListAsync<CaseCode>();
            ViewBag.CaseCreateFromId_ddl = nomService.GetDDL_CaseCreateFroms(NomenclatureConstants.CaseInstanceType.FirstInstance);
            ViewBag.ProcessPriorityId_ddl = await nomService.GetDropDownListAsync<ProcessPriority>();
            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav, (new List<int> { 1 }).ToArray());
        }

        /// <summary>
        /// Справка Постъпили дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataCaseFirstInstanceWithCourtListReport(IDataTablesRequest request, CaseFirstInstanceListFilterReportVM filter)
        {
            var model = await service.CaseFirstInstanceListReportDataTable_Select(filter, request.Start, request.Length, request.GetSortedColumnsForOrderBy());
            return request.GetResponseServerPaging(model.Records, model.TotalCount);
        }

        /// <summary>
        /// Справка Постъпили дела за период – първоинстанционни дела - в ексел
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseFirstInstanceWithCourtListReportExportExcel(CaseFirstInstanceListFilterReportVM model)
        {
            var xlsBytes = await service.CaseFirstInstanceWithCourtListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Справка постъпили дела за период – въззивни/касационни дела

        /// <summary>
        /// Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseSecondInstanceListReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка постъпили дела за период – въззивни дела");
            await ViewBagCaseSecondInstanceListReport();
            var model = new CaseSecondInstanceListFilterReportVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };
            SetHelpFile(HelpFileValues.Report16);
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка постъпили дела за период – въззивни/касационни дела
        /// </summary>
        private async Task ViewBagCaseSecondInstanceListReport()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseCodeId_ddl = await nomService.GetDropDownListAsync<CaseCode>();
            ViewBag.CaseCreateFromId_ddl = nomService.GetDDL_CaseCreateFroms(NomenclatureConstants.CaseInstanceType.SecondInstance);
            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav, (new List<int> { 2, 3 }).ToArray());
        }

        /// <summary>
        /// Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseSecondInstanceListReport(IDataTablesRequest request, CaseSecondInstanceListFilterReportVM filter)
        {
            var data = service.CaseSecondInstanceListReport_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка Постъпили дела за период – въззивни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseSecondInstanceListReportExportExcel(CaseSecondInstanceListFilterReportVM model)
        {
            var xlsBytes = await service.CaseSecondInstanceListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Справка свършени дела за период – първоинстанционни дела

        /// <summary>
        /// Справка свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseFinishFirstInstanceListReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка свършени дела за период – първоинстанционни дела");
            await ViewBagCaseFinishFirstInstanceListReport();
            var model = new CaseFinishListFilterReportVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };
            SetHelpFile(HelpFileValues.Report17);
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка свършени дела за период – първоинстанционни дела
        /// </summary>
        private async Task ViewBagCaseFinishFirstInstanceListReport()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseCodeId_ddl = await nomService.GetDropDownListAsync<CaseCode>();
            ViewBag.SessionResultId_ddl = await nomService.GetDDL_SessionResultFromRulesByFilter(0, userContext.CourtTypeId, true);
            ViewBag.DocumentTypeId_ddl = await nomService.GetDDL_DocumentTypeSortByName(true, DocumentConstants.DocumentKind.InitialDocument);
            ViewBag.ProcessPriorityId_ddl = await nomService.GetDropDownListAsync<ProcessPriority>();
            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav, (new List<int> { 1 }).ToArray());
        }

        /// <summary>
        /// Извличане на данни справка свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseFinishFirstInstanceListReport(IDataTablesRequest request, CaseFinishListFilterReportVM filter)
        {
            var data = service.CaseFinishFirstInstanceListReport_Select(filter, "<br>");
            return request.GetResponse(data);
        }

        /// <summary>
        /// Excel Справка Свършени дела за период – първоинстанционни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseFinishFirstInstanceListReportExportExcel(CaseFinishListFilterReportVM model)
        {
            var xlsBytes = await service.CaseFinishFirstInstanceListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Справка свършени дела за период – първоинстанционни дела - със съд

        /// <summary>
        /// Справка свършени дела за период – първоинстанционни дела - със съд
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseFinishFirstInstanceListReportWithCourt()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка свършени дела за период – първоинстанционни дела");
            await ViewBagCaseFinishFirstInstanceListReportWithCourt();
            var model = new CaseFinishListFilterReportVM()
            {
                DateFrom = DateTime.Now.AddMonths(-1),
                DateTo = DateTime.Now
            };
            SetHelpFile(HelpFileValues.Report17);
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка свършени дела за период – първоинстанционни дела - със съд
        /// </summary>
        private async Task ViewBagCaseFinishFirstInstanceListReportWithCourt()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseCodeId_ddl = await nomService.GetDropDownListAsync<CaseCode>();
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            ViewBag.SessionResultId_ddl = await nomService.GetDDL_SessionResultFromRulesByFilter(0, userContext.CourtTypeId, true);
            ViewBag.DocumentTypeId_ddl = await nomService.GetDDL_DocumentTypeSortByName(true, DocumentConstants.DocumentKind.InitialDocument);
            ViewBag.ProcessPriorityId_ddl = await nomService.GetDropDownListAsync<ProcessPriority>();
            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav, (new List<int> { 1 }).ToArray());
        }

        #endregion

        #region Справка свършени дела за период – въззивни/касационни дела

        /// <summary>
        /// Справка свършени дела за период – въззивни/касационни дела
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseFinishSecondInstanceListReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка свършени дела за период – въззивни/касационни дела");
            await ViewBagCaseFinishSecondInstanceListReport();
            var model = new CaseFinishListFilterReportVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };
            SetHelpFile(HelpFileValues.Report18);
            return View(model);
        }

        /// <summary>
        /// Зареждане на номенклатури за справка свършени дела за период – въззивни/касационни дела
        /// </summary>
        private async Task ViewBagCaseFinishSecondInstanceListReport()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseCodeId_ddl = await nomService.GetDropDownListAsync<CaseCode>();
            ViewBag.SessionResultId_ddl = await nomService.GetDDL_SessionResultFromRulesByFilter(0, userContext.CourtTypeId, true);
            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav, (new List<int> { 2, 3 }).ToArray());
        }

        /// <summary>
        /// Извличане на данни за свършени дела за период – въззивни/касационни дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseFinishSecondInstanceListReport(IDataTablesRequest request, CaseFinishListFilterReportVM filter)
        {
            var data = service.CaseFinishSecondInstanceListReport_Select(filter, "<br>");
            return request.GetResponse(data);
        }

        /// <summary>
        /// Excel Справка свършени дела за период – въззивни/касационни дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseFinishSecondInstanceListReportExportExcel(CaseFinishListFilterReportVM model)
        {
            var xlsBytes = await service.CaseFinishSecondInstanceListReportExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "Spravka.xlsx");
        }

        #endregion

        #region Справка съдебни актове

        /// <summary>
        /// Страница за визуализация на данни за справка съдебни актове
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = AuditConstants.Operations.List)]
        public async Task<IActionResult> ActReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка съдебни актове");
            CaseSessionActReportFilterVM filter = new CaseSessionActReportFilterVM()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            await SetViewbagActReport();
            SetHelpFile(HelpFileValues.Report21);

            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за справка съдебни актове
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataActReport(IDataTablesRequest request, CaseSessionActReportFilterVM filter)
        {
            var data = service.ActReport_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за справка съдебни актове
        /// </summary>
        private async Task SetViewbagActReport()
        {
            ViewBag.SessionResultId_ddl = await nomService.GetDDL_SessionResultAsync();
            ViewBag.ProcessPriorityId_ddl = await nomService.GetDropDownListAsync<ProcessPriority>();
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.ActTypeId_ddl = await nomService.GetDropDownListAsync<ActType>();
            ViewBag.DocumentGroupId_ddl = await nomService.GetDDL_DocumentGroupByDirection(DocumentConstants.DocumentDirection.Incoming);
            ViewBag.ActComplainResultId_ddl = await nomService.GetDDL_ActComplainResult();
            ViewBag.ActStateId_ddl = await nomService.GetDropDownListAsync<ActState>();
        }

        #endregion

        #region Дела с ненаписани съдебни актове от всички съдии

        /// <summary>
        /// Справка дела с ненаписани съдебни актове от всички съдии
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseWithoutFinalAct()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за дела с ненаписани съдебни актове от всички съдии");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFromNew = NomenclatureExtensions.GetStartYear(),
                DateToNew = NomenclatureExtensions.GetEndYear(),
            };
            await SetViewbagCaseWithoutFinalAct();
            SetHelpFile(HelpFileValues.Report6);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Дела с ненаписани съдебни актове от всички съдии
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseWithoutFinalAct(IDataTablesRequest request, CaseFilterReport filter)
        {
            var data = service.CaseWithoutFinalAct_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зарежда номенклатури за страница за Справка Дела с ненаписани съдебни актове от всички съдии
        /// </summary>
        async Task SetViewbagCaseWithoutFinalAct()
        {
            ViewBag.CaseGroupIds_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseClassificationId_ddl = await nomService.GetDropDownListAsync<Classification>();
            ViewBag.CaseStateId_ddl = await nomService.GetDropDownListAsync<CaseState>();
            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            ViewBag.CourtDepartmentOtdelenieId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Otdelenie);
        }

        #endregion

        #region Информация за лице

        /// <summary>
        /// Справка за Информация за страни
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CasePersonInformation()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Информация за страни");
            var model = new CasePersonFilterVM()
            {
                CaseCodeIds = new string[] {}
            };
            SetHelpFile(HelpFileValues.SidesInfo);
            await SetViewbagCasePersonInformation();
            return View(model);
        }

        /// <summary>
        /// Зиавлича данни за справка за Информация за страни
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCasePersonInformation(IDataTablesRequest request, CasePersonFilterVM filter)
        {
            var data = service.CasePersonInformation_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зарежда номенклатури за справка за Информация за страни
        /// </summary>
        async Task SetViewbagCasePersonInformation()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
        }

        #endregion

        #region Справка документи, постъпили чрез ЕЕСПП

        /// <summary>
        /// Справка документи, постъпили чрез ЕЕСПП
        /// </summary>
        /// <returns></returns>
        public IActionResult DocumentsReceivedEESPP()
        {
            CurrentContext_SetObjectInfo("Справка документи, постъпили чрез ЕЕСПП");
            var model = new DocumentsReceivedEESPPFilterVM()
            {
                DateReturnedFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateReturnedTo = DateTime.Now
            };
            return View(model);
        }

        /// <summary>
        /// Метод извличащ данни за справка документи, постъпили чрез ЕЕСПП
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataDocumentsReceivedEESPP(IDataTablesRequest request, DocumentsReceivedEESPPFilterVM filter)
        {
            var data = service.DocumentsReceivedEESPP_Select(filter);
            return request.GetResponse(data);
        }

        #endregion

        #region Движение на делата

        /// <summary>
        /// Справка движение на делата
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CaseMigrationReport()
        {
            CurrentContext_SetObjectInfo("Справка движение на делата");
            var model = new MigrationReportFilterVM()
            {
                DocumentRegDateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DocumentRegDateTo = DateTime.Now
            };
            await ViewBagCaseMigrationReport();
            return View(model);
        }

        /// <summary>
        /// Извличне на данни за справка за движение на делата
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseMigrationReport(IDataTablesRequest request, MigrationReportFilterVM filter)
        {
            var data = service.MigrationReport_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за движение на делата
        /// </summary>
        private async Task ViewBagCaseMigrationReport()
        {
            ViewBag.MigrationTypeId_ddl = await nomService.GetDDL_CaseMigrationType(NomenclatureConstants.CaseMigrationDirections.Outgoing);
            ViewBag.SendToCourtId_ddl = await commonService.CourtForDelivery_SelectDDLAsync(userContext.CourtId);
        }

        #endregion

        #region Справка Заявка 13

        /// <summary>
        /// Страница за специализирана справка - заявка 13
        /// </summary>
        /// <param name="filterTemplatesId">Идентификатор на шаблон за филтър</param>
        /// <returns></returns>
        public async Task<IActionResult> SpecializedReport(int? filterTemplatesId)
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран на специализирана справка");

            var model = new SpecializedReportIndexVM()
            {
                FilterTemplatesId = filterTemplatesId ?? -1,
                Filter = (filterTemplatesId > 0) ? (await commonService.GetFilterTemplatesSpecializedReportDataById(filterTemplatesId ?? 0)) : new SpecializedReportFilterVM()
                {
                    DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                    DateTo = DateTime.Now
                }
            };

            await SetViewbagSpecializedReport();
            SetHelpFile(HelpFileValues.Report39);
            return View(model);
        }

        /// <summary>
        /// Метод извличащ данни за специализирана справка
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSpecializedReport(IDataTablesRequest request, SpecializedReportFilterVM filter)
        {
            var data = service.SpecializedReport_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за специализирана справка - заявка 13
        /// </summary>
        private async Task SetViewbagSpecializedReport()
        {
            ViewBag.FilterTemplatesId_ddl = await commonService.GetFilterTemplates_SelectDDL(NomenclatureConstants.FilterTemplateTypeConstants.SpecializedReport);
            ViewBag.Filter_CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            ViewBag.Filter_InstanceId_ddl = await nomService.GetDropDownListAsync<CaseInstance>();
            ViewBag.Filter_CaseClassificationIds_ddl = await nomService.GetDropDownListAsync<Classification>();
            ViewBag.Filter_CaseGroupIds_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.Filter_ActComplainResultId_ddl = await nomService.GetDropDownListAsync<ActComplainResult>();
            ViewBag.Filter_CaseStateId_ddl = await nomService.GetDropDownListAsync<CaseState>();
        }

        /// <summary>
        /// Страница с данни за дела от движенията към дело от специализирана справка  - заявка 13
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public IActionResult SpecializedReportMigrations(int caseId)
        {
            var caseCase = service.GetById<Case>(caseId);
            ViewBag.CaseName = "Свързани дела за дело номер: " + caseCase.RegNumber;
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за специализирана справка движение по дело");
            return View(caseId);
        }

        /// <summary>
        /// Извличане на данни за дела от движенията към дело от специализирана справка  - заявка 13
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSpecializedReportMigrations(IDataTablesRequest request, int caseId)
        {
            var data = service.SpecializedReportMigrationCase_Select(caseId);
            return request.GetResponse(data);
        }

        #endregion

        #region Справка разпределение на дела по чл. 410 ГПК и чл. 417 ГПК

        /// <summary>
        /// Справка разпределение на дела
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CaseSelectionProtokolFastProcess()
        {
            CurrentContext_SetObjectInfo("Справка разпределение на дела");
            var model = new CaseSelectionProtokolFilterFastProcessVM()
            {
                DistributionDateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                DistributionDateTo = DateTime.Now
            };

            await ViewBagCaseSelectionProtokolFastProcess();
            return View(model);
        }

        /// <summary>
        /// Извличне на данни за справка за разпределение на дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseSelectionProtokolFast(IDataTablesRequest request, CaseSelectionProtokolFilterFastProcessVM filter)
        {
            var data = service.CaseSelectionProtokolFastProcess_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за справка разпределение на дела
        /// </summary>
        private async Task ViewBagCaseSelectionProtokolFastProcess()
        {
            ViewBag.CaseGroupIds_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            ViewBag.SelectionModeId_ddl = await nomService.GetDropDownListAsync<SelectionMode>();
        }

        #endregion

        #region Справка за дела по чл. 410 ГПК и чл. 417 ГПК

        /// <summary>
        /// Справка на заповедните производства
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CaseFastProcess()
        {
            CurrentContext_SetObjectInfo("Справка за дела по чл. 410 ГПК и чл. 417 ГПК");
            var model = new CaseFilterFastProcessVM()
            {
                CaseRegDateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                CaseRegDateTo = DateTime.Now
            };

            await ViewBagCaseFastProcess();
            return View(model);
        }

        /// <summary>
        /// Извличне на данни за справка на заповедните производства
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseFastProcess(IDataTablesRequest request, CaseFilterFastProcessVM filter)
        {
            var data = service.CaseFastProcess_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за справка на заповедните производства
        /// </summary>
        private async Task ViewBagCaseFastProcess()
        {
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            ViewBag.CaseStateId_ddl = await nomService.GetDropDownListAsync<CaseState>();
        }

        #endregion

        #region Справка за списък на длъжници/заявители за дела по чл. 410 ГПК и чл. 417 ГПК

        /// <summary>
        /// Справка за списък на длъжници/заявители за дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <returns></returns>
        public IActionResult ListDebtorsApplicantsFastProcess()
        {
            CurrentContext_SetObjectInfo("Справка списък на длъжници/заявители за дела по чл. 410 ГПК и чл. 417 ГПК");
            var model = new ListDebtorsApplicantsFilterFastProcessVM()
            {
                CaseRegDateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                CaseRegDateTo = DateTime.Now
            };

            return View(model);
        }

        /// <summary>
        /// Извличне на данни за списък на длъжници/заявители за дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataListDebtorsApplicantsFastProcess(IDataTablesRequest request, ListDebtorsApplicantsFilterFastProcessVM filter)
        {
            var data = service.ListDebtorsApplicantsFilterFastProcess_Select(filter);
            return request.GetResponse(data);
        }

        #endregion

        #region Справка за регистрираните документи за определен период по дела по чл. 410 ГПК и чл. 417 ГПК

        /// <summary>
        /// Справка за регистрираните документи за определен период
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> RegisteredDocumentsFastProcess()
        {
            CurrentContext_SetObjectInfo("Справка за регистрираните документи за определен период по дела по чл. 410 ГПК и чл. 417 ГПК");
            var model = new RegisteredDocumentsFilterFastProcessVM()
            {
                DocumentDateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DocumentDateTo = DateTime.Now
            };

            await ViewBagRegisteredDocumentsFastProcess();
            return View(model);
        }

        /// <summary>
        /// Извличне на данни за регистрираните документи за определен период
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataRegisteredDocumentsFastProcess(IDataTablesRequest request, RegisteredDocumentsFilterFastProcessVM filter)
        {
            var data = service.RegisteredDocumentsFastProcess_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за регистрираните документи за определен период
        /// </summary>
        private async Task ViewBagRegisteredDocumentsFastProcess()
        {
            ViewBag.DocumentDeliveryGroupId_ddl = await nomService.GetDropDownListAsync<DeliveryGroup>();
            ViewBag.DocumentCourtId_ddl = await nomService.GetDropDownListAsync<Court>();
        }

        #endregion

        #region Справка необработени документ по чл 410 и 417 от ЕИСС

        /// <summary>
        /// Справка необработени документ по чл 410 и 417 от ЕИСС
        /// </summary>
        /// <returns></returns>
        public IActionResult RawDocumentsFastProcess()
        {
            CurrentContext_SetObjectInfo("Справка необработени документ по чл 410 и 417");
            var model = new RawDocumentsFilterFastProcessVM()
            {
                DocumentDateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DocumentDateTo = DateTime.Now
            };

            return View(model);
        }

        /// <summary>
        /// Извличне на данни за необработени документ по чл 410 и 417 от ЕИСС
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataRawDocumentsFastProcess(IDataTablesRequest request, RawDocumentsFilterFastProcessVM filter)
        {
            var data = service.GetRawDocumentsFastProcess_Select(filter);
            return request.GetResponse(data);
        }

        #endregion

        #region Справка нотификации по дела по чл. 410 ГПК и чл. 417 ГПК

        /// <summary>
        /// Справка нотификации по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> WorkNotificationFatsProcess()
        {
            CurrentContext_SetObjectInfo("Справка нотификации  по дела по чл. 410 ГПК и чл. 417 ГПК");
            var model = new WorkNotificationFilterFatsProcessVM()
            {
                NotificationDateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                NotificationDateTo = DateTime.Now
            };

            await ViewBagWorkNotificationFatsProcess();
            return View(model);
        }

        /// <summary>
        /// Справка нотификации за дело по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<IActionResult> WorkNotificationCaseFatsProcess(int caseId)
        {
            CurrentContext_SetObjectInfo("Справка нотификации  по дела по чл. 410 ГПК и чл. 417 ГПК");
            var model = new WorkNotificationFilterFatsProcessVM()
            {
                NotificationDateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                NotificationDateTo = DateTime.Now,
                CaseId = caseId
            };

            await ViewBagWorkNotificationFatsProcess();
            return View(model);
        }

        /// <summary>
        /// Извличне на данни справка нотификации по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataWorkNotificationFatsProcess(IDataTablesRequest request, WorkNotificationFilterFatsProcessVM filter)
        {
            var data = service.WorkNotificationFatsProcess_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за справка нотификации по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        private async Task ViewBagWorkNotificationFatsProcess()
        {
            Expression<Func<WorkNotificationType, bool>> expressionWhere = x => (x.IsFastProcess ?? false);
            ViewBag.NotificationTypeId_ddl = await nomService.GetDDL_WorkNotificationType(expressionWhere);
        }

        /// <summary>
        /// Групова отмяна на нотификации
        /// </summary>
        /// <param name="notificationIds">Идентификатори на нотификации</param>
        /// <returns></returns>
        [DisableAudit]
        public IActionResult WorkNotificationsIsRead(string notificationIds)
        {
            var model = new WorkNotificationSetIsReadVM()
            {
                NotificationIds = notificationIds
            };

            return PartialView("WorkNotificationsIsRead", model);
        }

        /// <summary>
        /// Групова отмяна на нотификации
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [DisableAudit]
        [HttpPost]
        public async Task<IActionResult> WorkNotificationsIsRead(WorkNotificationSetIsReadVM model)
        {
            if (await service.WorkNotificationsSetTurnOff(model))
            {
                SetSuccessMessage("Нотификациите са успешно отменени.");
                AddAuditInfo(AuditConstants.Operations.Patch, $"Отменяне на нотификации {model.NotificationIds.Length}бр.", model.Description, SourceTypeSelectVM.WorkTask);
            }
            return RedirectToAction(nameof(WorkNotificationFatsProcess));
        }

        #endregion

        #region Справка изпълнителни листове по дела по чл. 410 ГПК и чл. 417 ГПК

        /// <summary>
        /// Справка изпълнителни листове по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ExecListFatsProcess()
        {
            CurrentContext_SetObjectInfo("Справка за регистрираните документи за определен период по дела по чл. 410 ГПК и чл. 417 ГПК");
            var model = new ExecListFilterFatsProcessVM()
            {
                ExecListSignDateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                ExecListSignDateTo = DateTime.Now
            };

            await ViewBagExecListFatsProcess();
            return View(model);
        }

        /// <summary>
        /// Извличне на данни за справка изпълнителни листове по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataExecListFatsProcess(IDataTablesRequest request, ExecListFilterFatsProcessVM filter)
        {
            var data = service.ExecListFatsProcess_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за справка изпълнителни листове по дела по чл. 410 ГПК и чл. 417 ГПК
        /// </summary>
        private async Task ViewBagExecListFatsProcess()
        {
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
        }

        #endregion

        #region Справка действителен зает щат в съд

        /// <summary>
        /// Справка действителен зает щат в съд
        /// </summary>
        /// <returns></returns>
        public IActionResult CourtJudgeCount()
        {
            CurrentContext_SetObjectInfo("Справка действителен зает щат в съд");
            return View();
        }

        /// <summary>
        /// Извличне на данни за справка действителен зает щат в съд
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCourtJudgeCount(IDataTablesRequest request)
        {
            var data = service.CourtJudgeCount_Select();
            return request.GetResponse(data);
        }

        #endregion

        #region Справка за актове по дела по чл. 410 ГПК и чл. 417 ГПК

        /// <summary>
        /// Справка действителен зает щат в съд
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseSessionActFastProcess()
        {
            CurrentContext_SetObjectInfo("Справка действителен зает щат в съд");

            CaseSessionActFastProcessFilterVM model = new()
            {
                ActDateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                ActDateTo = DateTime.Now,
            };

            ViewBag.ActTypeId_ddl = service.GetDDLActTypeForCaseSessionActFastProcess();

            return View(model);
        }

        /// <summary>
        /// Извличне на данни за справка действителен зает щат в съд
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseSessionActFastProcess(IDataTablesRequest request, CaseSessionActFastProcessFilterVM filter)
        {
            var data = service.CaseSessionActFastProcess_Select(filter);
            return request.GetResponse(data);
        }

        #endregion

        #region Справка за медиация

        /// <summary>
        /// Справка за медиация
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseMediation()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за медиация");
            var model = new Infrastructure.Models.ViewModels.Report.MediationCaseFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };
            
            await ViewBagCaseMediation();
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка за медиация
        /// </summary>
        private async Task ViewBagCaseMediation()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            ViewBag.MediatorId_ddl = await mediationCommonService.GetDDL_MediationMediatorsByCourt(0, DateTime.Now);
            ViewBag.MediationProcedureId_ddl = await nomService.GetDropDownListAsync<MediationProcedure>();
            ViewBag.CaseCodeSubId_ddl = await nomService.GetDDL_CaseCodeSub();
        }

        /// <summary>
        /// Справка за медиация
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataCaseMediation(IDataTablesRequest request, Infrastructure.Models.ViewModels.Report.MediationCaseFilterVM filter)
        {
            var data = await service.CaseMediation_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка за медиация
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseMediationCourt()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за медиация");
            var model = new Infrastructure.Models.ViewModels.Report.MediationCaseFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };

            await ViewBagCaseMediationCourt();
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка за медиация
        /// </summary>
        private async Task ViewBagCaseMediationCourt()
        {
            ViewBag.CourtId_ddl = await mediationCommonService.GetDDL_MediationCoordinatorCourt();
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.MediationProcedureId_ddl = await nomService.GetDropDownListAsync<MediationProcedure>();
            ViewBag.MediatorId_ddl = await mediationCommonService.GetDDL_MediationMediatorsByCourt(0, DateTime.Now);
            ViewBag.CaseCodeSubId_ddl = await nomService.GetDDL_CaseCodeSub();
        }

        /// <summary>
        /// Справка за медиация
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataCaseMediationCourt(IDataTablesRequest request, Infrastructure.Models.ViewModels.Report.MediationCaseFilterVM filter)
        {
            filter.IsFindCoordinatorCourt = true;
            var data = await service.CaseMediation_Select(filter);
            return request.GetResponse(data);
        }

        #endregion

        #region Справка по чл. 15

        /// <summary>
        /// Справка за медиация
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseMediationSession()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за медиация");
            var model = new Infrastructure.Models.ViewModels.Report.MediationCaseSessionFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };

            await ViewBagCaseMediationSession();
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка за медиация
        /// </summary>
        private async Task ViewBagCaseMediationSession()
        {
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.MediationProcedureId_ddl = await nomService.GetDropDownListAsync<MediationProcedure>();
            ViewBag.MediatorId_ddl = await mediationCommonService.GetDDL_MediationMediatorsByCourt(0, DateTime.Now);
            ViewBag.CaseCodeSubId_ddl = await nomService.GetDDL_CaseCodeSub();
        }

        /// <summary>
        /// Справка за медиация
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataCaseMediationSession(IDataTablesRequest request, Infrastructure.Models.ViewModels.Report.MediationCaseSessionFilterVM filter)
        {
            var data = await service.CaseMediationSession_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка за медиация
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> CaseMediationSessionCourt()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за медиация");
            var model = new Infrastructure.Models.ViewModels.Report.MediationCaseSessionFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };

            await ViewBagCaseMediationSessionCourt();
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка за медиация
        /// </summary>
        private async Task ViewBagCaseMediationSessionCourt()
        {
            ViewBag.CourtId_ddl = await mediationCommonService.GetDDL_MediationCoordinatorCourt();
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.MediationProcedureId_ddl = await nomService.GetDropDownListAsync<MediationProcedure>();
            ViewBag.MediatorId_ddl = await mediationCommonService.GetDDL_MediationMediatorsByCourt(0, DateTime.Now);
            ViewBag.CaseCodeSubId_ddl = await nomService.GetDDL_CaseCodeSub();
        }

        /// <summary>
        /// Справка за медиация
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataCaseMediationSessionCourt(IDataTablesRequest request, Infrastructure.Models.ViewModels.Report.MediationCaseSessionFilterVM filter)
        {
            filter.IsFindCoordinatorCourt = true;
            var data = await service.CaseMediationSession_Select(filter);
            return request.GetResponse(data);
        }

        #endregion

        #region Отчет за работата на съдебните центрове по медиация

        /// <summary>
        /// Отчет за работата на съдебните центрове по медиация
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> ReportWorkJudicialMediationCenters()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за медиация");
            var model = new ReportWorkJudicialMediationCentersFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };

            await ViewBagReportWorkJudicialMediationCenters();
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за отчет за работата на съдебните центрове по медиация
        /// </summary>
        private async Task ViewBagReportWorkJudicialMediationCenters()
        {
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            ViewBag.MediatorId_ddl = await mediationCommonService.GetDDL_MediationMediatorsByCourt(0, DateTime.Now);
        }

        /// <summary>
        /// Отчет за работата на съдебните центрове по медиация
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataReportWorkJudicialMediationCenters(IDataTablesRequest request, ReportWorkJudicialMediationCentersFilterVM filter)
        {
            DataTableResponseVM<ReportWorkJudicialMediationCentersVM> data = await service.GetReportWorkJudicialMediationCenters(filter, request.Start, request.Length < 0 ? 1000000 : request.Length);
            return request.GetResponseServerPaging(data.Records, data.TotalCount);
        }

        /// <summary>
        /// Отчет за работата на съдебните центрове по медиация
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> ReportWorkJudicialMediationCentersCurrentCourt()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за медиация");
            var model = new ReportWorkJudicialMediationCentersFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now,
            };

            await ViewBagReportWorkJudicialMediationCentersCurrentCourt();
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за отчет за работата на съдебните центрове по медиация
        /// </summary>
        private async Task ViewBagReportWorkJudicialMediationCentersCurrentCourt()
        {
            ViewBag.MediatorId_ddl = await mediationCommonService.GetDDL_MediationMediatorsByCoordinator(DateTime.Now);
        }

        /// <summary>
        /// Отчет за работата на съдебните центрове по медиация
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataReportWorkJudicialMediationCentersCurrentCourt(IDataTablesRequest request, ReportWorkJudicialMediationCentersFilterVM filter)
        {
            filter.IsFindCoordinatorCourt = true;
            DataTableResponseVM<ReportWorkJudicialMediationCentersVM> data = await service.GetReportWorkJudicialMediationCenters(filter, request.Start, request.Length < 0 ? 1000000 : request.Length);
            return request.GetResponseServerPaging(data.Records, data.TotalCount);
        }

        #endregion

        #region Справка за дейността на медиаторите

        /// <summary>
        /// Справка за дейността на медиаторите
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> InformationActivitiesMediators()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за медиация");
            var model = new ReportWorkJudicialMediationCentersFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now
            };

            await ViewBagInformationActivitiesMediators();
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка за дейността на медиаторите
        /// </summary>
        private async Task ViewBagInformationActivitiesMediators()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseCodeSubId_ddl = await nomService.GetDDL_CaseCodeSub();
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            ViewBag.MediatorId_ddl = await mediationCommonService.GetDDL_MediationMediatorsByCourt(0, DateTime.Now);
        }

        /// <summary>
        /// Справка за дейността на медиаторите
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataInformationActivitiesMediators(IDataTablesRequest request, ReportWorkJudicialMediationCentersFilterVM filter)
        {
            filter.TypeReport = 2;
            DataTableResponseVM<ReportWorkJudicialMediationCentersVM> data = await service.GetReportWorkJudicialMediationCenters(filter, request.Start, request.Length < 0 ? 1000000 : request.Length);
            return request.GetResponseServerPaging(data.Records, data.TotalCount);
        }

        /// <summary>
        /// Справка за дейността на медиаторите
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> InformationActivitiesMediatorsCurrentCourt()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за медиация");
            var model = new ReportWorkJudicialMediationCentersFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = DateTime.Now,
            };

            await ViewBagInformationActivitiesMediatorsCurrentCourt();
            return View(model);
        }

        /// <summary>
        /// Метод зареждащ номенклатури за справка за дейността на медиаторите
        /// </summary>
        private async Task ViewBagInformationActivitiesMediatorsCurrentCourt()
        {
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.CaseCodeSubId_ddl = await nomService.GetDDL_CaseCodeSub();
            ViewBag.MediatorId_ddl = await mediationCommonService.GetDDL_MediationMediatorsByCoordinator(DateTime.Now);
        }

        /// <summary>
        /// Справка за дейността на медиаторите
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataInformationActivitiesMediatorsCurrentCourt(IDataTablesRequest request, ReportWorkJudicialMediationCentersFilterVM filter)
        {
            filter.TypeReport = 2;
            filter.IsFindCoordinatorCourt = true;
            DataTableResponseVM<ReportWorkJudicialMediationCentersVM> data = await service.GetReportWorkJudicialMediationCenters(filter, request.Start, request.Length < 0 ? 1000000 : request.Length);
            return request.GetResponseServerPaging(data.Records, data.TotalCount);
        }

        #endregion

        #region Other

        public async Task<IActionResult> Excel()
        {
            return File(await excelReportService.GetReport(userContext.CourtId, 2020, 6, NomenclatureConstants.ExcelReportTemplateReportTypes.Normal), "application/octet-stream", $"Отчет{userContext.CourtName}.xlsx");
        }

        public async Task<IActionResult> ExcelSisma(int sheetIndex)
        {
            return File(await excelReportService.TestPrintSisma(userContext.CourtId, new DateTime(DateTime.Now.Year, 1, 1), DateTime.Now, sheetIndex), "application/octet-stream", $"Отчет{userContext.CourtName}.xlsx");
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
        /// Статистика медиация за период
        /// </summary>
        /// <returns></returns>
        public IActionResult ExcelReportPeriodMediation()
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
            var result = await excelReportService.GetReport_Test(userContext.CourtId, model.DateFrom, model.DateTo, NomenclatureConstants.ExcelReportTemplateReportTypes.Normal).ConfigureAwait(false);
            if (result != null)
                return File(result, "application/octet-stream", $"Отчет{userContext.CourtName}.xlsx");
            else
            {
                SetErrorMessage("Не е намерен темплейт за избрания съд");
                return RedirectToAction("ExcelReportPeriod");
            }
        }

        /// <summary>
        /// Статистика медиация експорт
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExcelReportPeriodMediation(ExcelReportDataFilterVM model)
        {
            var result = await excelReportService.GetReport_Test(userContext.CourtId, model.DateFrom, model.DateTo, NomenclatureConstants.ExcelReportTemplateReportTypes.Mediation).ConfigureAwait(false);
            if (result != null)
                return File(result, "application/octet-stream", $"Отчет{userContext.CourtName}.xlsx");
            else
            {
                SetErrorMessage("Не е намерен темплейт за избрания съд");
                return RedirectToAction("ExcelReportPeriodMediation");
            }
        }

        /// <summary>
        /// Статистика към месец/година от вече готови данни
        /// </summary>
        /// <returns></returns>
        public IActionResult ExcelReport()
        {
            var model = new ExcelReportDataFilterVM();
            model.DateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            AddAuditInfo(AuditConstants.Operations.GeneratingFile, "Статистическа отчетна форма към ВСС");

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
            return File(await excelReportService.GetReport(userContext.CourtId, model.DateTo.Year, model.DateTo.Month, NomenclatureConstants.ExcelReportTemplateReportTypes.Normal), "application/octet-stream", $"Отчет{userContext.CourtName}.xlsx");
        }

        /// <summary>
        /// Статистика медиация към месец/година от вече готови данни
        /// </summary>
        /// <returns></returns>
        public IActionResult ExcelReportMediation()
        {
            var model = new ExcelReportDataFilterVM();
            model.DateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            AddAuditInfo(AuditConstants.Operations.GeneratingFile, "Статистическа отчетна форма към ВСС медиация");

            return View(model);
        }

        /// <summary>
        /// Експорт в ексел Статистика медиация към месец/година от вече готови данни
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExcelReportMediation(ExcelReportDataFilterVM model)
        {
            return File(await excelReportService.GetReport(userContext.CourtId, model.DateTo.Year, model.DateTo.Month, NomenclatureConstants.ExcelReportTemplateReportTypes.Mediation), "application/octet-stream", $"Отчет{userContext.CourtName}.xlsx");
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
        public async Task<IActionResult> StatisticsSave()
        {
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            await excelReportService.StatisticsGenerate(date).ConfigureAwait(false);
            ViewBag.messageMonth = "Статистиката е генерирана успешно към месец " + date.Month + " година " + date.Year;
            return View("StatisticsGenerate");
        }

        /// <summary>
        /// Генериране на статистика медиация
        /// </summary>
        /// <returns></returns>
        public IActionResult StatisticsGenerateMediation()
        {
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            ViewBag.messageMonth = "Ще бъде генерирана статистиката медиация към месец " + date.Month + " година " + date.Year;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StatisticsSaveMediation()
        {
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            await excelReportService.StatisticsGenerateMediation(date).ConfigureAwait(false);
            ViewBag.messageMonth = "Статистиката медиация е генерирана успешно към месец " + date.Month + " година " + date.Year;
            return View("StatisticsGenerateMediation");
        }
        /// <summary>
        /// Проверка между 2 дати дали е по голамо от година дни
        /// </summary>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дата</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IsValidateDate(DateTime dateFrom, DateTime dateTo)
        {
            return Json(new { result = (dateTo - dateFrom).Days > 365 });
        }

        /// <summary>
        /// Проверка между 2 дати дали е по голамо от година дни
        /// </summary>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дата</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IsValidateDateMonth(DateTime dateFrom, DateTime dateTo)
        {
            return Json(new { result = (dateTo - dateFrom).Days > 31 });
        }

        #endregion
    }
}