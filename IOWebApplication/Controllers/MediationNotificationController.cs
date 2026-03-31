// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
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
using System.Text;
using System.Threading.Tasks;


namespace IOWebApplication.Controllers
{
    public class MediationNotificationController : BaseController
    {
        private readonly IMediationNotificationService service;
        private readonly ICasePersonLinkService casePersonLinkService;
        private readonly ICasePersonService casePersonService;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly IDeliveryAreaService areaService;
        private readonly IDeliveryItemService deliveryItemService;
        private readonly ICourtLawUnitService courtLawUnitService;
        private readonly IDeliveryItemOperService serviceDeliveryOper;
        private readonly IPrintDocumentService printDocumentService;
        private readonly ICdnService cdnService;
        public MediationNotificationController(
            IMediationNotificationService _service,
            ICasePersonLinkService _casePersonLinkService,
            ICasePersonService _casePersonService,
            INomenclatureService _nomService,
            ICommonService _commonService,
            IDeliveryAreaService _areaService,
            IDeliveryItemService _deliveryItemService,
            ICourtLawUnitService _courtLawUnitService,
            IDeliveryItemOperService _serviceDeliveryOper,
            IPrintDocumentService _printDocumentService,
            ICdnService _cdnService)
        {
            service = _service;
            casePersonLinkService = _casePersonLinkService;
            casePersonService = _casePersonService;
            nomService = _nomService;
            commonService = _commonService;
            areaService = _areaService;
            deliveryItemService = _deliveryItemService;
            courtLawUnitService = _courtLawUnitService;
            serviceDeliveryOper = _serviceDeliveryOper;
            printDocumentService = _printDocumentService;
            cdnService = _cdnService;
        }
       
