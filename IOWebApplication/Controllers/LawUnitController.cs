using System;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IOWebApplication.Components;
using IOWebApplication.Infrastructure.Constants;
using System.Linq;

namespace IOWebApplication.Controllers
{
    public class LawUnitController : BaseController
    {
        private readonly ICommonService commonService;
        private readonly INomenclatureService nomService;

        public LawUnitController(ICommonService _commonService, INomenclatureService _nomService)
        {
            commonService = _commonService;
            nomService = _nomService;
        }

        private void SetHelpByLawUnitType(int lawUnitTypeId)
        {
            switch (lawUnitTypeId)
            {
                case NomenclatureConstants.LawUnitTypes.Judge:
                    SetHelpFile(HelpFileValues.Nom1);
                    return;
                case NomenclatureConstants.LawUnitTypes.OtherEmployee:
                    SetHelpFile(HelpFileValues.Nom2);
                    return;
                case NomenclatureConstants.LawUnitTypes.MessageDeliverer:
                    SetHelpFile(HelpFileValues.Nom3);
                    return;
                case NomenclatureConstants.LawUnitTypes.Jury:
                    SetHelpFile(HelpFileValues.Nom5);
                    return;
                case NomenclatureConstants.LawUnitTypes.Expert:
                    SetHelpFile(HelpFileValues.Nom6);
                    return;
                default:
                    return;
            }
        }

        /// <summary>
        /// Страница с лица по даден вид
        /// </summary>
        /// <param name="lawUnitType"></param>
        /// <returns></returns>
        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        public IActionResult Index(int lawUnitType)
        {
            ViewBag.lawUnitType = commonService.GetById<LawUnitType>(lawUnitType);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForLawUnit(lawUnitType).DeleteOrDisableLast();
            ViewBag.SpecialityId_ddl = nomService.GetDDL_SpecialityForFilter(lawUnitType);
            LawUnitFilterVM model = new LawUnitFilterVM();
            model.SpecialityId = -1;
            SetHelpByLawUnitType(lawUnitType);
            return View(model);
        }

        /// <summary>
        /// Извличане на данни за лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="lawUnitType"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="fullName"></param>
        /// <param name="specialityId"></param>
        /// <param name="showFree"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int lawUnitType, DateTime? fromDate, DateTime? toDate, string fullName, int specialityId, bool showFree)
        {

            var data = commonService.LawUnit_Select(lawUnitType, fullName, fromDate, toDate, specialityId, showFree);

            return request.GetResponse(data);
        }

