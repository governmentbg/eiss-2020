using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Rotativa.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System.Linq;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Rotativa.AspNetCore.Options;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;

namespace IOWebApplication.Controllers
{
    public class DocumentTemplateController : BaseController
    {
        private readonly IDocumentTemplateService service;
        private readonly IDocumentService docService;
        private readonly INomenclatureService nomService;
        private readonly ICdnService cdnService;
        private readonly IWorkTaskService taskService;
        private readonly ICommonService commonService;
        private readonly IPrintDocumentService printService;
        private readonly ICasePersonService casePersonService;

        public DocumentTemplateController(
            IDocumentTemplateService _service,
            IDocumentService _docService,
            INomenclatureService _nomService,
            ICdnService _cdnService,
            IWorkTaskService _taskService,
            ICommonService _commonService,
            IPrintDocumentService _printService,
            ICasePersonService _casePersonService
        )
        {
            service = _service;
            docService = _docService;
            nomService = _nomService;
            cdnService = _cdnService;
            taskService = _taskService;
            commonService = _commonService;
            printService = _printService;
            casePersonService = _casePersonService;
        }

        /// <summary>
        /// Страница с документ към обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult Index(int sourceType, long sourceId)
        {
            if (!CheckAccess(service, sourceType, sourceId, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }

            ViewBag.sourceType = sourceType;
            ViewBag.sourceId = sourceId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentTemplate(sourceType, sourceId);
            SetHelpFileBySourceType(sourceType);

            return View();
        }

        /// <summary>
        /// Извличане на данните за документ към обект
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int sourceType, long sourceId)
        {
            var data = service.DocumentTemplate_Select(sourceType, sourceId);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне/Редакция на документ към обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult AppendUpdateSingle(int sourceType, long sourceId)
        {
            var singleDoc = service.DocumentTemplate_Select(sourceType, sourceId).FirstOrDefault();
            if (singleDoc != null)
            {
                return RedirectToAction(nameof(Edit), new { id = singleDoc.Id });
            }
            else
            {
                return RedirectToAction(nameof(Add), new { sourceType = sourceType, sourceId = sourceId });
            }
        }

        /// <summary>
        /// Добавяне на документ към обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult Add(int sourceType, long sourceId)
        {
            var model = service.DocumentTemplate_Init(sourceType, sourceId);
            if (!CheckAccess(service, sourceType, sourceId, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }

            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на документ към обект
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<DocumentTemplate>(id);
            if (!CheckAccess(service, model.SourceType, model.SourceId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на документ към обект
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(DocumentTemplate model)
        {
            if (!ModelState.IsValid)
            {
                SetViewBag(model);
                return View(nameof(Edit), model);
            }
            int currentId = model.Id;
            if (service.DocumentTemplate_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id, null, "edit");
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        private void SetViewBag(DocumentTemplate model)
        {
            ViewBag.DocumentKindId_ddl = nomService.GetDDL_DocumentKind(DocumentConstants.DocumentDirection.OutGoing);
            ViewBag.DocumentTemplateStateId_ddl = nomService.GetDropDownList<DocumentTemplateState>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentTemplate(model.SourceType, model.SourceId);
            if (model.DocumentId > 0)
            {
                ViewBag.docNumber = docService.Document_GetById(model.DocumentId.Value).Result.RegNumber;
            }

            ViewBag.haveFromToDateIds = nomService.GetHtmlTemplateForFromToDate();

            if ((model.CaseId ?? 0) > 0)
            {
                ViewBag.haveCasePersonIds = nomService.GetHtmlTemplateForCasePerson();
                ViewBag.CasePersonId_ddl = casePersonService.CasePerson_SelectForDropDownList(model.CaseId ?? 0, null, "", "Изберете");
            }

            SetHelpFileBySourceType(model.SourceType);
        }

        private void SetHelpFileBySourceType(int sourceType)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    SetHelpFile(HelpFileValues.SessionAct);
                    break;
                case SourceTypeSelectVM.Case:
                    SetHelpFile(HelpFileValues.CaseDocumentTemplate);
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    SetHelpFile(HelpFileValues.SessionNotification);
                    break;
                case SourceTypeSelectVM.CaseMigration:
                    SetHelpFile(HelpFileValues.CaseMigration);
                    break;
                case SourceTypeSelectVM.DocumentDecision:
                    SetHelpFile(HelpFileValues.DecisionTask);
                    break;
                case SourceTypeSelectVM.ExecList:
                    SetHelpFile(HelpFileValues.Finance);
                    break;
                case SourceTypeSelectVM.ExchangeDoc:
                    SetHelpFile(HelpFileValues.Finance);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Зареждане на данни за запис на файл по документ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Blank(int id, bool del = false)
        {
            var docTemplate = service.GetById<DocumentTemplate>(id);
            BlankEditVM blankModel = null;
            if (docTemplate.HtmlTemplateId > 0)
            {
                blankModel = await InitBlankFromTemplate(id, del);
            }
            else
            {
                //var actModel = service.CaseSessionAct_GetForBlankPrepare(id);
                int sourceType = SourceTypeSelectVM.DocumentTemplate;
                string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
                if (string.IsNullOrEmpty(html))
                {
                    var tmplt = service.GetById<DocumentTemplate>(id);
                    html = tmplt.Description;
                }
                var headerModel = service.DocumentTemplate_InitHeader(id);
                var headerTemplate = "Header";
                var headerHTML = string.Empty;
                if (!string.IsNullOrEmpty(headerModel.HeaderTemplateName))
                {
                    headerTemplate = headerModel.HeaderTemplateName;
                }
                headerHTML = await this.RenderViewAsync(headerTemplate, headerModel);
                var footerHTML = await this.RenderViewAsync("Footer", headerModel);

                blankModel = new BlankEditVM()
                {
                    Title = "Изготвяне на документ към " + SourceTypeSelectVM.GetSourceTypeName(sourceType).ToLower(),
                    SourceType = sourceType,
                    SourceId = id.ToString(),
                    SessionName = userContext.GenHash(id, sourceType),
                    HtmlHeader = headerHTML,
                    HtmlContent = html,
                    HtmlFooter = footerHTML,
                    FooterIsEditable = false,
                    ReturnUrl = Url.Action(nameof(Edit), new { id }),
                    HasPreviewButton = true,
                };
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentTemplate(docTemplate.SourceType, docTemplate.SourceId);
            return View("BlankEdit", blankModel);
        }



        private async Task<BlankEditVM> InitBlankFromTemplate(int id, bool del)
        {
            var docTemplate = service.GetById<DocumentTemplate>(id);
            int sourceType = SourceTypeSelectVM.DocumentTemplate;
            if (del)
            {
                var deleted = await cdnService.MongoCdn_DeleteFiles(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            }
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            if (string.IsNullOrEmpty(html))
            {
                TinyMCEVM htmlModel = printService.FillHtmlTemplateDocumentTemplate(id);
                html = htmlModel.Text;
            }

            var model = new BlankEditVM()
            {
                Title = "Изготвяне на документ към " + SourceTypeSelectVM.GetSourceTypeName(sourceType).ToLower(),
                SourceType = sourceType,
                SourceId = id.ToString(),
                SessionName = userContext.GenHash(id, sourceType),
                HtmlHeader = string.Empty,
                HtmlContent = html,
                HtmlFooter = string.Empty,
                FooterIsEditable = false,
                ReturnUrl = Url.Action(nameof(Edit), new { id }),
                HasPreviewButton = true,
                HasResetButton = true,
            };
            if (docTemplate.HtmlTemplateId > 0)
            {
                var htmlTemplate = service.GetById<HtmlTemplate>(docTemplate.HtmlTemplateId);
                var htmlModel = printService.ConvertToTinyMCVM(htmlTemplate, false);
                model.TemplateStyle = htmlModel.Style;
            }
            return model;
        }

        /// <summary>
        /// Преглед в pdf
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<IActionResult> blankPreview(BlankEditVM model)
        {
            int id = 0;
            int.TryParse(model.SourceId, out id);
            var docTemplate = service.GetById<DocumentTemplate>(id);

            byte[] pdfBytes = null;
            if (docTemplate.HtmlTemplateId > 0)
            {
                string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = model.SourceType, SourceId = model.SourceId });

                pdfBytes = await PdfBytesFromTemplate(id, html);
                //pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true, GetFooterInfoUrl(userContext.CourtId)).GetByte(this.ControllerContext);
            }
            else
            {
                var headerModel = service.DocumentTemplate_InitHeader(id);

                pdfBytes = await GetFileBytes(headerModel, model.SourceType, model.SourceId);
            }
            var contentDispositionHeader = new System.Net.Mime.ContentDisposition
            {
                Inline = true,
                FileName = "documentTemplatePreview.pdf"
            };
            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf);
        }

        /// <summary>
        /// Запис на файл
        /// </summary>
        /// <param name="model"></param>
        /// <param name="btnPreview"></param>
        /// <param name="reset_mode"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Blank(BlankEditVM model, string btnPreview = null, string reset_mode = null)
        {
            if (!userContext.CheckHash(model))
            {
                return Redirect_Denied();
            }
            int id = int.Parse(model.SourceId);
            if (!string.IsNullOrEmpty(reset_mode))
            {
                await cdnService.MongoCdn_DeleteFiles(new CdnFileSelect() { SourceType = model.SourceType, SourceId = model.SourceId });
                SetSuccessMessage("Информацията е обновена с данните от бланката.");
                this.SaveLogOperation(this.ControllerName, nameof(Edit), "Обновяване на данните от бланката", IO.LogOperation.Models.OperationTypes.Patch, id);
                return RedirectToAction(nameof(Blank), new { id = model.SourceId });
            }

            //return new ViewAsPdf("CreatePdf", model);
            var htmlRequest = new CdnUploadRequest()
            {
                SourceType = model.SourceType,
                SourceId = model.SourceId,
                FileName = "draft.html",
                ContentType = "text/html",
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.HtmlContent ?? ""))
            };
            if (await cdnService.MongoCdn_AppendUpdate(htmlRequest))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                this.SaveLogOperation(this.ControllerName, nameof(Edit), "Изготвяне на документ", IO.LogOperation.Models.OperationTypes.Update, id);
                if (!string.IsNullOrEmpty(btnPreview))
                {
                    return await blankPreview(model);
                }
                return RedirectToAction(nameof(Edit), new { id = model.SourceId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }



            return RedirectToAction(nameof(Blank), new { id = model.SourceId });
        }

        private async Task<byte[]> GetFileBytes(DocumentTemplateHeaderVM headerModel, int sourceType, string sourceId)
        {
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = sourceId.ToString() });

            var headerTemplate = "Header";
            var headerHTML = string.Empty;
            if (!string.IsNullOrEmpty(headerModel.HeaderTemplateName))
            {
                headerTemplate = headerModel.HeaderTemplateName;
            }
            headerHTML = await this.RenderViewAsync(headerTemplate, headerModel);
            var footerHTML = await this.RenderViewAsync("Footer", headerModel);
            return await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlHeader = headerHTML, HtmlContent = html, HtmlFooter = footerHTML }, true, GetFooterInfoUrl(userContext.CourtId)).GetByte(this.ControllerContext);
        }

