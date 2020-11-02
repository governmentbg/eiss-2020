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
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CommonController : BaseController
    {
        private readonly ICommonService service;
        private readonly INomenclatureService nomenclatureService;

        public CommonController(ICommonService _service,
                                INomenclatureService _nomenclatureService)
        {
            service = _service;
            nomenclatureService = _nomenclatureService;
        }

        /// <summary>
        /// Страница с адреси
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexAddress()
        {
            ViewBag.AddressTypeId_ddl = nomenclatureService.GetDropDownList<AddressType>();
            ViewBag.CountryCode_ddl = nomenclatureService.GetCountries();
            var model = new AddressFilterVM()
            {
                CountryCode = NomenclatureConstants.CountryBG
            };
            return View(model);
        }

        /// <summary>
        /// Метод за извличане на данни за адреси
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataAddress(IDataTablesRequest request, AddressFilterVM model)
        {
            var data = service.Address_Select(model);
            return request.GetResponse(data);
        }

        private void SetViewbagAddress()
        {
            ViewBag.AddressTypesDDL = nomenclatureService.GetDropDownList<AddressType>();
            ViewBag.CountriesDDL = nomenclatureService.GetCountries();
        }

        /// <summary>
        /// Валидация при добавяне на адрес
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidAddress(Address model)
        {
            if (model.AddressTypeId < 1)
                return "Изберете вид адрес";

            if (string.IsNullOrEmpty(model.CountryCode))
                return "Изберете държава";

            if (string.IsNullOrEmpty(model.CityCode))
                return "Изберете населено място";

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на нов адрес
        /// </summary>
        /// <returns></returns>
        public IActionResult AddAddress()
        {
            SetViewbagAddress();
            var model = new Address()
            { 
                CountryCode = NomenclatureConstants.CountryBG
            };
            return View(nameof(EditAddress), model);
        }

        /// <summary>
        /// Метод за редакция на адрес
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditAddress(long id)
        {
            SetViewbagAddress();
            var model = service.GetById<Address>(id);
            return View(nameof(EditAddress), model);
        }

        /// <summary>
        /// Запис на адрес
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditAddress(Address model)
        {
            SetViewbagAddress();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditAddress), model);
            }

            string _isvalid = IsValidAddress(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditAddress), model);
            }

            var currentId = model.Id;
            if (service.Address_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditAddress), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditAddress), model);
        }

        /// <summary>
        /// Страница с банкови сметки към обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult BankAccount(int sourceType, long sourceId)
        {
            ViewBag.sourceType = sourceType;
            ViewBag.sourceId = sourceId;
            ViewBag.breadcrumbs = service.BankAccount_LoadBreadCrumbs(sourceType, sourceId);

            return View();
        }

        /// <summary>
        /// Извличане на данните за банкови сметки към обект
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BankAccountListData(IDataTablesRequest request, int sourceType, long sourceId)
        {
            var data = service.BankAccount_Select(sourceType, sourceId);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на банкови сметки към обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult AddBankAccount(int sourceType, long sourceId)
        {
            SetViewBagBankAccount(sourceType, sourceId);
            BankAccountEditVM model = new BankAccountEditVM();
            model.SourceType = sourceType;
            model.SourceId = sourceId;
            return View(nameof(EditBankAccount), model);
        }

        /// <summary>
        /// Редакция на банкови сметки към обект
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditBankAccount(int id)
        {
            var model = service.BankAccount_GetById(id);
            SetViewBagBankAccount(model.SourceType, model.SourceId);
            return View(nameof(EditBankAccount), model);
        }

        /// <summary>
        /// Запис на банкови сметки към обект
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditBankAccount(BankAccountEditVM model)
        {
            if (!ModelState.IsValid)
            {
                SetViewBagBankAccount(model.SourceType, model.SourceId);
                return View(nameof(EditBankAccount), model);
            }
            if (service.BankAccount_SaveData(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditBankAccount), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetViewBagBankAccount(model.SourceType, model.SourceId);
            return View(nameof(EditBankAccount), model);
        }

        private void SetViewBagBankAccount(int sourceType, long sourceId)
        {
            ViewBag.breadcrumbs = service.BankAccount_LoadBreadCrumbsAddEdit(sourceType, sourceId);
        }
    }
}
