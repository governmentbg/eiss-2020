using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using IOWebApplicationApi.Contracts;
using IOWebApplicationApi.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace IOWebApplicationApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class DeliveryItemController : Controller
    {
        private readonly IDeliveryItemService deliveryItemService;
        private readonly IWorkingDaysService workingDaysService;
        private readonly IMobileFileService mobileFileService;
        public DeliveryItemController(
            IDeliveryItemService _deliveryItemService,
            IWorkingDaysService _workingDaysService,
            IMobileFileService _mobileFileService)
        {
            deliveryItemService = _deliveryItemService;
            workingDaysService = _workingDaysService;
            mobileFileService = _mobileFileService;
        }

        private int GetCourtId()
        {
            var User = HttpContext.User;
            int courtId = 0;
            if (User != null && User.Claims != null && User.Claims.Count() > 0)
            {
                var subClaim = User.Claims
                    .FirstOrDefault(c => c.Type == CustomClaimType.CourtId);

                if (subClaim != null)
                {
                    courtId = int.Parse(subClaim.Value);
                }
            }
            return courtId;
        }
        private int GetLawUnitId()
        {
            int lawUnitId = 0;
            if (User != null && User.Claims != null && User.Claims.Count() > 0)
            {
                var subClaim = User.Claims
                    .FirstOrDefault(c => c.Type == CustomClaimType.LawUnitId);

                if (subClaim != null)
                {
                    lawUnitId = int.Parse(subClaim.Value);
                }
            }
            return lawUnitId;
        }
        [HttpPost("LoadData")]
        public JsonResult LoadData([FromBody]SyncParam model)
        {
            int courtId = GetCourtId();
            int lawUnitId = GetLawUnitId();

            var deliveryItems = deliveryItemService.GetDeliveryItemMobileVM(courtId, lawUnitId, model.dateFrom, model.dateTo);

            var courts = deliveryItemService.GetCourtsMobile();
            courts = courts.Where(c => deliveryItems.Any(x => x.CourtId.ToString() == c.value)).ToList();
            var notificationStates = deliveryItemService.GetNotificationStateMobile();
            var reasons = deliveryItemService.GetDeliveryReasonMobile();
            var workingDays = workingDaysService.GetWorkingDaysMobile(courtId);
            var notificationTypes = deliveryItemService.GetNotificationTypeMobile();
            return Json(new { items = deliveryItems, courts, notificationStates, reasons, workingDays, notificationTypes });
        }
        [HttpPost("SaveVisit")]
        public JsonResult SaveVisit([FromBody]DeliveryItemVisitMobile model)
        {
            model.DateOper = model.DateOper.AddHours(3);
            model.LawUnitId = GetLawUnitId();
            if (model.CourtId <= 0)
                model.CourtId = GetCourtId();
            var result = deliveryItemService.DeliveryItemSaveOperMobile(model);
            return Json(result);
        }
        [HttpPost("SaveMobileFile")]
        [DisableRequestSizeLimit]
        public JsonResult SaveMobileFile() {
            var User = HttpContext.User;
            int courtId = 0;
            string deliveryAccountId = "";
            if (User != null && User.Claims != null && User.Claims.Count() > 0)
            {
                var subClaim = User.Claims
                    .FirstOrDefault(c => c.Type == CustomClaimType.CourtId);
                if (subClaim != null)
                {
                    courtId = int.Parse(subClaim.Value);
                }

                var subClaimId = User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (subClaimId != null)
                {
                    deliveryAccountId = subClaimId.Value.ToString();
                }
            }
            bool result = false;
            using (var reader = new StreamReader(Request.Body))
            {
                var content = reader.ReadToEnd();
                result = mobileFileService.SaveMobileFile(deliveryAccountId, courtId, content);
            }
            return Json(result);
        }

        //[AllowAnonymous]
        //[HttpPost("test_mobile")]
        //[HttpGet("test_mobile")]
        //public JsonResult TestMobile()
        //{
        //    var model = deliveryItemService.GetById<DeliveryItemVisitMobile>(65);
        //    var result = deliveryItemService.DeliveryItemSaveOperMobile(model);
        //    return Json(result);
        //}

    }
}