        /// <summary>
        /// Запис на файл за документ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GenerateDocumentFile(int id, long documentId)
        {
            if (!service.DocumentTemplate_UpdateDocumentId(id, documentId))
            {
                SetErrorMessage("Проблем при изчитане на създаден документ");
                return RedirectToAction("Edit", "Document", new { id = documentId });
            }
            var docTemplate = service.GetById<DocumentTemplate>(id);
            var headerModel = service.DocumentTemplate_InitHeader(id);

            byte[] fileBytes = null;

            if (docTemplate.HtmlTemplateId > 0)
            {
                string preparedBlank = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.DocumentTemplate, SourceId = id.ToString() });
                fileBytes = await PdfBytesFromTemplate(docTemplate.Id, preparedBlank);
                //string html = FillBlankByTemplate(docTemplate.Id, preparedBlank);
                ////string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
                //fileBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }).GetByte(this.ControllerContext);
            }
            else
            {
                int sourceType = SourceTypeSelectVM.DocumentTemplate;

                fileBytes = await GetFileBytes(headerModel, sourceType, id.ToString());
            }

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.DocumentPdf,
                SourceId = documentId.ToString(),
                FileName = "documentTemplate.pdf",
                ContentType = "application/pdf",
                Title = $"{headerModel.DocumentTypeLabel} {headerModel.DocumentNumber}/{headerModel.DocumentDate:dd.MM.yyyy}",
                FileContentBase64 = Convert.ToBase64String(fileBytes)
            };

            await cdnService.MongoCdn_AppendUpdate(pdfRequest);

            var newTask = new WorkTaskEditVM()
            {
                SourceType = SourceTypeSelectVM.Document,
                SourceId = documentId,
                TaskTypeId = WorkTaskConstants.Types.Document_Sign,
                UserId = headerModel.AuthorId,
                TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser
            };
            taskService.CreateTask(newTask);

            if (docTemplate.SourceType == SourceTypeSelectVM.ExchangeDoc)
            {
                await SaveFileExchangeDoc((int)docTemplate.SourceId);
            }
            SetSuccessMessage("Регистрирането на създадения документ премина успешно.");
            return RedirectToAction("Edit", "Document", new { id = documentId });
        }

        /// <summary>
        /// Зареждане на бланка по документ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="preparedBlank"></param>
        /// <returns></returns>
        private async Task<string> FillBlankByTemplate(int id, string preparedBlank = null)
        {
            string result = "";

            TinyMCEVM htmlModel = printService.FillHtmlTemplateDocumentTemplate(id, preparedBlank);
            if (htmlModel == null)
            {
                return result;
            }
            return await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
        }

        private async Task<byte[]> PdfBytesFromTemplate(int id, string preparedBlank = null)
        {
            TinyMCEVM htmlModel = printService.FillHtmlTemplateDocumentTemplate(id, preparedBlank);
            if (htmlModel == null)
            {
                return null;
            }

            return await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlModel, true, GetFooterInfoUrl(userContext.CourtId))
            {
                PageOrientation = (Orientation)htmlModel.PageOrientation,
                PageMargins = new Margins(20, 20, 10, 30),
                //PageMargins = new Margins(10, 5, 10, 5),
                PageSize = Size.A4,
                CustomSwitches = htmlModel.SmartShrinkingPDF ? "" : "--disable-smart-shrinking"
            }.GetByte(this.ControllerContext);
        }

        /// <summary>
        /// Запис на файл за бланка
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> PrintBlankByTemplate(int id)
        {
            TinyMCEVM htmlModel = printService.FillHtmlTemplateDocumentTemplate(id);
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
            return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "Template" + id.ToString() + ".pdf");
        }

        /// <summary>
        /// Запис на файл за Протокол на изпращане на ИЛ в НАП
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SaveFileExchangeDoc(int id)
        {
            TinyMCEVM htmlModel = printService.FillHtmlTemplateExchangeDoc(id);
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.ExchangeDoc,
                SourceId = id.ToString(),
                FileName = "exchangeDoc.pdf",
                ContentType = "application/pdf",
                Title = "Протокол на изпращане на ИЛ в НАП",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            bool result = await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            Response.Headers.Clear();

            return result;
        }
        public string GetFooterInfoText(int courtId)
        {
            var court = service.GetById<Infrastructure.Data.Models.Common.Court>(courtId);

            if (court != null)
            {
                return $"{court.Address}, {court.CityName}";
            }
            return "";
        }

        /// <summary>
        /// Анулиране на документ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DocumentTemplate_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.DocumentTemplate, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }

            if (string.IsNullOrEmpty(model.DescriptionExpired))
            {
                return Json(new { result = false, message = MessageConstant.Values.DescriptionExpireRequired });
            }

            (bool result, string errorMessage) = service.DocumentTemplate_SaveExpired(model);
            if (result)
            {
                var documentTemplate = service.GetById<DocumentTemplate>(model.Id);
                SetAuditContextDelete(docService, SourceTypeSelectVM.DocumentTemplate, model.Id);
                SetSuccessMessage(MessageConstant.Values.DocumentTemplateExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("SourceTypeAction", new { sourceType = documentTemplate.SourceType, sourceId = documentTemplate.SourceId }) });
            }
            else
            {
                return Json(new { result = false, message = errorMessage });
            }
        }
    }
}