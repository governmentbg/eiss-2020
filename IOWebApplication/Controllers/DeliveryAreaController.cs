using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class DeliveryAreaController : BaseController
    {
        private readonly IDeliveryAreaService service;
        private readonly ICommonService commonService;
        private readonly ICourtLawUnitService courtLawUnitService;
        private readonly IDeliveryAreaAddressService areaAddressService;
        public DeliveryAreaController(
            IDeliveryAreaService deliveryAreaService, 
            ICommonService _commonService, 
            ICourtLawUnitService _courtLawUnitService, 
            IDeliveryAreaAddressService _areaAddressService
            )
        {
            service = deliveryAreaService;
            commonService = _commonService;
            courtLawUnitService = _courtLawUnitService;
            areaAddressService = _areaAddressService;
        }

        public IActionResult Index(int? courtId)
        {
            var filter = new DeliveryAreaFilterVM()
            {
                CourtId = courtId ?? userContext.CourtId,
                ExpiredType = 1
            };
            ViewBag.CityCode_ddl = areaAddressService.GetEkatteByCourt(filter.CourtId);
            ViewBag.ExpiredType_ddl = areaAddressService.ExpiredTypeDDL();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryAreas().DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Nom3);

            return View(filter);
        }

        public IActionResult Add(int courtId)
        {
            SetViewbag(userContext.CourtId, 0);

            var model = new DeliveryArea()
            {
                CourtId = courtId,
                IsActive = true, 
                DateFrom = DateTime.Now.Date
            };
            return View(nameof(Edit), model);
        }

        public IActionResult Edit(int id)
        {
            SetViewbag(userContext.CourtId, id);
            var model = service.GetById(id);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult Edit(DeliveryArea model)
        {
            SetViewbag(userContext.CourtId, model.Id);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.DeliveryAreaSaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, DeliveryAreaFilterVM filter)
        {
            var data = service.DeliveryAreaSelect(filter);
            return request.GetResponse(data);
        }

        [HttpPost]
        public JsonResult getLawUnitId(int deliveryAreaId)
        {
            var model = service.GetById<DeliveryArea>(deliveryAreaId);
            int lawUnitId = model?.LawUnitId ?? -1;
            return Json(new { lawUnitId });
        }
        void SetViewbag(int forCourtId, int deliveryAreaId)
        {
            ViewBag.LawUnitId_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, forCourtId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryArea(deliveryAreaId).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Nom3);
        }
        [HttpPost]
        public IActionResult DeliveryArea_ExpiredInfo(ExpiredInfoVM model)
        {
            if (service.SaveExpireInfo<DeliveryArea>(model))
            {
                SetSuccessMessage(MessageConstant.Values.DeliveryAreaExpireOK);
                return Json(new { result = true, redirectUrl = model.ReturnUrl }); // Url.Action("CaseNotification", new { id = model.Id }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }
        //public JsonResult insertCaseRegion()
        //{
        //    service.insertCaseRegion();
        //    return Json("OK");
        //}
        //public JsonResult updateCaseRegionParent()
        //{
        //    service.updateCaseRegionParent();
        //    return Json("OK");
        //}
        //public JsonResult saveHtmlTemplateLink()
        //{
        //    service.saveHtmlTemplateLink();
        //    return Json("OK");
        //}
    }
}