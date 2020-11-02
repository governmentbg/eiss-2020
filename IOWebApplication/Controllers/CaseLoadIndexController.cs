// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.AspNetCore.Mvc;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;

namespace IOWebApplication.Controllers
{
    public class CaseLoadIndexController : BaseController
    {
        private readonly ICaseLoadIndexService service;
        private readonly INomenclatureService nomService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly ICommonService commonService;

        public CaseLoadIndexController(ICaseLoadIndexService _service, 
                                       INomenclatureService _nomService, 
                                       ICaseLawUnitService _caseLawUnitService,
                                       ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            caseLawUnitService = _caseLawUnitService;
            commonService = _commonService;
        }

        #region Case Load Index

        /// <summary>
        /// Страница с Натовареност по дела: основни и допълнителни дейности по дело
        /// </summary>
        /// <param name="id">Ид на делото</param>
        /// <returns></returns>
        public IActionResult Index(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLoadIndex, null, AuditConstants.Operations.View, id))
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

        /// <summary>
        /// Добавяне на Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="isMainActivity"></param>
        /// <returns></returns>
        public IActionResult Add(int caseId, int? CaseSessionId, bool isMainActivity, bool isFromCase)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLoadIndex, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            var caseLawUnit = caseLawUnitService.CaseLawUnit_Select(caseId, null).Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
            var judgeReporterId = (caseLawUnit != null) ? caseLawUnit.LawUnitId : 0;
            var model = new CaseLoadIndex()
            {
                CaseId = caseId,
                CaseSessionId = CaseSessionId,
                CourtId = userContext.CourtId,
                IsMainActivity = isMainActivity,
                DateActivity = DateTime.Now,
                LawUnitId = judgeReporterId,
                DescriptionExpired = isFromCase ? "Case" : null
            };
            SetViewbag(caseId, CaseSessionId, isMainActivity, isFromCase);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id, bool isFromCase)
        {
            var model = service.GetById<CaseLoadIndex>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеният от Вас интервал не е намерен и/или нямате достъп до него.");
            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseLoadIndex, id, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }
            SetViewbag(model.CaseId, model.CaseSessionId, model.IsMainActivity, isFromCase);
            model.DescriptionExpired = isFromCase ? "Case" : null;
            return View(nameof(Edit), model);
        }

        void SetViewbag(int caseId, int? caseSessionId, bool isMainA, bool isFromCase)
        {
            ViewBag.CaseLoadAddActivityId_ddl = service.GetDDL_CaseLoadAddActivity(caseId);
            ViewBag.CaseLoadElementGroupId_ddl = service.GetDDL_CaseLoadElementGroup(caseId);
            ViewBag.LawUnitId_ddl = caseLawUnitService.CaseLawUnit_OnlyJudge_SelectForDropDownList_ValueLawUnitId(caseId, null);

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

            if (model.DateActivity == null)
                return "Въведете дата";

            if (model.IsMainActivity)
            {
                if (model.CaseLoadElementGroupId < 1)
                    return "Изберете група";

                if (model.CaseLoadElementTypeId < 1)
                    return "Изберете елемент";
            }
            else
            {
                if (model.CaseLoadAddActivityId < 1)
                    return "Изберете група";
            }

            if (service.IsExistCaseLoadActivity(model.Id, model.CaseId, model.IsMainActivity, model.LawUnitId, model.CaseLoadElementTypeId, model.CaseLoadAddActivityId))
                return "Има въведена такава дейност.";

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
            SetViewbag(model.CaseId, model.CaseSessionId, model.IsMainActivity, model.DescriptionExpired != null);
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
            var isCase = model.DescriptionExpired != null;
            if (service.CaseLoadIndex_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseLoadIndex, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id, isFromCase = isCase });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        #endregion

        #region Case Load Element

        /// <summary>
        /// Страница с Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexElementGroupe()
        {
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

        /// <summary>
        /// Добавяне на Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <returns></returns>
        public IActionResult AddElementGroupe()
        {
            var model = new CaseLoadElementGroup()
            {
                DateStart = DateTime.Now,
                IsActive = true
            };
            SetViewbagElementGroupe();
            return View(nameof(EditElementGroupe), model);
        }

        /// <summary>
        /// Редакция на Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditElementGroupe(int id)
        {
            var model = service.GetById<CaseLoadElementGroup>(id);
            SetViewbagElementGroupe();
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

            if (model.CaseTypeId < 1)
                return "Изберете точен вид дело";

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
        public IActionResult EditElementGroupe(CaseLoadElementGroup model)
        {
            SetViewbagElementGroupe();
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
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditElementGroupe), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditElementGroupe), model);
        }

        void SetViewbagElementGroupe()
        {
            ViewBag.CaseInstanceId_ddl = nomService.GetDropDownList<CaseInstance>();
            ViewBag.CaseTypeId_ddl = nomService.GetDropDownList<CaseType>();
            ViewBag.DocumentTypeId_ddl = nomService.GetDDL_DocumentTypeSortByName();
            ViewBag.ProcessPriorityId_ddl = nomService.GetDropDownList<ProcessPriority>();
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
                DateStart = DateTime.Now
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

        #endregion

        #region Case Load Activity

        /// <summary>
        /// Страница с Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexLoadAddActivity()
        {
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

        #endregion

        #region Court Law Unit Activity

        /// <summary>
        /// Страница с Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCourtLawUnitActivity()
        {
            return View();
        }

        /// <summary>
        /// Извличане на данни за Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCourtLawUnitActivity(IDataTablesRequest request)
        {
            var data = service.CourtLawUnitActivity_Select(userContext.CourtId);
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
                ActivityDate = DateTime.Now
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

            if (model.ActivityDate == null)
                return "Няма въведена дата";

            if (model.DateTo != null)
            {
                if (model.DateTo < model.ActivityDate)
                    return "Дата до не може да бъде по малка от датата";

                if ((model.DateTo ?? DateTime.Now).Date < DateTime.Now.Date)
                    return "Дата до не може да е по-малка от текущата дата";
            }

            if (service.IsExistCourtLawUnitActivity(model.LawUnitId, model.JudgeLoadActivityId, model.Id, model.ActivityDate))
                return "За тази година има избрана тази дейност или от нейната група";

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

        #endregion

        #region Report

        /// <summary>
        /// Справка за Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexSpr()
        {
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="request"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="LawUnitId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSpr(IDataTablesRequest request, DateTime? DateFrom, DateTime? DateTo, int? LawUnitId)
        {
            var data = service.CaseLoadIndexSpr_Select(DateFrom ?? DateTime.Now, DateTo ?? DateTime.Now, LawUnitId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка за Натоварване на съдии извън дело
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexCourtLawUnitActivitySpr()
        {
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };

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
        public IActionResult ListDataCourtLawUnitActivitySpr(IDataTablesRequest request, DateTime? DateFrom, DateTime? DateTo, int? LawUnitId)
        {
            var data = service.CourtLawUnitActivitySpr_Select(DateFrom ?? DateTime.Now, DateTo ?? DateTime.Now, LawUnitId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Справка за Натовареност - извън и в дело
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexLawUnitActivitySprSpr()
        {
            CaseLoadIndexFilterVM model = new CaseLoadIndexFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };

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
        public IActionResult ListDataLawUnitActivitySprSpr(IDataTablesRequest request, DateTime? DateFrom, DateTime? DateTo, int? LawUnitId)
        {
            var data = service.LawUnitActivitySpr_Select(DateFrom ?? DateTime.Now, DateTo ?? DateTime.Now, LawUnitId);
            return request.GetResponse(data);
        }

        #endregion
    }
}