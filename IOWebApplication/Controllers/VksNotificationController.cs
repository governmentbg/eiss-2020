// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc;
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
    public class VksNotificationController : BaseController
    {
        private readonly ICommonService commonService;
        private readonly INomenclatureService nomService;
        private readonly IVksNotificationService service;
        private readonly ICdnService cdnService;
        public VksNotificationController(ICommonService _commonService, INomenclatureService _nomService, IVksNotificationService _service, ICdnService _cdnService)
        {
            commonService = _commonService;
            nomService = _nomService;
            service = _service;
            cdnService = _cdnService;
        }
        public IActionResult Index()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForVksNotificationList().DeleteOrDisableLast();
            var filter = new VksNotificationPrintFilter();
            return View(filter);
        }
        public JsonResult ListData(VksNotificationPrintFilter filter)
        {
            var data = service.FillVksNotificationPrintList(filter, null);
            var vksMonth = filter.DateFrom.ToString("yyyyMM").ToInt();
            string paperEdition = service.GetPaperEdition(filter);
            if (paperEdition != null)
                paperEdition = "За месеца вече има публикация в ДВ:" + paperEdition;
            return Json(new { data = data.ToList(), vksMonth, paperEdition});
        }
        public async Task<IActionResult> EditTinyMCE([AllowHtml] string caseSessionIdsJson, int vksMonth)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForVksNotificationListEdit().DeleteOrDisableLast();
            var caseSessionIds = new int[0];
            try
            {
                var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
                caseSessionIds = JsonConvert.DeserializeObject<int[]>(caseSessionIdsJson, dateTimeConverter);
            }
            catch
            {

            }
            var htmlModel = new TinyMCEVM();
            htmlModel.ItemsJson = caseSessionIdsJson;

            var filter = new VksNotificationPrintFilter();
            int year = vksMonth / 100;
            int month = vksMonth % 100;
            filter.DateFrom = new DateTime(year, month, 1);
            var data = service.FillVksNotificationPrintList(filter, caseSessionIds);
            data = data.OrderBy(x => x.SessionTimeLabel)
                       .ThenBy(x => x.JudicalCompositionLabel)
                       .ToList();
            var CompositionLabelGr = string.Empty;
            var TimeLabelGr = string.Empty;
            foreach (var item in data)
            {
                if (item.SessionTimeLabel != TimeLabelGr || item.JudicalCompositionLabel != CompositionLabelGr)
                {
                    item.JudicalCompositionLabelGr = item.JudicalCompositionLabel;
                }
                else
                {
                    TimeLabelGr = item.SessionTimeLabel;
                }

                if (item.SessionTimeLabel == TimeLabelGr)
                {
                    item.SessionTimeLabel = String.Empty;
                }
                else
                {
                    TimeLabelGr = item.SessionTimeLabel;
                }
            }
            var vksDate = new DateTime(year, month, 1);
            ViewBag.MonthLabel = $"{vksDate.MonthDiggitName()} {year}";
            string html = await this.RenderPartialViewAsync("~/Views/VksNotification/", "_PrintView.cshtml", data.ToList(), true);
            htmlModel.SourceType = SourceTypeSelectVM.CaseSessionNotificationListNotificationDP;
            htmlModel.SourceId = vksMonth;
            htmlModel.Title = "Списък за призоваване чрез държавен вестник";
            htmlModel.Text = html;
            htmlModel.PageOrientation = 1;
            htmlModel.Style = getVksStyle();
            return View("EditTinyMCE", htmlModel);
            //  return View("PreviewRaw", htmlModel);
        }
        private string getVksStyle()
        {
            var s = @"
div.columns {
    column-width: 90mm;
    -moz-column-width: 90mm;
    -webkit-column-width: 90mm;
}
.vks-container {
    width: 195mm;
    column-count: 2;
    -moz-column-count: 2;
    -webkit-column-count: 2;
} ";
            return @"div.columns { width: 90mm;}";
        }
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> DraftTinyMCE(TinyMCEVM htmlModel)
        {
            htmlModel.Style = getVksStyle();
            // htmlModel.Style += @"#background {
            //    position: absolute;
            //    display: block;
            //    min-width: 100%;
            //    opacity: 0.5;
            //    text-align: center;
            //    background-color: transparent;
            //    padding-top:40%;
            //}
            //#bg-text {
            //    color: lightgrey;
            //    font-size: 120px;
            //    transform: rotate(300deg);
            //    -webkit-transform: rotate(300deg);
            //    opacity: 0.9;
            //    filter: alpha(opacity=50);
            //    background-color: transparent;
            //}";
            // htmlModel.Text = "<div id=\"background\"> <p id = \"bg-text\">Draft Draft Draft</p> </div>" + htmlModel.Text;
            //var pdfBytes = await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlModel)
            //{
            //    PageOrientation = (Orientation)htmlModel.PageOrientation,
            //    PageMargins = new Margins(10, 5, 10, 5),
            //    PageSize = Size.A4,
            //    CustomSwitches = htmlModel.SmartShrinkingPDF ? "" : "--disable-smart-shrinking"
            //}.GetByte(this.ControllerContext);
            // return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "draft.pdf");
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            var docBytes = generateTwoColumnsDocument(Encoding.UTF8.GetBytes(html));
            return File(docBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "draft.doc");
        }
        [DisableAudit]
        [Route("[controller]/VksStyle/style.css")]
        public IActionResult VksStyle()
        {
            var deffStyle = getVksStyle();
            return Content(deffStyle, "text/css");
        }

        [HttpPost]
        public async Task<IActionResult> EditTinyMCESave(TinyMCEVM htmlModel)
        {
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            var headerId = service.SaveSelectedList(htmlModel.ItemsJson, htmlModel.SourceId);
            if (headerId <= 0)
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return View(nameof(EditTinyMCE), htmlModel);
            }
            int y = htmlModel.SourceId / 100;
            int m = htmlModel.SourceId % 100;
            var htmlRequest = new CdnUploadRequest()
            {
                SourceType = htmlModel.SourceType,
                SourceId = headerId.ToString(),
                FileName = $"Призовани_чрез_ДВ_месец_{m}_{y}.doc",
                ContentType = NomenclatureConstants.ContentTypes.Html,
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html ?? ""))
            };

            if (await cdnService.MongoCdn_AppendUpdate(htmlRequest))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return RedirectToAction(nameof(VksNotificationPrint), new { vksMonth = headerId });
        }
        public IActionResult VksNotificationPrint(int vksMonth)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForVksNotificationPrint().DeleteOrDisableLast();
            var model = new VksNotificationPrintVM()
            {
                VksNotificationHeaderId = vksMonth
            };
            ViewBag.VksNotificationHeaderId_ddl = service.GetDDL_NotificationPrintList();
            return View(model);
        }
        public JsonResult GetPaperEdition(int vksNotificationHeaderId)
        {
            var header = service.GetById<VksNotificationHeader>(vksNotificationHeaderId);
            return Json(header?.PaperEdition ?? "");
        }
        [HttpPost]
        public IActionResult VksNotificationPrint(VksNotificationPrintVM model)
        {
            ViewBag.VksNotificationHeaderId_ddl = service.GetDDL_NotificationPrintList();
            if (service.SaveVksNotificationPrint(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(model);
        }
        public async Task<IActionResult> PrintDoc(int vksMonth)
        {
            var cdnResult = await service.ReadPrintedFile(vksMonth);
            var pdfBytes = Convert.FromBase64String(cdnResult.FileContentBase64);
            var docBytes = generateTwoColumnsDocument(pdfBytes);
            return File(docBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, cdnResult.FileName);
        }

        private byte[] generateTwoColumnsDocument(byte[] fileBytes)
        {
            MemoryStream msResult = new MemoryStream();
            using (WordprocessingDocument package = WordprocessingDocument.Create(msResult, WordprocessingDocumentType.Document))
            {
                
                
                // Add a new main document part. 
                package.AddMainDocumentPart();
                

                var content = new Paragraph();
                var paragraphSectionProperties = new ParagraphProperties(new SectionProperties());
                var paragraphColumns = new Columns();
                paragraphColumns.EqualWidth = true;
                paragraphColumns.ColumnCount = 2;

                paragraphSectionProperties.Append(paragraphColumns);
                content.Append(paragraphSectionProperties);


                //var runProp = new RunProperties();

                //var runFont = new RunFonts { Ascii = "Times New Roman" };

                //// 48 half-point font size
                //var size = new FontSize { Val = new StringValue("20") };

                //runProp.Append(runFont);
                //runProp.Append(size);

                String cid = "chunkid";
                MemoryStream ms = new MemoryStream(fileBytes);
                AlternativeFormatImportPart formatImportPart = package.MainDocumentPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.Html, cid);
                formatImportPart.FeedData(ms);
                AltChunk altChunk = new AltChunk();
                altChunk.Id = cid;


                content.Append(new Run(new Text("")));
                content.AppendChild(altChunk);

                
                var body = new Body(content);

                // Create the Document DOM. 
                package.MainDocumentPart.Document = new Document(body);

                // Save changes to the main document part. 
                package.MainDocumentPart.Document.Save();
            }

            return msResult.ToArray();
        }
    }
}
