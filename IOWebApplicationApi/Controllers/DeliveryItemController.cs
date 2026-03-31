using DnsClient.Internal;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplicationApi.Contracts;
using IOWebApplicationApi.Data.Models;
using IOWebApplicationApi.Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IOWebApplicationApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class DeliveryItemController : Controller
    {
        private readonly IDeliveryItemService deliveryItemService;
        private readonly IWorkingDaysService workingDaysService;
        private readonly IMobileFileService mobileFileService;
        private readonly ILogger<DeliveryItemController> logger;
        public DeliveryItemController(
            IDeliveryItemService _deliveryItemService,
            IWorkingDaysService _workingDaysService,
            IMobileFileService _mobileFileService,
            ILogger<DeliveryItemController> logger)
        {
            deliveryItemService = _deliveryItemService;
            workingDaysService = _workingDaysService;
            mobileFileService = _mobileFileService;
            this.logger = logger;
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
        public JsonResult LoadData([FromBody] SyncParam model)
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
        public async Task<JsonResult> SaveVisit([FromBody] DeliveryItemVisitMobileModel model)
        {
            if (model == null)
            {
                logger.LogError("SaveVisit.DeliveryItemVisitMobileModel.Model is null");
            }


            DeliveryItemVisitMobile modelData = new DeliveryItemVisitMobile()
            {
                Id = model.Id,
                DateOper = model.DateOper,
                DeliveryItemId = model.DeliveryItemId,
                DeliveryOperId = model.DeliveryOperId,
                NotificationStateId = model.NotificationStateId,
                Lat = model.Lat.ToString(),
                Long = model.Long.ToString(),
                DeliveryReasonId = model.DeliveryReasonId,
                UserId = model.UserId,
                CourtId = model.CourtId,
                DeliveryUUID = model.DeliveryUUID

            };

            modelData.DateOper = modelData.DateOper.ConvertUtcToBGTime();
            modelData.LawUnitId = GetLawUnitId();
            if (modelData.CourtId <= 0)
                modelData.CourtId = GetCourtId();
            var result = await deliveryItemService.DeliveryItemSaveOperMobile(modelData);
            return Json(result);
        }

        [HttpPost("SaveMobileFile")]
        [DisableRequestSizeLimit]
        public async Task<JsonResult> SaveMobileFile()
        {
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
                var content = await reader.ReadToEndAsync();
                result = await mobileFileService.SaveMobileFile(deliveryAccountId, courtId, content);
            }
            return Json(result);
        }

        [AllowAnonymous]
        [HttpGet(nameof(test))]
        public IActionResult test()
        {
            return Ok("Test OK.");
        }
    }
}
