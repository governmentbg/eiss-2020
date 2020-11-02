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
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CourtDutyController : BaseController
    {
        private readonly ICourtDutyService service;

        public CourtDutyController(ICourtDutyService _service)
        {
            service = _service;
        }

        /// <summary>
        /// Страница с дежурства
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Извличане на данните за дежурства
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.CourtDuty_Select(userContext.CourtId, request.Search?.Value);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на дежурство
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            var model = new CourtDuty()
            {
                CourtId = userContext.CourtId
            };
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на дежурство
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<CourtDuty>(id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на дежурство
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CourtDuty model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CourtDuty_SaveData(model))
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

        /// <summary>
        /// Зареждане на лица за избор с CheckList
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult LawUnits(int id)
        {
            ViewBag.backUrl = Url.Action("Index", "CourtDuty");
            return View("CheckListViewVM", service.CheckListViewVM_Fill(userContext.CourtId, id));
        }

        /// <summary>
        /// Запис на избрани лица от CheckList
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult LawUnits(CheckListViewVM model)
        {
            if (service.CourtDutyLawUnit_SaveData(model))
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            ViewBag.backUrl = Url.Action("Index", "CourtDuty");
            return View("CheckListViewVM", model);
        }
    }
}