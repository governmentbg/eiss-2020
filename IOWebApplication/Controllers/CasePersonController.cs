using DataTables.AspNet.Core;
using IO.RegixClient;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
//using DocumentFormat.OpenXml.Bibliography;

namespace IOWebApplication.Controllers
{
    public class CasePersonController : BaseController
    {
        private readonly ICasePersonService service;
        private readonly INomenclatureService nomService;
        private readonly ICaseSessionService sessionService;
        private readonly ICaseService caseService;
        private readonly ICommonService commonService;
        private readonly ICdnService cdnService;
        private readonly ICaseSessionActService caseSessionActService;
        private readonly IEisppService eisppService;
        private readonly IRegixReportService regixReportService;
        private readonly ICaseSessionDocService sesDocService;
        private readonly IDocumentService docService;
        private readonly ICaseMigrationService migService;

        public CasePersonController(ICasePersonService _service, INomenclatureService _nomService,
            ICaseSessionService _sessionService, ICommonService _commonService, ICdnService _cdnService,
            ICaseService _caseService, ICaseSessionActService _caseSessionActService,
            ICaseSessionDocService _sesDocService,
            IDocumentService _docService,
            ICaseMigrationService _migService,
            IEisppService _eisppService,
            IRegixReportService _regixReportService)
        {
            service = _service;
            nomService = _nomService;
            sessionService = _sessionService;
            commonService = _commonService;
            cdnService = _cdnService;
            caseService = _caseService;
            caseSessionActService = _caseSessionActService;
            sesDocService = _sesDocService;
            docService = _docService;
            migService = _migService;
            eisppService = _eisppService;
            regixReportService = _regixReportService;
        }

        public async Task<IActionResult> Index()
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Информация за страни");
            var model = new CasePersonFilterVM();
            SetHelpFile(HelpFileValues.SidesInfo);
            return View(model);
        }

