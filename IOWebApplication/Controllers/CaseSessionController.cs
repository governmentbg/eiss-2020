﻿using System;
using System.Linq;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
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
        public CaseSessionController(ICaseSessionService _service,
                                     INomenclatureService _nomService,
                                     ICommonService _commonService,
                                     ICaseClassificationService _classficationService,
                                     ICaseLawUnitService _lawUnitService,
                                     ICourtDepartmentService _courtDepartmentService,
                                     ICaseSessionMeetingService _caseSessionMeetingService,
                                     ICaseNotificationService _caseNotificationService,
                                     ICaseSessionActService _caseSessionActService)
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
        }

        public IActionResult Index(int id)
        {
            var tcase = service.GetById<Case>(id);
            ViewBag.caseId = id;
            ViewBag.casenumber = tcase.EISSPNumber;
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSession, null, AuditConstants.Operations.View, id))
            {
                return Redirect_Denied();
            }
            return View();
        }

        /// <summary>
        /// Справка за заседания
        /// </summary>
        /// <returns></returns>
        public IActionResult Index_Spr()
        {
            ViewBag.CaseGroupIds_ddl = nomService.GetDropDownList<CaseGroup>(false);
            ViewBag.HallId_ddl = commonService.GetDropDownList_CourtHall(userContext.CourtId);
            ViewBag.CaseSessionTypeIds_ddl = nomService.GetDropDownList<SessionType>(false);
            ViewBag.SessionResultIds_ddl = nomService.GetDropDownList<SessionResult>(false);
            ViewBag.SessionStateId_ddl = nomService.GetDropDownList<SessionState>();
            ViewBag.CourtDepartmentId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            ViewBag.CourtDepartmentOtdelenieId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Otdelenie);
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
        public IActionResult Add(int caseId, DateTime? sessionDate = null)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSession, null, AuditConstants.Operations.Append, caseId))
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

            SetViewbag(0, caseId, null);
            SetHelpFile(HelpFileValues.CaseSession);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// редакция на заседание
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.CaseSessionVMById(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеното от Вас заседание не е намерено и/или нямате достъп до него.");
            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSession, id, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }
            ViewBag.CaseSessionName = model.SessionTypeLabel + " " + model.DateFrom.ToString("dd.MM.yyyy");
            SetViewbag(id, model.CaseId, model.Id);
            SetHelpFile(HelpFileValues.SessionMainData);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Зареждане на цялостен преглед на заседание с табове за различни елементи на заседанието
        /// </summary>
        /// <param name="id"></param>
        /// <param name="notifListTypeId"></param>
        /// <returns></returns>
        public IActionResult Preview(int id, int? notifListTypeId)
        {
            var model = service.CaseSessionVMById(id);
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSession, model.Id, AuditConstants.Operations.View, model.CaseId))
            {
                return Redirect_Denied();
            }
            model.NotificationListTypeId = notifListTypeId;
            ViewBag.CaseSessionName = model.SessionTypeLabel + " " + model.DateFrom.ToString("dd.MM.yyyy");
            SetViewbag(id, model.CaseId, model.Id, false);
            SetHelpFile(HelpFileValues.SessionMainData);

            return View(nameof(Preview), model);
        }

        void SetViewbag(int id, int caseId, int? caseSessionId, bool IsViewRowSessionBreadcrumbs = true)
        {
            var modelSession = new CaseSessionVM();

            if (id > 0)
            {
                modelSession = service.CaseSessionVMById(id);
                ViewBag.IsExpired = modelSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno;
            }
            else
                ViewBag.IsExpired = false;

            ViewBag.CaseSessionName = modelSession.SessionTypeLabel + " " + modelSession.DateFrom.ToString("dd.MM.yyyy");
            ViewBag.CourtHallId = modelSession.CourtHallId ?? 0;
            ViewBag.SessionTypeId_ddl = nomService.GetDDL_SessionTypesByCase(caseId);
            ViewBag.SessionStateId_ddl = nomService.GetDDL_SessionStateRoute(modelSession.SessionStateId);
            ViewBag.CourtHallId_ddl = commonService.GetDropDownList_CourtHall(userContext.CourtId);
            ViewBag.CaseClassification_ddl = classficationService.CaseClassification_Select(caseId, caseSessionId);
            ViewBag.DateTo_Minutes_ddl = nomService.GetDDL_SessionDuration();

            if (caseSessionId > 0)
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(caseSessionId.Value, IsViewRowSessionBreadcrumbs);
            }
            else
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            }
            ViewBag.hasSubstitutions = (lawUnitService.LawUnitSubstitution_SelectForSession(caseSessionId ?? 0) != null) ? lawUnitService.LawUnitSubstitution_SelectForSession(caseSessionId ?? 0).Any() : false;

            if (id == 0)
            {
                var caseModel = service.GetById<Case>(caseId);
                if (caseModel.CourtId == NomenclatureConstants.Courts.VKS && caseModel.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                {
                    ViewBag.VksLawunitChange_ddl = nomService.GetDDL_VksSessionLawunitChange();
                }
            }
        }

        private void SetViewbagAddSessionAndAct(int caseId)
        {
            ViewBag.SessionTypeId_ddl = nomService.GetDDL_SessionTypesByCaseByGroupe(caseId, NomenclatureConstants.CaseSessionTypeGroup.PrivateSession);
            //ViewBag.SessionStateId_ddl = nomService.GetDDL_SessionStateFiltered(0);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            //ViewBag.ActTypeId_ddl = caseSessionActService.GetActTypesFromCaseByCase(caseId);
            var caseCase = service.GetById<Case>(caseId);
            var actComplainResults = nomService.GetDDL_ActComplainResult(caseCase.CaseTypeId);
            ViewBag.ActComplainResultId_ddl = actComplainResults;
            ViewBag.hasComplainResult = actComplainResults.Count > 1;
        }

        /// <summary>
        /// Запис на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CaseSessionVM model)
        {
            model.DateFrom = model.DateFrom.MakeEndSeconds();
            model.DateTo = model.DateFrom.AddMinutes(model.DateTo_Minutes).MakeEndSeconds();
            SetHelpFile(HelpFileValues.SessionMainData);

            if (!ModelState.IsValid)
            {
                SetViewbag(model.Id, model.CaseId, model.Id);
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetViewbag(model.Id, model.CaseId, model.Id);
                SetErrorMessage(_isvalid);
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            var saveResult = service.CaseSession_SaveData(model);
            if (saveResult.Result)
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSession, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);

                if ((!service.IsExistCaseSessionResult(model.Id)) && (model.SessionStateId == NomenclatureConstants.SessionState.Provedeno))
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

            SetViewbag(model.Id, model.CaseId, model.Id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Добавяне на заседание към дело и акт към заседанието
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IActionResult AddSessionAndAct(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSession, null, AuditConstants.Operations.Append, caseId))
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
                CaseSessions = service.GetCheckListCaseSession(caseId)
            };

            //Следващия кръгъл час
            //model.DateFrom = model.DateFrom.AddMinutes(-model.DateFrom.Minute).AddHours(1);
            SetViewbagAddSessionAndAct(caseId);
            return View(nameof(AddSessionAndAct), model);
        }

        [HttpPost]
        public IActionResult AddSessionAndAct(CaseSessionVM model)
        {
            model.DateFrom = model.DateFrom.MakeEndSeconds();
            model.DateTo = model.DateFrom.AddMinutes(model.DateTo_Minutes).MakeEndSeconds();

            ModelState["SessionStateId"].Errors.Clear();
            ModelState["SessionStateId"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;

            if (!ModelState.IsValid)
            {
                SetViewbagAddSessionAndAct(model.CaseId);
                return View(nameof(AddSessionAndAct), model);
            }

            string _isvalid = IsValidSessionAndAct(model);
            if (_isvalid != string.Empty)
            {
                SetViewbagAddSessionAndAct(model.CaseId);
                SetErrorMessage(_isvalid);
                return View(nameof(AddSessionAndAct), model);
            }

            var currentId = model.Id;
            var saveResult = service.CaseSession_SaveData(model);
            if (saveResult.Result)
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSession, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id, null, nameof(Edit));
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

            SetViewbagAddSessionAndAct(model.CaseId);
            return View(nameof(AddSessionAndAct), model);
        }

        private string IsValidSessionAndAct(CaseSessionVM model)
        {
            if (model.SessionTypeId <= 0)
                return "Няма избран вид заседание";

            if (model.DateFrom == null)
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

                var caseCase = service.GetById<Case>(model.CaseId);

                if (model.DateFrom < caseCase.RegDate)
                    return "Не можете да насрочвате заседание с дата/час по-малка от дата/час на регистрация на делото";
            }

            //if (model.SessionStateId <= 0)
            //    return "Няма избран статус";

            if (model.ActTypeId <= 0)
                return "Няма избран тип акт";

            if (model.IsFinalDoc ?? false)
            {
                var _case = service.GetById<Case>(model.CaseId);
                var actComplainResults = nomService.GetDDL_ActComplainResult(_case.CaseTypeId);
                if ((model.ActComplainResultId < 1) && (actComplainResults.Count() > 1))
                {
                    return "Изберете резултат/степен на уважаване на иска";
                }
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

            if (model.CaseSessions != null)
            {
                if (model.CaseSessions.Where(x => x.Checked).Count() > 1)
                {
                    return "Може да бъде избрано само едно заседание";
                }
            }

            var caseSessions = service.CaseSession_OldSelect(model.CaseId, null, null).ToList();
            if (caseSessions.Count > 0)
            {
                if (caseSessions.Any(x => x.DateFrom == model.DateFrom))
                {
                    return "Вече има заседание в това дело с тази начална дата/час";
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

            if (model.DateFrom == null)
                return "Няма въведена начална дата";

            var caseCase = service.GetById<Case>(model.CaseId);

            if (model.DateFrom < caseCase.RegDate)
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

            if (model.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno)
            {
                if (model.DateFrom <= DateTime.Now)
                {
                    return "Не може да насрочвате/коригирате заседание с минала дата/час.";
                }
            }

            var caseSessions = service.CaseSession_OldSelect(model.CaseId, null, null).ToList();
            if (caseSessions.Count > 0)
            {
                if (caseSessions.Any(x => (x.DateFrom == model.DateFrom) && ((model.Id > 0) ? x.Id != model.Id : true)))
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

                var caseSessionOld = service.GetById<CaseSession>(model.Id);
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
        public IActionResult AddResult(int caseSessionId, int CallFromActId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionResult, null, AuditConstants.Operations.Append, caseSessionId))
            {
                return Redirect_Denied();
            }

            var caseSession = service.CaseSessionVMById(caseSessionId);
            var model = new CaseSessionResultEditVM()
            {
                CaseId = caseSession.CaseId,
                CourtId = caseSession.CourtId,
                CaseSessionId = caseSessionId,
                IsActive = true,
                IsMain = !service.IsExistMainResult(caseSessionId),
                CaseLawUnitByCase = lawUnitService.GetCheckListCaseLawUnitByCaseAll(caseSession.CaseId),
                CallFromActId = CallFromActId
            };

            SetViewbagResult(caseSessionId);
            return View(nameof(EditResult), model);
        }

        void SetViewbagResult(int caseSessionId)
        {
            ViewBag.SessionResultId_ddl = nomService.GetDDL_SessionResultFromRules(caseSessionId);
            var caseSession = service.CaseSessionVMById(caseSessionId);
            var caseCase = service.GetById<Case>(caseSession.CaseId);
            ViewBag.CaseName = caseCase.RegNumber;
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
        public IActionResult EditResult(int id, int CallFromActId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionResult, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            var model = service.GetSessionResultEditVMById(id);
            model.CallFromActId = CallFromActId;

            SetViewbagResult(model.CaseSessionId);
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

            var selectListItems = nomService.GetDDL_SessionResultBase(model.SessionResultId);
            if (selectListItems.Count > 1)
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
                if (model.SessionResultId == NomenclatureConstants.CaseSessionResult.S_opredelenie_za_otvod ||
                    model.SessionResultId == NomenclatureConstants.CaseSessionResult.S_razporejdane_za_otvod)
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
        public IActionResult EditResult(CaseSessionResultEditVM model)
        {
            SetViewbagResult(model.CaseSessionId);
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
            if (service.CaseSessionResult_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionResult, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);

                if ((model.SessionResultId == NomenclatureConstants.CaseSessionResult.S_opredelenie_za_otvod) ||
                    (model.SessionResultId == NomenclatureConstants.CaseSessionResult.S_razporejdane_za_otvod))
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
        public IActionResult CopySession(int id)
        {
            var model = service.CaseSessionVMById(id);
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSession, null, AuditConstants.Operations.Append, model.CaseId))
            {
                return Redirect_Denied();
            }
            if (model == null)
            {
                throw new NotFoundException("Търсеното от Вас заседание не е намерено и/или нямате достъп до него.");
            }

            SetViewbag(model.Id, model.CaseId, model.Id);
            SetHelpFile(HelpFileValues.SessionMainData);
            return View(nameof(CopySession), model);
        }

        /// <summary>
        /// Запис при копиране на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CopySession(CaseSessionVM model)
        {
            SetViewbag(model.Id, model.CaseId, model.Id);
            SetHelpFile(HelpFileValues.SessionMainData);
            if (!ModelState.IsValid)
            {
                return View(nameof(CopySession), model);
            }

            model.DateFrom = model.DateFrom.MakeEndSeconds();
            var caseSessions = service.CaseSession_OldSelect(model.CaseId, null, null).ToList();
            if (caseSessions.Count > 0)
            {
                if (caseSessions.Any(x => x.DateFrom == model.DateFrom))
                {
                    SetErrorMessage("Вече има заседание в това дело с тази начална дата и час");
                    return View(nameof(CopySession), model);
                }
            }

            var currentId = model.Id;

            if (service.CaseSession_CopyData(model))
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
        public IActionResult CaseSessionHallUseSpr(int HallId)
        {
            ViewBag.CourtHallId_ddl = commonService.GetDropDownList_CourtHall(userContext.CourtId);
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
        public JsonResult CourtHallBusy(int CourtHallId, DateTime DateFrom, int DateTo_Minutes, int ModelId)
        {
            return Json(new { result = service.CourtHallBusy(CourtHallId, DateFrom, DateTo_Minutes, ModelId) });
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
        public IActionResult CaseSession_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSession, model.Id, AuditConstants.Operations.Delete))
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

            var expireObject = service.GetById<CaseSession>(model.Id);
            if (service.CaseSession_ExpiredInfo(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseSession, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseSessionExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("CasePreview", "Case", new { id = expireObject.CaseId }) });
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
        public IActionResult CaseSessionResult_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionResult, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var expireObject = service.GetById<CaseSessionResult>(model.Id);
            if (service.CaseSessionResult_ExpiredInfo(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseSessionResult, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseSessionResultExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Preview", "CaseSession", new { id = expireObject.CaseSessionId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        /// <summary>
        /// Проверка за пълен състав по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IsFullComposition(int CaseId)
        {
            return Json(new { result = lawUnitService.IsFullComposition(CaseId) });
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
        public JsonResult IsExistLastSessionWithoutAct(int CaseId, int SessionId)
        {
            return Json(new { result = service.IsExistLastSessionWithoutAct(CaseId, SessionId) });
        }

        /// <summary>
        /// Справка Заседания с не написани съдебни актове към [дата] от всички съдии
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCaseSessionWithActProject()
        {
            CaseFilterReport filter = new CaseFilterReport()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                //DateTo = NomenclatureExtensions.GetEndYear(),
                ActDateToSpr = DateTime.Now
            };
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.SessionTypeId_ddl = nomService.GetDropDownList<SessionType>();
            SetHelpFile(HelpFileValues.Report19);

            return View(filter);
        }

        /// <summary>
        /// Извличане на данни за Заседания с не написани съдебни актове към [дата] от всички съдии
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseSessionWithActProject(IDataTablesRequest request, CaseFilterReport model)
        {
            var data = service.CaseSessionWithActProject_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }
    }
}