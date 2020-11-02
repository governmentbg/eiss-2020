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
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOWebApplication.Controllers
{
    public class LoadGroupController : GlobalAdminBaseController
    {
        private readonly ILoadGroupService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;

        public LoadGroupController(ILoadGroupService _service, INomenclatureService _nomService, ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
        }

        /// <summary>
        /// Групи по натовареност
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_LoadGroup().DeleteOrDisableLast();
            return View();
        }

        /// <summary>
        /// Извличане Групи по натовареност
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.LoadGroup_Select();

            return request.GetResponse(data);
        }

        public void SetBreadcrums(int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_LoadGroupEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_LoadGroupAdd().DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне на група по натовареност
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            SetBreadcrums(0);
            return View(nameof(Edit), new LoadGroup());
        }

        /// <summary>
        /// Редакция на група по натовареност
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            SetBreadcrums(id);
            var model = service.GetById<LoadGroup>(id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на група по натовареност
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(LoadGroup model)
        {
            if (!ModelState.IsValid)
            {
                SetBreadcrums(model.Id);
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.LoadGroup_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetBreadcrums(model.Id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// натовареност по група
        /// </summary>
        /// <param name="caseGroupId"></param>
        /// <returns></returns>
        public IActionResult LoadGroupLinkList(int loadGroupId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_LoadGroupLink(loadGroupId).DeleteOrDisableLast();

            var loadgroup = service.GetById<LoadGroup>(loadGroupId);
            ViewBag.loadGroupId = loadgroup.Id;
            ViewBag.LoadGroupLabel = loadgroup.Label;

            return View();
        }

        /// <summary>
        /// Извличане на данните за натовареност по група
        /// </summary>
        /// <param name="request"></param>
        /// <param name="loadGroupId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataLoadGroupLink(IDataTablesRequest request, int loadGroupId)
        {
            var data = service.LoadGroupLink_Select(loadGroupId);

            return request.GetResponse(data);
        }

        public void SetBreadcrumsLink(int loadGroupId, int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_LoadGroupLinkEdit(loadGroupId, id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_LoadGroupLinkAdd(loadGroupId).DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне на натовареност към група
        /// </summary>
        /// <param name="loadGroupId"></param>
        /// <returns></returns>
        public IActionResult AddLoadGroupLink(int loadGroupId)
        {
            SetBreadcrumsLink(loadGroupId, 0);
            SetViewbagLoadGroupLink();

            var model = new LoadGroupLink()
            {
                LoadGroupId = loadGroupId
            };
            return View(nameof(EditLoadGroupLink), model);
        }

        /// <summary>
        /// Редакция на натовареност към група
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditLoadGroupLink(int id)
        {
            SetViewbagLoadGroupLink();
            var model = service.GetById<LoadGroupLink>(id);
            SetBreadcrumsLink(model.LoadGroupId, model.Id);
            return View(nameof(EditLoadGroupLink), model);
        }

        /// <summary>
        /// Запис на натовареност към група
        /// </summary>
        /// <param name="model"></param>
        /// <param name="caseCodesJson"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditLoadGroupLink(LoadGroupLink model, string caseCodesJson)
        {
            SetViewbagLoadGroupLink();

            List<int> caseCodes = new List<int>();
            string errCodes = "";
            try
            {
                caseCodes = JsonConvert.DeserializeObject<List<int>>(caseCodesJson);
            }
            catch (Exception ex)
            {
                errCodes = "Проблем със списъка с кодове" + ex.Message;
                ModelState.AddModelError("caseCodesJson", errCodes);
            }


            if ((!ModelState.IsValid) || (errCodes != ""))
            {
                SetBreadcrumsLink(model.LoadGroupId, model.Id);
                return View(nameof(EditLoadGroupLink), model);
            }
            var currentId = model.Id;
            string errorMessage = "";
            if (service.LoadGroupLink_SaveData(model, caseCodes, ref errorMessage))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditLoadGroupLink), new { id = model.Id });
            }
            else
            {
                if (errorMessage == "")
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            SetBreadcrumsLink(model.LoadGroupId, model.Id);
            return View(nameof(EditLoadGroupLink), model);
        }

        void SetViewbagLoadGroupLink()
        {
            ViewBag.filterCaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CourtTypeId_ddl = nomService.GetDropDownList<CourtType>();
            ViewBag.CaseInstanceId_ddl = nomService.GetDropDownList<CaseInstance>();
        }

        /// <summary>
        /// Избрани шифри за група
        /// </summary>
        /// <param name="loadGroupLinkId"></param>
        /// <returns></returns>
        public JsonResult LoadGroupLinkCodeRightList(int loadGroupLinkId)
        {
            var data = service.LoadGroupLinkCode_Select(loadGroupLinkId);
            return Json(data);
        }

        /// <summary>
        /// Шифри за избор за група
        /// </summary>
        /// <param name="caseGroupId"></param>
        /// <returns></returns>
        public JsonResult LoadGroupLinkCodeLeftList(int caseGroupId, int courtTypeId, int caseInstanceId)
        {
            var data = nomService.CaseCodeForSelect_SelectAll(caseGroupId, courtTypeId, caseInstanceId);
            return Json(data);
        }

    }
}