        public async Task<IActionResult> Add(int mediationSessionId, int? mediationPersonId, int notificationTypeId)
        {
            // TODO:
            //if (!CheckAccess(service, SourceTypeSelectVM.MediationNotification, null, AuditConstants.Operations.Append, mediationSessionId))
            //{
            //    return Redirect_Denied();
            //}
            var mediationSession = await service.GetByIdAsync<MediationCaseSession>(mediationSessionId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForMediationNotificationEdit(mediationSessionId, 0).DeleteOrDisableLast();
            var casePersonId = 0;
            if (mediationPersonId > 0)
            {
                var mediationPerson = await service.GetByIdAsync<MediationCasePerson>(mediationPersonId);
                casePersonId = mediationPerson.CasePersonId;
            }
            var model = new MediationNotification()
            {
                MediationCaseSessionId = mediationSessionId,
                CaseId = mediationSession.CaseId,
                CasePersonId = casePersonId,
                CourtId = userContext.CourtId,
                NotificationDeliveryGroupId = NomenclatureConstants.NotificationDeliveryGroup.WithSummons,
                NotificationStateId = NomenclatureConstants.NotificationState.Ready,
                RegDate = DateTime.Now,
                NotificationTypeId = NomenclatureConstants.NotificationType.Subpoena,
                IsOfficialNotification = true
            };

            SetViewbag(model);
           //  SetHelpFile(HelpFileValues.SessionNotification);
            return View(nameof(Edit), model);
        }
        public IActionResult Edit(int id)
        {
            var model = service.ReadById(id);
            if (model.DeliveryDateCC == null)
                model.DeliveryDateCC = DateTime.Now;
            if (model == null)
            {
                return NotFoundError("Търсеното от Вас уведомление не е намерено и/или нямате достъп до него.");
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForMediationNotificationEdit(model.MediationCaseSessionId, model.Id).DeleteOrDisableLast();
            // TODO:
            //if (!CheckAccess(service, SourceTypeSelectVM.MediationNotification, null, AuditConstants.Operations.Append, model.MediationCaseSessionId))
            //{
            //    return Redirect_Denied();
            //}
            SetViewbag(model);
           // SetHelpFile(HelpFileValues.SessionNotification);

            return View(nameof(Edit), model);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Edit(MediationNotification model, string mediationPersonLinksJson)
        {
            var logVM = new DeliveryLogVM()
            {
                Action = model.Id > 0 ? "Редакция" : "Добавяне",
                PageLabel = "Призовки/съобщения към медиация",
                PageUrl = "MediationNotification/Edit"
            };
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            List<MediationNotificationMLink> casePersonLinks = JsonConvert.DeserializeObject<List<MediationNotificationMLink>>(mediationPersonLinksJson, dateTimeConverter);

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
                        ModelState.Remove(nameof(model.CasePersonAddressId));
                    }
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
            if (await service.MediationNotification_SaveData(model, casePersonLinks, logVM))
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
        public async Task<IActionResult> ListData(IDataTablesRequest request, int mediationSessionId)
        {
            (var data, var totalCount) = await service.MediationNotification_Select(mediationSessionId, request.Start, request.Length, request.GetSortedColumnsForOrderBy(), request.Search.Value);
            return request.GetResponseServerPaging(data, totalCount);
        }


        void SetViewbag(MediationNotification model)
        {

            ViewBag.NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>();
            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(model.NotificationDeliveryGroupId ?? 0, model.NotificationStateId);
            ViewBag.CasePersonId_ddl = service.GetDDL_PersonList(model.CaseId, model.MediationCaseSessionId, model.NotificationTypeId);
            var linkListVM = casePersonLinkService.GetLinkForPersonMediation(model.CasePersonId, model.MediationCaseSessionId);
            List<SelectListItem> addrList = service.GetAddrForPerson(linkListVM, model.CasePersonId, model.CasePersonLinkId ?? 0, model.NotificationDeliveryGroupId ?? 0);
            ViewBag.CasePersonLinkId_ddl = casePersonLinkService.ListForPersonToDropDown(linkListVM, model.CasePersonId);

            ViewBag.CasePersonAddressId_ddl = addrList;
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupDDL(model.NotificationTypeId ?? 0);

            var NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>().Select(x => new { value = x.Value, text = x.Text });
            ViewBag.NotificationTypeSummonsJson = JsonConvert.SerializeObject(NotificationTypeId_ddl.Where(x => x.value != NomenclatureConstants.NotificationType.GovernmentPaper.ToString()).ToList());
            ViewBag.NotificationTypeGovernmentJson = JsonConvert.SerializeObject(NotificationTypeId_ddl.Where(x => x.value == NomenclatureConstants.NotificationType.GovernmentPaper.ToString()).ToList());

            var HtmlTemplateId_ddl = nomService.GetDDL_HtmlTemplateMediation(model.NotificationTypeId ?? 0);
            ViewBag.HtmlTemplateId_ddl = HtmlTemplateId_ddl.Select(x => new SelectListItem() { Value = x.Value, Text = x.Text }).ToList();
            ViewBag.HtmlTemplateId_json = JsonConvert.SerializeObject(HtmlTemplateId_ddl);

            ViewBag.NotificationDeliveryTypeId_ddl = nomService.GetDDL_NotificationDeliveryType(NomenclatureConstants.NotificationDeliveryGroup.WithCourier);
            ViewBag.ToCourtId_ddl = commonService.CourtForDelivery_SelectDDL(-1);
            ViewBag.CourtId = userContext.CourtId;

            ViewBag.DeliveryOperId_ddl = serviceDeliveryOper.DeliveryOperForNotificationStateSelect(model.NotificationStateId);
            ViewBag.DeliveryReasonId_ddl = nomService.GetDropDownList<DeliveryReason>();

            ViewBag.LawUnitId_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, model.ToCourtId ?? 0);
            ViewBag.DeliveryAreaId_ddl = areaService.DeliveryAreaSelectDDL(model.ToCourtId ?? 0, false);
            // TODO: ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentNotification(model.Id, model.DocumentId ?? 0, model.DocumentResolutionId, model.NotificationTypeId ?? 0).DeleteOrDisableLast();
        }
        
        public JsonResult LoadAddrForPerson(int casePersonId, int mediationSessionId, int casePersonLinkId, int notificationDeliveryGroupId)
        {
            List<SelectListItem> addrList;
            if (casePersonLinkId > 0)
            {
                var linkListVM = casePersonLinkService.GetLinkForPersonMediation(casePersonId, mediationSessionId);
                addrList = service.GetAddrForPerson(linkListVM, casePersonId, casePersonLinkId, notificationDeliveryGroupId);
            }
            else
            {
                addrList = casePersonService.GetDDL_CasePersonAddress(casePersonId, notificationDeliveryGroupId);
            }
            return Json(addrList);
        }
        public JsonResult LoadDropDownListForPerson(int casePersonId, int mediationSessionId, int casePersonLinkId, int notificationTypeId, int notificationDeliveryGroupId)
        {
            var linkListVM = casePersonLinkService.GetLinkForPersonMediation(casePersonId, mediationSessionId);
            List<SelectListItem> addrList = service.GetAddrForPerson(linkListVM, casePersonId, casePersonLinkId, notificationDeliveryGroupId);
            var linkList = casePersonLinkService.ListForPersonToDropDown(linkListVM, casePersonId);
            return Json(new { linkList, addrList });
        }

        public async Task<IActionResult> EditTinyMCE(int sourceId)
        {
            var htmlModel = await printDocumentService.FillHtmlTemplateMediationNotification(sourceId);
            htmlModel.SourceId = sourceId;
            htmlModel.SourceType = SourceTypeSelectVM.MediationNotificationPrint;
            var mediationNotification = service.ReadById(sourceId);

            //if (!CheckAccess(service, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, mediationNotification.MediationCaseSessionId))
            //{
            //    return Redirect_Denied();
            //}
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForMediationNotificationEditTinyMCE(mediationNotification).DeleteOrDisableLast();
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
                var mediationNotification = service.ReadById(htmlModel.SourceId);
                mediationNotification.DatePrint = DateTime.Now;
                await service.MediationNotification_SaveData(mediationNotification, mediationNotification.MediationNotificationMLinks?.ToList(), logVM);
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
        public IActionResult MediationNotification_ExpiredInfo(ExpiredInfoVM model)
        {
            var mediationNotification = service.GetById<MediationNotification>(model.Id);
            //if (!CheckAccess(service, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, mediationNotification.MediationCaseSessionId))
            //{
            //    return Redirect_Denied();
            //}

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
        public JsonResult LoadMLinkForPerson(int mediationNotificationId, int casePersonId, int notificationTypeId, int notificationDeliveryGroupId)
        {
            var linkList = service.MediationPersonLinksByNotificationId(mediationNotificationId, casePersonId, notificationTypeId).Where(x => x.IsActive).ToList();
            var addrList = casePersonService.GetDDL_CasePersonAddress(casePersonId, notificationDeliveryGroupId);
            return Json(new { linkList, addrList });
        }
        public async Task<IActionResult> PrintPdf(int id)
        {
            (var pdfBytesR, var FileName) = await makePrintAndSavePdfMediationNotification(id);
            return File(pdfBytesR, System.Net.Mime.MediaTypeNames.Application.Pdf, FileName);
        }
        private async Task<(byte[] pdfBytes, string FileName)> makePrintAndSavePdfMediationNotification(int id)
        {
            var cdnResult = await service.ReadPrintedFile(id);
            TinyMCEVM htmlModel = await printDocumentService.FillHtmlTemplateMediationNotification(id);

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
    }
}
