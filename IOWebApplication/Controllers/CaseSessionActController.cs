using DataTables.AspNet.AspNetCore;
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
using IOWebApplication.Infrastructure.Models.IndexService;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rotativa.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace IOWebApplication.Controllers
{
    public class CaseSessionActController : BaseController
    {
        private readonly ICaseSessionActService service;
        private readonly ICaseSessionActCoordinationService coordinationService;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICaseService caseService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly ICaseSessionService sessionService;
        private readonly ICaseSessionMeetingService sessionMeetingService;
        private readonly ICdnService cdnService;
        private readonly IWorkTaskService taskService;
        private readonly ICasePersonService casePersonService;
        private readonly IPrintDocumentService printDocumentService;
        private readonly IMQEpepService mqEpepService;
        private readonly ICaseSessionActComplainService caseSessionActComplainService;
        private readonly ICaseLifecycleService caseLifecycleService;
        private readonly ICourtDepartmentService courtDepartmentService;
        private readonly IElasticService elasticService;
        private readonly IBlankTemplateService blankTemplateService;
        private readonly ILogger logger;

        public CaseSessionActController(
            ICaseSessionActService _service,
            INomenclatureService _nomService,
            ICommonService _commonService,
            ICaseLawUnitService _caseLawUnitService,
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
            ICourtDepartmentService _courtDepartmentService,
            ICaseLifecycleService _caseLifecycleService,
            IElasticService _elasticService,
            IBlankTemplateService _blankTemplateService,
            ILogger<CaseSessionActController> logger
            )
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            sessionService = _sessionService;
            sessionMeetingService = _sessionMeetingService;
            cdnService = _cdnService;
            caseService = _caseService;
            caseLawUnitService = _caseLawUnitService;
            coordinationService = _coordinationService;
            taskService = _taskService;
            casePersonService = _casePersonService;
            printDocumentService = _printDocumentService;
            mqEpepService = _mqEpepService;
            caseSessionActComplainService = _caseSessionActComplainService;
            caseLifecycleService = _caseLifecycleService;
            courtDepartmentService = _courtDepartmentService;
            elasticService = _elasticService;
            blankTemplateService = _blankTemplateService;
            this.logger = logger;
        }

        public async Task<IActionResult> Index(int caseSessionId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Съдебни актове и протоколи");
            CaseSessionActFilterVM filter = new CaseSessionActFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31),
                IsFinalDoc = false
            };
            ViewBag.ActTypeIds_ddl = await nomService.GetDropDownListAsync<ActType>(false);
            ViewBag.CaseGroupIds_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.CourtDepartmentId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            SetHelpFile(HelpFileValues.CourtActsandProtocols);
            return View(filter);
        }

        public async Task<IActionResult> IndexForLawUnitCurrent()
        {
            CaseSessionActFilterVM filter = new CaseSessionActFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31),
                IsFinalDoc = false
            };
            ViewBag.ActTypeIds_ddl = await nomService.GetDropDownListAsync<ActType>(false);
            ViewBag.CaseGroupIds_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.CourtDepartmentId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
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

        [HttpPost]
        public IActionResult ListDataForlawUnitCurrentSpr(IDataTablesRequest request, CaseSessionActFilterVM model)
        {
            var data = service.CaseSessionActSpr_Select(userContext.CourtId, model, true);
            return request.GetResponse(data);
        }

        public async Task<IActionResult> Add(int caseSessionId, bool autoSave = false, int? prevActId = null, int? actTypeId = null, int? actKindId = null)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, null, AuditConstants.Operations.Append, caseSessionId))
            {
                return Redirect_Denied();
            }
            var caseSession = await service.GetByIdAsync<CaseSession>(caseSessionId);
            if (caseSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno && caseSession.DateFrom.Date > DateTime.Now.Date)
            {
                SetErrorMessage("Не можете да добавяте акт в насрочено заседание с бъдеща дата.");
                return RedirectToAction("Preview", "CaseSession", new
                {
                    id = caseSessionId
                });
            }

            if ((caseSession.SessionStateId != NomenclatureConstants.SessionState.Provedeno)
                && (caseSession.SessionStateId != NomenclatureConstants.SessionState.Nasrocheno))
            {
                SetErrorMessage("Заседанието не е със статус проведено.");
                return RedirectToAction("Preview", "CaseSession", new { id = caseSessionId });
            }

            var model = new CaseSessionActEditVM()
            {
                CaseSessionId = caseSessionId,
                CaseId = caseSession.CaseId,
                CourtId = userContext.CourtId,
                ActStateId = NomenclatureConstants.SessionActState.Project,
                GenerateExecProcess = true,
                CorrectedActsIds = new string[] { }
            };
            if (prevActId.HasValue)
            {
                model.RelatedActId = prevActId.Value;
                if (autoSave)
                {
                    ViewBag.autoSave = true;
                }
            }
            if (actTypeId.HasValue)
            {
                model.ActTypeId = actTypeId.Value;
            }
            if (actKindId.HasValue)
            {
                model.ActKindId = actKindId.Value;
            }
            await SetViewbag(model);
            return View(nameof(Edit), model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await service.ReadActById(id);
            if (model == null)
            {
                return NotFoundError("Търсеният от Вас акт не е намерен и/или нямате достъп до него.");
            }
            if (model.DateExpired != null)
            {
                return NotFoundError(MessageConstant.Values.ObjectWasDeleted);
            }
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, id, AuditConstants.Operations.Update, model.CaseSessionId))
            {
                return Redirect_Denied();
            }

            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            await SetViewbag(model);
            return View(nameof(Edit), model);
        }

        async Task SetViewbag(CaseSessionActEditVM model)
        {
            ViewBag.hasEditFinishDoc = true;
            ViewBag.canAccessFile = false;
            if (model.Id > 0)
            {
                var _coordinations = await coordinationService.CaseSessionActCoordination_Select(model.Id).ToListAsync();
                ViewBag.hasActCoordinations = await coordinationService.CaseSessionActCoordination_Select(model.Id, null, NomenclatureConstants.CoordinationTypes.Act).CountAsync() > 0;
                ViewBag.hasMotiveCoordinations = await coordinationService.CaseSessionActCoordination_Select(model.Id, null, NomenclatureConstants.CoordinationTypes.Motive).CountAsync() > 0;
                ViewBag.hasCoordination = ViewBag.hasActCoordinations || ViewBag.hasMotiveCoordinations;
                ViewBag.hasCoordinationWithOpinion = model.ActDeclaredDate.HasValue && _coordinations.Where(x => NomenclatureConstants.ActCoordinationTypes.WithOpinion.Contains(x.ActCoordinationTypeId)).Any();
                int[] actFilesSourceTypes = {
                                SourceTypeSelectVM.CaseSessionActPdf,
                                SourceTypeSelectVM.CaseSessionActDepersonalizedBlank,
                                SourceTypeSelectVM.CaseSessionActMotiveDepersonalizedBlank,
                                SourceTypeSelectVM.CaseSessionActMotivePdf,
                                SourceTypeSelectVM.CaseSessionActDepersonalized,
                                SourceTypeSelectVM.CaseSessionActMotiveDepersonalized
                                };
                var actFiles = await cdnService.Select(actFilesSourceTypes, model.Id.ToString()).ToListAsync();

                ViewBag.hasActPdf = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActPdf);
                ViewBag.hasDefacedBlank = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActDepersonalizedBlank);
                ViewBag.hasDefacedMotivesBlank = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActMotiveDepersonalizedBlank);
                ViewBag.hasDefacedAct = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActDepersonalized);
                ViewBag.hasDefacedMotives = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActMotiveDepersonalized);
                ViewBag.hasMotives = actFiles.Any(x => x.SourceType == SourceTypeSelectVM.CaseSessionActMotivePdf);
                ViewBag.hasEditFinishDoc = !caseLifecycleService.CaseLifecycle_IsExistLifcycleAfter(model.CaseId ?? 0, model.Id);
                var actEntity = model.Adapt<CaseSessionAct>();

                ViewBag.canAccessFile = (await service.CheckActAccess(model.Id, actEntity)).canAccess;
                ViewBag.canAccessDefaceAct = (await service.CheckActBlankAccess(actEntity, NomenclatureConstants.ActAccessMode.ActDefaceBlank)).canAccess;
                ViewBag.canAccessDefaceMotive = (await service.CheckActBlankAccess(actEntity, NomenclatureConstants.ActAccessMode.MotiveDefaceBlank)).canAccess;
                //Когато няма регистриран акт (няма изготвен файл по бланка) може да се прегледа съдържанието на бъдещия акт
                if (!actFiles.Any() && model.ActKindId > 0)
                {
                    ViewBag.quickViewEnabled = true;
                }
            }
            else
            {
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(model.CaseSessionId);

            var caseSession = await sessionService.CaseSessionByIdAsync(model.CaseSessionId);
            var caseCase = caseSession.Case;
            ViewBag.isFastProcess = caseCase.IsFastProcess ?? false;
            if (caseCase.IsFastProcess ?? false && model.Id < 1)
                model.AppealNotificationStartFastProcess = DateTime.Now.AddDays(7);

            ViewBag.CaseGroupId = caseCase.CaseGroupId;
            ViewBag.ActTypeId_ddl = service.GetActTypesByCase(model.CaseSessionId);
            ViewBag.ActDirectionId_ddl = service.GetActDirectionItems();
            ViewBag.ActResultId_ddl = caseSessionActComplainService.GetDropDownList_ActResultFromCaseSessionActComplainResult(model.Id);
            bool actStateInitial = string.IsNullOrEmpty(model.RegNumber);
            ViewBag.ActStateId_ddl = nomService.GetDDL_CaseSessionActState(actStateInitial, !actStateInitial, model.ActDeclaredDate != null);
            var secretaryListDDL = service.GetDDLSelect2_SecretaryList(model.CaseSessionId, model.Id);
            ViewBag.SecretaryUserId_ddl = secretaryListDDL;
            if (model.Id == 0 && secretaryListDDL.Count == 1)
            {
                var firstSecretaryUserId = secretaryListDDL.Select(x => x.Value).FirstOrDefault();
                if (firstSecretaryUserId != "-1")
                {
                    model.SecretaryUserId = firstSecretaryUserId;
                    model.SecretaryUserId_list = model.SecretaryUserId;
                }
            }


            ViewBag.RelatedActId_ddl = await service.GetDropDownList_CaseSessionActEnforced(model.CaseId ?? 0);
            ViewBag.CorrectedActsIds_ddl = ViewBag.RelatedActId_ddl;

            ViewBag.isDivorce = false;
            ViewBag.isISPNcase = caseCase.IsISPNcase == true;
            if (ViewBag.isISPNcase == true)
            {
                ViewBag.ActISPNReasonId_ddl = await nomService.GetDLL_ActIspnReasonByGroup(NomenclatureConstants.ActISPNReasonGroupings.CaseSessionAct_ISPN);
                ViewBag.ActISPNDebtorStateId_ddl = await nomService.GetDropDownListAsync<ActISPNDebtorState>();
            }
            ViewBag.isRNFLcase = caseCase.IspnKind == NomenclatureConstants.IspnKinds.Rnfl;
            if (ViewBag.isRNFLcase == true)
            {
                ViewBag.ActISPNReasonId_ddl = await nomService.GetDLL_ActIspnReasonByGroup(NomenclatureConstants.ActISPNReasonGroupings.CaseSessionAct_RNFL);
                if (model.RnflEffectiveImmediately == null)
                {
                    model.RnflEffectiveImmediately = true;
                }
            }
            ViewBag.hasTDActForRegistration = await nomService.CheckCaseFeature(model.CaseId ?? 0, NomenclatureConstants.CaseFeatures.ISPN_ActHasForRegistration);
            if (model.Id > 0)
            {
                int[] codeDivorce = nomService.GetCaseCodeGroupingByGroup(NomenclatureConstants.CaseCodeGroupings.Divorce);
                if (codeDivorce.Contains(caseCase.CaseCodeId ?? 0))
                {
                    var caseSessionAct = service.ReadById<CaseSessionAct>(model.Id);
                    if (NomenclatureConstants.SessionActState.EnforcedStates.Contains(caseSessionAct.ActStateId))
                        ViewBag.isDivorce = true;
                }
            }
            if ((model.SignJudgeLawUnitId ?? 0) <= 0)
            {
                model.SignJudgeLawUnitId = (await service.GetCaseLawUnitsByAct(model.Id, model.CaseSessionId))
                                            .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                            .Select(x => x.LawUnitId)
                                            .FirstOrDefault();
            }

            ViewBag.isRegisterCompany = false;
            if (caseCase.CaseGroupId == NomenclatureConstants.CaseGroups.Company)
            {
                ViewBag.isRegisterCompany = await caseService.IsRegisterCompany(caseCase.Id);
            }
            if (caseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
            {
                var hasResults = await sessionService.CaseSessionResult_Select(caseSession.Id).AnyAsync();
                if (!hasResults)
                {
                    ViewBag.sessionNoResults = true;
                }
            }
            var actComplainResults = await nomService.GetDDL_ActComplainResultAsync(caseCase.CaseTypeId);
            ViewBag.ActComplainResultId_ddl = actComplainResults;
            ViewBag.hasComplainResult = actComplainResults.Count > 1;
            var actComplainIndex = nomService.GetDDL_ActComplainIndex(caseCase.Id, model.Id);
            ViewBag.ActComplainIndexId_ddl = actComplainIndex;
            ViewBag.hasComplainIndex = actComplainIndex.Count > 1;
            ViewBag.hasActComplainResultRespect = actComplainResults.Any(x => x.Value == NomenclatureConstants.ActComplainResults.Respect.ToString());

            if (model.ActDeclaredDate != null)
            {
                ViewBag.canCorrectAfterDeclare = userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CaseSessionActCorrection);
            }

            if (NomenclatureConstants.ActType.ExecListActs.Contains(model.ActTypeId) && model.Id > 0)
            {
                string execProcessGid = await service.GetIntegrationKey(NomenclatureConstants.IntegrationTypes.EPEP, SourceTypeSelectVM.ExecProcessCaseSessionAct, model.Id);
                ViewBag.hasExecProcess = true;
            }

            setNextActUrl(model);
            SetHelpFile(HelpFileValues.SessionAct);
            model.CorrectedActsIds = model.CorrectedActsIds ?? new string[] { };
        }

        public IActionResult GetAllUsers()
        {
            var users = service.GetDDLSelect2_SecretaryList(0);
            return Json(users);
        }

        private void setNextActUrl(CaseSessionActEditVM model)
        {
            if (model.RegDate == null)
            {
                return;
            }
            var hasNextAct = service.GetByRelatedActId(model.Id) != null;
            if (hasNextAct)
            {
                return;
            }
            string actCode = $"{model.ActTypeId}";
            if (model.ActKindId > 0)
            {
                actCode += $",{model.ActKindId}";
            }
            var nextActCode = nomService.GetInnerCodeFromCodeMappingStr("act_nextact", actCode);
            if (nextActCode == null || string.IsNullOrEmpty(nextActCode.InnerCode))
            {
                return;
            }

            string[] nextActValues = nextActCode.InnerCode.Split(',');
            int nextActTypeId = int.Parse(nextActValues[0]);
            int nextActKindId = 0;
            ViewBag.nextActLabel = nextActCode.Description;
            ViewBag.nextActUrl = Url.Action(nameof(Add), new { caseSessionId = model.CaseSessionId, autoSave = true, prevActId = model.Id, actTypeId = nextActTypeId });
            if (nextActValues.Length > 1)
            {
                nextActKindId = int.Parse(nextActValues[1]);
                ViewBag.nextActUrl = Url.Action(nameof(Add), new { caseSessionId = model.CaseSessionId, autoSave = true, prevActId = model.Id, actTypeId = nextActTypeId, actKindId = nextActKindId });
            }
        }

        /// <summary>
        /// Метод зареждащ списък с ActKind
        /// </summary>
        /// <param name="actTypeId">Идентификатор на тип на акт</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        [DisableAudit]
        public IActionResult Get_ActKindsByActType(int actTypeId, int? caseId)
        {
            var model = service.GetActKindsByActType(actTypeId, caseId);
            return Json(model);
        }

        [DisableAudit]
        public async Task<IActionResult> Get_ActKindInfo(int actKindId)
        {
            var model = await service.GetReadonlyAsync<ActKind>(actKindId);
            if (model == null)
            {
                return Json(null);
            }
            return Json(new { model.ProcessType, model.MustSelectRelatedAct, actDirection = NomenclatureConstants.ActBlankNames.ActDirectionAlter.Contains(model.BlankName) });
        }

        private string IsValid(CaseSessionActEditVM model)
        {
            if (model.Id == 0)
            {
                var _sessionDateExpired = service.GetPropById<CaseSession, DateTime?>(model.CaseSessionId, x => x.DateExpired);
                if (_sessionDateExpired != null)
                {
                    return "Заседанието е изтрито. Проверете данните по делото.";
                }
            }
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
                var _caseTypeId = service.GetPropById<Case, int>(model.CaseId.Value, x => x.CaseTypeId);
                var actComplainResults = nomService.GetDDL_ActComplainResult(_caseTypeId);
                if ((model.ActComplainResultId < 1) && (actComplainResults.Count() > 1))
                {
                    return "Изберете резултат/степен на уважаване на иска";
                }
            }

            if ((model.ActInforcedDate.HasValue || model.ActStateId == NomenclatureConstants.SessionActState.ComingIntoForce) && model.ActDeclaredDate == null)
            {
                return "Актът не е постановен. Не можете да го промените в статус Влязъл в сила.";
            }
            if (model.ActDeclaredDate == null && model.ActStateId == NomenclatureConstants.SessionActState.Enforced)
            {
                return "Постановяването на актове се извършва с приключване на последната задача за подпис.";
            }
            if (model.ActKindId > 0 && model.ActDirectionId == NomenclatureConstants.ActBlankDirection.RightToLeft)
            {
                var actKindBlankName = service.GetPropById<ActKind, string>(model.ActKindId ?? 0, x => x.BlankName);
                if (!string.IsNullOrEmpty(actKindBlankName))
                    if (!NomenclatureConstants.ActBlankNames.ActDirectionAlter.Contains(actKindBlankName))
                    {
                        model.ActDirectionId = null;
                    }

            }
            else
            {
                model.ActDirectionId = null;
            }

            if (model.NotificationOn ?? false)
            {
                if ((model.NotificationDays ?? 0) < 1 &&
                    (model.NotificationMonts ?? 0) < 1 &&
                    (model.NotificationWeeks ?? 0) < 1)
                {
                    return "Маркиран е чек, да се създаде нотификация, а не е въведена стойност за период.";
                }
            }

            return string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CaseSessionActEditVM model)
        {

            if (!ModelState.IsValid)
            {
                await SetViewbag(model);
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                await SetViewbag(model);
                SetErrorMessage(_isvalid);
                return View(nameof(Edit), model);
            }

            if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.FearProtectsVineyard))
            {
                try
                {
                    service.ClearEntityTracker();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ClearEntityTrackerError");
                }
            }

            var currentId = model.Id;
            var saveResult = await service.CaseSessionAct_SaveData(model);
            if (saveResult.Result)
            {
                await SetAuditContextAsync(service, SourceTypeSelectVM.CaseSessionAct, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                if (saveResult.ReloadNeeded)
                {
                    SetErrorMessage(MessageConstant.Values.NewerDateWrt);
                    return RedirectToAction(nameof(Edit), new { id = model.Id });
                }
                else
                {
                    await SetViewbag(model);
                    SetErrorMessage(MessageConstant.Values.SaveFailed);
                    return View(nameof(Edit), model);
                }
            }

        }

        public async Task<IActionResult> Blank(int id)
        {
            var checkBlankInfo = await service.CheckActBlankAccess(id, NomenclatureConstants.ActAccessMode.ActBlank);
            if (!checkBlankInfo.canAccess)
            {
                if (!string.IsNullOrEmpty(checkBlankInfo.lawunitName))
                    SetErrorMessage($"По проекта на акта работи {checkBlankInfo.lawunitName}.");
                else
                    SetErrorMessage($"Нямате достъп до бланката.");
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            var actModel = await service.CaseSessionAct_GetForPrint(id);

            if (actModel.ActDeclaredDate != null)
            {
                if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CaseSessionActCorrection))
                {
                    SetErrorMessage("Съдебният акт е постановен. Не можете да извършвате корекция по него.");
                    return RedirectToAction("Edit", new { id = id });
                }
            }

            int sourceType = SourceTypeSelectVM.CaseSessionActBlank;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });


            var model = new BlankEditVM()
            {
                Title = "Изготвяне на съдебен акт",
                SourceType = sourceType,
                SourceId = id.ToString(),
                SessionName = userContext.GenHash(id, sourceType),
                HtmlHeader = await this.RenderViewAsync("ActHeader", actModel),
                //HtmlContent = html,
                //HtmlFooter = actModel.Dispositiv,
                FooterIsEditable = true,
                FooterIsHtml = true,
                FooterTitle = "Диспозитив",
                ReturnUrl = Url.Action(nameof(Edit), new { id }),
                HasPreviewButton = true,
                AutoSaveKey = $"actBlank{id}",
                BlankSourceType = SourceTypeSelectVM.CaseSessionAct,
                BlankSourceId = actModel.ActTypeId,
                BlankCaseId = actModel.CaseId,
                RelatedActId = actModel.RelatedActId
            };

            var blanks = await blankTemplateService.GetBlankTemplates(model.BlankSourceType, model.BlankSourceId, model.BlankCaseId);
            if (!blanks.Any())
            {
                model.BlankSourceType = 0;
            }


            var decodedHtml = decodeBlank(html, actModel.Dispositiv);
            model.HtmlContent = decodedHtml.Body;
            model.HtmlFooter = decodedHtml.Dispositive;

            await fillBlankByActKind(model, actModel, html);

            ViewBag.breadcrumbs = await commonService.Breadcrumbs_GetForCaseSessionActAsync(id);
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

        //[RequestSizeLimit(2147483648)]
        [HttpPost]
        public async Task<IActionResult> Blank(BlankEditVM model, string btnPreview = null)
        {


            int actId = 0;
            if (!int.TryParse(model.SourceId, out actId))
            {
                return Redirect_Denied();
            }

            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, actId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            if (!userContext.CheckHash(model))
            {
                return Redirect_Denied();
            }

            var checkBlankInfo = await service.CheckActBlankAccess(actId, NomenclatureConstants.ActAccessMode.ActBlank);
            if (!checkBlankInfo.canAccess)
            {
                if (!string.IsNullOrEmpty(checkBlankInfo.lawunitName))
                    SetErrorMessage($"По проекта на акта работи {checkBlankInfo.lawunitName}.");
                else
                    SetErrorMessage($"Нямате достъп до бланката.");

                return RedirectToAction(nameof(Edit), new { id = actId });
            }
            var htmlRequest = new CdnUploadRequest()
            {
                SourceType = model.SourceType,
                SourceId = model.SourceId,
                FileName = "draft.html",
                ContentType = NomenclatureConstants.ContentTypes.Html,
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(encodeBlank(model.HtmlContent, model.HtmlFooter)))
            };

            if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.FearProtectsVineyard))
            {
                try
                {
                    service.ClearEntityTracker();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ClearEntityTrackerError");
                }
            }

            if (await cdnService.MongoCdn_AppendUpdate(htmlRequest))
            {
                if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.FearProtectsVineyard))
                {
                    if (service.StopTrackingApplicationUser())
                    {
                        logger.LogError($"StopTrackingApplicationUser - CaseSessionAct.Blank {actId}");
                    }
                }
                await service.CaseSessionAct_SaveDispositiv(int.Parse(model.SourceId), convertToPlainBlank(model.HtmlFooter ?? ""), convertToPlainBlank(model.HtmlContent ?? ""));
                SaveLogOperation(this.ControllerName, "edit", $"Актуализирано съдържание на акт.", IO.LogOperation.Models.OperationTypes.Patch, actId);
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
            var checkBlankInfo = await service.CheckActBlankAccess(id, NomenclatureConstants.ActAccessMode.ActBlank);
            if (!checkBlankInfo.canAccess)
            {
                if (!string.IsNullOrEmpty(checkBlankInfo.lawunitName))
                    SetErrorMessage($"По проекта на акта работи {checkBlankInfo.lawunitName}.");
                else
                    SetErrorMessage($"Нямате достъп до бланката.");
                return RedirectToAction(nameof(Edit), new { id = id });
            }

            int sourceType = SourceTypeSelectVM.CaseSessionActBlankComplete;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            if (string.IsNullOrEmpty(html))
            {
                var actModel = await service.CaseSessionAct_GetForPrint(id);
                html = await GetActHTML(actModel);
                if (string.IsNullOrEmpty(html))
                {
                    return RedirectToAction(nameof(Edit), new { id = id });
                }
                await saveCompleteBlank(id.ToString(), html);
            }

            var model = new BlankEditVM()
            {
                Title = "Корекция на съдебен акт",
                SourceType = sourceType,
                SourceId = id.ToString(),
                SessionName = userContext.GenHash(id, sourceType),
                HtmlContent = html,
                FooterIsEditable = false,
                ReturnUrl = Url.Action(nameof(Edit), new { id }),
                HasPreviewButton = true,
                HasResetButton = true,
                BlankCaseId = await caseService.GetPropByIdAsync<CaseSessionAct, int>(x => x.Id == id, x => x.CaseId ?? 0),
                RelatedActId = await caseService.GetPropByIdAsync<CaseSessionAct, int?>(x => x.Id == id, x => x.RelatedActId)

            };
            var _sessionAct = service.ReadById<CaseSessionAct>(id);
            if (_sessionAct.RelatedActId > 0)
            {
                model.RelatedDocumentPreviewUrl = Url.Action("PreviewST", "Files", new { st = SourceTypeSelectVM.CaseSessionActPdf, si = _sessionAct.RelatedActId.ToString() });
            }
            ViewBag.breadcrumbs = await commonService.Breadcrumbs_GetForCaseSessionActAsync(id);
            SetHelpFile(HelpFileValues.SessionAct);

            return View("BlankEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> BlankComplete(BlankEditVM model, string btnPreview = null, string reset_mode = null)
        {
            int actId = 0;
            if (!int.TryParse(model.SourceId, out actId))
            {
                return Redirect_Denied();
            }

            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, actId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            if (!userContext.CheckHash(model))
            {
                return Redirect_Denied();
            }

            var checkBlankInfo = await service.CheckActBlankAccess(actId, NomenclatureConstants.ActAccessMode.ActBlank);
            if (!checkBlankInfo.canAccess)
            {
                if (!string.IsNullOrEmpty(checkBlankInfo.lawunitName))
                    SetErrorMessage($"По проекта на акта работи {checkBlankInfo.lawunitName}.");
                else
                    SetErrorMessage($"Нямате достъп до бланката.");
                return RedirectToAction(nameof(Edit), new { id = actId });
            }

            if (!string.IsNullOrEmpty(reset_mode))
            {
                await cdnService.MongoCdn_DeleteFiles(new CdnFileSelect() { SourceType = model.SourceType, SourceId = model.SourceId });
                SetSuccessMessage("Информацията в акта е обновена с данните по делото.");
                return RedirectToAction(nameof(BlankComplete), new { id = model.SourceId });
            }

            if (await saveCompleteBlank(model.SourceId, model.HtmlContent))
            {
                SaveLogOperation(this.ControllerName, "edit", $"Актуализирано съдържание на акт.", IO.LogOperation.Models.OperationTypes.Patch, actId);

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

        public async Task<IActionResult> html(int id)
        {
            var actModel = await service.CaseSessionAct_GetForPrint(id);

            var blankHtml = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = id.ToString() });

            var decodedHtml = decodeBlank(blankHtml, actModel.Dispositiv);
            actModel.MainBody = decodedHtml.Body;
            actModel.Dispositiv = decodedHtml.Dispositive;
            if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {
                actModel.Coordinations = await getCoordinationFooterHtml(actModel.Id, NomenclatureConstants.CoordinationTypes.Act);
            }
            var html = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "ActFormat.cshtml", actModel, true);

            return new ContentResult()
            {
                Content = html,
                ContentType = "text/html",
            };
        }

        private async Task<IActionResult> blankPreview(BlankEditVM model)
        {
            int caseSessionActId = int.Parse(model.SourceId);
            var actModel = await service.CaseSessionAct_GetForPrint(caseSessionActId);
            string html;

            if (model.SourceType == SourceTypeSelectVM.CaseSessionActBlankComplete)
            {
                html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = model.SourceType, SourceId = model.SourceId });
            }
            else
            {

                if (!string.IsNullOrEmpty(actModel.ActKindBlankName))
                {
                    var blankResult = await GenerateCustomActBlank(actModel);
                    if (!blankResult.Result)
                    {
                        SetErrorMessage(blankResult.ErrorMessage);
                        return RedirectToAction(nameof(Edit), new { id = caseSessionActId });
                    }
                    html = blankResult.Content;
                }
                else
                {
                    var blankHtml = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = caseSessionActId.ToString() });

                    var decodedHtml = decodeBlank(blankHtml, actModel.Dispositiv);
                    actModel.MainBody = decodedHtml.Body;
                    actModel.Dispositiv = decodedHtml.Dispositive;
                    if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
                    {
                        actModel.Coordinations = await getCoordinationFooterHtml(actModel.Id, NomenclatureConstants.CoordinationTypes.Act);
                    }
                    html = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "ActFormat.cshtml", actModel, true);
                }
            }

            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
            var contentDispositionHeader = new System.Net.Mime.ContentDisposition
            {
                Inline = true,
                FileName = "sessionActPreview.pdf"
            };
            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf);
        }

        private async Task<string> getCoordinationFooterHtml(int actId, int coordinationType)
        {
            var coordinations = await coordinationService.CaseSessionActCoordination_Select(actId, null, coordinationType).ToListAsync();
            string result = string.Empty;
            foreach (var coordination in coordinations.Where(c => NomenclatureConstants.ActCoordinationTypes.WithOpinion.Contains(c.ActCoordinationTypeId)))
            {
                string lawunitTypeText = "съдията";
                if (coordination.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
                {
                    lawunitTypeText = "съдебния заседател";
                }
                result += $"<p><b>Особено мнение на {lawunitTypeText} {coordination.CaseLawUnitName}</b></p>";
                result += coordination.Content + "<br/>";
            }
            return result;
        }

        public async Task<IActionResult> QuickView(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = new BlankEditVM()
            {
                Title = "Преглед на съдебен акт",
                SourceType = SourceTypeSelectVM.CaseSessionActBlank,
                SourceId = id.ToString(),
                FooterIsEditable = false,
                ReturnUrl = Url.Action(nameof(Edit), new { id }),
                HasPreviewButton = true,
                HasResetButton = true

            };

            return await blankPreview(model);
        }

        private async Task<IActionResult> blankMotivePreview(BlankEditVM model)
        {
            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = model.HtmlContent }, true).GetByte(this.ControllerContext);

            var contentDispositionHeader = new System.Net.Mime.ContentDisposition
            {
                Inline = true,
                FileName = "sessionActMotivePreview.pdf"
            };

            Response.Headers.Append(new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("Content-Disposition", contentDispositionHeader.ToString()));
            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf);
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
            var checkBlankInfo = await service.CheckActBlankAccess(id, NomenclatureConstants.ActAccessMode.MotiveBlank);
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
                //TODO: да се направи първоначално зареждане данните от дело/акта
            }

            var model = new BlankEditVM()
            {
                Title = "Изготвяне на мотиви към съдебен акт",
                SourceType = sourceType,
                SourceId = id.ToString(),
                SessionName = userContext.GenHash(id, sourceType),
                HtmlContent = html,
                HasPreviewButton = true,
                ReturnUrl = Url.Action(nameof(Edit), new { id }),
                AutoSaveKey = $"actMotives{id}"
            };
            ViewBag.breadcrumbs = await commonService.Breadcrumbs_GetForCaseSessionActAsync(id);
            SetHelpFile(HelpFileValues.SessionAct);

            return View("BlankEdit", model);
        }
        [HttpPost]
        public async Task<IActionResult> BlankMotives(BlankEditVM model, string btnPreview = null)
        {
            int actId = 0;
            if (!int.TryParse(model.SourceId, out actId))
            {
                return Redirect_Denied();
            }

            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, actId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            if (!userContext.CheckHash(model))
            {
                return Redirect_Denied();
            }

            var checkBlankInfo = await service.CheckActBlankAccess(actId, NomenclatureConstants.ActAccessMode.MotiveBlank);
            if (!checkBlankInfo.canAccess)
            {
                if (!string.IsNullOrEmpty(checkBlankInfo.lawunitName))
                    SetErrorMessage($"По проекта на акта работи {checkBlankInfo.lawunitName}.");
                else
                    SetErrorMessage($"Нямате достъп до бланката.");

                return RedirectToAction(nameof(Edit), new { id = actId });
            }


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
                SaveLogOperation(this.ControllerName, "edit", $"Актуализирано съдържание на мотиви към акт.", IO.LogOperation.Models.OperationTypes.Patch, actId);

                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            if (!string.IsNullOrEmpty(btnPreview))
            {
                return await blankMotivePreview(model);
            }
            return RedirectToAction(nameof(Edit), new { id = model.SourceId });
        }

        public async Task<IActionResult> ResetDepersonalizeAct(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            int[] depFilesSourceTypes = { SourceTypeSelectVM.CaseSessionActDepersonalizedBlank, SourceTypeSelectVM.CaseSessionActDepersonalized };
            var files = await cdnService.Select(depFilesSourceTypes, id.ToString()).ToListAsync();
            foreach (var item in files)
            {
                await cdnService.MongoCdn_DeleteFile(item.FileId);
                if (item.SourceType == SourceTypeSelectVM.CaseSessionActDepersonalized)
                {
                    await mqEpepService.AppendCaseSessionAct_Public(id, EpepConstants.ServiceMethod.Delete);
                }
            }
            SetSuccessMessage("Данните за акта са обновени от актуалната бланка.");
            SaveLogOperation(this.ControllerName, "edit", "Обновяване на бланка за обезличаване", IO.LogOperation.Models.OperationTypes.Patch, id);
            return RedirectToAction(nameof(DepersonalizeAct), new { id = id });
        }


        public async Task<IActionResult> DepersonalizeAct(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            var checkBlankInfo = await service.CheckActBlankAccess(id, NomenclatureConstants.ActAccessMode.ActDefaceBlank);
            if (!checkBlankInfo.canAccess)
            {
                SetErrorMessage($"Нямате достъп до бланката.");
                return RedirectToAction(nameof(Edit), new { id = id });
            }

            CurrentContext_SetOperation(AuditConstants.Operations.View);
            CurrentContext_SetObjectInfo(" - Обезличаване", true);

            await prepareDepersonalizedBlankAct(id);

            var caseId = service.GetPropById<CaseSessionAct, int?>(x => x.Id == id, x => x.CaseId);
            int sourceType = SourceTypeSelectVM.CaseSessionActDepersonalizedBlank;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            var model = new DepersonalizationModel()
            {
                SubmitAction = this.ActionName,
                CaseId = caseId ?? 0,
                SourceType = sourceType,
                SourceId = id.ToString(),
                //DocumentName = $"{actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate:dd.MM.yyyy}",
                DocumentContent = html,
                ResetUrl = Url.Action("ResetDepersonalizeAct", new { id = id }),
                CancelUrl = Url.Action("Edit", new { id = id })
            };
            ViewBag.breadcrumbs = await commonService.Breadcrumbs_GetForCaseSessionActAsync(id);
            SetHelpFile(HelpFileValues.SessionAct);

            return View("DepersonalizeDocument", model);
        }


        [HttpPost]
        public async Task<IActionResult> DepersonalizeAct(DepersonalizationModel model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, model.SourceIdInt, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            var checkBlankInfo = await service.CheckActBlankAccess(model.SourceIdInt, NomenclatureConstants.ActAccessMode.ActDefaceBlank);
            if (!checkBlankInfo.canAccess)
            {
                SetErrorMessage($"Нямате достъп до бланката.");
                return RedirectToAction(nameof(Edit), new { id = model.SourceIdInt });
            }


            CurrentContext_SetObjectInfo(" - Обезличаване", true);
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
                var actModel = await service.CaseSessionAct_GetForPrint(int.Parse(model.SourceId));
                if (actModel.ActDeclaredDate == null)
                {
                    SetErrorMessage("Не можете да финализирате обезличаването на акт, който не е постановен.");
                    return RedirectToAction(nameof(Edit), new { id = model.SourceId });
                }
                var pdfRequest = new CdnUploadRequest();
                var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = model.DocumentContent ?? "" }, true).GetByte(this.ControllerContext);

                pdfRequest.SourceType = SourceTypeSelectVM.CaseSessionActDepersonalized;
                pdfRequest.SourceId = model.SourceId;
                pdfRequest.FileName = $"{actModel.ActTypeName} {actModel.ActRegNumber}-{actModel.ActRegDate:dd.MM.yyyy}-ОФ.pdf";
                pdfRequest.ContentType = NomenclatureConstants.ContentTypes.Pdf;
                pdfRequest.Title = $"{actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate:dd.MM.yyyy} - обезличен";
                pdfRequest.FileContentBase64 = Convert.ToBase64String(pdfBytes);

                isOk &= await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            }

            if (isOk)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                var replaceItems = JsonTextSerializer.Deserialize<IEnumerable<DepersonalizationHistoryItem>>(model.DepersonalizationNewItems);
                //Само ако е финализаращо обезличаване
                caseService.SaveDataDepersonalizationHistory(model.CaseId, replaceItems, SourceTypeSelectVM.CaseSessionAct, int.Parse(model.SourceId), isFinal);
                await mqEpepService.AppendCaseSessionAct_Public(int.Parse(model.SourceId), EpepConstants.ServiceMethod.Add);
                var operName = isFinal ? "Публикуване на обезличен акт" : "Обезличаване на акт";
                SaveLogOperation(this.ControllerName, "edit", operName, IO.LogOperation.Models.OperationTypes.Patch, model.SourceId);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return RedirectToAction(nameof(Edit), new { id = model.SourceId });
        }

        public async Task<IActionResult> DoTask_SentForCoordinate(long id)
        {
            int actId = (int)(await taskService.GetPropByIdAsync<Infrastructure.Data.Models.Common.WorkTask, long>(x => x.Id == id, x => x.SourceId));
            if (await service.SendForCoordination_Init(actId, id, NomenclatureConstants.CoordinationTypes.Act))
            {
                SetSuccessMessage("Задачите за съгласуване са създадени успешно.");
                await taskService.CompleteTask(id);
            }
            return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
        }
        public async Task<IActionResult> DoTask_SentMotiveForCoordinate(long id)
        {
            int actId = (int)(await taskService.GetPropByIdAsync<Infrastructure.Data.Models.Common.WorkTask, long>(x => x.Id == id, x => x.SourceId));
            if (await service.SendForCoordination_Init(actId, id, NomenclatureConstants.CoordinationTypes.Motive))
            {
                SetSuccessMessage("Задачите за съгласуване на мотиви са създадени успешно.");
                await taskService.CompleteTask(id);
            }
            return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
        }

        public async Task<IActionResult> SentForSign(int actId)
        {
            if (CheckDoublePostback("sendsgn"))
            {
                return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
            }
            var rnflDebtorCheck = await service.CheckBeforeSignRNFLAct(actId);
            if (!rnflDebtorCheck.Result)
            {
                SetErrorMessage(rnflDebtorCheck.ErrorMessage);
                return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
            }
            var actModel = await service.CaseSessionAct_GetForPrint(actId);
            string actHTML = await GetActHTML(actModel);
            if (string.IsNullOrEmpty(actHTML))
            {
                SetErrorMessage("Моля, въведете данни за акта!");
                return RedirectToAction(nameof(Edit), new { id = actId });
            }
            var valResult = await Validate_SentForSign(actModel, actHTML);
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

            if (await taskService.CreateTask(newSendFormSignTask))
            {
                var taskId = newSendFormSignTask.Id;
                await PrepareSessionActPdfFile(actModel, actHTML);

                var taskInitResult = await service.SendForSign_Init(actId, taskId);
                if (taskInitResult.Result)
                {
                    SetSuccessMessage("Задачите за подписване са създадени успешно.");
                    await taskService.CompleteTask(taskId);
                }
                else
                {
                    SetErrorMessage(taskInitResult.ErrorMessage);
                    await taskService.RejectTask(taskId, taskInitResult.ErrorMessage);
                }
            }
            return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
        }

        public async Task<IActionResult> SentForSignMotives(int actId)
        {
            if (CheckDoublePostback("sendsgnm"))
            {
                return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
            }


            string motivesHtml = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect()
            {
                SourceType = SourceTypeSelectVM.CaseSessionActMotiveBlank,
                SourceId = actId.ToString()
            });
            if (string.IsNullOrEmpty(motivesHtml))
            {
                SetErrorMessage("Няма изготвени мотиви.");
                return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
            }

            var newSendForSignMotivesTask = new WorkTaskEditVM()
            {
                SourceType = SourceTypeSelectVM.CaseSessionAct,
                SourceId = actId,
                TaskTypeId = WorkTaskConstants.Types.CaseSessionActMotives_SentToSign,
                TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser
            };

            if (await taskService.CreateTask(newSendForSignMotivesTask))
            {
                switch (newSendForSignMotivesTask.SourceType)
                {
                    case SourceTypeSelectVM.CaseSessionAct:
                        string fileError = await PrepareSessionActMotivesPdfFile(actId);
                        if (string.IsNullOrEmpty(fileError))
                        {
                            if (await service.SendForSignMotives_Init(actId, newSendForSignMotivesTask.Id))
                            {
                                SetSuccessMessage("Задачите за подписване на мотиви са създадени успешно.");
                                await taskService.CompleteTask(newSendForSignMotivesTask.Id);
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
            return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
        }

        private async Task<SaveResultVM> Validate_SentForSign(CaseSessionActPrintVM actModel, string actHTML)
        {
            if (actModel.SessionStateId != NomenclatureConstants.SessionState.Provedeno)
            {
                if (NomenclatureConstants.ActType.CanSignBeforeSessionEnd.Contains(actModel.ActTypeId))
                {
                    if (actModel.SessionDate > DateTime.Now)
                    {
                        return new SaveResultVM(false, "Заседанието е с бъдеща дата.");
                    }
                }
                else
                {
                    return new SaveResultVM(false, "Заседанието не е проведено.");
                }
            }

            if (string.IsNullOrEmpty(actHTML))
            {
                return new SaveResultVM(false, "Няма изготвен акт.");
            }

            if (actModel.ActDeclaredDate != null)
            {
                if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CaseSessionActCorrection))
                {
                    return new SaveResultVM(false, "Съдебният акт е постановен. Не можете да извършвате корекция по него.", "expiretask");
                }
            }

            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, actModel.Id, AuditConstants.Operations.Update, actModel.CaseSessionId))
            {
                return new SaveResultVM(false, "Нямате достъп до акта.");
            }

            var hasUnfinishedCoordinations = await coordinationService.CaseSessionActCoordination_Select(actModel.Id)
                                                    .Where(x => x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.New)
                                                    .AnyAsync();
            if (hasUnfinishedCoordinations)
            {
                return new SaveResultVM(false, "Процесът по съгласуване на акта не е завършен.");
            }

            return new SaveResultVM(true);
        }

        public async Task<IActionResult> DoTask_SentForSign(long id)
        {
            var task = await taskService.ReadById(id);

            if (CheckDoublePostback("asgn"))
            {
                return RedirectToAction("Edit", "CaseSessionAct", new { id = (int)task.SourceId });
            }

            if (task.TaskStateId == WorkTaskConstants.States.Completed || task.DateCompleted != null)
            {
                if (task.DateCompleted < DateTime.Now.AddSeconds(-1))
                {
                    SetErrorMessage("Задачата вече е изпълнена успешно.");
                }
                else
                {
                    SetSuccessMessage("Задачите за подписване са създадени успешно.");
                }
                return RedirectToAction("Edit", "CaseSessionAct", new { id = (int)task.SourceId });
            }
            switch (task.SourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    int actId = (int)task.SourceId;
                    var actModel = await service.CaseSessionAct_GetForPrint(actId);
                    string actHTML = await GetActHTML(actModel);
                    if (string.IsNullOrEmpty(actHTML))
                    {
                        return RedirectToAction(nameof(Edit), new { id = actId });
                    }

                    var valResult = await Validate_SentForSign(actModel, actHTML);
                    if (!valResult.Result)
                    {
                        if (valResult.SaveMethod == "expiretask")
                        {
                            taskService.ExpireTasks(new[] { id }, "");
                        }

                        SetErrorMessage(valResult.ErrorMessage);
                        return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });

                    }

                    await PrepareSessionActPdfFile(actModel, actHTML);

                    using (var ts = taskService.BeginTransaction())
                    {
                        task = await taskService.ReadById(id);
                        if (task.TaskStateId == WorkTaskConstants.States.Completed || task.DateCompleted != null)
                        {
                            SetErrorMessage("Задачата вече е изпълнена успешно.");
                            return RedirectToAction("Edit", "CaseSessionAct", new { id = (int)task.SourceId });
                        }

                        var taskInitResult = await service.SendForSign_Init(actId, id);
                        if (taskInitResult.Result)
                        {
                            SetSuccessMessage("Задачите за подписване са създадени успешно.");
                            await taskService.CompleteTask(id);
                            ts.Commit();
                        }
                        else
                        {
                            SetErrorMessage(taskInitResult.ErrorMessage);
                        }
                    }

                    return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
                default:
                    return null;
            }
        }

        public async Task<IActionResult> DoTask_MotivesSentForSign(long id)
        {
            var task = await taskService.GetByIdAsync<WorkTask>(id);
            switch (task.SourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    var actId = (int)task.SourceId;
                    var actModel = service.ReadById<CaseSessionAct>(actId);
                    if (actModel.ActDeclaredDate == null)
                    {
                        SetErrorMessage("Не можете да подпишете мотивите преди да е постановен акта.");
                        return RedirectToAction("Edit", "CaseSessionAct", new { id = actId });
                    }
                    string fileError = await PrepareSessionActMotivesPdfFile(actId);
                    if (string.IsNullOrEmpty(fileError))
                    {
                        if (await service.SendForSignMotives_Init(actId, id))
                        {
                            SetSuccessMessage("Задачите за подписване на мотиви са създадени успешно.");
                            await taskService.CompleteTask(id);
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
            var actFiles = await cdnService.Select(SourceTypeSelectVM.CaseSessionAct, caseSessionActId.ToString()).ToListAsync();
            if (actFiles.Count() > 0)
            {
                var fileId = actFiles.FirstOrDefault().FileId;
                return RedirectToAction("Download", "Files", new { id = fileId });
            }

            var actModel = await service.CaseSessionAct_GetForPrint(caseSessionActId);


            var blankHtml = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = caseSessionActId.ToString() });
            var decodedBlank = decodeBlank(blankHtml, actModel.Dispositiv);
            actModel.MainBody = decodedBlank.Body;
            actModel.Dispositiv = decodedBlank.Dispositive;

            //actModel.MainBody = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = caseSessionActId.ToString() });
            string html = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "ActFormat.cshtml", actModel, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);

            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf, "sessionAct.pdf");
        }

        private async Task<string> GetActHTML(CaseSessionActPrintVM actModel)
        {
            string html = string.Empty;

            if (!string.IsNullOrEmpty(actModel.ActKindBlankName))
            {
                var blankResult = await GenerateCustomActBlank(actModel);
                if (!blankResult.Result)
                {
                    SetErrorMessage(blankResult.ErrorMessage);
                    return null;
                }
                html = blankResult.Content;
            }
            else
            {
                var blankHtml = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = actModel.Id.ToString() });
                var decodedBlank = decodeBlank(blankHtml, actModel.Dispositiv);
                actModel.MainBody = decodedBlank.Body;
                actModel.Dispositiv = decodedBlank.Dispositive;
                if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
                {
                    actModel.Coordinations = await getCoordinationFooterHtml(actModel.Id, NomenclatureConstants.CoordinationTypes.Act);
                }


                if (string.IsNullOrEmpty(actModel.MainBody) && string.IsNullOrEmpty(actModel.Dispositiv))
                {
                    return null;
                }
                html = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "ActFormat.cshtml", actModel, true);
            }

            return html;
        }

        //Преместено е в бланката на ИЛ и общата бланка ActHeader. Условията за показване са в самото View _ExecListExecProcessInfo
        ///// <summary>
        ///// Добавяне на Генериране на партира за заповед за бързо производство
        ///// </summary>
        ///// <param name="actHTML">HTML вид на акта</param>
        ///// <returns></returns>
        //private string FillElPartidaFastProcess(string actHTML)
        //{
        //    int _indexCenter = actHTML.IndexOf("<center>");
        //    if (_indexCenter == -1)
        //    {
        //        _indexCenter = actHTML.IndexOf("<h2>");
        //        _indexCenter = _indexCenter - 1;
        //    }
        //    else
        //        _indexCenter = _indexCenter + 8;
        //    return actHTML.Substring(0, _indexCenter) + "<div align=\"right\" style=\"font-size: 10.0pt;\"><b><i>ЕЛЕКТРОННА ПАРТИДА НА ИЛ</i></b><br /><br /> </div>" + actHTML.Substring(_indexCenter);
        //}

        private async Task<string> PrepareSessionActPdfFile(CaseSessionActPrintVM actModel, string actHTML)
        {
            string result = string.Empty;

            var blankComplete = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlankComplete, SourceId = actModel.Id.ToString() });
            if (!string.IsNullOrEmpty(blankComplete) && !string.IsNullOrEmpty(actModel.ActKindBlankName))
            {
                actHTML = blankComplete;
            }

            //if (NomenclatureConstants.ActType.ExecListActs.Contains(actModel.ActTypeId) && actModel.GenerateExecProcess)
            //    actHTML = FillElPartidaFastProcess(actHTML);

            byte[] pdfBytes = await (new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = actHTML }, true)).GetByte(this.ControllerContext);

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseSessionActPdf,
                SourceId = actModel.Id.ToString(),
                FileName = $"{actModel.ActTypeName} {actModel.ActRegNumber}-{actModel.ActRegDate}.pdf",
                ContentType = NomenclatureConstants.ContentTypes.Pdf,
                Title = $"{actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate}",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };

            await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            result = pdfRequest.FileId;


            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {
                var coordinations = await coordinationService.CaseSessionActCoordination_Select(actModel.Id).Where(x => (x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.AcceptWithOpinion) || (x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.DontAccept)).ToListAsync();
                foreach (var coordination in coordinations)
                {
                    string coordinationHtml = await this.RenderViewAsync("CoordinationBlank", coordination);
                    string lawunitTypeText = coordination.JudgeRoleLabel;
                    if (coordination.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
                    {
                        lawunitTypeText = "Заседател";
                    }

                    var pdfBytesCoordination = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = coordinationHtml }, true).GetByte(this.ControllerContext);
                    var pdfRequestCoordination = new CdnUploadRequest()
                    {
                        SourceType = SourceTypeSelectVM.CaseSessionActCoordinationPdf,
                        SourceId = coordination.Id.ToString(),
                        FileName = "sessionActCoordination.pdf",
                        ContentType = NomenclatureConstants.ContentTypes.Pdf,
                        Title = $"Особено мнение към {actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate} на {coordination.CaseLawUnitName},{lawunitTypeText}",
                        FileContentBase64 = Convert.ToBase64String(pdfBytesCoordination)
                    };

                    if (coordination.CoordinationDeclaredDate.HasValue)
                    {
                        var _coordinationModel = coordinationService.GetById<CaseSessionActCoordination>(coordination.Id);
                        _coordinationModel.CoordinationDeclaredDate = null;
                        _coordinationModel.DepersonalizeEndDate = null;
                        _coordinationModel.DepersonalizeUserId = null;
                    }
                    await cdnService.MongoCdn_AppendUpdate(pdfRequestCoordination);
                }
            }

            return result;
        }

        async Task prepareDepersonalizedBlankAct(int actId)
        {
            if (await cdnService.Select(SourceTypeSelectVM.CaseSessionActDepersonalizedBlank, actId.ToString()).AnyAsync())
            {
                return;
            }

            var actModel = await service.CaseSessionAct_GetForPrint(actId);
            string actHTML = await GetActHTML(actModel);
            var blankComplete = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlankComplete, SourceId = actModel.Id.ToString() });
            if (!string.IsNullOrEmpty(blankComplete) && !string.IsNullOrEmpty(actModel.ActKindBlankName))
            {
                actHTML = blankComplete;
            }

            //Преобразува unicode символи от вида &#x417; в съответната буква.
            string htmlALL = System.Net.WebUtility.HtmlDecode(await this.RenderPartialViewAsync("~/Views/Shared/", "CreatePdf.cshtml", new BlankEditVM() { HtmlContent = actHTML, AppendWatermarkforTest = false }, true));


            var dpRules = service.AutoDepersonalizeAct_GenerateRules(actModel.CaseId);
            caseService.SaveDataDepersonalizationHistory(actModel.CaseId, dpRules, SourceTypeSelectVM.CaseSessionAct, actModel.Id, false);
            //dpRules.AddRange(caseService.GetSimilarDepersonalizationHistory(actModel.CaseId)); 
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
            }
        }


        private async Task<string> PrepareSessionActMotivesPdfFile(int caseSessionActId)
        {
            var actModel = await service.CaseSessionAct_GetForPrint(caseSessionActId);

            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActMotiveBlank, SourceId = caseSessionActId.ToString() });
            if (string.IsNullOrEmpty(html))
            {
                return "Няма изготвени мотиви.";
            }
            if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {
                html += await getCoordinationFooterHtml(actModel.Id, NomenclatureConstants.CoordinationTypes.Motive);
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


            //var actFullModel = service.CaseSessionAct_GetFullInfo(caseSessionActId);
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
            var checkBlankInfo = await service.CheckActBlankAccess(id, NomenclatureConstants.ActAccessMode.MotiveDefaceBlank);
            if (!checkBlankInfo.canAccess)
            {
                SetErrorMessage($"Нямате достъп до бланката.");
                return RedirectToAction(nameof(Edit), new { id = id });
            }


            var actModel = await service.CaseSessionAct_GetForPrint(id);
            int sourceType = SourceTypeSelectVM.CaseSessionActMotiveDepersonalizedBlank;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            var model = new DepersonalizationModel()
            {
                SubmitAction = this.ActionName,
                SourceType = sourceType,
                CaseId = actModel.CaseId,
                SourceId = id.ToString(),
                DocumentName = $"Мотиви",
                DocumentContent = html,
                CancelUrl = Url.Action("Edit", new { id = id })
            };
            ViewBag.breadcrumbs = await commonService.Breadcrumbs_GetForCaseSessionActAsync(id);
            SetHelpFile(HelpFileValues.SessionAct);

            return View("DepersonalizeDocument", model);
        }

        [HttpPost]
        public async Task<IActionResult> DepersonalizeMotives(DepersonalizationModel model)
        {
            var checkBlankInfo = await service.CheckActBlankAccess(model.SourceIdInt, NomenclatureConstants.ActAccessMode.MotiveDefaceBlank);
            if (!checkBlankInfo.canAccess)
            {
                SetErrorMessage($"Нямате достъп до бланката.");
                return RedirectToAction(nameof(Edit), new { id = model.SourceIdInt });
            }

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

                var actModel = await service.CaseSessionAct_GetForPrint(int.Parse(model.SourceId));

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
                var replaceItems = JsonTextSerializer.Deserialize<IEnumerable<DepersonalizationHistoryItem>>(model.DepersonalizationNewItems);
                caseService.SaveDataDepersonalizationHistory(model.CaseId, replaceItems, SourceTypeSelectVM.CaseSessionActMotive, int.Parse(model.SourceId), isFinal);
                if (isFinal)
                {
                    mqEpepService.AppendCaseSessionAct_PublicMotive(int.Parse(model.SourceId), EpepConstants.ServiceMethod.Add);
                    SaveLogOperation(this.ControllerName, "edit", "Обезличаване на мотиви - публикувано", IO.LogOperation.Models.OperationTypes.Patch, model.SourceId);
                }
                else
                {
                    SaveLogOperation(this.ControllerName, "edit", "Обезличаване на мотиви", IO.LogOperation.Models.OperationTypes.Patch, model.SourceId);
                }
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return RedirectToAction(nameof(Edit), new { id = model.SourceId });
        }

        async Task prepareDepersonalizedCoordination(int coordinationId)
        {
            if (await cdnService.Select(SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedBlank, coordinationId.ToString()).AnyAsync())
            {
                return;
            }

            var coordinationModel = await coordinationService.CaseSessionActCoordination_Select(0, coordinationId).FirstOrDefaultAsync();

            string coordinationHtml = await this.RenderViewAsync("CoordinationBlank", coordinationModel);

            var actModel = await service.CaseSessionAct_GetForPrint(coordinationModel.CaseSessionActId);

            //Преобразува unicode символи от вида &#x417; в съответната буква.
            string htmlALL = System.Net.WebUtility.HtmlDecode(await this.RenderPartialViewAsync("~/Views/Shared/", "CreatePdf.cshtml", new BlankEditVM() { HtmlContent = coordinationHtml, AppendWatermarkforTest = false }, true));


            var dpRules = service.AutoDepersonalizeAct_GenerateRules(actModel.CaseId);
            caseService.SaveDataDepersonalizationHistory(actModel.CaseId, dpRules, SourceTypeSelectVM.CaseSessionAct, actModel.Id, false);
            if (!string.IsNullOrEmpty(actModel.ActRegDate))
            {
                //Само регистрирани актове получава бланка за обезличаване и файлове с особено мнение
                var defacedHTML = service.AutoDepersonalizeAct(dpRules, htmlALL);
                var defacedActCoordinationBlankRequest = new CdnUploadRequest()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedBlank,
                    SourceId = coordinationId.ToString(),
                    FileName = "draftCoordination.html",
                    ContentType = NomenclatureConstants.ContentTypes.Html,
                    FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(defacedHTML ?? ""))
                };
                await cdnService.MongoCdn_AppendUpdate(defacedActCoordinationBlankRequest);
            }
        }

        public async Task<PartialViewResult> GetDepersonalizeCoordination(int actId)
        {
            var model = await coordinationService.GetDepersonalizationInfo(actId);
            return PartialView("_DepersonalizeCoordination", model.Where(x => x.HasSignedPrivateFile));
        }
        public async Task<IActionResult> DepersonalizeCoordination(int id)
        {
            var coordinationModel = await coordinationService.CaseSessionActCoordination_Select(0, id).FirstOrDefaultAsync();

            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, coordinationModel.CaseSessionActId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            CurrentContext_SetOperation(AuditConstants.Operations.View);
            CurrentContext_SetObjectInfo(" - Обезличаване", true);

            await prepareDepersonalizedCoordination(id);

            int sourceType = SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedBlank;
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            var model = new DepersonalizationModel()
            {
                SubmitAction = this.ActionName,
                CaseId = coordinationModel.CaseId,
                SourceType = sourceType,
                SourceId = id.ToString(),
                DocumentContent = html,
                //ResetUrl = Url.Action("ResetDepersonalizeAct", new { id = id }),
                CancelUrl = Url.Action("Edit", new { id = coordinationModel.CaseSessionActId })
            };
            ViewBag.breadcrumbs = await commonService.Breadcrumbs_GetForCaseSessionActAsync(coordinationModel.CaseSessionActId);
            SetHelpFile(HelpFileValues.SessionAct);

            return View("DepersonalizeDocument", model);
        }



        [HttpPost]
        public async Task<IActionResult> DepersonalizeCoordination(DepersonalizationModel model)
        {
            bool isFinal = model.SaveMode == "finalize";
            bool isOk = false;
            var htmlRequest = new CdnUploadRequest();
            htmlRequest.SourceType = SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedBlank;
            htmlRequest.SourceId = model.SourceId;
            htmlRequest.FileName = "draft.html";
            htmlRequest.ContentType = NomenclatureConstants.ContentTypes.Html;
            htmlRequest.FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.DocumentContent ?? ""));
            var coordinationModel = await coordinationService.CaseSessionActCoordination_Select(0, model.SourceIdInt).FirstOrDefaultAsync();
            isOk = await cdnService.MongoCdn_AppendUpdate(htmlRequest);

            if (isFinal)
            {
                var pdfRequest = new CdnUploadRequest();

                int sourceType = SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedBlank;
                string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = model.SourceId.ToString() });

                var actModel = await service.CaseSessionAct_GetForPrint(coordinationModel.CaseSessionActId);

                var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html ?? "" }, true).GetByte(this.ControllerContext);

                pdfRequest.SourceType = SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedPdf;
                pdfRequest.SourceId = model.SourceId;
                pdfRequest.FileName = "sessionActCoordinationDepersonilized.pdf";
                pdfRequest.ContentType = NomenclatureConstants.ContentTypes.Pdf;
                pdfRequest.Title = $"Особено мнение към {actModel.ActTypeName} {actModel.ActRegNumber}/{actModel.ActRegDate} на {coordinationModel.CaseLawUnitName},{coordinationModel.JudgeRoleLabel} - обезличено";
                pdfRequest.FileContentBase64 = Convert.ToBase64String(pdfBytes);

                isOk &= await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            }

            if (isOk)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                var replaceItems = JsonTextSerializer.Deserialize<IEnumerable<DepersonalizationHistoryItem>>(model.DepersonalizationNewItems);
                caseService.SaveDataDepersonalizationHistory(model.CaseId, replaceItems, SourceTypeSelectVM.CaseSessionActCoordination, int.Parse(model.SourceId), isFinal);
                if (isFinal)
                {
                    mqEpepService.AppendAttachedDocument(SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedPdf, coordinationModel.Id, coordinationModel.CaseSessionActId, EpepConstants.ServiceMethod.Add);
                    //mqEpepService.AppendCaseSessionAct_PublicMotive(int.Parse(model.SourceId), EpepConstants.ServiceMethod.Add);
                    SaveLogOperation(this.ControllerName, "edit", $"Обезличаване на особено мнение на {coordinationModel.CaseLawUnitName} - публикувано", IO.LogOperation.Models.OperationTypes.Patch, coordinationModel.CaseSessionActId);
                }
                else
                {
                    SaveLogOperation(this.ControllerName, "edit", $"Обезличаване на особено мнение на {coordinationModel.CaseLawUnitName}", IO.LogOperation.Models.OperationTypes.Patch, coordinationModel.CaseSessionActId);
                }
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return RedirectToAction(nameof(Edit), new { id = coordinationModel.CaseSessionActId });
        }

        public async Task<IActionResult> RegisterAct(int id)
        {
            if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {
                SetErrorMessage(MessageConstant.Values.Req1ActRegisterDisabled);
                return RedirectToAction("Edit", new { id = id });
            }
            var _act = service.ReadById<CaseSessionAct>(id);
            if (_act == null)
            {
                return Redirect_Denied("Търсения от Вас обект не беше намерен!");
            }

            var registerResult = await service.CaseSessionAct_RegisterAct(_act, "RegisterAct button");
            if (registerResult.Result)
            {
                SetSuccessMessage("Актът е регистриран успешно.");
                this.SaveLogOperation(this.ControllerName, nameof(Edit), "Регистриране на акт", IO.LogOperation.Models.OperationTypes.Patch, id);
                var actModel = await service.CaseSessionAct_GetForPrint(id);
                string actHTML = await GetActHTML(actModel);
                if (!string.IsNullOrEmpty(actHTML))
                {
                    await PrepareSessionActPdfFile(actModel, actHTML);
                }
            }
            else
            {
                SetErrorMessage(registerResult.ErrorMessage);
            }

            return RedirectToAction("Edit", new { id = id });
        }

        public async Task<IActionResult> SendActForSign(int id, long taskId)
        {
            var _act = service.ReadById<CaseSessionAct>(id);
            if (_act == null)
            {
                return Redirect_Denied("Търсения от Вас обект не беше намерен!");
            }
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, _act.Id, AuditConstants.Operations.Update, _act.CaseSessionId))
            {
                return Redirect_Denied();
            }

            
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {

                var registerResult = await service.CaseSessionAct_RegisterAct(_act, "SendActForSign task");
                if (!registerResult.Result)
                {
                    SetErrorMessage(registerResult.ErrorMessage);
                    return RedirectToAction("Edit", new { id = id });
                }
                else
                {
                    if (registerResult.SaveMethod == NomenclatureConstants.CounterResults.Register)
                    {
                        return RedirectToAction(nameof(SendActForSign), new { id, taskId });
                    }
                }
                if (string.IsNullOrEmpty(_act.RegNumber) || _act.RegDate == null)
                {
                    SetErrorMessage("Проблем при регистриране на съдебен акт. Моля, опитайте отново по-късно.");
                    return RedirectToAction("Edit", new { id = id });
                }
            }


            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {
                var actPdfFile = await cdnService.Select(SourceTypeSelectVM.CaseSessionActPdf, id.ToString()).FirstOrDefaultAsync();
                if (actPdfFile == null || actPdfFile.SignituresCount == 0)
                {
                    //Ако файла няма подписи и датата на създаването на файла е по-стара от датата на регистриране на акта
                    if (actPdfFile == null || actPdfFile.DateUploaded < _act.RegDate || !actPdfFile.Title.Contains(_act.RegNumber))
                    {
                        var actModel = await service.CaseSessionAct_GetForPrint(id);
                        var actHTML = await GetActHTML(actModel);
                        await PrepareSessionActPdfFile(actModel, actHTML);
                        return RedirectToAction(nameof(SendActForSign), new { id, taskId });
                    }
                }
            }

            Uri urlSuccess = new Uri(Url.Action("Edit", "CaseSessionAct", new { id = id }), UriKind.Relative);
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
                ErrorUrl = url,
                WorkTaskId = taskId
            };

            var lu = taskService.GetLawUnitByTaskId(taskId);
            if (lu != null)
            {
                model.SignerName = lu.FullName;
                model.SignerUic = lu.Uic;
            }

            return View("_SignPdf", model);
        }
        public async Task<IActionResult> SendActForSignCoordination(int id, int coordinationId, long taskId)
        {
            var _act = service.ReadById<CaseSessionAct>(id);
            if (_act == null)
            {
                return Redirect_Denied("Търсения от Вас обект не беше намерен!");
            }
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {
                var registerResult = await service.CaseSessionAct_RegisterAct(_act, "SendActForSignCoordination");
                if (!registerResult.Result)
                {
                    SetErrorMessage(registerResult.ErrorMessage);
                    return RedirectToAction("Edit", new { id = id });
                }
                else
                {
                    if (!string.IsNullOrEmpty(registerResult.SaveMethod))
                    {
                        var actModel = await service.CaseSessionAct_GetForPrint(id);
                        var actHTML = await GetActHTML(actModel);
                        if (string.IsNullOrEmpty(actHTML))
                        {
                            return RedirectToAction(nameof(Edit), new { id = id });
                        }
                        await PrepareSessionActPdfFile(actModel, actHTML);
                        return RedirectToAction(nameof(SendActForSignCoordination), new { id, coordinationId, taskId });
                    }
                }
            }
            Uri urlSuccess = new Uri(Url.Action("Edit", "CaseSessionAct", new { id = id }), UriKind.Relative);
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
                ErrorUrl = url,
                WorkTaskId = taskId
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
            Uri urlSuccess = new Uri(Url.Action("Edit", "CaseSessionAct", new { id = id }), UriKind.Relative);
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
                ErrorUrl = url,
                WorkTaskId = taskId
            };
            var lu = taskService.GetLawUnitByTaskId(taskId);
            if (lu != null)
            {
                model.SignerName = lu.FullName;
                model.SignerUic = lu.Uic;
            }
            return View("_SignPdf", model);
        }

        public async Task<IActionResult> RemoveDepersonalizedFile(int sourceId, int sourceType)
        {
            var actFile = await cdnService.Select(sourceType, sourceId.ToString()).FirstOrDefaultAsync();
            var actId = sourceId;
            var coordinationName = "";
            if (sourceType == SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedPdf)
            {
                actId = await coordinationService.GetPropByIdAsync<CaseSessionActCoordination, int>(x => x.Id == sourceId, x => x.CaseSessionActId);
                var coordinationModel = await coordinationService.CaseSessionActCoordination_Select(actId, sourceId).FirstOrDefaultAsync();
                if (coordinationModel != null)
                {
                    coordinationName = coordinationModel.CaseLawUnitName;
                }
            }
            if (actFile != null)
            {
                if (await cdnService.MongoCdn_DeleteFile(actFile.FileId))
                {
                    switch (sourceType)
                    {
                        case SourceTypeSelectVM.CaseSessionActDepersonalized:
                            service.RemoveDepersonalizationInfo(sourceId);
                            await mqEpepService.AppendCaseSessionAct_Public(sourceId, EpepConstants.ServiceMethod.Delete);
                            SaveLogOperation(this.ControllerName, nameof(Edit), "Премахване на обезличен акт", IO.LogOperation.Models.OperationTypes.Patch, actId);
                            SetSuccessMessage("Обезличения акт е премахнат успешно.");
                            break;
                        case SourceTypeSelectVM.CaseSessionActMotiveDepersonalized:
                            service.RemoveMotiveDepersonalizationInfo(sourceId);
                            mqEpepService.AppendCaseSessionAct_PublicMotive(sourceId, EpepConstants.ServiceMethod.Delete);
                            SaveLogOperation(this.ControllerName, nameof(Edit), "Премахване на обезличени мотиви", IO.LogOperation.Models.OperationTypes.Patch, actId);
                            SetSuccessMessage("Обезличените мотиви са премахнати успешно.");
                            break;
                        case SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedPdf:
                            coordinationService.RemoveDepersonalizationInfo(sourceId);
                            mqEpepService.AppendAttachedDocument(SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedPdf, sourceId, actId, EpepConstants.ServiceMethod.Delete);
                            SaveLogOperation(this.ControllerName, nameof(Edit), $"Премахване на обезличено особено мнение на {coordinationName}", IO.LogOperation.Models.OperationTypes.Patch, actId);
                            SetSuccessMessage("Обезличеното особено мнение е премахнато успешно.");
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

        private async Task<SaveResultVM> GenerateCustomActBlank(CaseSessionActPrintVM actModel)
        {
            var caseSessionActPrint = new CaseSessionActPrintVM();
            var caseSessionActCommand = new CaseSessionActCommandVM();
            if ((actModel.ActKindBlankName == "CProtection") || (actModel.ActKindBlankName == "CIProtection") || (actModel.ActKindBlankName == "ProtectiveOrder"))
                caseSessionActPrint = await service.CaseSessionAct_GetForPrint(actModel.Id);
            else
            {
                caseSessionActCommand = await service.CaseSessionActCommand_GetForPrint(actModel.Id);
                if (caseSessionActCommand.CaseSessionActPrint?.CaseByDocumentRequest == true)
                {
                    if (caseSessionActCommand.FastProcessRequest == null)
                    {
                        return new SaveResultVM(false, "Няма въведени данни за заповедното производство!");
                    }
                }
            }

            string htmlResult = "";
            switch (actModel.ActKindBlankName)
            {
                case "CProtection":
                    htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_CommandmentProtection.cshtml", caseSessionActPrint, true);
                    break;
                case "CIProtection":
                    htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_CommandmentImmediatelyProtection.cshtml", caseSessionActPrint, true);
                    break;
                case "410moneyNew":
                    {
                        if (caseSessionActCommand.CaseSessionActPrint.CaseByDocumentRequest)
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMoney410NewPrintBlank.cshtml", caseSessionActCommand, true);
                        else
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMoney410PrintBlank.cshtml", caseSessionActCommand, true);
                    }
                    break;
                case "410itemNew":
                    {
                        if (caseSessionActCommand.CaseSessionActPrint.CaseByDocumentRequest)
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMovables410NewPrintBlank.cshtml", caseSessionActCommand, true);
                        else
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMovables410PrintBlank.cshtml", caseSessionActCommand, true);
                    }
                    break;
                case "417moneyNew":
                    {
                        if (caseSessionActCommand.CaseSessionActPrint.CaseByDocumentRequest)
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMoney417NewPrintBlank.cshtml", caseSessionActCommand, true);
                        else
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMoney417PrintBlank.cshtml", caseSessionActCommand, true);
                    }
                    break;
                case "417itemNew":
                    {
                        if (caseSessionActCommand.CaseSessionActPrint.CaseByDocumentRequest)
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMovables417NewPrintBlank.cshtml", caseSessionActCommand, true);
                        else
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActCommandMovables417PrintBlank.cshtml", caseSessionActCommand, true);
                    }
                    break;
                case "execlist410itemNew":
                    {
                        if (caseSessionActCommand.CaseSessionActPrint.CaseByDocumentRequest)
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_410MovableExecutiveListNew.cshtml", caseSessionActCommand, true);
                        else
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_410MovableExecutiveList.cshtml", caseSessionActCommand, true);
                    }
                    break;
                case "execlist410moneyNew":
                    {
                        if (caseSessionActCommand.CaseSessionActPrint.CaseByDocumentRequest)
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_410MoneyExecutiveListNew.cshtml", caseSessionActCommand, true);
                        else
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_410MoneyExecutiveList.cshtml", caseSessionActCommand, true);
                    }
                    break;
                case "execlist417itemNew":
                    {
                        if (caseSessionActCommand.CaseSessionActPrint.CaseByDocumentRequest)
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_417MovableExecutiveListNew.cshtml", caseSessionActCommand, true);
                        else
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_417MovableExecutiveList.cshtml", caseSessionActCommand, true);
                    }
                    break;
                case "execlist417moneyNew":
                    {
                        if (caseSessionActCommand.CaseSessionActPrint.CaseByDocumentRequest)
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_417MoneyExecutiveListNew.cshtml", caseSessionActCommand, true);
                        else
                            htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_417MoneyExecutiveList.cshtml", caseSessionActCommand, true);
                    }
                    break;
                case "ProtectiveOrder":
                    {
                        string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = actModel.Id.ToString() });
                        if (string.IsNullOrEmpty(html))
                        {
                            html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = actModel.RelatedActId?.ToString() });
                            var decodedHtml = decodeBlank(html, string.Empty);
                            caseSessionActPrint.MainBody = decodedHtml.Body;
                            caseSessionActPrint.Dispositiv = decodedHtml.Dispositive;
                        }
                        else
                        {
                            var decodedHtml = decodeBlank(html, string.Empty);
                            caseSessionActPrint.Dispositiv = decodedHtml.Body;
                        }

                        htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_ProtectiveOrder.cshtml", caseSessionActPrint, true);
                    }
                    break;
                case "InjunctionMoney410":
                    htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActInjunctionMoney410NewPrintBlank.cshtml", caseSessionActCommand, true);
                    break;
                case "InjunctionMoney417":
                    htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActInjunctionMoney417NewPrintBlank.cshtml", caseSessionActCommand, true);
                    break;
                case "InjunctionWithInstructionMoney410":
                    htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActInjunctionWithInstructionMoney410NewPrintBlank.cshtml", caseSessionActCommand, true);
                    break;
                case "InjunctionWithInstructionMoney417":
                    htmlResult = await this.RenderPartialViewAsync("~/Views/CaseSessionAct/", "_SessionActInjunctionWithInstructionMoney417NewPrintBlank.cshtml", caseSessionActCommand, true);
                    break;
                case "ACT_R 410/417":
                case "ACT_414а":
                case "ACT_IL 410":
                case "ACT_TAX 410/417":
                case "ACT_V 414":
                    {
                        TinyMCEVM htmlModel = await printDocumentService.GetActFastProcess(actModel.ActKindBlankName, caseSessionActCommand);
                        htmlResult = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
                    }
                    break;
            }
            return new SaveResultVM(true)
            {
                Content = htmlResult
            };
        }

        public async Task<IActionResult> SessionActCommandMoneyPrint(int actId)
        {
            actId = 127;
            var caseSessionActCommand = await service.CaseSessionActCommand_GetForPrint(actId);
            return View("_410ExecutiveList", caseSessionActCommand);
        }

        public async Task<IActionResult> SessionActCommandMovablesPrint(int actId)
        {
            actId = 127;
            var caseSessionActCommand = await service.CaseSessionActCommand_GetForPrint(actId);
            return View("_410ExecutiveList", caseSessionActCommand);
        }

        [HttpPost]
        public async Task<IActionResult> Act_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var actModel = service.ReadById<CaseSessionAct>(model.Id);
            if (actModel.ActDeclaredDate != null)
            {
                return Json(new { result = false, message = "Актът е постановен!" });
            }
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

        public async Task<IActionResult> AddDivorce(int actId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActDivorce, null, AuditConstants.Operations.Append, actId))
            {
                return Redirect_Denied();
            }
            var divorce = service.GetDivorceByActId(actId);
            if (divorce == null)
            {
                var act = service.ReadById<CaseSessionAct>(actId);
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

        public async Task<IActionResult> EditDivorce(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActDivorce, id, AuditConstants.Operations.Update))
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

            var divorce = service.GetById<CaseSessionActDivorce>(id);
            var fileName = $"Съобщение за прекратен гр. брак_{divorce.RegNumber}_{divorce.RegDate:dd.MM.yyyy}.pdf";

            return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, fileName);
        }

        /// <summary>
        /// Анулиране на документ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Divorce_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActDivorce, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }

            if (string.IsNullOrEmpty(model.DescriptionExpired))
            {
                return Json(new { result = false, message = MessageConstant.Values.DescriptionExpireRequired });
            }

            (bool result, string errorMessage) = service.CaseSessionActDivorce_SaveExpired(model);
            if (result)
            {
                var divorce = service.GetById<CaseSessionActDivorce>(model.Id);
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseSessionActDivorce, model.Id);
                SetSuccessMessage(MessageConstant.Values.DivorceExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Edit", new { id = divorce.CaseSessionActId }) });
            }
            else
            {
                return Json(new { result = false, message = errorMessage });
            }
        }

        [HttpGet]
        public IActionResult GetDDL_CaseSessionAct(int caseId)
        {
            var model = service.GetDropDownList_CaseSessionAct(caseId);
            return Json(model);
        }

        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexExecListReport()
        {
            ViewBag.ActKindId_ddl = service.GetActKindsByActType(NomenclatureConstants.ActType.ExecListPrivatePerson, null);
            var model = new CaseSessionActELSprFilterVM();
            model.DateFrom = DateTime.Now.AddDays(-7);
            model.DateTo = DateTime.Now;
            SetHelpFile(HelpFileValues.Report37);

            return View(model);
        }

        [HttpPost]
        public IActionResult ListDataExecListSpr(IDataTablesRequest request, CaseSessionActELSprFilterVM model)
        {
            var data = service.CaseSessionActELSpr_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        #region Справка за съдебни актове

        /// <summary>
        /// Справка за съдебни актове
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> IndexActReport()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за съдебни актове");
            CaseSessionActReportFilterVM filter = new CaseSessionActReportFilterVM()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            await SetViewbagIndexActReport();
            SetHelpFile(HelpFileValues.Report21);
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за справка за съдебни актове
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataActReport(IDataTablesRequest request, CaseSessionActReportFilterVM filter)
        {
            var data = service.CaseSessionActReport_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за справка за съдебни актове
        /// </summary>
        private async Task SetViewbagIndexActReport()
        {
            ViewBag.SessionResultId_ddl = await nomService.GetDDL_SessionResultAsync();
            ViewBag.ProcessPriorityId_ddl = await nomService.GetDropDownListAsync<ProcessPriority>();
            ViewBag.CaseGroupId_ddl = await nomService.GetDropDownListAsync<CaseGroup>();
            ViewBag.ActTypeId_ddl = await nomService.GetDropDownListAsync<ActType>();
            ViewBag.DocumentGroupId_ddl = await nomService.GetDDL_DocumentGroupByDirection(DocumentConstants.DocumentDirection.Incoming);
            ViewBag.ActComplainResultId_ddl = await nomService.GetDDL_ActComplainResult();
            ViewBag.ActStateId_ddl = await nomService.GetDropDownListAsync<ActState>();
        }

        #endregion

        void SetViewBagCompany(int actId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(actId);
            SetHelpFile(HelpFileValues.SessionAct);

            var act = service.GetByIdWithOtherData(actId);

            ViewBag.ActData = act.ActType.Label + " " + (act.ActDate != null ? (act.RegNumber + "/" + ((DateTime)act.RegDate).ToString("dd.MM.yyyy")) : "");
        }

        public async Task<IActionResult> AddCompany(int actId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActCompany, null, AuditConstants.Operations.Append, actId))
            {
                return Redirect_Denied();
            }
            var company = service.GetCompanyByActId(actId);
            if (company == null)
            {
                var act = service.ReadById<CaseSessionAct>(actId);
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

        public async Task<IActionResult> EditCompany(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActCompany, id, AuditConstants.Operations.Update))
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

            var caseCaseRegNumber = service.GetPropById<Case, string>(caseId, x => x.RegNumber);
            ViewBag.CaseName = caseCaseRegNumber;
            var model = service.GetSessionActsFinal(caseId);
            return View(model);
        }

        public IActionResult ActSearch()
        {
            var model = new SearchFilterModel();
            ViewBag.CaseGroupIds_ddl = nomService.GetDropDownList<CaseGroup>(false);
            ViewBag.ActTypeIds_ddl = nomService.GetDropDownList<ActType>(false);
            ViewBag.OtdelenieId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Otdelenie);
            ViewBag.JudicalCompositionId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            AddAuditInfo(AuditConstants.Operations.List, "Търсене на съдебни актове по съдържание", "", SourceTypeSelectVM.CaseSessionAct);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ActSearch_ListData(IDataTablesRequest request, SearchFilterModel filter)
        {
            filter.CourtId = userContext.CourtId;
            filter.Skip = request.Start;
            filter.Take = request.Length < 0 ? 1000000 : request.Length;
            var data = await elasticService.Search(filter);


            var dtResponse = DataTablesResponse.Create(request, data.TotalCount, data.TotalCount, data.Items, null);

            var settings = new System.Text.Json.JsonSerializerOptions()
            {
                PropertyNamingPolicy = new DataTablesResponseDataNamingPolicy()
            };

            return new JsonResult(dtResponse, settings);
        }

        [Authorize(Roles = AccountConstants.Roles.GlobalAdministrator)]
        public async Task<IActionResult> FixActDeclaration(int id)
        {
            var result = await service.FixActDeclaration(id);
            if (result.Result)
            {
                SetSuccessMessage("Актът е постановен успешно.");
            }
            else
            {
                SetErrorMessage(result.ErrorMessage);
            }
            return RedirectToAction(nameof(Edit), new { id });
        }

        /// <summary>
        /// Изтегляне на бланка от свързан акт
        /// </summary>
        /// <param name="actId"></param>
        /// <param name="actPart">1-Основно съдържание,2-Диспозитив</param>
        /// <returns></returns>
        public async Task<IActionResult> GetContentByActId(int actId, int actPart)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionAct, actId, AuditConstants.Operations.Update))
            {
                return null;
            }

            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlank, SourceId = actId.ToString() });

            if (string.IsNullOrEmpty(html))
            {
                html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActBlankComplete, SourceId = actId.ToString() });
            }

            var decodedHtml = decodeBlank(html, string.Empty);
            string result = string.Empty;
            switch (actPart)
            {
                case 1:
                    result = decodedHtml.Body;
                    break;
                case 2:
                    result = decodedHtml.Dispositive;
                    break;
            }
            return Content(result);
        }
    }
}
