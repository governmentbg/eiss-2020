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
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseSessionMeetingController : BaseController
    {
        private readonly ICaseSessionMeetingService service;
        private readonly ICaseSessionService sessionService;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;

        public CaseSessionMeetingController(
            ICaseSessionMeetingService _service,
            ICaseSessionService _sessionService,
            INomenclatureService _nomService,
            ICommonService _commonService)
        {
            service = _service;
            sessionService = _sessionService;
            nomService = _nomService;
            commonService = _commonService;
        }

        public IActionResult Index(int caseSessionId)
        {
            SetViewbag(caseSessionId);
            return View();
        }

        void SetViewbag(int CaseSessionId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(CaseSessionId);
            ViewBag.caseSessionId = CaseSessionId;
            ViewBag.SessionMeetingTypeId_ddl = nomService.GetDropDownList<SessionMeetingType>(false);
            ViewBag.CourtHallId_ddl = commonService.GetDropDownList_CourtHall(userContext.CourtId);
            SetHelpFile(HelpFileValues.SessionMainData);
        }

        void SetViewbagMeetengUser(int CaseSessionMeetingId)
        {
            ViewBag.caseSessionMeetingId = CaseSessionMeetingId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionMeeting(CaseSessionMeetingId);
        }

        /// <summary>
        /// Извличане на информация сесии към заседание
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseSessionId)
        {
            var data = service.CaseSessionMeeting_Select(caseSessionId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на сесии към заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public IActionResult Add(int caseSessionId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionMeeting, null, AuditConstants.Operations.Append, caseSessionId))
            {
                return Redirect_Denied();
            }
            var modelSession = service.GetById<CaseSession>(caseSessionId);
            var model = new CaseSessionMeetingEditVM()
            {
                CourtId = modelSession.CourtId,
                CaseId = modelSession.CaseId,
                CaseSessionId = caseSessionId,
                DateFrom = modelSession.DateFrom,
                DateTo = (modelSession.DateTo ?? modelSession.DateFrom),
                CourtHallId = modelSession.CourtHallId,
                IsAutoCreate = false,
                CaseSessionMeetingUser = service.GetCheckListCaseSessionMeetingUser(caseSessionId)
            };
            SetViewbag(caseSessionId);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на сесии към заседание
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionMeeting, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.CaseSessionMeetingEdit_ById(id);
            model.CaseSessionMeetingUser = service.GetCheckListCaseSessionMeetingUser(model.CaseSessionId, model.Id);
            if (model == null)
            {
                throw new NotFoundException("Търсенata от Вас сесия не е намерен и/или нямате достъп до нея.");
            }
            SetViewbag(model.CaseSessionId);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация преди запис на сесии към заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CaseSessionMeetingEditVM model)
        {
            var caseSession = service.GetById<CaseSession>(model.CaseSessionId);

            if (model.DateFrom == null)
                return "Няма въведена начална дата";
            //else
            //{
            //    if (model.DateFrom < caseSession.DateFrom)
            //        return "Началната дата е преди началната дата на заседанието";
            //}

            //if (model.DateFrom >= DateTime.Now)
            //    return "Сесията трябва да е с минала начална дата";

            if (model.DateTo != null)
            {
                //if (model.DateTo >= DateTime.Now)
                //    return "Сесията трябва да е с минала крайна дата";

                if (model.DateFrom > model.DateTo)
                    return "Началната дата е по-голяма от крайната";

                //if (model.DateTo > caseSession.DateTo)
                //    return "Крайната дата е след датата на заседанието";
            }

            if (service.IsExistMeetengInSession(model.DateFrom, model.DateTo, model.CaseSessionId, model.Id))
            {
                return "Има сесия в това заседание съвпадаща като време";
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на сесии към заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CaseSessionMeetingEditVM model)
        {
            model.DateFrom = model.DateFrom.MakeEndSeconds();
            model.DateTo = model.DateTo.MakeEndSeconds();

            SetViewbag(model.CaseSessionId);
            
            if (model.CaseSessionMeetingUser == null)
                model.CaseSessionMeetingUser = new List<Infrastructure.Models.ViewModels.CheckListVM>();

            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            model.DateTo = new DateTime(model.DateFrom.Year, model.DateFrom.Month, model.DateFrom.Day, model.DateTo.Hour, model.DateTo.Minute, model.DateTo.Second);
            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CaseSessionMeeting_SaveData(model))
            {
                //model.CaseSessionMeetingUser = service.GetCheckListCaseSessionMeetingUser(model.CaseSessionId, model.Id);
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionMeeting, model.Id, currentId == 0);
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
        /// Анулиране на сесии към заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CaseSessionMeeting_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionMeeting, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var expireObject = service.GetById<CaseSessionMeeting>(model.Id);
            if (service.SaveExpireInfo<CaseSessionMeeting>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseSessionMeeting, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseSessionMeetingExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Preview", "CaseSession", new { id = expireObject.CaseSessionId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        public IActionResult IndexMeetingUser(int caseSessionMeetingId)
        {
            ViewBag.caseSessionMeetingId = caseSessionMeetingId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionMeeting(caseSessionMeetingId, true);
            return View();
        }

        [HttpPost]
        public IActionResult ListDataMeetingUser(IDataTablesRequest request, int caseSessionMeetingId)
        {
            var data = service.CaseSessionMeetingUser_Select(caseSessionMeetingId);
            return request.GetResponse(data);
        }

        public IActionResult AddMeetingUser(int caseSessionMeetingId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionMeetingUser, null, AuditConstants.Operations.Append, caseSessionMeetingId))
            {
                return Redirect_Denied();
            }
            var meeting = service.GetById<CaseSessionMeeting>(caseSessionMeetingId);
            var model = new CaseSessionMeetingUser()
            {
                CaseId = meeting.CaseId,
                CourtId = meeting.CourtId,
                CaseSessionMeetingId = caseSessionMeetingId
            };
            SetViewbagMeetengUser(caseSessionMeetingId);
            return View(nameof(EditMeetingUser), model);
        }

        public IActionResult EditMeetingUser(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionMeetingUser, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CaseSessionMeetingUser>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеният от Вас секретар не е намерен и/или нямате достъп до него.");
            }
            SetViewbagMeetengUser(model.CaseSessionMeetingId);
            return View(nameof(EditMeetingUser), model);
        }

        private string IsValidMeetingUser(CaseSessionMeetingUser model)
        {
            if ((model.SecretaryUserId ?? string.Empty) == string.Empty)
                return "Няма избран юзер";

            return string.Empty;
        }

        [HttpPost]
        public IActionResult EditMeetingUser(CaseSessionMeetingUser model)
        {
            SetViewbagMeetengUser(model.CaseSessionMeetingId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditMeetingUser), model);
            }

            string _isvalid = IsValidMeetingUser(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditMeetingUser), model);
            }

            var currentId = model.Id;
            if (service.CaseSessionMeetingUser_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionMeetingUser, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditMeetingUser), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditMeetingUser), model);
        }

        [HttpPost]
        public JsonResult CourtHallBusyFromSession(int CourtHallId, DateTime DateFrom, int DateTo_Minutes, int CaseSessionId)
        {
            return Json(new { result = service.CourtHallBusyFromSession(CourtHallId, DateFrom, DateTo_Minutes, CaseSessionId) });
        }

        [HttpPost]
        public JsonResult CourtHallBusy(int CourtHallId, DateTime DateFrom, DateTime DateTo, int CaseSessionId)
        {
            return Json(new { result = service.CourtHallBusy(CourtHallId, DateFrom, DateTo, CaseSessionId) });
        }

        [HttpPost]
        public JsonResult IsCaseLawUnitFromCaseBusy(int caseId, int caseSessionId, DateTime dateTimeFrom, int DateTo_Minutes)
        {
            return Json(new { result = service.IsCaseLawUnitFromCaseBusy(caseId, caseSessionId, dateTimeFrom, dateTimeFrom.AddMinutes(DateTo_Minutes)) });
        }

        [HttpPost]
        public JsonResult IsCaseLawUnitFromCaseBusyMeeting(int caseId, int caseSessionId, DateTime dateTimeFrom, DateTime dateTimeТо)
        {
            return Json(new { result = service.IsCaseLawUnitFromCaseBusy(caseId, caseSessionId, dateTimeFrom, dateTimeТо) });
        }

        [HttpPost]
        public JsonResult CheckExistSecretaryOfAllMeeting(int caseSessionId)
        {
            return Json(new { result = service.CheckExistSecretaryOfAllMeeting(caseSessionId) });
        }
    }
}