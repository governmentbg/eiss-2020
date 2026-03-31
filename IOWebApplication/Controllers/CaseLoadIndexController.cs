using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseLoadIndexController : BaseController
    {
        private readonly ICaseLoadIndexService service;
        private readonly INomenclatureService nomService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly ICommonService commonService;
        private readonly ICaseSessionActService caseSessionActService;
        private readonly ICourtDepartmentService courtDepartmentService;

        public CaseLoadIndexController(ICaseLoadIndexService _service,
                                       INomenclatureService _nomService,
                                       ICaseLawUnitService _caseLawUnitService,
                                       ICommonService _commonService,
                                       ICourtDepartmentService _courtDepartmentService,
                                       ICaseSessionActService _caseSessionActService)
        {
            service = _service;
            nomService = _nomService;
            caseLawUnitService = _caseLawUnitService;
            commonService = _commonService;
            caseSessionActService = _caseSessionActService;
            courtDepartmentService = _courtDepartmentService;
        }

        #region Case Load Index

        /// <summary>
        /// Страница с Натовареност по дела: основни и допълнителни дейности по дело
        /// </summary>
        /// <param name="id">Ид на делото</param>
        /// <returns></returns>
        [TitleAudit(Operation = AuditConstants.Operations.List)]
        public async Task<IActionResult> Index(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLoadIndex, null, AuditConstants.Operations.View, id))
            {
                return Redirect_Denied();
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(id);
            SetHelpFile(HelpFileValues.CaseLoadIndex);
            return View(id);
        }

        /// <summary>
        /// Извличане на информация за Натовареност по дела: основни и допълнителни дейности по дело
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId, int? caseSessionId)
        {
            var data = service.CaseLoadIndex_Select(caseId, caseSessionId);
            return request.GetResponse(data);
        }

        [HttpPost]
        public IActionResult ListDataNew(IDataTablesRequest request, int caseId, int? caseSessionId)
        {
            var data = service.CaseLoadIndexNew_Select(caseId, caseSessionId);
            return request.GetResponseFetched(data);
        }

        void auditInfoCaseLoadIndex(string operation, int id, string add = "")
        {
            var caseLoadIndex = service.CaseLoadIndexVM_ByID(id);
            if (caseLoadIndex != null)
            {
                AddAuditInfo(operation, $"Име: {caseLoadIndex.LawUnitName} дейност: {caseLoadIndex.NameActivity} основна натовареност: {caseLoadIndex.IsMainActivityText}", add, $"Натовареност по дело {caseLoadIndex.CaseName}");
            }
        }

        /// <summary>
        /// Добавяне на Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="isMainActivity"></param>
        /// <returns></returns>
        public async Task<IActionResult> Add(int caseId, int? CaseSessionId, bool isMainActivity, bool isFromCase)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLoadIndex, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            var caseLawUnit = await caseLawUnitService.CaseLawUnit_Select(caseId, null).Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefaultAsync();
            var judgeReporterId = (caseLawUnit != null) ? caseLawUnit.LawUnitId : 0;
            var model = new CaseLoadIndex()
            {
                CaseId = caseId,
                CaseSessionId = CaseSessionId,
                CourtId = userContext.CourtId,
                IsMainActivity = isMainActivity,
                DateActivity = DateTime.Now,
                LawUnitId = judgeReporterId,
                ActTypeId = isFromCase ? 1 : (int?)null
            };
            SetViewbag(caseId, CaseSessionId, isMainActivity, isFromCase);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id, bool isFromCase)
        {
            var model = await service.GetByIdAsync<CaseLoadIndex>(id);
            if (model == null)
            {
                return NotFoundError("Търсеният от Вас интервал не е намерен и/или нямате достъп до него.");
            }
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseLoadIndex, id, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }
            SetViewbag(model.CaseId, model.CaseSessionId, model.IsMainActivity, isFromCase);
            auditInfoCaseLoadIndex(AuditConstants.Operations.View, id);
            model.ActTypeId = isFromCase ? 1 : (int?)null;
            return View(nameof(Edit), model);
        }

        void SetViewbag(int caseId, int? caseSessionId, bool isMainA, bool isFromCase)
        {
            ViewBag.CaseLoadAddActivityId_ddl = service.GetDDL_CaseLoadAddActivity(caseId);
            ViewBag.CaseLoadElementGroupId_ddl = service.GetDDL_CaseLoadElementGroup(caseId);
            ViewBag.LawUnitId_ddl = caseLawUnitService.CaseLawUnit_OnlyJudge_SelectForDropDownList_ValueLawUnitId(caseId, null);
            ViewBag.CaseSessionActId_ddl = caseSessionActService.GetDropDownList_CaseSessionActByCaseBySession(caseId, caseSessionId);

            if (isFromCase)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseLoadIndex(caseId);
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(caseSessionId ?? 0);

            SetHelpFile(HelpFileValues.CaseLoadIndex);
        }

        [HttpGet]
        public IActionResult GetDDL_CaseLoadElementType(int caseLoadElementGroupId)
        {
            var model = service.GetDDL_CaseLoadElementType(caseLoadElementGroupId);
            return Json(model);
        }

        /// <summary>
        /// Валидация преди запис на Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CaseLoadIndex model)
        {
            if (model.LawUnitId < 1)
                return "Изберете съдия";

            if (model.DateActivity.Year < 2000)
                return "Въведете дата";

            if (model.CaseSessionActId < 1)
                return "Изберете акт";

            if (model.IsMainActivity)
            {
                if (model.CaseLoadElementGroupId < 1)
                    return "Изберете група";

                if (model.CaseLoadElementTypeId < 1)
                    return "Изберете елемент";
            }
            else
            {
                if ((model.CaseLoadAddActivityId < 1) || (model.CaseLoadAddActivityId == null))
                    return "Изберете група";

                if (!service.IsCaseExistSessionAct(model.CaseId))
                    return "По това дело няма заседание или постановен акт.";
            }

            if (model.IsMainActivity)
            {
                if (service.IsExistCaseLoadActivity(model.Id, model.CaseId, model.IsMainActivity, model.LawUnitId, model.CaseLoadElementTypeId, model.CaseLoadAddActivityId))
                    return "Има въведена такава дейност.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CaseLoadIndex model)
        {
            SetViewbag(model.CaseId, model.CaseSessionId, model.IsMainActivity, model.ActTypeId != null);
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
            var isCase = model.ActTypeId != null;
            model.ActTypeId = null;
            if (service.CaseLoadIndex_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLoadIndex, model.Id, currentId == 0);
                auditInfoCaseLoadIndex(currentId == 0 ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model.Id);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);

                if (isCase)
                    return RedirectToAction("Index", "CaseLoadIndex", new { id = model.CaseId });
                else
                    return RedirectToAction("Preview", "CaseSession", new { id = model.CaseSessionId });

                //return RedirectToAction(nameof(Edit), new { id = model.Id, isFromCase = isCase });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        public IActionResult RecalcCaseLoadIndexByCase(int CaseId)
        {
            service.RecalcCaseLoadIndexByCase(CaseId);
            SetSuccessMessage(MessageConstant.Values.SaveOK);
            return RedirectToAction("Index", "CaseLoadIndex", new { id = CaseId });
        }

        public IActionResult RecalcCaseLoadIndexAllCase()
        {
            service.RecalcAllCase();
            SetSuccessMessage(MessageConstant.Values.SaveOK);
            return RedirectToAction("IndexElementGroupe", "CaseLoadIndex");
        }

        [HttpPost]
        public IActionResult CaseLoadIndex_ExpiredInfo(ExpiredInfoVM model)
        {
            var expireObject = service.GetById<CaseLoadIndex>(model.Id);
            if (service.SaveExpireInfo<CaseLoadIndex>(model))
            {
                auditInfoCaseLoadIndex(AuditConstants.Operations.Delete, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseLoadIndexExpireOK);
                return model.OtherBool ? Json(new { result = true, redirectUrl = Url.Action("Index", "CaseLoadIndex", new { id = expireObject.CaseId }) }) : Json(new { result = true, redirectUrl = Url.Action("Preview", "CaseSession", new { id = expireObject.CaseSessionId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        #endregion

        #region Case Load Element

        /// <summary>
        /// Страница с Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = AuditConstants.Operations.List)]
        public IActionResult IndexElementGroupe()
        {
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CriticalDataChange))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            return View();
        }

        /// <summary>
        /// Извличане на информация за Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataElementGroupe(IDataTablesRequest request)
        {
            var data = service.CaseLoadElementGroup_Select();
            return request.GetResponse(data);
        }

        void auditInfoCaseLoadElementGroup(string operation, int Id, string add = "")
        {
            var caseLoadElementGroup = service.CaseLoadElementGroupVM_ById(Id);
            if (caseLoadElementGroup != null)
            {
                AddAuditInfo(operation, $"Наказателно дело: {caseLoadElementGroup.IsNDLabel} инстанция: {caseLoadElementGroup.CaseInstanceLabel} име: {caseLoadElementGroup.Label}", add, "Вид група за натовареност по дела - основни дейности");
            }
        }

        /// <summary>
        /// Добавяне на Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddElementGroupe()
        {
            var model = new CaseLoadElementGroup()
            {
                DateStart = DateTime.Now,
                IsActive = true,
                IsAdditional = false
            };
            await SetViewbagElementGroupe();
            return View(nameof(EditElementGroupe), model);
        }

        /// <summary>
        /// Редакция на Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditElementGroupe(int id)
        {
            var model = service.GetById<CaseLoadElementGroup>(id);

            if (!string.IsNullOrEmpty(model.DocumentTypeIds))
                model.ArrayDocumentTypeIds = model.DocumentTypeIds.Split(",").Select(x => int.Parse(x).ToString()).ToArray();

            await SetViewbagElementGroupe();
            auditInfoCaseLoadElementGroup(AuditConstants.Operations.View, id);
            return View(nameof(EditElementGroupe), model);
        }

        /// <summary>
        /// Валидация преди запис на Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidElementGroupe(CaseLoadElementGroup model)
        {
            if (model.CaseInstanceId < 1)
                return "Изберете инстанция";

            //if (model.CaseTypeId < 1)
            //    return "Изберете точен вид дело";

            if (model.Label == string.Empty)
                return "Въведете име";

            return string.Empty;
        }

        /// <summary>
        /// Запис на Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditElementGroupe(CaseLoadElementGroup model)
        {
            await SetViewbagElementGroupe();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditElementGroupe), model);
            }

            string _isvalid = IsValidElementGroupe(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditElementGroupe), model);
            }

            var currentId = model.Id;
            if (service.CaseLoadElementGroup_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                auditInfoCaseLoadElementGroup(currentId == 0 ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditElementGroupe), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditElementGroupe), model);
        }

        async Task SetViewbagElementGroupe()
        {
            ViewBag.CaseInstanceId_ddl = await nomService.GetDropDownListAsync<CaseInstance>();
            ViewBag.CaseTypeId_ddl = await nomService.GetDropDownListAsync<CaseType>();
            //ViewBag.DocumentTypeId_ddl = nomService.GetDDL_DocumentTypeSortByName();
            ViewBag.ArrayDocumentTypeIds_ddl = await nomService.GetDDL_DocumentTypeSortByName();
            ViewBag.ProcessPriorityId_ddl = await nomService.GetDropDownListAsync<ProcessPriority>();
            ViewBag.CourtId_ddl = await nomService.GetCourtsAsync();
            ViewBag.CourtTypeId_ddl = await nomService.GetDropDownListAsync<CourtType>();
        }

        /// <summary>
        /// Страница с Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="CaseLoadElementGroupId"></param>
        /// <returns></returns>
        public IActionResult IndexElementType(int CaseLoadElementGroupId)
        {
            SetViewbagElementType(CaseLoadElementGroupId, 0);
            return View();
        }

        /// <summary>
        /// Извличане на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="request"></param>
        /// <param name="CaseLoadElementGroupId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseLoadElementType(IDataTablesRequest request, int CaseLoadElementGroupId)
        {
            var data = service.CaseLoadElementType_Select(CaseLoadElementGroupId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="CaseLoadElementGroupId"></param>
        /// <returns></returns>
        public IActionResult AddElementType(int CaseLoadElementGroupId)
        {
            var model = new CaseLoadElementType()
            {
                CaseLoadElementGroupId = CaseLoadElementGroupId,
                IsActive = true,
                DateStart = DateTime.Now
            };
            SetViewbagElementType(model.CaseLoadElementGroupId, 0);
            return View(nameof(EditElementType), model);
        }

        /// <summary>
        /// Редакция на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditElementType(int id)
        {
            var model = service.GetById<CaseLoadElementType>(id);
            SetViewbagElementType(model.CaseLoadElementGroupId, model.Id);
            return View(nameof(EditElementType), model);
        }

        /// <summary>
        /// валидация преди запис на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidElementType(CaseLoadElementType model)
        {
            if (model.Label == string.Empty)
                return "Въведете име";

            if (model.LoadProcent < 0)
                return "Въведете процент";

            return string.Empty;
        }

        /// <summary>
        /// Запис на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditElementType(CaseLoadElementType model)
        {
            SetViewbagElementType(model.CaseLoadElementGroupId, model.Id);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditElementType), model);
            }

            string _isvalid = IsValidElementType(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditElementType), model);
            }

            var currentId = model.Id;
            if (service.CaseLoadElementType_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditElementType), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditElementType), model);
        }

        void SetViewbagElementType(int CaseLoadElementGroupId, int CorrentId)
        {
            var caseLoadElementGroup = service.GetById<CaseLoadElementGroup>(CaseLoadElementGroupId);
            ViewBag.ReplaceCaseLoadElementTypeId_ddl = service.GetDDL_CaseLoadElementType_Replace(CorrentId);
            ViewBag.caseLoadElementGroupName = caseLoadElementGroup.Label;
            ViewBag.caseLoadElementGroupId = caseLoadElementGroup.Id;
        }

        /// <summary>
        /// Извличане на правила за елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="request"></param>
        /// <param name="CaseLoadElementTypeId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseLoadElementTypeRule(IDataTablesRequest request, int CaseLoadElementTypeId)
        {
            var data = service.CaseLoadElementTypeRule_Select(CaseLoadElementTypeId);
            return request.GetResponse(data);
        }

        public IActionResult AddElementTypeRule(int CaseLoadElementTypeId)
        {
            var caseLoad = service.GetById<CaseLoadElementType>(CaseLoadElementTypeId);
            var model = new CaseLoadElementTypeRule()
            {
                CaseLoadElementTypeId = CaseLoadElementTypeId,
                IsActive = true,
                Code = CaseLoadElementTypeId.ToString(),
                Label = caseLoad.Label,
                DateStart = DateTime.Now,
                IsCreateCase = false,
                IsCreateMotive = false,
                IsSpecialOpinion = false
            };
            SetViewbagElementTypeRule(model.CaseLoadElementTypeId);
            return View(nameof(EditElementTypeRule), model);
        }

        public IActionResult EditElementTypeRule(int id)
        {
            var model = service.GetById<CaseLoadElementTypeRule>(id);
            SetViewbagElementTypeRule(model.CaseLoadElementTypeId);
            return View(nameof(EditElementTypeRule), model);
        }

        private string IsValidElementTypeRule(CaseLoadElementTypeRule model)
        {
            //if (model.SessionTypeId < 1)
            //    return "Изберете вид заседание";

            //if (model.SessionResultId < 1)
            //    return "Изберете резултат";

            //if (model.ActTypeId < 1)
            //    return "Изберете вид акт";

            if (model.DateEnd != null)
            {
                if (model.DateStart > model.DateEnd)
                    return "Началната дата не може да е по-голяма от крайната";
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditElementTypeRule(CaseLoadElementTypeRule model)
        {
            SetViewbagElementTypeRule(model.CaseLoadElementTypeId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditElementTypeRule), model);
            }

            string _isvalid = IsValidElementTypeRule(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditElementTypeRule), model);
            }

            var currentId = model.Id;
            if (service.CaseLoadElementTypeRule_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditElementTypeRule), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditElementTypeRule), model);
        }

        void SetViewbagElementTypeRule(int CaseLoadElementTypeId)
        {
            var caseLoadElementType = service.GetById<CaseLoadElementType>(CaseLoadElementTypeId);
            ViewBag.caseLoadElementTypeName = caseLoadElementType.Label;
            ViewBag.caseLoadElementTypeId = caseLoadElementType.Id;
            var caseLoadElementGroup = service.GetById<CaseLoadElementGroup>(caseLoadElementType.CaseLoadElementGroupId);
            ViewBag.caseLoadElementGroupName = caseLoadElementGroup.Label;
            ViewBag.caseLoadElementGroupId = caseLoadElementGroup.Id;
            ViewBag.SessionTypeId_ddl = nomService.GetDropDownList<SessionType>();
            ViewBag.SessionResultId_ddl = nomService.GetDDL_SessionResult();
            ViewBag.ActTypeId_ddl = nomService.GetDropDownList<ActType>();
        }

        public IActionResult ElementTypeRule_ExpiredInfo(ExpiredInfoVM model)
        {
            var expireObject = service.GetById<CaseLoadElementTypeRule>(model.Id);
            if (service.ElementTypeRule_Expired(model))
            {
                SetSuccessMessage(MessageConstant.Values.CaseSessionExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("EditElementType", "CaseLoadIndex", new { id = expireObject.CaseLoadElementTypeId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        [HttpPost]
        public IActionResult ListDataCaseLoadElementTypeStop(IDataTablesRequest request, int CaseLoadElementTypeId)
        {
            var data = service.CaseLoadElementTypeStop_Select(CaseLoadElementTypeId);
            return request.GetResponse(data);
        }

        public IActionResult AddElementTypeStop(int CaseLoadElementTypeId)
        {
            var model = new CaseLoadElementTypeStop()
            {
                CaseLoadElementTypeId = CaseLoadElementTypeId,
            };

            SetViewbagElementTypeStop(model.CaseLoadElementTypeId);
            return View(nameof(EditElementTypeStop), model);
        }

        public IActionResult EditElementTypeStop(int id)
        {
            var model = service.GetById<CaseLoadElementTypeStop>(id);
            SetViewbagElementTypeStop(model.CaseLoadElementTypeId);
            return View(nameof(EditElementTypeStop), model);
        }

        private string IsValidElementTypeStop(CaseLoadElementTypeStop model)
        {
            if (model.CaseLoadElementTypeStopId < 1)
                return "Изберете стопиращ елемент";

            return string.Empty;
        }

        /// <summary>
        /// Запис на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditElementTypeStop(CaseLoadElementTypeStop model)
        {
            SetViewbagElementTypeStop(model.CaseLoadElementTypeId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditElementTypeStop), model);
            }

            string _isvalid = IsValidElementTypeStop(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditElementTypeStop), model);
            }

            var currentId = model.Id;
            if (service.CaseLoadElementTypeStop_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditElementTypeStop), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditElementTypeStop), model);
        }

        void SetViewbagElementTypeStop(int CaseLoadElementTypeId)
        {
            var caseLoadElementType = service.GetById<CaseLoadElementType>(CaseLoadElementTypeId);
            ViewBag.caseLoadElementTypeName = caseLoadElementType.Label;
            ViewBag.caseLoadElementTypeId = caseLoadElementType.Id;
            var caseLoadElementGroup = service.GetById<CaseLoadElementGroup>(caseLoadElementType.CaseLoadElementGroupId);
            ViewBag.caseLoadElementGroupName = caseLoadElementGroup.Label;
            ViewBag.caseLoadElementGroupId = caseLoadElementGroup.Id;
            ViewBag.CaseLoadElementTypeStopId_ddl = service.GetDDL_CaseLoadElementType_Replace(CaseLoadElementTypeId);
        }

        public IActionResult ElementTypeStop_ExpiredInfo(ExpiredInfoVM model)
        {
            var caseLoadElementTypeId = service.GetPropById<CaseLoadElementTypeStop, int>(model.Id, x => x.CaseLoadElementTypeId);
            if (service.ElementTypeStop_Expired(model))
            {
                SetSuccessMessage(MessageConstant.Values.CaseSessionExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("EditElementType", "CaseLoadIndex", new { id = caseLoadElementTypeId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        #endregion

        #region Case Load Activity

        /// <summary>
        /// Страница с Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexLoadAddActivity()
        {
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CriticalDataChange))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            return View();
        }

        /// <summary>
        /// Извличане на Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataLoadAddActivity(IDataTablesRequest request)
        {
            var data = service.CaseLoadAddActivity_Select();
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IActionResult AddLoadAddActivity()
        {
            var model = new CaseLoadAddActivity()
            {
                DateStart = DateTime.Now,
                IsActive = true
            };
            SetViewbagLoadAddActivity();
            return View(nameof(EditLoadAddActivity), model);
        }

        /// <summary>
        /// Редакция на Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditLoadAddActivity(int id)
        {
            var model = service.GetById<CaseLoadAddActivity>(id);
            SetViewbagLoadAddActivity();
            return View(nameof(EditLoadAddActivity), model);
        }

        /// <summary>
        /// Валидация преди запис на Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidLoadAddActivity(CaseLoadAddActivity model)
        {
            if (model.Label == string.Empty)
                return "Въведете име";

            return string.Empty;
        }

        /// <summary>
        /// Запис на Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditLoadAddActivity(CaseLoadAddActivity model)
        {
            SetViewbagLoadAddActivity();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditLoadAddActivity), model);
            }

            string _isvalid = IsValidLoadAddActivity(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditLoadAddActivity), model);
            }

            var currentId = model.Id;
            if (service.CaseLoadAddActivity_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditLoadAddActivity), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditLoadAddActivity), model);
        }

        void SetViewbagLoadAddActivity()
        {

        }

        /// <summary>
        /// Страница с Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="CaseLoadAddActivityId"></param>
        /// <returns></returns>
        public IActionResult IndexLoadAddActivityIndex(int CaseLoadAddActivityId)
        {
            SetViewbagLoadAddActivityIndex(CaseLoadAddActivityId);
            return View();
        }

        /// <summary>
        /// Извличане на данни Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="request"></param>
        /// <param name="CaseLoadAddActivityId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataLoadAddActivityIndex(IDataTablesRequest request, int CaseLoadAddActivityId)
        {
            var data = service.CaseLoadAddActivityIndex_Select(CaseLoadAddActivityId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="CaseLoadAddActivityId"></param>
        /// <returns></returns>
        public IActionResult AddLoadAddActivityIndex(int CaseLoadAddActivityId)
        {
            var loadAddActivity = service.GetById<CaseLoadAddActivity>(CaseLoadAddActivityId);
            var model = new CaseLoadAddActivityIndex()
            {
                CaseLoadAddActivityId = CaseLoadAddActivityId,
                Label = loadAddActivity.Label,
                IsActive = true,
                DateStart = DateTime.Now.AddMonths(-1)
            };
            SetViewbagLoadAddActivityIndex(model.CaseLoadAddActivityId);
            return View(nameof(EditLoadAddActivityIndex), model);
        }

        /// <summary>
        /// Редакция на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditLoadAddActivityIndex(int id)
        {
            var model = service.GetById<CaseLoadAddActivityIndex>(id);
            SetViewbagLoadAddActivityIndex(model.CaseLoadAddActivityId);
            return View(nameof(EditLoadAddActivityIndex), model);
        }

        /// <summary>
        /// Валидация преди запис на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidLoadAddActivityIndex(CaseLoadAddActivityIndex model)
        {
            if (model.Label == string.Empty)
                return "Въведете име";

            if (model.LoadIndex < 0)
                return "Въведете стойност";

            return string.Empty;
        }

        /// <summary>
        /// Запис Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditLoadAddActivityIndex(CaseLoadAddActivityIndex model)
        {
            SetViewbagLoadAddActivityIndex(model.CaseLoadAddActivityId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditLoadAddActivityIndex), model);
            }

            string _isvalid = IsValidLoadAddActivityIndex(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditLoadAddActivityIndex), model);
            }

            var currentId = model.Id;
            if (service.CaseLoadAddActivityIndex_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditLoadAddActivityIndex), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditLoadAddActivityIndex), model);
        }

        void SetViewbagLoadAddActivityIndex(int CaseLoadAddActivityId)
        {
            var caseLoadAddActivity = service.GetById<CaseLoadAddActivity>(CaseLoadAddActivityId);
            ViewBag.caseLoadAddActivityName = caseLoadAddActivity.Label;
            ViewBag.caseLoadAddActivityId = caseLoadAddActivity.Id;
            ViewBag.CourtTypeId_ddl = nomService.GetDropDownList<CourtType>();
        }

        #endregion

        #region Judge Load Activity

        /// <summary>
        /// Страница с Натовареност на съдии - допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexJudgeLoadActivity()
        {
            return View();
        }

        /// <summary>
        /// Извличане на информация за Натовареност на съдии - допълнителни дейности
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataJudgeLoadActivity(IDataTablesRequest request)
        {
            var data = service.JudgeLoadActivity_Select();
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Натовареност на съдии - допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IActionResult AddJudgeLoadActivity()
        {
            var model = new JudgeLoadActivity()
            {
                DateStart = DateTime.Now,
                IsActive = true
            };
            SetViewbagJudgeLoadActivity();
            return View(nameof(EditJudgeLoadActivity), model);
        }

        /// <summary>
        /// Редакция на Натовареност на съдии - допълнителни дейности
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditJudgeLoadActivity(int id)
        {
            var model = service.GetById<JudgeLoadActivity>(id);
            SetViewbagJudgeLoadActivity();
            return View(nameof(EditJudgeLoadActivity), model);
        }

        /// <summary>
        /// Валидация преди запис на Натовареност на съдии - допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidJudgeLoadActivity(JudgeLoadActivity model)
        {
            if (model.Label == string.Empty)
                return "Въведете име";

            if (model.GroupNo != null)
            {
                if (model.GroupNo < 0)
                    return "Не може да се въвежда отрицателна стойност";
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на Натовареност на съдии - допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditJudgeLoadActivity(JudgeLoadActivity model)
        {
            SetViewbagJudgeLoadActivity();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditJudgeLoadActivity), model);
            }

            string _isvalid = IsValidJudgeLoadActivity(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditJudgeLoadActivity), model);
            }

            var currentId = model.Id;
            if (service.JudgeLoadActivity_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditJudgeLoadActivity), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditJudgeLoadActivity), model);
        }

        void SetViewbagJudgeLoadActivity()
        {

        }

        /// <summary>
        /// Страница с Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="JudgeLoadActivityId"></param>
        /// <returns></returns>
        public IActionResult IndexJudgeLoadActivityIndex(int JudgeLoadActivityId)
        {
            SetViewbagJudgeLoadActivityIndex(JudgeLoadActivityId);
            return View();
        }

        /// <summary>
        /// Извличане на информация за Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="request"></param>
        /// <param name="JudgeLoadActivityId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataJudgeLoadActivityIndex(IDataTablesRequest request, int JudgeLoadActivityId)
        {
            var data = service.JudgeLoadActivityIndex_Select(JudgeLoadActivityId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="JudgeLoadActivityId"></param>
        /// <returns></returns>
        public IActionResult AddJudgeLoadActivityIndex(int JudgeLoadActivityId)
        {
            var loadAddActivity = service.GetById<CaseLoadAddActivity>(JudgeLoadActivityId);
            var model = new JudgeLoadActivityIndex()
            {
                JudgeLoadActivityId = JudgeLoadActivityId,
                IsActive = true,
                DateStart = DateTime.Now.AddMonths(-1)
            };
            SetViewbagJudgeLoadActivityIndex(model.JudgeLoadActivityId);
            return View(nameof(EditJudgeLoadActivityIndex), model);
        }

        /// <summary>
        /// Редакция на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditJudgeLoadActivityIndex(int id)
        {
            var model = service.GetById<JudgeLoadActivityIndex>(id);
            SetViewbagJudgeLoadActivityIndex(model.JudgeLoadActivityId);
            return View(nameof(EditJudgeLoadActivityIndex), model);
        }

        /// <summary>
        /// Валидация преди запис на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidJudgeLoadActivityIndex(JudgeLoadActivityIndex model)
        {
            if (model.LoadIndex < 0)
                return "Въведете стойност";

            return string.Empty;
        }

        /// <summary>
        /// Запис на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditJudgeLoadActivityIndex(JudgeLoadActivityIndex model)
        {
            SetViewbagJudgeLoadActivityIndex(model.JudgeLoadActivityId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditJudgeLoadActivityIndex), model);
            }

            string _isvalid = IsValidJudgeLoadActivityIndex(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditJudgeLoadActivityIndex), model);
            }

            var currentId = model.Id;
            if (service.JudgeLoadActivityIndex_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditJudgeLoadActivityIndex), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditJudgeLoadActivityIndex), model);
        }

        void SetViewbagJudgeLoadActivityIndex(int JudgeLoadActivityId)
        {
            var judgeLoadActivity = service.GetById<JudgeLoadActivity>(JudgeLoadActivityId);
            ViewBag.judgeLoadActivityName = judgeLoadActivity.Label;
            ViewBag.judgeLoadActivityId = judgeLoadActivity.Id;
            ViewBag.CourtTypeId_ddl = nomService.GetDropDownList<CourtType>();
        }


        public IActionResult RecalcAllCourtLawUnitActivity()
        {
            service.RecalcAllCourtLawUnitActivity();
            SetSuccessMessage(MessageConstant.Values.SaveOK);
            return RedirectToAction("IndexJudgeLoadActivity", "CaseLoadIndex");
        }

        #endregion

        #region Court Law Unit Activity

        /// <summary>
        /// Страница с Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCourtLawUnitActivity()
        {
            ViewBag.JudgeLoadActivityId_ddl = nomService.GetDropDownList<JudgeLoadActivity>();
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                Year = DateTime.Now.Year
            };
            return View(model);
        }

        /// <summary>
        /// Извличане на данни за Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCourtLawUnitActivity(IDataTablesRequest request, CaseLoadIndexFilterVM model)
        {
            var data = service.CourtLawUnitActivity_Select(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <returns></returns>
        public IActionResult AddCourtLawUnitActivity()
        {
            var model = new CourtLawUnitActivity()
            {
                CourtId = userContext.CourtId,
                ActivityDate = DateTime.Now,
                DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };
            SetViewbagCourtLawUnitActivity();
            return View(nameof(EditCourtLawUnitActivity), model);
        }

        /// <summary>
        /// Редакция на Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditCourtLawUnitActivity(int id)
        {
            var model = service.GetById<CourtLawUnitActivity>(id);
            SetViewbagCourtLawUnitActivity();
            return View(nameof(EditCourtLawUnitActivity), model);
        }

        /// <summary>
        /// Валидация преди запис на Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidCourtLawUnitActivity(CourtLawUnitActivity model)
        {
            if (model.LawUnitId < 1)
                return "Няма избран съдия";

            if (model.JudgeLoadActivityId < 1)
                return "Няма избрана дейност";

            if (model.ActivityDate.Year < 2000)
                return "Няма въведена дата";

            if (model.DateTo == null)
                return "Няма въведена до дата";

            if (model.DateTo != null)
            {
                if (model.DateTo < model.ActivityDate)
                    return "Дата до не може да бъде по малка от датата";

                //if ((model.DateTo ?? DateTime.Now).Date < DateTime.Now.Date)
                //    return "Дата до не може да е по-малка от текущата дата";

                if (model.ActivityDate.Year != (model.DateTo ?? DateTime.Now).Year)
                    return "Датите не може да са в различни години";
            }

            //if (service.IsExistCourtLawUnitActivity(model.LawUnitId, model.JudgeLoadActivityId, model.Id, model.ActivityDate))
            //    return "За тази година има избрана тази дейност или от нейната група";

            if (service.IsExistCourtLawUnitActivityNew(model.LawUnitId, model.JudgeLoadActivityId, model.Id, model.ActivityDate, model.DateTo ?? DateTime.Now))
                return "За тази година/период има избрана тази дейност";

            return string.Empty;
        }

        /// <summary>
        /// запис на Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCourtLawUnitActivity(CourtLawUnitActivity model)
        {
            SetViewbagCourtLawUnitActivity();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCourtLawUnitActivity), model);
            }

            string _isvalid = IsValidCourtLawUnitActivity(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCourtLawUnitActivity), model);
            }

            var currentId = model.Id;
            if (service.CourtLawUnitActivity_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCourtLawUnitActivity), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCourtLawUnitActivity), model);
        }

        void SetViewbagCourtLawUnitActivity()
        {
            ViewBag.JudgeLoadActivityId_ddl = nomService.GetDropDownList<JudgeLoadActivity>();
        }

        [HttpPost]
        public IActionResult CourtLawUnitActivity_ExpiredInfo(ExpiredInfoVM model)
        {
            var expireObject = service.GetById<CourtLawUnitActivity>(model.Id);
            if (service.SaveExpireInfo<CourtLawUnitActivity>(model))
            {
                SetSuccessMessage(MessageConstant.Values.CourtLawUnitActivityExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("IndexCourtLawUnitActivity", "CaseLoadIndex") });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        #endregion

        #region Report

        #region Натовареност по дела: основни и допълнителни дейности

        /// <summary>
        /// Справка за Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexSpr()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за Натовареност по дела: основни и допълнителни дейности");
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };
            ViewBagIndexSpr();
            SetHelpFile(HelpFileValues.Report24);
            return View(model);
        }

        /// <summary>
        /// Справка за Натовареност по дела: основни и допълнителни дейности - със съд
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexSprWithCourt()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за Натовареност по дела: основни и допълнителни дейности");
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                DateFrom = DateTime.Now.AddMonths(-1),
                DateTo = DateTime.Now
            };
            ViewBagIndexSpr();
            SetHelpFile(HelpFileValues.Report24);
            return View(model);
        }

        /// <summary>
        /// Извличане на данни за Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSpr(IDataTablesRequest request, CaseLoadIndexFilterVM filter)
        {
            var data = service.CaseLoadIndexSpr_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на номенклатури за натовареност по дела: основни и допълнителни дейности
        /// </summary>
        private void ViewBagIndexSpr()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CourtId_ddl = nomService.GetDropDownList<Court>();
            ViewBag.CourtDepartmentId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            ViewBag.CourtDepartmentOtdelenieId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Otdelenie);
            ViewBag.SessionTypeId_ddl = nomService.GetDropDownList<SessionType>();
            ViewBag.SessionResultId_ddl = nomService.GetDDL_SessionResult();
            ViewBag.ActTypeId_ddl = nomService.GetDropDownList<ActType>();
        }

        #endregion

        /// <summary>
        /// Натовареност по групи: основни и допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCourtGroupSpr()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за натовареност по групи: основни и допълнителни дейности");
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };

            ViewBag.CourtGroupId_ddl = nomService.GetDDL_CourtGroup(userContext.CourtId);

            return View(model);
        }

        /// <summary>
        /// Натовареност по групи: основни и допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> IndexCourtGroupSprWithCourt()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за натовареност по групи: основни и допълнителни дейности");
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                DateFrom = DateTime.Now.AddMonths(-1),
                DateTo = DateTime.Now
            };

            ViewBag.CourtGroupId_ddl = nomService.GetDDL_CourtGroup(userContext.CourtId);
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за натовареност по групи: основни и допълнителни дейности
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCourtGroupeSpr(IDataTablesRequest request, CaseLoadIndexFilterVM model)
        {
            var data = service.CaseLoadIndexCourtGroupSpr_Select(model);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка за натоварване на съдии извън дело
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexCourtLawUnitActivitySpr()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за натоварване на съдии извън дело");
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };
            SetHelpFile(HelpFileValues.Report25);
            ViewBag.JudgeLoadActivityId_ddl = nomService.GetDropDownList<JudgeLoadActivity>();

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за Натоварване на съдии извън дело
        /// </summary>
        /// <param name="request"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="LawUnitId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCourtLawUnitActivitySpr(IDataTablesRequest request, DateTime? DateFrom, DateTime? DateTo, int? LawUnitId, int JudgeLoadActivityId, int? CourtId)
        {
            var data = service.CourtLawUnitActivitySpr_Select(DateFrom ?? DateTime.Now.AddYears(-100), DateTo ?? DateTime.Now.AddYears(100), LawUnitId, JudgeLoadActivityId, CourtId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка за натовареност - извън и в дело
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexLawUnitActivitySprSpr()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за натовареност - извън и в дело");
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };
            SetHelpFile(HelpFileValues.Report26);

            return View(model);
        }

        /// <summary>
        /// Справка за натовареност - извън и в дело - със съд
        /// </summary>
        /// <returns></returns>
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public async Task<IActionResult> IndexLawUnitActivitySprSprWithCourt()
        {
            CurrentContext_SetObjectInfo("Търсене в списъчен екран за справка за натовареност - извън и в дело");
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                DateFrom = DateTime.Now.AddMonths(-1),
                DateTo = DateTime.Now
            };
            ViewBag.CourtId_ddl = await nomService.GetDropDownListAsync<Court>();
            SetHelpFile(HelpFileValues.Report26);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за Натовареност - извън и в дело
        /// </summary>
        /// <param name="request"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="LawUnitId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataLawUnitActivitySprSpr(IDataTablesRequest request, DateTime? DateFrom, DateTime? DateTo, int? LawUnitId, int? CourtId)
        {
            var data = service.LawUnitActivitySpr_Select(DateFrom ?? DateTime.Now.AddYears(-100), DateTo ?? DateTime.Now.AddYears(100), LawUnitId, CourtId);
            return request.GetResponse(data);
        }

        #endregion
    }
}