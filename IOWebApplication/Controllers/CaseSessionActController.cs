// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Rotativa.AspNetCore.Options;
using Rotativa.Extensions;

namespace IOWebApplication.Controllers
{
    public class CaseSessionActController : BaseController
    {
        private readonly ICaseSessionActService service;
        private readonly ICaseSessionActCoordinationService coordinationService;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICaseSessionService sessionService;
        private readonly ICaseSessionMeetingService sessionMeetingService;
        private readonly ICaseService caseService;
        private readonly ICdnService cdnService;
        private readonly IWorkTaskService taskService;
        private readonly ICasePersonService casePersonService;
        private readonly IPrintDocumentService printDocumentService;
        private readonly IMQEpepService mqEpepService;
        private readonly ICaseSessionActComplainService caseSessionActComplainService;
        private readonly ICaseLifecycleService caseLifecycleService;

        public CaseSessionActController(
            ICaseSessionActService _service,
            INomenclatureService _nomService,
            ICommonService _commonService,
            ICaseSessionActCoordinationService _coordinationService,
            ICaseSessionMeetingService _sessionMeetingService,
            ICaseSessionService _sessionService,
            ICaseService _caseService,
            IWorkTaskService _taskService,
            ICdnService _cdnService,
            ICasePersonService _casePersonService,
            IPrintDocumentService _printDocumentService,
            IMQEpepService _mqEpepService,
            ICaseSessionActComplainService _caseSessionActComplainService,
            ICaseLifecycleService _caseLifecycleService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            sessionService = _sessionService;
            sessionMeetingService = _sessionMeetingService;
            cdnService = _cdnService;
            caseService = _caseService;
            coordinationService = _coordinationService;
            taskService = _taskService;
            casePersonService = _casePersonService;
            printDocumentService = _printDocumentService;
            mqEpepService = _mqEpepService;
            caseSessionActComplainService = _caseSessionActComplainService;
            caseLifecycleService = _caseLifecycleService;
        }

