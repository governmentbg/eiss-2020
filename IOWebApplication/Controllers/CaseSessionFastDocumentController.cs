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
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseSessionFastDocumentController : BaseController
    {
        private readonly ICaseSessionFastDocumentService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICasePersonService casePersonService;
        private readonly ICaseSessionService caseSessionService;

        public CaseSessionFastDocumentController(ICaseSessionFastDocumentService _service,
                                                 INomenclatureService _nomService,
                                                 ICommonService _commonService,
                                                 ICasePersonService _casePerson,
                                                 ICaseSessionService _caseSessionService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            casePersonService = _casePerson;
            caseSessionService = _caseSessionService;
        }

        /// <summary>
        /// Списък със Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public IActionResult Index(int CaseSessionId)
        {
            SetViewbag(CaseSessionId);
            return View();
        }

        void SetViewbag(int CaseSessionId)
        {
            var modelSession = caseSessionService.CaseSessionById(CaseSessionId);
            //ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(CaseSessionId);
            //ViewBag.breadcrumbsEdit = commonService.Breadcrumbs_GetCaseSessionFastDocument(CaseSessionId);
            ViewBag.breadcrumbsEdit = commonService.Breadcrumbs_GetForCaseSession(CaseSessionId);
            ViewBag.caseSessionId = CaseSessionId;
            ViewBag.caseSessionName = modelSession.SessionType.Label + " " + modelSession.DateFrom.ToString("dd.MM.yyyy");
            ViewBag.CasePersonId_ddl = casePersonService.GetDropDownList(modelSession.CaseId, modelSession.Id, true, 0, 0, false);
            ViewBag.SessionDocTypeId_ddl = nomService.GetDropDownList<SessionDocType>();
            ViewBag.SessionDocStateId_ddl = nomService.GetDropDownList<SessionDocState>();
            SetHelpFile(HelpFileValues.SessionDoc);
        }

        /// <summary>
        /// Метод за извличане на Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseSessionId)
        {
            var data = service.CaseSessionFastDocument_Select(caseSessionId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public IActionResult Add(int caseSessionId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionFastDocument, null, AuditConstants.Operations.Append, caseSessionId))
            {
                return Redirect_Denied();
            }
            var modelSession = service.GetById<CaseSession>(caseSessionId);
            var model = new CaseSessionFastDocument()
            {
                CourtId = modelSession.CourtId ?? 0,
                CaseId = modelSession.CaseId,
                CaseSessionId = caseSessionId,
            };
            SetViewbag(caseSessionId);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionFastDocument, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CaseSessionFastDocument>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсенata от Вас сесия не е намерен и/или нямате достъп до нея.");
            }
            SetViewbag(model.CaseSessionId);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация преди запис на Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CaseSessionFastDocument model)
        {
            if (model.CasePersonId < 1)
                return "Изберете свързано лице";

            if (model.SessionDocTypeId < 1)
                return "Изберете вид документ";

            if (model.SessionDocStateId < 1)
                return "Изберете статус";

            return string.Empty;
        }

        /// <summary>
        /// Запис на Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CaseSessionFastDocument model)
        {
            SetViewbag(model.CaseSessionId);
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
            if (service.CaseSessionFastDocument_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionFastDocument, model.Id, currentId == 0);
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

        /// <summary>
        /// Сторно на Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseSessionFastDocument_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionFastDocument, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var expireObject = service.GetById<CaseSessionFastDocument>(model.Id);
            if (service.SaveExpireInfo<CaseSessionFastDocument>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseSessionFastDocument, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseSessionExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Index", "CaseSessionFastDocument", new { CaseSessionId = expireObject.CaseSessionId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        /// <summary>
        /// Копиране на документи в друго заседание
        /// </summary>
        /// <param name="caseSessionToId"></param>
        /// <returns></returns>
        public IActionResult CopyCaseSessionFastDocument(int caseSessionToId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionFastDocument, null, AuditConstants.Operations.ChoiceByList, caseSessionToId))
            {
                return Redirect_Denied();
            }

            ViewBag.breadcrumbsEdit = commonService.Breadcrumbs_GetCaseSessionFastDocument(caseSessionToId);
            SetHelpFile(HelpFileValues.SessionDoc);
            return View("CopyCaseSessionFastDocument", caseSessionToId);
        }

        /// <summary>
        /// зареждане на документи които могат да бъдат копирани
        /// </summary>
        /// <param name="caseSessionFromId"></param>
        /// <param name="caseSessionToId"></param>
        /// <returns></returns>
        public IActionResult CaseSessionFastDocument_SelectForSessionCheck(int caseSessionFromId, int caseSessionToId)
        {
            ViewBag.backUrl = Url.Action("Index", "CaseSessionFastDocument", new { CaseSessionId = caseSessionToId });
            var data = service.CaseSessionFastDocument_SelectForSessionCheck(caseSessionFromId, caseSessionToId);
            return PartialView("CheckListViewVM", data);
        }

        /// <summary>
        /// Валидация преди запис на копирани Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidCopyCaseSessionFastDocument(CheckListViewVM model)
        {
            if (model.checkListVMs == null)
                return "Няма избрани документи";

            if (model.checkListVMs != null)
            {
                if (model.checkListVMs.Where(x => x.Checked).Count() < 1)
                    return "Няма избрани документи";
            }

            return string.Empty;
        }

        /// <summary>
        /// запис на Съпровождащ документ представен в заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseSessionFastDocument_SelectForSessionCheck(CheckListViewVM model)
        {
            string _isvalid = IsValidCopyCaseSessionFastDocument(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View("CopyCaseSessionFastDocument", model.ObjectId);
            }
            
            if (service.CaseSessionFastDocument_SaveSelectForSessionCheck(model))
            {
                CheckAccess(service, SourceTypeSelectVM.CaseSessionPerson, null, AuditConstants.Operations.Update, model.ObjectId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return RedirectToAction("Index", "CaseSessionFastDocument", new { CaseSessionId = model.ObjectId });
        }

        /// <summary>
        /// Извличане на заседания зареждани в комбо от кое от тях да се копира сесията
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetDDL_CaseCaseSessionForCopy(int caseSessionId)
        {
            var model = caseSessionService.GetDDL_CaseSessionForCopy(caseSessionId);
            return Json(model);
        }
    }
}