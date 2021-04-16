using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOWebApplication.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Core.Helper.GlobalConstants;
using DataTables.AspNet.Core;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Newtonsoft.Json;
using IOWebApplication.Infrastructure.Constants;
using Microsoft.Extensions.Logging;

namespace IOWebApplication.Controllers
{
    public class DeliveryAreaAddressController : BaseController
    {
        private readonly IDeliveryAreaAddressService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ILogger logger;

        public DeliveryAreaAddressController(
            IDeliveryAreaAddressService deliveryAreaAddressService, 
            INomenclatureService _nomService,  
            ICommonService _commonService,
            ILogger<DeliveryAreaAddressController> _logger)
        {
            service = deliveryAreaAddressService;
            nomService = _nomService;
            commonService = _commonService;
            logger = _logger;
        }

        public IActionResult Index(int? deliveryAreaId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryAreaAddresses(deliveryAreaId ?? 0).DeleteOrDisableLast();
            var deliveryArea = service.GetById<DeliveryArea>(deliveryAreaId);
            ViewBag.deliveryAreaLabel = "";
            if (deliveryArea != null) {
                ViewBag.deliveryAreaLabel = " към район " +deliveryArea.Description;
                var lawUnit = service.GetById<LawUnit>(deliveryArea.LawUnitId);
                if (lawUnit != null)
                    ViewBag.deliveryAreaLabel += "  с призовкар "+ lawUnit.FullName;
            }
            ViewBag.ExpiredType_ddl = service.ExpiredTypeDDL();
            var filter = new DeliveryAreaAddressFilterVM()
            {
                DeliveryAreaId = deliveryAreaId ?? 0,
                ExpiredType = 1
            };
            SetHelpFile(HelpFileValues.Nom3);

            return View(filter);
        }

        public IActionResult Add(int deliveryAreaId)
        {
            SetViewbag(deliveryAreaId, deliveryAreaId, 0);
            var court = commonService.GetCourt(userContext.CourtId);
            string cityCode = court?.CityCode ?? "";

            var model = new DeliveryAreaAddress()
            {
                DeliveryAreaId = deliveryAreaId,
                CityCode = cityCode,
                IsActive = true,
                DateFrom = DateTime.Now.Date
            };
            return View(nameof(Edit), model);
        }

