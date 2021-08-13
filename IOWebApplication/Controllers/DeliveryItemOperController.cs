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
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IOWebApplication.Controllers
{
    public class DeliveryItemOperController : BaseController
    {
        private readonly IDeliveryItemOperService service;
        private readonly IDeliveryItemService itemService;
        private readonly ICommonService commonService;
        private readonly INomenclatureService nomService;
        private readonly IConfiguration configuration;
        private readonly IWorkingDaysService workingDaysService;
        public DeliveryItemOperController(
            IDeliveryItemOperService _service, 
            IDeliveryItemService _itemService, 
            ICommonService _commonService, 
            INomenclatureService _nomService,
            IConfiguration _configuration,
            IWorkingDaysService _workingDaysService)
        {
            service = _service;
            itemService = _itemService;
            commonService = _commonService;
            nomService = _nomService;
            configuration = _configuration;
            workingDaysService = _workingDaysService;
        }
        public IActionResult LoadIndex(int DeliveryItemId, string filterJson)
        {
            ViewBag.filterJson = filterJson;
            int filterType = getFilterTypeFromJson(filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemOpers(filterType, DeliveryItemId).DeleteOrDisableLast();
            ViewBag.canAdd = service.CanAdd(DeliveryItemId);
            ViewBag.NotificationInfo = itemService.GetNotificationInfoByDeliveryItemId(DeliveryItemId);
            SetHelpFile(HelpFileValues.Summons);
            return View(nameof(Index),DeliveryItemId);
        }
        [HttpPost]
        public IActionResult Index(int DeliveryItemId, [AllowHtml] string filterJson)
        {
            return LoadIndex(DeliveryItemId, filterJson);
        }
        [HttpGet]
        public IActionResult Index(int notificationId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationDeliveryOper(notificationId).DeleteOrDisableLast();
            var deliveryItem = itemService.GetDeliveryItemByCaseNotificationId(notificationId);
            int deliveryItemId = deliveryItem?.Id ?? 0;
            ViewBag.filterJson = "";
            ViewBag.canAdd = service.CanAdd(deliveryItemId);
            ViewBag.NotificationInfo = itemService.GetNotificationInfo(notificationId);
            return View(deliveryItemId);
        }
        [HttpGet]
        public IActionResult IndexDocument(int notificationId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDocumentNotificationDeliveryOper(notificationId).DeleteOrDisableLast();
            var deliveryItem = itemService.GetDeliveryItemByDocumentNotificationId(notificationId);
            int deliveryItemId = deliveryItem?.Id ?? 0;
            ViewBag.filterJson = "";
            ViewBag.canAdd = service.CanAdd(deliveryItemId);
            ViewBag.NotificationInfo = itemService.GetNotificationInfo(notificationId);
            return View(nameof(Index), deliveryItemId);
        }
        public IActionResult IndexHistory(int deliveryItemId, [AllowHtml] string filterJson)
        {
            ViewBag.filterJson = filterJson;
            int filterType = getFilterTypeFromJson(filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemHistoryOpers(filterType, deliveryItemId).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Summons);

            return View(deliveryItemId);
        }
        public IActionResult Edit(int id, [AllowHtml] string filterJson)
        {
            var model = service.getDeliveryItemOper(id);
            var deliveryItem = itemService.getDeliveryItem(model.DeliveryItemId);
            if (filterJson == null)
            {
                if (deliveryItem.DocumentNotificationId > 0)
                {
                    ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDocumentNotificationDeliveryOperEdit(deliveryItem.DocumentNotificationId ?? 0, 0).DeleteOrDisableLast();
                }
                else
                {
                    ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationDeliveryOperEdit(deliveryItem.CaseNotificationId ?? 0, 0).DeleteOrDisableLast();
                }
            }
            else
            {
                ViewBag.filterJson = filterJson;
                int filterType = getFilterTypeFromJson(filterJson); 
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemOperEdit(filterType, id).DeleteOrDisableLast();
            }
            SetViewbag(model.DeliveryOperId);
            ModelState.Clear();
            return View(nameof(Edit), model);
        }
        public IActionResult Add(int deliveryItemId, [AllowHtml] string filterJson)
        {
            ViewBag.filterJson = filterJson;
            var deliveryItem = itemService.getDeliveryItem(deliveryItemId);
            if (filterJson == null)
            {
                if (deliveryItem.DocumentNotificationId > 0)
                {
                    ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDocumentNotificationDeliveryOperEdit(deliveryItem.DocumentNotificationId ?? 0, 0).DeleteOrDisableLast();
                }
                else
                {
                    ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationDeliveryOperEdit(deliveryItem.CaseNotificationId ?? 0, 0).DeleteOrDisableLast();
                }
            }
            else
            {
                int filterType = getFilterTypeFromJson(filterJson);
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemAddOper(filterType, deliveryItemId).DeleteOrDisableLast();
            }
            var model = service.makeDeliveryItemOper(deliveryItemId);
            SetViewbag(model.DeliveryOperId);
            ModelState.Clear();
            return View(nameof(Edit), model);
        }
        [HttpPost]
        public IActionResult EditPost(DeliveryItemOperVM model, [AllowHtml] string filterJson)
        {
            var deliveryItem = itemService.getDeliveryItem(model.DeliveryItemId);
            ViewBag.notificationId = deliveryItem.CaseNotificationId;
            if (filterJson == null)
            {
                if (deliveryItem.DocumentNotificationId > 0)
                {
                    ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDocumentNotificationDeliveryOperEdit(deliveryItem.DocumentNotificationId ?? 0, 0).DeleteOrDisableLast();
                }
                else
                {
                    ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationDeliveryOperEdit(deliveryItem.CaseNotificationId ?? 0, 0).DeleteOrDisableLast();
                }
            }
            else
            {
                int filterType = getFilterTypeFromJson(filterJson);
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemAddOper(filterType, model.DeliveryItemId).DeleteOrDisableLast();
            }
            ViewBag.filterJson = filterJson;
            SetViewbag(model.DeliveryOperId);
            ValidateModel(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            bool isInsert = !service.HaveSameOper(model.DeliveryItemId, model.DeliveryOperId); 
             if (itemService.DeliveryItemSaveOper(model))
            {
                SaveLogOperation(isInsert, $"{model.DeliveryItemId}|NEW");
                ModelState.Clear();
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                if (filterJson != null)
                {
                    // return LoadIndex(model.DeliveryItemId, filterJson);
                    // TempData["filterJson"] = filterJson;
                    // return RedirectToAction("EditReturn", "DeliveryItem", new { deliveryItemId = model.DeliveryItemId });
                    return View(nameof(Edit), model);
                }
                else
                {
                    if (deliveryItem.DocumentNotificationId > 0)
                    {
                        return RedirectToAction(nameof(IndexDocument), new { notificationId = deliveryItem.DocumentNotificationId });
                    }
                    {
                        return RedirectToAction(nameof(Index), new { notificationId = deliveryItem.CaseNotificationId });
                    }
                }
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }
        public JsonResult NotificationState(int operId)
        {
            return Json(service.NotificationStateForDeliveryOperSelect(operId));
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int deliveryItemId, bool onlyLast)
        {
            var data = service.DeliveryItemOperSelect(deliveryItemId, onlyLast);
            return request.GetResponse(data);
        }
        public IActionResult Location(int id, [AllowHtml] string filterJson)
        {
            var deliveryItemOper = service.getDeliveryItemOper(id);
            ViewBag.hereMaps_ApiKey = configuration["HereMaps_ApiKey"];
                //"0JBbcgy_ITFIew-SnnpOTej3HtnfMx4Jooma8Z0qMYo";
                //"CvStcFcbCy3KfJFf3DmNDBMi70NT-ggyfBKMyU7YNXo";

            if (filterJson == null)
            {
                var deliveryItem = itemService.getDeliveryItem(deliveryItemOper.DeliveryItemId);
                if (deliveryItem.DocumentNotificationId > 0)
                {
                    ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDocumentNotificationDeliveryOperEdit(deliveryItem.DocumentNotificationId ?? 0, 0).DeleteOrDisableLast();
                }
                else
                {
                    ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationDeliveryOperEdit(deliveryItem.CaseNotificationId ?? 0, 0).DeleteOrDisableLast();
                }
            }
            else
            {
                ViewBag.filterJson = filterJson;
                int filterType = getFilterTypeFromJson(filterJson);
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemOperEdit(filterType, id).DeleteOrDisableLast();
            }
            SetHelpFile(HelpFileValues.Summons);

            return View(deliveryItemOper);
        }
        void SetViewbag(int operId)
        {
            ViewBag.DeliveryOperId_ddl = service.DeliveryOperSelect().Where(x => x.Value == operId.ToString()).ToList();
            ViewBag.NotificationStateId_ddl = service.NotificationStateForDeliveryOperSelect(operId);
            ViewBag.DeliveryReasonId_ddl = nomService.GetDropDownList<DeliveryReason>();
            SetHelpFile(HelpFileValues.Summons);
        }
        private int getFilterTypeFromJson(string filterJson)
        {
            try
            {
                var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
                DeliveryItemFilterVM model = JsonConvert.DeserializeObject<DeliveryItemFilterVM>(filterJson, dateTimeConverter);
                return model.FilterType;
            }
            catch
            {
                return 0;
            }
        }
       
        public JsonResult GetDeliveryReasonDDL(int notificationStateId)
        {
            var reasons = service.GetDeliveryReasonDDL(notificationStateId);
            return Json(reasons);
        }
        private void ValidateModel(DeliveryItemOperVM model)
        {
            var regDate = service.GetRegDate(model.DeliveryItemId);
            if (model.DateOper < regDate?.Date)
            {
                ModelState.AddModelError(nameof(model.DateOper), $"{MessageConstant.ValidationErrors.DeliveryDateBeforeRegDate} {regDate?.ToString(FormattingConstant.NormalDateFormat)}");
            }
            if (model.DateOper > DateTime.Now.AddMinutes(10))
            {
                ModelState.AddModelError(nameof(model.DateOper), MessageConstant.ValidationErrors.DeliveryDateBeforeRegDate);
            }

            var opers = service.DeliveryItemOperSelect(model.DeliveryItemId, true)
                               .Where(x => x.DeliveryOperId < model.DeliveryOperId)
                               .OrderBy(x => x.DeliveryOperId)
                               .ToList();
            var prevOper = opers.LastOrDefault();
            if (prevOper != null && model.DateOper != null)
            {
                if (model.DateOper?.Date < prevOper.DateOper.Date)
                    ModelState.AddModelError(nameof(model.DateOper), $"Не може да въвеждате посещение преди предното посещение {prevOper.DateOper.ToString(FormattingConstant.NormalDateFormat)}");
            }
   
        }
        public JsonResult CheckDeliveryDate(DeliveryItemOperVM model)
        {
            ValidateModel(model);
            var result = "OK";
            if (ModelState.IsValid)
            {
                var opers = service.DeliveryItemOperSelect(model.DeliveryItemId, true)
                                           .Where(x => x.DeliveryOperId < model.DeliveryOperId)
                                           .OrderBy(x => x.DeliveryOperId)
                                           .ToList();
                var prevOper = opers.LastOrDefault();
                if (prevOper != null)
                {
                    var dR = model.DateOper - prevOper.DateOper;
                    if (dR.Value.TotalDays < 7)
                        result = $"Няма 7 дни от предното посещение {prevOper.DateOper.ToString(FormattingConstant.NormalDateFormat)}. ";
                }
                if (opers.Count >= 2 && model.DeliveryOperId != NomenclatureConstants.NotificationState.Delivered)
                {
                    bool haveHoliday = !workingDaysService.IsWorkingDay(userContext.CourtId, model.DateOper?.Date ?? DateTime.Now);
                    haveHoliday = haveHoliday || opers.Max(x => !workingDaysService.IsWorkingDay(userContext.CourtId, (DateTime)(x.DateOper).Date));
                    if (!haveHoliday)
                    {
                        result += $"Няма посещение в почивен ден";
                    }
                }
            } else
            {
                result = "NOT_VALID";
            }
            return Json(result);
        }
    }
}