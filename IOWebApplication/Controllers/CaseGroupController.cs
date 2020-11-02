// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace IOWebApplication.Controllers
{
    public class CaseGroupController : GlobalAdminBaseController
    {
        private readonly ICaseGroupService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;

        public CaseGroupController(ICaseGroupService _service, INomenclatureService _nomService, ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
        }

        //основен вид дело
        public IActionResult Index()
        {

            return View();
        }

        /// <summary>
        /// Извличане на данни за основни видове дела
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.CaseGroup_Select();

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на основен вид дело
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            return View(nameof(Edit), new CaseGroup());
        }

        /// <summary>
        /// Редакция на на основен вид дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<CaseGroup>(id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на на основен вид дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CaseGroup model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.CaseGroup_SaveData(model))
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
        /// Промяна на подредбата на основен вид дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangeOrder(ChangeOrderModel model)
        {
            var caseGroup = service.GetById<CaseGroup>(model.Id);
            Func<CaseGroup, int?> orderProp = x => x.OrderNumber;
            Expression<Func<CaseGroup, int?>> setterProp = (x) => x.OrderNumber;
            bool result = service.ChangeOrder(model.Id, model.Direction == "up", orderProp, setterProp, null);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Проблем при смяна на реда");
            }

            return Ok();
        }

        //Точен вид дело
        public IActionResult CaseTypeList(int caseGroupId)
        {
            var casegroup = service.GetById<CaseGroup>(caseGroupId);
            ViewBag.caseGroupId = casegroup.Id;
            ViewBag.caseGrouplabel = casegroup.Label;
            ViewBag.breadcrumbs = new List<BreadcrumbsVM>()
            {
                new BreadcrumbsVM()
                {
                    Title = casegroup.Label,
                    Href = Url.Action("Index")
                }
            };
            return View();
        }

        /// <summary>
        /// Извличане на данни за точни видове дела
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseGroupId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseType(IDataTablesRequest request, int caseGroupId)
        {
            var data = service.CaseType_Select(caseGroupId);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на точен вид дело
        /// </summary>
        /// <param name="caseGroupId"></param>
        /// <returns></returns>
        public IActionResult AddCaseType(int caseGroupId)
        {
            SetViewbagCaseType();

            var model = new CaseType()
            {
                CaseGroupId = caseGroupId
            };
            return View(nameof(EditCaseType), model);
        }

        /// <summary>
        /// Редакция на Точен вид дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditCaseType(int id)
        {
            SetViewbagCaseType();
            var model = service.GetById<CaseType>(id);
            return View(nameof(EditCaseType), model);
        }

        /// <summary>
        /// Запис на Точен вид дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCaseType(CaseType model)
        {
            SetViewbagCaseType();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditCaseType), model);
            }
            var currentId = model.Id;
            if (service.CaseType_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCaseType), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCaseType), model);
        }

        /// <summary>
        /// Промяна на подредбата на точен вид дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangeOrderCaseType(ChangeOrderModel model)
        {
            var caseType = service.GetById<CaseType>(model.Id);
            Func<CaseType, int?> orderProp = x => x.OrderNumber;
            Expression<Func<CaseType, int?>> setterProp = (x) => x.OrderNumber;
            Expression<Func<CaseType, bool>> predicate = x => x.CaseGroupId == caseType.CaseGroupId;
            bool result = service.ChangeOrder(model.Id, model.Direction == "up", orderProp, setterProp, predicate);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Проблем при смяна на реда");
            }

            return Ok();
        }

        void SetViewbagCaseType()
        {
        }



        //Списък шифри
        public IActionResult CaseCodeList()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_CaseCode().DeleteOrDisableLast();

            ViewBag.filterCaseTypeId_ddl = nomService.GetDropDownList<CaseType>();
            ViewBag.filterCaseTypeId = -1;

            return View();
        }

        /// <summary>
        /// Извличане на данни за шифри
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseTypeId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCaseCode(IDataTablesRequest request, int caseTypeId)
        {
            var data = service.CaseCode_Select(caseTypeId);

            return request.GetResponse(data);
        }

        public void SetBreadcrumsCaseCode(int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_CaseCodeEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_CaseCodeAdd().DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне на шифър
        /// </summary>
        /// <returns></returns>
        public IActionResult AddCaseCode()
        {
            SetBreadcrumsCaseCode(0);
            var model = new CaseCode()
            {
            };
            return View(nameof(EditCaseCode), model);
        }

        /// <summary>
        /// Редакция на шифър
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditCaseCode(int id)
        {
            SetBreadcrumsCaseCode(id);
            var model = service.GetById<CaseCode>(id);
            return View(nameof(EditCaseCode), model);
        }

        /// <summary>
        /// Запис на шифър
        /// </summary>
        /// <param name="model"></param>
        /// <param name="typesJson"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCaseCode(CaseCode model, string typesJson)
        {
            if (!ModelState.IsValid)
            {
                SetBreadcrumsCaseCode(model.Id);
                return View(nameof(EditCaseCode), model);
            }
            var currentId = model.Id;
            List<int> codeTypes = JsonConvert.DeserializeObject<List<int>>(typesJson);
            if (service.CaseCode_SaveData(model, codeTypes))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCaseCode), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetBreadcrumsCaseCode(model.Id);
            return View(nameof(EditCaseCode), model);
        }

        /// <summary>
        /// Промяна на подредбата на точен вид дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangeOrderCaseCode(ChangeOrderModel model)
        {
            var caseCode = service.GetById<CaseCode>(model.Id);
            Func<CaseCode, int?> orderProp = x => x.OrderNumber;
            Expression<Func<CaseCode, int?>> setterProp = (x) => x.OrderNumber;
            bool result = service.ChangeOrder(model.Id, model.Direction == "up", orderProp, setterProp, null);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Проблем при смяна на реда");
            }

            return Ok();
        }

        /// <summary>
        /// Списък избрани шифри за точен вид дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult CaseTypeRightList(int id)
        {            
            IQueryable<MultiSelectTransferVM> data;
            if (id > 0)
                data = service.CaseTypeForSelect_Select(id);
            else
                data = Enumerable.Empty<MultiSelectTransferVM>().AsQueryable();
            return Json(data);
        }

        /// <summary>
        /// Списък с шифри за избор към точен вид дело
        /// </summary>
        /// <returns></returns>
        public JsonResult CaseTypeLeftList()
        {
            var data = service.CaseTypeForSelect_Select(0);
            return Json(data);
        }

        #region CaseTypeUnit

        /// <summary>
        /// Състави към точен вид дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult IndexCaseTypeUnit(int id)
        {
            ViewBag.caseTypeId = id;
            var caseType = service.GetById<CaseType>(id);
            var caseGroup = service.GetById<CaseGroup>(caseType.CaseGroupId);
            ViewBag.breadcrumbs = new List<BreadcrumbsVM>()
            {
                new BreadcrumbsVM()
                {
                    Title = caseGroup.Label,
                    Href = Url.Action("Index")
                },
                new BreadcrumbsVM()
                {
                    Title = caseType.Label,
                    Href = Url.Action(nameof(CaseTypeList),new{ caseGroupId=caseType.CaseGroupId})
                }
            };

            return View();
        }

        /// <summary>
        /// Извличане на данни за състави към точен вид дело
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseTypeUnitId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataTypeUnit(IDataTablesRequest request, int caseTypeUnitId)
        {
            var data = service.CaseTypeUnit_Select(caseTypeUnitId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на състав към точен вид дело
        /// </summary>
        /// <param name="caseTypeId"></param>
        /// <returns></returns>
        public IActionResult AddTypeUnit(int caseTypeId)
        {
            var model = new CaseTypeUnitEditVM()
            {
                CaseTypeId = caseTypeId,
                IsActive = true,
                CaseTypeUnitCounts = service.GetList_CaseTypeUnitCounts()
            };
            return View(nameof(EditTypeUnit), model);
        }

        /// <summary>
        /// Редакция на състав към точен вид дело
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditTypeUnit(int id)
        {
            var model = service.GetById_CaseTypeUnit(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеният от Вас интервал не е намерен и/или нямате достъп до него.");
            }
            return View(nameof(EditTypeUnit), model);
        }

        /// <summary>
        /// Валидация преди запис на състав
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidTypeUnit(CaseTypeUnitEditVM model)
        {
            if (model.Label == string.Empty)
                return "Няма въведено име";

            if (!model.CaseTypeUnitCounts.Any(x => x.Value > 0))
                return "Няма въведена поне 1бройка за състава";

            return string.Empty;
        }

        /// <summary>
        /// Запис на състав към точен вид дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditTypeUnit(CaseTypeUnitEditVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(EditTypeUnit), model);
            }

            string _isvalid = IsValidTypeUnit(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditTypeUnit), model);
            }

            var currentId = model.Id;
            if (service.CaseTypeUnit_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditTypeUnit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditTypeUnit), model);
        }

        #endregion
    }
}