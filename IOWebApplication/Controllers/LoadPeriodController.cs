using System;
using System.Linq;
using DataTables.AspNet.Core;
using IOWebApplication.Components;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
    public class LoadPeriodController : BaseController
    {
        private readonly ICommonService service;
        private readonly ICourtLoadPeriodService courtLoadPeriodService;

        public LoadPeriodController(ICommonService _service, ICourtLoadPeriodService _courtLoadPeriodService)
        {
            service = _service;
            courtLoadPeriodService = _courtLoadPeriodService;
        }

        /// <summary>
        /// Страница с периоди към съд
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            SetHelpFile(HelpFileValues.Nom18);
            return View();
        }

        /// <summary>
        /// Извличане на данни за периоди към съд
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = courtLoadPeriodService.CourtLoadResetPeriod_Select(userContext.CourtId);
            return request.GetResponse(data);
        }

        private void SetViewBag()
        {
            SetHelpFile(HelpFileValues.Nom18);
        }

        /// <summary>
        /// Добавяне на периоди към съд
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            var model = new CourtLoadResetPeriod()
            {
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now
            };
            SetViewBag();

            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на периоди към съд
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<CourtLoadResetPeriod>(id);
            SetViewBag();

            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на периоди към съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CourtLoadResetPeriod model)
        {
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                SetViewBag();
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (courtLoadPeriodService.CourtLoadResetPeriod_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetViewBag();

            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация преди запис на периоди към съд
        /// </summary>
        /// <param name="model"></param>
        void ValidateModel(CourtLoadResetPeriod model)
        {

            var lastLoadPeriods = courtLoadPeriodService.Get_CourtLoadResetPeriod_CrossPeriod(model);
            if (lastLoadPeriods.Count() > 0)
            {
                ModelState.AddModelError("", "Периода има сечение с друг период");

            }

        }
    }
}