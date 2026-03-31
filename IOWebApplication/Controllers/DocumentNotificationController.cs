// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rotativa.AspNetCore.Options;
using Rotativa.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace IOWebApplication.Controllers
{
    public class DocumentNotificationController : BaseController
    {
        private readonly IDocumentNotificationService service;
        private readonly IDocumentPersonLinkService documentPersonLinkService;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly IDocumentResolutionService drService;
        private readonly IDeliveryAreaService areaService;
        private readonly IDeliveryItemService deliveryItemService;
        private readonly ICourtLawUnitService courtLawUnitService;
        private readonly IDeliveryItemOperService serviceDeliveryOper;
        private readonly IPrintDocumentService printDocumentService;
        private readonly ICdnService cdnService;
        public DocumentNotificationController(
            IDocumentNotificationService _service,
            IDocumentPersonLinkService _documentPersonLinkService,
            INomenclatureService _nomService,
            ICommonService _commonService,
            IDocumentResolutionService _drService,
            IDeliveryAreaService _areaService,
            IDeliveryItemService _deliveryItemService,
            ICourtLawUnitService _courtLawUnitService,
            IDeliveryItemOperService _serviceDeliveryOper,
            IPrintDocumentService _printDocumentService,
            ICdnService _cdnService)
        {
            service = _service;
            documentPersonLinkService = _documentPersonLinkService;
            nomService = _nomService;
            commonService = _commonService;
            drService = _drService;
            areaService = _areaService;
            deliveryItemService = _deliveryItemService;
            courtLawUnitService = _courtLawUnitService;
            serviceDeliveryOper = _serviceDeliveryOper;
            printDocumentService = _printDocumentService;
            cdnService = _cdnService;
        }
        public async Task<IActionResult> Index(long documentId, long? documentResolutionId)
        {
            if (!await CheckAccessAsync(drService, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, documentId))
            {
                return Redirect_Denied();
            }
            ViewBag.documentResolutionId = documentResolutionId;
            ViewBag.documentId = documentId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentPersonLink(documentId, documentResolutionId ?? 0).DeleteOrDisableLast();
            // SetHelpFile(HelpFileValues.CasePerson);
            return View();
        }

        public async Task<IActionResult> Add(long documentId, long? documentResolutionId, int notificationTypeId)
        {
            if (!await CheckAccessAsync(drService, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, documentId))
            {
                return Redirect_Denied();
            }
            var model = new DocumentNotification()
            {
                DocumentId = documentId,
                DocumentResolutionId = documentResolutionId,
                CourtId = userContext.CourtId,
                NotificationDeliveryGroupId = NomenclatureConstants.NotificationDeliveryGroup.WithSummons,
                NotificationStateId = NomenclatureConstants.NotificationState.Ready,
                RegDate = DateTime.Now,
                NotificationTypeId = notificationTypeId,
                IsOfficialNotification = true
            };

            SetViewbag(model);
           //  SetHelpFile(HelpFileValues.SessionNotification);
            return View(nameof(Edit), model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var model = service.ReadById(id);
            if (model.DeliveryDateCC == null)
                model.DeliveryDateCC = DateTime.Now;
            if (model == null)
            {
                return NotFoundError("Търсеното от Вас уведомление не е намерено и/или нямате достъп до него.");
            }
            if (!await CheckAccessAsync(drService, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, model.DocumentId))
            {
                return Redirect_Denied();
            }
            SetViewbag(model);
           // SetHelpFile(HelpFileValues.SessionNotification);

            return View(nameof(Edit), model);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Edit(DocumentNotification model, string documentPersonLinksJson)
        {
            var logVM = new DeliveryLogVM()
            {
                Action = model.Id > 0 ? "Редакция" : "Добавяне",
                PageLabel = "Призовки/съобщения към документ",
                PageUrl = "CaseNotification/Edit"
            };
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            List<DocumentNotificationMLink> documentPersonLinks = JsonConvert.DeserializeObject<List<DocumentNotificationMLink>>(documentPersonLinksJson, dateTimeConverter);

            SetViewbag(model);
            var listNotificationTypeId = NomenclatureConstants.NotificationType.ToListType(model.NotificationTypeId);
            //SetHelpFile(HelpFileValues.SessionNotification);

            if (@NomenclatureConstants.NotificationDeliveryGroup.OnMoment(model.NotificationDeliveryGroupId))
            {
                if (model.DatePrint == null && model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnEMail)
                    model.DatePrint = DateTime.Now;

                if (model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnPhone &&
                        model.NotificationDeliveryGroupId != @NomenclatureConstants.NotificationDeliveryGroup.OnEMail)
                {
                    if (!ModelState.IsValid)
                    {
                        ModelState.Remove(nameof(model.DocumentPersonAddressId));
                    }
                    model.DocumentPersonAddressId = null;
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

            if (@NomenclatureConstants.NotificationDeliveryGroup.WithCourierLike(model.NotificationDeliveryGroupId))
            {
                model.DeliveryDate = model.DeliveryDateCC;
                model.DeliveryInfo = model.DeliveryInfoCC;
            }

            // ValidateModel(model, complainIds);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (await service.DocumentNotification_SaveData(model, documentPersonLinks, logVM))
            {
                //if (currentId == 0)
                //    CheckAccessAdd(model.CaseId, model.CaseSessionId, model.CaseSessionActId, AuditConstants.Operations.Append);
                //else
                //    CheckAccessWithId(model.Id, model.CaseId, model.CaseSessionId, model.CaseSessionActId, AuditConstants.Operations.Update);

                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(message: MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, long documentId, long? documentResolutionId)
        {
            var data = service.DocumentNotification_Select(documentId, documentResolutionId);
            return request.GetResponse(data);
        }


        void SetViewbag(DocumentNotification model)
        {

            ViewBag.NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>();
            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(model.NotificationDeliveryGroupId ?? 0, model.NotificationStateId);
            ViewBag.DocumentPersonId_ddl = documentPersonLinkService.GetPersonDropDownList(model.DocumentId ?? 0, model.NotificationTypeId);
            var linkListVM = documentPersonLinkService.GetLinkForPerson(model.DocumentPersonId ?? 0, model.NotificationTypeId ?? 0, null);
            List<SelectListItem> addrList = service.GetAddrForPerson(linkListVM, model.DocumentPersonId ?? 0, model.DocumentPersonLinkId ?? 0, model.NotificationDeliveryGroupId ?? 0);
            ViewBag.DocumentPersonLinkId_ddl = documentPersonLinkService.ListForPersonToDropDown(linkListVM, model.DocumentPersonId ?? 0); // casePersonLink.GetDropDownListForPerson(personId);

            ViewBag.DocumentPersonAddressId_ddl = addrList;
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupDDL(model.NotificationTypeId ?? 0);

            var NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>().Select(x => new { value = x.Value, text = x.Text });
            ViewBag.NotificationTypeSummonsJson = JsonConvert.SerializeObject(NotificationTypeId_ddl.Where(x => x.value != NomenclatureConstants.NotificationType.GovernmentPaper.ToString()).ToList());
            ViewBag.NotificationTypeGovernmentJson = JsonConvert.SerializeObject(NotificationTypeId_ddl.Where(x => x.value == NomenclatureConstants.NotificationType.GovernmentPaper.ToString()).ToList());

            var HtmlTemplateId_ddl = nomService.GetDDL_HtmlTemplateDocument(model.NotificationTypeId ?? 0);
            ViewBag.HtmlTemplateId_ddl = HtmlTemplateId_ddl.Select(x => new SelectListItem() { Value = x.Value, Text = x.Text }).ToList();
            ViewBag.HtmlTemplateId_json = JsonConvert.SerializeObject(HtmlTemplateId_ddl);

            ViewBag.NotificationDeliveryTypeId_ddl = nomService.GetDDL_NotificationDeliveryType(NomenclatureConstants.NotificationDeliveryGroup.WithCourier);
            ViewBag.ToCourtId_ddl = commonService.CourtForDelivery_SelectDDL(-1);
            ViewBag.CourtId = userContext.CourtId;

            ViewBag.DeliveryOperId_ddl = serviceDeliveryOper.DeliveryOperForNotificationStateSelect(model.NotificationStateId);
            ViewBag.DeliveryReasonId_ddl = nomService.GetDropDownList<DeliveryReason>();

            ViewBag.LawUnitId_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, model.ToCourtId ?? 0);
            ViewBag.DeliveryAreaId_ddl = areaService.DeliveryAreaSelectDDL(model.ToCourtId ?? 0, false);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentNotification(model.Id, model.DocumentId ?? 0, model.DocumentResolutionId, model.NotificationTypeId ?? 0).DeleteOrDisableLast();
        }
        
        public JsonResult LoadAddrForPerson(long documentPersonId, int documentPersonLinkId, int notificationDeliveryGroupId)
        {
            List<SelectListItem> addrList;
            if (documentPersonLinkId > 0)
            {
                var linkListVM = documentPersonLinkService.GetLinkForPerson(documentPersonId, 0, null);
                addrList = service.GetAddrForPerson(linkListVM, documentPersonId, documentPersonLinkId, notificationDeliveryGroupId);
            }
            else
            {
                addrList = documentPersonLinkService.GetDDL_DocumentPersonAddress(documentPersonId, notificationDeliveryGroupId);
            }
            return Json(addrList);
        }
        public JsonResult LoadDropDownListForPerson(long documentPersonId, int documentPersonLinkId, int notificationTypeId, int notificationDeliveryGroupId)
        {
            var linkListVM = documentPersonLinkService.GetLinkForPerson(documentPersonId, notificationTypeId, null);
            List<SelectListItem> addrList = service.GetAddrForPerson(linkListVM, documentPersonId, documentPersonLinkId, notificationDeliveryGroupId);
            var linkList = documentPersonLinkService.ListForPersonToDropDown(linkListVM, documentPersonId);
            return Json(new { linkList, addrList });
        }

        public async Task<IActionResult> EditTinyMCE(int sourceId)
        {
            var htmlModel = printDocumentService.FillHtmlTemplateDocumentNotification(sourceId);
            htmlModel.SourceId = sourceId;
            htmlModel.SourceType = SourceTypeSelectVM.DocumentNotificationPrint;
            var documentNotification = service.ReadById(sourceId);

            if (!await CheckAccessAsync(drService, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, documentNotification.DocumentId))
            {
                return Redirect_Denied();
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDocumentNotificationEditTinyMCE(documentNotification).DeleteOrDisableLast();

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
            var logVM = new DeliveryLogVM()
            {
                Action = "Генериране",
                PageLabel = "Печат на призовка/съобщение към документ",
                PageUrl = "EditTinyMCE"
            };
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);

            var htmlRequest = new CdnUploadRequest()
            {
                SourceType = htmlModel.SourceType,
                SourceId = htmlModel.SourceId.ToString(),
                FileName = "draft.html",
                ContentType = NomenclatureConstants.ContentTypes.Html,
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html ?? ""))
            };
            if (await cdnService.MongoCdn_AppendUpdate(htmlRequest))
            {
                var documentNotification = service.ReadById(htmlModel.SourceId);
                documentNotification.DatePrint = DateTime.Now;
                await service.DocumentNotification_SaveData(documentNotification, documentNotification.DocumentNotificationMLinks?.ToList(), logVM);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return RedirectToAction(nameof(Edit), new { id = htmlModel.SourceId });
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
        private byte[] RotateSecondPage180(byte[] pdfBytes)
        {
            // return pdfBytes;
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
        
        [HttpPost]
        public async Task<IActionResult> DocumentNotification_ExpiredInfo(ExpiredInfoVM model)
        {
            var documentNotification = await service.GetByIdAsync<DocumentNotification>(model.Id);
            if (!await CheckAccessAsync(drService, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, documentNotification.DocumentId ??0))
            {
                return Redirect_Denied();
            }

            if (service.SaveExpireInfoPlus(model))
            {
                //SetAuditContextDelete(service, SourceTypeSelectVM.CaseNotification, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseNotificationExpireOK);
                return Json(new { result = true, redirectUrl = model.ReturnUrl });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }
        public JsonResult LoadMLinkForPerson(int documentNotificationId, int documentPersonId, int notificationTypeId, int notificationDeliveryGroupId)
        {
            var linkList = service.DocumentPersonLinksByNotificationId(documentNotificationId, documentPersonId, notificationTypeId).Where(x => x.IsActive).ToList();
            var addrList = documentPersonLinkService.GetDDL_DocumentPersonAddress(documentPersonId, notificationDeliveryGroupId);
            return Json(new { linkList, addrList });
        }
        [HttpGet]
        public IActionResult NotificationGroup(long? documentId, long? documentResolutionId, int notificationTypeId)
        {
            SetViewBagNotificationGroup(documentId, documentResolutionId, notificationTypeId);
            var model = service.GenerateNotificationGroup(documentId, documentResolutionId, notificationTypeId);
            return View(model);
        }
        void SetViewBagNotificationGroup(long? documentId, long? documentResolutionId, int notificationTypeId)
        {
            ViewBag.NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>().Where(x => x.Value != NomenclatureConstants.NotificationType.GovernmentPaper.ToString()).ToList();
            var HtmlTemplateId_ddl = nomService.GetDDL_HtmlTemplateDocument(notificationTypeId);
            ViewBag.HtmlTemplateId_ddl = HtmlTemplateId_ddl.Select(x => new SelectListItem() { Value = x.Value, Text = x.Text }).ToList();
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupDDL(notificationTypeId);
            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(NomenclatureConstants.NotificationDeliveryGroup.WithSummons, NomenclatureConstants.NotificationState.Ready);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentNotification(0, documentId ?? 0, documentResolutionId, notificationTypeId).DeleteOrDisableLast();
        }
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> NotificationGroup(NotificationDocGroupVM model)
        {
            var logVM = new DeliveryLogVM()
            {
                Action = "Множествено добавяне",
                PageLabel = "Призовки/съобщения",
                PageUrl = "DocumentNotification/Edit"
            };
       
            SetViewBagNotificationGroup(model.DocumentResolutionId, model.DocumentResolutionId, model.NotificationStateId);
            SetHelpFile(HelpFileValues.SessionNotification);
            if (!ModelState.IsValid)
            {
                return View(nameof(NotificationGroup), model);
            }
            try
            {
                await service.SaveMultiNotification(model, logVM);
                return RedirectToAction("Edit", "DocumentResolution", new { id = model.DocumentResolutionId});
            }
            catch (Exception ex)
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(NotificationGroup), model);
        }
    }
}
