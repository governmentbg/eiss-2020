using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace IOWebApplication.Controllers
{
    public class CaseLawUnitManualJudgeController : BaseController
    {

        private readonly ICaseLawUnitService service;
        private readonly INomenclatureService nomService;

        public CaseLawUnitManualJudgeController(
            ICaseLawUnitService _service,
            INomenclatureService _nomService)
        {
            this.service = _service;
            this.nomService = _nomService;
        }

        public IActionResult Index()
        {
            var model = new CaseLawUnitManualJudgeFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1)
            };
            SetHelpFile(HelpFileValues.RecusalRegister);
            return View(model);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, CaseLawUnitManualJudgeFilterVM filter)
        {
            var data = service.LawUnitManualJudge_Select(null, filter.DateFrom, filter.DateTo, filter.CaseNumber, filter.LawUnitName);
            return request.GetResponse(data);
        }

        public IActionResult Add()
        {
            var model = new CaseLawUnitManualJudge()
            {
                DateFrom = DateTime.Now
            };
            SetViewBag();
            return View(nameof(Edit), model);
        }

        public IActionResult View(int id)
        {
            var model = service.LawUnitManualJudge_Select(id, null, null, null, null).FirstOrDefault();
            SetHelpFile(HelpFileValues.RecusalRegister);
            return View(model);
        }
        private void SetViewBag()
        {
            ViewBag.JudgeRoleId_ddl = nomService.GetDropDownList<JudgeRole>();
            SetHelpFile(HelpFileValues.RecusalRegister);
        }
        [HttpPost]
        public IActionResult Edit(CaseLawUnitManualJudge model)
        {
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                SetViewBag();
                return View(nameof(Edit), model);
            }

            var saveResult = service.LawUnitManualJudge_SaveData(model);
            if (saveResult.Result)
            {
                SaveLogOperation(true, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(View), new { id = model.Id });
            }
            else
            {
                if (!string.IsNullOrEmpty(saveResult.ErrorMessage))
                {
                    SetErrorMessage(saveResult.ErrorMessage);
                }
                else
                {
                    SetErrorMessage(MessageConstant.Values.SaveFailed);
                }
            }
            SetViewBag();
            return View(nameof(Edit), model);
        }

        private void ValidateModel(CaseLawUnitManualJudge model)
        {
            if (model.CaseId <= 0)
            {
                ModelState.AddModelError(nameof(CaseLawUnitManualJudge.CaseId), "Изберете дело");
            }
            if (model.LawUnitId <= 0)
            {
                ModelState.AddModelError(nameof(CaseLawUnitManualJudge.LawUnitId), "Изберете съдия");
            }
            if (model.JudgeRoleId <= 0)
            {
                ModelState.AddModelError(nameof(CaseLawUnitManualJudge.JudgeRoleId), "Изберете роля в делото");
            }
            if (string.IsNullOrEmpty(model.Description))
            {
                ModelState.AddModelError(nameof(CaseLawUnitTaskChange.Description), "Въведете причина за добавяне");
            }
        }

        //public IActionResult Get_WorkTaskToChange(int caseSessionActId)
        //{
        //    var tasks = taskService.Select(SourceTypeSelectVM.CaseSessionAct, caseSessionActId)
        //        .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
        //        .Where(x => WorkTaskConstants.Types.TaskCanChangeUser.Contains(x.TaskTypeId))
        //        .ToList()
        //        .OrderBy(x => x.Id)
        //        .Select(x => new SelectListItem
        //        {
        //            Value = x.Id.ToString(),
        //            Text = $"{x.TaskTypeName}-{x.UserFullName} ({x.TaskStateName})"
        //        }).ToList().AddAllItem();

        //    return Json(tasks);
        //}
    }
}
