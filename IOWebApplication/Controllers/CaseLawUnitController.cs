using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseLawUnitController : BaseController
    {
        private readonly ICaseLawUnitService service;
        private readonly INomenclatureService nomService;
        private readonly ICaseSessionActService actService;
        private readonly ICaseSessionService sessionService;
        private readonly ICommonService commonService;
        private readonly IDocumentService docService;
        private readonly IWorkTaskService taskService;
        private readonly ICaseService caseService;

        public CaseLawUnitController(ICaseLawUnitService _service,
                                     INomenclatureService _nomService,
                                     ICaseSessionActService _actService,
                                     ICaseSessionService _sessionService,
                                     ICommonService _commonService,
                                     IDocumentService _docService,
                                     IWorkTaskService _taskService,
                                     ICaseService _caseService)
        {
            service = _service;
            nomService = _nomService;
            actService = _actService;
            sessionService = _sessionService;
            commonService = _commonService;
            docService = _docService;
            taskService = _taskService;
            caseService = _caseService;
        }

        /// <summary>
        /// Страница със съдебен състав по дело/заседание
        /// </summary>
        /// <param name="id"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public IActionResult Index(int id, int? caseSessionId)
        {
            var tcase = service.GetById<Case>(id);
            ViewBag.caseId = id;
            ViewBag.caseSessionId = caseSessionId;
            ViewBag.casenumber = tcase.EISSPNumber;

            return View();
        }

        /// <summary>
        /// Списък на отводи
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> IndexDismisal(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawUnitDismisalList, null, AuditConstants.Operations.View, id))
            {
                return Redirect_Denied();
            }
            var tcase = await caseService.GetCaseInfo(id);
            ViewBag.caseId = id;
            ViewBag.CaseName = tcase.CaseTypeCodeShortNumberRegDate;
            SetHelpFile(HelpFileValues.CaseLawunit);

            return View();
        }

        /// <summary>
        /// Извличане на данни за съдебен състав по дело/заседание
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId, int? caseSessionId)
        {
            var data = service.CaseLawUnit_Select(caseId, caseSessionId);
            return request.GetResponse(data);
        }

        [HttpPost]
        public IActionResult ListDataManualRoles(IDataTablesRequest request, int caseId, int? caseSessionId)
        {
            var data = service.CaseLawUnit_Select(caseId, caseSessionId, false, true);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на състав от дело към заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public IActionResult FillSessionLawUnitFromCase(int caseId, int caseSessionId)
        {
            if (service.FillSessionLawUnitFromCase(caseId, caseSessionId))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return RedirectToAction("Preview", "CaseSession", new { id = caseSessionId });
        }

        /// <summary>
        /// Добавяне на състав от дело по заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public async Task<IActionResult> SessionLawUnitFromCase(int caseId, int caseSessionId)
        {
            var checkListViewVM = service.CheckListViewVM_Fill(caseId, caseSessionId);
            if (checkListViewVM.checkListVMs.Count < 1)
            {
                SetErrorMessage("Няма данни за копиране");
                return RedirectToAction("Preview", "CaseSession", new { id = caseSessionId });
            }
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionLawUnit, null, AuditConstants.Operations.ChoiceByList, caseSessionId))
            {
                return Redirect_Denied();
            }
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { id = caseSessionId });
            SetHelpFile(HelpFileValues.SessionLawunit);
            checkListViewVM.ShowLogOperation = true;
            return View("CheckListViewVM", checkListViewVM);
        }

        /// <summary>
        /// Запис на състава от дело по заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SessionLawUnitFromCase(CheckListViewVM model)
        {
            if (service.SessionLawUnitFromCase_SaveData(model))
            {
                await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionLawUnit, null, AuditConstants.Operations.ChoiceByList, model.ObjectId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                this.SaveLogOperation(IO.LogOperation.Models.OperationTypes.Patch, model.ObjectId);
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            //ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { id = model.ObjectId });
            //return View("CheckListViewVM", model);
            return RedirectToAction("Preview", "CaseSession", new { id = model.ObjectId });
        }

        /// <summary>
        /// Добавяне на отвод
        /// </summary>
        /// <param name="lawUnitId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddEditDismisal(int lawUnitId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawUnitDismisal, null, AuditConstants.Operations.Append, lawUnitId))
            {
                return Redirect_Denied();
            }
            // SetViewbag(lawUnitId);
            var model = service.CaseLawUnitDismisal_GetByCaseLawUnitId(lawUnitId);

            if (model == null)
            {
                return RedirectToAction(nameof(Add), new { lawUnitId = lawUnitId, });
            }
            else
                return RedirectToAction(nameof(ViewDismisal), new { id = model.Id });
        }

        private bool IsExistResultOtvod(int CaseSessionActId)
        {
            var sessionAct = service.GetById<CaseSessionAct>(CaseSessionActId);
            var caseSessionResults = sessionService.CaseSessionResult_Select(sessionAct.CaseSessionId);
            return caseSessionResults.Any(x => NomenclatureConstants.CaseSessionResult.ActZaOtvod.Contains(x.SessionResultId));
        }

        /// <summary>
        /// Добавяне на член на съдебен състав
        /// </summary>
        /// <param name="lawUnitId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Add(int lawUnitId)
        {
            var lawUnit = await service.GetByIdAsync<CaseLawUnit>(lawUnitId);
            var model = new CaseLawUnitDismisal()
            {
                CourtId = lawUnit.CourtId,
                CaseId = lawUnit.CaseId,
                CaseLawUnitId = lawUnitId,
                DismisalDate = DateTime.Now,
                DismissalRequestType = NomenclatureConstants.DismissalRequestTypes.Document
            };

            await SetViewbagDismissal(lawUnitId);
            model.DismisalKindId = ViewBag.DismisalKind;
            return View(nameof(EditDismisal), model);

        }

        /// <summary>
        /// Редакция на отвод
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> ViewDismisal(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawUnitDismisal, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            var model = await service.GetByIdAsync<CaseLawUnitDismisal>(id);
            if (model == null)
            {
                return NotFoundError("Търсеният от Вас отвод не е намерен и/или нямате достъп до него.");
            }
            model.DismissalStateId = model.DismissalStateId ?? NomenclatureConstants.DismissalStates.Confirmed;
            model.DismissalRequestType = model.DismissalRequestType ?? NomenclatureConstants.DismissalRequestTypes.Document;
            await SetViewbagDismissal(model.CaseLawUnitId);
            return View(nameof(EditDismisal), model);
        }

        private async Task SetViewbagDismissal(int caseLawUnitId)
        {
            var caseLawUnit = await service.GetByIdAsync<CaseLawUnit>(caseLawUnitId);

            ViewBag.DismisalTypeId_ddl = await nomService.GetDismisalTypes_SelectForDropDownListAsync(caseLawUnitId);
            ViewBag.CaseSessionActId_ddl = await actService.GetDropDownListForDismisalAsync(caseLawUnit.CaseId);
            ViewBag.DismissalSessionActId_ddl = await actService.GetDropDownListForDismisalRequestAsync(caseLawUnit.CaseId);
            var caseCase = await caseService.GetCaseInfo(caseLawUnit.CaseId);
            ViewBag.CaseName = caseCase.CaseTypeCodeShortNumberRegDate;
            ViewBag.caseId = caseCase.Id;

            ViewBag.DismisalKind = NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(caseLawUnit.JudgeRoleId) ? NomenclatureConstants.LawUnitTypes.Judge :
                                                                                                                      NomenclatureConstants.LawUnitTypes.Jury;

            ViewBag.DocumentId_ddl = await docService.GetCompliantDocumentsByCaseIdAsync(caseLawUnit.CaseId, true);
            ViewBag.DismissalStateId_ddl = await nomService.GetDropDownListAsync<DismissalState>(false);
            SetHelpFile(HelpFileValues.CaseLawunit);
        }

        /// <summary>
        /// Валидация преди запис на отвод
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidDismissal(CaseLawUnitDismisal model)
        {
            if (service.CaseLawUnitDismisal_GetByCaseLawUnitId(model.CaseLawUnitId) != null)
            {
                return "Съществува отвод/самоотвод за избраното лице.";
            }
            if (model.DismisalTypeId < 0)
            {
                return "Не е избран тип на отвеждане";
            }
            if (model.DismisalTypeId == NomenclatureConstants.DismisalType.Otvod || model.DismisalTypeId == NomenclatureConstants.DismisalType.SamoOtvod)
            {
                if ((model.CaseSessionActId ?? -1) < 0)
                    return "Няма избран акт";
            }


            if ((model.Description ?? string.Empty) == string.Empty)
                return "Няма въведен мотив";

            if (model.DismisalDate > DateTime.Now)
                return "Не може да избирате бъдеща дата";

            if (model.Id < 1)
            {
                if (model.DismisalTypeId == NomenclatureConstants.DismisalType.Otvod || model.DismisalTypeId == NomenclatureConstants.DismisalType.SamoOtvod)
                {
                    if ((model.CaseSessionActId ?? -1) < 0)
                        return "Няма избран акт";
                    else
                    {
                        var sessionAct = actService.GetById<CaseSessionAct>(model.CaseSessionActId);

                        if (!((sessionAct.ActStateId == NomenclatureConstants.SessionActState.Enforced) || (sessionAct.ActStateId == NomenclatureConstants.SessionActState.ComingIntoForce)))
                            return "Актът трябва да е постановен или влязъл в сила ";

                        if (sessionAct.RegNumber == string.Empty)
                            return "Актът няма генериран номер";
                    }

                    if (!IsExistResultOtvod(model.CaseSessionActId ?? -1))
                    {
                        return "Избраният акт е от заседание в което няма резултат свързан с отвод";
                    }

                }
            }
            if (model.DismisalKindId == NomenclatureConstants.LawUnitTypes.Judge)
            {
                if (model.DismisalTypeId == NomenclatureConstants.DismisalType.Otvod || model.DismisalTypeId == NomenclatureConstants.DismisalType.SamoOtvod)
                {

                    var lastDate = actService.GetLastSignCaseDate(model.CaseLawUnitId, model.CaseSessionActId ?? -1);
                    if (lastDate.AddSeconds(-lastDate.Second) > model.DismisalDate.AddSeconds(-model.DismisalDate.Second))
                    {
                        return $"Избраният акт е с дата({lastDate.ToString("dd.MM.yyyy")}) по-късна от дaтата на отвеждането";
                    }
                }
                if (model.DismisalTypeId == NomenclatureConstants.DismisalType.Prerazpredelqne)
                {

                    var lastDate = actService.GetLastSignCaseDate(model.CaseLawUnitId, null);
                    if (lastDate.AddSeconds(-lastDate.Second) > model.DismisalDate.AddSeconds(-model.DismisalDate.Second))
                    {
                        return $"Датата на отвеждане е по-ранна от последната дата на подпис ({lastDate.ToString("dd.MM.yyyy")})";
                    }
                }
            }


            if (model.DismisalTypeId == NomenclatureConstants.DismisalType.Otvod)
            {
                if (model.DismissalRequestType == NomenclatureConstants.DismissalRequestTypes.Document)
                {
                    if ((model.DocumentId ?? 0) <= 0)
                    {
                        return "Изберете 'Документ, с който се иска отвода'.";
                    }

                    if ((model.DocumentPersonId ?? 0) <= 0)
                    {
                        return "Изберете 'Вносител на искането'.";
                    }
                }

                if (model.DismissalRequestType == NomenclatureConstants.DismissalRequestTypes.Session)
                {
                    if ((model.DismissalSessionActId ?? 0) <= 0)
                    {
                        return "Изберете 'Протокола от заседанието, в което е внесено искането за отвода'.";
                    }
                }
                if (model.DismissalRequestType == null)
                {
                    return "Изберете 'Вид искане за отвод'.";
                }
            }
            else
            {
                model.DismissalRequestType = null;
                model.DismissalSessionActId = null;
                model.DismissalCasePersonId = null;
                model.DocumentPersonId = null;
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на отвод
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditDismisal(CaseLawUnitDismisal model, string btnRedirectSelection = null)
        {
            await SetViewbagDismissal(model.CaseLawUnitId);

            string _isvalid = IsValidDismissal(model);
            if (_isvalid != string.Empty)
            {
                ModelState.AddModelError("", _isvalid);
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(EditDismisal), model);
            }

            var currentId = model.Id;
            if (service.CaseLawUnitDismisal_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLawUnitDismisal, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);

                //return RedirectToAction(nameof(EditDismisal), new { id = model.Id });
                if (btnRedirectSelection is null)
                {
                    SetSuccessMessage(MessageConstant.Values.SaveOK);
                    return RedirectToAction("CasePreview", "Case", new { id = model.CaseId });

                }
                else
                {
                    SetSuccessMessage("Извеждането от състав по дело е успено. Извършете преразпреление.");
                    return RedirectToAction("Add", "CaseSelectionProtokol", new { caseId = model.CaseId });
                }
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(EditDismisal), model);
        }

        /// <summary>
        /// Извличане на данни за отводи
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataDismisal(IDataTablesRequest request, int caseId)
        {
            var data = service.CaseLawUnitDismisal_Select(caseId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Извличане на данни за отводи за които няма ново разпределение
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetDDL_FreeDismisal(int caseId, int roleId)
        {
            var model = service.CaseLawUnitFreeDismisal_SelectForDropDownList(caseId, roleId);
            return Json(model);
        }

        public async Task<IActionResult> AddManualRoles(int caseId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawUnit, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            var caseCase = await service.GetByIdAsync<Case>(caseId);
            var model = new CaseLawUnit()
            {
                CourtId = caseCase.CourtId,
                CaseId = caseCase.Id,
                DateFrom = DateTime.Now
            };

            await SetViewbagManualRoles(caseId);
            return View(nameof(EditManualRoles), model);
        }

        public async Task<IActionResult> EditManualRoles(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLawUnit, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await service.GetByIdAsync<CaseLawUnit>(id);
            if (model == null)
            {
                return NotFoundError("Търсеният от Вас интервал не е намерен и/или нямате достъп до него.");
            }
            await SetViewbagManualRoles(model.CaseId);
            return View(nameof(EditManualRoles), model);
        }

        private async Task SetViewbagManualRoles(int caseId)
        {
            var caseCase = await caseService.GetCaseInfo(caseId);
            ViewBag.CaseName = caseCase.CaseTypeCodeShortNumberRegDate;
            ViewBag.caseId = caseCase.Id;
            ViewBag.JudgeRoleId_ddl = await nomService.GetDDL_JudgeRoleManualRolesAsync();
            SetHelpFile(HelpFileValues.CaseLawunit);
        }

        private string IsValidManualRoles(CaseLawUnit model)
        {
            if (model.LawUnitId < 1)
            {
                return "Няма избран служител";
            }

            if (model.JudgeRoleId < 1)
            {
                return "Няма избрана роля";
            }

            if (model.Id < 1)
            {
                if (service.IsExistManualLawUnitByCase(model.CaseId, model.LawUnitId, DateTime.Now))
                {
                    return "Този потребител вече е добавен";
                }
            }

            if (model.DateTo != null)
            {
                if (service.IsExistIsExistManualLawUnitInConductedSession(model.CaseId, model.DateTo ?? DateTime.Now, model.LawUnitId))
                {
                    return "Има проведено заседание в което участва този потребител и не може да се въведе дато до по-малка от началото на това заседание.";
                }
            }

            return string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> EditManualRoles(CaseLawUnit model)
        {
            await SetViewbagManualRoles(model.CaseId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditManualRoles), model);
            }

            string _isvalid = IsValidManualRoles(model);
            if (_isvalid != string.Empty)
            {
                ModelState.AddModelError("", _isvalid);
                return View(nameof(EditManualRoles), model);
            }

            var currentId = model.Id;
            if (await service.CaseLawUnit_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLawUnit, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction("CasePreview", "Case", new { id = model.CaseId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(EditManualRoles), model);
        }

        public IActionResult CaseLawUnitRefresh(int caseid, int caseSessionId)
        {
            if (service.CaseLawUnit_RefreshData(caseid, caseSessionId))
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return RedirectToAction("Preview", "CaseSession", new { id = caseSessionId });
        }

        async Task auditInfoCaseLawUnitChangeDepRol(string operation, int caseId, int? caseSessionId = null, string add = "")
        {
            var caseCase = await caseService.Case_SelectForEdit(caseId);
            var caseSession = (caseSessionId != null) ? sessionService.CaseSessionById(caseSessionId ?? 0) : null;

            if (caseSession != null)
            {
                AddAuditInfo(operation, $"По заседание: {caseSession.SessionType.Label} от: {caseSession.DateFrom.ToString("dd.MM.yyyy")}", add, $"Промяна на председател/състав");
            }
            else
            {
                AddAuditInfo(operation, $"По дело: {caseCase.RegNumberText}", add, $"Промяна на председател/състав");
            }
        }

        /// <summary>
        /// Страница за промяна на председател и състав
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public async Task<IActionResult> CaseLawUnitChangeDepRol(int caseId, int? caseSessionId = null)
        {
            var model = service.GetCaseLawUnitChangeDepRol(caseId, caseSessionId);
            //Ако не няма избран състав - да покаже Без избран състав
            model.DepartmentId = model.DepartmentId ?? -1;
            SetViewbagCaseLawUnitChangeDepRol(model.CaseId, model.CaseSessionId);

            if (!ViewBag.hasLawUnit && !ViewBag.hasDepartment)
            {
                SetErrorMessage("Няма данни за промяна.");
                return RedirectToAction("CasePreview", "Case", new { id = caseId });
            }

            await auditInfoCaseLawUnitChangeDepRol(AuditConstants.Operations.View, caseId, caseSessionId);
            return View(nameof(CaseLawUnitChangeDepRol), model);
        }

        [HttpPost]
        public async Task<IActionResult> CaseLawUnitChangeDepRol(CaseLawUnitChangeDepRolVM model)
        {
            SetViewbagCaseLawUnitChangeDepRol(model.CaseId, model.CaseSessionId);
            if (service.GetCaseLawUnitChangeDepRol_Save(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                await auditInfoCaseLawUnitChangeDepRol(AuditConstants.Operations.Update, model.CaseId, model.CaseSessionId);
                if (model.CaseSessionId > 0)
                {
                    this.SaveLogOperation(IO.LogOperation.Models.OperationTypes.Patch, model.CaseSessionId);
                    return RedirectToAction("Preview", "CaseSession", new { id = model.CaseSessionId });
                }
                else
                {
                    this.SaveLogOperation(IO.LogOperation.Models.OperationTypes.Patch, model.CaseId);
                    return RedirectToAction("CasePreview", "Case", new { id = model.CaseId });
                }
                //return RedirectToAction(nameof(CaseLawUnitChangeDepRol), new { caseId = model.CaseId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(CaseLawUnitChangeDepRol), model);
        }

        private void SetViewbagCaseLawUnitChangeDepRol(int caseId, int? caseSessionId = null)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            var selectListItemCaseLawUnits = service.GetDDL_GetJudgeFromCase(caseId, caseSessionId);
            var selectListItemDepartments = service.GetDDL_GetListDepartmentFromRealDepartment(caseId);
            ViewBag.CaseLawUnitId_ddl = selectListItemCaseLawUnits;
            ViewBag.hasLawUnit = selectListItemCaseLawUnits.Count > 0;
            ViewBag.DepartmentId_ddl = selectListItemDepartments.OrderBy(x => x.Text).ToList();
            ViewBag.hasDepartment = selectListItemDepartments.Count > 0;
            SetHelpFile(HelpFileValues.CaseLawunit);
        }

        [HttpPost]
        public IActionResult LawUnitSubstitute_LoadData(IDataTablesRequest request, int caseSessionId)
        {
            var data = service.LawUnitSubstitution_SelectForSession(caseSessionId);
            return request.GetResponse(data, null, null, false);
        }

        [HttpPost]
        public async Task<IActionResult> LoadDataCaseSelectionProtokolSubstitution(IDataTablesRequest request, int caseSessionId)
        {
            var data = await service.GetCaseSelectionProtokolSubstitution(caseSessionId);
            return request.GetResponse(data, null, null, false);
        }

        [HttpPost]
        public IActionResult LawUnitSubstitute_Apply(int substsitution_id, int from, int to, int caseSessionId)
        {
            var result = service.LawUnitSubstitution_Apply(substsitution_id, from, to, caseSessionId);
            return Json(new { isOk = result });
        }

        /// <summary>
        /// Запис на заместване (случайно разпределение извън дело)
        /// </summary>
        /// <param name="protokolId">Идентификатор на протокола</param>
        /// <param name="caseSessionId">Идентификатор на заседанието</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CaseSelectionProtokolSubstitutionApply(int protokolId, int caseSessionId)
        {
            var result = await service.CaseSelectionProtokolSubstitutionApply(protokolId, caseSessionId);
            return Json(new { isOk = result });
        }

        [HttpPost]
        public async Task<JsonResult> IsExistJudgeLawUnitInCase(int caseId)
        {
            return Json(new { result = await service.IsExistJudgeLawUnitInCase(caseId) });
        }

        public async Task<JsonResult> CheckActSignersForDismissal(int lawUnitId, int actId)
        {
            var lawUnit = service.GetById<CaseLawUnit>(lawUnitId);
            lawUnit.LawUnit = service.GetById<Infrastructure.Data.Models.Common.LawUnit>(lawUnit.LawUnitId);
            var tasks = await taskService.Select(SourceTypeSelectVM.CaseSessionAct, actId);
            var lastSendToSignTask = tasks.Where(x => x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_SentToSign
                        && x.TaskStateId == WorkTaskConstants.States.Completed)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault();
            if (lastSendToSignTask == null)
            {
                return Json(new { result = false, message = "Актът не е подписан." });
            }

            var lastSignTasks = tasks.Where(x => x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_Sign
                                            && x.Id > lastSendToSignTask.Id
                                            && x.TaskStateId == WorkTaskConstants.States.Completed)
                                            .ToList();

            if (!lastSignTasks.Any())
            {
                return Json(new { result = false, message = "Актът не е подписан." });
            }

            var caseSessionId = service.GetPropById<CaseSessionAct, int>(x => x.Id == actId, x => x.CaseSessionId);
            var lawunits = service.CaseLawUnit_Select(lawUnit.CaseId, caseSessionId);
            var dissmissedJudgeName = lawUnit.LawUnit.FirstNameInitial_Family;

            var dismissedIsSigner = lastSignTasks.Any(x => x.UserId == service.GetUserIdByLawUnitId(lawUnit.LawUnitId));

            var signers = new List<string>();
            foreach (var task in lastSignTasks)
            {
                var signer = lawunits.Where(x => x.LawUnitUserId == task.UserId).FirstOrDefault();
                if (signer != null)
                {
                    signers.Add(signer.LawUnitNameInitials);
                }
            }

            return Json(new { result = true, dismisalSigner = dismissedIsSigner, dissmissedJudgeName = dissmissedJudgeName, signers = string.Join(',', signers) });
        }
    }
}