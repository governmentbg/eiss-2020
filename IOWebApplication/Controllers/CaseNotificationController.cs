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
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Layout;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rotativa.AspNetCore.Options;
using Rotativa.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace IOWebApplication.Controllers
{
    public class CaseNotificationController : BaseController
    {
        private readonly ICaseNotificationService service;
        private readonly INomenclatureService nomService;
        private readonly ICasePersonLinkService casePersonLinkService;
        private readonly ICasePersonService casePersonService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly IPrintDocumentService printDocumentService;
        private readonly ICaseSessionService sessionService;
        private readonly ICommonService commonService;
        private readonly ICourtLawUnitService courtLawUnitService;
        private readonly IDeliveryItemOperService serviceDeliveryOper;
        private readonly ICdnService cdnService;
        private readonly ICaseService caseService;
        private readonly IDeliveryAreaService areaService;
        private readonly IDeliveryItemService deliveryItemService;
        private readonly ICaseSessionActService caseSessionActService;
        private readonly ICaseSessionActComplainService caseSessionActComplainService;
        private readonly IVksNotificationService vksNotificationService;
        private readonly IDocumentNotificationService documentNotificationService;

        public CaseNotificationController(
            ICaseNotificationService _service,
            INomenclatureService _nomService,
            ICasePersonLinkService _casePersonLink,
            ICasePersonService _casePerson,
            IPrintDocumentService _printDocumentService,
            ICaseSessionService _sessionService,
            ICommonService _commonService,
            ICaseLawUnitService _caseLawUnitService,
            ICourtLawUnitService _courtLawUnitService,
            IDeliveryItemOperService _serviceDeliveryOper,
            ICdnService _cdnService,
            ICaseService _caseService,
            IDeliveryAreaService _areaService,
            IDeliveryItemService _deliveryItemService,
            ICaseSessionActService _caseSessionActService,
            ICaseSessionActComplainService _caseSessionActComplainService,
            IVksNotificationService _vksNotificationService,
            IDocumentNotificationService _documentNotificationService)
        {
            service = _service;
            nomService = _nomService;
            casePersonLinkService = _casePersonLink;
            casePersonService = _casePerson;
            printDocumentService = _printDocumentService;
            sessionService = _sessionService;
            commonService = _commonService;
            caseLawUnitService = _caseLawUnitService;
            courtLawUnitService = _courtLawUnitService;
            serviceDeliveryOper = _serviceDeliveryOper;
            cdnService = _cdnService;
            caseService = _caseService;
            areaService = _areaService;
            deliveryItemService = _deliveryItemService;
            caseSessionActService = _caseSessionActService;
            caseSessionActComplainService = _caseSessionActComplainService;
            vksNotificationService = _vksNotificationService;
            documentNotificationService = _documentNotificationService;
        }
        //public async Task<JsonResult> PintNotPrintedEPEP()
        //{
        //    var notifications = service.GetNotPrintedEpep();
        //    if (notifications.Count > 20)
        //        return Json(new { result = "Много са" });
        //    foreach (var notification in notifications)
        //    {
        //        _ = await makePrintAndSavePdf(notification.Id);
        //    }
        //    return Json(new { result = "Ок" });
        //}
        public async Task<IActionResult> Index(int id, int? caseSessionId, int? caseSessionActId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseNotification, null, AuditConstants.Operations.View, id))
            {
                return Redirect_Denied();
            }
            ViewBag.caseId = id;
            ViewBag.caseSessionId = caseSessionId;
            ViewBag.caseSessionActId = caseSessionActId;
            await SetViewbagCaption(id, caseSessionId, caseSessionActId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ListData(IDataTablesRequest request, int caseId, int? caseSessionId, int? caseSessionActId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseNotification, null, AuditConstants.Operations.View, caseId))
            {
                return Redirect_Denied();
            }
            var data = service.CaseNotification_Select(caseId, caseSessionId, caseSessionActId);
            return request.GetResponse(data);
        }

        private async Task<bool> CheckAccessAdd(int caseId, int? caseSessionId, int? caseSessionActId, string operation)
        {
            if (caseSessionActId != null)
                return await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActNotification, null, operation, caseSessionActId);
            else
            {
                if (caseSessionId != null)
                    return await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionNotification, null, operation, caseSessionId);
                else
                    return await CheckAccessAsync(service, SourceTypeSelectVM.CaseNotification, null, operation, caseId);
            }
        }

        private async Task<bool> CheckAccessWithId(int id, int caseId, int? caseSessionId, int? caseSessionActId, string operation)
        {
            if (caseSessionActId != null)
                return await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActNotification, id, operation);
            else
            {
                if (caseSessionId != null)
                    return await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionNotification, id, operation);
                else
                    return await CheckAccessAsync(service, SourceTypeSelectVM.CaseNotification, id, operation);
            }
        }

        public async Task<IActionResult> Add(int caseId, int? caseSessionId, int? caseSessionActId, int notificationPersonType, int notificationTypeId)
        {
            if (!await CheckAccessAdd(caseId, caseSessionId, caseSessionActId, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var model = new CaseNotification()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                CaseSessionId = caseSessionId,
                CaseSessionActId = caseSessionActId,
                NotificationDeliveryGroupId = NomenclatureConstants.NotificationDeliveryGroup.WithSummons,
                NotificationStateId = NomenclatureConstants.NotificationState.Ready,
                RegDate = DateTime.Now,
                NotificationPersonType = notificationPersonType,
                NotificationTypeId = notificationTypeId,
                IsOfficialNotification = true
            };

            await SetViewbag(model);
            await SetViewbagCaption(caseId, caseSessionId, caseSessionActId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, 0).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.SessionNotification);
            return View(nameof(Edit), model);
        }

        public async Task<IActionResult> AddWithParent(int caseId, int? caseSessionId, int? caseSessionActId, int? caseParentId)
        {
            if (!await CheckAccessAdd(caseId, caseSessionId, caseSessionActId, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var caseNotification = await service.ReadNotificationByIdAsync(caseParentId);
            var model = new CaseNotification()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                CaseSessionId = caseSessionId,
                CaseSessionActId = caseSessionActId,
                ParentId = caseParentId,
                NotificationDeliveryGroupId = NomenclatureConstants.NotificationDeliveryGroup.WithSummons,
                NotificationStateId = NomenclatureConstants.NotificationState.Ready,
                RegDate = DateTime.Now,
                NotificationPersonType = caseNotification.NotificationPersonType,
                IsOfficialNotification = true
            };

            await SetViewbag(caseNotification);
            await SetViewbagCaption(caseId, caseSessionId, caseSessionActId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, 0).DeleteOrDisableLast();
            return View(nameof(Edit), model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await service.ReadNotificationByIdAsync(id);
            if (model == null)
            {
                return NotFoundError("Търсеното от Вас уведомление не е намерено и/или нямате достъп до него.");
            }
            if (!await CheckAccessWithId(id, model.CaseId, model.CaseSessionId, model.CaseSessionActId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            if (model.DeliveryDateCC == null)
                model.DeliveryDateCC = DateTime.Now;
            await SetViewbag(model);
            await SetViewbagCaption(model.CaseId, model.CaseSessionId, model.CaseSessionActId);
            //var listNotificationTypeId = NomenclatureConstants.NotificationType.ToListType(model.NotificationTypeId);
            //ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, listNotificationTypeId).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.SessionNotification);

            return View(nameof(Edit), model);
        }

        private async Task SetViewbagCaption(int caseId, int? caseSessionId, int? caseSessionActId)
        {
            if (caseSessionId != null)
            {
                var caseSession = await sessionService.CaseSessionByIdAsync(caseSessionId ?? 0);
                ViewBag.CaseSessionName = caseSession.SessionType?.Label + " " + caseSession.DateFrom.ToString("dd.MM.yyyy");
                ViewBag.caseSessionId = caseSessionId;
            }

            var caseCase = await caseService.GetCaseInfo(caseId);
            ViewBag.CaseName = caseCase.CaseTypeCodeShortNumberRegDate;
            ViewBag.CaseId = caseCase.Id;

            if (caseSessionActId != null)
            {
                var caseAct = await service.GetReadonlyAsync<CaseSessionAct>(caseSessionActId ?? 0);
                var actType = await nomService.GetReadonlyAsync<ActType>(caseAct.ActTypeId);
                ViewBag.CaseActName = $"{actType.Label} {caseAct.RegNumber} / {caseAct.RegDate:dd.MM.yyyy}";
            }

            ViewBag.Caption = "по дело " + ViewBag.CaseName;

            if (caseSessionId != null)
                if (caseSessionId > 0)
                {
                    ViewBag.Caption = "по заседание " + ViewBag.CaseSessionName;
                }

            if (caseSessionActId != null)
                if (caseSessionActId > 0)
                {
                    ViewBag.Caption = "по " + ViewBag.CaseActName;
                }
        }

        private void AddValidateAddressError(CaseNotification model)
        {
            var errorMsg = NomenclatureConstants.NotificationAddressError.Message;
            if (model.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)
            {
                ModelState.AddModelError(nameof(CaseNotification.CasePersonAddressId), errorMsg);
            }
            else
            {
                ModelState.AddModelError(nameof(CaseNotification.LawUnitAddressId), errorMsg);
            }

        }

        private async Task<bool> IsValidAddreess(int casePersonAddressId, long lawUnitAddressId)
        {
            Address address = null;
            if (casePersonAddressId > 0)
            {
                address = casePersonService.CasePersonAddress_GetById(casePersonAddressId)?.Address;
            }
            if (lawUnitAddressId > 0)
            {
                address = commonService.GetById<Address>(lawUnitAddressId);
            }
            if (address == null)
            {
                return true;
            }
            if (!string.IsNullOrEmpty(address?.CityCode))
            {
                if (!string.IsNullOrEmpty(address?.StreetCode) && (address?.StreetNumber > 0))
                {
                    return true;
                }
                if (!string.IsNullOrEmpty(address?.StreetCode) && (address?.Block > 0))
                {
                    return true;
                }
                if (!string.IsNullOrEmpty(address?.ResidentionAreaCode) &&
                    ((address?.StreetNumber > 0) || (address?.Block > 0)))
                {
                    return true;
                }
            }
            return false;
        }
        private async Task ValidateAddress(CaseNotification model)
        {
            if (model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.WithSummons)
            {
                return;
            }
            int casePersonAddressId = 0;
            long lawUnitAddressId = 0;
            if (model.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)
            {
                if (model.CasePersonAddressId != null)
                {
                    casePersonAddressId = model.CasePersonAddressId ?? 0;
                }
            }
            else
            {
                if (model.CaseLawUnitId != null && model.LawUnitAddressId != null)
                {
                    lawUnitAddressId = model.LawUnitAddressId ?? 0;
                }
            }
            //if (!(await IsValidAddreess(casePersonAddressId, lawUnitAddressId))) {
            //    AddValidateAddressError(model);
            //}
        }

        private async Task ValidateModel(CaseNotification model)
        {

            var requireAddr = (!NomenclatureConstants.NotificationDeliveryGroup.OnMomentWithOutMail(model.NotificationDeliveryGroupId) &&
                               model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.WithGovernmentPaper);
            if (model.NotificationTypeId < 0)
                ModelState.AddModelError(nameof(CaseNotification.NotificationTypeId), "Въведете вид известие.");
            if (model.Id == 0)
            {
                var _sessionDateExpired = service.GetPropById<CaseSession, DateTime?>(x => x.Id == model.CaseSessionId, x => x.DateExpired);
                if (_sessionDateExpired != null)
                {
                    string err = "Заседанието е изтрито. Проверете данните по делото.";
                    if (model.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)
                    {
                        ModelState.AddModelError(nameof(CaseNotification.CasePersonId), err);
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(CaseNotification.CaseLawUnitId), err);
                    }
                }
            }
            if (model.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)
            {
                if (model.CasePersonId < 0)
                    ModelState.AddModelError(nameof(CaseNotification.CasePersonId), "Няма избрана страна по дело");

                if (model.CasePersonAddressId < 1 && requireAddr)
                    ModelState.AddModelError(nameof(CaseNotification.CasePersonAddressId), "Няма избран адрес");
            }
            else
            {
                if (model.CaseLawUnitId < 1)
                    ModelState.AddModelError(nameof(CaseNotification.CaseLawUnitId), "Няма избран член от съдебният състав");

                if (model.LawUnitAddressId < 1 && requireAddr)
                    ModelState.AddModelError(nameof(CaseNotification.LawUnitAddressId), "Няма избран адрес");
            }
            await ValidateAddress(model);
            if (model.NotificationStateId < 0)
                ModelState.AddModelError(nameof(CaseNotification.NotificationStateId), "Няма избран статус");
            if (model.Id == 0 && model.IsMultiLink != true && (model.CasePersonLinkId ?? 0) <= 0)
            {
                if (casePersonService.IsPersonDead(model.CasePersonId ?? 0))
                {
                    ModelState.AddModelError(nameof(CaseNotification.CasePersonId), "Лицето е починало, не може да бъде уведомено!");
                }
            }
            if (@NomenclatureConstants.NotificationDeliveryGroup.WithCourierLike(model.NotificationDeliveryGroupId))
            {
                if (NomenclatureConstants.NotificationState.NotificationEndState().Contains(model.NotificationStateId))
                {
                    if (model.DeliveryDateCC < model.RegDate.Date)
                    {
                        ModelState.AddModelError(nameof(model.DeliveryDateCC), $"{MessageConstant.ValidationErrors.DeliveryDateBeforeRegDate} {model.RegDate.ToString(FormattingConstant.NormalDateFormat)}");
                    }
                    if (model.DeliveryDateCC > DateTime.Now.AddMinutes(10))
                    {
                        ModelState.AddModelError(nameof(model.DeliveryDateCC), MessageConstant.ValidationErrors.DeliveryDateFuture);
                    }
                }
            }
            if (NomenclatureConstants.NotificationDeliveryGroup.OnMoment(model.NotificationDeliveryGroupId) &&
                model.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.OnSession &&
                model.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.OnMember56)
            {
                var regDate = model.RegDate.Date;
                if (model.Id > 0)
                {
                    var notification = service.GetReadonly<CaseNotification>(model.Id);
                    if (notification != null)
                        regDate = notification.RegDate.Date;
                }
                if (regDate < (new DateTime(2019, 1, 1)))
                    regDate = DateTime.Now.Date;
                if (model.DeliveryDate < regDate)
                {
                    ModelState.AddModelError(nameof(model.DeliveryDate), $"{MessageConstant.ValidationErrors.DeliveryDateBeforeRegDate} {regDate.ToString(FormattingConstant.NormalDateFormat)}");
                }
                if (model.DeliveryDate > DateTime.Now.AddMinutes(10))
                {
                    ModelState.AddModelError(nameof(model.DeliveryDate), MessageConstant.ValidationErrors.DeliveryDateFuture);
                }
            }
            if (model.HtmlTemplateId > 0)
            {
                var htmlTemplate = service.GetReadonly<HtmlTemplate>(model.HtmlTemplateId.Value);
                if (htmlTemplate?.HaveMultiActComplain == true && htmlTemplate?.RequiredSessionActComplain == true)
                {
                    var complainIds = Array.Empty<int>();
                    if (!string.IsNullOrEmpty(model.MultiComplainIdResultVM))
                    {
                        complainIds = model.MultiComplainIdResultVM.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                        complainIds = complainIds.Where(x => x > 0).ToArray();
                    }
                    if (complainIds.Length <= 0)
                    {
                        ModelState.AddModelError(nameof(model.MultiComplainIdVM), "Изберете поне една жалба");
                    }
                }
                if (htmlTemplate.HaveSessionMultiAct == true)
                {
                    model.CaseSessionActId = null;
                    ModelState.Remove(nameof(model.CaseSessionActId));
                }
            }
            //Добавена проверка призовките през ЕПЕП да могат да се записват само когато има валиден потребител за лицето
            //К.Борисов, 08.09.2021
            if (model.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
            {
                var epepInfo = casePersonLinkService.GetEpepSummonInfo(model);
                if (epepInfo == null || !epepInfo.CanSummonByEpep)
                {
                    ModelState.AddModelError(nameof(model.CasePersonId), "За избраното лице няма разрешен достъп през ЕПЕП");
                }

                model.EpepCasePersonId = epepInfo.CasePersonId;
            }
            else
            {
                model.EpepCasePersonId = null;
            }

        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Edit(CaseNotification model, [AllowHtml] string casePersonLinksJson)
        {
            var logVM = new DeliveryLogVM()
            {
                Action = model.Id > 0 ? "Редакция" : "Добавяне",
                PageLabel = "Призовки/съобщения",
                PageUrl = "CaseNotification/Edit"
            };

            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            model.CaseNotificationMLinks = JsonConvert.DeserializeObject<List<CaseNotificationMLink>>(casePersonLinksJson, dateTimeConverter);

            SetHelpFile(HelpFileValues.SessionNotification);

            if (@NomenclatureConstants.NotificationDeliveryGroup.OnMoment(model.NotificationDeliveryGroupId))
            {
                if (model.DatePrint == null &&
                    model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnEMail &&
                    model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.ByRNFL)
                {
                    model.DatePrint = DateTime.Now;
                }
                if (model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnPhone &&
                    model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnEMail &&
                    model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.ByRNFL)
                {
                    if (!ModelState.IsValid)
                    {
                        ModelState.Remove(nameof(model.LawUnitAddressId));
                        ModelState.Remove(nameof(model.CasePersonAddressId));
                    }
                    model.LawUnitAddressId = null;
                    model.CasePersonAddressId = null;
                }
                if (model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnEMail &&
                    model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.ByRNFL)
                {
                    if (!ModelState.IsValid)
                    {
                        ModelState.Remove(nameof(model.HtmlTemplateId));
                    }
                    model.HtmlTemplateId = null;
                }
            }
            else
            {
                //Ако начина на доставка не е от видовете Доставка Веднага се премахва DeliveryDate
                model.DeliveryDate = null;
            }

            if ((@NomenclatureConstants.NotificationDeliveryGroup.WithCourierLike(model.NotificationDeliveryGroupId)) &&
                 model.NotificationStateId != @NomenclatureConstants.NotificationState.Ready)
            {
                model.DeliveryDate = model.DeliveryDateCC;
                model.DeliveryInfo = model.DeliveryInfoCC;
            }

            await ValidateModel(model);
            if (!ModelState.IsValid)
            {
                await SetViewbag(model);
                return View(nameof(Edit), model);
            }
            if (((model.ToCourtId ?? 0) <= 0 && model.NotificationDeliveryGroupId == @NomenclatureConstants.NotificationDeliveryGroup.WithSummons))
            {
                await SetViewbag(model);
                ViewData["RajonAlert"] = true;
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (await service.CaseNotification_SaveData(model, logVM))
            {
                if (currentId == 0)
                    await CheckAccessAdd(model.CaseId, model.CaseSessionId, model.CaseSessionActId, AuditConstants.Operations.Append);
                else
                    await CheckAccessWithId(model.Id, model.CaseId, model.CaseSessionId, model.CaseSessionActId, AuditConstants.Operations.Update);

                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            await SetViewbag(model);
            return View(nameof(Edit), model);
        }

        public IActionResult AutoSetForDelivery(int Id)
        {
            var caseNotification = service.ReadWithMlinkById(Id);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(caseNotification, 0).DeleteOrDisableLast();
            return RedirectToAction(nameof(Edit), new { id = Id });
        }

        /// <summary>
        /// CreatePdf 
        /// </summary>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public async Task<IActionResult> PrintNotificationByTemplate(int sourceId)
        {
            (TinyMCEVM htmlModel, _) = await printDocumentService.FillHtmlTemplateNotification(sourceId);
            var pdfBytes = await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlModel)
            {
                PageOrientation = (Orientation)htmlModel.PageOrientation,
                PageMargins = new Margins(10, 5, 10, 5),
                PageSize = Size.A4,
                CustomSwitches = htmlModel.SmartShrinkingPDF ? "" : "--disable-smart-shrinking"
            }.GetByte(this.ControllerContext);

            return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "Template" + sourceId.ToString() + ".pdf");
        }

        public async Task<IActionResult> EditTinyMCE(int sourceType, int sourceId)
        {
            var htmlModel = new TinyMCEVM();
            if (sourceType == SourceTypeSelectVM.CaseNotificationPrint)
            {
                if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseNotification, sourceId, AuditConstants.Operations.Update))
                {
                    return Redirect_Denied();
                }
                (htmlModel, _) = await printDocumentService.FillHtmlTemplateNotification(sourceId);
                htmlModel.SourceId = sourceId;
                htmlModel.SourceType = SourceTypeSelectVM.CaseNotificationPrint;
                var caseNotification = service.ReadWithMlinkById(sourceId);
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEditTinyMCE(caseNotification).DeleteOrDisableLast();
            }
            else
            {
                if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionNotificationListPersonLawUnit, null, AuditConstants.Operations.GeneratingFile, sourceId))
                {
                    return Redirect_Denied();
                }
                var session = await sessionService.CaseSessionVMById(sourceId);
                var caseCase = await caseService.Case_GetByIdForNotification(session.CaseId);
                var typeList = "призованите";
                switch (sourceType)
                {
                    case SourceTypeSelectVM.CaseSessionNotificationListMessage:
                        typeList = "известените";
                        break;
                    case SourceTypeSelectVM.CaseSessionNotificationListNotification:
                        typeList = "уведомените";
                        break;
                }
                (var notificationLists, _) = await service.CaseSessionNotificationList_Select(sourceId, sourceType, 0, -1, null, string.Empty);
                Print_CaseSessionNotificationListVM print_CaseSessionNotificationList = new Print_CaseSessionNotificationListVM()
                {
                    Title = "Списък на " + typeList + " лица по " + caseCase.CaseTypeCode + " " + caseCase.RegNumber + "/" + caseCase.RegDate.ToString("dd.MM.yyyy") + (!string.IsNullOrEmpty(caseCase.DepartmentOtdelenieText) ? " - " + caseCase.DepartmentOtdelenieText : string.Empty),
                    NameReport = "",
                    SessionTitle = session.SessionTypeLabel + " от " + session.DateFrom.ToString("dd.MM.yyyy HH:mm"),
                    NotificationLists = notificationLists
                };

                string html = await this.RenderPartialViewAsync("~/Views/CaseNotification/", "_NotificationListBlank.cshtml", print_CaseSessionNotificationList, true);
                htmlModel.SourceType = sourceType;
                htmlModel.SourceId = sourceId;
                htmlModel.Title = print_CaseSessionNotificationList.Title;
                htmlModel.Text = html;
                htmlModel.PageOrientation = 1;
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationListPrint(sourceId, sourceType).DeleteOrDisableLast();
            }
            SetHelpFile(HelpFileValues.SessionNotification);

            return View("EditTinyMCE", htmlModel);
        }

        private void SetDraftNotification(TinyMCEVM htmlModel)
        {
            htmlModel.Style += @"#background {
               position: absolute;
               display: block;
               min-width: 100%;
               opacity: 0.5;
               text-align: center;
               background-color: transparent;
               padding-top:40%;
           }
           #bg-text {
               color: lightgrey;
               font-size: 120px;
               transform: rotate(300deg);
               -webkit-transform: rotate(300deg);
               opacity: 0.9;
               filter: alpha(opacity=50);
               background-color: transparent;
           }";
            htmlModel.Text = "<div id=\"background\"> <p id = \"bg-text\">Draft Draft Draft</p> </div>" + htmlModel.Text;
        }


        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> DraftTinyMCE(TinyMCEVM htmlModel)
        {
            if (string.IsNullOrEmpty(htmlModel.Style))
                htmlModel.Style = FormattingConstant.TinyMceTableDefStyle + FormattingConstant.PrintTableDefStyle;

            SetDraftNotification(htmlModel);

            var pdfBytes = await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlModel)
            {
                PageOrientation = (Orientation)htmlModel.PageOrientation,
                PageMargins = new Margins(10, 5, 10, 5),
                PageSize = Size.A4,
                CustomSwitches = htmlModel.SmartShrinkingPDF ? "" : "--disable-smart-shrinking"
            }.GetByte(this.ControllerContext);
            if (htmlModel.Id > 0)
            {
                pdfBytes = await ZoomIfHave3Pages(htmlModel, pdfBytes);
                if ((Orientation)htmlModel.PageOrientation == Orientation.Landscape)
                    pdfBytes = RotateSecondPage180(pdfBytes);
            }
            return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "draft.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> EditTinyMCE(TinyMCEVM htmlModel)
        {
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            var logVM = new DeliveryLogVM()
            {
                Action = "Генериране",
                PageLabel = "Печат на призовка/съобщение",
                PageUrl = "EditTinyMCE"
            };
            var htmlRequest = new CdnUploadRequest()
            {
                SourceType = htmlModel.SourceType,
                SourceId = htmlModel.SourceId.ToString(),
                FileName = htmlModel.SourceType == SourceTypeSelectVM.CaseNotificationPrint ? "draft.html" : "caseSessionNotificationList.pdf",
                ContentType = htmlModel.SourceType == SourceTypeSelectVM.CaseNotificationPrint ? NomenclatureConstants.ContentTypes.Html : NomenclatureConstants.ContentTypes.Pdf,
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html ?? ""))
            };
            if (htmlModel.SourceType != SourceTypeSelectVM.CaseNotificationPrint)
            {
                var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html })
                {
                    CustomSwitches = "--disable-smart-shrinking --margin-top 10mm --margin-right 5mm  --margin-left 5mm"
                }.GetByte(this.ControllerContext);
                htmlRequest.FileContentBase64 = Convert.ToBase64String(pdfBytes);
            }
            if (await cdnService.MongoCdn_AppendUpdate(htmlRequest))
            {
                if (htmlModel.SourceType == SourceTypeSelectVM.CaseNotificationPrint)
                {
                    var caseNotification = service.ReadWithMlinkById(htmlModel.SourceId);
                    caseNotification.DatePrint = DateTime.Now;
                    caseNotification.DeliveryOperId = NomenclatureConstants.NotificationState.Ready;
                    caseNotification.SkipSaveLists = true;
                    await service.CaseNotification_SaveData(caseNotification, logVM);
                    if (!NomenclatureConstants.NotificationDeliveryGroup.OnMoment(caseNotification.NotificationDeliveryGroupId) ||
                        caseNotification.NotificationDeliveryGroupId == @NomenclatureConstants.NotificationDeliveryGroup.OnEMail ||
                        caseNotification.NotificationDeliveryGroupId == @NomenclatureConstants.NotificationDeliveryGroup.ByRNFL)
                    {
                        _ = await makePrintAndSavePdf(caseNotification.Id, false);
                    }
                }
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            if (htmlModel.SourceType == SourceTypeSelectVM.CaseNotificationPrint)
            {
                return RedirectToAction(nameof(Edit), new { id = htmlModel.SourceId });
            }
            else
            {
                return RedirectToAction("Preview", "CaseSession", new { id = htmlModel.SourceId, notifListTypeId = htmlModel.SourceType });
            }
        }
        #region test 
        public async Task<IActionResult> PreviewRaw(int id, int htmlTemplateId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseNotification, id, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }
            TinyMCEVM htmlModel = await printDocumentService.FillHtmlTemplateNotificationTestOne(id, htmlTemplateId);
            // return View("PreviewRaw", htmlModel);
            var pdfBytes = await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlModel)
            {
                PageOrientation = (Orientation)htmlModel.PageOrientation,
                PageMargins = new Margins(10, 5, 10, 5),
                PageSize = Size.A4,
                CustomSwitches = htmlModel.SmartShrinkingPDF ? "" : "--disable-smart-shrinking"
            }.GetByte(this.ControllerContext);
            return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "Template" + id.ToString() + ".pdf");
        }

        public async Task<IActionResult> PreviewRawDoc(int id)
        {
            TinyMCEVM htmlModel = await printDocumentService.FillHtmlTemplateDocumentTemplate(id);
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            return View("PreviewRaw", htmlModel);
        }
        public IActionResult F_FIRST_SET_NO_YEAR()
        {
            printDocumentService.HtmlTemplateNotificationHave_F_FIRST_SET_NO_YEAR();
            return View("PreviewRaw", null);
        }
        public async Task<IActionResult> HaveSaveTest(int id)
        {
            await printDocumentService.FillHtmlTemplateNotificationHaveSaveTest(id);
            return View("PreviewRaw", null);
        }
        public IActionResult FillHtmlTemplate_F_DISPOSITIV()
        {
            printDocumentService.FillHtmlTemplate_F_DISPOSITIV();
            return View("PreviewRaw", null);
        }
        #endregion test 
        private byte[] SetDuplexFlipLongEdge(byte[] pdfBytes)
        {
            MemoryStream memoryStreamNew = new MemoryStream();
            using (var stream = new MemoryStream(pdfBytes))
            {
                using (var pdf = new PdfDocument(new PdfReader(stream), new PdfWriter(memoryStreamNew)))
                {
                    PdfViewerPreferences viewerPreferences = pdf.GetCatalog().GetViewerPreferences();
                    if (viewerPreferences == null)
                    {
                        viewerPreferences = new PdfViewerPreferences();
                        viewerPreferences.SetDuplex(PdfViewerPreferences.PdfViewerPreferencesConstants.SIMPLEX);
                        pdf.GetCatalog().SetViewerPreferences(viewerPreferences);
                    }
                    // Setting printing mode on the both sides of the pdf document (duplex mode) along with "flip on long edge" mode
                    // viewerPreferences.SetDuplex(PdfViewerPreferences.PdfViewerPreferencesConstants.DUPLEX_FLIP_LONG_EDGE);
                    // viewerPreferences.SetDuplex(PdfViewerPreferences.PdfViewerPreferencesConstants.DUPLEX_FLIP_SHORT_EDGE);
                    viewerPreferences.SetDuplex(PdfViewerPreferences.PdfViewerPreferencesConstants.SIMPLEX);
                    pdf.Close();
                    return memoryStreamNew.ToArray();
                }
            }
        }
        private byte[] RotateSecondPage180(byte[] pdfBytes)
        {
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.PrintLandscapeRotate180))
                return pdfBytes;
            MemoryStream memoryStreamNew = new MemoryStream();
            if (pdfBytes != null)
            {
                using (MemoryStream memoryStream = new MemoryStream(pdfBytes))
                {
                    using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(memoryStream), new PdfWriter(memoryStreamNew)))
                    {
                        var numberOfPages = pdfDocument.GetNumberOfPages();
                        for (int i = 1; i <= numberOfPages; i++)
                        {
                            if (i % 2 == 0)
                            {
                                PdfPage page = pdfDocument.GetPage(i);
                                page.SetRotation(180);
                            }
                        }
                        pdfDocument.Close();
                    }
                }
            }
            return memoryStreamNew.ToArray();
        }
        private async Task<(byte[] pdfBytes, string FileName)> makePrintAndSavePdf(int id, bool epepIsDraft)
        {
            var cdnResult = await service.ReadPrintedFile(id);
            (TinyMCEVM htmlModel, var notification) = await printDocumentService.FillHtmlTemplateNotification(id);
            byte[] pdfBytesC;
            if (cdnResult == null)
            {
                if (htmlModel == null)
                {
                    return (null, string.Empty);
                }
                var cdnResultDraft = await service.ReadDraftFile(id);
                if (cdnResultDraft != null)
                {
                    htmlModel.Text = Encoding.UTF8.GetString(Convert.FromBase64String(cdnResultDraft.FileContentBase64));
                }
                if (epepIsDraft && notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
                {
                    SetDraftNotification(htmlModel);
                }
                var pdfBytes = await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlModel)
                {
                    PageOrientation = (Orientation)htmlModel.PageOrientation,
                    PageMargins = new Margins(10, 5, 10, 5),
                    PageSize = Size.A4,
                    CustomSwitches = htmlModel.SmartShrinkingPDF ? "" : "--disable-smart-shrinking"
                }.GetByte(this.ControllerContext);
                pdfBytes = await ZoomIfHave3Pages(htmlModel, pdfBytes);
                pdfBytesC = pdfBytes;
                if (!epepIsDraft || notification.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
                {
                    //КБорисов - изчистване на тракера преди запис на файла - траква погрешно потребител и се опитва да прави keyviolation
                    if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.FearProtectsVineyard))
                    {
                        try
                        {
                            service.ClearEntityTracker();
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    await service.SavePrintedFile(id, pdfBytes);
                }
                if (notification.DatePrint == null && notification.CourierTrackNum == "M" &&
                    notification.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.ByEPEP
                    )
                {


                    await service.SaveDatePrintMulti(notification.Id);
                }
            }
            else
            {
                pdfBytesC = Convert.FromBase64String(cdnResult.FileContentBase64);
            }
            if ((Orientation)htmlModel.PageOrientation == Orientation.Landscape)
            {
                pdfBytesC = RotateSecondPage180(pdfBytesC);
            }
            return (pdfBytesC, cdnResult?.FileName ?? service.GetFileNameNotification(notification));
        }

        private async Task<byte[]> ZoomIfHave3Pages(TinyMCEVM htmlModel, byte[] pdfBytes)
        {
            var hmlTemplateDefaultZoom = await service.GetPropByIdAsync<HtmlTemplate, decimal?>(htmlModel.Id, x => x.DefaultZoom);
            decimal defaultZoom = hmlTemplateDefaultZoom ?? 1;
            decimal zoom = defaultZoom;
            for (int i = 0; i <= 6; i++)
            {
                if (defaultZoom == 1m)
                {
                    using (MemoryStream memoryStream = new MemoryStream(pdfBytes))
                    {
                        using (PdfDocument newDoc = new PdfDocument(new PdfReader(memoryStream)))
                        {
                            if (newDoc.GetNumberOfPages() <= 2)
                                break;
                        }
                    }
                    zoom -= 0.05m;
                }
                string zoomStr = $" --zoom {zoom}".Replace(",", ".");
                pdfBytes = await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlModel)
                {
                    PageOrientation = (Orientation)htmlModel.PageOrientation,
                    PageMargins = new Margins(10, 5, 10, 5),
                    PageSize = Size.A4,
                    CustomSwitches = (htmlModel.SmartShrinkingPDF ? "" : "--disable-smart-shrinking") + zoomStr
                }.GetByte(this.ControllerContext);
                if (defaultZoom != 1m)
                    break;
            }
            return pdfBytes;
        }

        public async Task<IActionResult> PrintPdf(int id)
        {
            var caseNotification = await service.GetReadonlyAsync<CaseNotification>(id);
            await CheckAccessWithId(caseNotification.Id, caseNotification.CaseId, caseNotification.CaseSessionId, caseNotification.CaseSessionActId, AuditConstants.Operations.Print);
            AddAuditInfo(AuditConstants.Operations.Print,
                      CurrentContext?.Info?.BaseObject,
                      CurrentContext?.Info?.ObjectInfo,
                      SourceTypeSelectVM.CaseNotification,
                      true);
            //(var pdfBytesR, var FileName) = await makePrintAndSavePdf(id, true);
            //if (pdfBytesR == null)
            //{
            //    pdfBytesR = await NoDataPrintPdf();
            //    FileName = $"{caseNotification.RegNumber}.pdf";
            //}
            //return File(pdfBytesR, System.Net.Mime.MediaTypeNames.Application.Pdf, FileName);
            List<NotificationPrintIdVM> notificationIds = new() { new NotificationPrintIdVM { CaseNotificationId = id } };
            return await PrintPdfs(notificationIds);
        }

        [HttpPost]
        public async Task<IActionResult> ListDataNotificationList(IDataTablesRequest request, int caseSessionId, int NotificationListTypeId)
        {
            (var data, var totalCount) = await service.CaseSessionNotificationList_Select(caseSessionId, NotificationListTypeId, request.Start, request.Length, request.GetSortedColumnsForOrderBy(), request.Search.Value);
            return request.GetResponseServerPaging(data, totalCount);
        }

        [HttpPost]
        public IActionResult ChangeOrderNotificationList(ChangeOrderModel model)
        {
            var caseSessionNotificationList = service.GetReadonly<CaseSessionNotificationList>(model.Id);
            if (caseSessionNotificationList == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Проблем не намира реда за смяна");
            }
            if (caseSessionNotificationList != null)
            {
                var caseSessionCaseId = service.GetPropById<CaseSession, int>(caseSessionNotificationList.CaseSessionId, x => x.CaseId);
                var caseNotifications = service.CaseNotification_Select(caseSessionCaseId, caseSessionNotificationList.CaseSessionId, null).ToList();
                if (caseNotifications.Any(x => (caseSessionNotificationList.NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList ||
                                                caseSessionNotificationList.NotificationListTypeId == null) ?
                                               (x.NotificationTypeId == NomenclatureConstants.NotificationType.Subpoena || x.NotificationTypeId == null) :
                                               (caseSessionNotificationList.NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationListMessage ?
                                               x.NotificationTypeId == NomenclatureConstants.NotificationType.Message :
                                               x.NotificationTypeId == NomenclatureConstants.NotificationType.Notification)))
                {
                    return Ok();
                }
            }

            Func<CaseSessionNotificationList, int?> orderProp = x => x.RowNumber;
            Expression<Func<CaseSessionNotificationList, int?>> setterProp = (x) => x.RowNumber;
            Expression<Func<CaseSessionNotificationList, bool>> predicate = x => (x.CaseSessionId == caseSessionNotificationList.CaseSessionId) &&
                                                                                 (x.NotificationListTypeId == caseSessionNotificationList.NotificationListTypeId);
            if (caseSessionNotificationList.NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList)
            {
                predicate = x => (x.CaseSessionId == caseSessionNotificationList.CaseSessionId) &&
                                 (x.NotificationListTypeId == caseSessionNotificationList.NotificationListTypeId ||
                                  x.NotificationListTypeId == null);
            }

            bool result = service.ChangeOrder(model.Id, model.Direction == "up", orderProp, setterProp, predicate);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Проблем при смяна на реда");
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> CasePersonNotificationList(int caseId, int caseSessionId, int NotificationListTypeId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionNotificationListPerson, null, AuditConstants.Operations.ChoiceByList, caseSessionId))
            {
                return Redirect_Denied();
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(caseSessionId);
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { @id = caseSessionId, notifListTypeId = NotificationListTypeId });
            SetHelpFile(HelpFileValues.SessionPersonNotification);
            var model = await service.Person_SelectForCheck(caseId, caseSessionId, NotificationListTypeId, true);
            return View("CheckNotificationListViewVM", model);
        }

        [HttpPost]
        public async Task<IActionResult> CasePersonNotificationList(CheckListViewVM model)
        {
            if (service.CaseNotificationList_Save(model, true))
            {
                await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionNotificationListPerson, null, AuditConstants.Operations.ChoiceByList, model.ObjectId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction("Preview", "CaseSession", new { @id = model.ObjectId, notifListTypeId = model.OtherId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(model.ObjectId);
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { @id = model.ObjectId, notifListTypeId = model.OtherId });
            SetHelpFile(HelpFileValues.SessionPersonNotification);
            return View("CheckNotificationListViewVM", model);
        }

        [HttpGet]
        public async Task<IActionResult> CaseLawUnitNotificationList(int caseId, int caseSessionId, int notificationListTypeId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionNotificationListLawUnit, null, AuditConstants.Operations.ChoiceByList, caseSessionId))
            {
                return Redirect_Denied();
            }
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { @id = caseSessionId, notifListTypeId = notificationListTypeId });
            SetHelpFile(HelpFileValues.SessionPersonNotification);
            return View("CheckNotificationListViewVM", await service.Person_SelectForCheck(caseId, caseSessionId, notificationListTypeId, false));
        }

        [HttpPost]
        public async Task<IActionResult> CaseLawUnitNotificationList(CheckListViewVM model)
        {
            if (service.CaseNotificationList_Save(model, false))
            {
                await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionNotificationListLawUnit, null, AuditConstants.Operations.ChoiceByList, model.ObjectId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction("Preview", "CaseSession", new { @id = model.ObjectId, notifListTypeId = model.OtherId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { @id = model.ObjectId, notifListTypeId = model.OtherId });
            SetHelpFile(HelpFileValues.SessionPersonNotification);
            return View("CheckNotificationListViewVM", model);
        }

        [HttpPost]
        [HttpGet]
        public JsonResult LoadDataLawUnitAndArea(int toCourtId)
        {
            var lawUnitDdl = courtLawUnitService.LawUnitForCourt_Select2Data(NomenclatureConstants.LawUnitTypes.MessageDeliverer, toCourtId);
            var deliveryAreaDdl = areaService.DeliveryAreaDdlSelect2(toCourtId);
            return Json(new { lawUnitDdl, deliveryAreaDdl });
        }
        [HttpPost]
        public JsonResult LoadDataLawUnitAndAreaNew(int toCourtId, int lawUnitType)
        {
            var lawUnitDdl = areaService.RemoveSelectAddNoChangeSelect2(
                courtLawUnitService.LawUnitForCourt_Select2Data(lawUnitType, toCourtId),
                "Без избран призовкар"
            );
            var deliveryAreaDdl = areaService.RemoveSelectAddNoChangeSelect2(
                areaService.DeliveryAreaDdlSelect2(toCourtId),
                "Без избран район"
            );
            return Json(new { lawUnitDdl, deliveryAreaDdl });
        }

        [HttpPost]
        public JsonResult DeliveryOper(int notificationStateId)
        {
            return Json(serviceDeliveryOper.DeliveryOperForNotificationStateSelect(notificationStateId));
        }

        [DisableAudit]
        public IActionResult GetFileList(int sourceType, int sourceID)
        {
            var model = cdnService.Select(sourceType, sourceID.ToString()).SetCanDelete(false).ToList();
            return Json(model);
        }

        public async Task<IActionResult> EditNotificationList(int id)
        {
            var model = service.GetById<CaseSessionNotificationList>(id);
            if (model == null)
            {
                return NotFoundError("Търсеното от Вас уведомление не е намерено и/или нямате достъп до него.");
            }
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionNotificationList, model.Id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            ViewBag.NotificationAddressId_ddl = (model.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)
                                                 ? await casePersonService.GetDDL_AddressByCasePersonAddressAsync(model.CasePersonId ?? 0)
                                                 : await commonService.LawUnitAddress_SelectDDL_ByCaseLawUnitIdAsync(model.CaseLawUnitId ?? 0);
            var caseSession = await service.GetByIdAsync<CaseSession>(model.CaseSessionId);

            await SetViewbagCaption(caseSession.CaseId, caseSession.Id, null);
            SetHelpFile(HelpFileValues.SessionPersonNotification);

            return View(nameof(EditNotificationList), model);
        }

        [HttpPost]
        public async Task<IActionResult> EditNotificationList(CaseSessionNotificationList model)
        {
            var caseSession = await service.GetByIdAsync<CaseSession>(model.CaseSessionId);
            await SetViewbagCaption(caseSession.CaseId, caseSession.Id, null);
            SetHelpFile(HelpFileValues.SessionPersonNotification);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditNotificationList), model);
            }

            var currentId = model.Id;
            if (service.CaseNotificationList_SaveData(model))
            {
                await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionNotificationList, model.Id, AuditConstants.Operations.Update);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditNotificationList), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            ViewBag.NotificationAddressId_ddl = (model.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)
                                             ? await casePersonService.GetDDL_AddressByCasePersonAddressAsync(model.CasePersonId ?? 0)
                                             : await commonService.LawUnitAddress_SelectDDL_ByCaseLawUnitIdAsync(model.CaseLawUnitId ?? 0);
            return View(nameof(EditNotificationList), model);
        }

        public async Task<IActionResult> AddNotificationFromNotificationList(int caseNotificationListId)
        {
            var sessionNotificationList = await service.GetByIdAsync<CaseSessionNotificationList>(caseNotificationListId);
            var caseSession = await service.GetByIdAsync<CaseSession>(sessionNotificationList.CaseSessionId);

            var model = new CaseNotification()
            {
                CaseId = caseSession.CaseId,
                CourtId = userContext.CourtId,
                CaseSessionId = caseSession.Id,
                CaseSessionActId = null,
                NotificationDeliveryGroupId = NomenclatureConstants.NotificationDeliveryGroup.WithSummons,
                NotificationStateId = NomenclatureConstants.NotificationState.Ready,
                RegDate = DateTime.Now,
                NotificationPersonType = sessionNotificationList.NotificationPersonType,
                CasePersonId = sessionNotificationList.CasePersonId,
                CaseLawUnitId = sessionNotificationList.CaseLawUnitId,
                LawUnitAddressId = (sessionNotificationList.NotificationAddressId != null) ?
                                   ((sessionNotificationList.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CaseLawUnit) ? sessionNotificationList.NotificationAddressId : null) :
                                   null,
                CasePersonAddressId = (sessionNotificationList.NotificationAddressId != null) ?
                                      (
                                        (sessionNotificationList.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson) ?
                                        casePersonService.Get_CasePersonAddress(sessionNotificationList.CasePersonId ?? 0)
                                        .Where(x => x.AddressId == sessionNotificationList.NotificationAddressId).FirstOrDefault()?.Id :
                                        (int?)null
                                      ) :
                                      null,
                NotificationTypeId = NomenclatureConstants.NotificationType.FromListType(sessionNotificationList.NotificationListTypeId),
                IsOfficialNotification = true
            };

            if (!await CheckAccessAdd(model.CaseId, model.CaseSessionId, null, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }

            await SetViewbag(model);
            await SetViewbagCaption(caseSession.CaseId, caseSession.Id, null);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, sessionNotificationList?.NotificationListTypeId ?? 0).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.SessionPersonNotification);

            return View(nameof(Edit), model);
        }


        public async Task<IActionResult> PrintPdfsFromDeliveryItemIds([AllowHtml] string deliveryItemIdsJson)
        {
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            int[] deliveryIdArr = JsonConvert.DeserializeObject<int[]>(deliveryItemIdsJson, dateTimeConverter);
            var notificationIds = new List<NotificationPrintIdVM>();
            AddAuditInfo(AuditConstants.Operations.Print,
                        "Печат на маркираните",
                        $"{deliveryIdArr.Length} бр. Уведомления",
                        SourceTypeSelectVM.CaseNotification);
            foreach (int itemId in deliveryIdArr)
            {
                var delivery = deliveryItemService.getDeliveryItem(itemId);
                if (delivery.CaseNotificationId == null)
                    continue;
                int id = delivery.CaseNotificationId ?? 0;
                notificationIds.Add(new NotificationPrintIdVM
                {
                    CaseNotificationId = delivery.CaseNotificationId,
                    DocumentNotificationId = delivery.DocumentNotificationId
                });
            }
            return await PrintPdfs(notificationIds);
        }

        public async Task<JsonResult> GetPrintNotificationInListCount(int? CaseId, int? caseSessionId, int? caseSessionActId, bool isList, int? notificationListTypeId)
        {
            var filter = new NotificationPrintFilterVM
            {
                CaseId = CaseId,
                CaseSessionId = caseSessionId,
                CaseSessionActId = caseSessionActId,
                IsList = isList,
                NotificationListTypeId = notificationListTypeId
            };
            List<int> notificationIds = await service.NotificationIdSelect(filter);
            return Json(new { count = notificationIds.Count });
        }

        public async Task<IActionResult> PrintNotificationInList(int? CaseId, int? caseSessionId, int? caseSessionActId, bool isList, int? notificationListTypeId)
        {
            await CheckAccessAdd(CaseId ?? 0, caseSessionId, caseSessionActId, AuditConstants.Operations.Print);
            var filter = new NotificationPrintFilterVM
            {
                CaseId = CaseId,
                CaseSessionId = caseSessionId,
                CaseSessionActId = caseSessionActId,
                IsList = isList,
                NotificationListTypeId = notificationListTypeId
            };

            List<int> notificationIds = await service.NotificationIdSelect(filter);
            return await PrintPdfs(notificationIds.Select(x => new NotificationPrintIdVM { CaseNotificationId = x }).ToList());
        }
        [HttpGet]
        public IActionResult NotificationPrintFilter(int? CaseId, int? caseSessionId, int? caseSessionActId, bool isList, int? notificationListTypeId, int fromRowNumber, int toRowNumber)
        {
            var filter = new NotificationPrintFilterVM
            {
                CaseId = CaseId,
                CaseSessionId = caseSessionId,
                CaseSessionActId = caseSessionActId,
                IsList = isList,
                NotificationListTypeId = notificationListTypeId,
                FromRowNumber = fromRowNumber,
                ToRowNumber = toRowNumber
            };
            return PartialView("_NotificationPrintFilter", filter);
        }
        [HttpPost]
        public async Task<IActionResult> PrintNotificationInListOnFilter(NotificationPrintFilterVM model)
        {
            await CheckAccessAdd(model.CaseId ?? 0, model.CaseSessionId, model.CaseSessionActId, AuditConstants.Operations.Print);
            List<int> notificationIds = await service.NotificationIdSelect(model);
            return await PrintPdfs(notificationIds.Select(x => new NotificationPrintIdVM { CaseNotificationId = x }).ToList());
        }
        private async Task<byte[]> FillFilesInArchive(List<NotificationFileVM> files)
        {
            // create a working memory stream
            using (var memoryStream = new MemoryStream())
            {
                // create a zip
                using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        ZipArchiveEntry zipItem = zip.CreateEntry(file.FileName);
                        // add the item bytes to the zip entry by opening the original file and copying the bytes
                        using (var originalFileMemoryStream = new MemoryStream(file.Content))
                        {
                            using (Stream entryStream = zipItem.Open())
                            {
                                await originalFileMemoryStream.CopyToAsync(entryStream);
                            }
                        }
                    }
                }
                return memoryStream.ToArray();
            }
        }

        public async Task<IActionResult> PrintPdfs(List<NotificationPrintIdVM> notificationIds)
        {
            MemoryStream memoryStreamUnion = new MemoryStream();
            PdfDocument pdfDoc = new PdfDocument(new PdfWriter(memoryStreamUnion));
            PdfMerger merger = new PdfMerger(pdfDoc);
            bool isWrite = false;
            var FileName = string.Empty;
            var noPdfFiles = new List<NotificationFileVM>();
            foreach (var notificationId in notificationIds)
            {
                if (notificationId.DocumentNotificationId != null)
                {
                    (var pdfBytesRDoc, FileName) = await makePrintAndSavePdf(notificationId.DocumentNotificationId ?? 0, true);
                    if (pdfBytesRDoc != null)
                    {
                        using (MemoryStream memoryStream = new MemoryStream(pdfBytesRDoc))
                        {
                            using (PdfDocument newDoc = new PdfDocument(new PdfReader(memoryStream)))
                            {
                                var numberOfPages = newDoc.GetNumberOfPages();
                                merger.Merge(newDoc, 1, numberOfPages);
                                newDoc.Close();
                                if ((numberOfPages % 2) == 1)
                                {
                                    await PrintBlankPage(merger, notificationId.CaseNotificationId ?? 0, notificationId.DocumentNotificationId ?? 0);
                                }
                            }
                        }
                    }
                    isWrite = true;
                    continue;
                }

                var id = notificationId.CaseNotificationId ?? 0;
                (var pdfBytesR, FileName) = await makePrintAndSavePdf(id, true);
                if (pdfBytesR != null)
                {
                    using (MemoryStream memoryStream = new MemoryStream(pdfBytesR))
                    {
                        using (PdfDocument newDoc = new PdfDocument(new PdfReader(memoryStream)))
                        {
                            var numberOfPages = newDoc.GetNumberOfPages();
                            merger.Merge(newDoc, 1, numberOfPages);
                            newDoc.Close();
                            if ((numberOfPages % 2) == 1)
                            {
                                await PrintBlankPage(merger, notificationId.CaseNotificationId ?? 0, notificationId.DocumentNotificationId ?? 0);
                            }
                        }
                    }
                    isWrite = true;
                    if (!await service.IsNotificationOnFastProcess(id))
                    {
                        continue;
                    }
                    var linkDocuments = await service.GetLinkDocument(id);
                    await PrintNotificationDocuments(merger, noPdfFiles, notificationId, linkDocuments);

                    var actAndComplains = await service.GetActAndComplainDocument(id);
                    await PrintNotificationDocuments(merger, noPdfFiles, notificationId, actAndComplains);

                    var mongoFiles = await service.GetCaseNotificationMongoFiles(id);
                    await PrintNotificationDocuments(merger, noPdfFiles, notificationId, mongoFiles);

                    var caseNotificationDocuments = await service.GetCaseNotificationDocuments(id);
                    await PrintNotificationDocuments(merger, noPdfFiles, notificationId, caseNotificationDocuments);
                }

            }
            string FileNameU = notificationIds.Count() == 1 ? FileName : DateTime.Now.ToString("yyyyMMdd_hhmm") + ".pdf";

            if (isWrite)
            {
                pdfDoc.Close();
                var pdfBytes = memoryStreamUnion.ToArray();
                if (!noPdfFiles.Any())
                {
                    return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, FileNameU);
                }
                else
                {
                    noPdfFiles.Insert(0, new NotificationFileVM
                    {
                        FileName = FileNameU,
                        Content = pdfBytes
                    });
                    var zip = await FillFilesInArchive(noPdfFiles);
                    FileNameU = FileNameU.Replace(".pdf", ".zip");
                    return File(zip, System.Net.Mime.MediaTypeNames.Application.Zip, FileNameU);
                }
            }
            else
            {
                byte[] pdfBytes = await NoDataPrintPdf();
                return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, FileNameU);
            }
        }

        private async Task PrintNotificationDocuments(PdfMerger merger, List<NotificationFileVM> noPdfFiles, NotificationPrintIdVM notificationId, List<NotificationFileVM> linkDocuments)
        {
            noPdfFiles.AddRange(linkDocuments.Where(x => !x.IsPdf));
            foreach (var linkDocument in linkDocuments.Where(x => x.IsPdf))
            {
                using (MemoryStream memoryStreamLink = new MemoryStream(linkDocument.Content))
                {
                    using (PdfDocument newDocLink = new PdfDocument(new PdfReader(memoryStreamLink)))
                    {
                        var numberOfPages = newDocLink.GetNumberOfPages();
                        merger.Merge(newDocLink, 1, newDocLink.GetNumberOfPages());
                        newDocLink.Close();
                        if ((numberOfPages % 2) == 1)
                        {
                            await PrintBlankPage(merger, notificationId.CaseNotificationId ?? 0, notificationId.DocumentNotificationId ?? 0);
                        }
                    }
                }
            }
        }

        private async Task<byte[]> NoDataPrintPdf()
        {
            var htmlModel = new TinyMCEVM();
            htmlModel.Style = "";
            htmlModel.Text = "<div style=\"font-zize: 30px; margin: 30px\">Няма уведомления за печат</div>";
            var pdfBytes = await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlModel)
            {
                PageOrientation = (Orientation)htmlModel.PageOrientation,
                PageMargins = new Margins(10, 5, 10, 5),
                PageSize = Size.A4,
                CustomSwitches = htmlModel.SmartShrinkingPDF ? "" : "--disable-smart-shrinking"
            }.GetByte(this.ControllerContext);
            return pdfBytes;
        }

        private async Task PrintBlankPage(PdfMerger merger, int caseNotificationId, int documentNotificationId)
        {
            var htmlBlank = printDocumentService.GetHtmlTemplateNull(caseNotificationId, documentNotificationId);
            var pdfBytesBlank = await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlBlank)
            {
                PageOrientation = (Orientation)htmlBlank.PageOrientation,
                PageMargins = new Margins(10, 5, 10, 5),
                PageSize = Size.A4,
                CustomSwitches = "--disable-smart-shrinking"
            }.GetByte(this.ControllerContext);
            using (MemoryStream memoryStreamBlank = new MemoryStream(pdfBytesBlank))
            {
                using (PdfDocument newDocBlank = new PdfDocument(new PdfReader(memoryStreamBlank)))
                {
                    merger.Merge(newDocBlank, 1, 1);
                    newDocBlank.Close();
                }
            }
        }


        public JsonResult LoadDropDownListForPerson(int casePersonId, int casePersonLinkId, int notificationTypeId, int notificationDeliveryGroupId, int? caseSessionId)
        {
            var linkListVM = casePersonLinkService.GetLinkForPerson(casePersonId, NomenclatureConstants.FilterPersonOnNotification, notificationTypeId, null);
            linkListVM = service.FilterLinkOnSession(linkListVM, caseSessionId, null);
            List<SelectListItem> addrList = service.GetAddrForPerson(linkListVM, casePersonId, casePersonLinkId, notificationDeliveryGroupId);
            var linkList = casePersonLinkService.ListForPersonToDropDown(linkListVM, casePersonId);
            return Json(new { linkList, addrList });
        }

        public JsonResult LoadAddrForPerson(int casePersonId, int casePersonLinkId, int notificationDeliveryGroupId)
        {
            List<SelectListItem> addrList;
            if (casePersonLinkId > 0)
            {
                var oldLinks = new List<int>() { casePersonLinkId };
                var linkListVM = casePersonLinkService.GetLinkForPerson(casePersonId, false, 0, oldLinks);
                addrList = service.GetAddrForPerson(linkListVM, casePersonId, casePersonLinkId, notificationDeliveryGroupId);
            }
            else
            {
                addrList = casePersonService.GetDDL_CasePersonAddress(casePersonId, notificationDeliveryGroupId);
            }
            return Json(addrList);
        }

        public JsonResult LoadMLinkForPerson(int caseNotificationId, int casePersonId, int notificationTypeId, int notificationDeliveryGroupId, int? caseSessionId)
        {
            var linkList = service.CasePersonLinksByNotificationId(caseNotificationId, casePersonId, NomenclatureConstants.FilterPersonOnNotification, notificationTypeId, caseSessionId)
                                  .Where(x => x.IsActive).ToList();
            var addrList = casePersonService.GetDDL_CasePersonAddress(casePersonId, notificationDeliveryGroupId);
            return Json(new { linkList, addrList });
        }

        [HttpPost]
        public async Task<IActionResult> CaseNotification_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseNotification, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var caseNotification = service.GetReadonly<CaseNotification>(model.Id);
            if (!await CheckAccessWithId(caseNotification.Id, caseNotification.CaseId, caseNotification.CaseSessionId, caseNotification.CaseSessionActId, AuditConstants.Operations.Delete))
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });

            if (service.SaveExpireInfoPlus(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseNotification, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseNotificationExpireOK);
                return Json(new { result = true, redirectUrl = model.ReturnUrl });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        [HttpPost]
        public JsonResult CaseSessionActComplainDDL(int caseId, int caseSessionActId, int htmlTemplateId)
        {
            return Json(caseSessionActComplainService.GetDropDownListForAct(caseId, [caseSessionActId], htmlTemplateId));
        }

        public JsonResult CaseSessionActComplainMultiDDL(int caseId, int caseNotificationId, string caseSessionActIdsStr, int htmlTemplateId)
        {
            var caseSessionActIds = Array.Empty<int>();
            if (!string.IsNullOrEmpty(caseSessionActIdsStr))
            {
                caseSessionActIds = caseSessionActIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                caseSessionActIds = caseSessionActIds.Where(x => x > 0).ToArray();
            }

            var caseNotification = service.ReadById(caseNotificationId);
            service.InitCaseNotificationComplains(caseNotification);
            var result = caseSessionActComplainService.GetDropDownListForAct(caseId, caseSessionActIds, htmlTemplateId);
            if (caseNotification != null)
            {
                foreach (var item in result)
                {
                    item.Selected = caseNotification.CaseNotificationComplains
                                                    .Any(x => x.IsChecked &&
                                                              x.CaseSessionActComplainId.ToString() == item.Value);
                }
            }
            return Json(result);
        }

        private async Task SetViewbag(CaseNotification model)
        {
            int caseId = model.CaseId;
            int? caseSessionId = model.CaseSessionId;
            var caseSessionActIds = Array.Empty<int>();
            if ((model.CaseSessionActId ?? 0) > 0)
                caseSessionActIds = new int[] { model.CaseSessionActId ?? 0 };
            if (model.CaseNotificationActs?.Any() == true)
            {
                caseSessionActIds = model.CaseNotificationActs?.Select(x => x.CaseSessionActId).ToArray() ?? caseSessionActIds;
            }
            int notificationStateId = model.NotificationStateId;
            int notificationGroupId = model.NotificationDeliveryGroupId ?? 0;
            int personId = model.CasePersonId ?? 0;
            int personLinkId = model.CasePersonLinkId ?? 0;
            int notificationTypeId = model.NotificationTypeId ?? 0;
            int toCourtId = model.ToCourtId ?? 0;
            int caseLawUnitId = model.CaseLawUnitId ?? 0;

            var caseCase = await caseService.GetCaseInfo(model.CaseId);
            //var caseCase = await caseService.Case_GetById(model.CaseId);
            ViewBag.CaseName = caseCase.CaseTypeCodeShortNumberRegDate;

            ViewBag.NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>();
            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(notificationGroupId, notificationStateId);

            var oldLinks = new List<int>() { model.CasePersonLinkId ?? 0 };
            if (model.CaseNotificationMLinks != null)
                oldLinks.AddRange(model.CaseNotificationMLinks.Select(x => x.CasePersonLinkId ?? 0).ToList());
            var linkListVM = casePersonLinkService.GetLinkForPerson(personId, NomenclatureConstants.FilterPersonOnNotification, model.NotificationTypeId ?? 0, oldLinks);
            linkListVM = service.FilterLinkOnSession(linkListVM, model.CaseSessionId, oldLinks);
            ViewBag.CasePersonLinkId_ddl = casePersonLinkService.ListForPersonToDropDown(linkListVM, personId); // casePersonLink.GetDropDownListForPerson(personId);
            ViewBag.CasePersonId_ddl = casePersonService.GetDropDownList(caseId, caseSessionId, true, model.NotificationTypeId, model.CasePersonId, NomenclatureConstants.FilterPersonOnNotification);
            List<SelectListItem> addrList = service.GetAddrForPerson(linkListVM, personId, personLinkId, model.NotificationDeliveryGroupId ?? 0);
            ViewBag.CasePersonAddressId_ddl = addrList;

            //List<SelectListItem> notificationDeliveryGroups = nomService.GetDropDownList<NotificationDeliveryGroup>();
            //ViewBag.NotificationDeliveryGroupId_ddl = notificationDeliveryGroups;
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupDDL(model.NotificationTypeId ?? 0, model.CaseId);

            var NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>().Select(x => new { value = x.Value, text = x.Text });
            ViewBag.NotificationTypeSummonsJson = JsonConvert.SerializeObject(NotificationTypeId_ddl.Where(x => x.value != NomenclatureConstants.NotificationType.GovernmentPaper.ToString()).ToList());
            ViewBag.NotificationTypeGovernmentJson = JsonConvert.SerializeObject(NotificationTypeId_ddl.Where(x => x.value == NomenclatureConstants.NotificationType.GovernmentPaper.ToString()).ToList());

            var HtmlTemplateId_ddl = nomService.GetDDL_HtmlTemplate(notificationTypeId, caseId, model.HtmlTemplateId);
            ViewBag.HtmlTemplateId_ddl = HtmlTemplateId_ddl.Select(x => new SelectListItem() { Value = x.Value, Text = x.Text }).ToList();
            ViewBag.HtmlTemplateId_json = JsonConvert.SerializeObject(HtmlTemplateId_ddl);

            ViewBag.NotificationDeliveryTypeId_ddl = nomService.GetDDL_NotificationDeliveryType(NomenclatureConstants.NotificationDeliveryGroup.WithCourier);
            ViewBag.ToCourtId_ddl = commonService.CourtForDelivery_SelectDDL(-1);
            ViewBag.CourtId = userContext.CourtId;

            ViewBag.CaseLawUnitId_ddl = caseLawUnitService.CaseLawUnit_SelectForDropDownList(caseId, caseSessionId);
            var lawUnitAddressId_ddl = commonService.LawUnitAddress_SelectDDL_ByCaseLawUnitId(caseLawUnitId);
            ViewBag.LawUnitAddressId_ddl = lawUnitAddressId_ddl;

            ViewBag.DeliveryOperId_ddl = serviceDeliveryOper.DeliveryOperForNotificationStateSelect(notificationStateId);
            ViewBag.DeliveryReasonId_ddl = nomService.GetDropDownList<DeliveryReason>();

            ViewBag.LawUnitId_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, toCourtId);
            ViewBag.DeliveryAreaId_ddl = areaService.DeliveryAreaSelectDDL(toCourtId, false);
            if (caseSessionId != null)
            {
                var ddl = await caseSessionActService.GetDropDownList_CaseSessionActEnforced(caseId);/* caseSessionActService.GetDropDownListBySessionId(caseSessionId ?? 0);*/
                ViewBag.CaseSessionActId_ddl = ddl;
                ViewBag.MultiActIdVM_ddl = ddl;
            }
            if (caseSessionActIds.Any())
                ViewBag.CaseSessionActComplainId_ddl = caseSessionActComplainService.GetDropDownListForAct(caseId, caseSessionActIds, model.HtmlTemplateId ?? 0);
            ViewBag.DocumentSenderPersonId_ddl = service.DocumentSenderPersonDDL(model.CaseId);
            ViewBag.InstitutionDocumentId_ddl = service.GetDDL_ConnectedCases(model.CaseId);
            ViewBag.MoneyObligationId_ddl = service.GetMoneyObligationDDL(model.CasePersonId ?? 0, model.CasePersonLinkId ?? 0, model.CaseSessionActId ?? 0);
            ViewBag.NotificationIspnReasonId_ddl = service.GetNotificationIspnReasonDDL();

            var listNotificationTypeId = NomenclatureConstants.NotificationType.ToListType(model.NotificationTypeId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, listNotificationTypeId).DeleteOrDisableLast();
            ViewBag.DocumentsVM_ddl = await service.GetDocumentsDDL(caseId);

            if (model.EpepCasePersonId > 0)
            {
                ViewBag.EpepCasePersonName = await service.GetPropByIdAsync<CasePerson, string>(x => x.Id == model.EpepCasePersonId.Value, x => x.FullName);
            }
        }

        /// <summary>
        /// Метод за изчитане на данни за типа на списъкът за уведомяване/съобщаване
        /// </summary>
        /// <returns></returns>
        [DisableAudit]
        [HttpGet]
        public IActionResult GetDDL_NotificationListType()
        {
            var model = service.GetDDL_NotificationListType();
            return Json(model);
        }

        /// <summary>
        /// Метод за извличане сума на Държавна такса
        /// </summary>
        /// <returns></returns>
        [DisableAudit]
        [HttpPost]
        public IActionResult GetMoneyObligationDDL(int casePersonId, int caseLinkId, int caseSessionActId)
        {
            var model = service.GetMoneyObligationDDL(casePersonId, caseLinkId, caseSessionActId);
            return Json(model);
        }

        [HttpPost]
        public async Task<JsonResult> IsNotificationDeliveryGroupByEpep(int caseId, int? caseSessionId, int casePersonId, string casePersonLinkIds)
        {
            var result = await service.IsNotificationDeliveryGroupByEpep(caseId, caseSessionId, casePersonId, casePersonLinkIds);

            return Json(new { result = result });
        }

        public IActionResult VksNotificationList(int caseSessionId)
        {
            var model = vksNotificationService.GetNotificationItem(caseSessionId);
            return View(model);
        }
        [HttpPost]
        public IActionResult VksNotificationList(VksNotificationListVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (vksNotificationService.SaveData(model))
            {
                SetSuccessMessage("Списъка за призоваване с държавен вестник беше актуализиран успешно.");
            }
            ModelState.Clear();
            vksNotificationService.FillNotificationItemDDL(model);

            return RedirectToAction("Preview", "CaseSession", new { id = model.CaseSessionId });
        }
        public JsonResult GetVkspersonAdress(int casePersonId, int? casePersonLinkId)
        {
            var ddl = vksNotificationService.GetVksPersonAdress(casePersonId, casePersonLinkId);
            return Json(new { addrList = ddl });
        }
        public IActionResult GetVksNotificationList(int caseSessionId)
        {
            var model = vksNotificationService.GetNotificationItem(caseSessionId);
            return PartialView(nameof(VksNotificationList), model);
        }

        [HttpGet]
        public JsonResult GetDDL_NotificationStateFromDeliveryGroup(int notificationDeliveryGroupId, int notificationStateId, int caseSessionId)
        {
            var datePrevSess = service.GetDatePrevSession(caseSessionId);
            var notificationStates = nomService.GetDDL_NotificationStateFromDeliveryGroup(notificationDeliveryGroupId, notificationStateId);
            return Json(new { datePrevSess = datePrevSess?.ToString(FormattingConstant.NormalDateFormat + " HH:mm"), notificationStates });
        }
        [HttpGet]
        public async Task<IActionResult> NotificationGroup(int caseId, int caseSessionId, int notificationListTypeId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseNotification, null, AuditConstants.Operations.View, caseId))
            {
                return Redirect_Denied();
            }

            int notificationTypeId = NomenclatureConstants.NotificationType.FromListType(notificationListTypeId);
            await SetViewBagNotificationGroup(notificationTypeId, caseId, caseSessionId, null);
            var model = service.GenerateNotificationGroup(caseId, caseSessionId, notificationListTypeId);
            await service.LoadNotificationGroupList(model);
            return View(model);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> NotificationGroup(NotificationGroupVM model)
        {
            await SetViewBagNotificationGroup(model.NotificationTypeId, model.CaseId, model.CaseSessionId, null);
            model.NotificationItems = new();
            await service.LoadNotificationGroupList(model);
            ModelState.Clear();
            return View(nameof(NotificationGroup), model);
        }
        async Task SetViewBagNotificationGroup(int? notificationTypeId, int caseId, int caseSessionId, int? htmlTemplateId)
        {
            ViewBag.NotificationTypeId_ddl = (await nomService.GetDropDownListAsync<NotificationType>()).Where(x => x.Value != NomenclatureConstants.NotificationType.GovernmentPaper.ToString()).ToList();
            var HtmlTemplateId_ddl = nomService.GetDDL_HtmlTemplate(notificationTypeId ?? 0, caseId, htmlTemplateId);
            ViewBag.HtmlTemplateId_ddl = HtmlTemplateId_ddl.Select(x => new SelectListItem() { Value = x.Value, Text = x.Text }).ToList();
            ViewBag.HtmlTemplateId_json = JsonConvert.SerializeObject(HtmlTemplateId_ddl);
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupDDL(notificationTypeId ?? 0, caseId);
            var ddl = await caseSessionActService.GetDropDownList_CaseSessionActEnforced(caseId);
            ViewBag.CaseSessionActId_ddl = ddl;
            ViewBag.MultiActIdVM_ddl = ddl;


            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(NomenclatureConstants.NotificationDeliveryGroup.WithSummons, NomenclatureConstants.NotificationState.Ready);
            var model = new CaseNotification()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                CaseSessionId = caseSessionId,
            };
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, 0).DeleteOrDisableLast();
        }
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> NotificationGroupSave(NotificationGroupVM model)
        {
            var logVM = new DeliveryLogVM()
            {
                Action = "Множествено добавяне",
                PageLabel = "Призовки/съобщения",
                PageUrl = "CaseNotification/Edit"
            };
            ValidateModelGroup(model);
            await SetViewBagNotificationGroup(model.NotificationTypeId, model.CaseId, model.CaseSessionId, model.HtmlTemplateId);
            SetHelpFile(HelpFileValues.SessionNotification);
            if (@NomenclatureConstants.NotificationDeliveryGroup.OnMoment(model.NotificationDeliveryGroupId))
            {
                if (model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnEMail &&
                    model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.ByRNFL)
                {
                    if (!ModelState.IsValid)
                    {
                        ModelState.Remove(nameof(model.HtmlTemplateId));
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                await service.LoadNotificationGroupList(model);
                return View(nameof(NotificationGroup), model);
            }
            try
            {
                await service.SaveMultiNotification(model, logVM);
                return RedirectToAction("Preview", "CaseSession", new { id = model.CaseSessionId, tab = "#tabNotification" });
            }
            catch (Exception)
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(NotificationGroup), model);
        }

        /// <summary>
        /// Аддед Бай Косьо
        /// </summary>
        /// <param name="model"></param>
        private void ValidateModelGroup(NotificationGroupVM model)
        {
            for (int i = 0; i < model.NotificationItems.Count; i++)
            {

                var notificationItem = model.NotificationItems[i];
                if (!notificationItem.IsChecked)
                {
                    continue;
                }

                if ((notificationItem.LinkId ?? 0) <= 0)
                {
                    if (casePersonService.IsPersonDead(notificationItem.PersonId))
                    {
                        ModelState.AddModelError($"NotificationItems[{i}].PersonId", "Лицето е починало, не може да бъде уведомено!");
                    }
                }


                if (model.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
                {
                    var epepInfo = casePersonLinkService.GetEpepSummonInfo(new CaseNotification()
                    {
                        CaseSessionId = model.CaseSessionId,
                        CasePersonId = notificationItem.PersonId,
                        CasePersonLinkId = notificationItem.LinkId,
                        NotificationTypeId = model.NotificationTypeId
                    });
                    if (epepInfo == null || !epepInfo.CanSummonByEpep)
                    {
                        ModelState.AddModelError($"NotificationItems[{i}].PersonId", "За избраното лице няма разрешен достъп през ЕПЕП");
                    }
                }
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetCourtForDelivery_Select2Data()
        {
            var deliveryAreaDdl = await areaService.GetCourtForDelivery_Select2Data();
            return Json(deliveryAreaDdl);
        }
        public async Task<IActionResult> PrintPdfNotification(int id)
        {
            (var pdfBytesR, var FileName) = await makePrintAndSavePdfDocumentNotification(id);
            return File(pdfBytesR, System.Net.Mime.MediaTypeNames.Application.Pdf, FileName);
        }
        private async Task<(byte[] pdfBytes, string FileName)> makePrintAndSavePdfDocumentNotification(int id)
        {
            var cdnResult = await documentNotificationService.ReadPrintedFile(id);
            TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplateDocumentNotification(id);
            if (cdnResult == null)
            {
                if (htmlModel == null)
                {
                    return (null, "");
                }
                var cdnResultDraft = await documentNotificationService.ReadDraftFile(id);
                if (cdnResultDraft != null)
                {
                    htmlModel.Text = Encoding.UTF8.GetString(Convert.FromBase64String(cdnResultDraft.FileContentBase64));
                }
                var pdfBytes = await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlModel)
                {
                    PageOrientation = (Orientation)htmlModel.PageOrientation,
                    PageMargins = new Margins(10, 5, 10, 5),
                    PageSize = Size.A4,
                    CustomSwitches = htmlModel.SmartShrinkingPDF ? "" : "--disable-smart-shrinking"
                }.GetByte(this.ControllerContext);
                pdfBytes = await ZoomIfHave3Pages(htmlModel, pdfBytes);
                await documentNotificationService.SavePrintedFile(id, pdfBytes);
                cdnResult = await documentNotificationService.ReadPrintedFile(id);

            }

            var pdfBytesC = Convert.FromBase64String(cdnResult.FileContentBase64);
            if ((Orientation)htmlModel.PageOrientation == Orientation.Landscape)
            {
                pdfBytesC = RotateSecondPage180(pdfBytesC);
            }
            return (pdfBytesC, cdnResult.FileName);
        }
        [HttpGet]
        public IActionResult RajonAlert()
        {
            return PartialView("_RajonAlert");
        }

        [HttpGet]
        public async Task<JsonResult> GetIsValidAddress(int casePersonAddressId, long lawUnitAddressId)
        {
            var isValid = await IsValidAddreess(casePersonAddressId, lawUnitAddressId);
            return Json(new { 
                isValid, 
                errMsg = isValid? string.Empty : NomenclatureConstants.NotificationAddressError.Message
            });
        }
    }
}