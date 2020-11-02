// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using IO.LogOperation.Models;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using IOWebApplication.ModelBinders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IOWebApplication.Controllers
{
    public class NomenclatureController : BaseController
    {
        /// <summary>
        /// Работа с номенклатури
        /// </summary>
        private readonly INomenclatureService nomenclatureService;

        /// <summary>
        /// Локализиране на текст
        /// </summary>
        private readonly IStringLocalizer<NomenclatureController> localizer;

        /// <summary>
        /// Текущ тип на номенклатура
        /// </summary>
        private Type nomenclatureType;

        private readonly IStringLocalizer _messagelocalizer;

        /// <summary>
        /// Инжектиране на зависимости
        /// </summary>
        /// <param name="_nomenclatureRepo"></param>
        /// <param name="_localizer"></param>
        public NomenclatureController(
            INomenclatureService _nomenclatureService,
            IStringLocalizer<NomenclatureController> _localizer,
            IStringLocalizerFactory _localizerFactory)
        {
            nomenclatureService = _nomenclatureService;
            localizer = _localizer;
            _messagelocalizer = _localizerFactory.Create(LocalizationConstant.SharedResourcesName, LocalizationConstant.SharedResourcesLocation);
        }

        /// <summary>
        /// Списък с елементи на номенклатурата
        /// </summary>
        /// <param name="nomenclatureName">Име на типа на номенклатурата</param>
        /// <returns></returns>
        public IActionResult Index(string nomenclatureName)
        {
            if (nomenclatureName == null)
                return NotFound();
            TempData["NomenclatureName"] = nomenclatureName;
            TempData["Title"] = localizer[nomenclatureName]?.Value;

            return View();
        }



        /// <summary>
        /// Метод за достъп на DataTables до данните от номенклатурата
        /// </summary>
        /// <param name="request">Заявка на DataTables</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult NomenclatureListData(IDataTablesRequest request)
        {

            var method = nomenclatureService.GetType().GetMethod("GetList");
            var generic = method.MakeGenericMethod(nomenclatureType);

            IQueryable<CommonNomenclatureListItem> data = (IQueryable<CommonNomenclatureListItem>)generic
                .Invoke(nomenclatureService, null);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на нов елемент
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Add()
        {
            var model = (ICommonNomenclature)Activator.CreateInstance(nomenclatureType);
            model.IsActive = true;
            model.DateStart = new DateTime(2000, 1, 1);
            GetDropDownLists();

            return View("Edit", model);
        }

        /// <summary>
        /// Редактиране на елемент
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var method = nomenclatureService.GetType().GetMethod("GetItem");
            var generic = method.MakeGenericMethod(nomenclatureType);

            var model = Convert.ChangeType(generic.Invoke(nomenclatureService, new object[] { id }), nomenclatureType);
            GetDropDownLists();

            return View(model);
        }

        /// <summary>
        /// Запис на промените в елемент
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit([ModelBinder(typeof(NomenclatureModelBinder))]object model)
        {
            var method = nomenclatureService.GetType().GetMethod("SaveItem");
            var generic = method.MakeGenericMethod(nomenclatureType);

            var nomInstance = (BaseCommonNomenclature)model;
            bool isUpdate = nomInstance.Id > 0;
            bool result = (bool)generic.Invoke(nomenclatureService, new object[] { model });

            if (result)
            {
                this.SaveLogOperation((isUpdate) ? OperationTypes.Update : OperationTypes.Insert,
                    string.Format("{0}_{1}", TempData.Peek("NomenclatureName").ToString(), nomInstance.Id));

                TempData[MessageConstant.SuccessMessage] = _messagelocalizer["SaveOk"]?.Value;
            }
            else
            {
                TempData[MessageConstant.ErrorMessage] = _messagelocalizer["SaveFailed"]?.Value;
            }

            GetDropDownLists();

            return View(model);
        }

        /// <summary>
        /// Промяна на подредбата на елементите
        /// </summary>
        /// <param name="orderArray"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangeOrder(ChangeOrderModel model)
        {
            var method = nomenclatureService.GetType().GetMethod("ChangeOrder");
            var generic = method.MakeGenericMethod(nomenclatureType);

            bool result = (bool)generic.Invoke(nomenclatureService, new object[] { model });

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, _messagelocalizer["SaveFailed"]?.Value);
            }

            return Ok();
        }

        /// <summary>
        /// Действия, изпълнявани след изпълнението на всеки Action
        /// -- Управлява менюто, избира кое да е отворено
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            string menuItem = (string)TempData.Peek("NomenclatureName");

            if (menuItem != null)
            {
                ViewBag.MenuItemValue = menuItem;
            }

            base.OnActionExecuted(context);
        }

        /// <summary>
        /// Действия, изпълнявани преди изпълнението на всеки Action
        /// -- Избира типа на текущата номенклатура
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string nomenclatureName = (string)TempData.Peek("NomenclatureName");

            if (nomenclatureName != null)
            {
                nomenclatureType = Type.GetType(String.Format(NomenclatureConstants.AssemblyQualifiedName, nomenclatureName), false);

                if (nomenclatureType == null)
                {
                    filterContext.Result = BadRequest(String.Format("Номенклатурата не е намерена ({0})", nomenclatureName));
                    TempData.Remove("NomenclatureName");
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private void GetDropDownLists()
        {
            var fkProperties = nomenclatureType.GetProperties()
                .Where(p => p.CustomAttributes
                .Any(a => a.AttributeType == typeof(ForeignKeyAttribute)));

            foreach (var property in fkProperties)
            {
                var attrConstructorValue = property.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType == typeof(ForeignKeyAttribute))
                    ?.ConstructorArguments
                    ?.FirstOrDefault();

                if (attrConstructorValue != null && attrConstructorValue.HasValue)
                {
                    string fkPropertyName = attrConstructorValue.Value.Value.ToString();

                    var method = nomenclatureService.GetType().GetMethod("GetDropDownList");
                    var generic = method.MakeGenericMethod(property.PropertyType);

                    ViewData[fkPropertyName + "_ddl"] = Convert.ChangeType(generic.Invoke(nomenclatureService, new object[] { true, false, false }), typeof(List<SelectListItem>));
                }
            }
        }

        [Authorize(Roles = AccountConstants.Roles.GlobalAdministrator)]
        public IActionResult IndexEkStreet(string CityCodeInput)
        {
            ViewBag.CountryCode_ddl = nomenclatureService.GetCountries();
            ViewBag.StreetTipeId_ddl = nomenclatureService.GetDDL_StreetType();

            var model = new EkStreetFilterVM()
            {
                CountryCode = NomenclatureConstants.CountryBG,
                CityCode = (string.IsNullOrEmpty(CityCodeInput)) ? NomenclatureConstants.EkattCitySofiq : CityCodeInput
            };
            return View(model);
        }

        private void SetViewbagEkStreet()
        {
            ViewBag.StreetType_ddl = nomenclatureService.GetDDL_StreetType();
        }

        [HttpPost]
        public IActionResult ListDataEkStreet(IDataTablesRequest request, EkStreetFilterVM model)
        {
            var data = nomenclatureService.EkStreet_Select(model);
            return request.GetResponse(data);
        }

        private string IsValidEkStreet(EkStreet model)
        {
            if (string.IsNullOrEmpty(model.Ekatte))
                return "Изберете град";

            if (string.IsNullOrEmpty(model.Name))
                return "Въведете име";

            if (model.DateFrom == null)
                return "Въведете дата";

            if (model.StreetType < 1)
                return "Изберете вид";

            return string.Empty;
        }

        public IActionResult AddEkStreet(string CitiCode)
        {
            SetViewbagEkStreet();
            var model = new EkStreet()
            {
                Ekatte = CitiCode,
                DateFrom = DateTime.Now
            };
            return View(nameof(EditEkStreet), model);
        }

        /// <summary>
        /// Метод за редакция на адрес
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditEkStreet(int id)
        {
            SetViewbagEkStreet();
            var model = nomenclatureService.GetById<EkStreet>(id);
            return View(nameof(EditEkStreet), model);
        }

        /// <summary>
        /// Запис на адрес
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditEkStreet(EkStreet model)
        {
            SetViewbagEkStreet();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditEkStreet), model);
            }

            string _isvalid = IsValidEkStreet(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditEkStreet), model);
            }

            var currentId = model.Id;
            if (nomenclatureService.EkStreet_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditEkStreet), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditEkStreet), model);
        }
    }
}