        /// <summary>
        /// Извличане на данни за страни по дело/заседание
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId, int? caseSessionId)
        {
            var data = service.CasePerson_Select(caseId, caseSessionId, true, false, true);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Извличане на данни за страни по дело/заседание
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ListDataList(IDataTablesRequest request, int caseId, int? caseSessionId)
        {
            var orderColums = request.Columns.Where(x => x.Sort != null);

            var data = await service.CasePersonList_Select(caseId, caseSessionId, true, false, true, request.Start, request.Length < 0 ? 1000000 : request.Length, request.GetSortedColumnsForOrderBy());
            return request.GetResponseServerPaging(data.Records, data.TotalCount);
        }

        /// <summary>
        /// Извличане на страни по дело/заседание за уведомяване
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataNotification(IDataTablesRequest request, int caseId, int? caseSessionId)
        {
            var data = service.CasePerson_Select(caseId, caseSessionId, false, false, false).Where(x => x.ForNotification == true);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на страна
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Add(int caseId, int? caseSessionId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePerson, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            var caseModel = await service.GetReadonlyAsync<Case>(caseId);
            var model = new CasePersonVM()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                CaseSessionId = caseSessionId,
                DateFrom = DateTime.Now,
                CaseTypeId = caseModel.CaseTypeId,
                IsArrested = false,
                IsDeceased = false
            };
            await SetViewbag(caseId, caseSessionId, caseModel.CaseGroupId, model);

            ViewBag.canChange = CurrentContext.CanChange;
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на страна
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            var model = await service.CasePerson_GetById(id);
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePerson, id, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }
            await SetViewbag(model.CaseId, model.CaseSessionId, model.CaseGroupId, model);
            ViewBag.canChange = CurrentContext.CanChange;
            return View(nameof(Edit), model);
        }

        public async Task<IActionResult> View(int id)
        {
            var model = await service.CasePerson_GetById(id);
            await SetViewbag(model.CaseId, model.CaseSessionId, model.CaseGroupId, model);
            ViewBag.canChange = false;
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация на страна преди запис
        /// </summary>
        /// <param name="model"></param>
        void ValidateModelCasePerson(CasePersonVM model)
        {
            switch (model.UicTypeId)
            {
                case NomenclatureConstants.UicTypes.Bulstat:
                case NomenclatureConstants.UicTypes.EIK:
                    if (string.IsNullOrEmpty(model.FullName))
                    {
                        ModelState.AddModelError($"{nameof(CasePersonVM.FullName)}", "Въведете 'Наименование'.");
                    }
                    model.DateDeceased = null;
                    model.IsDeceased = null;
                    break;
                default:
                    if (string.IsNullOrEmpty(model.FirstName))
                    {
                        ModelState.AddModelError($"{nameof(CasePersonVM.FirstName)}", "Въведете поне едно име.");
                    }
                    break;
            }
            if (model.DateTo != null)
            {
                if (((DateTime)model.DateTo).Date < model.DateFrom.Date)
                {
                    ModelState.AddModelError("", "От дата не може да е по-голяма от До дата");
                }
            }
        }

        /// <summary>
        /// Запис на страна
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(CasePersonVM model)
        {
            await SetViewbag(model.CaseId, model.CaseSessionId, model.CaseGroupId, model);
            ViewBag.canChange = CurrentContext.CanChange;
            ValidateModelCasePerson(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = await service.CasePerson_SaveData(model);
            if (result == true)
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePerson, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                if (errorMessage == "")
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            return View(nameof(Edit), model);
        }

        private async Task SetViewbag(int caseId, int? caseSessionId, int caseGroupId, CasePersonVM model)
        {
            //ViewBag.PersonRoleId_ddl = nomService.GetDropDownList<PersonRole>(true, false, false);
            ViewBag.PersonRolesForArrested = await nomService.GetPersonRoleIdsByGroup(NomenclatureConstants.PersonRoleGroupings.RoleArrested);

            ViewBag.PersonMaturityId_ddl = await nomService.GetDropDownListAsync<PersonMaturity>();

            ViewBag.MilitaryRangId_ddl = await nomService.GetDropDownListAsync<MilitaryRang>();
            if (!NomenclatureConstants.CourtType.MillitaryCourts.Contains(userContext.CourtTypeId))
            {
                ViewBag.MilitaryRangs = null;
            }
            if (caseSessionId == null)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(caseSessionId ?? 0);

            ViewBag.isRegisterCompany = false;
            if (caseGroupId == NomenclatureConstants.CaseGroups.Company)
            {
                ViewBag.isRegisterCompany = await caseService.IsRegisterCompany(caseId);
                if (ViewBag.isRegisterCompany)
                    ViewBag.CompanyTypeId_ddl = await nomService.GetDropDownListAsync<CompanyType>();
            }

            model.RegixRequestReason.RegixReasonCaseId = caseId;
            model.RegixRequestReason.RegixRequestTypeId = NomenclatureConstants.RegixRequestTypes.FromCase;
            if (caseSessionId == null)
            {
                bool isRNFL = await caseService.GetPropByIdAsync<Case, int?>(x => x.Id == caseId, x => x.IspnKind) == NomenclatureConstants.IspnKinds.Rnfl;
                if (isRNFL)
                {
                    ViewBag.isRNFL = true;
                    ViewBag.RelatedActId_ddl = caseSessionActService.GetDropDownList_CaseSessionAct(caseId, false);
                }
            }

            SetHelpFile(HelpFileValues.CasePerson);
        }

        /// <summary>
        /// Списък на адреси към страна
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public async Task<IActionResult> CasePersonAddressList(int casePersonId)
        {
            var casePerson = await service.GetReadonlyAsync<CasePerson>(casePersonId);
            ViewBag.casePersonId = casePerson.Id;
            ViewBag.casePersonName = casePerson.FullName;
            ViewBag.caseId = casePerson.CaseId;
            ViewBag.caseSessionId = casePerson.CaseSessionId;

            if (casePerson.CaseSessionId == null)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(casePerson.CaseId);
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(casePerson.CaseSessionId ?? 0);

            return View();
        }

        /// <summary>
        /// Извличане на данни за адреси на страна
        /// </summary>
        /// <param name="request"></param>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCasePersonAddress(IDataTablesRequest request, int casePersonId)
        {
            var data = service.CasePersonAddress_Select(casePersonId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на адрес
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddCasePersonAdr(int casePersonId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonAddress, null, AuditConstants.Operations.Append, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = await service.GetReadonlyAsync<CasePerson>(casePersonId);
            await SetViewBagPersonAddress(casePersonId);

            var model = new CasePersonAddress()
            {
                CasePersonId = casePersonId,
                CaseId = casePerson.CaseId,
                CourtId = userContext.CourtId,
                Address = new Address(),
                ForNotification = false
            };
            return View(nameof(EditCasePersonAdr), model);
        }

        /// <summary>
        /// Редакция на адрес
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCasePersonAdr(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonAddress, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.CasePersonAddress_GetById(id);
            if (model == null)
            {
                return NotFoundError("Търсеният от Вас адрес не е намерен и/или нямате достъп до него.");
            }
            await SetViewBagPersonAddress(model.CasePersonId);
            return View(nameof(EditCasePersonAdr), model);
        }

        /// <summary>
        /// Валидация преди запис
        /// </summary>
        /// <param name="model"></param>
        void ValidateModel(CasePersonAddress model)
        {
            if (model.Address.CountryCode == NomenclatureConstants.CountryBG && string.IsNullOrEmpty(model.Address.CityCode))
            {
                ModelState.AddModelError("", "Въведете адрес");
            }
            if (model.Address.AddressTypeId <= 0)
            {
                ModelState.AddModelError("", "Въведете вид адрес");
            }
        }

        public async Task<IActionResult> CasePersonAdr_FromRegix(int casePersonId, int adrId, int addressTypeId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonAddress, null, AuditConstants.Operations.Append, casePersonId))
            {
                return Redirect_Denied();
            }
            var personInfo = await service.GetPropByIdAsync<CasePerson, dynamic>(casePersonId,
                    x => new
                    {
                        x.CaseId,
                        CaseNumber = x.Case.RegNumber,
                        x.CourtId,
                        x.Uic,
                        x.UicTypeId
                    });
            CasePersonAddress model = null;
            string regixReasonDescription = string.Empty;
            if (adrId > 0)
            {
                model = service.CasePersonAddress_GetById(adrId);
                regixReasonDescription = "Редактиране данни за адрес на страна";
            }
            else
            {
                model = new CasePersonAddress()
                {
                    CasePersonId = casePersonId,
                    CaseId = personInfo.CaseId,
                    CourtId = userContext.CourtId,
                    Address = new Address(),
                    ForNotification = false
                };
                regixReasonDescription = "Добавяне на адрес на страна";
            }
            string baseInfo = $"Дело {personInfo.CaseNumber}";
            switch (addressTypeId)
            {
                case NomenclatureConstants.AddressType.Permanent:
                    PermanentAddressResponseType pAdres = await regixReportService.GetPermanentAddressAndSave(personInfo.Uic, null, personInfo.CaseId, regixReasonDescription, null, NomenclatureConstants.RegixRequestTypes.FromCase);
                    model.Address = pAdres.ToEntity();
                    if (model.Address != null)
                        commonService.Address_LocationCorrection(model.Address);
                    AddAuditInfo("Преглед", baseInfo, $"Проверка в НБД за постоянен адрес на лице по ЕГН {personInfo.Uic}");
                    break;
                case NomenclatureConstants.AddressType.Current:
                    TemporaryAddressResponseType tAdres = await regixReportService.GetCurrentAddressAndSave(personInfo.Uic, null, personInfo.CaseId, regixReasonDescription, null, NomenclatureConstants.RegixRequestTypes.FromCase);
                    model.Address = tAdres.ToEntity();
                    if (model.Address != null)
                        commonService.Address_LocationCorrection(model.Address);
                    AddAuditInfo("Преглед", baseInfo, $"Проверка в НБД за настоящ адрес на лице по ЕГН {personInfo.Uic}");
                    break;
                default:
                    {
                        await SetViewBagPersonAddress(casePersonId);
                        SetErrorMessage("Справка към Regix можете да направите само за постоянен или настоящ адрес!");
                        return View(nameof(EditCasePersonAdr), model);
                    }
            }
            await SetViewBagPersonAddress(casePersonId);
            SetSuccessMessage("Данните от Regix са заредени успешно!");
            return View(nameof(EditCasePersonAdr), model);
        }

        /// <summary>
        /// запис на адрес
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditCasePersonAdr(CasePersonAddress model)
        {
            await SetViewBagPersonAddress(model.CasePersonId);
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCasePersonAdr), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = await service.CasePersonAddress_SaveData(model);
            if (result == true)
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonAddress, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.CasePersonId });
            }
            else
            {
                if (errorMessage == "")
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            return View(nameof(EditCasePersonAdr), model);
        }

        public async Task SetViewBagPersonAddress(int casePersonId)
        {
            ViewBag.CountriesDDL = await nomService.GetCountriesAsync();
            ViewBag.AddressTypesDDL = await nomService.GetDropDownListAsync<AddressType>();

            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonAddress(casePersonId);
            SetHelpFile(HelpFileValues.CasePerson);
        }

        [HttpPost]
        public async Task<IActionResult> CasePersonAdr_ExpiredInfo(ExpiredInfoVM model)
        {
            var expireModel = await service.GetByIdAsync<CasePersonAddress>(model.Id);
            var isUsed = await service.CasePersonAddress_IsUsed(expireModel);
            if (isUsed.Result)
            {
                //адреса е използван
                return Json(new { result = false, message = isUsed.ErrorMessage });
            }

            if (service.SaveExpireInfo<CasePersonAddress>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CasePersonAddress, model.Id);
                SetSuccessMessage(MessageConstant.Values.CasePersonAddressExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Edit", "CasePerson", new { id = expireModel.CasePersonId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        /// <summary>
        /// Промяна на подредбата на страни по дело/заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangeOrderCasePerson(ChangeOrderModel model)
        {
            var casePerson = await service.GetByIdAsync<CasePerson>(model.Id);
            CurrentContext_Set(await service.GetCurrentContextAsync(SourceTypeSelectVM.CasePerson, model.Id, AuditConstants.Operations.Patch));
            var personRole = await service.GetPropByIdAsync<PersonRole, string>(x => x.Id == casePerson.PersonRoleId, x => x.Label);
            if (model.Direction == "up")
            {
                CurrentContext_SetObjectInfo($"Преместване на {casePerson.FullName} ({personRole}) в списък нагоре");
            }
            else
            {
                CurrentContext_SetObjectInfo($"Преместване на {casePerson.FullName} ({personRole}) в списък надолу");
            }
            Func<CasePerson, int?> orderProp = x => x.RowNumber;
            Expression<Func<CasePerson, int?>> setterProp = (x) => x.RowNumber;
            Expression<Func<CasePerson, bool>> predicate = x => x.CaseId == casePerson.CaseId && (x.CaseSessionId ?? 0) == (casePerson.CaseSessionId ?? 0);
            bool result = service.ChangeOrder(model.Id, model.Direction == "up", orderProp, setterProp, predicate);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Проблем при смяна на реда");
            }

            return Ok();
        }

        /// <summary>
        /// запис на Промяна на подредбата на страни по дело/заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangeOrderCasePersonNotification(ChangeOrderModel model)
        {
            var casePerson = await service.GetByIdAsync<CasePerson>(model.Id);
            Func<CasePerson, int?> orderProp = x => x.NotificationNumber;
            Expression<Func<CasePerson, int?>> setterProp = (x) => x.NotificationNumber;
            Expression<Func<CasePerson, bool>> predicate = x => x.CaseId == casePerson.CaseId && (x.CaseSessionId ?? 0) == (casePerson.CaseSessionId ?? 0) && (x.ForNotification == true);
            bool result = service.ChangeOrder(model.Id, model.Direction == "up", orderProp, setterProp, predicate);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Проблем при смяна на реда");
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult GetDDL_Case_CaseSession_ForPersonCopy(int caseId, int caseSessionId)
        {
            var model = service.GetDDL_Case_CaseSession_ForPersonCopy(caseId, caseSessionId);

            return Json(model);
        }

        public async Task<IActionResult> CasePerson_SelectForCheck(int caseId, int caseSessionId, int realCaseSessionId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionPerson, null, AuditConstants.Operations.Update, realCaseSessionId))
            {
                return Redirect_Denied();
            }
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { id = realCaseSessionId });
            var data = await service.CasePerson_SelectForCheck(caseId, caseSessionId, realCaseSessionId);
            return PartialView("CheckListViewVM", data);
        }

        /// <summary>
        /// Запис на лицата копирани от предно заседание или от делото
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        //[DisableRequestSizeLimit]
        //[RequestFormLimits(ValueCountLimit = 100000, MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue, KeyLengthLimit = 100000)]
        //[RequestSizeLimit(2147483648)]
        public async Task<IActionResult> CasePerson_SelectForCheck(CheckListViewVM model)
        {
            string ids = string.Join(",", model.checkListVMs.Where(x => x.Checked).Select(x => x.Value));
            if (service.CasePerson_CopyCasePerson(ids, model.CourtId, model.ObjectId))
            {
                await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionPerson, null, AuditConstants.Operations.Update, model.ObjectId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                this.SaveLogOperation(IO.LogOperation.Models.OperationTypes.Patch, model.ObjectId);
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return RedirectToAction("Preview", "CaseSession", new { @id = model.ObjectId });
        }

        /// <summary>
        /// Избор на страни за уведомяване
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CasePersonNotification(int caseId, int caseSessionId)
        {
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { @id = caseSessionId });
            return View("CheckListViewVM", service.CasePersonNotification_SelectForCheck(caseId, caseSessionId));
        }

        /// <summary>
        /// Запис на избраните страни
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CasePersonNotification(CheckListViewVM model)
        {
            if (service.CasePerson_SaveNotification(model))
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { @id = model.ObjectId });
            return View("CheckListViewVM", model);
        }

        [HttpPost]
        public IActionResult ListDataReport(IDataTablesRequest request, string uic, string fullName, string caseRegnumber, DateTime? DateFrom, DateTime? DateTo, DateTime? FinalDateFrom, DateTime? FinalDateTo, DateTime? WithoutFinalDateTo)
        {
            var data = service.CasePerson_SelectForReport(userContext.CourtId, uic, fullName, caseRegnumber, DateFrom, DateTo, FinalDateFrom, FinalDateTo, WithoutFinalDateTo);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Опресняване на информацията на страни от заседание от страните по делото
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ReloadPersonData(int caseId, int caseSessionId)
        {
            object res = null;
            (bool result, string errorMessage) = service.ReloadPersonData(caseId, caseSessionId);

            if (result == true)
            {
                await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionPerson, null, AuditConstants.Operations.Update, caseSessionId);
                res = new { result = result, message = "Обновяването на данните премина успешно" };
            }
            else
            {
                if (errorMessage == "")
                    errorMessage = "Проблем при обновяването на данните";
                res = new { result = result, message = errorMessage };
            }

            return Json(res);
        }

        public async Task<IActionResult> CasePersonPrint_SelectForCheck(int caseId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePerson, null, AuditConstants.Operations.View, caseId))
            {
                return Redirect_Denied();
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            ViewBag.backUrl = Url.Action("CasePreview", "Case", new { id = caseId });
            var data = await service.CasePersonPrint_SelectForCheck(caseId);

            if (data.checkListVMs.Count < 1)
            {
                SetErrorMessage("Няма данни за избор.");
                return RedirectToAction("CasePreview", "Case", new { id = caseId });
            }

            return View("CheckListViewVM", data);
        }

        /// <summary>
        /// Генериране на списък на страните
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CasePersonPrint_SelectForCheck(CheckListViewVM model)
        {
            if (!model.checkListVMs.Any(x => x.Checked == true))
            {
                SetErrorMessage("Няма избрани лица.");
                return RedirectToAction("CasePreview", "Case", new { id = model.CourtId });
            }

            var caseVM = await caseService.GetCaseInfo(model.CourtId);

            Print_CaseSessionNotificationListVM print_CaseSessionNotificationList = new Print_CaseSessionNotificationListVM()
            {
                Title = "Списък на страните",
                NameReport = caseVM.CaseTypeLabel + " №:" + caseVM.RegNumber + "/" + caseVM.RegDate.ToString("yyyy"),
                SessionTitle = "",
                NotificationLists = service.PersonListForPrint_Select(model).ToList()
            };

            string html = await this.RenderPartialViewAsync("~/Views/CasePerson/", "_CasePersonBlank.cshtml", print_CaseSessionNotificationList, true);

            TinyMCEVM htmlModel = new TinyMCEVM();
            htmlModel.SourceType = SourceTypeSelectVM.CasePerson;
            htmlModel.SourceId = model.CourtId;
            htmlModel.Title = print_CaseSessionNotificationList.Title;
            htmlModel.Text = html;
            htmlModel.PageOrientation = 1;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(htmlModel.SourceId);
            return View("EditTinyMCE", htmlModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditTinyMCE(TinyMCEVM htmlModel)
        {

            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html })
            {
                CustomSwitches = "--disable-smart-shrinking --margin-top 10mm --margin-right 5mm  --margin-left 5mm"
            }.GetByte(this.ControllerContext);

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = htmlModel.SourceType,
                SourceId = htmlModel.SourceId.ToString(),
                FileName = "casePersonList.pdf",
                ContentType = "application/pdf",
                Title = htmlModel.Title,
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            bool result = await cdnService.MongoCdn_AppendUpdate(pdfRequest);

            if (result)
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return RedirectToAction("CasePreview", "Case", new { id = htmlModel.SourceId });
        }

        public async Task<IActionResult> AddLikeAnotherPerson(int personId)
        {

            var model = await service.CasePerson_GetById(personId);
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePerson, null, AuditConstants.Operations.Append, model.CaseId))
            {
                return Redirect_Denied();
            }
            model.Id = 0;
            model.DateFrom = DateTime.Now;
            //model.IsInitialPerson = false;
            model.PersonRoleId = 0;
            model.FromPersonId = personId;

            await SetViewbag(model.CaseId, model.CaseSessionId, model.CaseGroupId, model);
            ViewBag.canChange = true;
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Добавяне на институция
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddInstitution(int sourceType, long sourceId, int caseId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePerson, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            CasePersonVM model = new CasePersonVM();
            model.CaseId = caseId;
            model.CourtId = userContext.CourtId;
            model.DateFrom = DateTime.Now;
            //model.IsInitialPerson = false;
            model.Person_SourceType = sourceType;
            model.Person_SourceId = sourceId;
            if (sourceType == SourceTypeSelectVM.LawUnit)
            {
                var lawUnit = commonService.GetById<LawUnit>((int)sourceId);
                switch (lawUnit.LawUnitTypeId)
                {
                    case NomenclatureConstants.LawUnitTypes.Lawyer:
                        model.PersonRoleId = 1;
                        break;
                    case NomenclatureConstants.LawUnitTypes.Expert:
                        model.PersonRoleId = 4;
                        break;
                    case NomenclatureConstants.LawUnitTypes.Prosecutor:
                        model.PersonRoleId = 43;
                        break;
                    default:
                        break;
                }
                model.FirstName = lawUnit.FirstName;
                model.MiddleName = lawUnit.MiddleName;
                model.FamilyName = lawUnit.FamilyName;
                model.Family2Name = lawUnit.Family2Name;
            }
            var entityData = await commonService.SelectEntity_Select(sourceType, null, null, sourceId)
                                                .FirstOrDefaultAsync();
            if (entityData != null)
            {
                model.FullName = entityData.Label;
                model.UicTypeId = entityData.UicTypeId;
                model.Uic = entityData.Uic;
                if (entityData.SourceType == SourceTypeSelectVM.Instutution)
                {
                    var inst = commonService.GetById<Institution>((int)sourceId);
                    if (inst != null)
                    {
                        model.FirstName = inst.FirstName;
                        model.MiddleName = inst.MiddleName;
                        model.FamilyName = inst.FamilyName;
                        model.Family2Name = inst.Family2Name;
                        switch (inst.InstitutionTypeId)
                        {
                            case NomenclatureConstants.InstitutionTypes.Syndic:
                                model.PersonRoleId = 47;
                                break;
                        }
                    }
                }
            }

            await SetViewbag(model.CaseId, model.CaseSessionId, 0, model);
            ViewBag.canChange = true;
            return View(nameof(Edit), model);
        }

        public async Task<IActionResult> ActualizeFromBase(int id)
        {
            var casePerson = await commonService.GetByIdAsync<CasePerson>(id);
            if (casePerson == null || !(casePerson.Person_SourceType > 0) || !(casePerson.Person_SourceId > 0))
            {
                return RedirectToAction(nameof(Edit), new { id });
            }

            var actualData = await commonService.SelectEntity_Select(casePerson.Person_SourceType.Value, null, null, casePerson.Person_SourceId)
                                                .FirstOrDefaultAsync();

            if (actualData == null)
            {
                return RedirectToAction(nameof(Edit), new { id });
            }

            var model = await service.CasePerson_GetById(id);

            if (casePerson.Person_SourceType == SourceTypeSelectVM.LawUnit)
            {
                var lawUnit = await commonService.GetByIdAsync<LawUnit>((int)casePerson.Person_SourceId);
                model.Uic = lawUnit.Uic;
                model.FirstName = lawUnit.FirstName;
                model.MiddleName = lawUnit.MiddleName;
                model.FamilyName = lawUnit.FamilyName;
                model.Family2Name = lawUnit.Family2Name;
            }
            model.FullName = actualData.Label;

            (bool result, string errorMessage) = await service.CasePerson_SaveData(model);
            if (result == true)
            {
                this.SaveLogOperation(this.ControllerName, nameof(Edit), $"Актуализиране данни от основен регистър: {actualData.Label}", IO.LogOperation.Models.OperationTypes.Patch, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                if (errorMessage == "")
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }

            return RedirectToAction(nameof(Edit), new { id });
        }

        /// <summary>
        /// Анулиране на страна
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CasePerson_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePerson, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var expireObject = await service.GetByIdAsync<CasePerson>(model.Id);
            (bool result, string errorMessage) = await service.CheckCasePersonExpired(expireObject);
            if (result == false)
            {
                return Json(new { result = false, message = errorMessage });
            }
            else
            {
                if (await service.CasePerson_SaveExpiredPlus(model))
                {
                    SetAuditContextDelete(service, SourceTypeSelectVM.CasePerson, model.Id);
                    var isExistCFP = service.IsExistFastProcess(expireObject.CaseId, expireObject.Id);
                    var resMess = MessageConstant.Values.CasePersonExpireOK + (isExistCFP ? " За това лице има данни в заповедното производство!" : string.Empty);
                    SetSuccessMessage(resMess);
                    return Json(new { result = true, redirectUrl = Url.Action("CasePreview", "Case", new { id = expireObject.CaseId }) });
                }
                else
                {
                    return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
                }
            }
        }

        /// <summary>
        /// Търсене на адреси
        /// </summary>
        /// <param name="uic"></param>
        /// <param name="uicTypeId"></param>
        /// <param name="casePersonId"></param>
        /// <param name="personSourceType"></param>
        /// <param name="personSourceId"></param>
        /// <returns></returns>
        public IActionResult CasePersonAddress_Search(string uic, int uicTypeId, int casePersonId, int? personSourceType, long? personSourceId)
        {
            ViewBag.uic = uic;
            ViewBag.uicTypeId = uicTypeId;
            ViewBag.casePersonId = casePersonId;
            ViewBag.personSourceType = personSourceType;
            ViewBag.personSourceId = personSourceId;
            return PartialView("_CasePersonAddressSearch");
        }

        [HttpPost]
        public async Task<JsonResult> AddAddressFromSearch(int casePersonId, int addressId)
        {
            object res = null;
            (bool result, string errorMessage) = await service.CasePersonAddress_AddFromSearch(casePersonId, addressId);

            if (result == true)
            {
                res = new { result = result, message = MessageConstant.Values.SaveOK };
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = MessageConstant.Values.SaveFailed;
                res = new { result = result, message = errorMessage };
            }

            return Json(res);
        }

        /// <summary>
        /// Списък с наследство към страна
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public async Task<IActionResult> IndexInheritance(int casePersonId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonInheritance, null, AuditConstants.Operations.View, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = await service.GetByIdAsync<CasePerson>(casePersonId);
            ViewBag.casePersonId = casePersonId;
            ViewBag.casePersonName = casePerson.FullName;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(casePerson.CaseId);
            SetHelpFile(HelpFileValues.CasePerson);

            return View();
        }

        /// <summary>
        /// Извличане на данни за наследство към страна
        /// </summary>
        /// <param name="request"></param>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataInheritance(IDataTablesRequest request, int casePersonId)
        {
            var data = service.CasePersonInheritance_Select(casePersonId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на наследство към страна
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddInheritance(int casePersonId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonInheritance, null, AuditConstants.Operations.Append, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = await service.GetByIdAsync<CasePerson>(casePersonId);
            await SetViewbagInheritance(casePerson.CaseId, casePerson.Id);
            var model = new CasePersonInheritance()
            {
                CasePersonId = casePerson.Id,
                CaseId = casePerson.CaseId,
                CourtId = userContext.CourtId,
                DecreedCourtId = userContext.CourtId,
                IsActive = true,
            };
            return View(nameof(EditInheritance), model);
        }

        /// <summary>
        /// Редакция на наследство към страна
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditInheritance(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonInheritance, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await service.GetByIdAsync<CasePersonInheritance>(id);
            await SetViewbagInheritance(model.CaseId, model.CasePersonId);
            return View(nameof(EditInheritance), model);
        }

        /// <summary>
        /// Валидация преди запис
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidInheritance(CasePersonInheritance model)
        {
            if (model.CaseSessionActId < 1)
                return "Изберете акт";

            if (model.CasePersonInheritanceResultId < 1)
                return "Изберете резултат";

            return string.Empty;
        }

        /// <summary>
        /// запис на наследство към страна
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditInheritance(CasePersonInheritance model)
        {
            await SetViewbagInheritance(model.CaseId, model.CasePersonId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditInheritance), model);
            }

            string _isvalid = IsValidInheritance(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditInheritance), model);
            }

            var currentId = model.Id;
            if (await service.CasePersonInheritance_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonInheritance, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditInheritance), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditInheritance), model);
        }

        private async Task SetViewbagInheritance(int caseId, int casePersonId)
        {
            ViewBag.CaseSessionActId_ddl = await caseSessionActService.GetDropDownListAsync(caseId, null, true);
            ViewBag.CasePersonInheritanceResultId_ddl = await nomService.GetDropDownListAsync<CasePersonInheritanceResult>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonInheritance(casePersonId);
            SetHelpFile(HelpFileValues.CasePerson);
        }

        [HttpPost]
        public async Task<IActionResult> CasePersonInheritance_ExpiredInfo(ExpiredInfoVM model)
        {
            var expireObject = await service.GetByIdAsync<CasePersonInheritance>(model.Id);
            if (service.SaveExpireInfo<CasePersonInheritance>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CasePersonInheritance, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseLoadIndexExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("IndexInheritance", "CasePerson", new { casePersonId = expireObject.CasePersonId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        /// <summary>
        /// Списък с мерки към лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public async Task<IActionResult> IndexCasePersonMeasure(int casePersonId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonSentence, null, AuditConstants.Operations.View, casePersonId))
            {
                return Redirect_Denied();
            }

            var casePerson = await service.GetByIdAsync<CasePerson>(casePersonId);
            ViewBag.casePersonId = casePersonId;
            ViewBag.casePersonName = casePerson.FullName;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(casePerson.CaseId);
            SetHelpFile(HelpFileValues.CasePerson);

            return View();
        }

        /// <summary>
        /// Извличане на данни за мерки към лице
        /// </summary>
        /// <param name="request"></param>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCasePersonMeasure(IDataTablesRequest request, int casePersonId)
        {
            var data = service.CasePersonMeasure_Select(casePersonId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на мерки към лице
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddCasePersonMeasure(int personId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonMeasure, null, AuditConstants.Operations.Append, personId))
            {
                return Redirect_Denied();
            }
            var casePerson = await service.GetByIdAsync<CasePerson>(personId);
            await SetViewbagCasePersonMeasure(casePerson.Id);
            var model = new CasePersonMeasureEditVM()
            {
                CasePersonId = casePerson.Id,
                CaseId = casePerson.CaseId,
                CourtId = userContext.CourtId,
                MeasureStatusDate = DateTime.Now,
                MeasureType = "0",
                Punishments = await service.CasePersonSentencePunishmentMeasure_GetPunishmentChecks(0, personId)
            };
            return View(nameof(EditCasePersonMeasure), model);
        }

        /// <summary>
        /// Редакция на мерки към лице
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCasePersonMeasure(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonMeasure, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await service.CasePersonMeasure_GetById(id);
            model.Punishments = await service.CasePersonSentencePunishmentMeasure_GetPunishmentChecks(id, 0);
            await SetViewbagCasePersonMeasure(model.CasePersonId);
            return View(nameof(EditCasePersonMeasure), model);
        }

        /// <summary>
        /// Валидация преди запис на мерки към лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidCasePersonMeasure(CasePersonMeasureEditVM model)
        {
            model.MeasureCourtId = model.MeasureCourtId.EmptyToNull(0);
            model.MeasureInstitutionId = model.MeasureInstitutionId.EmptyToNull(0);

            if ((model.MeasureCourtId == null) && (model.MeasureInstitutionId == null))
                return "Изберете съд или институция, определила мярката";

            if ((model.MeasureCourtId != null) && (model.MeasureInstitutionId != null))
                return "Може да изберете съд или институция, определила мярката";

            if (model.MeasureType == "0")
                return "Изберете вид мярка";

            if (model.MeasureStatus == "0")
                return "Изберете статус";

            if (model.MeasureStatusDate.Year < 2000)
                return "Въведете дата на мярката";

            return string.Empty;
        }

        /// <summary>
        /// Запис на мерки към лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditCasePersonMeasure(CasePersonMeasureEditVM model)
        {
            await SetViewbagCasePersonMeasure(model.CasePersonId);

            if (ModelState.ContainsKey("MeasureInstitutionId"))
            {
                ModelState["MeasureInstitutionId"].Errors.Clear();
                ModelState["MeasureInstitutionId"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(EditCasePersonMeasure), model);
            }

            string _isvalid = IsValidCasePersonMeasure(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCasePersonMeasure), model);
            }

            var currentId = model.Id;
            if (await service.CasePersonMeasure_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonMeasure, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCasePersonMeasure), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCasePersonMeasure), model);
        }

        private async Task SetViewbagCasePersonMeasure(int casePersonId)
        {
            ViewBag.MeasureUnit_ddl = await eisppService.GetDDL_EISPPTblElementAsync(EISPPConstants.EisppTableCode.MeasureUnit);
            ViewBag.MeasureStatus_ddl = await eisppService.GetDDL_EISPPTblElementAsync(EISPPConstants.EisppTableCode.MeasureStatus);
            ViewBag.MeasureInstitutionTypeId_ddl = await nomService.GetDropDownListAsync<InstitutionType>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonMeasure(casePersonId);
            ViewBag.MeasureKindId_ddl = nomService.GetDDL_MeasureKind();
            SetHelpFile(HelpFileValues.CasePerson);
        }

        [HttpGet]
        public async Task<IActionResult> GetDDLMeasureType(int measureKindId)
        {
            string _tableName = measureKindId switch
            {
                1 => EISPPConstants.EisppTableCode.MeasureType,
                2 => EISPPConstants.EisppTableCode.ProbationMeasureType,
                _ => string.Empty
            };

            var model = await eisppService.GetDDL_EISPPTblElementAsync(_tableName);
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> CasePersonMeasure_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonMeasure, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            if (await eisppService.HaveEventForMeasure(model.Id))
            {
                return Json(new { result = false, message = "Има ЕИСПП събитие за тази мярка" });
            }
            if (service.SaveExpireInfo<CasePersonMeasure>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CasePersonMeasure, model.Id);
                SetSuccessMessage(MessageConstant.Values.CasePersonMeasureExpireOK);
                return Json(new { result = true, redirectUrl = model.ReturnUrl });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        /// <summary>
        /// Страница с лични документи към лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public async Task<IActionResult> IndexCasePersonDocument(int casePersonId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonDocument, null, AuditConstants.Operations.View, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = await service.GetByIdAsync<CasePerson>(casePersonId);
            ViewBag.casePersonId = casePersonId;
            ViewBag.casePersonName = casePerson.FullName;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(casePerson.CaseId);
            SetHelpFile(HelpFileValues.CasePerson);

            return View();
        }

        /// <summary>
        /// Извличане на данни за лични документи към лице
        /// </summary>
        /// <param name="request"></param>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCasePersonDocument(IDataTablesRequest request, int casePersonId)
        {
            var data = service.CasePersonDocument_Select(casePersonId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на лични документи към лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddCasePersonDocument(int casePersonId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonDocument, null, AuditConstants.Operations.Append, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = await service.GetByIdAsync<CasePerson>(casePersonId);
            await SetViewbagCasePersonDocument(casePerson.Id);
            var model = new CasePersonDocument()
            {
                CasePersonId = casePerson.Id,
                CaseId = casePerson.CaseId,
                CourtId = userContext.CourtId,
                IssuerCountryCode = NomenclatureConstants.CountryBG
            };
            return View(nameof(EditCasePersonDocument), model);
        }

        /// <summary>
        /// Редакция на лични документи към лице
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCasePersonDocument(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePersonDocument, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await service.GetByIdAsync<CasePersonDocument>(id);
            await SetViewbagCasePersonDocument(model.CasePersonId);
            return View(nameof(EditCasePersonDocument), model);
        }

        /// <summary>
        /// Валидация преди запис на лични документи към лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidCasePersonDocument(CasePersonDocument model)
        {
            if (model.IssuerCountryCode == string.Empty)
                return "Изберете държава";

            if (model.PersonalDocumentTypeId == "0")
                return "Изберете вид документ";

            if (model.DocumentNumber == string.Empty)
                return "Въведете номер документ";

            if (model.DocumentDate.Year < 1900)
                return "Въведете дата на издаване";

            return string.Empty;
        }

        /// <summary>
        /// Запис на лични документи към лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditCasePersonDocument(CasePersonDocument model)
        {
            await SetViewbagCasePersonDocument(model.CasePersonId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditCasePersonDocument), model);
            }

            string _isvalid = IsValidCasePersonDocument(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCasePersonDocument), model);
            }

            var currentId = model.Id;
            if (await service.CasePersonDocument_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonDocument, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCasePersonDocument), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCasePersonDocument), model);
        }

        private async Task SetViewbagCasePersonDocument(int casePersonId)
        {
            ViewBag.IssuerCountryCode_ddl = await nomService.GetCountriesAsync();
            ViewBag.PersonalDocumentTypeId_ddl = await eisppService.GetDDL_EISPPTblElementAsync(EISPPConstants.EisppTableCode.PersonalDocumentType);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonDocument(casePersonId);
            SetHelpFile(HelpFileValues.CasePerson);
        }

        /// <summary>
        /// Връзка с regix по егн и номер документ
        /// </summary>
        /// <param name="CasePersonId"></param>
        /// <param name="DocumentNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetPersonalIdentityV2(int CasePersonId, string DocumentNumber)
        {
            var casePerson = await service.GetByIdAsync<CasePerson>(CasePersonId);
            var documentRegixVM = await regixReportService.GetPersonalIdentity(DocumentNumber, casePerson.Uic);
            return Json(new { documentRegixVM });
        }


        /// <summary>
        /// Зареждане на панел с избор на лица от свързани данни
        /// </summary>
        /// <param name="caseId">ЕИСПП номер</param>
        /// <returns></returns>
        public async Task<IActionResult> CasePersons_SelectData(int caseId)
        {
            var model = await casePersonsData(caseId);

            if (model.Count > 0)
            {
                ViewBag.dataUrl = Url.Action(nameof(CasePersons_GetData), new { caseId });

                return PartialView("_PersonSelectData");
            }
            else
            {
                return Content("");
            }
        }

        public async Task<JsonResult> CasePersons_GetData(int caseId)
        {
            var model = await casePersonsData(caseId);
            return Json(model);
        }

        private async Task<List<DocumentSelectPersonsVM>> casePersonsData(int caseId)
        {
            var caseModel = await caseService.GetByIdAsync<Case>(caseId);
            var eisppNumber = caseModel.EISSPNumber;
            var model = new List<DocumentSelectPersonsVM>();
            //Добавяне на лица и адреси по ЕИСПП номер
            if (!string.IsNullOrEmpty(eisppNumber))
            {
                var selectFromEisppPersons = new DocumentSelectPersonsVM()
                {
                    SourceType = SourceTypeSelectVM.Integration_EISPP,
                    SourceId = eisppNumber,
                    SourceTypeName = "Лица по ЕИСПП номер: " + eisppNumber
                };
                var eisppActualData = await eisppService.GetActualData(eisppNumber);
                if (eisppActualData != null)
                {
                    foreach (var eisppPerson in eisppActualData.Persons)
                    {
                        var selectPerson = new DocumentSelectPersonItemVM();
                        selectPerson.ConvertFromEisppPerson(eisppPerson);
                        selectFromEisppPersons.Persons.Add(selectPerson);
                    }
                    model.Add(selectFromEisppPersons);
                }
            }

            //Добавяне на лица и адреси по свързани дела от движение на дело
            var caseMigrations = await migService.Select(caseId).Select(x => x.CaseId).Where(x => x != caseId).Distinct().ToListAsync();
            foreach (var item in caseMigrations)
            {
                var selectFromPriorCase = docService.Case_SelectPersons(item);
                model.Add(selectFromPriorCase);
            }

            //Добавяне на лица и адреси по свързани документи по заседание
            var sessionDocs = sesDocService.CaseSessionDocByCaseId_Select(caseId);
            foreach (var item in sessionDocs)
            {
                var selectFromPriorDocument = docService.Document_SelectPersons(item.DocumentId);
                model.Add(selectFromPriorDocument);
            }

            return model;
        }

        [HttpGet]
        public async Task<IActionResult> GetDDL_CasePersonAddress(int casePersonId)
        {
            var model = await service.GetAddressByCasePerson_DropDown(casePersonId);
            return Json(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDDL_CasePersonByActId(int actId)
        {
            if (actId == 0)
            {
                return Content("[]");
            }
            var actModel = await service.GetByIdAsync<CaseSessionAct>(actId);
            var model = await service.CasePersonFast_SelectForCasePreview(actModel.CaseId ?? 0, actModel.CaseSessionId)
                                     .Select(x => new SelectListItem
                                     {
                                         Value = x.Id.ToString(),
                                         Text = $"{x.FullName} ({x.RoleName})"
                                     })
                                     .ToListAsync();

            return Json(model);
        }

        [HttpPost]
        public IActionResult ListDataCasePersonPrevName(IDataTablesRequest request, int casePersonId)
        {
            var data = service.CasePersonPrevName_Select(casePersonId);
            return request.GetResponse(data);
        }

        public async Task<IActionResult> AddPrevName(int casePersonId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePerson, casePersonId, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var model = new CasePersonPrevName()
            {
                CasePersonId = casePersonId
            };
            await setViewBagPrevName();
            return View("EditCasePersonPrevName", model);
        }

        public async Task<IActionResult> EditPrevName(int id)
        {
            var model = await service.GetByIdAsync<CasePersonPrevName>(id);
            if (model == null)
            {
                return NotFoundError("Търсеният от Вас име на лице не е намерено и/или нямате достъп до него.");
            }
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CasePerson, model.CasePersonId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            await setViewBagPrevName();
            return View("EditCasePersonPrevName", model);
        }

        async Task setViewBagPrevName()
        {
            ViewBag.PersonPrevNamesTypeId_ddl = await nomService.GetDropDownListAsync<PersonPrevNamesType>();
        }

        [HttpPost]
        public async Task<IActionResult> EditPrevName(CasePersonPrevName model)
        {
            int currentId = model.Id;
            var result = await service.CasePersonPrevName_SaveData(model);
            if (result.Result)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.CasePersonId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            await setViewBagPrevName();
            return View("EditCasePersonPrevName", model);
        }
    }
}