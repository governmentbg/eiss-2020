using DataTables.AspNet.Core;
using IOWebApplication.Components;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]

    public class InstitutionController : BaseController
    {
        private readonly ICommonService commonService;
        private readonly INomenclatureService nomService;

        public InstitutionController(ICommonService _commonService, INomenclatureService _nomService)
        {
            commonService = _commonService;
            nomService = _nomService;
        }

        /// <summary>
        /// Страница с институции
        /// </summary>
        /// <param name="institutionType"></param>
        /// <returns></returns>
        public IActionResult Index(int institutionType)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_Institution(institutionType).DeleteOrDisableLast();

            ViewBag.institutionType = commonService.GetById<InstitutionType>(institutionType);

            LawUnitFilterVM model = new LawUnitFilterVM();
            return View(model);
        }

        /// <summary>
        /// Извличане на данните за институции
        /// </summary>
        /// <param name="request"></param>
        /// <param name="institutionType"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int institutionType, string fullName)
        {

            var data = commonService.Institution_Select(institutionType, fullName);

            return request.GetResponse(data);
        }

        public void SetBreadcrums(int id, int institutionTypeId)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_InstitutionEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_InstitutionAdd(institutionTypeId).DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне на институция
        /// </summary>
        /// <param name="institutionType"></param>
        /// <returns></returns>
        public IActionResult Add(int institutionType)
        {
            SetBreadcrums(0, institutionType);
            var model = new Institution()
            {
                InstitutionTypeId = institutionType
            };
            ViewBag.institutionType = commonService.GetById<InstitutionType>(institutionType);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на институция
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            SetBreadcrums(id, 0);

            var model = commonService.GetById<Institution>(id);
            ViewBag.institutionType = commonService.GetById<InstitutionType>(model.InstitutionTypeId);
            return View(model);
        }

        /// <summary>
        /// Запис на институция
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(Institution model)
        {
            ViewBag.institutionType = commonService.GetById<InstitutionType>(model.InstitutionTypeId);
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                SetBreadcrums(model.Id, model.InstitutionTypeId);
                return View(model);
            }
            var currentId = model.Id;
            if (commonService.Institution_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetBreadcrums(model.Id, model.InstitutionTypeId);
            return View(model);
        }

        /// <summary>
        /// Валидация на институция преди запис
        /// </summary>
        /// <param name="model"></param>
        void ValidateModel(Institution model)
        {
            if (string.IsNullOrEmpty(model.FirstName) && string.IsNullOrEmpty(model.FullName))
            {
                ModelState.AddModelError(nameof(model.FirstName), "Въведете поне едно име");
            }
            switch (model.InstitutionTypeId)
            {
                case NomenclatureConstants.InstitutionTypes.Attourney:
                    if (string.IsNullOrEmpty(model.EISPPCode))
                    {
                        ModelState.AddModelError(nameof(model.EISPPCode), "Въведете 'ЕИСПП код'.");
                    }
                    break;
            }
            var valMessage = commonService.Institution_Validate(model);
            if (!string.IsNullOrEmpty(valMessage))
                    {
                        ModelState.AddModelError(nameof(model.Code), valMessage);
                    }
            
        }

        /// <summary>
        /// Страница с адресите за институция
        /// </summary>
        /// <param name="institutionId"></param>
        /// <returns></returns>
        public IActionResult InstitutionAddressList(int institutionId)
        {
            var institution = commonService.GetById<Institution>(institutionId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForInstitutionAddress(institutionId).DeleteOrDisableLast();

            ViewBag.institutionId = institution.Id;
            ViewBag.institutionTypeId = institution.InstitutionTypeId;
            ViewBag.institutionName = institution.FullName;

            return View();
        }

        /// <summary>
        /// Извличане на данните за адресите на институция
        /// </summary>
        /// <param name="request"></param>
        /// <param name="institutionId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataInstitutionAddress(IDataTablesRequest request, int institutionId)
        {
            var data = commonService.InstitutionAddress_Select(institutionId);

            return request.GetResponse(data);
        }

        public void SetBreadcrumsInstitutionAddress(int institutionId, long id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForInstitutionAddressEdit(institutionId, id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForInstitutionAddressAdd(institutionId).DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне на адрес на институция
        /// </summary>
        /// <param name="institutionId"></param>
        /// <returns></returns>
        public IActionResult AddInstitutionAdr(int institutionId)
        {
            SetBreadcrumsInstitutionAddress(institutionId, 0);
            SetViewBagInstitutionAddress(institutionId);

            var model = new InstitutionAddress()
            {
                InstitutionId = institutionId,
                Address = new Address()
            };
            return View(nameof(EditInstitutionAdr), model);
        }

        /// <summary>
        /// Редакция на адрес на институция
        /// </summary>
        /// <param name="institutionId"></param>
        /// <param name="addressId"></param>
        /// <returns></returns>
        public IActionResult EditInstitutionAdr(int institutionId, long addressId)
        {
            SetBreadcrumsInstitutionAddress(institutionId, addressId);

            var model = commonService.InstitutionAddress_GetById(institutionId, addressId);
            SetViewBagInstitutionAddress(model.InstitutionId);

            return View(nameof(EditInstitutionAdr), model);
        }

        /// <summary>
        /// Валидация на адрес на институция преди запис
        /// </summary>
        /// <param name="model"></param>
        void ValidateModelAdr(InstitutionAddress model)
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
        /// Запис на адрес на институция
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditInstitutionAdr(InstitutionAddress model)
        {
            SetViewBagInstitutionAddress(model.InstitutionId);
            ValidateModelAdr(model);
            if (!ModelState.IsValid)
            {
                SetBreadcrumsInstitutionAddress(model.InstitutionId, model.AddressId);
                return View(nameof(EditInstitutionAdr), model);
            }
            var currentId = model.AddressId;
            (bool result, string errorMessage) = commonService.InstitutionAddress_SaveData(model);
            if (result == true)
            {
                this.SaveLogOperation(currentId == 0, model.AddressId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditInstitutionAdr), new { institutionId = model.InstitutionId, addressId = model.AddressId });
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage) == true)
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            SetBreadcrumsInstitutionAddress(model.InstitutionId, model.AddressId);
            return View(nameof(EditInstitutionAdr), model);
        }

        public void SetViewBagInstitutionAddress(int institutionId)
        {
            var institution = commonService.GetById<Institution>(institutionId);
            ViewBag.institutionName = institution.FullName;

            ViewBag.CountriesDDL = nomService.GetCountries();
            ViewBag.AddressTypesDDL = nomService.GetDropDownList<AddressType>();
        }
    }
}