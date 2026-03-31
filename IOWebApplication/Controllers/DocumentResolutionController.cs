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
using Microsoft.EntityFrameworkCore;
using Rotativa.Extensions;
using System;
using System.Collections.Generic;
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
            var bc = commonService.Breadcrumbs_DocumentResolution(0);
            ViewBag.breadcrumbs = bc;
            SetHelpFile(HelpFileValues.RegisteredDocumentsDisposition);
            AddAuditInfo(AuditConstants.Operations.List, bc?.LastOrDefault()?.Title, "", SourceTypeSelectVM.DocumentResolution);
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

        public async Task<IActionResult> Add(long documentId)
        {
            if (!await CheckAccessAsync(drService, SourceTypeSelectVM.DocumentResolution, null, AuditConstants.Operations.Append, documentId))
            {
                return Redirect_Denied();
            }
            var model = new DocumentResolution()
            {
                CourtId = userContext.CourtId,
                DocumentId = documentId,
                UserDecisionId = userContext.UserId,
                JudgeDecisionCount = 1
            };
            await SetViewBag(model);
            return View(nameof(Edit), model);
        }

        public async Task<IActionResult> Edit(long id)
        {
            if (!await CheckAccessAsync(drService, SourceTypeSelectVM.DocumentResolution, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await drService.ReadByIdAsync<DocumentResolution>(id);
            if (model.DateExpired != null)
            {
                return NotFoundError(MessageConstant.Values.ObjectWasDeleted);
            }
            await SetViewBag(model);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DocumentResolution model)
        {
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                await SetViewBag(model);
                return View(nameof(Edit), model);
            }
            long currentId = model.Id;
            var saveResult = await drService.SaveData(model);
            if (saveResult.Result)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);

                addAuditInfo(currentId == 0, model);

                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetSuccessMessage(saveResult.ErrorMessage);
                await SetViewBag(model);
                return View(nameof(Edit), model);
            }
        }

        private void addAuditInfo(bool isInsert, DocumentResolution model)
        {

            if (CurrentContext_IsSame(SourceTypeSelectVM.DocumentResolution, model.Id))
            {
                if (isInsert)
                {
                    AddAuditInfo(AuditConstants.Operations.Append, CurrentContext?.Info?.BaseObject, CurrentContext?.Info?.ObjectInfo, SourceTypeSelectVM.DocumentResolution);
                }
                else
                {
                    AddAuditInfo(AuditConstants.Operations.Update, CurrentContext?.Info?.BaseObject, CurrentContext?.Info?.ObjectInfo, SourceTypeSelectVM.DocumentResolution);
                }
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
            if (model.JudgeDecisionCount == 2 && (model.JudgeDecisionLawunit2Id ?? 0) <= 0)
            {
                ModelState.AddModelError(nameof(DocumentResolution.JudgeDecisionLawunit2Id), "Изберете 'Съдия'");
            }
            if (string.IsNullOrEmpty(model.UserDecisionId) || model.UserDecisionId == "0")
            {
                ModelState.AddModelError(nameof(DocumentResolution.JudgeDecisionLawunitId), "Изберете 'Изготвил'");
            }

            if (model.ResolutionTypeId == DocumentConstants.ResolutionTypes.ResolutionForSelection)
            {
                if (string.IsNullOrEmpty(model.TaskUserId))
                {
                    ModelState.AddModelError(nameof(DocumentResolution.TaskUserId), "Изберете 'Изпълнител'");
                }
            }
        }

        private async Task SetViewBag(DocumentResolution model)
        {
            ViewBag.ResolutionTypeId_ddl = nomService.GetDropDownList<ResolutionType>().SingleOrChoose();
            if (model.Id > 0)
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentResolution(model.Id).DeleteOrDisableLast();
                ViewBag.docInfo = commonService.Breadcrumbs_Document(model.DocumentId).LastOrDefault()?.Title;
                ViewBag.canChange = (model.DeclaredDate == null);

                ViewBag.documentId = model.DocumentId;
                ViewBag.documentResolutionId = model.Id;
            }
            else
            {
                var bc = commonService.Breadcrumbs_Document(model.DocumentId);
                ViewBag.breadcrumbs = bc;
                ViewBag.docInfo = bc.LastOrDefault()?.Title;
            }

            ViewBag.hasActFile = await cdnService.Select(SourceTypeSelectVM.DocumentResolutionPdf, model.Id.ToString()).AnyAsync();

            if (model.DeclaredDate != null)
            {
                if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CaseSessionActCorrection))
                {
                    ViewBag.canCorrectAfterDeclare = false;
                }
            }

            SetHelpFile(userContext.CourtTypeId == NomenclatureConstants.CourtType.VKS ? HelpFileValues.Scheduletask : HelpFileValues.DispositionTask);
        }

        public async Task<IActionResult> Blank(long id)
        {
            var checkBlankInfo = drService.CheckActBlankAccess(true, id);
            if (!checkBlankInfo.canAccess)
            {
                SetErrorMessage($"По проекта на акта работи {checkBlankInfo.lawunitName}.");
                return RedirectToAction(nameof(Edit), new { id = id });
            }

            var actModel = drService.Select(0, id).FirstOrDefault();

            if (actModel.DeclaredDate != null)
            {
                if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CaseSessionActCorrection))
                {
                    SetErrorMessage("Съдебният акт е постановен. Не можете да извършвате корекция по него.");
                    return RedirectToAction("Edit", new { id = id });
                }
            }

            int sourceType = SourceTypeSelectVM.DocumentResolutionBlank;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });


            var model = new BlankEditVM()
            {
                Title = "Изготвяне на разпореждане",
                SourceType = sourceType,
                SourceId = id.ToString(),
                SessionName = userContext.GenHash(id, sourceType),
                HtmlHeader = await this.RenderViewAsync("ActHeader", actModel),
                HtmlContent = html,
                FooterIsEditable = false,
                ReturnUrl = Url.Action(nameof(Edit), new { id }),
                HasPreviewButton = true
            };

            ViewBag.breadcrumbs = commonService.Breadcrumbs_DocumentResolution(id);
            SetHelpFile(userContext.CourtTypeId == NomenclatureConstants.CourtType.VKS ? HelpFileValues.Scheduletask : HelpFileValues.DispositionTask);

            return View("BlankEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> Blank(BlankEditVM model, string btnPreview = null)
        {
            if (!userContext.CheckHash(model))
            {
                return Redirect_Denied();
            }
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
                drService.UpdateActCreator(long.Parse(model.SourceId));

                var context = await drService.GetCurrentContextAsync(SourceTypeSelectVM.DocumentResolution, long.Parse(model.SourceId), AuditConstants.Operations.View);
                AddAuditInfo("Изготвяне", CurrentContext?.Info?.BaseObject, CurrentContext?.Info?.ObjectInfo, SourceTypeSelectVM.DocumentResolution);


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

        [HttpPost]
        public async Task<IActionResult> Resolution_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(drService, SourceTypeSelectVM.DocumentResolution, model.LongId, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            if (string.IsNullOrEmpty(model.DescriptionExpired))
            {
                return Json(new { result = false, message = MessageConstant.Values.DescriptionExpireRequired });
            }

            var resolution = await drService.GetByIdAsync<DocumentResolution>(model.LongId);
            if (resolution.DeclaredDate != null || !CurrentContext.CanChangeFull)
            {
                return Json(new { result = false, message = "Разпореждането не може да бъде премахнато." });
            }

            var expResult = drService.ResolutionExpire(model);
            if (expResult.Result)
            {
                SetAuditContextDelete(drService, SourceTypeSelectVM.DocumentResolution, model.LongId);
                SetSuccessMessage(MessageConstant.Values.DocumentResolutionExpireOK);
                var docRes = await drService.GetByIdAsync<DocumentResolution>(model.LongId);
                return Json(new { result = true, redirectUrl = Url.Action(nameof(ResolutionsByDocument), new { documentId = docRes.DocumentId }) });
            }
            else
            {
                return Json(new { result = false, message = expResult.ErrorMessage ?? MessageConstant.Values.SaveFailed });
            }
        }

        private async Task<IActionResult> blankPreview(BlankEditVM model)
        {
            long resolutionId = long.Parse(model.SourceId);
            var actModel = drService.Select(0, resolutionId).FirstOrDefault();

            string html = await GetActHTML(actModel);

            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
            var contentDispositionHeader = new System.Net.Mime.ContentDisposition
            {
                Inline = true,
                FileName = "documentResolution.pdf"
            };
            Response.Headers.Append(new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("Content-Disposition", contentDispositionHeader.ToString()));
            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf);
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

        public async Task<IActionResult> SentForSignQuick(int id)
        {
            var actModel = drService.Select(0, id).FirstOrDefault();
            string actHTML = await GetActHTML(actModel);
            if (string.IsNullOrEmpty(actHTML))
            {
                SetErrorMessage("Няма изготвен акт.");
                return RedirectToAction("Edit", new { id = id });
            }

            var newSendForSignTask = new WorkTaskEditVM()
            {
                SourceType = SourceTypeSelectVM.DocumentResolution,
                SourceId = id,
                TaskTypeId = WorkTaskConstants.Types.DocumentResolution_SentToSign,
                TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser
            };
            if (await taskService.CreateTask(newSendForSignTask))
            {
                return RedirectToAction(nameof(DoTask_SentForSign), new { id = newSendForSignTask.Id });
            }

            return RedirectToAction("Edit", new { id = id });
        }

        public async Task<IActionResult> DoTask_SentForSign(long id)
        {
            var task = taskService.Select_ById(id);
            switch (task.SourceType)
            {
                case SourceTypeSelectVM.DocumentResolution:
                    var actId = task.SourceId;
                    var actModel = drService.Select(0, actId).FirstOrDefault();

                    if (actModel.DeclaredDate != null)
                    {
                        if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CaseSessionActCorrection))
                        {
                            taskService.ExpireTasks(new List<long>() { id }.ToArray(), "");

                            SetErrorMessage("Съдебният акт е постановен. Не можете да извършвате корекция по него.");
                            return RedirectToAction("Edit", new { id = actId });
                        }
                    }

                    string actHTML = await GetActHTML(actModel);
                    if (string.IsNullOrEmpty(actHTML))
                    {
                        SetErrorMessage("Няма изготвен акт.");
                        return RedirectToAction("Edit", new { id = actId });
                    }

                    await PrepareActFile(actModel, actHTML);
                    bool isOk = true;
                    var newTask = new WorkTaskEditVM()
                    {
                        ParentTaskId = id,
                        SourceType = SourceTypeSelectVM.DocumentResolution,
                        SourceId = actId,
                        TaskTypeId = WorkTaskConstants.Types.DocumentResolution_Sign,
                        TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                        UserId = actModel.JudgeUserId,
                    };

                    if (actModel.JudgeCount > 1)
                    {
                        var newTask2 = new WorkTaskEditVM()
                        {
                            ParentTaskId = id,
                            SourceType = SourceTypeSelectVM.DocumentResolution,
                            SourceId = actId,
                            TaskTypeId = WorkTaskConstants.Types.DocumentResolution_Sign,
                            TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                            UserId = actModel.JudgeUser2Id,
                        };

                        if ((await taskService.CreateTask(newTask)) && (await taskService.CreateTask(newTask2)))
                        {
                            SetSuccessMessage("Задачите за подпис са създадени успешно.");
                            isOk = true;
                        }
                    }
                    else
                    {
                        if (await taskService.CreateTask(newTask))
                        {
                            SetSuccessMessage("Задачата за подпис е създадена успешно.");
                            isOk = true;
                        }
                    }

                    if (isOk)
                    {
                        await taskService.CompleteTask(id);
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
            if (CheckDoublePostback("ress"))
            {
                return RedirectToAction("Edit", new { id });
            }

            Uri urlSuccess = new Uri(Url.Action(nameof(SignedOk), new { id }), UriKind.Relative);
            Uri url = new Uri(Url.Action("Edit", new { id }), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.DocumentResolutionPdf,
                DestinationType = SourceTypeSelectVM.DocumentResolutionPdf,
                Location = userContext.CourtName,
                Reason = "Подписване на разпореждане",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url,
                WorkTaskId = taskId
            };

            var docResolution = drService.GetById<DocumentResolution>(id);

            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {
                var registerResult = drService.Register(docResolution);
                if (!registerResult.Result)
                {
                    SetErrorMessage(registerResult.ErrorMessage);
                    return RedirectToAction("Edit", new { id });
                }
                else
                {
                    if (registerResult.SaveMethod == "register")
                    {
                        var actModel = drService.Select(0, id).FirstOrDefault();

                        string actHTML = await GetActHTML(actModel);
                        await PrepareActFile(actModel, actHTML);
                        ClearCheckDoublePostback("ress");
                        return RedirectToAction(nameof(SendForSign), new { id, taskId });
                    }
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

        public IActionResult SignedOk(int id)
        {
            SetSuccessMessage("Подписването на документа премина успешно.");
            return RedirectToAction(nameof(Edit), new { id });

        }

        [HttpPost]
        public IActionResult CasesByResolutions_ListData(IDataTablesRequest request, long documentResolutionId)
        {
            var data = drService.SelectCasesByResolution(documentResolutionId);
            return request.GetResponse(data);
        }

        public IActionResult AddCase(long documentResolutionId)
        {
            ViewBag.documentResolutionId = documentResolutionId;
            var model = new DocumentResolutionCase()
            {
                DocumentResolutionId = documentResolutionId
            };
            return PartialView("_AddCase", model);
        }

        [HttpPost]
        public IActionResult AddCase(DocumentResolutionCase model)
        {
            if (drService.AppendCaseToResolution(model.DocumentResolutionId, model.CaseId))
            {
                return Content("ok");
            }
            else
            {
                return Content("failed");
            }
        }

        [HttpPost]
        public IActionResult RemoveCase(long id)
        {
            if (drService.RemoveCaseToResolution(id))
            {
                return Content("ok");
            }
            else
            {
                return Content("failed");
            }
        }
    }
}
