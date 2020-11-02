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
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CourtOrganizationController : AdminBaseController
    {
        private readonly ICourtOrganizationService service;
        private readonly INomenclatureService nomService;

        public CourtOrganizationController(ICourtOrganizationService _service, INomenclatureService _nomService)
        {
            service = _service;
            nomService = _nomService;
        }

        /// <summary>
        /// Страница за организационна структура на съд
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Извличане на данни за организационна структура на съд
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.CourtOrganization_Select(userContext.CourtId);
            return request.GetResponse(data);
        }

        void SetViewbag(int id = 0)
        {
            ViewBag.ParentId_ddl = service.GetDropDownList(userContext.CourtId, id);
            ViewBag.OrganizationLevelId_ddl = nomService.GetDropDownList<OrganizationLevel>();
        }

        /// <summary>
        /// Добавяне на организационна структура на съд
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            var model = new CourtOrganizationEditVM()
            {
                CourtId = userContext.CourtId,
                CourtOrganizationCaseGroups = service.FillCheckListCourtOrganizationCaseGroups()
            };
            SetViewbag();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на организационна структура на съд
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.CourtOrganization_GetById(id);
            SetViewbag(id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация преди запис на организационна структура на съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CourtOrganizationEditVM model)
        {
            if (model.OrganizationLevelId < 1)
                return "Изберете ниво";
            
            if (string.IsNullOrEmpty(model.Label))
                return "Въведете име";

            if (model.DateFrom == null)
                return "Въведете дата от";

            if (model.IsDocumentRegistry ?? false)
            {
                if (!model.CourtOrganizationCaseGroups.Any(x => x.Checked))
                {
                    return "Маркирали сте деловодна регистратура и трябва да изберете поне един вид дело.";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на организационна структура на съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CourtOrganizationEditVM model)
        {
            SetViewbag(model.Id);
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
            if (service.CourtOrganization_SaveData(model))
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
    }
}