        public IActionResult Index(int caseSessionId)
        {
            //if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionAct, null, AuditConstants.Operations.View, caseSessionId))
            //{
            //    return Redirect_Denied();
            //}
            CaseSessionActFilterVM filter = new CaseSessionActFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31),
                IsFinalDoc = false
            };
            SetHelpFile(HelpFileValues.CourtActsandProtocols);
            return View(filter);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseSessionId)
        {
            var data = service.CaseSessionAct_Select(caseSessionId, null, null, null, null, null);
            return request.GetResponse(data);
        }

        [HttpPost]
        public IActionResult ListDataSpr(IDataTablesRequest request, CaseSessionActFilterVM model)
        {
            var data = service.CaseSessionActSpr_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        public IActionResult Add(int caseSessionId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionAct, null, AuditConstants.Operations.Append, caseSessionId))
            {
                return Redirect_Denied();
            }
            var caseSession = service.GetById<CaseSession>(caseSessionId);
            if (caseSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno && caseSession.DateFrom.Date > DateTime.Now.Date)
            {
                SetErrorMessage("Не можете да добавяте акт в насрочено заседание с бъдеща дата.");
                return RedirectToAction("Preview", "CaseSession", new { id = caseSessionId });
            }

            if ((caseSession.SessionStateId != NomenclatureConstants.SessionState.Provedeno)
                && (caseSession.SessionStateId != NomenclatureConstants.SessionState.Nasrocheno))
            {
                SetErrorMessage("Заседанието не е със статус проведено.");
                return RedirectToAction("Preview", "CaseSession", new { id = caseSessionId });
            }

            var model = new CaseSessionAct()
            {
                CaseSessionId = caseSessionId,
                CaseId = caseSession.CaseId,
                CourtId = userContext.CourtId,
                ActStateId = NomenclatureConstants.SessionActState.Project
            };
            SetViewbag(model);
            return View(nameof(Edit), model);
        }

        public IActionResult Edit(int id, long? taskId = null)
        {
            var model = service.GetById<CaseSessionAct>(id);
            if (model == null || model.DateExpired != null)
            {
                throw new NotFoundException("Търсеният от Вас акт не е намерен и/или нямате достъп до него.");
            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionAct, id, AuditConstants.Operations.Update, model.CaseSessionId))
            {
                return Redirect_Denied();
            }
            if (taskId > 0)
            {
                var task = taskService.Select_ById(taskId.Value);
                if (task != null && task.TaskStateId != WorkTaskConstants.States.Completed)
                {
                    switch (task.TaskTypeId)
                    {
                        case WorkTaskConstants.Types.CaseSessionAct_Sign:
                        case WorkTaskConstants.Types.CaseSessionActMotives_Sign:
                        case WorkTaskConstants.Types.CaseSessionActCoordination_Sign:
                            taskService.CompleteTask(taskId.Value);
                            var saveResult = taskService.UpdateAfterCompleteTask(task);
                            if (saveResult.Result)
                            {
                                switch (saveResult.SaveMethod)
                                {
                                    case "reload":
                                        model = service.GetById<CaseSessionAct>(id);
                                        break;
                                }
                            }
                            break;
                        case WorkTaskConstants.Types.CaseSessionAct_Coordinate:
                            break;

                    }
                }

            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionAct, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            SetViewbag(model);
            return View(nameof(Edit), model);
        }

        void SetViewbag(CaseSessionAct model)
        {
            ViewBag.hasEditFinishDoc = true;
            ViewBag.canAccessFile = false;
            if (model.Id > 0)
            {
                ViewBag.hasCoordination = coordinationService.CaseSessionActCoordination_Select(model.Id).Count() > 0;
                int[] actFilesSourceTypes = {
                                SourceTypeSelectVM.CaseSessionActPdf,
                                SourceTypeSelectVM.CaseSessionActDepersonalizedBlank,
                                SourceTypeSelectVM.CaseSessionActMotiveDepersonalizedBlank,
                                SourceTypeSelectVM.CaseSessionActDepersonalized,
                                SourceTypeSelectVM.CaseSessionActMotiveDepersonalized
                                };
                var actFiles = cdnService.Select(actFilesSourceTypes, model.Id.ToString());

                ViewBag.hasActPdf = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActPdf);
                ViewBag.hasDefacedBlank = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActDepersonalizedBlank);
                ViewBag.hasDefacedMotivesBlank = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActMotiveDepersonalizedBlank);
                ViewBag.hasDefacedAct = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActDepersonalized);
                ViewBag.hasDefacedMotives = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActMotiveDepersonalized);
                ViewBag.hasEditFinishDoc = !caseLifecycleService.CaseLifecycle_IsExistLifcycleAfter(model.CaseId ?? 0, model.Id);
                ViewBag.canAccessFile = service.CheckActBlankAccess(model.Id).canAccess;
            }
            else
            {
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(model.CaseSessionId);

            var caseSession = sessionService.CaseSessionById(model.CaseSessionId);
            var caseCase = service.GetById<Case>(caseSession.CaseId);
            ViewBag.CaseGroupId = caseCase.CaseGroupId;
            ViewBag.ActTypeId_ddl = service.GetActTypesByCase(model.CaseSessionId);
            ViewBag.ActResultId_ddl = caseSessionActComplainService.GetDropDownList_ActResultFromCaseSessionActComplainResult(model.Id);
            bool actStateInitial = string.IsNullOrEmpty(model.RegNumber);
            ViewBag.ActStateId_ddl = nomService.GetDDL_CaseSessionActState(actStateInitial, !actStateInitial);
            ViewBag.SecretaryUserId_ddl = sessionMeetingService.GetDDL_MeetingUserBySessionId(model.CaseSessionId);
            ViewBag.ActISPNReasonId_ddl = nomService.GetDropDownList<ActISPNReason>();
            ViewBag.ActISPNDebtorStateId_ddl = nomService.GetDropDownList<ActISPNDebtorState>();
            ViewBag.RelatedActId_ddl = service.GetDropDownList_CaseSessionActEnforced(model.CaseId ?? 0);

            ViewBag.isDivorce = false;
            ViewBag.isISPNcase = caseCase.IsISPNcase == true;
            if (model.Id > 0)
            {
                int[] codeDivorce = nomService.GetCaseCodeGroupingByGroup(NomenclatureConstants.CaseCodeGroupings.Divorce);
                if (codeDivorce.Contains(caseCase.CaseCodeId ?? 0))
                {
                    var caseSessionAct = service.GetById<CaseSessionAct>(model.Id);
                    if (NomenclatureConstants.SessionActState.EnforcedStates.Contains(caseSessionAct.ActStateId))
                        ViewBag.isDivorce = true;
                }
            }

            ViewBag.isRegisterCompany = false;
            if (caseCase.CaseGroupId == NomenclatureConstants.CaseGroups.Company)
            {
                ViewBag.isRegisterCompany = caseService.IsRegisterCompany(caseCase.Id);
            }

            var actComplainResults = nomService.GetDDL_ActComplainResult(caseCase.CaseTypeId);
            ViewBag.ActComplainResultId_ddl = actComplainResults;
            ViewBag.hasComplainResult = actComplainResults.Count > 1;
            var actComplainIndex = nomService.GetDDL_ActComplainIndex(caseCase.Id);
            ViewBag.ActComplainIndexId_ddl = actComplainIndex;
            ViewBag.hasComplainIndex = actComplainIndex.Count > 1;
            SetHelpFile(HelpFileValues.SessionAct);
        }

        public IActionResult Get_ActKindsByActType(int actTypeId)
        {
            var model = service.GetActKindsByActType(actTypeId);
            return Json(model);
        }

        public IActionResult Get_ActKindInfo(int actKindId)
        {
            var model = service.GetById<ActKind>(actKindId);
            return Json(model);
        }

        private string IsValid(CaseSessionAct model)
        {
            if (model.ActTypeId < 1)
                return "Няма избран вид";

            if (model.ActStateId < 1)
                return "Няма избран статус";

            if (model.ActComplainIndexId > 0)
            {
                var actResults = caseSessionActComplainService.GetDropDownList_ActResultFromCaseSessionActComplainResult(model.Id);
                if ((model.ActResultId < 1) && (actResults.Count() > 1))
                    return "Избрали сте индекс, но не сте избрали резултат от обжалване";
            }

            if (model.IsFinalDoc)
            {
                var _case = service.GetById<Case>(model.CaseId);
                var actComplainResults = nomService.GetDDL_ActComplainResult(_case.CaseTypeId);
                if ((model.ActComplainResultId < 1) && (actComplainResults.Count() > 1))
                {
                    return "Изберете резултат/степен на уважаване на иска";
                }
            }

            //if (model.ActKindId > 0)
            //{
            //    var actKind = service.GetById<ActKind>(model.ActKindId ?? 0);
            //    if (actKind != null)
            //        if (actKind.MustSelectRelatedAct == true && (model.RelatedActId ?? 0) <= 0)
            //        {
            //            return "Изберете свързан акт";
            //        }
            //}

            return string.Empty;
        }

        [HttpPost]
        public IActionResult Edit(CaseSessionAct model)
        {
            SetViewbag(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CaseSessionAct_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionAct, model.Id, currentId == 0);
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

        public async Task<IActionResult> Blank(int id)
        {
            var checkBlankInfo = service.CheckActBlankAccess(id);
            if (!checkBlankInfo.canAccess)
            {
                SetErrorMessage($"По проекта на акта работи {checkBlankInfo.lawunitName}.");
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            var actModel = service.CaseSessionAct_GetForPrint(id);
            int sourceType = SourceTypeSelectVM.CaseSessionActBlank;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });


            var model = new BlankEditVM()
            {
                Title = "Изготвяне на съдебен акт",
                SourceType = sourceType,
                SourceId = id.ToString(),
                HtmlHeader = await this.RenderViewAsync("ActHeaderChairman", actModel),
                //HtmlContent = html,
                //HtmlFooter = actModel.Dispositiv,
                FooterIsEditable = true,
                FooterIsHtml = true,
                FooterTitle = "Диспозитив",
                ReturnUrl = Url.Action(nameof(Edit), new { id }),
                HasPreviewButton = true
            };

            var decodedHtml = decodeBlank(html, actModel.Dispositiv);
            model.HtmlContent = decodedHtml.Body;
            model.HtmlFooter = decodedHtml.Dispositive;

            await fillBlankByActKind(model, actModel, html);

            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(id);
            SetHelpFile(HelpFileValues.SessionAct);

            return View("BlankEdit", model);
        }
        private async Task fillBlankByActKind(BlankEditVM blankModel, CaseSessionActPrintVM actModel, string html)
        {
            switch (actModel.ActKindBlankName)
            {
                case "ProtectiveOrder":
                    actModel.HeaderOnly = true;
                    blankModel.HtmlHeader = await this.RenderViewAsync("_ProtectiveOrder", actModel);
                    actModel.HeaderOnly = false;
                    if (string.IsNullOrEmpty(html))
                    {
                        blankModel.HtmlContent = actModel.RelatedActDispositive;
                    }
                    blankModel.FooterIsEditable = false;
                    break;
                default:
                    return;
            }
        }
        private const string blankDispositiveSeparator = "|||ENDOFBODY|||";
        private (string Body, string Dispositive) decodeBlank(string html, string actDispositiv)
        {
            var body = html;
            var dispositive = actDispositiv;
            int sepPosition = html.IndexOf(blankDispositiveSeparator);
            if (sepPosition >= 0)
            {
                body = html.Substring(0, sepPosition);
                dispositive = html.Substring(sepPosition).Replace(blankDispositiveSeparator, "");
            }
            return (Body: body, Dispositive: dispositive);
        }

        private string encodeBlank(string htmlBody, string htmlDispositiv)
        {
            return (htmlBody ?? "") + blankDispositiveSeparator + (htmlDispositiv ?? "");
        }
        private string convertToPlainBlank(string html)
        {
            html = html.Replace("<br/>", System.Environment.NewLine).Replace("<p", System.Environment.NewLine + "<p");
            return HttpUtility.HtmlDecode(
                                            Regex.Replace(html, "<(.|\n)*?>", "")
                                         );
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
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(encodeBlank(model.HtmlContent, model.HtmlFooter)))
            };
            if (await cdnService.MongoCdn_AppendUpdate(htmlRequest))
            {
                service.CaseSessionAct_SaveDispositiv(int.Parse(model.SourceId), convertToPlainBlank(model.HtmlFooter ?? ""));
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            if (!string.IsNullOrEmpty(btnPreview))
            {
                return await blankPreview(model);
            }
            return RedirectToAction(nameof(Edit), new { id = model.SourceId });
            //return RedirectToAction(nameof(Blank), new { id = model.SourceId });
        }

        public async Task<IActionResult> BlankComplete(int id)
        {
            var checkBlankInfo = service.CheckActBlankAccess(id);
            if (!checkBlankInfo.canAccess)
            {
                SetErrorMessage($"По проекта на акта работи {checkBlankInfo.lawunitName}.");
                return RedirectToAction(nameof(Edit), new { id = id });
            }

            int sourceType = SourceTypeSelectVM.CaseSessionActBlankComplete;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            if (string.IsNullOrEmpty(html))
            {
                var actModel = service.CaseSessionAct_GetForPrint(id);
                html = await GetActHTML(actModel);
                await saveCompleteBlank(id.ToString(), html);
            }

            var model = new BlankEditVM()
            {
                Title = "Корекция на съдебен акт",
                SourceType = sourceType,
                SourceId = id.ToString(),
                HtmlContent = html,
                FooterIsEditable = false,
                ReturnUrl = Url.Action(nameof(Edit), new { id }),
                HasPreviewButton = true,
                HasResetButton = true

            };
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(id);
            SetHelpFile(HelpFileValues.SessionAct);

            return View("BlankEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> BlankComplete(BlankEditVM model, string btnPreview = null, string reset_mode = null)
        {
            if (!string.IsNullOrEmpty(reset_mode))
            {
                await cdnService.MongoCdn_DeleteFiles(new CdnFileSelect() { SourceType = model.SourceType, SourceId = model.SourceId });
                SetSuccessMessage("Информацията в акта е обновена с данните по делото.");
                return RedirectToAction(nameof(BlankComplete), new { id = model.SourceId });
            }

            if (await saveCompleteBlank(model.SourceId, model.HtmlContent))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            if (!string.IsNullOrEmpty(btnPreview))
            {
                return await blankPreview(model);
            }
            return RedirectToAction(nameof(Edit), new { id = model.SourceId });
            //return RedirectToAction(nameof(BlankComplete), new { id = model.SourceId });
        }

        private async Task<IActionResult> blankPreview(BlankEditVM model)
        {
            int caseSessionActId = int.Parse(model.SourceId);
            var actModel = service.CaseSessionAct_GetForPrint(caseSessionActId);
            string html;

            if (model.SourceType == SourceTypeSelectVM.CaseSessionActBlankComplete)
            {
                html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = model.SourceType, SourceId = model.SourceId });
            }
            else
            {

                if (!string.IsNullOrEmpty(actModel.ActKindBlankName))
                {
                    html = await GenerateCustomActBlank(actModel);
                }
                else
                {
                    var blankHtml = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = caseSessionActId.ToString() });

                    var decodedHtml = decodeBlank(blankHtml, actModel.Dispositiv);
                    actModel.MainBody = decodedHtml.Body;
                    actModel.Dispositiv = decodedHtml.Dispositive;
                    html = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "ActFormat.cshtml", actModel, true);
                }
            }

            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);

            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf, "sessionActPreview.pdf");
        }

        private async Task<bool> saveCompleteBlank(string id, string html)
        {
            var htmlRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseSessionActBlankComplete,
                SourceId = id,
                FileName = "completeDraft.html",
                ContentType = NomenclatureConstants.ContentTypes.Html,
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html))
            };
            return await cdnService.MongoCdn_AppendUpdate(htmlRequest);
        }

        public async Task<IActionResult> BlankMotives(int id)
        {
            var checkBlankInfo = service.CheckMotiveBlankAccess(id);
            if (!checkBlankInfo.canAccess)
            {
                SetErrorMessage($"По проекта на мотивите работи {checkBlankInfo.lawunitName}.");
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            int sourceType = SourceTypeSelectVM.CaseSessionActMotiveBlank;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            if (string.IsNullOrEmpty(html))
            {
                //html = await this.RenderViewAsync("ActPrepare", actModel);
            }

            var model = new BlankEditVM()
            {
                Title = "Изготвяне на мотиви към съдебен акт",
                SourceType = sourceType,
                SourceId = id.ToString(),
                HtmlContent = html,
                ReturnUrl = Url.Action(nameof(Edit), new { id })
            };
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(id);
            SetHelpFile(HelpFileValues.SessionAct);

            return View("BlankEdit", model);
        }
        [HttpPost]
        public async Task<IActionResult> BlankMotives(BlankEditVM model)
        {
            var htmlRequest = new CdnUploadRequest()
            {
                SourceType = model.SourceType,
                SourceId = model.SourceId,
                FileName = "draft.html",
                ContentType = NomenclatureConstants.ContentTypes.Html,
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.HtmlContent ?? ""))
            };
            if (await cdnService.MongoCdn_AppendUpdate(htmlRequest))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return RedirectToAction(nameof(BlankMotives), new { id = model.SourceId });
        }
        public async Task<IActionResult> DepersonalizeAct(int id)
        {
            var actModel = service.CaseSessionAct_GetForPrint(id);
            int sourceType = SourceTypeSelectVM.CaseSessionActDepersonalizedBlank;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            var model = new DepersonalizationModel()
            {
                SubmitAction = this.ActionName,
                CaseId = actModel.CaseId,
                SourceId = id.ToString(),
                //DocumentName = $"{actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate:dd.MM.yyyy}",
                DocumentContent = html,
                CancelUrl = Url.Action("Edit", new { id = id }),
                DepersonalizationHistory = caseService.GetDepersonalizationHistory(actModel.CaseId)
            };
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(id);
            SetHelpFile(HelpFileValues.SessionAct);

            return View("DepersonalizeDocument", model);
        }
        [HttpPost]
        public async Task<IActionResult> DepersonalizeAct(DepersonalizationModel model)
        {
            bool isFinal = model.SaveMode == "finalize";
            bool isOk = false;
            var htmlRequest = new CdnUploadRequest();
            htmlRequest.SourceType = SourceTypeSelectVM.CaseSessionActDepersonalizedBlank;
            htmlRequest.SourceId = model.SourceId;
            htmlRequest.FileName = "draft.html";
            htmlRequest.ContentType = NomenclatureConstants.ContentTypes.Html;
            htmlRequest.FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.DocumentContent ?? ""));
            isOk = await cdnService.MongoCdn_AppendUpdate(htmlRequest);
            if (isFinal)
            {
                var actModel = service.CaseSessionAct_GetForPrint(int.Parse(model.SourceId));
                if (actModel.ActDeclaredDate == null)
                {
                    SetErrorMessage("Не можете да финализирате обезличаването на акт, който не е постановен.");
                    return RedirectToAction(nameof(Edit), new { id = model.SourceId });
                }
                var pdfRequest = new CdnUploadRequest();
                var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = model.DocumentContent ?? "" }, true).GetByte(this.ControllerContext);

                pdfRequest.SourceType = SourceTypeSelectVM.CaseSessionActDepersonalized;
                pdfRequest.SourceId = model.SourceId;
                pdfRequest.FileName = "sessionActDepersonilized.pdf";
                pdfRequest.ContentType = NomenclatureConstants.ContentTypes.Pdf;
                pdfRequest.Title = $"{actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate:dd.MM.yyyy} - обезличен";
                pdfRequest.FileContentBase64 = Convert.ToBase64String(pdfBytes);

                isOk &= await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            }

            if (isOk)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                var replaceItems = JsonConvert.DeserializeObject<IEnumerable<DepersonalizationHistoryItem>>(model.DepersonalizationNewItems);
                caseService.SaveDataDepersonalizationHistory(model.CaseId, replaceItems, int.Parse(model.SourceId));
                mqEpepService.AppendCaseSessionAct_Public(int.Parse(model.SourceId), EpepConstants.ServiceMethod.Add);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return RedirectToAction(nameof(Edit), new { id = model.SourceId });
        }

        public IActionResult DoTask_SentForCoordinate(long id)
        {
            int actId = (int)taskService.Select_ById(id).SourceId;
            if (service.SendForCoordination_Init(actId, id))
            {
                SetSuccessMessage("Задачите за съгласуване са създадени успешно.");
                taskService.CompleteTask(id);
            }
            return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
        }

        public async Task<IActionResult> SentForSign(int actId)
        {
            var actModel = service.CaseSessionAct_GetForPrint(actId);
            string actHTML = await GetActHTML(actModel);
            var valResult = Validate_SentForSign(actModel, actHTML);
            if (!valResult.Result)
            {
                SetErrorMessage(valResult.ErrorMessage);
                return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
            }

            var newSendFormSignTask = new WorkTaskEditVM()
            {
                SourceType = SourceTypeSelectVM.CaseSessionAct,
                SourceId = actId,
                TaskTypeId = WorkTaskConstants.Types.CaseSessionAct_SentToSign,
                TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser
            };

            if (taskService.CreateTask(newSendFormSignTask))
            {
                var taskId = newSendFormSignTask.Id;
                await PrepareSessionActPdfFile(actModel, actHTML);

                var taskInitResult = service.SendForSign_Init(actId, taskId);
                if (taskInitResult.Result)
                {
                    SetSuccessMessage("Задачите за подписване са създадени успешно.");
                    taskService.CompleteTask(taskId);
                }
                else
                {
                    SetErrorMessage(taskInitResult.ErrorMessage);
                }
            }
            return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
        }

        private SaveResultVM Validate_SentForSign(CaseSessionActPrintVM actModel, string actHTML)
        {
            if (actModel.SessionStateId != NomenclatureConstants.SessionState.Provedeno)
            {
                return new SaveResultVM(false, "Заседанието не е проведено.");
            }

            if (string.IsNullOrEmpty(actHTML))
            {
                return new SaveResultVM(false, "Няма изготвен акт.");
            }
            return new SaveResultVM(true);
        }

        public async Task<IActionResult> DoTask_SentForSign(long id)
        {
            var task = taskService.Select_ById(id);
            switch (task.SourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    var actId = (int)task.SourceId;
                    var actModel = service.CaseSessionAct_GetForPrint(actId);
                    string actHTML = await GetActHTML(actModel);
                    var valResult = Validate_SentForSign(actModel, actHTML);
                    if (!valResult.Result)
                    {
                        SetErrorMessage(valResult.ErrorMessage);
                        return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
                    }
                    await PrepareSessionActPdfFile(actModel, actHTML);

                    var taskInitResult = service.SendForSign_Init(actId, id);
                    if (taskInitResult.Result)
                    {
                        SetSuccessMessage("Задачите за подписване са създадени успешно.");
                        taskService.CompleteTask(id);

                    }
                    else
                    {
                        SetErrorMessage(taskInitResult.ErrorMessage);
                    }

                    return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
                default:
                    return null;
            }
        }

        public async Task<IActionResult> DoTask_MotivesSentForSign(long id)
        {
            var task = taskService.Select_ById(id);
            switch (task.SourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    var actId = (int)task.SourceId;
                    string fileError = await PrepareSessionActMotivesPdfFile(actId);
                    if (string.IsNullOrEmpty(fileError))
                    {
                        if (service.SendForSignMotives_Init(actId, id))
                        {
                            SetSuccessMessage("Задачите за подписване на мотиви са създадени успешно.");
                            taskService.CompleteTask(id);
                        }
                    }
                    else
                    {
                        SetErrorMessage(fileError);
                    }

                    return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
                default:
                    return null;
            }
        }

        public async Task<IActionResult> PreviewAct(int caseSessionActId)
        {
            var actFiles = cdnService.Select(SourceTypeSelectVM.CaseSessionAct, caseSessionActId.ToString()).ToList();
            if (actFiles.Count() > 0)
            {
                var fileId = actFiles.FirstOrDefault().FileId;
                return RedirectToAction("Download", "Files", new { id = fileId });
            }

            var actModel = service.CaseSessionAct_GetForPrint(caseSessionActId);
            actModel.MainBody = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = caseSessionActId.ToString() });
            string html = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "ActFormat.cshtml", actModel, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);

            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf, "sessionAct.pdf");
        }

        private async Task<string> GetActHTML(CaseSessionActPrintVM actModel)
        {
            string html = string.Empty;

            if (!string.IsNullOrEmpty(actModel.ActKindBlankName))
            {
                html = await GenerateCustomActBlank(actModel);
            }
            else
            {
                var blankHtml = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = actModel.Id.ToString() });
                var decodedBlank = decodeBlank(blankHtml, actModel.Dispositiv);
                actModel.MainBody = decodedBlank.Body;
                actModel.Dispositiv = decodedBlank.Dispositive;
                if (string.IsNullOrEmpty(actModel.MainBody) && string.IsNullOrEmpty(actModel.Dispositiv))
                {
                    return null;
                }
                html = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "ActFormat.cshtml", actModel, true);
            }

            return html;
        }

        private async Task<string> PrepareSessionActPdfFile(CaseSessionActPrintVM actModel, string actHTML)
        {
            var blankComplete = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlankComplete, SourceId = actModel.Id.ToString() });
            if (!string.IsNullOrEmpty(blankComplete) && !string.IsNullOrEmpty(actModel.ActKindBlankName))
            {
                actHTML = blankComplete;
            }


            string htmlALL = string.Empty;

            htmlALL = await this.RenderPartialViewAsync("~/Views/Shared/", "CreatePdf.cshtml", new BlankEditVM() { HtmlContent = actHTML, AppendWatermarkforTest = false }, true);
            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = actHTML }, true).GetByte(this.ControllerContext);

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseSessionActPdf,
                SourceId = actModel.Id.ToString(),
                FileName = "sessionAct.pdf",
                ContentType = NomenclatureConstants.ContentTypes.Pdf,
                Title = $"{actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate}",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };

            await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            var actFullModel = service.CaseSessionAct_GetFullInfo(actModel.Id);
            var dpRules = service.AutoDepersonalizeAct_GenerateRules(actFullModel);
            caseService.SaveDataDepersonalizationHistory(actModel.CaseId, dpRules, actModel.Id);
            if (!string.IsNullOrEmpty(actModel.ActRegDate))
            {
                //Само регистрирани актове получава бланка за обезличаване и файлове с особено мнение
                var defacedHTML = service.AutoDepersonalizeAct(dpRules, htmlALL);
                var defacedActBlankRequest = new CdnUploadRequest()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionActDepersonalizedBlank,
                    SourceId = actModel.Id.ToString(),
                    FileName = "draft.html",
                    ContentType = NomenclatureConstants.ContentTypes.Html,
                    FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(defacedHTML ?? ""))
                };
                await cdnService.MongoCdn_AppendUpdate(defacedActBlankRequest);


                var coordinations = coordinationService.CaseSessionActCoordination_Select(actModel.Id).Where(x => (x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.AcceptWithOpinion) || (x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.DontAccept)).ToList();
                foreach (var coordination in coordinations)
                {
                    string coordinationHtml = await this.RenderViewAsync("CoordinationBlank", coordination);

                    var pdfBytesCoordination = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = coordinationHtml }, true).GetByte(this.ControllerContext);
                    var pdfRequestCoordination = new CdnUploadRequest()
                    {
                        SourceType = SourceTypeSelectVM.CaseSessionActCoordinationPdf,
                        SourceId = coordination.Id.ToString(),
                        FileName = "sessionActCoordination.pdf",
                        ContentType = NomenclatureConstants.ContentTypes.Pdf,
                        Title = $"Особено мнение към {actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate} на {coordination.CaseLawUnitName},{coordination.JudgeRoleLabel}",
                        FileContentBase64 = Convert.ToBase64String(pdfBytesCoordination)
                    };

                    await cdnService.MongoCdn_AppendUpdate(pdfRequestCoordination);
                }
            }
            return string.Empty;
        }

        private async Task<string> PrepareSessionActMotivesPdfFile(int caseSessionActId)
        {
            var actModel = service.CaseSessionAct_GetForPrint(caseSessionActId);

            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActMotiveBlank, SourceId = caseSessionActId.ToString() });
            if (string.IsNullOrEmpty(html))
            {
                return "Няма изготвени мотиви.";
            }
            string htmlALL = await this.RenderPartialViewAsync("~/Views/Shared/", "CreatePdf.cshtml", new BlankEditVM() { HtmlContent = html, AppendWatermarkforTest = false }, true);
            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseSessionActMotivePdf,
                SourceId = caseSessionActId.ToString(),
                FileName = "sessionActMotives.pdf",
                ContentType = NomenclatureConstants.ContentTypes.Pdf,
                Title = $"Мотиви към {actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate:dd.MM.yyyy}",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };

            await cdnService.MongoCdn_AppendUpdate(pdfRequest);


            var actFullModel = service.CaseSessionAct_GetFullInfo(caseSessionActId);
            var dpRules = caseService.GetDepersonalizationHistory(actModel.CaseId);
            var defacedHTML = service.AutoDepersonalizeAct(dpRules, htmlALL);
            var defacedActBlankRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseSessionActMotiveDepersonalizedBlank,
                SourceId = caseSessionActId.ToString(),
                FileName = "draft.html",
                ContentType = NomenclatureConstants.ContentTypes.Html,
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(defacedHTML ?? ""))

            };

            await cdnService.MongoCdn_AppendUpdate(defacedActBlankRequest);

            return string.Empty;
        }

        public async Task<IActionResult> DepersonalizeMotives(int id)
        {
            var actModel = service.CaseSessionAct_GetForPrint(id);
            int sourceType = SourceTypeSelectVM.CaseSessionActMotiveDepersonalizedBlank;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            var model = new DepersonalizationModel()
            {
                SubmitAction = this.ActionName,
                CaseId = actModel.CaseId,
                SourceId = id.ToString(),
                DocumentName = $"Мотиви",
                DocumentContent = html,
                CancelUrl = Url.Action("Edit", new { id = id }),
                DepersonalizationHistory = caseService.GetDepersonalizationHistory(actModel.CaseId)
            };
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(id);
            SetHelpFile(HelpFileValues.SessionAct);

            return View("DepersonalizeDocument", model);
        }

        [HttpPost]
        public async Task<IActionResult> DepersonalizeMotives(DepersonalizationModel model)
        {
            bool isFinal = model.SaveMode == "finalize";
            bool isOk = false;
            var htmlRequest = new CdnUploadRequest();
            htmlRequest.SourceType = SourceTypeSelectVM.CaseSessionActMotiveDepersonalizedBlank;
            htmlRequest.SourceId = model.SourceId;
            htmlRequest.FileName = "draft.html";
            htmlRequest.ContentType = NomenclatureConstants.ContentTypes.Html;
            htmlRequest.FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.DocumentContent ?? ""));

            isOk = await cdnService.MongoCdn_AppendUpdate(htmlRequest);

            if (isFinal)
            {
                var pdfRequest = new CdnUploadRequest();

                var actModel = service.CaseSessionAct_GetForPrint(int.Parse(model.SourceId));

                var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = model.DocumentContent ?? "" }, true).GetByte(this.ControllerContext);

                pdfRequest.SourceType = SourceTypeSelectVM.CaseSessionActMotiveDepersonalized;
                pdfRequest.SourceId = model.SourceId;
                pdfRequest.FileName = "sessionActMotivesDepersonilized.pdf";
                pdfRequest.ContentType = NomenclatureConstants.ContentTypes.Pdf;
                pdfRequest.Title = $"Мотиви към {actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate:dd.MM.yyyy} - обезличени";
                pdfRequest.FileContentBase64 = Convert.ToBase64String(pdfBytes);

                isOk &= await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            }

            if (isOk)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                var replaceItems = JsonConvert.DeserializeObject<IEnumerable<DepersonalizationHistoryItem>>(model.DepersonalizationNewItems);
                caseService.SaveDataDepersonalizationHistory(model.CaseId, replaceItems, int.Parse(model.SourceId));
                mqEpepService.AppendCaseSessionAct_PublicMotive(int.Parse(model.SourceId), EpepConstants.ServiceMethod.Add);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return RedirectToAction(nameof(Edit), new { id = model.SourceId });
        }

        public IActionResult RegisterAct(int id)
        {
            var _act = service.GetById<CaseSessionAct>(id);
            if (_act == null)
            {
                return Redirect_Denied("Търсения от Вас обект не беше намерен!");
            }

            var registerResult = service.CaseSessionAct_RegisterAct(_act);
            if (registerResult.Result)
            {
                SetSuccessMessage("Актът е регистриран успешно.");
                this.SaveLogOperation(this.ControllerName, nameof(Edit), "Регистриране на акт", IO.LogOperation.Models.OperationTypes.Patch, id);
            }
            else
            {
                SetErrorMessage(registerResult.ErrorMessage);
            }

            return RedirectToAction("Edit", new { id = id });
        }

        public async Task<IActionResult> SendActForSign(int id, long taskId)
        {
            var _act = service.GetById<CaseSessionAct>(id);
            if (_act == null)
            {
                return Redirect_Denied("Търсения от Вас обект не беше намерен!");
            }

            var registerResult = service.CaseSessionAct_RegisterAct(_act);
            if (!registerResult.Result)
            {
                SetErrorMessage(registerResult.ErrorMessage);
                return RedirectToAction("Edit", new { id = id });
            }
            else
            {
                if (registerResult.SaveMethod == "register")
                {
                    var actModel = service.CaseSessionAct_GetForPrint(id);
                    var actHTML = await GetActHTML(actModel);
                    await PrepareSessionActPdfFile(actModel, actHTML);
                    return RedirectToAction(nameof(SendActForSign), new { id, taskId });
                }
            }

            Uri urlSuccess = new Uri(Url.Action("Edit", "CaseSessionAct", new { id = id, taskId = taskId }), UriKind.Relative);
            Uri url = new Uri(Url.Action("Edit", "CaseSessionAct", new { id = id }), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.CaseSessionActPdf,
                DestinationType = SourceTypeSelectVM.CaseSessionActPdf,
                Location = userContext.CourtName,
                Reason = "Подписване на съдебен протокол/акт",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url
            };

            var lu = taskService.GetLawUnitByTaskId(taskId);
            if (lu != null)
            {
                model.SignerName = lu.FullName;
                model.SignerUic = lu.Uic;
            }

            return View("_SignPdf", model);
        }
        public IActionResult SendActForSignCoordination(int id, int coordinationId, long taskId)
        {
            Uri urlSuccess = new Uri(Url.Action("Edit", "CaseSessionAct", new { id = id, taskId = taskId }), UriKind.Relative);
            Uri url = new Uri(Url.Action("Edit", "CaseSessionAct", new { id = id }), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = coordinationId.ToString(),
                SourceType = SourceTypeSelectVM.CaseSessionActCoordinationPdf,
                DestinationType = SourceTypeSelectVM.CaseSessionActCoordinationPdf,
                Location = userContext.CourtName,
                Reason = "Подписване на особено мнение към съдебен протокол/акт",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url
            };

            var lu = taskService.GetLawUnitByTaskId(taskId);
            if (lu != null)
            {
                model.SignerName = lu.FullName;
                model.SignerUic = lu.Uic;
            }

            return View("_SignPdf", model);
        }

        public IActionResult SendActForSignMotives(int id, long taskId)
        {
            Uri urlSuccess = new Uri(Url.Action("Edit", "CaseSessionAct", new { id = id, taskId = taskId }), UriKind.Relative);
            Uri url = new Uri(Url.Action("Edit", "CaseSessionAct", new { id = id }), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.CaseSessionActMotivePdf,
                DestinationType = SourceTypeSelectVM.CaseSessionActMotivePdf,
                Location = userContext.CourtName,
                Reason = "Подписване на мотиви към съдебен протокол/акт",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url
            };
            var lu = taskService.GetLawUnitByTaskId(taskId);
            if (lu != null)
            {
                model.SignerName = lu.FullName;
                model.SignerUic = lu.Uic;
            }
            return View("_SignPdf", model);
        }

        public async Task<IActionResult> RemoveDepersonalizedFile(int actId, int sourceType)
        {
            var actFile = cdnService.Select(sourceType, actId.ToString()).FirstOrDefault();
            if (actFile != null)
            {
                if (await cdnService.MongoCdn_DeleteFile(actFile.FileId))
                {
                    switch (sourceType)
                    {
                        case SourceTypeSelectVM.CaseSessionActDepersonalized:
                            mqEpepService.AppendCaseSessionAct_Public(actId, EpepConstants.ServiceMethod.Delete);
                            SetSuccessMessage("Обезличения акт е премахнат успешно.");
                            break;
                        case SourceTypeSelectVM.CaseSessionActMotiveDepersonalized:
                            mqEpepService.AppendCaseSessionAct_PublicMotive(actId, EpepConstants.ServiceMethod.Delete);
                            SetSuccessMessage("Обезличените мотиви са премахнати успешно.");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    SetErrorMessage("Проблем при премахване на файл.");
                }
            }
            return RedirectToAction(nameof(Edit), new { id = actId });
        }

        private async Task<string> GenerateCustomActBlank(CaseSessionActPrintVM actModel)
        {
            var caseSessionActPrint = new CaseSessionActPrintVM();
            var caseSessionActCommand = new CaseSessionActCommandVM();
            if ((actModel.ActKindBlankName == "CProtection") || (actModel.ActKindBlankName == "CIProtection") || (actModel.ActKindBlankName == "ProtectiveOrder"))
                caseSessionActPrint = service.CaseSessionAct_GetForPrint(actModel.Id);
            else
                caseSessionActCommand = service.CaseSessionActCommand_GetForPrint(actModel.Id);

            switch (actModel.ActKindBlankName)
            {
                case "410money":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMoney410PrintBlank.cshtml", caseSessionActCommand, true);
                case "410moneyNew":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMoney410PrintBlankNew.cshtml", caseSessionActCommand, true);
                case "410item":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMovables410PrintBlank.cshtml", caseSessionActCommand, true);
                case "410itemNew":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMovables410PrintBlankNew.cshtml", caseSessionActCommand, true);
                case "417money":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMoney417PrintBlank.cshtml", caseSessionActCommand, true);
                case "417moneyNew":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMoney417PrintBlankNew.cshtml", caseSessionActCommand, true);
                case "417item":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMovables417PrintBlank.cshtml", caseSessionActCommand, true);
                case "417itemNew":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMovables417PrintBlankNew.cshtml", caseSessionActCommand, true);
                case "execlist410money":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_410MoneyExecutiveList.cshtml", caseSessionActCommand, true);
                case "execlist410item":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_410MovableExecutiveList.cshtml", caseSessionActCommand, true);
                case "execlist417money":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_417MoneyExecutiveList.cshtml", caseSessionActCommand, true);
                case "execlist417item":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_417MovableExecutiveList.cshtml", caseSessionActCommand, true);
                case "execlist410moneyNew":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_410MoneyExecutiveListNew.cshtml", caseSessionActCommand, true);
                case "execlist410itemNew":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_410MovableExecutiveListNew.cshtml", caseSessionActCommand, true);
                case "execlist417moneyNew":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_417MoneyExecutiveListNew.cshtml", caseSessionActCommand, true);
                case "execlist417itemNew":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_417MovableExecutiveListNew.cshtml", caseSessionActCommand, true);
                case "CProtection":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_CommandmentProtection.cshtml", caseSessionActPrint, true);
                case "CIProtection":
                    return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_CommandmentImmediatelyProtection.cshtml", caseSessionActPrint, true);
                case "ProtectiveOrder":
                    {
                        string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = actModel.Id.ToString() });
                        var decodedHtml = decodeBlank(html, string.Empty);
                        caseSessionActPrint.MainBody = decodedHtml.Body;
                        return await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_ProtectiveOrder.cshtml", caseSessionActPrint, true);
                    }
                default:
                    return "";
            }
        }

        public IActionResult SessionActCommandMoneyPrint(int actId)
        {
            actId = 127;
            var caseSessionActCommand = service.CaseSessionActCommand_GetForPrint(actId);
            return View("_410ExecutiveList", caseSessionActCommand);
        }

        public IActionResult SessionActCommandMovablesPrint(int actId)
        {
            actId = 127;
            var caseSessionActCommand = service.CaseSessionActCommand_GetForPrint(actId);
            return View("_410ExecutiveList", caseSessionActCommand);
        }

        [HttpPost]
        public IActionResult Act_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionAct, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var actModel = service.GetById<CaseSessionAct>(model.Id);
            if (service.SaveExpireInfo<CaseSessionAct>(model))
            {
                taskService.ExpireAllUnfinishedTasks(SourceTypeSelectVM.CaseSessionAct, model.Id);
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseSessionAct, model.Id);
                SetSuccessMessage(MessageConstant.Values.ActExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Preview", "CaseSession", new { id = actModel.CaseSessionId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        void SetViewBagDivorce(int actId, CaseSessionActDivorce model)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(actId);
            SetHelpFile(HelpFileValues.SessionAct);

            (List<SelectListItem> men, List<SelectListItem> women, List<PersonDataVM> personData) = casePersonService.GetCasePersonForDivorce(actId);
            ViewBag.CasePersonManId_ddl = men;
            ViewBag.CasePersonWomanId_ddl = women;
            ViewBag.CountryCode_ddl = nomService.GetCountriesWitoutBG_DDL();

            if (model != null)
            {
                if (men.Count > 0)
                {
                    model.CasePersonManId = int.Parse(men[0].Value);
                    DateTime? birthDay = personData.Where(x => x.Id == model.CasePersonManId).Select(x => x.BirthDay).FirstOrDefault();
                    if (birthDay != null)
                        model.BirthDayMan = (DateTime)birthDay;
                }
                if (women.Count > 0)
                {
                    model.CasePersonWomanId = int.Parse(women[0].Value);
                    DateTime? birthDay = personData.Where(x => x.Id == model.CasePersonWomanId).Select(x => x.BirthDay).FirstOrDefault();
                    if (birthDay != null)
                        model.BirthDayWoman = (DateTime)birthDay;
                }
            }
        }

        public IActionResult AddDivorce(int actId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActDivorce, null, AuditConstants.Operations.Append, actId))
            {
                return Redirect_Denied();
            }
            var divorce = service.GetDivorceByActId(actId);
            if (divorce == null)
            {
                var act = service.GetById<CaseSessionAct>(actId);
                var model = new CaseSessionActDivorce()
                {
                    CaseId = act.CaseId,
                    CourtId = act.CourtId,
                    CaseSessionActId = actId
                };
                SetViewBagDivorce(actId, model);
                return View(nameof(EditDivorce), model);
            }
            else
            {
                return RedirectToAction(nameof(EditDivorce), new { id = divorce.Id });
            }
        }

        public IActionResult EditDivorce(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActDivorce, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CaseSessionActDivorce>(id);
            SetViewBagDivorce(model.CaseSessionActId, null);
            return View(nameof(EditDivorce), model);
        }

        [HttpPost]
        public IActionResult EditDivorce(CaseSessionActDivorce model)
        {
            SetViewBagDivorce(model.CaseSessionActId, null);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditDivorce), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = service.CaseSessionActDivorce_SaveData(model);
            if (result == true)
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionActDivorce, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditDivorce), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(string.IsNullOrEmpty(errorMessage) == false ? errorMessage : MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditDivorce), model);
        }

        public async Task<IActionResult> PreviewRawDivorce(int id)
        {
            TinyMCEVM htmlModel = printDocumentService.FillHtmlTemplateCaseSessionActDivorce(id);
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true)
            {
                CustomSwitches = "--disable-smart-shrinking --margin-top 0mm --margin-right 0mm --margin-left 15mm --margin-bottom 0mm",
            }
            .GetByte(this.ControllerContext);
            return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "Divorce" + id.ToString() + ".pdf");
        }

        [HttpGet]
        public IActionResult GetDDL_CaseSessionAct(int caseId)
        {
            var model = service.GetDropDownList_CaseSessionAct(caseId);
            return Json(model);
        }

        public IActionResult IndexExecListReport()
        {
            ViewBag.ActKindId_ddl = service.GetActKindsByActType(NomenclatureConstants.ActType.ExecListPrivatePerson);
            var model = new CaseSessionActELSprFilterVM();
            model.DateFrom = DateTime.Now.AddDays(-7);
            model.DateTo = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        public IActionResult ListDataExecListSpr(IDataTablesRequest request, CaseSessionActELSprFilterVM model)
        {
            var data = service.CaseSessionActELSpr_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        private void SetViewbagIndexActReport()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.ActTypeId_ddl = nomService.GetDropDownList<ActType>();
            ViewBag.DocumentGroupId_ddl = nomService.GetDDL_DocumentGroupByDirection(DocumentConstants.DocumentDirection.Incoming);
            ViewBag.ActComplainResultId_ddl = nomService.GetDDL_ActComplainResult();
        }

        public IActionResult IndexActReport()
        {
            CaseSessionActReportFilterVM filter = new CaseSessionActReportFilterVM()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            SetViewbagIndexActReport();
            return View(filter);
        }

        [HttpPost]
        public IActionResult ListDataActReport(IDataTablesRequest request, CaseSessionActReportFilterVM model)
        {
            var data = service.CaseSessionActReport_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        void SetViewBagCompany(int actId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(actId);
            SetHelpFile(HelpFileValues.SessionAct);

            var act = service.GetByIdWithOtherData(actId);

            ViewBag.ActData = act.ActType.Label + " " + (act.ActDate != null ? (act.RegNumber + "/" + ((DateTime)act.RegDate).ToString("dd.MM.yyyy")) : "");
        }

        public IActionResult AddCompany(int actId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActCompany, null, AuditConstants.Operations.Append, actId))
            {
                return Redirect_Denied();
            }
            var company = service.GetCompanyByActId(actId);
            if (company == null)
            {
                var act = service.GetById<CaseSessionAct>(actId);
                var model = new CaseSessionActCompany()
                {
                    CaseId = act.CaseId ?? 0,
                    CourtId = act.CourtId ?? 0,
                    CaseSessionActId = actId
                };
                SetViewBagCompany(actId);
                return View(nameof(EditCompany), model);
            }
            else
            {
                return RedirectToAction(nameof(EditCompany), new { id = company.Id });
            }
        }

        public IActionResult EditCompany(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActCompany, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CaseSessionActCompany>(id);
            SetViewBagCompany(model.CaseSessionActId);
            return View(nameof(EditCompany), model);
        }

        [HttpPost]
        public IActionResult EditCompany(CaseSessionActCompany model)
        {
            SetViewBagCompany(model.CaseSessionActId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCompany), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = service.CaseSessionActCompany_SaveData(model);
            if (result == true)
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionActCompany, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCompany), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(string.IsNullOrEmpty(errorMessage) == false ? errorMessage : MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCompany), model);
        }

        [HttpGet]
        public IActionResult GetActTypesFromCaseByCase(int caseId, int SessionTypeId)
        {
            var model = service.GetActTypesFromCaseByCase(caseId, SessionTypeId);
            return Json(model);
        }

        public IActionResult ActFinalView(int caseId)
        {
            if (caseService.IsCaseRestricted(caseId))
            {
                var isUserRestrictedAccess = userContext.IsUserInRole(AccountConstants.Roles.RestrictedAccess);

                if (!isUserRestrictedAccess)
                {
                    return Redirect_Denied();
                }
            }

            var caseCase = service.GetById<Case>(caseId);
            ViewBag.CaseName = caseCase.RegNumber;
            var model = service.GetSessionActsFinal(caseId);
            return View(model);
        }
    }
}
