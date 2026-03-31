using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IWorkNotificationService workNotificationService;


        public DeliveryItemOperController(
            IDeliveryItemOperService _service,
            IDeliveryItemService _itemService,
            ICommonService _commonService,
            INomenclatureService _nomService,
            IConfiguration _configuration,
            IWorkingDaysService _workingDaysService,
            IWorkNotificationService _workNotificationService)
        {
            service = _service;
            itemService = _itemService;
            commonService = _commonService;
            nomService = _nomService;
            configuration = _configuration;
            workingDaysService = _workingDaysService;
            workNotificationService = _workNotificationService;
        }
        public async Task<IActionResult> LoadIndex(int DeliveryItemId, string filterJson)
        {
            ViewBag.filterJson = filterJson;
            int filterType = getFilterTypeFromJson(filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemOpers(filterType, DeliveryItemId).DeleteOrDisableLast();
            ViewBag.canAdd = await service.CanAdd(DeliveryItemId);
            ViewBag.NotificationInfo = itemService.GetNotificationInfoByDeliveryItemId(DeliveryItemId);
            SetHelpFile(HelpFileValues.Summons);
            return View(nameof(Index), DeliveryItemId);
        }
        [HttpPost]
        public async Task<IActionResult> Index(int DeliveryItemId, [AllowHtml] string filterJson)
        {
            var deliveryItem = itemService.getDeliveryItemWithNotification(DeliveryItemId);
            AddAuditInfoDelivery(deliveryItem, AuditConstants.Operations.View, NomenclatureConstants.DeliveryItemAuditLog.Visit, true);
            return await LoadIndex(DeliveryItemId, filterJson);
        }
        private void AddAuditInfoDelivery(DeliveryItem deliveryItem, string operation, string addInfo, bool setUrl = false)
        {
            try
            {
                var visit = service.LastVisitLabel(deliveryItem.Id);
                if (deliveryItem.CaseNotificationId == null)
                {
                    AddAuditInfo(operation,
                                 $"{deliveryItem.CaseInfo}",
                                 $"{deliveryItem.RegNumber}  {visit} {addInfo}",
                                 SourceTypeSelectVM.CaseNotification,
                                 setUrl);
                    return;
                }

                var context = service.GetCurrentContext(SourceTypeSelectVM.CaseNotification, deliveryItem.CaseNotificationId, AuditConstants.Operations.Append, deliveryItem.CaseNotification?.CaseId);
                AddAuditInfo(operation,
                             context?.Info?.BaseObject,
                             $"{context?.Info?.ObjectInfo}  {visit} {addInfo}",
                             SourceTypeSelectVM.CaseNotification,
                             setUrl);
            }
            catch
            {

            }
        }
        [HttpGet]
        public async Task<IActionResult> Index(int notificationId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationDeliveryOper(notificationId).DeleteOrDisableLast();
            var deliveryItem = itemService.GetDeliveryItemByCaseNotificationId(notificationId);
            int deliveryItemId = deliveryItem?.Id ?? 0;

            if (deliveryItem != null)
                AddAuditInfoDelivery(deliveryItem, AuditConstants.Operations.View, NomenclatureConstants.DeliveryItemAuditLog.Visit, true);
            //else
            //{
            //    throw new NotFoundException("Ненамерено или невалидно уведомление;");
            //}
            ViewBag.filterJson = "";
            ViewBag.canAdd = await service.CanAdd(deliveryItemId);
            ViewBag.NotificationInfo = itemService.GetNotificationInfo(notificationId);
            return View(deliveryItemId);
        }
        [HttpGet]
        public async Task<IActionResult> IndexDocument(int notificationId)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDocumentNotificationDeliveryOper(notificationId).DeleteOrDisableLast();
            var deliveryItem = itemService.GetDeliveryItemByDocumentNotificationId(notificationId);
            int deliveryItemId = deliveryItem?.Id ?? 0;
            ViewBag.filterJson = "";
            ViewBag.canAdd = await service.CanAdd(deliveryItemId);
            ViewBag.NotificationInfo = itemService.GetNotificationInfo(notificationId);
            return View(nameof(Index), deliveryItemId);
        }
        public IActionResult IndexHistory(int deliveryItemId, [AllowHtml] string filterJson)
        {
            ViewBag.filterJson = filterJson;
            int filterType = getFilterTypeFromJson(filterJson);

            var deliveryItem = itemService.getDeliveryItemWithNotification(deliveryItemId);
            AddAuditInfoDelivery(deliveryItem, AuditConstants.Operations.View, NomenclatureConstants.DeliveryItemAuditLog.VisitHistory, true);

            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemHistoryOpers(filterType, deliveryItemId).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Summons);

            return View(deliveryItemId);
        }
        public async Task<IActionResult> Edit(int id, [AllowHtml] string filterJson)
        {
            var model = service.getDeliveryItemOper(id);
            var deliveryItem = itemService.getDeliveryItemWithNotification(model.DeliveryItemId);
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
            if (deliveryItem.DocumentNotificationId > 0)
            {
            }
            else
            {
                AddAuditInfoDelivery(deliveryItem, AuditConstants.Operations.View, NomenclatureConstants.DeliveryItemAuditLog.VisitEdit, true);
            }
            await SetViewbag(model.DeliveryOperId);
            ModelState.Clear();
            return View(nameof(Edit), model);
        }
        [DisableAudit]
        public async Task<IActionResult> Add(int deliveryItemId, [AllowHtml] string filterJson)
        {
            ViewBag.filterJson = filterJson;
            var deliveryItem = itemService.getDeliveryItemWithNotification(deliveryItemId);
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
            await SetViewbag(model.DeliveryOperId);
            ModelState.Clear();
            return View(nameof(Edit), model);
        }
        [HttpPost]
        public async Task<IActionResult> EditPost(DeliveryItemOperVM model, [AllowHtml] string filterJson)
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
            await SetViewbag(model.DeliveryOperId);
            await ValidateModel(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            var deliveryItemOperSame = await service.GetSameOperIfHave(model.DeliveryItemId, model.DeliveryOperId);
            var oldIsStuck = deliveryItemOperSame == null ? 
                             false : 
                             NomenclatureConstants.NotificationState.NotificationStateStuckMessage().Contains(deliveryItemOperSame.NotificationStateId);
            var isInsert = deliveryItemOperSame == null;
            var logVM = new DeliveryLogVM()
            {
                Action = isInsert ? "Добавяне" : "Редактиране",
                PageLabel = "Посещения",
                PageUrl = "DeliveryItemOper//EditPost"
            };
            if (await itemService.DeliveryItemSaveOper(model, logVM))
            {
                if (deliveryItem.DocumentNotificationId > 0)
                {
                }
                else
                {
                    var IsStuck = NomenclatureConstants.NotificationState.NotificationStateStuckMessage().Contains(model.NotificationStateId);
                    if (oldIsStuck)
                    {
                        await workNotificationService.ExpiredNotificationsForStuckMessagesFastProcess(deliveryItem.CaseNotificationId ?? 0);
                    }
                    if (IsStuck)
                    {
                        //await workNotificationService.SaveNotificationsForStuckMessagesFastProcess(deliveryItem.CaseNotificationId ?? 0, model.DateOper ?? DateTime.Now, true);
                        await workNotificationService.GetNotificationsForLackSubmittedObjectionFastProcess(deliveryItem.CaseNotificationId ?? 0, (model.DateOper ?? DateTime.Now).AddDays(14));
                    }
                    AddAuditInfoDelivery(deliveryItem, isInsert ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, NomenclatureConstants.DeliveryItemAuditLog.VisitEdit, false);
                }

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
        public async Task<JsonResult> NotificationState(int operId)
        {
            return Json(await service.NotificationStateForDeliveryOperSelect(operId));
        }

        [HttpPost]
        public async Task<IActionResult> ListData(IDataTablesRequest request, int deliveryItemId, bool onlyLast)
        {
            var data = await service.DeliveryItemOperSelect(deliveryItemId, onlyLast);
            return request.GetResponse(data.AsQueryable());
        }
        public IActionResult Location(int id, [AllowHtml] string filterJson)
        {
            var deliveryItemOper = service.getDeliveryItemOper(id);
          
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
        async Task SetViewbag(int operId)
        {
            ViewBag.DeliveryOperId_ddl = await service.DeliveryOperSelect(operId);
            ViewBag.NotificationStateId_ddl = await service.NotificationStateForDeliveryOperSelect(operId);
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
        private async Task ValidateModel(DeliveryItemOperVM model)
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

            var opers = (await service.DeliveryItemOperSelect(model.DeliveryItemId, true))
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
        public async Task<JsonResult> CheckDeliveryDate(DeliveryItemOperVM model)
        {
            await ValidateModel(model);
            var result = "OK";
            var errors = new List<string>();
            if (ModelState.IsValid)
            {
                errors = await service.CheckDeliveryDate(model);
                if (errors.Any())
                    result = "ERROR";
            }
            else
            {
                result = "NOT_VALID";
            }
            return Json(new { result, errors });
        }
    }
}