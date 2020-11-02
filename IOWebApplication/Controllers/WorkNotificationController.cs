// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class WorkNotificationController : BaseController
    {
        private readonly IWorkNotificationService service;
        public WorkNotificationController(IWorkNotificationService _service)
        {
            service = _service;
        }
        public IActionResult Index()
        {

            ViewBag.WorkNotificationTypeId_ddl = service.GetDDL_WorkNotificationTypes(0);
            ViewBag.ReadTypeId_ddl = service.ReadTypeId_SelectDDL();
            WorkNotificationFilterVM model = service.MakeDefaultFilter();
            return View(model);
        }
       
        public IActionResult Dashboard()
        {
            ViewBag.WorkNotificationTypeId_ddl = service.GetDDL_WorkNotificationTypes(0);
            ViewBag.ReadTypeId_ddl = service.ReadTypeId_SelectDDL();
            WorkNotificationFilterVM model = service.MakeDefaultFilter();
            return View(model);
        }
        public IActionResult DashboardReload(WorkNotificationFilterVM filterData)
        {
            ViewBag.WorkNotificationTypeId_ddl = service.GetDDL_WorkNotificationTypes(0);
            ViewBag.ReadTypeId_ddl = service.ReadTypeId_SelectDDL();
            filterData.UserId = userContext.UserId;
            filterData.CourtId = userContext.CourtId;
            filterData.SourceType = 0;
            filterData.SourceId = 0;

            return PartialView("_Dashboard", filterData);
        }
        public JsonResult SaveReaded(long id)
        {
            bool result = service.SaveWorkNotificationRead(id);
            return Json(result);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, WorkNotificationFilterVM filterData)
        {
            filterData.UserId = userContext.UserId;
            filterData.CourtId = userContext.CourtId;
            filterData.SourceType = 0;
            filterData.SourceId = 0;
            var data = service.SelectWorkNotifications(filterData);
            return request.GetResponse(data);
        }

    }
}