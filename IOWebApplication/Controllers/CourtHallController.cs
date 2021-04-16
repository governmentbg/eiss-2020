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
    public class CourtHallController : BaseController
    {
        private readonly ICommonService service;

        public CourtHallController(ICommonService _service)
        {
            service = _service;
        }

        /// <summary>
        /// Страница със зали към съд
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            SetHelpFile(HelpFileValues.Nom14);
            return View();
        }

        /// <summary>
        /// Извличане на данни за зали
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.CourtHall_Select(userContext.CourtId);
            return request.GetResponse(data);
        }

        private void SetViewBag()
        {
            SetHelpFile(HelpFileValues.Nom14);
        }

        /// <summary>
        /// Добавяне на зала
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            var model = new CourtHall()
            {
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now
            };
            SetViewBag();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на зала
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<CourtHall>(id);
            SetViewBag();

            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация преди запис на зала
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValid(CourtHall model)
        {
            if (string.IsNullOrEmpty(model.Name))
                return "Въведете име";

            if (model.DateFrom == null)
                return "Въведете дата от";

            return string.Empty;
        }

        /// <summary>
        /// Запис на зала
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CourtHall model)
        {
            if (!ModelState.IsValid)
            {
                SetViewBag();
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                SetViewBag();
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CourtHall_SaveData(model))
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
    }
}