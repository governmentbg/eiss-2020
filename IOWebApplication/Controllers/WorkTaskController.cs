// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Rotativa.Extensions;
using System;
using System.Linq;
using static IOWebApplication.Infrastructure.Constants.AccountConstants;

namespace IOWebApplication.Controllers
{
    public class WorkTaskController : BaseController
    {
        private readonly ICommonService commonService;
        private readonly INomenclatureService nomenclatureService;
        private readonly IWorkTaskService workTaskService;
        private readonly ICourtOrganizationService organizationService;

        public WorkTaskController(ICommonService _commonService,
                                  INomenclatureService _nomenclatureService,
                                  ICourtOrganizationService _organizationService,
                                  IWorkTaskService _workTaskService)
        {
            commonService = _commonService;
            nomenclatureService = _nomenclatureService;
            organizationService = _organizationService;
            workTaskService = _workTaskService;
        }

        public IActionResult Index(DateTime? dateFrom, DateTime? dateTo, int? userMode, string userId, int? taskTypeId, int? taskStateId, string sourceDescription)
        {
            WorkTaskFilterVM model = new WorkTaskFilterVM()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                UserMode = userMode ?? 1,
                UserId = userId,
                TaskTypeId = taskTypeId,
                TaskStateId = taskStateId,
                SourceDescription = sourceDescription
            };
            ViewBag.TaskTypeId_ddl = nomenclatureService.GetDropDownList<TaskType>();
            var states = nomenclatureService.GetDropDownList<TaskState>(false);
            states.Insert(0, new SelectListItem("Неприключили", WorkTaskConstants.States.NotFinishedId.ToString()));
            states.Insert(1, new SelectListItem("Всички", NomenclatureConstants.NullVal.ToString()));
            ViewBag.TaskStateId_ddl = states;
            return View(model);
        }

