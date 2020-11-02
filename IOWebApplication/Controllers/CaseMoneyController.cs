// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseMoneyController : BaseController
    {
        private readonly ICaseMoneyService service;
        private readonly INomenclatureService nomService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly ICaseSessionService sessionService;

        public CaseMoneyController(ICaseMoneyService _service, INomenclatureService _nomService, ICaseLawUnitService _caseLawUnitService,
            ICaseSessionService _sessionService)
        {
            service = _service;
            nomService = _nomService;
            caseLawUnitService = _caseLawUnitService;
            sessionService = _sessionService;
        }

        public IActionResult Index(int id, int? caseSessionId)
        {
            var tcase = service.GetById<Case>(id);
            ViewBag.caseId = id;
            ViewBag.caseSessionId = caseSessionId;
            ViewBag.casenumber = tcase.EISSPNumber;

            return View();
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId, int? caseSessionId)
        {
            var data = service.CaseMoney_Select(caseId, caseSessionId);

            return request.GetResponse(data);
        }

        public IActionResult Add(int caseId, int? caseSessionId)
        {
            SetViewbag(caseId, caseSessionId);
            var model = new CaseMoney()
            {
                CaseId = caseId,
                CourtId = userContext.CourtId,
                CaseSessionId = caseSessionId
            };
            return View(nameof(Edit), model);
        }

        public IActionResult Edit(int id)
        {
            var model = service.GetById<CaseMoney>(id);
            SetViewbag(model.CaseId, model.CaseSessionId);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult Edit(CaseMoney model)
        {
            SetViewbag(model.CaseId, model.CaseSessionId);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.CaseMoney_SaveData(model))
            {
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

        void SetViewbag(int caseId, int? caseSessionId)
        {
            ViewBag.MoneyTypeId_ddl = nomService.GetDropDownList<MoneyType>();
            ViewBag.CaseLawUnitId_ddl = caseLawUnitService.CaseLawUnit_SelectForDropDownList(caseId, null);
            ViewBag.RegNumber = service.GetById<Case>(caseId).RegNumber;

            if (caseSessionId != null)
            {
                var caseSession = sessionService.CaseSessionById(caseSessionId ?? 0);
                ViewBag.CaseSessionName = caseSession.SessionType?.Label + "/" + caseSession.DateFrom.ToString("dd.MM.yyyy");
            }
        }

    }
}