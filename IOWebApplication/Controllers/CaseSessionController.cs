using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseSessionController : BaseController
    {
        private readonly ICaseSessionService service;
        private readonly ICaseLawUnitService lawUnitService;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICaseClassificationService classficationService;
        private readonly ICourtDepartmentService courtDepartmentService;
        private readonly ICaseSessionMeetingService caseSessionMeetingService;
        private readonly ICaseNotificationService caseNotificationService;
        private readonly ICaseSessionActService caseSessionActService;
        private readonly ICaseService caseService;

        public CaseSessionController(ICaseSessionService _service,
                                     INomenclatureService _nomService,
                                     ICommonService _commonService,
                                     ICaseClassificationService _classficationService,
                                     ICaseLawUnitService _lawUnitService,
                                     ICourtDepartmentService _courtDepartmentService,
                                     ICaseSessionMeetingService _caseSessionMeetingService,
                                     ICaseNotificationService _caseNotificationService,
                                     ICaseSessionActService _caseSessionActService,
                                     ICaseService _caseService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            classficationService = _classficationService;
            lawUnitService = _lawUnitService;
            courtDepartmentService = _courtDepartmentService;
            caseSessionMeetingService = _caseSessionMeetingService;
            caseNotificationService = _caseNotificationService;
            caseSessionActService = _caseSessionActService;
            caseService = _caseService;
        }

        public async Task<IActionResult> Index(int id)
        {
            var EISSPNumber = await service.GetPropByIdAsync<Case, string>(id, x => x.EISSPNumber);
            ViewBag.caseId = id;
            ViewBag.casenumber = EISSPNumber;
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSession, null, AuditConstants.Operations.View, id))
            {
                return Redirect_Denied();
            }
            return View();
        }

        /// <summary>
        /// Справка за заседания
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index_Spr()
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Съдебни заседания");

            ViewBag.CaseGroupIds_ddl = await nomService.GetDropDownListAsync<CaseGroup>(false);
            ViewBag.HallId_ddl = await commonService.GetDropDownList_CourtHall(userContext.CourtId);
            ViewBag.CaseSessionTypeIds_ddl = await nomService.GetDropDownListAsync<SessionType>(false);
            ViewBag.SessionResultIds_ddl = await nomService.GetDDL_SessionResultAsync();
            ViewBag.SessionStateId_ddl = await nomService.GetDropDownListAsync<SessionState>();
            ViewBag.CourtDepartmentId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            ViewBag.CourtDepartmentOtdelenieId_ddl = await courtDepartmentService.Department_SelectDDLAsync(userContext.CourtId, NomenclatureConstants.DepartmentType.Otdelenie);
            SetHelpFile(HelpFileValues.CourtHearings);

            CaseSessionFilterVM filter = new CaseSessionFilterVM();
            //filter.Year = DateTime.Now.Year;
            filter.DateFrom = new DateTime(DateTime.Now.Year, 1, 1).ForceStartDate();
            filter.DateTo = new DateTime(DateTime.Now.Year, 12, 31).ForceEndDate();
            return View(filter);
        }

        /// <summary>
        /// Метод за извличане на заседания към дело
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId)
        {
            var data = service.CaseSession_Select(caseId, null, null);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Метод за извличане на заседания за спраката за заседания
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSpr(IDataTablesRequest request, CaseSessionFilterVM model)
        {
            var data = service.CaseSessionSpr_Select(model);
            return request.GetResponse(data);
        }

        [HttpPost]
        public IActionResult ListDataSprExportExcel(CaseSessionFilterVM model)
        {
            var xlsBytes = service.ListDataSprExportExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "SessionReport.xlsx");
        }

        [HttpPost]
        public IActionResult ListDataSprCalendar(DateTime dateFrom, DateTime dateTo, int year, int caseSessionTypeId, int hallId, string secretaryUserId, string caseGroupIds_text,
                                                 string caseTypeIds_text, string regNumber, int sessionResultId, int sessionStateId, int judgeReporterId, int courtDepartmentId,
                                                 int courtDepartmentOtdelenieId, string caseSessionTypeIds_text, string sessionResultIds_text)
        {
            var model = new CaseSessionFilterVM()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                Year = year,
                CaseSessionTypeId = caseSessionTypeId,
                HallId = hallId,
                SecretaryUserId = secretaryUserId,
                CaseGroupIds_text = caseGroupIds_text,
                CaseTypeIds_text = caseTypeIds_text,
                RegNumber = regNumber,
                SessionResultId = sessionResultId,
                SessionStateId = sessionStateId,
                JudgeReporterId = judgeReporterId,
                CourtDepartmentId = courtDepartmentId,
                CourtDepartmentOtdelenieId = courtDepartmentOtdelenieId,
                CaseSessionTypeIds_text = caseSessionTypeIds_text,
                SessionResultIds_text = sessionResultIds_text
            };
            var calendarVMs = service.CaseSessionSprCalendar_Select(model).ToList();
            return Json(calendarVMs);
        }

        public IActionResult CaseSessionSprCalendar(DateTime dateFrom, DateTime dateTo, int year, int caseSessionTypeId, int hallId, string secretaryUserId, string caseGroupIds_text,
                                                    string caseTypeIds_text, string regNumber, int sessionResultId, int sessionStateId, int judgeReporterId, int courtDepartmentId,
                                                    int courtDepartmentOtdelenieId, string caseSessionTypeIds_text, string sessionResultIds_text)
        {
            var model = new CaseSessionFilterVM()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                Year = year,
                CaseSessionTypeId = caseSessionTypeId,
                HallId = hallId,
                SecretaryUserId = secretaryUserId,
                CaseGroupIds_text = caseGroupIds_text,
                CaseTypeIds_text = caseTypeIds_text,
                RegNumber = regNumber,
                SessionResultId = sessionResultId,
                SessionStateId = sessionStateId,
                JudgeReporterId = judgeReporterId,
                CourtDepartmentId = courtDepartmentId,
                CourtDepartmentOtdelenieId = courtDepartmentOtdelenieId,
                CaseSessionTypeIds_text = caseSessionTypeIds_text,
                SessionResultIds_text = sessionResultIds_text
            };

            return PartialView("_CaseSession_Spr_Calendar", model);
        }

        /// <summary>
        /// Проверка за съдия-докладчик към дело дали съществува
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private bool IsExistJudge(int caseId)
        {
            return lawUnitService.CaseLawUnit_Select(caseId, null).Any(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter);
        }

        /// <summary>
        /// Добавяне на заседание към дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="sessionDate"></param>
        /// <returns></returns>
        public async Task<IActionResult> Add(int caseId, DateTime? sessionDate = null)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSession, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }

            var model = new CaseSessionVM()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now,
                SessionStateId = NomenclatureConstants.SessionState.Nasrocheno,
            };
            //Следващия кръгъл час
            model.DateFrom = model.DateFrom.AddMinutes(-model.DateFrom.Minute).AddHours(1);

            if (sessionDate != null)
            {
                model.DateFrom = sessionDate.ForceStartDate().Value.AddHours(9);
            }

            await SetViewbag(0, caseId, null);
            SetHelpFile(HelpFileValues.CaseSession);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// редакция на заседание
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            var model = await service.CaseSessionVMById(id);
            if (model == null)
            {
                return NotFoundError("Търсеното от Вас заседание не е намерено и/или нямате достъп до него.");
            }
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSession, id, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }
            ViewBag.CaseSessionName = model.SessionTypeLabel + " " + model.DateFrom.ToString("dd.MM.yyyy");
            await SetViewbag(id, model.CaseId, model.Id);
            SetHelpFile(HelpFileValues.SessionMainData);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Зареждане на цялостен преглед на заседание с табове за различни елементи на заседанието
        /// </summary>
        /// <param name="id"></param>
        /// <param name="notifListTypeId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Preview(int id, int? notifListTypeId)
        {
            var model = await service.CaseSessionVMById(id);
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSession, model.Id, AuditConstants.Operations.View, model.CaseId))
            {
                return Redirect_Denied();
            }
            model.NotificationListTypeId = notifListTypeId;
            ViewBag.CaseSessionName = model.SessionTypeLabel + " " + model.DateFrom.ToString("dd.MM.yyyy");
            await SetViewbag(id, model.CaseId, model.Id, false);
            SetHelpFile(HelpFileValues.SessionMainData);

            return View(nameof(Preview), model);
        }

        async Task SetViewbag(int id, int caseId, int? caseSessionId, bool IsViewRowSessionBreadcrumbs = true)
        {
            var modelSession = new CaseSessionVM();
            DateTime? dtNow = DateTime.Now;

            if (id > 0)
            {
                modelSession = await service.CaseSessionVMById(id);
                ViewBag.IsExpired = modelSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno;
                dtNow = modelSession.DateWrt;
                //dtNow = modelSession.DateFrom;
            }
            else
                ViewBag.IsExpired = false;

            ViewBag.CaseSessionName = modelSession.SessionTypeLabel + " " + modelSession.DateFrom.ToString("dd.MM.yyyy");
            ViewBag.CourtHallId = modelSession.CourtHallId ?? 0;
            List<SelectListItem> sessionTypes = await nomService.GetDDL_SessionTypesByCase(caseId, (((id > 0) && (modelSession.DateFrom < DateTime.Now)) ? modelSession.SessionTypeId : (int?)null), true, false, dtNow);
            if (id > 0 && !sessionTypes.Any(t => t.Value == modelSession.SessionTypeId.ToString()))
            {
                var firstDateWrt = service.GetFirstHistoryDate<CaseSessionH>(id);
                sessionTypes = await nomService.GetDDL_SessionTypesByCase(caseId, (((id > 0) && (modelSession.DateFrom < DateTime.Now)) ? modelSession.SessionTypeId : (int?)null), true, false, firstDateWrt);
            }
            ViewBag.SessionTypeId_ddl = sessionTypes;


            ViewBag.SessionStateId_ddl = await nomService.GetDDL_SessionStateRoute(modelSession.SessionStateId);
            ViewBag.CourtHallId_ddl = await commonService.GetDropDownList_CourtHall(userContext.CourtId);
            ViewBag.CaseClassification_ddl = await classficationService.CaseClassification_Select(caseId, caseSessionId);
            ViewBag.DateTo_Minutes_ddl = await nomService.GetDDL_SessionDuration();

            if (caseSessionId > 0)
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(caseSessionId.Value, IsViewRowSessionBreadcrumbs);
            }
            else
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            }

            ViewBag.hasSubstitutions = lawUnitService.LawUnitSubstitution_SelectForSession(caseSessionId ?? 0)?.Any();
            ViewBag.hasSubstitutionsOutCase = (await lawUnitService.GetCaseSelectionProtokolSubstitution(caseSessionId ?? 0))?.Any();
            //ViewBag.hasSubstitutions = (lawUnitService.LawUnitSubstitution_SelectForSession(caseSessionId ?? 0) != null) ? lawUnitService.LawUnitSubstitution_SelectForSession(caseSessionId ?? 0).Any() : false;

            if (id == 0)
            {
                var caseModel = await service.GetReadonlyAsync<Case>(caseId);
                if (caseModel.CourtId == NomenclatureConstants.Courts.VKS && caseModel.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                {
                    ViewBag.VksLawunitChange_ddl = nomService.GetDDL_VksSessionLawunitChange();
                }
            }
        }

        private async Task SetViewbagAddSessionAndAct(int caseId)
        {
            ViewBag.SessionTypeId_ddl = await nomService.GetDDL_SessionTypesByCaseByGroupe(caseId, NomenclatureConstants.CaseSessionTypeGroup.PrivateSession, true, false, DateTime.Now);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            var caseCaseCaseTypeId = await service.GetPropByIdAsync<Case, int>(caseId, x => x.CaseTypeId);
            var actComplainResults = await nomService.GetDDL_ActComplainResultAsync(caseCaseCaseTypeId);
            ViewBag.ActComplainResultId_ddl = actComplainResults;
            ViewBag.hasComplainResult = actComplainResults.Count > 1;
            ViewBag.SessionResultId_ddl = await nomService.GetDDL_SessionResultFromRulesByCaseId(caseId);
            ViewBag.hasActComplainResultRespect = actComplainResults.Any(x => x.Value == NomenclatureConstants.ActComplainResults.Respect.ToString());
            var selectListItemsCaseSession = await service.GetDDL_CaseSessionAddAct(caseId);
            ViewBag.CaseSessionAddActId_ddl = selectListItemsCaseSession;
            ViewBag.hasSessionAddAct = selectListItemsCaseSession.Count > 1;
            ViewBag.RelatedActId_ddl = await caseSessionActService.GetDropDownList_CaseSessionActEnforced(caseId);
            ViewBag.hasTDActForRegistration = await nomService.CheckCaseFeature(caseId, NomenclatureConstants.CaseFeatures.ISPN_ActHasForRegistration);
            ViewBag.CorrectedActsIds_ddl = ViewBag.RelatedActId_ddl;

            var caseInfo = await service.GetPropByIdAsync<Case, dynamic>(caseId, x => new
            {
                x.IsISPNcase,
                x.IspnKind
            });
            ViewBag.isISPNcase = caseInfo.IsISPNcase == true;
            if (ViewBag.isISPNcase == true)
            {
                ViewBag.ActISPNReasonId_ddl = await nomService.GetDLL_ActIspnReasonByGroup(NomenclatureConstants.ActISPNReasonGroupings.CaseSessionAct_ISPN);
                ViewBag.ActISPNDebtorStateId_ddl = await nomService.GetDropDownListAsync<ActISPNDebtorState>();
            }
            ViewBag.isRNFLcase = caseInfo.IspnKind == NomenclatureConstants.IspnKinds.Rnfl;
            if (ViewBag.isRNFLcase == true)
            {
                ViewBag.ActISPNReasonId_ddl = await nomService.GetDLL_ActIspnReasonByGroup(NomenclatureConstants.ActISPNReasonGroupings.CaseSessionAct_RNFL);
            }
        }

        /// <summary>
        /// Запис на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(CaseSessionVM model)
        {
            model.DateFrom = model.DateFrom.MakeEndSeconds();
            model.DateTo = model.DateFrom.AddMinutes(model.DateTo_Minutes).MakeEndSeconds();
            SetHelpFile(HelpFileValues.SessionMainData);

            if (!ModelState.IsValid)
            {
                await SetViewbag(model.Id, model.CaseId, model.Id);
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                await SetViewbag(model.Id, model.CaseId, model.Id);
                SetErrorMessage(_isvalid);
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            var saveResult = await service.CaseSession_SaveData(model);
            if (saveResult.Result)
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSession, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);

                if ((!await service.IsExistCaseSessionResult(model.Id)) && (model.SessionStateId == NomenclatureConstants.SessionState.Provedeno))
                {
                    SetSuccessMessage(MessageConstant.Values.SaveOK + " Моля, добавете резултат от заседание.");
                    return RedirectToAction("AddResult", new { caseSessionId = model.Id });
                }
                else
                {
                    SetSuccessMessage(MessageConstant.Values.SaveOK);
                    return RedirectToAction(nameof(Preview), new { id = model.Id });
                }
            }
            else
            {
                if (saveResult.ReloadNeeded)
                {
                    SetErrorMessage(MessageConstant.Values.NewerDateWrt);
                    return RedirectToAction(nameof(Edit), new { id = model.Id });
                }
                SetErrorMessage(saveResult.ErrorMessage);
            }

            await SetViewbag(model.Id, model.CaseId, model.Id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Добавяне на заседание към дело и акт към заседанието
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddSessionAndAct(int caseId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSession, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }

            var model = new CaseSessionVM()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now.AddMinutes(-1),
                SessionTypeId = NomenclatureConstants.SessionType.ClosedSession,
                SessionStateId = NomenclatureConstants.SessionState.Provedeno,
                CaseLawUnitByCase = lawUnitService.GetCheckListCaseLawUnitByCase(caseId),
                ActCanAppeal = false,
                IsFinalDoc = false,
                IsMainResult = true,
                RnflEffectiveImmediately = true
            };

            //Следващия кръгъл час
            //model.DateFrom = model.DateFrom.AddMinutes(-model.DateFrom.Minute).AddHours(1);
            await SetViewbagAddSessionAndAct(caseId);
            return View(nameof(AddSessionAndAct), model);
        }

        [HttpPost]
        public async Task<IActionResult> AddSessionAndAct(CaseSessionVM model)
        {
            model.DateFrom = model.DateFrom.MakeEndSeconds();
            model.DateTo = model.DateFrom.AddMinutes(model.DateTo_Minutes).MakeEndSeconds();
            model.DateWrt = DateTime.Now;

            if (model.CaseSessionAddActId > 0)
            {
                model.SessionResultId = null;
            }

            ModelState["SessionStateId"].Errors.Clear();
            ModelState["SessionStateId"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;

            if (!ModelState.IsValid)
            {
                await SetViewbagAddSessionAndAct(model.CaseId);
                return View(nameof(AddSessionAndAct), model);
            }

            string _isvalid = IsValidSessionAndAct(model);
            if (_isvalid != string.Empty)
            {
                await SetViewbagAddSessionAndAct(model.CaseId);
                SetErrorMessage(_isvalid);
                return View(nameof(AddSessionAndAct), model);
            }

            var currentId = model.Id;
            var saveResult = await service.CaseSession_SaveData(model);
            if (saveResult.Result)
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSession, model.Id, currentId == 0);
                if (model.ActSaveId > 0)
                {
                    this.SaveLogOperation(true, model.Id, null, nameof(Edit));
                    this.SaveLogOperation(true, model.ActSaveId, null, nameof(Edit), nameof(CaseSessionAct));
                }
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                if (string.IsNullOrEmpty(model.ActSaveType))
                    return RedirectToAction("Edit", "CaseSessionAct", new { id = model.ActSaveId });
                else
                    return RedirectToAction("Blank", "CaseSessionAct", new { id = model.ActSaveId });
            }
            else
            {
                SetErrorMessage(saveResult.ErrorMessage);
            }

            await SetViewbagAddSessionAndAct(model.CaseId);
            return View(nameof(AddSessionAndAct), model);
        }

        private string IsValidSessionAndAct(CaseSessionVM model)
        {
            if (model.SessionTypeId <= 0)
                return "Няма избран вид заседание";

            if (model.DateFrom.Year < 2000)
                return "Няма въведена начална дата";
            else
            {
                //if (model.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                //{

                //    if (model.DateFrom > DateTime.Now)
                //        return "Не може да отразите проведено заседание с бъдеща дата/час.";
                //}

                //if (model.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno)
                //{
                //    if (model.DateFrom <= DateTime.Now)
                //    {
                //        return "Не може да насрочвате заседание с минала дата/час.";
                //    }
                //}

                var caseCaseRegDate = service.GetPropById<Case, DateTime>(model.CaseId, x => x.RegDate);

                if (model.DateFrom < caseCaseRegDate)
                    return "Не можете да насрочвате заседание с дата/час по-малка от дата/час на регистрация на делото";
            }

            //if (model.SessionStateId <= 0)
            //    return "Няма избран статус";

            if (model.ActTypeId <= 0)
                return "Няма избран тип акт";

            if (model.IsFinalDoc ?? false)
            {
                var _caseCaseTypeId = service.GetPropById<Case, int>(model.CaseId, x => x.CaseTypeId);
                var actComplainResults = nomService.GetDDL_ActComplainResult(_caseCaseTypeId);
                if ((model.ActComplainResultId < 1) && (actComplainResults.Count() > 1))
                {
                    return "Изберете резултат/степен на уважаване на иска";
                }
            }
            else
            {
                model.ActComplainResultId = null;
            }

            if (!lawUnitService.IsExistJudgeReporterByCase(model.CaseId, model.DateFrom))
            {
                return "Няма активен съдия докладчик";
            }

            if (model.ActTypeId == NomenclatureConstants.ActType.Injunction)
            {
                if (model.CaseLawUnitByCase != null)
                {
                    if (model.CaseLawUnitByCase.Count > 0)
                    {
                        if (!model.CaseLawUnitByCase.Any(x => x.Checked))
                        {
                            return "Няма избран съдия, който ще се произнесе по разпореждането";
                        }
                    }
                }
            }

            //if (model.CaseSessions != null)
            //{
            //    if (model.CaseSessions.Where(x => x.Checked).Count() > 1)
            //    {
            //        return "Може да бъде избрано само едно заседание";
            //    }
            //}

            var caseSessions = service.CaseSession_OldSelect(model.CaseId, null, null).ToList();
            if (caseSessions.Count > 0)
            {
                if (caseSessions.Any(x => x.DateFrom == model.DateFrom))
                {
                    return "Вече има заседание в това дело с тази начална дата/час";
                }
            }

            if ((model.SessionResultId > 0) && (model.DateFrom <= DateTime.Now))
            {
                var selectListItems = nomService.GetDDL_SessionResultBase(model.SessionResultId ?? 0, false, false, true, model.CaseId);
                if (selectListItems.Any())
                {
                    if (model.SessionResultBaseId < 1)
                        return "Няма избрано основание";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Валидация преди запис на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CaseSessionVM model)
        {
            if (model.Id < 1)
            {
                //if (!lawUnitService.IsExistLawUnitByCase(model.CaseId, model.DateFrom))
                //{
                //    return "Няма активен състав";
                //}

                if (!lawUnitService.IsExistJudgeReporterByCase(model.CaseId, model.DateFrom))
                {
                    return "Няма активен съдия докладчик";
                }
            }

            if (model.SessionTypeId < 0)
                return "Няма избран вид";

            if (model.DateFrom.Year < 2000)
                return "Няма въведена начална дата";

            var caseRegDate = service.GetPropById<Case, DateTime>(x => x.Id == model.CaseId, x => x.RegDate);

            if (model.DateFrom < caseRegDate)
                return "Не можете да насрочвате заседание с дата/час по-малка от дата/час на регистрация на делото";

            if (model.CourtHallId > 0)
            {
                if (model.DateTo_Minutes < 1)
                    return "Изберете прогнозна продължителност";
            }

            if (model.DateTo != null)
            {
                if (model.DateFrom > model.DateTo)
                    return "Началната дата/час е по-голяма от крайната дата/час";
            }

            if (model.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
            {

                if (model.DateFrom > DateTime.Now)
                    return "Не може да отразите проведено заседание с бъдеща дата/час.";

                if (caseSessionMeetingService.IsExistMeetengInSessionAfterDate(DateTime.Now, model.Id, null))
                {
                    return "Има сесии в това заседание, които не са проведени.";
                }

                if (!lawUnitService.IsExistJudgeReporterByCase(model.CaseId, model.DateFrom))
                {
                    return "Няма активен съдия докладчик";
                }
            }
            var caseSessionOld = (model.Id > 0) ? service.GetReadonly<CaseSession>(model.Id) : new CaseSession();

            if (model.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno)
            {
                if (model.Id > 0 && NomenclatureConstants.SessionType.OpenSessionsForPastSessions.Contains(caseSessionOld.SessionTypeId))
                {

                }
                else
                {
                    if (model.DateFrom <= DateTime.Now)
                    {
                        return "Не може да насрочвате/коригирате заседание с минала дата/час.";
                    }
                }
            }

            var caseSessions = service.CaseSession_OldSelect(model.CaseId, null, null).ToList();
            if (caseSessions.Count > 0)
            {
                if (caseSessions.Any(x => (x.DateFrom == model.DateFrom) &&
                                          ((model.Id > 0) ? x.Id != model.Id : true) &&
                                          (x.SessionStateId != NomenclatureConstants.SessionState.Cancel)))
                {
                    return "Вече има заседание в това дело с тази начална дата/час";
                }
            }

            if (model.Id > 0)
            {
                var caseSessionMeeting = caseSessionMeetingService.CaseSessionMeetingAutoCreateGetBySessionId(model.Id);
                if (caseSessionMeeting != null)
                {
                    if (caseSessionMeetingService.IsExistMeetengInSession(model.DateFrom, (model.DateTo ?? model.DateFrom), model.Id, caseSessionMeeting.Id))
                    {
                        return "Има сесия в това заседание съвпадаща като време с автоматичната сесия";
                    }
                }

                if (model.DateFrom != caseSessionOld.DateFrom)
                {
                    if (caseNotificationService.IsExistNotificationForSession(model.Id))
                    {
                        return "Има издадени призовки за това заседание и не може да смените началната дата или залата.";
                    }
                }

                if (model.SessionStateId != NomenclatureConstants.SessionState.Nasrocheno)
                {
                    if (model.SessionTypeId != caseSessionOld.SessionTypeId)
                    {
                        return "Не може да промените вида на заседанието при статус различен от насрочено.";
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Метод за извличане на резултати към заседание
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataResult(IDataTablesRequest request, int caseSessionId)
        {
            var data = service.CaseSessionResult_Select(caseSessionId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на резултат
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddResult(int caseSessionId, int CallFromActId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionResult, null, AuditConstants.Operations.Append, caseSessionId))
            {
                return Redirect_Denied();
            }

            var caseSession = await service.CaseSessionVMByIdAsync(caseSessionId);
            var model = new CaseSessionResultEditVM()
            {
                CaseId = caseSession.CaseId,
                CourtId = caseSession.CourtId,
                CaseSessionId = caseSessionId,
                IsActive = true,
                IsMain = !service.IsExistMainResult(caseSessionId),
                CaseLawUnitByCase = await lawUnitService.GetCheckListCaseLawUnitByCaseAllAsync(caseSession.CaseId),
                CallFromActId = CallFromActId
            };

            await SetViewbagResult(caseSessionId);
            return View(nameof(EditResult), model);
        }

        private async Task SetViewbagResult(int caseSessionId)
        {
            ViewBag.SessionResultId_ddl = await nomService.GetDDL_SessionResultFromRulesAsync(caseSessionId);
            var caseSession = await service.CaseSessionVMByIdAsync(caseSessionId);
            var caseCase = await caseService.GetCaseInfo(caseSession.CaseId);
            ViewBag.CaseName = caseCase.CaseTypeCodeShortNumberRegDate;
            ViewBag.CaseSessionName = caseSession.SessionTypeLabel + " " + caseSession.DateFrom.ToString("dd.MM.yyyy");
            ViewBag.caseId = caseCase.Id;
            ViewBag.caseSessionId = caseSession.Id;
            SetHelpFile(HelpFileValues.SessionMainData);
        }

        /// <summary>
        /// Редакция на резултат
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditResult(int id, int CallFromActId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionResult, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            var model = await service.GetSessionResultEditVMByIdAsync(id);
            model.CallFromActId = CallFromActId;

            await SetViewbagResult(model.CaseSessionId);
            return View(nameof(EditResult), model);
        }

        /// <summary>
        /// Валидация преди запис на резултат
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidResult(CaseSessionResultEditVM model)
        {
            if (model.SessionResultId < 0)
                return "Няма избран резултат";

            var selectListItems = nomService.GetDDL_SessionResultBase(model.SessionResultId, false, false, true, model.CaseId);
            if (selectListItems.Any())
            {
                if (model.SessionResultBaseId < 1)
                    return "Няма избрано основание";
            }

            if (model.IsMain)
            {
                if (service.IsExistMainResult(model.CaseSessionId, model.Id))
                    return "Вече има избран основен резултат";
            }

            if (model.Id == 0)
            {
                if (NomenclatureConstants.CaseSessionResult.ActZaOtvod.Contains(model.SessionResultId))
                {
                    if (model.CaseLawUnitByCase == null)
                        return "Няма избран съдия";
                    else
                    {
                        if (!model.CaseLawUnitByCase.Any(x => x.Checked))
                            return "Няма избран съдия";

                        if (model.CaseLawUnitByCase.Where(x => x.Checked).Count() > 1)
                            return "Избрани са повече от 1 съдия.";

                        var check = model.CaseLawUnitByCase.Where(x => x.Checked).FirstOrDefault();
                        model.CaseLawUnitSelectId = int.Parse(check.Value);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на резултат
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditResult(CaseSessionResultEditVM model)
        {
            await SetViewbagResult(model.CaseSessionId);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValidResult(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditResult), model);
            }

            var currentId = model.Id;
            if (await service.CaseSessionResult_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionResult, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);

                if (NomenclatureConstants.CaseSessionResult.ActZaOtvod.Contains(model.SessionResultId))
                {
                    if (currentId == 0)
                    {
                        if (model.CaseLawUnitByCase != null)
                        {
                            var check = model.CaseLawUnitByCase.Where(x => x.Checked).FirstOrDefault();
                            if (check != null)
                                return RedirectToAction("AddEditDismisal", "CaseLawUnit", new { lawUnitId = int.Parse(check.Value) });
                        }
                    }
                }

                if (model.CallFromActId > 0)
                    return RedirectToAction("Edit", "CaseSessionAct", new { id = model.CallFromActId });
                else
                    return RedirectToAction(nameof(Preview), new { id = model.CaseSessionId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(EditResult), model);
        }

        /// <summary>
        /// Създаване на заседание с данни от избрано заседание
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> CopySession(int id)
        {
            var model = await service.CaseSessionVMById(id);
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSession, null, AuditConstants.Operations.Append, model.CaseId))
            {
                return Redirect_Denied();
            }
            if (model == null)
            {
                return NotFoundError("Търсеното от Вас заседание не е намерено и/или нямате достъп до него.");
            }

            await SetViewbag(model.Id, model.CaseId, model.Id);
            SetHelpFile(HelpFileValues.SessionMainData);
            return View(nameof(CopySession), model);
        }

        /// <summary>
        /// Запис при копиране на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CopySession(CaseSessionVM model)
        {
            await SetViewbag(model.Id, model.CaseId, model.Id);
            SetHelpFile(HelpFileValues.SessionMainData);
            if (!ModelState.IsValid)
            {
                return View(nameof(CopySession), model);
            }

            model.DateFrom = model.DateFrom.MakeEndSeconds();
            var caseSessions = await service.CaseSession_OldSelect(model.CaseId, null, null).ToListAsync();
            if (caseSessions.Count > 0)
            {
                if (caseSessions.Any(x => x.DateFrom == model.DateFrom))
                {
                    SetErrorMessage("Вече има заседание в това дело с тази начална дата и час");
                    return View(nameof(CopySession), model);
                }
            }

            var caseCase = await service.GetByIdAsync<Case>(model.CaseId);

            if (model.DateFrom < caseCase.RegDate)
            {
                SetErrorMessage("Не можете да насрочвате заседание с дата/час по-малка от дата/час на регистрация на делото");
                return View(nameof(CopySession), model);
            }

            var currentId = model.Id;

            if (await service.CaseSession_CopyData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSession, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Preview), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(CopySession), model);
        }

        /// <summary>
        /// Справка за заетост на зали по заседание/сесии
        /// </summary>
        /// <param name="HallId"></param>
        /// <returns></returns>
        public async Task<IActionResult> CaseSessionHallUseSpr(int HallId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Търсене в списъчен екран Заетост на зали");
            ViewBag.CourtHallId_ddl = await commonService.GetDropDownList_CourtHall(userContext.CourtId);
            var model = new CaseSessionHallUseFilterVM();
            model.DateFrom = NomenclatureExtensions.ForceStartDate(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
            model.DateTo = NomenclatureExtensions.ForceEndDate(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            model.IsCalendar = false;
            if (HallId > 0) model.CourtHallId = HallId;
            SetHelpFile(HelpFileValues.HallOcupancy);
            return View(model);
        }

        /// <summary>
        /// Извличане на данни за заетост на зали
        /// </summary>
        /// <param name="request"></param>
        /// <param name="CourtHallId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseSessionHallUseSpr(IDataTablesRequest request, int? CourtHallId, DateTime? DateFrom, DateTime? DateTo, int? JudgeReporterId)
        {
            var data = service.CaseSessionHallUse_Select(userContext.CourtId, CourtHallId, DateFrom, DateTo, JudgeReporterId);
            return request.GetResponse(data);
        }

        [HttpPost]
        public IActionResult ListDataCaseSessionHallUseCalendarSpr(int? CourtHallId, DateTime? DateFrom, DateTime? DateTo)
        {
            var model = service.CaseSessionHallUseCalendar_Select(userContext.CourtId, CourtHallId, DateFrom, DateTo);
            return Json(model);
        }

        public IActionResult CaseSessionCalendar(int? CourtHallId, DateTime? dateFrom, DateTime? dateTo)
        {
            var model = new CaseSessionCalendarFilterVM()
            {
                CourtHallId = CourtHallId,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            return PartialView("_CaseSessionCalendar", model);
        }

        [HttpPost]
        public async Task<JsonResult> CourtHallBusy(int CourtHallId, DateTime DateFrom, int DateTo_Minutes, int ModelId)
        {
            return Json(new { result = await service.CourtHallBusy(CourtHallId, DateFrom, DateTo_Minutes, ModelId) });
        }

        /// <summary>
        /// Срочна книга
        /// </summary>
        /// <returns></returns>
        public IActionResult CaseSessionTimeBookSpr()
        {
            var model = new CaseSessionTimeBookFilterVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            model.DateTo = new DateTime(DateTime.Now.Year, 12, 31);
            model.CaseGroupId = -1;
            model.DepartmentId = -1;
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.DepartmentId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            return View(model);
        }

        /// <summary>
        /// Извличане на данни за Срочна книга
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseSessionTimeBookSpr(CaseSessionTimeBookFilterVM model)
        {
            var xlsBytes = service.CaseSessionTimeBook_ToExcel(userContext.CourtId, model.DateFrom ?? DateTime.Now.AddYears(-100), model.DateTo ?? DateTime.Now.AddYears(100), model.CaseGroupId, model.DepartmentId);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, "CaseSessionTimeBook.xlsx");
        }

        /// <summary>
        /// Анулиране на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseSession_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSession, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            if (!service.IsCanExpired(model.Id))
            {
                return Json(new { result = false, message = "По заседанието има създадени и/или добавени документи." });
            }
            if (string.IsNullOrEmpty(model.DescriptionExpired))
            {
                return Json(new { result = false, message = "Няма въведена причина за изтриване." });
            }

            var expireObjectCaseId = service.GetPropById<CaseSession, int>(model.Id, x => x.CaseId);
            if (service.CaseSession_ExpiredInfo(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseSession, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseSessionExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("CasePreview", "Case", new { id = expireObjectCaseId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        /// <summary>
        /// Справка за Заседания за период с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexReportMaturity()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.SessionTypeId_ddl = nomService.GetDropDownList<SessionType>();
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            SetHelpFile(HelpFileValues.Report20);

            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Заседания за период с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataReportMaturity(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseSessionReportMaturity_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Анулиране на резултат
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseSessionResult_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionResult, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var expireObjectCaseSessionId = service.GetPropById<CaseSessionResult, int>(model.Id, x => x.CaseSessionId);
            if (service.CaseSessionResult_ExpiredInfo(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseSessionResult, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseSessionResultExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Preview", "CaseSession", new { id = expireObjectCaseSessionId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        /// <summary>
        /// Функция за групова проверка при запис на заседание - за сега няма да се ползва
        /// </summary>
        /// <param name="CaseId">ИД на дело</param>
        /// <param name="CaseSessionId">Ид на заседание</param>
        /// <param name="SessionStateId">Статус</param>
        /// <param name="SessionTypeId">Тип на заседание</param>
        /// <param name="DateTimeFrom">Час на започване</param>
        /// <param name="DateTo_Minutes">Колко минути ще е заседанието</param>
        /// <param name="CourtHallId">ИД на зала</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ChecksOnSaveSession(int CaseId, int CaseSessionId, int SessionStateId, int SessionTypeId, DateTime DateTimeFrom, int DateTo_Minutes, int CourtHallId)
        {
            var messageResult = string.Empty;

            if (await lawUnitService.IsFullComposition(CaseId))
                messageResult += (!string.IsNullOrEmpty(messageResult) ? " " : string.Empty) + "Съставът по делото не е пълен!";

            if (await lawUnitService.IsExistJudgeLawUnitInCase(CaseId))
                messageResult += (!string.IsNullOrEmpty(messageResult) ? " " : string.Empty) + "Не фигурирате по това дело!";

            if (SessionStateId == @NomenclatureConstants.SessionState.Provedeno &&
                SessionTypeId != @NomenclatureConstants.SessionType.ClosedSession &&
                CaseSessionId > 0)
            {
                if (await caseSessionMeetingService.CheckExistSecretaryOfAllMeeting(CaseSessionId))
                    messageResult += (!string.IsNullOrEmpty(messageResult) ? " " : string.Empty) + "Има сесия без секретар!";
            }

            if (DateTo_Minutes > 0)
            {
                var _res = await caseSessionMeetingService.IsCaseLawUnitFromCaseBusy(CaseId, SessionStateId, DateTimeFrom, DateTimeFrom.AddMinutes(DateTo_Minutes));
                if (!string.IsNullOrEmpty(_res))
                    messageResult += (!string.IsNullOrEmpty(messageResult) ? " " : string.Empty) + _res;
            }

            if (CaseSessionId < 1)
            {
                if (await service.IsExistLastSessionWithoutAct(CaseId, CaseSessionId))
                    messageResult += (!string.IsNullOrEmpty(messageResult) ? " " : string.Empty) + "Има насрочено/проведено заседание без акт!";
            }

            if ((CourtHallId > 0) && (DateTo_Minutes > 0))
            {
                if (await caseSessionMeetingService.CourtHallBusyFromSession(CourtHallId, DateTimeFrom, DateTo_Minutes, CaseSessionId))
                    messageResult += (!string.IsNullOrEmpty(messageResult) ? " " : string.Empty) + "Залата е заета в този интервал!";
            }

            return Json(new { result = messageResult });
        }

        /// <summary>
        /// Проверка за пълен състав по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> IsFullComposition(int CaseId)
        {
            return Json(new { result = await lawUnitService.IsFullComposition(CaseId) });
        }

        /// <summary>
        /// Проверка за открито и закрито заседание без постановен акт
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="SessionId"></param>
        /// <param name="SessionTypeId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IsExistSessionWithoutAct(int CaseId, int SessionId, int SessionTypeId)
        {
            if (SessionTypeId != NomenclatureConstants.SessionType.ClosedSession &&
                SessionTypeId != NomenclatureConstants.SessionType.OpenSession)
                return Json(new { result = false });

            return Json(new { result = service.IsExistSessionWithoutAct(CaseId, SessionId, SessionTypeId) });
        }

        /// <summary>
        /// Проверка за последно заседание дали има постановен акт
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="SessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> IsExistLastSessionWithoutAct(int CaseId, int SessionId)
        {
            return Json(new { result = await service.IsExistLastSessionWithoutAct(CaseId, SessionId) });
        }

        #region Заседания с ненаписани съдебни актове от всички съдии

        /// <summary>
        /// Справка заседания с не написани съдебни актове към [дата] от всички съдии
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexCaseSessionWithActProject()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка заседания с не написани съдебни актове към [дата] от всички съдии");
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                ActDateToSpr = DateTime.Now
            };

            SetHelpFile(HelpFileValues.Report19);
            ViewBagCaseSessionWithActProject();
            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Заседания с не написани съдебни актове към [дата] от всички съдии
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseSessionWithActProject(IDataTablesRequest request, CaseFilterReport filter)
        {
            var data = service.CaseSessionWithActProject_Select(filter);
            return request.GetResponse(data);
        }

        private void ViewBagCaseSessionWithActProject()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.SessionTypeIds_ddl = nomService.GetDropDownList<SessionType>(false);
        }

        #endregion

        //public IActionResult multidata()
        //{
        //    var model = service.CaseSession_MnogoZasedania(10658, 83);
        //    return Json(model);
        //}
    }
}