        public IActionResult Index_LoadData(IDataTablesRequest request, WorkTaskFilterVM model)
        {
            model.TaskTypeId = model.TaskTypeId.EmptyToNull();
            model.TaskStateId = model.TaskStateId.EmptyToNull();
            model.SourceDescription = model.SourceDescription.EmptyToNull();
            model.UserId = model.UserId.EmptyToNull("0");
            var data = workTaskService.Select(model);

            return request.GetResponse(data.AsQueryable());
        }
        public IActionResult IndexAll(DateTime? dateFrom, DateTime? dateTo, string createdBy, string assignedTo, int? taskTypeId, int? taskStateId, string sourceDescription)
        {
            WorkTaskFilterVM model = new WorkTaskFilterVM()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                CreatedBy = createdBy,
                AssignedTo = assignedTo,
                TaskTypeId = taskTypeId,
                TaskStateId = taskStateId,
                SourceDescription = sourceDescription
            };
            ViewBag.TaskTypeId_ddl = nomenclatureService.GetDropDownList<TaskType>();
            ViewBag.TaskStateId_ddl = nomenclatureService.GetDropDownList<TaskState>();
            return View(model);
        }
        public IActionResult IndexAll_LoadData(IDataTablesRequest request, WorkTaskFilterVM model)
        {
            model.TaskTypeId = model.TaskTypeId.EmptyToNull();
            model.TaskStateId = model.TaskStateId.EmptyToNull();
            model.SourceDescription = model.SourceDescription.EmptyToNull();
            model.CreatedBy = model.CreatedBy.EmptyToNull();
            model.AssignedTo = model.AssignedTo.EmptyToNull();
            var data = workTaskService.SelectAll(model);

            return request.GetResponse(data);
        }
        public IActionResult MyTasksComponent(string view = "MyTasks")
        {
            return ViewComponent("MyTasksComponent", new { view });
        }

        [DisableAudit]
        public JsonResult Select(int sourceType, long sourceId)
        {
            var model = workTaskService.Select(sourceType, sourceId);

            return Json(model);
        }
        [DisableAudit]
        public JsonResult SelectMyNewTasks(int sourceType, long sourceId)
        {
            var model = workTaskService.Select(sourceType, sourceId)
                .Where(x => x.UserId == userContext.UserId &&
                        WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId));

            return Json(model);
        }

        public ContentResult GetTaskObjectUrl(int sourceType, long sourceId)
        {
            return Content(workTaskService.GetTaskObjectUrl(sourceType, sourceId));
        }
        public IActionResult CreateTask(int sourceType, long sourceId)
        {
            var model = workTaskService.InitTask(sourceType, sourceId);
            ViewBag.TaskTypeId_ddl = workTaskService.GetDDL_TaskTypes(sourceType, sourceId);
            ViewBag.CourtOrganizationId_ddl = organizationService.CourtOrganization_SelectForDropDownList(userContext.CourtId);
            ViewBag.SelfTasks = JsonConvert.SerializeObject(workTaskService.GetSelfTask());
            ViewBag.TaskInfo = JsonConvert.SerializeObject(nomenclatureService.GetList<TaskType>());
            return PartialView("EditTask", model);
        }
        [HttpPost]
        public JsonResult CreateTask(WorkTaskEditVM model)
        {
            string validationError = ValidateTaskModel(model);
            if (!string.IsNullOrEmpty(validationError))
            {
                return Json(new { result = false, message = validationError });
            }
            return Json(new { result = workTaskService.CreateTask(model), message = "Задачата е създадена успешно." });
        }
        public IActionResult UpdateTask(long id)
        {
            var model = workTaskService.Get_ById(id);
            ViewBag.TaskTypeId_ddl = workTaskService.GetDDL_TaskTypes(model.SourceType, model.SourceId);
            ViewBag.CourtOrganizationId_ddl = organizationService.CourtOrganization_SelectForDropDownList(userContext.CourtId);
            ViewBag.SelfTasks = JsonConvert.SerializeObject(workTaskService.GetSelfTask());
            ViewBag.TaskInfo = JsonConvert.SerializeObject(nomenclatureService.GetList<TaskType>());
            return PartialView("EditTask", model);
        }
        [HttpPost]
        public JsonResult UpdateTask(WorkTaskEditVM model)
        {
            string validationError = ValidateTaskModel(model);
            if (!string.IsNullOrEmpty(validationError))
            {
                return Json(new { result = false, message = validationError });
            }
            return Json(new { result = workTaskService.UpdateTask(model), message = "Задачата е редактирана успешно." });
        }


        private string ValidateTaskModel(WorkTaskEditVM model)
        {
            model.UserId = model.UserId.EmptyToNull("0");
            string errorMessage = string.Empty;
            var taskType = commonService.GetById<TaskType>(model.TaskTypeId);
            if (model.TaskExecutionId == WorkTaskConstants.TaskExecution.ByUser && string.IsNullOrEmpty(model.UserId) && (taskType.SelfTask == false))
            {
                errorMessage = "Изберете потребител.";
            }
            if (model.TaskExecutionId == WorkTaskConstants.TaskExecution.ByOrganization && model.CourtOrganizationId <= 0 && (taskType.SelfTask == false))
            {
                errorMessage = "Изберете структура.";
            }
            if (model.Id == 0 && model.DateEnd.HasValue && model.DateEnd.Value.Date < DateTime.Now.Date)
            {
                errorMessage = "Срокът за изпълнение не може да бъде по-малък от днешна дата.";
            }
            if (!workTaskService.ValidateSourceCourt(model.SourceType, model.SourceId))
            {
                errorMessage = $"Съда е променен на {userContext.CourtName}. Презаредете текущия екран.";
            }
            return errorMessage;
        }

        public IActionResult RedirectTask(long id)
        {
            var model = workTaskService.Get_ById(id);
            model.UserId = null;
            model.CourtOrganizationId = null;
            model.TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser;
            ViewBag.CourtOrganizationId_ddl = organizationService.CourtOrganization_SelectForDropDownList(userContext.CourtId);
            return PartialView(model);
        }
        [HttpPost]
        public JsonResult RedirectTask(WorkTaskEditVM model)
        {
            string validationError = ValidateTaskModel(model);
            if (!string.IsNullOrEmpty(validationError))
            {
                return Json(new { result = false, message = validationError });
            }
            return Json(new { result = workTaskService.RedirectTask(model) });
        }

        [HttpPost]
        public JsonResult AcceptTask(long id)
        {
            return Json(new { result = workTaskService.AcceptTask(id) });
        }

        public IActionResult CompleteTask(long id)
        {
            var model = workTaskService.Select_ById(id);
            if (model == null)
            {
                return Content("Няма задача с такъв идентификатор");
            }
            if (model.UserId != userContext.UserId && !userContext.IsUserInRole(Roles.GlobalAdministrator))
            {
                return Content("Нямате достъп до търсената задача");
            }
            if (model.TaskStateId != WorkTaskConstants.States.Accepted)
            {
                return Content("Задачата не е приета или вече е приключила");
            }
            if (!workTaskService.ValidateSourceCourt(model.SourceType, model.SourceId))
            {
                return Content("Текущия съд е променен");
            }

            ViewBag.TaskActionId_ddl = workTaskService.GetDDL_TaskActions(model.TaskTypeId);
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult CompleteTask(WorkTask model)
        {
            bool result = workTaskService.CompleteTask(model);

            bool reloadNeeded = false;
            if (result)
            {
                var task = workTaskService.GetById<WorkTask>(model.Id);
                reloadNeeded = workTaskService.UpdateAfterCompleteTask(task).Result;
            }

            return Json(new { result, reloadNeeded });
        }

        #region DoAction methods

        public IActionResult DoTask_Case_SelectLawUnit(long id)
        {
            var caseId = workTaskService.GetCaseIdByDocTaskId(id);
            return RedirectToAction("Edit", "Case", new { id = caseId });
        }

        #endregion
    }
}