using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace IOWebApplication.Controllers
{
    public class CaseLawUnitTaskChangeController : BaseController
    {

        private readonly ICaseLawUnitTaskChangeService service;
        private readonly IWorkTaskService taskService;

        public CaseLawUnitTaskChangeController(
            ICaseLawUnitTaskChangeService _service,
            IWorkTaskService _taskService)
        {
            this.service = _service;
            this.taskService = _taskService;
        }

        public IActionResult Index()
        {
            var model = new CaseLawUnitTaskChangeFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1)
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, CaseLawUnitTaskChangeFilterVM filter)
        {
            var data = service.Select(null, filter.DateFrom, filter.DateTo, filter.CaseNumber, filter.NewUserName);
            return request.GetResponse(data);
        }

        public IActionResult Add()
        {
            var model = new CaseLawUnitTaskChange();
            ViewBag.courtId = userContext.CourtId;
            return View(nameof(Edit), model);
        }

        public IActionResult View(int id)
        {
            var model = service.Select(id, null, null, null, null).FirstOrDefault();
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(CaseLawUnitTaskChange model)
        {
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            var saveResult = service.SaveData(model);
            if (saveResult.Result)
            {
                SaveLogOperation(true, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(View), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(Edit), model);
        }

        private void ValidateModel(CaseLawUnitTaskChange model)
        {
            if (model.CaseId <= 0)
            {
                ModelState.AddModelError(nameof(CaseLawUnitTaskChange.CaseId), "Изберете дело");
            }
            if (model.CaseSessionActId <= 0)
            {
                ModelState.AddModelError(nameof(CaseLawUnitTaskChange.CaseSessionActId), "Изберете акт");
            }
            if (model.WorkTaskId <= 0)
            {
                ModelState.AddModelError(nameof(CaseLawUnitTaskChange.WorkTaskId), "Изберете задача");
            }
            if (string.IsNullOrEmpty(model.NewTaskUserId))
            {
                ModelState.AddModelError(nameof(CaseLawUnitTaskChange.NewTaskUserId), "Изберете потребител");
            }
            if (string.IsNullOrEmpty(model.Description))
            {
                ModelState.AddModelError(nameof(CaseLawUnitTaskChange.Description), "Въведете причина за заместване");
            }
        }

        public IActionResult Get_WorkTaskToChange(int caseSessionActId)
        {
            var tasks = taskService.Select(SourceTypeSelectVM.CaseSessionAct, caseSessionActId)
                .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                .Where(x => WorkTaskConstants.Types.TaskCanChangeUser.Contains(x.TaskTypeId))
                .ToList()
                .OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.TaskTypeName}-{x.UserFullName} ({x.TaskStateName})"
                }).ToList().SingleOrChoose();

            return Json(tasks);
        }
    }
}
