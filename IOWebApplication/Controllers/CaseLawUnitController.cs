using System;
using System.Linq;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseLawUnitController : BaseController
    {
        private readonly ICaseLawUnitService service;
        private readonly INomenclatureService nomService;
        private readonly ICaseSessionActService actService;
        private readonly ICaseSessionService sessionService;
        private readonly ICommonService commonService;

        public CaseLawUnitController(ICaseLawUnitService _service,
                                     INomenclatureService _nomService,
                                     ICaseSessionActService _actService,
                                     ICaseSessionService _sessionService,
                                     ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            actService = _actService;
            sessionService = _sessionService;
            commonService = _commonService;
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
        public IActionResult IndexDismisal(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawUnitDismisalList, null, AuditConstants.Operations.View, id))
            {
                return Redirect_Denied();
            }
            var tcase = service.GetById<Case>(id);
            ViewBag.caseId = id;
            ViewBag.CaseName = tcase.RegNumber;
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
        public IActionResult SessionLawUnitFromCase(int caseId, int caseSessionId)
        {
            var checkListViewVM = service.CheckListViewVM_Fill(caseId, caseSessionId);
            if (checkListViewVM.checkListVMs.Count < 1)
            {
                SetErrorMessage("Няма данни за копиране");
                return RedirectToAction("Preview", "CaseSession", new { id = caseSessionId });
            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionLawUnit, null, AuditConstants.Operations.ChoiceByList, caseSessionId))
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
        public IActionResult SessionLawUnitFromCase(CheckListViewVM model)
        {
            if (service.SessionLawUnitFromCase_SaveData(model))
            {
                CheckAccess(service, SourceTypeSelectVM.CaseSessionLawUnit, null, AuditConstants.Operations.ChoiceByList, model.ObjectId);
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
        public IActionResult AddEditDismisal(int lawUnitId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawUnitDismisal, null, AuditConstants.Operations.Append, lawUnitId))
            {
                return Redirect_Denied();
            }
            SetViewbag(lawUnitId);
            var model = service.CaseLawUnitDismisal_GetByCaseLawUnitId(lawUnitId);

            if (model == null)
            {
                return RedirectToAction(nameof(Add), new { lawUnitId = lawUnitId, });
            }
            else
                return RedirectToAction(nameof(EditDismisal), new { id = model.Id });
        }

        private bool IsExistResultOtvod(int CaseSessionActId)
        {
            var sessionAct = service.GetById<CaseSessionAct>(CaseSessionActId);
            var caseSessionResults = sessionService.CaseSessionResult_Select(sessionAct.CaseSessionId);
            return caseSessionResults.Any(x => x.SessionResultId == NomenclatureConstants.CaseSessionResult.S_opredelenie_za_otvod || x.SessionResultId == NomenclatureConstants.CaseSessionResult.S_razporejdane_za_otvod);
        }

        /// <summary>
        /// Добавяне на член на съдебен състав
        /// </summary>
        /// <param name="lawUnitId"></param>
        /// <returns></returns>
        public IActionResult Add(int lawUnitId)
        {
            var lawUnit = service.GetById<CaseLawUnit>(lawUnitId);
            var model = new CaseLawUnitDismisal()
            {
                CourtId = lawUnit.CourtId,
                CaseId = lawUnit.CaseId,
                CaseLawUnitId = lawUnitId,
                DismisalDate = DateTime.Now
            };

            SetViewbag(lawUnitId);
            model.DismisalKindId = ViewBag.DismisalKind;
            return View(nameof(EditDismisal), model);

        }

        /// <summary>
        /// Редакция на отвод
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult ViewDismisal(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawUnitDismisal, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            
            var model = service.GetById<CaseLawUnitDismisal>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеният от Вас отвод не е намерен и/или нямате достъп до него.");
            }
            SetViewbag(model.CaseLawUnitId);
            return View(nameof(EditDismisal), model);
        }

        void SetViewbag(int caseLawUnitId)
        {
            var caseLawUnit = service.GetById<CaseLawUnit>(caseLawUnitId);
            //ViewBag.DismisalTypeId_ddl = nomService.GetDropDownList<DismisalType>(false);
            ViewBag.DismisalTypeId_ddl = nomService.GetDismisalTypes_SelectForDropDownList(caseLawUnitId);
            //  ViewBag.CaseSessionActId_ddl = actService.GetDropDownList(caseLawUnit.CaseId);
            ViewBag.CaseSessionActId_ddl = actService.GetDropDownListForDismisal(caseLawUnit.CaseId);
            var caseCase = service.GetById<Case>(caseLawUnit.CaseId);
            ViewBag.CaseName = caseCase.RegNumber;
            ViewBag.caseId = caseCase.Id;

            if (NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(caseLawUnit.JudgeRoleId))
            {
                ViewBag.DismisalKind = NomenclatureConstants.LawUnitTypes.Judge;
            }
            else
            {
                ViewBag.DismisalKind = NomenclatureConstants.LawUnitTypes.Jury;
            }
            SetHelpFile(HelpFileValues.CaseLawunit);
        }

        /// <summary>
        /// Валидация преди запис на отвод
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CaseLawUnitDismisal model)
        {
            if (model.DismisalTypeId < 0)
            {
                return "Не е избран тип на отвеждане";
            }
            if (model.DismisalTypeId == NomenclatureConstants.DismisalType.Otvod || model.DismisalTypeId == NomenclatureConstants.DismisalType.SamoOtvod)
            {
                if (model.CaseSessionActId < 0)
                    return "Няма избран акт";
            }

            if ((model.Description ?? string.Empty) == string.Empty)
                return "Няма въведен мотив";

            if (model.Id < 1)
            {
                if (model.DismisalTypeId == NomenclatureConstants.DismisalType.Otvod || model.DismisalTypeId == NomenclatureConstants.DismisalType.SamoOtvod)
                {
                    if ((model.CaseSessionActId??-1) < 0)
                        return "Няма избран акт";
                    else
                    {
                        var sessionAct = actService.GetById<CaseSessionAct>(model.CaseSessionActId);

                        if (!((sessionAct.ActStateId == NomenclatureConstants.SessionActState.Enforced)|| (sessionAct.ActStateId == NomenclatureConstants.SessionActState.ComingIntoForce)))
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
            return string.Empty;
        }

        /// <summary>
        /// Запис на отвод
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditDismisal(CaseLawUnitDismisal model, string btnRedirectSelection = null)
        {
            SetViewbag(model.CaseLawUnitId);

            string _isvalid = IsValid(model);
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

        public IActionResult AddManualRoles(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawUnit, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            var caseCase = service.GetById<Case>(caseId);
            var model = new CaseLawUnit()
            {
                CourtId = caseCase.CourtId,
                CaseId = caseCase.Id,
                DateFrom = DateTime.Now
            };

            SetViewbagManualRoles(caseId);
            return View(nameof(EditManualRoles), model);
        }

        public IActionResult EditManualRoles(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLawUnit, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CaseLawUnit>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеният от Вас интервал не е намерен и/или нямате достъп до него.");
            }
            SetViewbagManualRoles(model.CaseId);
            return View(nameof(EditManualRoles), model);
        }

        void SetViewbagManualRoles(int caseId)
        {
            var caseCase = service.GetById<Case>(caseId);
            ViewBag.CaseName = caseCase.RegNumber;
            ViewBag.caseId = caseCase.Id;
            ViewBag.JudgeRoleId_ddl = nomService.GetDDL_JudgeRoleManualRoles();
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

            return string.Empty;
        }

        [HttpPost]
        public IActionResult EditManualRoles(CaseLawUnit model)
        {
            SetViewbagManualRoles(model.CaseId);

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
            if (service.CaseLawUnit_SaveData(model))
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

        /// <summary>
        /// Страница за промяна на председател и състав
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CaseLawUnitChangeDepRol(int caseId, int? caseSessionId = null)
        {
            var model = service.GetCaseLawUnitChangeDepRol(caseId, caseSessionId);
            SetViewbagCaseLawUnitChangeDepRol(model.CaseId, model.CaseSessionId);

            if (!ViewBag.hasLawUnit && !ViewBag.hasDepartment)
            {
                SetErrorMessage("Няма данни за промяна.");
                return RedirectToAction("CasePreview", "Case", new { id = caseId });
            }

            return View(nameof(CaseLawUnitChangeDepRol), model);
        }

        [HttpPost]
        public IActionResult CaseLawUnitChangeDepRol(CaseLawUnitChangeDepRolVM model)
        {
            SetViewbagCaseLawUnitChangeDepRol(model.CaseId, model.CaseSessionId);
            if (service.GetCaseLawUnitChangeDepRol_Save(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
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
            ViewBag.DepartmentId_ddl = selectListItemDepartments;
            ViewBag.hasDepartment = selectListItemDepartments.Count > 0;
            SetHelpFile(HelpFileValues.CaseLawunit);
        }

        [HttpPost]
        public IActionResult LawUnitSubstitute_LoadData(IDataTablesRequest request, int caseSessionId)
        {
            var data = service.LawUnitSubstitution_SelectForSession(caseSessionId);
            return request.GetResponse(data);
        }

        [HttpPost]
        public IActionResult LawUnitSubstitute_Apply(int substsitution_id, int from, int to, int caseSessionId)
        {
            var result = service.LawUnitSubstitution_Apply(substsitution_id, from, to, caseSessionId);
            return Json(new { isOk = result });
        }

        [HttpPost]
        public JsonResult IsExistJudgeLawUnitInCase(int caseId)
        {
            return Json(new { result = service.IsExistJudgeLawUnitInCase(caseId) });
        }
    }
}