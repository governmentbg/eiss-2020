using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Rotativa.Extensions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using iText.Kernel.Utils;
using Newtonsoft.Json.Converters;
using Rotativa.AspNetCore.Options;
using System.Text;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;

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
            IVksNotificationService _vksNotificationService)
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
        public IActionResult Index(int id, int? caseSessionId, int? caseSessionActId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseNotification, null, AuditConstants.Operations.View, id))
            {
                return Redirect_Denied();
            }
            ViewBag.caseId = id;
            ViewBag.caseSessionId = caseSessionId;
            ViewBag.caseSessionActId = caseSessionActId;
            SetViewbagCaption(id, caseSessionId, caseSessionActId);
            return View();
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId, int? caseSessionId, int? caseSessionActId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseNotification, null, AuditConstants.Operations.View, caseId))
            {
                return Redirect_Denied();
            }
            var data = service.CaseNotification_Select(caseId, caseSessionId, caseSessionActId);
            return request.GetResponse(data);
        }

        private bool CheckAccessAdd(int caseId, int? caseSessionId, int? caseSessionActId, string operation)
        {
            if (caseSessionActId != null)
                return CheckAccess(service, SourceTypeSelectVM.CaseSessionActNotification, null, operation, caseSessionActId);
            else
            {
                if (caseSessionId != null)
                    return CheckAccess(service, SourceTypeSelectVM.CaseSessionNotification, null, operation, caseSessionId);
                else
                    return CheckAccess(service, SourceTypeSelectVM.CaseNotification, null, operation, caseId);
            }
        }

        private bool CheckAccessWithId(int id, int caseId, int? caseSessionId, int? caseSessionActId, string operation)
        {
            if (caseSessionActId != null)
                return CheckAccess(service, SourceTypeSelectVM.CaseSessionActNotification, id, operation);
            else
            {
                if (caseSessionId != null)
                    return CheckAccess(service, SourceTypeSelectVM.CaseSessionNotification, id, operation);
                else
                    return CheckAccess(service, SourceTypeSelectVM.CaseNotification, id, operation);
            }
        }

        public IActionResult Add(int caseId, int? caseSessionId, int? caseSessionActId, int notificationPersonType, int notificationTypeId)
        {
            if (!CheckAccessAdd(caseId, caseSessionId, caseSessionActId, AuditConstants.Operations.Append))
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

            SetViewbag(model);
            SetViewbagCaption(caseId, caseSessionId, caseSessionActId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, 0).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.SessionNotification);
            return View(nameof(Edit), model);
        }

        public IActionResult AddWithParent(int caseId, int? caseSessionId, int? caseSessionActId, int? caseParentId)
        {
            if (!CheckAccessAdd(caseId, caseSessionId, caseSessionActId, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }
            var caseNotification = service.ReadById(caseParentId);
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

            SetViewbag(caseNotification);
            SetViewbagCaption(caseId, caseSessionId, caseSessionActId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, 0).DeleteOrDisableLast();
            return View(nameof(Edit), model);
        }

        public IActionResult Edit(int id)
        {
            var model = service.ReadById(id);
            if (model.DeliveryDateCC == null)
                model.DeliveryDateCC = DateTime.Now;
            if (model == null)
            {
                throw new NotFoundException("Търсеното от Вас уведомление не е намерен и/или нямате достъп до него.");
            }
            if (!CheckAccessWithId(id, model.CaseId, model.CaseSessionId, model.CaseSessionActId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            SetViewbag(model);
            SetViewbagCaption(model.CaseId, model.CaseSessionId, model.CaseSessionActId);
            var listNotificationTypeId = NomenclatureConstants.NotificationType.ToListType(model.NotificationTypeId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, listNotificationTypeId).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.SessionNotification);

            return View(nameof(Edit), model);
        }

        private void SetViewbagCaption(int caseId, int? caseSessionId, int? caseSessionActId)
        {
            if (caseSessionId != null)
            {
                var caseSession = sessionService.CaseSessionById(caseSessionId ?? 0);
                ViewBag.CaseSessionName = caseSession.SessionType?.Label + " " + caseSession.DateFrom.ToString("dd.MM.yyyy");
                ViewBag.caseSessionId = caseSessionId;
            }

            var caseCase = service.GetById<Case>(caseId);
            ViewBag.CaseName = caseCase.RegNumber;
            ViewBag.CaseId = caseCase.Id;

            if (caseSessionActId != null)
            {
                var caseAct = service.GetById<CaseSessionAct>(caseSessionActId ?? 0);
                var actType = nomService.GetById<ActType>(caseAct.ActTypeId);
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

        private void ValidateModel(CaseNotification model, int[] complainIds)
        {
            var requireAddr = (!NomenclatureConstants.NotificationDeliveryGroup.OnMoment(model.NotificationDeliveryGroupId) &&
                               model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.WithGovernmentPaper);
            if (model.NotificationTypeId < 0)
                ModelState.AddModelError(nameof(CaseNotification.NotificationTypeId), "Въведете вид известие.");
            if (model.Id == 0)
            {
                var _session = service.GetById<CaseSession>(model.CaseSessionId);
                if (_session.DateExpired != null)
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

            if (model.NotificationStateId < 0)
                ModelState.AddModelError(nameof(CaseNotification.NotificationStateId), "Няма избран статус");
            if (model.Id == 0 && model.IsMultiLink != true && (model.CasePersonLinkId ?? 0) <= 0)
            {
                if (casePersonService.IsPersonDead(model.CasePersonId ?? 0))
                {
                    ModelState.AddModelError(nameof(CaseNotification.CasePersonId), "Лицето е починало, не може да бъде уведомено!");
                }
            }
            if (model.NotificationDeliveryGroupId == @NomenclatureConstants.NotificationDeliveryGroup.WithCityHall ||
               model.NotificationDeliveryGroupId == @NomenclatureConstants.NotificationDeliveryGroup.WithCourier)
            {
                if (model.NotificationStateId == @NomenclatureConstants.NotificationState.Delivered ||
                   model.NotificationStateId == @NomenclatureConstants.NotificationState.Delivered47 ||
                   model.NotificationStateId == @NomenclatureConstants.NotificationState.Delivered50 ||
                   model.NotificationStateId == @NomenclatureConstants.NotificationState.Delivered51 ||
                   model.NotificationStateId == @NomenclatureConstants.NotificationState.UnDelivered)
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
            if (NomenclatureConstants.NotificationDeliveryGroup.OnMoment(model.NotificationDeliveryGroupId))
            {
                if (model.DeliveryDate < model.RegDate.Date)
                {
                    ModelState.AddModelError(nameof(model.DeliveryDate), $"{MessageConstant.ValidationErrors.DeliveryDateBeforeRegDate} {model.RegDate.ToString(FormattingConstant.NormalDateFormat)}");
                }
                if (model.DeliveryDate > DateTime.Now.AddMinutes(10))
                {
                    ModelState.AddModelError(nameof(model.DeliveryDate), MessageConstant.ValidationErrors.DeliveryDateFuture);
                }
            }
            if (model.HtmlTemplateId > 0)
            {
                var htmlTemplate = service.GetById<HtmlTemplate>(model.HtmlTemplateId);
                if (htmlTemplate?.HaveMultiActComplain == true && htmlTemplate?.RequiredSessionActComplain == true && complainIds.Length == 0)
                {
                    ModelState.AddModelError(nameof(model.MultiComplainIdVM), "Изберете поне една жалба");
                }
            }
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult Edit(CaseNotification model, [AllowHtml] string casePersonLinksJson)
        {
            var complainIds = Array.Empty<int>();
            if (!string.IsNullOrEmpty(model.MultiComplainIdResultVM))
            {
                complainIds = model.MultiComplainIdResultVM.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                complainIds = complainIds.Where(x => x > 0).ToArray();
            }
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            List<CaseNotificationMLink> casePersonLinks = JsonConvert.DeserializeObject<List<CaseNotificationMLink>>(casePersonLinksJson, dateTimeConverter);

            SetViewbag(model);
            var listNotificationTypeId = NomenclatureConstants.NotificationType.ToListType(model.NotificationTypeId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, listNotificationTypeId).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.SessionNotification);

            if (@NomenclatureConstants.NotificationDeliveryGroup.OnMoment(model.NotificationDeliveryGroupId))
            {
                if (model.DatePrint == null && model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnEMail)
                    model.DatePrint = DateTime.Now;

                if (model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnPhone &&
                        model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnEMail)
                {
                    if (!ModelState.IsValid)
                    {
                        ModelState.Remove(nameof(model.LawUnitAddressId));
                        ModelState.Remove(nameof(model.CasePersonAddressId));
                    }
                    model.LawUnitAddressId = null;
                    model.CasePersonAddressId = null;
                }
                if (model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnEMail)
                {
                    if (!ModelState.IsValid)
                    {
                        ModelState.Remove(nameof(model.HtmlTemplateId));
                    }
                    model.HtmlTemplateId = null;
                }
            }

            if (model.NotificationDeliveryGroupId == @NomenclatureConstants.NotificationDeliveryGroup.WithCityHall ||
                model.NotificationDeliveryGroupId == @NomenclatureConstants.NotificationDeliveryGroup.WithCourier)
            {
                model.DeliveryDate = model.DeliveryDateCC;
                model.DeliveryInfo = model.DeliveryInfoCC;
            }

            ValidateModel(model, complainIds);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.CaseNotification_SaveData(model, casePersonLinks, complainIds))
            {
                if (currentId == 0)
                    CheckAccessAdd(model.CaseId, model.CaseSessionId, model.CaseSessionActId, AuditConstants.Operations.Append);
                else
                    CheckAccessWithId(model.Id, model.CaseId, model.CaseSessionId, model.CaseSessionActId, AuditConstants.Operations.Update);

                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
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
            TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplateNotification(sourceId);
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
                if (!CheckAccess(service, SourceTypeSelectVM.CaseNotification, sourceId, AuditConstants.Operations.Update))
                {
                    return Redirect_Denied();
                }
                htmlModel = printDocumentService.FillHtmlTemplateNotification(sourceId);
                htmlModel.SourceId = sourceId;
                htmlModel.SourceType = SourceTypeSelectVM.CaseNotificationPrint;
                var caseNotification = service.ReadWithMlinkById(sourceId);
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEditTinyMCE(caseNotification).DeleteOrDisableLast();
            }
            else
            {
                if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionNotificationListPersonLawUnit, null, AuditConstants.Operations.GeneratingFile, sourceId))
                {
                    return Redirect_Denied();
                }
                var session = sessionService.CaseSessionVMById(sourceId);
                var caseCase = caseService.Case_GetById(session.CaseId);
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

                Print_CaseSessionNotificationListVM print_CaseSessionNotificationList = new Print_CaseSessionNotificationListVM()
                {
                    Title = "Списък на " + typeList + " лица по " + caseCase.CaseTypeCode + " " + caseCase.RegNumber + "/" + caseCase.RegDate.ToString("dd.MM.yyyy"),
                    NameReport = "",
                    SessionTitle = session.SessionTypeLabel + " от " + session.DateFrom.ToString("dd.MM.yyyy HH:mm"),
                    NotificationLists = service.CaseSessionNotificationList_Select(sourceId, sourceType).ToList()
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
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> DraftTinyMCE(TinyMCEVM htmlModel)
        {
            if (string.IsNullOrEmpty(htmlModel.Style))
                htmlModel.Style = FormattingConstant.TinyMceTableDefStyle + FormattingConstant.PrintTableDefStyle;
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
                    service.CaseNotification_SaveData(caseNotification, caseNotification.CaseNotificationMLinks?.ToList(), null);
                    if (caseNotification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
                    {
                        _ = await makePrintAndSavePdf(caseNotification.Id);
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
            if (!CheckAccess(service, SourceTypeSelectVM.CaseNotification, id, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }
            TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplateNotificationTestOne(id, htmlTemplateId);
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
            TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplateDocumentTemplate(id);
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            return View("PreviewRaw", htmlModel);
        }
        public IActionResult F_FIRST_SET_NO_YEAR()
        {
            printDocumentService.HtmlTemplateNotificationHave_F_FIRST_SET_NO_YEAR();
            return View("PreviewRaw", null);
        }
        public IActionResult HaveSaveTest(int id)
        {
            printDocumentService.FillHtmlTemplateNotificationHaveSaveTest(id);
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
        private async Task<(byte[] pdfBytes, string FileName)> makePrintAndSavePdf(int id)
        {
            var cdnResult = await service.ReadPrintedFile(id);
            TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplateNotification(id);
            if (cdnResult == null)
            {
                if (htmlModel == null)
                {
                    return (null, "");
                }
                var cdnResultDraft = await service.ReadDraftFile(id);
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
                await service.SavePrintedFile(id, pdfBytes);
                cdnResult = await service.ReadPrintedFile(id);

            }

            var pdfBytesC = Convert.FromBase64String(cdnResult.FileContentBase64);
            if ((Orientation)htmlModel.PageOrientation == Orientation.Landscape)
            {
                pdfBytesC = RotateSecondPage180(pdfBytesC);
            }
            return (pdfBytesC, cdnResult.FileName);
        }

        private async Task<byte[]> ZoomIfHave3Pages(TinyMCEVM htmlModel, byte[] pdfBytes)
        {
            var hmlTemplate = service.GetById<HtmlTemplate>(htmlModel.Id);
            decimal defaultZoom = hmlTemplate?.DefaultZoom ?? 1;
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
            var caseNotification = service.GetById<CaseNotification>(id);
            CheckAccessWithId(caseNotification.Id, caseNotification.CaseId, caseNotification.CaseSessionId, caseNotification.CaseSessionActId, AuditConstants.Operations.Print);
            (var pdfBytesR, var FileName) = await makePrintAndSavePdf(id);
            if (pdfBytesR == null) {
                pdfBytesR = await NoDataPrintPdf();
                FileName = $"{caseNotification.RegNumber}.pdf";
            }
            return File(pdfBytesR, System.Net.Mime.MediaTypeNames.Application.Pdf, FileName);
        }

        [HttpPost]
        public IActionResult ListDataNotificationList(IDataTablesRequest request, int caseSessionId, int NotificationListTypeId)
        {
            var data = service.CaseSessionNotificationList_Select(caseSessionId, NotificationListTypeId);
            return request.GetResponse(data);
        }

        [HttpPost]
        public IActionResult ChangeOrderNotificationList(ChangeOrderModel model)
        {
            var caseSessionNotificationList = service.GetById<CaseSessionNotificationList>(model.Id);
            if (caseSessionNotificationList == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Проблем не намира реда за смяна");
            }
            if (caseSessionNotificationList != null)
            {
                var caseSession = service.GetById<CaseSession>(caseSessionNotificationList.CaseSessionId);
                var caseNotifications = service.CaseNotification_Select(caseSession.CaseId, caseSessionNotificationList.CaseSessionId, null).ToList();
                if (caseNotifications.Any(x => caseSessionNotificationList.NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList ? x.NotificationTypeId == NomenclatureConstants.NotificationType.Subpoena : (caseSessionNotificationList.NotificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationListMessage ? x.NotificationTypeId == NomenclatureConstants.NotificationType.Message : x.NotificationTypeId == NomenclatureConstants.NotificationType.Notification)))
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
        public IActionResult CasePersonNotificationList(int caseId, int caseSessionId, int NotificationListTypeId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionNotificationListPerson, null, AuditConstants.Operations.ChoiceByList, caseSessionId))
            {
                return Redirect_Denied();
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(caseSessionId);
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { @id = caseSessionId, notifListTypeId = NotificationListTypeId });
            SetHelpFile(HelpFileValues.SessionPersonNotification);
            return View("CheckListViewVM", service.Person_SelectForCheck(caseId, caseSessionId, NotificationListTypeId, true));
        }

        [HttpPost]
        public IActionResult CasePersonNotificationList(CheckListViewVM model)
        {
            if (service.CaseNotificationList_Save(model, true))
            {
                CheckAccess(service, SourceTypeSelectVM.CaseSessionNotificationListPerson, null, AuditConstants.Operations.ChoiceByList, model.ObjectId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction("Preview", "CaseSession", new { @id = model.ObjectId, notifListTypeId = model.OtherId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(model.ObjectId);
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { @id = model.ObjectId, notifListTypeId = model.OtherId });
            SetHelpFile(HelpFileValues.SessionPersonNotification);
            return View("CheckListViewVM", model);
        }

        [HttpGet]
        public IActionResult CaseLawUnitNotificationList(int caseId, int caseSessionId, int notificationListTypeId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionNotificationListLawUnit, null, AuditConstants.Operations.ChoiceByList, caseSessionId))
            {
                return Redirect_Denied();
            }
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { @id = caseSessionId, notifListTypeId = notificationListTypeId });
            SetHelpFile(HelpFileValues.SessionPersonNotification);
            return View("CheckListViewVM", service.Person_SelectForCheck(caseId, caseSessionId, notificationListTypeId, false));
        }

        [HttpPost]
        public IActionResult CaseLawUnitNotificationList(CheckListViewVM model)
        {
            if (service.CaseNotificationList_Save(model, false))
            {
                CheckAccess(service, SourceTypeSelectVM.CaseSessionNotificationListLawUnit, null, AuditConstants.Operations.ChoiceByList, model.ObjectId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction("Preview", "CaseSession", new { @id = model.ObjectId, notifListTypeId = model.OtherId });
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { @id = model.ObjectId, notifListTypeId = model.OtherId });
            SetHelpFile(HelpFileValues.SessionPersonNotification);
            return View("CheckListViewVM", model);
        }

        [HttpPost]
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

        public IActionResult EditNotificationList(int id)
        {
            var model = service.GetById<CaseSessionNotificationList>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеното от Вас уведомление не е намерен и/или нямате достъп до него.");
            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionNotificationList, model.Id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            ViewBag.NotificationAddressId_ddl = (model.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)
                                                 ? casePersonService.GetDDL_AddressByCasePersonAddress(model.CasePersonId ?? 0)
                                                 : commonService.LawUnitAddress_SelectDDL_ByCaseLawUnitId(model.CaseLawUnitId ?? 0);
            var caseSession = service.GetById<CaseSession>(model.CaseSessionId);

            SetViewbagCaption(caseSession.CaseId, caseSession.Id, null);
            SetHelpFile(HelpFileValues.SessionPersonNotification);

            return View(nameof(EditNotificationList), model);
        }

        [HttpPost]
        public IActionResult EditNotificationList(CaseSessionNotificationList model)
        {
            var caseSession = service.GetById<CaseSession>(model.CaseSessionId);
            SetViewbagCaption(caseSession.CaseId, caseSession.Id, null);
            SetHelpFile(HelpFileValues.SessionPersonNotification);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditNotificationList), model);
            }

            var currentId = model.Id;
            if (service.CaseNotificationList_SaveData(model))
            {
                CheckAccess(service, SourceTypeSelectVM.CaseSessionNotificationList, model.Id, AuditConstants.Operations.Update);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditNotificationList), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            ViewBag.NotificationAddressId_ddl = (model.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson)
                                             ? casePersonService.GetDDL_AddressByCasePersonAddress(model.CasePersonId ?? 0)
                                             : commonService.LawUnitAddress_SelectDDL_ByCaseLawUnitId(model.CaseLawUnitId ?? 0);
            return View(nameof(EditNotificationList), model);
        }

        public IActionResult AddNotificationFromNotificationList(int caseNotificationListId)
        {
            var sessionNotificationList = service.GetById<CaseSessionNotificationList>(caseNotificationListId);
            var caseSession = service.GetById<CaseSession>(sessionNotificationList.CaseSessionId);

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
                LawUnitAddressId = (sessionNotificationList.NotificationAddressId != null) ? ((sessionNotificationList.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CaseLawUnit) ? sessionNotificationList.NotificationAddressId : null) : null,
                CasePersonAddressId = (sessionNotificationList.NotificationAddressId != null) ? ((sessionNotificationList.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson) ? casePersonService.Get_CasePersonAddress(sessionNotificationList.CasePersonId ?? 0).Where(x => x.AddressId == sessionNotificationList.NotificationAddressId).FirstOrDefault().Id : (int?)null) : null,
                NotificationTypeId = NomenclatureConstants.NotificationType.FromListType(sessionNotificationList.NotificationListTypeId),
                IsOfficialNotification = true
            };

            if (!CheckAccessAdd(model.CaseId, model.CaseSessionId, null, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }

            SetViewbag(model);
            SetViewbagCaption(caseSession.CaseId, caseSession.Id, null);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEdit(model, sessionNotificationList?.NotificationListTypeId ?? 0).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.SessionPersonNotification);

            return View(nameof(Edit), model);
        }

        
        public async Task<IActionResult> PrintPdfsFromDeliveryItemIds([AllowHtml] string deliveryItemIdsJson)
        {
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            int[] deliveryIdArr = JsonConvert.DeserializeObject<int[]>(deliveryItemIdsJson, dateTimeConverter);
            List<int> notificationIds = new List<int>();
            foreach (int itemId in deliveryIdArr)
            {
                var delivery = deliveryItemService.getDeliveryItem(itemId);
                if (delivery.CaseNotificationId == null)
                    continue;
                int id = delivery.CaseNotificationId ?? 0;
                notificationIds.Add(id);
            }
            return await PrintPdfs(notificationIds);
        }
        public async Task<IActionResult> PrintNotificationInList(int? CaseId, int? caseSessionId, int? caseSessionActId, bool isList, int? notificationListTypeId)
        {
            CheckAccessAdd(CaseId ?? 0, caseSessionId, caseSessionActId, AuditConstants.Operations.Print);
            List<int> notificationIds = service.NotificationIdSelect(CaseId, caseSessionId, caseSessionActId, isList, notificationListTypeId);
            return await PrintPdfs(notificationIds);
        }
        public async Task<IActionResult> PrintPdfs(IEnumerable<int> notificationIds)
        {
            MemoryStream memoryStreamUnion = new MemoryStream();
            PdfDocument pdfDoc = new PdfDocument(new PdfWriter(memoryStreamUnion));
            PdfMerger merger = new PdfMerger(pdfDoc);
            bool isWrite = false;

            foreach (var id in notificationIds)
            {
                (var pdfBytesR, var FileName) = await makePrintAndSavePdf(id);
                if (pdfBytesR != null)
                {
                    using (MemoryStream memoryStream = new MemoryStream(pdfBytesR))
                    {
                        using (PdfDocument newDoc = new PdfDocument(new PdfReader(memoryStream)))
                        {
                            var numberOfPages = newDoc.GetNumberOfPages();
                            merger.Merge(newDoc, 1, numberOfPages);
                            newDoc.Close();
                            if (numberOfPages == 1)
                            {
                                await PrintBlankPage(merger, id);
                            }
                        }
                    }
                    isWrite = true;

                    var linkDocuments = await service.GetLinkDocument(id);
                    foreach (var linkDocument in linkDocuments)
                    {
                        using (MemoryStream memoryStreamLink = new MemoryStream(linkDocument))
                        {
                            using (PdfDocument newDocLink = new PdfDocument(new PdfReader(memoryStreamLink)))
                            {
                                var numberOfPages = newDocLink.GetNumberOfPages();
                                merger.Merge(newDocLink, 1, newDocLink.GetNumberOfPages());
                                newDocLink.Close();
                                if (numberOfPages == 1)
                                {
                                    await PrintBlankPage(merger, id);
                                }
                            }
                        }
                    }
                }
            }
            if (isWrite)
            {
                pdfDoc.Close();
                var pdfBytes = memoryStreamUnion.ToArray();
                string FileNameU = DateTime.Now.ToString("yyyyMMdd_hhmm") + ".pdf";
                return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, FileNameU);
            }
            else
            {
                byte[] pdfBytes = await NoDataPrintPdf();
                string FileNameU = DateTime.Now.ToString("yyyyMMdd_hhmm") + ".pdf";
                return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, FileNameU);
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

        private async Task PrintBlankPage(PdfMerger merger, int id)
        {
            var htmlBlank = printDocumentService.GetHtmlTemplateNull(id);
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

        private List<SelectListItem> GetAddrForPerson(List<CaseNotificationLinkVM> linkListVM, int casePersonId, int casePersonLinkId, int notificationDeliveryGroupId)
        {
            List<SelectListItem> addrList;
            if (casePersonLinkId > 0 && linkListVM.Any(x => x.Id == casePersonLinkId))
            {
                int casePersonAddrId = casePersonId;
                var casePersonLink = linkListVM.FirstOrDefault(x => x.Id == casePersonLinkId);
                if (casePersonLink != null)
                {
                    casePersonAddrId = (casePersonLink.PersonSecondRelId ?? 0) != 0 ? (casePersonLink.PersonSecondRelId ?? 0) :
                                       (casePersonLink.isXFirst ? casePersonLink.PersonRelId : casePersonLink.PersonId);
                }
                addrList = casePersonService.GetDDL_CasePersonAddress(casePersonAddrId, notificationDeliveryGroupId);
            }
            else
            {
                addrList = casePersonService.GetDDL_CasePersonAddress(casePersonId, notificationDeliveryGroupId);
            }
            return addrList;
        }
        public JsonResult LoadDropDownListForPerson(int casePersonId, int casePersonLinkId, int notificationTypeId, int notificationDeliveryGroupId)
        {
            var linkListVM = casePersonLinkService.GetLinkForPerson(casePersonId, NomenclatureConstants.FilterPersonOnNotification, notificationTypeId, null);
            List<SelectListItem> addrList = GetAddrForPerson(linkListVM, casePersonId, casePersonLinkId, notificationDeliveryGroupId);
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
                addrList = GetAddrForPerson(linkListVM, casePersonId, casePersonLinkId, notificationDeliveryGroupId);
            }
            else
            {
                addrList = casePersonService.GetDDL_CasePersonAddress(casePersonId, notificationDeliveryGroupId);
            }
            return Json(addrList);
        }

        public JsonResult LoadMLinkForPerson(int caseNotificationId, int casePersonId, int notificationTypeId, int notificationDeliveryGroupId)
        {
            var linkList = service.CasePersonLinksByNotificationId(caseNotificationId, casePersonId, NomenclatureConstants.FilterPersonOnNotification, notificationTypeId).Where(x => x.IsActive).ToList();
            var addrList = casePersonService.GetDDL_CasePersonAddress(casePersonId, notificationDeliveryGroupId);
            return Json(new { linkList, addrList });
        }

        [HttpPost]
        public IActionResult CaseNotification_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseNotification, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var caseNotification = service.GetById<CaseNotification>(model.Id);
            if (!CheckAccessWithId(caseNotification.Id, caseNotification.CaseId, caseNotification.CaseSessionId, caseNotification.CaseSessionActId, AuditConstants.Operations.Delete))
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
            return Json(caseSessionActComplainService.GetDropDownListForAct(caseId, caseSessionActId, htmlTemplateId));
        }

        public JsonResult CaseSessionActComplainMultiDDL(int caseId, int caseNotificationId, int caseSessionActId, int htmlTemplateId)
        {
            var caseNotification = service.ReadById(caseNotificationId);
            service.InitCaseNotificationComplains(caseNotification);
            var result = caseSessionActComplainService.GetDropDownListForAct(caseId, caseSessionActId, htmlTemplateId);
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

        void SetViewbag(CaseNotification model)
        {
            int caseId = model.CaseId;
            int? caseSessionId = model.CaseSessionId;
            int? caseSessionActId = model.CaseSessionActId;
            int notificationStateId = model.NotificationStateId;
            int notificationGroupId = model.NotificationDeliveryGroupId ?? 0;
            int personId = model.CasePersonId ?? 0;
            int personLinkId = model.CasePersonLinkId ?? 0;
            int notificationTypeId = model.NotificationTypeId ?? 0;
            int toCourtId = model.ToCourtId ?? 0;
            int caseLawUnitId = model.CaseLawUnitId ?? 0;

            ViewBag.NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>();
            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(notificationGroupId, notificationStateId);

            var oldLinks = new List<int>() { model.CasePersonLinkId ?? 0 };
            if (model.CaseNotificationMLinks != null)
                oldLinks.AddRange(model.CaseNotificationMLinks.Select(x => x.CasePersonLinkId ?? 0).ToList());
            var linkListVM = casePersonLinkService.GetLinkForPerson(personId, NomenclatureConstants.FilterPersonOnNotification, model.NotificationTypeId ?? 0, oldLinks);
            List<SelectListItem> addrList = GetAddrForPerson(linkListVM, personId, personLinkId, model.NotificationDeliveryGroupId ?? 0);
            ViewBag.CasePersonLinkId_ddl = casePersonLinkService.ListForPersonToDropDown(linkListVM, personId); // casePersonLink.GetDropDownListForPerson(personId);
            ViewBag.CasePersonId_ddl = casePersonService.GetDropDownList(caseId, caseSessionId, true, model.NotificationTypeId, model.CasePersonId, NomenclatureConstants.FilterPersonOnNotification);
            ViewBag.CasePersonAddressId_ddl = addrList;

            //List<SelectListItem> notificationDeliveryGroups = nomService.GetDropDownList<NotificationDeliveryGroup>();
            //ViewBag.NotificationDeliveryGroupId_ddl = notificationDeliveryGroups;
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupDDL(model.NotificationTypeId ?? 0, model.CaseId);

            var NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>().Select(x => new { value = x.Value, text = x.Text });
            ViewBag.NotificationTypeSummonsJson = JsonConvert.SerializeObject(NotificationTypeId_ddl.Where(x => x.value != NomenclatureConstants.NotificationType.GovernmentPaper.ToString()).ToList());
            ViewBag.NotificationTypeGovernmentJson = JsonConvert.SerializeObject(NotificationTypeId_ddl.Where(x => x.value == NomenclatureConstants.NotificationType.GovernmentPaper.ToString()).ToList());

            var HtmlTemplateId_ddl = nomService.GetDDL_HtmlTemplate(notificationTypeId, caseId);
            ViewBag.HtmlTemplateId_ddl = HtmlTemplateId_ddl.Select(x => new SelectListItem() { Value = x.Value, Text = x.Text }).ToList();
            ViewBag.HtmlTemplateId_json = JsonConvert.SerializeObject(HtmlTemplateId_ddl);

            ViewBag.NotificationDeliveryTypeId_ddl = nomService.GetDDL_NotificationDeliveryType(NomenclatureConstants.NotificationDeliveryGroup.WithCourier);
            ViewBag.ToCourtId_ddl = commonService.CourtForDelivery_SelectDDL(-1);
            ViewBag.CourtId = userContext.CourtId;

            ViewBag.CaseLawUnitId_ddl = caseLawUnitService.CaseLawUnit_SelectForDropDownList(caseId, caseSessionId);
            ViewBag.LawUnitAddressId_ddl = commonService.LawUnitAddress_SelectDDL_ByCaseLawUnitId(caseLawUnitId);

            ViewBag.DeliveryOperId_ddl = serviceDeliveryOper.DeliveryOperForNotificationStateSelect(notificationStateId);
            ViewBag.DeliveryReasonId_ddl = nomService.GetDropDownList<DeliveryReason>();

            ViewBag.LawUnitId_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, toCourtId);
            ViewBag.DeliveryAreaId_ddl = areaService.DeliveryAreaSelectDDL(toCourtId, false);
            if (caseSessionId != null)
                ViewBag.CaseSessionActId_ddl = caseSessionActService.GetDropDownList_CaseSessionActEnforced(caseId);/* caseSessionActService.GetDropDownListBySessionId(caseSessionId ?? 0);*/
            if (caseSessionActId != null)
                ViewBag.CaseSessionActComplainId_ddl = caseSessionActComplainService.GetDropDownListForAct(caseId, caseSessionActId ?? 0, model.HtmlTemplateId ?? 0);
            ViewBag.DocumentSenderPersonId_ddl = service.DocumentSenderPersonDDL(model.CaseId);
            ViewBag.InstitutionDocumentId_ddl = service.GetDDL_ConnectedCases(model.CaseId);
            ViewBag.MoneyObligationId_ddl = service.GetMoneyObligationDDL(model.CasePersonId ?? 0, model.CasePersonLinkId ?? 0, model.CaseSessionActId ?? 0);
            ViewBag.NotificationIspnReasonId_ddl = service.GetNotificationIspnReasonDDL();
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


        public JsonResult InsertDeliveryItem(int? courtId)
        {
            var result = service.InsertDeliveryItem(courtId);
            return Json(new { result });
        }

        [HttpPost]
        public JsonResult IsNotificationDeliveryGroupByEpep(int caseId, int casePersonId, string casePersonLinkIds)
        {
            var result = service.IsNotificationDeliveryGroupByEpep(caseId, casePersonId, casePersonLinkIds);

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
    }
}