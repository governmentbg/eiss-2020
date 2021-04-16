using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc;
using Rotativa.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class DocumentResolutionController : BaseController
    {
        private readonly IDocumentResolutionService drService;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICdnService cdnService;
        private readonly IWorkTaskService taskService;

        public DocumentResolutionController(
            IDocumentResolutionService _drService,
            INomenclatureService _nomService,
            ICommonService _commonService,
            ICdnService _cdnService,
            IWorkTaskService _taskService)
        {
            drService = _drService;
            nomService = _nomService;
            commonService = _commonService;
            cdnService = _cdnService;
            taskService = _taskService;
        }
        public IActionResult Index()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentResolution(0);
            SetHelpFile(HelpFileValues.RegisteredDocumentsDisposition);
            return View();
        }


        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, DocumentResolutionFilterVM filter)
        {
            var data = drService.Select(filter);
            return request.GetResponse(data);
        }

        public IActionResult ResolutionsByDocument(long documentId)
        {
            ViewBag.documentId = documentId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_Document(documentId);
            return View();
        }


        [HttpPost]
        public IActionResult ResolutionsByDocument_ListData(IDataTablesRequest request, long documentId)
        {
            var data = drService.Select(documentId);
            return request.GetResponse(data);
        }

        public IActionResult Add(long documentId)
        {
            if (!CheckAccess(drService, SourceTypeSelectVM.DocumentDecision, null, AuditConstants.Operations.Append, documentId))
            {
                return Redirect_Denied();
            }
            var model = new DocumentResolution()
            {
                CourtId = userContext.CourtId,
                DocumentId = documentId,
                UserDecisionId = userContext.UserId
            };
            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        public IActionResult Edit(long id)
        {
            if (!CheckAccess(drService, SourceTypeSelectVM.DocumentDecision, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = drService.GetById<DocumentResolution>(id);
            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult Edit(DocumentResolution model)
        {
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                SetViewBag(model);
                return View(nameof(Edit), model);
            }
            long currentId = model.Id;
            var saveResult = drService.SaveData(model);
            if (saveResult.Result)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetSuccessMessage(saveResult.ErrorMessage);
                SetViewBag(model);
                return View(nameof(Edit), model);
            }
        }

        private void ValidateModel(DocumentResolution model)
        {
            if (model.ResolutionTypeId <= 0)
            {
                ModelState.AddModelError(nameof(DocumentResolution.ResolutionTypeId), "Изберете 'Вид разпореждане'");
            }
            if (model.JudgeDecisionLawunitId <= 0)
            {
                ModelState.AddModelError(nameof(DocumentResolution.JudgeDecisionLawunitId), "Изберете 'Съдия'");
            }
            if (string.IsNullOrEmpty(model.UserDecisionId) || model.UserDecisionId == "0")
            {
                ModelState.AddModelError(nameof(DocumentResolution.JudgeDecisionLawunitId), "Изберете 'Изготвил'");
            }
        }

        private void SetViewBag(DocumentResolution model)
        {
            ViewBag.ResolutionTypeId_ddl = nomService.GetDropDownList<ResolutionType>().SingleOrChoose();
            if (model.Id > 0)
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentResolution(model.Id);
                ViewBag.docInfo = commonService.Breadcrumbs_Document(model.DocumentId).LastOrDefault()?.Title;
            }
            else
            {
                var bc = commonService.Breadcrumbs_Document(model.DocumentId);
                ViewBag.breadcrumbs = bc;
                ViewBag.docInfo = bc.LastOrDefault()?.Title;
            }

            SetHelpFile(HelpFileValues.DispositionTask);
        }

        public async Task<IActionResult> Blank(long id)
        {
            var actModel = drService.Select(0, id).FirstOrDefault();

            int sourceType = SourceTypeSelectVM.DocumentResolutionBlank;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });


            var model = new BlankEditVM()
            {
                Title = "Изготвяне на разпореждане",
                SourceType = sourceType,
                SourceId = id.ToString(),
                HtmlHeader = await this.RenderViewAsync("ActHeader", actModel),
                HtmlContent = html,
                FooterIsEditable = false,
                ReturnUrl = Url.Action(nameof(Edit), new { id }),
                HasPreviewButton = true
            };

            ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentResolution(id);
            SetHelpFile(HelpFileValues.DispositionTask);

            return View("BlankEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> Blank(BlankEditVM model, string btnPreview = null)
        {
            var htmlRequest = new CdnUploadRequest()
            {
                SourceType = model.SourceType,
                SourceId = model.SourceId,
                FileName = "draft.html",
                ContentType = NomenclatureConstants.ContentTypes.Html,
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.HtmlContent))
            };
            if (await cdnService.MongoCdn_AppendUpdate(htmlRequest))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                if (!string.IsNullOrEmpty(btnPreview))
                {
                    return await blankPreview(model);
                }
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return RedirectToAction(nameof(Blank), new { id = model.SourceId });
            }

            return RedirectToAction(nameof(Edit), new { id = model.SourceId });
        }

        private async Task<IActionResult> blankPreview(BlankEditVM model)
        {
            long resolutionId = long.Parse(model.SourceId);
            var actModel = drService.Select(0, resolutionId).FirstOrDefault();

            string html = await GetActHTML(actModel);

            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);

            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf, "documentResolution.pdf");
        }

        private async Task<string> GetActHTML(DocumentResolutionVM actModel)
        {
            actModel.Content = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.DocumentResolutionBlank, SourceId = actModel.Id.ToString() });
            if (string.IsNullOrEmpty(actModel.Content))
            {
                return null;
            }
            return await this.RenderPartialViewAsync("~/Views/DocumentResolution/", "ActFormat.cshtml", actModel, true);
        }

        private async Task PrepareActFile(DocumentResolutionVM actModel, string html)
        {
            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.DocumentResolutionPdf,
                SourceId = actModel.Id.ToString(),
                FileName = "documentResolution.pdf",
                ContentType = NomenclatureConstants.ContentTypes.Pdf,
                Title = actModel.GetFileTitle,
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };

            await cdnService.MongoCdn_AppendUpdate(pdfRequest);
        }

        public async Task<IActionResult> DoTask_SentForSign(long id)
        {
            var task = taskService.Select_ById(id);
            switch (task.SourceType)
            {
                case SourceTypeSelectVM.DocumentResolution:
                    var actId = task.SourceId;
                    var actModel = drService.Select(0, actId).FirstOrDefault();

                    string actHTML = await GetActHTML(actModel);
                    if (string.IsNullOrEmpty(actHTML))
                    {
                        SetErrorMessage("Няма изготвен акт.");
                        return RedirectToAction("Edit", new { id = actId });
                    }

                    await PrepareActFile(actModel, actHTML);

                    var newTask = new WorkTaskEditVM()
                    {
                        ParentTaskId = id,
                        SourceType = SourceTypeSelectVM.DocumentResolution,
                        SourceId = actId,
                        TaskTypeId = WorkTaskConstants.Types.DocumentResolution_Sign,
                        TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                        UserId = actModel.JudgeUserId,
                    };

                    if (taskService.CreateTask(newTask))
                    {
                        SetSuccessMessage("Задачата за подпис е създадена успешно.");
                        taskService.CompleteTask(id);
                    }
                    else
                    {
                        SetErrorMessage("Проблем при създаване на задача");
                    }

                    return RedirectToAction("Edit", new { id = actId });
                default:
                    return null;
            }
        }

        public async Task<IActionResult> SendForSign(long id, long taskId)
        {
            Uri urlSuccess = new Uri(Url.Action(nameof(SignedOk), new { taskId }), UriKind.Relative);
            Uri url = new Uri(Url.Action("Edit", new { id = id }), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.DocumentResolutionPdf,
                DestinationType = SourceTypeSelectVM.DocumentResolutionPdf,
                Location = userContext.CourtName,
                Reason = "Подписване на разпореждане",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url
            };

            var docResolution = drService.GetById<DocumentResolution>(id);

            var registerResult = drService.Register(docResolution);
            if (!registerResult.Result)
            {
                SetErrorMessage(registerResult.ErrorMessage);
                return RedirectToAction("Edit", new { id = id });
            }
            else
            {
                if (registerResult.SaveMethod == "register")
                {
                    var actModel = drService.Select(0, id).FirstOrDefault();

                    string actHTML = await GetActHTML(actModel);
                    await PrepareActFile(actModel, actHTML);
                    return RedirectToAction(nameof(SendForSign), new { id, taskId });
                }
            }

            var lu = taskService.GetLawUnitByTaskId(taskId);
            if (lu != null)
            {
                model.SignerName = lu.FullName;
                model.SignerUic = lu.Uic;
            }

            return View("_SignPdf", model);
        }

        public IActionResult SignedOk(long taskId)
        {
            var task = taskService.Select_ById(taskId);
            if (task != null && task.TaskStateId != WorkTaskConstants.States.Completed)
            {
                switch (task.TaskTypeId)
                {
                    case WorkTaskConstants.Types.DocumentResolution_Sign:
                        taskService.CompleteTask(taskId);
                        var saveResult = taskService.UpdateAfterCompleteTask(task);
                        if (saveResult.Result)
                        {
                            SetSuccessMessage("Подписването на документа премина успешно.");
                        }
                        break;
                    case WorkTaskConstants.Types.CaseSessionAct_Coordinate:
                        break;

                }

                return RedirectToAction(nameof(Edit), new { id = task.SourceId });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
