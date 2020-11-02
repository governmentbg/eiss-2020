// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Components;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace IOWebApplication.Controllers
{
    public class WorkingDaysController : GlobalAdminBaseController
    {
        private readonly ICommonService commonService;
        private readonly INomenclatureService nomenclatureService;

        public WorkingDaysController(ICommonService _commonService,
                                     INomenclatureService _nomenclatureService)
        {
            commonService = _commonService;
            nomenclatureService = _nomenclatureService;
        }

        [HttpGet]
        [MenuItem("WorkingDays")]
        public IActionResult Index()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_WorkingDays().DeleteOrDisableLast();

            WorkingDaysFilter filter = new WorkingDaysFilter();
            filter.DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            filter.DateTo = new DateTime(DateTime.Now.Year, 12, 31);

            return View(filter);
        }


        [HttpPost]
        [MenuItem("WorkingDays")]
        public IActionResult ListData(IDataTablesRequest request, DateTime DateFrom, DateTime DateTo)
        {
            var data = commonService.WorkingDay_GetList(DateFrom, DateTo);
            return request.GetResponse(data);
        }

        public void SetBreadcrums(int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_WorkingDaysEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_WorkingDaysAdd().DeleteOrDisableLast();
        }



        [HttpGet]
        [MenuItem("WorkingDays")]
        public IActionResult Add()
        {
            SetBreadcrums(0);
            var model = new WorkingDay();
            model.DayTypeId = CommonContants.WorkingDays.NotWorkDay; // по подразбиране
            FillViewBags();

            return View(nameof(Edit), model);
        }

              


        [HttpGet]
        [MenuItem("WorkingDays")]
        public IActionResult Edit(int Id)
        {
            SetBreadcrums(Id);

            // Извличане на основните данни за проверката
            var model = commonService.GetById<WorkingDay>(Id);

            // Проверка за наличие на запис
            if (!CheckIsRecordExist(model)) { return RedirectToAction("Index"); }

            FillViewBags();
            return View(model);
        }


        [HttpPost]
        [MenuItem("WorkingDays")]
        public IActionResult Edit(WorkingDay model)
        {
            FillViewBags();

            // Проверка за съществуващ запис за работен ден за същата дата
            if (commonService.WorkingDay_IsExist(model.Day, model.Id, model.CourtId))
            {
                SetBreadcrums(model.Id);
                SetErrorMessage(MessageConstant.Values.WorkDayExist);
                return View(model);
            }           

            // Валидация на модела
            if (!ModelState.IsValid)
            {
                SetBreadcrums(model.Id);
                return View(model);
            }

            //Запис на данните
            if (commonService.WorkingDay_SaveData(model) > 0)
            {
                this.SaveLogOperation(false, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {              
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return View(model);
            }
            return RedirectToAction("Index");
        }




        [HttpPost]
        [MenuItem("WorkingDays")]
        public IActionResult Delete(int Id)
        {
            object res = null;

            // Изтриване на работен ден
            if (commonService.WorkingDay_Delete(Id))
            {
                res = new { status = "ok", message = MessageConstant.Values.UpdateOK };
            }
            else
            {
                res = new { status = "error", message = MessageConstant.Values.UpdateFailed };
            }
            return Ok(res);
        }




        protected bool CheckIsRecordExist(Object record)
        {
            // Проверка за наличие на запис          
            if (record == null)
            {
                SetErrorMessage(MessageConstant.Values.RecordNotFound);              
                return false;
            }
            return true;
        }

        private void FillViewBags()
        {
            ViewBag.CourtId_ddl = nomenclatureService.GetDropDownList<Court>(false, true);
            ViewBag.DayTypes = nomenclatureService.GetDropDownList<DayType>(false);
        }      

    }
}