        public void SetBreadcrums(int lawUnitTypeId, int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForLawUnitEdit(lawUnitTypeId, id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForLawUnitAdd(lawUnitTypeId).DeleteOrDisableLast();

            SetHelpByLawUnitType(lawUnitTypeId);
        }

        /// <summary>
        /// Добавяне на лице
        /// </summary>
        /// <param name="lawUnitType"></param>
        /// <returns></returns>
        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        public IActionResult Add(int lawUnitType)
        {
            SetBreadcrums(lawUnitType, 0);
            var model = new LawUnit()
            {
                LawUnitTypeId = lawUnitType,
                DateFrom = DateTime.Now
            };
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на лице
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        public IActionResult Edit(int id)
        {
            var model = commonService.GetById<LawUnit>(id);
            SetBreadcrums(model.LawUnitTypeId, id);

            return View(model);
        }

        /// <summary>
        /// Запис на лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(LawUnit model)
        {
            SetBreadcrums(model.LawUnitTypeId, model.Id);
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentId = model.Id;
            if (commonService.LawUnit_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return View(model);
            }
        }

        /// <summary>
        /// Валидация на лице преди запис
        /// </summary>
        /// <param name="model"></param>
        void ValidateModel(LawUnit model)
        {
            if (!string.IsNullOrEmpty(model.Code))
            {
                model.Code = model.Code.Trim();
            }

            switch (model.LawUnitTypeId)
            {
                case NomenclatureConstants.LawUnitTypes.Lawyer:
                    if (string.IsNullOrEmpty(model.Code))
                    {
                        ModelState.AddModelError(nameof(model.Code), "Въведете 'Номер на адвокат'");
                    }
                    if (string.IsNullOrEmpty(model.Department))
                    {
                        ModelState.AddModelError(nameof(model.Department), "Въведете 'Колегия'");
                    }
                    break;
                default:
                    if (model.DateTo == null)
                    {
                        if (!NomenclatureConstants.LawUnitTypes.NoApointmentPersons.Contains(model.LawUnitTypeId))
                        {
                            if (string.IsNullOrEmpty(model.Uic))
                            {
                                ModelState.AddModelError(nameof(model.Uic), "Въведете ЕГН");
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(model.Uic) && string.IsNullOrEmpty(model.Code))
                            {
                                ModelState.AddModelError(nameof(model.Code), "Въведете 'Код' или 'ЕГН'");
                            }
                        }
                    }
                    break;
            }


            if (string.IsNullOrEmpty(model.FirstName))
            {
                ModelState.AddModelError(nameof(model.FirstName), "Въведете поне едно име");
            }

            if (model.DateTo.HasValue && model.DateTo.Value < DateTime.Now.Date)
            {
                ModelState.AddModelError(nameof(model.DateTo), "Не можете да деактивирате лице със задна дата.");
            }

            var valMessage = commonService.LawUnit_Validate(model);
            if (!string.IsNullOrEmpty(valMessage))
            {
                ModelState.AddModelError(nameof(model.Code), valMessage);
            }

            //if (!string.IsNullOrEmpty(model.Uic))
            //{
            //    if (commonService.IsExistLawUnit_ByUicUicType(model.Uic, ((model.Id < 1) ? null : (int?)model.Id)))
            //        ModelState.AddModelError(nameof(model.Uic), "Има лице с този идентификатор.");
            //}
        }

        /// <summary>
        /// Търсене на лице
        /// </summary>
        /// <param name="lawUnitType"></param>
        /// <param name="lawUnitTypes"></param>
        /// <param name="query"></param>
        /// <param name="courtId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SearchLawUnit(int lawUnitType, string lawUnitTypes, string query, int courtId, string selectmode = NomenclatureConstants.LawUnitSelectMode.CurrentWithHistory)
        {
            return Json(commonService.GetLawUnitAutoComplete(lawUnitType, lawUnitTypes, query, courtId, selectmode));
        }

        /// <summary>
        /// Извличане на лице по ид
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetLawUnit(int id)
        {
            var lawUnit = commonService.GetLawUnitById(id);

            if (lawUnit == null)
            {
                return BadRequest();
            }

            return Json(lawUnit);
        }

        /// <summary>
        /// Специалности за заседател
        /// </summary>
        /// <param name="lawUnitId"></param>
        /// <returns></returns>
        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        [HttpGet]
        public IActionResult LawUnitSpeciality(int lawUnitId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForSpeciality(lawUnitId).DeleteOrDisableLast();

            var model = commonService.LawUnitSpeciality_SelectForCheck(lawUnitId);
            SetViewBag(lawUnitId, model);
            return View("CheckListViewVM", model);
        }

        /// <summary>
        /// Запис на чекнатите Специалности за заседател
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult LawUnitSpeciality(CheckListViewVM model)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForSpeciality(model.CourtId).DeleteOrDisableLast();

            if (commonService.LawUnitSpeciality_SaveData(model))
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            var newModel = commonService.LawUnitSpeciality_SelectForCheck(model.CourtId);
            SetViewBag(model.CourtId, newModel);
            return View("CheckListViewVM", newModel);
        }

        void SetViewBag(int lawUnitId, CheckListViewVM model)
        {
            var lawUnit = commonService.GetById<LawUnit>(lawUnitId);

            model.Label = "Специалности за " + lawUnit.FullName;
            ViewBag.backUrl = Url.Action("Index", "LawUnit", new { lawUnitType = lawUnit.LawUnitTypeId });
        }

        /// <summary>
        /// Адреси за лице
        /// </summary>
        /// <param name="lawUnitId"></param>
        /// <returns></returns>
        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        public IActionResult LawUnitAddressList(int lawUnitId)
        {
            var lawUnit = commonService.GetById<LawUnit>(lawUnitId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForLawUnitAddress(lawUnitId).DeleteOrDisableLast();

            ViewBag.lawUnitId = lawUnit.Id;
            ViewBag.lawUnitTypeId = lawUnit.LawUnitTypeId;
            ViewBag.lawUnitName = lawUnit.FullName;

            return View();
        }

        /// <summary>
        /// Извличане на адреси за лице
        /// </summary>
        /// <param name="request"></param>
        /// <param name="lawUnitId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataLawUnitAddress(IDataTablesRequest request, int lawUnitId)
        {
            var data = commonService.LawUnitAddress_Select(lawUnitId);

            return request.GetResponse(data);
        }

        public void SetBreadcrumslawUnitAddress(int lawUnitId, long id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForLawUnitAddressEdit(lawUnitId, id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForLawUnitAddressAdd(lawUnitId).DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне на адрес на лице
        /// </summary>
        /// <param name="lawUnitId"></param>
        /// <returns></returns>
        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        public IActionResult AddLawUnitAdr(int lawUnitId)
        {
            SetBreadcrumslawUnitAddress(lawUnitId, 0);
            SetViewBagLawUnitAddress(lawUnitId);

            var model = new LawUnitAddress()
            {
                LawUnitId = lawUnitId,
                Address = new Address()
            };
            return View(nameof(EditLawUnitAdr), model);
        }

        /// <summary>
        /// Редакция на адрес на лице
        /// </summary>
        /// <param name="lawUnitId"></param>
        /// <param name="addressId"></param>
        /// <returns></returns>
        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        public IActionResult EditLawUnitAdr(int lawUnitId, long addressId)
        {
            SetBreadcrumslawUnitAddress(lawUnitId, addressId);

            var model = commonService.LawUnitAddress_GetById(lawUnitId, addressId);
            SetViewBagLawUnitAddress(model.LawUnitId);

            return View(nameof(EditLawUnitAdr), model);
        }

        /// <summary>
        /// Валидация на адрес на лице преди запис
        /// </summary>
        /// <param name="model"></param>
        void ValidateModelAdr(LawUnitAddress model)
        {
            if (string.IsNullOrEmpty(model.Address.CityCode))
            {
                ModelState.AddModelError("", "Въведете адрес");
            }
            if (model.Address.AddressTypeId <= 0)
            {
                ModelState.AddModelError("", "Изберете вид адрес");
            }
        }

        /// <summary>
        /// Запис на адрес на лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditLawUnitAdr(LawUnitAddress model)
        {
            SetViewBagLawUnitAddress(model.LawUnitId);
            ValidateModelAdr(model);
            if (!ModelState.IsValid)
            {
                SetBreadcrumslawUnitAddress(model.LawUnitId, model.AddressId);
                return View(nameof(EditLawUnitAdr), model);
            }
            var currentId = model.AddressId;
            (bool result, string errorMessage) = commonService.LawUnitAddress_SaveData(model);
            if (result == true)
            {
                this.SaveLogOperation(currentId == 0, model.AddressId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditLawUnitAdr), new { lawUnitId = model.LawUnitId, addressId = model.AddressId });
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage) == true)
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            SetBreadcrumslawUnitAddress(model.LawUnitId, model.AddressId);
            return View(nameof(EditLawUnitAdr), model);
        }

        public void SetViewBagLawUnitAddress(int lawUnitId)
        {
            var lawUnit = commonService.GetById<LawUnit>(lawUnitId);
            ViewBag.lawUnitName = lawUnit.FullName;

            ViewBag.CountriesDDL = nomService.GetCountries();
            ViewBag.AddressTypesDDL = nomService.GetDropDownList<AddressType>();

            SetHelpByLawUnitType(lawUnit.LawUnitTypeId);
        }
    }
}