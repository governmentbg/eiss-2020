// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rotativa.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.RegixReport;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Rotativa.AspNetCore.Options;

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

        public IActionResult Index()
        {
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
            var data = service.CasePerson_Select(caseId, caseSessionId, true);
            return request.GetResponse(data);
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
            var data = service.CasePerson_Select(caseId, caseSessionId).Where(x => x.ForNotification == true);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на страна
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public IActionResult Add(int caseId, int? caseSessionId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePerson, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            var caseModel = service.GetById<Case>(caseId);
            SetViewbag(caseId, caseSessionId, caseModel.CaseGroupId);
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
            ViewBag.canChange = CurrentContext.CanChange;
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на страна
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.CasePerson_GetById(id);
            if (!CheckAccess(service, SourceTypeSelectVM.CasePerson, id, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }
            SetViewbag(model.CaseId, model.CaseSessionId, model.CaseGroupId);
            ViewBag.canChange = CurrentContext.CanChange;
            return View(nameof(Edit), model);
        }
        public IActionResult View(int id)
        {
            var model = service.CasePerson_GetById(id);
            SetViewbag(model.CaseId, model.CaseSessionId, model.CaseGroupId);
            ViewBag.canChange = false;
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация на страна преди запис
        /// </summary>
        /// <param name="model"></param>
        void ValidateModelCasePerson(CasePersonVM model)
        {
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
        public IActionResult Edit(CasePersonVM model)
        {
            SetViewbag(model.CaseId, model.CaseSessionId, model.CaseGroupId);
            ViewBag.canChange = CurrentContext.CanChange;
            ValidateModelCasePerson(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = service.CasePerson_SaveData(model);
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

        void SetViewbag(int caseId, int? caseSessionId, int caseGroupId)
        {
            ViewBag.PersonRoleId_ddl = nomService.GetDropDownList<PersonRole>(true, false, false);
            ViewBag.PersonRolesForArrested = nomService.GetPersonRoleIdsByGroup(NomenclatureConstants.PersonRoleGroupings.RoleArrested);

            ViewBag.PersonMaturityId_ddl = nomService.GetDropDownList<PersonMaturity>();

            ViewBag.MilitaryRangId_ddl = nomService.GetDropDownList<MilitaryRang>();
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
                ViewBag.isRegisterCompany = caseService.IsRegisterCompany(caseId);
                if (ViewBag.isRegisterCompany)
                    ViewBag.CompanyTypeId_ddl = nomService.GetDropDownList<CompanyType>();
            }

            SetHelpFile(HelpFileValues.CasePerson);
        }

        /// <summary>
        /// Списък на адреси към страна
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public IActionResult CasePersonAddressList(int casePersonId)
        {
            var casePerson = service.GetById<CasePerson>(casePersonId);
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
        public IActionResult AddCasePersonAdr(int casePersonId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonAddress, null, AuditConstants.Operations.Append, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = service.GetById<CasePerson>(casePersonId);
            SetViewBagPersonAddress(casePersonId);

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
        public IActionResult EditCasePersonAdr(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonAddress, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.CasePersonAddress_GetById(id);
            SetViewBagPersonAddress(model.CasePersonId);
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

        /// <summary>
        /// запис на адрес
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCasePersonAdr(CasePersonAddress model)
        {
            SetViewBagPersonAddress(model.CasePersonId);
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCasePersonAdr), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = service.CasePersonAddress_SaveData(model);
            if (result == true)
            {
                SetAuditContext(service, SourceTypeSelectVM.CasePersonAddress, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCasePersonAdr), new { id = model.Id });
            }
            else
            {
                if (errorMessage == "")
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            return View(nameof(EditCasePersonAdr), model);
        }

        public void SetViewBagPersonAddress(int casePersonId)
        {
            ViewBag.CountriesDDL = nomService.GetCountries();
            ViewBag.AddressTypesDDL = nomService.GetDropDownList<AddressType>();

            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonAddress(casePersonId);
            SetHelpFile(HelpFileValues.CasePerson);
        }

        /// <summary>
        /// Промяна на подредбата на страни по дело/заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangeOrderCasePerson(ChangeOrderModel model)
        {
            var casePerson = service.GetById<CasePerson>(model.Id);
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
        public IActionResult ChangeOrderCasePersonNotification(ChangeOrderModel model)
        {
            var casePerson = service.GetById<CasePerson>(model.Id);
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

        public IActionResult CasePerson_SelectForCheck(int caseId, int caseSessionId, int realCaseSessionId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionPerson, null, AuditConstants.Operations.Update, realCaseSessionId))
            {
                return Redirect_Denied();
            }
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { id = realCaseSessionId });
            var data = service.CasePerson_SelectForCheck(caseId, caseSessionId, realCaseSessionId);
            return PartialView("CheckListViewVM", data);
        }

        /// <summary>
        /// Запис на лицата копирани от предно заседание или от делото
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CasePerson_SelectForCheck(CheckListViewVM model)
        {
            string ids = "";
            foreach (var item in model.checkListVMs)
            {
                if (item.Checked == false) continue;
                if (ids != "")
                    ids += ",";
                ids += item.Value;
            }
            if (service.CasePerson_CopyCasePerson(ids, model.CourtId, model.ObjectId))
            {
                CheckAccess(service, SourceTypeSelectVM.CaseSessionPerson, null, AuditConstants.Operations.Update, model.ObjectId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
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
        public IActionResult ListDataReport(IDataTablesRequest request, string uic, string fullName, string caseRegnumber)
        {
            var data = service.CasePerson_SelectForReport(userContext.CourtId, uic, fullName, caseRegnumber);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Опресняване на информацията на страни от заседание от страните по делото
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReloadPersonData(int caseId, int caseSessionId)
        {
            object res = null;
            (bool result, string errorMessage) = service.ReloadPersonData(caseId, caseSessionId);

            if (result == true)
            {
                CheckAccess(service, SourceTypeSelectVM.CaseSessionPerson, null, AuditConstants.Operations.Update, caseSessionId);
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

        public IActionResult CasePersonPrint_SelectForCheck(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePerson, null, AuditConstants.Operations.View, caseId))
            {
                return Redirect_Denied();
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            ViewBag.backUrl = Url.Action("CasePreview", "Case", new { id = caseId });
            var data = service.CasePersonPrint_SelectForCheck(caseId);

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

            var caseVM = caseService.Case_GetById(model.CourtId);

            Print_CaseSessionNotificationListVM print_CaseSessionNotificationList = new Print_CaseSessionNotificationListVM()
            {
                Title = "Списък на страните",
                NameReport = caseVM.CaseTypeLabel + " №:" + caseVM.RegNumber + "/" + caseVM.RegDate.ToString("yyyy"),
                SessionTitle = "",
                NotificationLists = service.PersonListForPrint_Select(model).ToList()
            };

            string html = await this.RenderPartialViewAsync("~/Views/CasePerson/", "_CasePersonBlank.cshtml", print_CaseSessionNotificationList, true);

            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html })
            {
                CustomSwitches = "--disable-smart-shrinking --margin-top 10mm --margin-right 5mm  --margin-left 5mm"
            }.GetByte(this.ControllerContext);

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CasePerson,
                SourceId = model.CourtId.ToString(),
                FileName = "casePersonList.pdf",
                ContentType = "application/pdf",
                Title = print_CaseSessionNotificationList.Title,
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            bool result = await cdnService.MongoCdn_AppendUpdate(pdfRequest);

            if (result)
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return RedirectToAction("CasePreview", "Case", new { id = model.CourtId });
        }

        public IActionResult AddLikeAnotherPerson(int personId)
        {

            var model = service.CasePerson_GetById(personId);
            if (!CheckAccess(service, SourceTypeSelectVM.CasePerson, null, AuditConstants.Operations.Append, model.CaseId))
            {
                return Redirect_Denied();
            }
            model.Id = 0;
            model.DateFrom = DateTime.Now;
            //model.IsInitialPerson = false;
            model.PersonRoleId = 0;
            model.FromPersonId = personId;

            SetViewbag(model.CaseId, model.CaseSessionId, model.CaseGroupId);
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
        public IActionResult AddInstitution(int sourceType, long sourceId, int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePerson, null, AuditConstants.Operations.Append, caseId))
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
            var entityData = commonService.SelectEntity_Select(sourceType, null, null, sourceId).FirstOrDefault();
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

            SetViewbag(model.CaseId, model.CaseSessionId, 0);
            ViewBag.canChange = true;
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Анулиране на страна
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CasePerson_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePerson, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var expireObject = service.GetById<CasePerson>(model.Id);
            (bool result, string errorMessage) = service.CheckCasePersonExpired(expireObject);
            if (result == false)
            {
                return Json(new { result = false, message = errorMessage });
            }
            else
            {
                if (service.SaveExpireInfo<CasePerson>(model))
                {
                    SetAuditContextDelete(service, SourceTypeSelectVM.CasePerson, model.Id);
                    SetSuccessMessage(MessageConstant.Values.CasePersonExpireOK);
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
        public IActionResult CasePersonAddress_Search(string uic, int uicTypeId, int casePersonId, int? personSourceType,
                        long? personSourceId)
        {
            ViewBag.uic = uic;
            ViewBag.uicTypeId = uicTypeId;
            ViewBag.casePersonId = casePersonId;
            ViewBag.personSourceType = personSourceType;
            ViewBag.personSourceId = personSourceId;
            return PartialView("_CasePersonAddressSearch");
        }

        [HttpPost]
        public JsonResult AddAddressFromSearch(int casePersonId, int addressId)
        {
            object res = null;
            (bool result, string errorMessage) = service.CasePersonAddress_AddFromSearch(casePersonId, addressId);

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
        public IActionResult IndexInheritance(int casePersonId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonInheritance, null, AuditConstants.Operations.View, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = service.GetById<CasePerson>(casePersonId);
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
        public IActionResult AddInheritance(int casePersonId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonInheritance, null, AuditConstants.Operations.Append, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = service.GetById<CasePerson>(casePersonId);
            SetViewbagInheritance(casePerson.CaseId, casePerson.Id);
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
        public IActionResult EditInheritance(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonInheritance, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CasePersonInheritance>(id);
            SetViewbagInheritance(model.CaseId, model.CasePersonId);
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
        public IActionResult EditInheritance(CasePersonInheritance model)
        {
            SetViewbagInheritance(model.CaseId, model.CasePersonId);

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
            if (service.CasePersonInheritance_SaveData(model))
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

        void SetViewbagInheritance(int caseId, int casePersonId)
        {
            ViewBag.CaseSessionActId_ddl = caseSessionActService.GetDropDownList(caseId, null, true);
            ViewBag.CasePersonInheritanceResultId_ddl = nomService.GetDropDownList<CasePersonInheritanceResult>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonInheritance(casePersonId);
            SetHelpFile(HelpFileValues.CasePerson);
        }

        /// <summary>
        /// Списък с мерки към лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public IActionResult IndexCasePersonMeasure(int casePersonId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonSentence, null, AuditConstants.Operations.View, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = service.GetById<CasePerson>(casePersonId);
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
        public IActionResult AddCasePersonMeasure(int personId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonMeasure, null, AuditConstants.Operations.Append, personId))
            {
                return Redirect_Denied();
            }
            var casePerson = service.GetById<CasePerson>(personId);
            SetViewbagCasePersonMeasure(casePerson.Id);
            var model = new CasePersonMeasureEditVM()
            {
                CasePersonId = casePerson.Id,
                CaseId = casePerson.CaseId,
                CourtId = userContext.CourtId,
                MeasureStatusDate = DateTime.Now
            };
            return View(nameof(EditCasePersonMeasure), model);
        }

        /// <summary>
        /// Редакция на мерки към лице
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditCasePersonMeasure(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonMeasure, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.CasePersonMeasure_GetById(id);
            SetViewbagCasePersonMeasure(model.CasePersonId);
            return View(nameof(EditCasePersonMeasure), model);
        }

        /// <summary>
        /// Валидация преди запис на мерки към лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidCasePersonMeasure(CasePersonMeasureEditVM model)
        {
            model.MeasureCourtId = NomenclatureExtensions.EmptyToNull(model.MeasureCourtId);
            model.MeasureInstitutionId = NomenclatureExtensions.EmptyToNull(model.MeasureInstitutionId);

            if ((model.MeasureCourtId == null) && (model.MeasureInstitutionId == null))
                return "Изберете съд или институция, определила мярката";

            if ((model.MeasureCourtId != null) && (model.MeasureInstitutionId != null))
                return "Може да изберете съд или институция, определила мярката";

            if (model.MeasureType == "0")
                return "Изберете вид мярка";

            if (model.MeasureStatus == "0")
                return "Изберете статус";

            if (model.MeasureStatusDate == null)
                return "Въведете дата на мярката";

            return string.Empty;
        }

        /// <summary>
        /// Запис на мерки към лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCasePersonMeasure(CasePersonMeasureEditVM model)
        {
            SetViewbagCasePersonMeasure(model.CasePersonId);

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
            if (service.CasePersonMeasure_SaveData(model))
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

        void SetViewbagCasePersonMeasure(int casePersonId)
        {
            ViewBag.MeasureType_ddl = eisppService.GetDDL_EISPPTblElement(EISPPConstants.EisppTableCode.MeasureType);
            ViewBag.MeasureStatus_ddl = eisppService.GetDDL_EISPPTblElement(EISPPConstants.EisppTableCode.MeasureStatus);
            ViewBag.MeasureInstitutionTypeId_ddl = nomService.GetDropDownList<InstitutionType>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCasePersonMeasure(casePersonId);
            SetHelpFile(HelpFileValues.CasePerson);
        }

        /// <summary>
        /// Страница с лични документи към лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public IActionResult IndexCasePersonDocument(int casePersonId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonDocument, null, AuditConstants.Operations.View, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = service.GetById<CasePerson>(casePersonId);
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
        public IActionResult AddCasePersonDocument(int casePersonId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonDocument, null, AuditConstants.Operations.Append, casePersonId))
            {
                return Redirect_Denied();
            }
            var casePerson = service.GetById<CasePerson>(casePersonId);
            SetViewbagCasePersonDocument(casePerson.Id);
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
        public IActionResult EditCasePersonDocument(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CasePersonDocument, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CasePersonDocument>(id);
            SetViewbagCasePersonDocument(model.CasePersonId);
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

            if (model.DocumentDate == null)
                return "Въведете дата на издаване";

            return string.Empty;
        }

        /// <summary>
        /// Запис на лични документи към лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCasePersonDocument(CasePersonDocument model)
        {
            SetViewbagCasePersonDocument(model.CasePersonId);

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
            if (service.CasePersonDocument_SaveData(model))
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

        void SetViewbagCasePersonDocument(int casePersonId)
        {
            ViewBag.IssuerCountryCode_ddl = nomService.GetCountries();
            ViewBag.PersonalDocumentTypeId_ddl = eisppService.GetDDL_EISPPTblElement(EISPPConstants.EisppTableCode.PersonalDocumentType);
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
        public JsonResult GetPersonalIdentityV2(int CasePersonId, string DocumentNumber)
        {
            var casePerson = service.GetById<CasePerson>(CasePersonId);
            var documentRegixVM = regixReportService.GetPersonalIdentity(DocumentNumber, casePerson.Uic);
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
            var caseModel = caseService.GetById<Case>(caseId);
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
            var caseMigrations = migService.Select(caseId).Select(x => x.CaseId).Where(x => x != caseId).Distinct().ToList();
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
    }
}