        public IActionResult Edit(int id, int? editDeliveryAreaId)
        {
            var model = service.GetById<DeliveryAreaAddress>(id);
            SetViewbag(model.DeliveryAreaId, editDeliveryAreaId ?? 0, id);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult Edit(DeliveryAreaAddress model, int? editDeliveryAreaId)
        {
            SetViewbag(model.DeliveryAreaId, editDeliveryAreaId ?? 0, model.Id);
            if (model.NumberType == NomenclatureConstants.DeliveryAddressNumberType.BlockName && string.IsNullOrEmpty(model.BlockName))
               ModelState.AddModelError(nameof(DeliveryAreaAddress.BlockName), "Въведете име на блок");
            if (model.NumberType == NomenclatureConstants.DeliveryAddressNumberType.NumberName && string.IsNullOrEmpty(model.BlockName))
                ModelState.AddModelError(nameof(DeliveryAreaAddress.BlockName), "Въведете подномер");
            if ((model.NumberType == NomenclatureConstants.DeliveryAddressNumberType.EvenNumber ||
                 model.NumberType == NomenclatureConstants.DeliveryAddressNumberType.OddNumber ||
                 model.NumberType == NomenclatureConstants.DeliveryAddressNumberType.OddEvenNumber) && 
                string.IsNullOrEmpty(model.StreetCode) &&
                (model.NumberFrom > 0 || model.NumberTo > 0)
              )
                ModelState.AddModelError(nameof(DeliveryAreaAddress.NumberType), "За да се районира от номер до номер улица трябва да е въведена улица");
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.DeliveryAreaAddressSaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id, editDeliveryAreaId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, DeliveryAreaAddressFilterVM filter)
        {
            var data = service.DeliveryAreaAddressSelect(filter);
            return request.GetResponse(data);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult VerifyNumberFrom(int? NumberFrom, int? NumberType, int? NumberTo)
        {
            var err = service.VerifyNumberFrom(NumberFrom, NumberType, NumberTo);
            if (err != "")
                return Json(err);
            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult VerifyNumberTo(int? NumberFrom, int? NumberType, int? NumberTo)
        {
            var err = service.VerifyNumberTo(NumberFrom, NumberType, NumberTo);
            if (err != "")
                return Json(err);
            return Json(true);
        }

     
        void SetViewbag(int deliveryAreaId,int editDeliveryAreaId, int deliveryAreaAddressId)
        {
            ViewBag.editDeliveryAreaId = editDeliveryAreaId;
            ViewBag.CityCode_ddl = service.GetEkatteByArea(deliveryAreaId);
            ViewBag.NumberType_ddl = nomService.GetDDL_DeliveryNumberType();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryAreaAddressEdit(editDeliveryAreaId, deliveryAreaAddressId).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Nom3);
        }
        [HttpPost]
        public IActionResult DeliveryAreaAddress_ExpiredInfo(ExpiredInfoVM model)
        {
            if (service.SaveExpireInfo<DeliveryAreaAddress>(model))
            {
                SetSuccessMessage(MessageConstant.Values.DeliveryAreaAddressExpireOK);
                return Json(new { result = true, redirectUrl = model.ReturnUrl }); 
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }
        #region Duplication
        public IActionResult IndexDuplication()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryAreaAddressesDuplication().DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Nom3);
            return View();
        }
        [HttpPost]
        public IActionResult ListDataDuplication(IDataTablesRequest request)
        {
            var data = service.DeliveryAreaAddressDuplication(userContext.CourtId);
            return request.GetResponse(data);
        }
        #endregion Duplication
        #region TestFindAdr
        public void SetViewBagPersonAddress()
        {
            ViewBag.CountriesDDL = nomService.GetCountries();
            ViewBag.AddressTypesDDL = nomService.GetDropDownList<AddressType>();
        }

        public IActionResult TestFindAdr()
        {
            DeliveryAreaAddressTestVM model = new DeliveryAreaAddressTestVM();
            model.Address = new Address();
            model.Address.CityCode = "12259";
            SetViewBagPersonAddress();
            return View(nameof(TestFindAdr), model);
        }
        [HttpPost]
        public IActionResult TestFindAdr(DeliveryAreaAddressTestVM model)
        {
            model = service.DeliveryAreaAddressFindTest(model);
            SetViewBagPersonAddress();
            ModelState.Clear();
            return View(nameof(TestFindAdr), model);
        }
        #endregion TestFindAdr

        /// <summary>
        /// Всички улици/квартали, които не са влезнали в район - да може да се изберат към район
        /// </summary>
        /// <param name="cityId"></param>
        /// <returns></returns>
        public JsonResult EkStreetLeftList(string cityId)
        {
            var data = service.EkStreetForSelect_Select(cityId, userContext.CourtId);
            return Json(data);
        }

        /// <summary>
        /// Добавяне на цели улици към район
        /// </summary>
        /// <param name="deliveryAreaId"></param>
        /// <returns></returns>
        public IActionResult AddStreets(int deliveryAreaId)
        {
            ViewBag.filterCity_ddl = service.GetEkatteByArea(deliveryAreaId);
            ViewBag.deliveryAreaId = deliveryAreaId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryAreaAddresses(deliveryAreaId);
            SetHelpFile(HelpFileValues.Nom3);

            return View();
        }

        /// <summary>
        /// Запис на улици към район
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="caseStreetsJson"></param>
        /// <param name="cityId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddStreets(int areaId, string caseStreetsJson, string cityId)
        {
            List<int> codes = new List<int>();
            string errStreet = "";
            try
            {
                codes = JsonConvert.DeserializeObject<List<int>>(caseStreetsJson);
            }
            catch (Exception ex)
            {
                errStreet = "Проблем със списъка с улици";
                SetErrorMessage(errStreet);
                logger.LogError(ex, errStreet);
            }

            if (string.IsNullOrEmpty(errStreet))
            {
                if (service.DeliveryAreaAddressSaveListData(areaId, codes, cityId) == true)
                {
                    SetSuccessMessage(MessageConstant.Values.SaveOK);
                }
                else
                {
                    SetErrorMessage(MessageConstant.Values.SaveFailed);
                }
            }

            return RedirectToAction(nameof(Index), new { deliveryAreaId = areaId });
        }
    }
}