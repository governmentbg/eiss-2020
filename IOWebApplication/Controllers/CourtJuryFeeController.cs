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
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CourtJuryFeeController : BaseController
    {
        private readonly ICommonService service;

        public CourtJuryFeeController(ICommonService _service)
        {
            service = _service;
        }

        /// <summary>
        /// Страница Ставка възнаграждение за заседатели
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.breadcrumbs = service.Breadcrumbs_ForCourtJuryFee().DeleteOrDisableLast();
            return View();
        }

        /// <summary>
        /// Извличане на данни Ставка възнаграждение за заседатели
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.CourtJuryFee_Select(userContext.CourtId);

            return request.GetResponse(data);
        }

        public void SetBreadcrums(int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = service.Breadcrumbs_ForCourtJuryFeeEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = service.Breadcrumbs_ForCourtJuryFeeAdd().DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне Ставка възнаграждение за заседатели
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            SetBreadcrums(0);

            var model = new CourtJuryFee()
            {
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now
            };

            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция Ставка възнаграждение за заседатели
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            SetBreadcrums(id);
            var model = service.GetById<CourtJuryFee>(id);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис Ставка възнаграждение за заседатели
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CourtJuryFee model)
        {
            if (!ModelState.IsValid)
            {
                SetBreadcrums(model.Id);
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            string errorMessage = "";
            if (service.CourtJuryFee_SaveData(model, ref errorMessage))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                if (errorMessage == "")
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            SetBreadcrums(model.Id);
            return View(nameof(Edit), model);
        }
    }
}