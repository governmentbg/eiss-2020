using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class DeliveryItemController : BaseController
    {
        private readonly IDeliveryItemService service;
        private readonly IDeliveryAreaService areaService;
        private readonly IDeliveryAreaAddressService deliveryAreaAddressService;
        private readonly ICommonService commonService;
        private readonly ICompositeViewEngine viewEngine;
        private readonly INomenclatureService nomService;
        private readonly ICourtLawUnitService courtLawUnitService;
        private readonly ICaseNotificationService notificationService;
        private readonly ICdnService cdnService;
        private readonly ICourtDepartmentService courtDepartmentService;
        public DeliveryItemController(
            IDeliveryItemService _deliveryItemService,
            ICommonService _commonService,
            ICompositeViewEngine _viewEngine,
            INomenclatureService _nomService,
            IDeliveryAreaService _areaService,
            IDeliveryAreaAddressService _deliveryAreaAddressService,
            ICourtLawUnitService _courtLawUnitService,
            ICaseNotificationService _notificationService,
            ICdnService _cdnService,
            ICourtDepartmentService _courtDepartmentService
            )
        {
            service = _deliveryItemService;
            commonService = _commonService;
            viewEngine = _viewEngine;
            nomService = _nomService;
            areaService = _areaService;
            deliveryAreaAddressService = _deliveryAreaAddressService;
            courtLawUnitService = _courtLawUnitService;
            notificationService = _notificationService;
            cdnService = _cdnService;
            courtDepartmentService = _courtDepartmentService;
        }

        private void SetHelpFileByFilterType(int filterType)
        {
            if (filterType == NomenclatureConstants.DeliveryItemFilterType.Inner)
                SetHelpFile(HelpFileValues.Summons1);
            else if (filterType == NomenclatureConstants.DeliveryItemFilterType.FromOther)
                SetHelpFile(HelpFileValues.Summons2);
            else if (filterType == NomenclatureConstants.DeliveryItemFilterType.ToOther)
                SetHelpFile(HelpFileValues.Summons3);
        }
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]

        public IActionResult Index(int? filterType)
        {
            var model = new DeliveryItemFilterVM();
            model.FilterType = filterType ?? 0;
            if (model.FilterType == 0)
                model.FilterType = NomenclatureConstants.DeliveryItemFilterType.Inner;
            model.NoAutoLoad = "Y";

            SetHelpFileByFilterType(model.FilterType);

            return LoadIndex(model);
        }
        public IActionResult LoadIndex(DeliveryItemFilterVM model)
        {
            if (model == null)
                model = new DeliveryItemFilterVM();
            if ((model.FilterType <= 0) || (model.FilterType > NomenclatureConstants.DeliveryItemFilterType.ToOther))
                model.FilterType = NomenclatureConstants.DeliveryItemFilterType.Inner;
            SetViewbag(userContext.CourtId, model.FilterType);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItems(model.FilterType).DeleteOrDisableLast();
            return View(nameof(Index), model);
        }

        [HttpPost]
        [DisableAudit]
        public IActionResult Index([AllowHtml] string filterJson)
        {
            DeliveryItemFilterVM model = null;
            if (!string.IsNullOrEmpty(filterJson))
            {
                var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
                model = JsonConvert.DeserializeObject<DeliveryItemFilterVM>(filterJson, dateTimeConverter);
            }
            return LoadIndex(model);
        }

        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult IndexTrans(int toNotificationStateId)
        {
            var model = new DeliveryItemTransFilterVM
            {
                NotificationDeliveryGroupId = NomenclatureConstants.NotificationDeliveryGroup.WithSummons
            };
            model.ToNotificationStateId = toNotificationStateId;
            model.initNotificationStateId();
            model.NewCourtId = userContext.CourtId;
            SetViewbagToCourt(model);

            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemsTrans(toNotificationStateId).DeleteOrDisableLast();

            if (model.ToNotificationStateId == NomenclatureConstants.NotificationState.Send)
                SetHelpFile(HelpFileValues.Summons4);
            else if (model.ToNotificationStateId == NomenclatureConstants.NotificationState.Received)
                SetHelpFile(HelpFileValues.Summons5);
            else if (model.ToNotificationStateId == NomenclatureConstants.NotificationState.ForDelivery)
                SetHelpFile(HelpFileValues.Summons6);

            return View(nameof(IndexTrans), model);
        }

        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult ChangeLawUnit()
        {
            DeliveryItemChangeLawUnitVM model = new DeliveryItemChangeLawUnitVM();
            model.CourtId = userContext.CourtId;
            model.NewCourtId = model.CourtId;
            SetViewbagChangeLawUnit();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemChangeLawUnit().DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Summons7);

            return View(model);
        }


        public IActionResult Edit(int id, string filterJson)
        {
            ViewBag.filterJson = filterJson;
            SetViewbagArea(userContext.CourtId);
            SetViewbag(-1);
            var model = service.getDeliveryItemWithNotification(id);

            AddAuditInfoDelivery(model, AuditConstants.Operations.View, NomenclatureConstants.DeliveryItemAuditLog.ChangeRaion, true);

            ModelState.Clear();
            int filterType = getFilterTypeFromJson(filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemEditRaion(filterType, id).DeleteOrDisableLast();
            ViewBag.filterType = filterType;
            SetHelpFile(HelpFileValues.Summons);

            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult EditPost(DeliveryItem model, string filterJson)
        {
            ViewBag.filterJson = filterJson;
            int filterType = getFilterTypeFromJson(filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemEditRaion(filterType, model.Id).DeleteOrDisableLast();
            SetViewbagArea(userContext.CourtId);
            SetViewbag(-1);

            var currentId = model.Id;
            var logVM = new DeliveryLogVM()
            {
                Action = new DeliveryItemFilterVM() { FilterType = filterType }.getDeliveryTypeName(),
                PageLabel = "Смяна на район и призовкар",
                PageUrl = "EditPost"
            };
            if (service.DeliveryItemSaveArea(model.Id, model.CourtId, model.DeliveryAreaId, model.LawUnitId, logVM))
            {
                // Журнал
                var deliveryItem = service.getDeliveryItemWithNotification(model.Id);
                AddAuditInfoDelivery(deliveryItem, AuditConstants.Operations.Update, NomenclatureConstants.DeliveryItemAuditLog.ChangeRaion);

                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id, filterJson });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        private void AddAuditInfoDelivery(DeliveryItem deliveryItem, string operation, string addInfo, bool setUrl = false)
        {
            try
            {
                if (deliveryItem.CaseNotificationId == null)
                {
                    AddAuditInfo(operation,
                                 deliveryItem.CaseInfo,
                                 $"{deliveryItem.RegNumber} {addInfo}",
                                 SourceTypeSelectVM.CaseNotification,
                                 setUrl);
                    return;
                }
                var context = service.GetCurrentContext(SourceTypeSelectVM.CaseNotification, deliveryItem.CaseNotificationId, AuditConstants.Operations.Append, deliveryItem.CaseNotification?.CaseId);
                AddAuditInfo(operation,
                             context?.Info?.BaseObject,
                             $"{context?.Info?.ObjectInfo} {addInfo}",
                             SourceTypeSelectVM.CaseNotification,
                             setUrl);
            }
            catch (Exception)
            {
            }
        }

        [HttpPost]
        public IActionResult CheckReceived(string filterJson)
        {
            ViewBag.filterJson = filterJson;
            SetViewbagArea(userContext.CourtId);
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> CheckReceivedEdit(string regNumber)
        {
            bool saveIfErr = false;
            var logVM = new DeliveryLogVM()
            {
                Action = "Чекиране",
                PageLabel = "Приемане на призовки/писма",
                PageUrl = "CheckReceivedEdit"
            };
            SetViewbagArea(userContext.CourtId);
            (DeliveryItemRecieveVM delivery, string messageErr) = await service.SaveRecieved(regNumber, saveIfErr, logVM);
            if (delivery == null)
                return Json(new { messageErr = messageErr });
            else
                return Json(new { delivery = delivery });
        }
        [DisableAudit]
        public IActionResult AddReceived(string filterJson)
        {
            ViewBag.filterJson = filterJson;
            ViewBag.ToCourtId = userContext.CourtId;
            SetViewbag(userContext.CourtId);
            SetViewbagArea(userContext.CourtId);

            DeliveryItem newModel = new DeliveryItem();
            newModel.CourtId = userContext.CourtId;
            ViewBag.conteinerId = Guid.NewGuid();
            int filterType = getFilterTypeFromJson(filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemAdd(filterType).DeleteOrDisableLast();
            return View(newModel);
        }
        private void ValidateModel(DeliveryItem model)
        {
            if (model.RegDate == null)
                ModelState.AddModelError(nameof(DeliveryItem.RegDate), "Въведете дата изготвяне");
            if (model.DateAccepted == null)
                ModelState.AddModelError(nameof(DeliveryItem.DateAccepted), "Въведете дата приемане");
            if (model.RegDate?.Date > model.DateAccepted?.Date)
                ModelState.AddModelError(nameof(DeliveryItem.RegDate), "Въведете дата изготвяне трябва да е преди дата приeмане.");
        }

        [HttpPost]
        [DisableAudit]
        public async Task<JsonResult> AddReceivedEdit(DeliveryItem model, string guidForSave)
        {
            SetViewbag(userContext.CourtId);
            SetViewbagArea(userContext.CourtId);

            string viewAdd = "";
            string viewSaved = "";
            string saveOk = "";
            string saveFailed = "";
            bool isOk = false;
            string messageErr = IsExists(model);
            List<SelectListItem> errors = null;
            ViewBag.conteinerId = guidForSave;
            string conteinerIdAdd = ViewBag.conteinerId;
            ValidateModel(model);
            if (!ModelState.IsValid || !string.IsNullOrEmpty(messageErr))
            {
                viewAdd = await RenderPartialViewToString("_AddReceived", model);
                errors = ModelState.Where(x => x.Value.ValidationState == ModelValidationState.Invalid)
                                   .Select(x => new SelectListItem() { Value = x.Key, Text = x.Value.Errors.First().ErrorMessage })
                                   .ToList();
                //errors = modelErrors.Select(x => new SelectListItem() {Value = x.Key, Text = x. })
            }
            else
            {
                model.CaseNotificationId = null;
                model.NotificationStateId = NomenclatureConstants.NotificationState.Received;
                var logVM = new DeliveryLogVM()
                {
                    Action = "Добавяне",
                    PageLabel = "Призовки/съобщения изготвени в друг съд",
                    PageUrl = "AddReceivedEdit"
                };
                if (await service.DeliveryItemSaveDataAddReceived(model, logVM))
                {
                    isOk = true;

                    saveOk = MessageConstant.Values.SaveOK;
                    DeliveryItem newModel = new DeliveryItem();
                    newModel.FromCourtId = model.FromCourtId;
                    var guid = Guid.NewGuid();
                    ViewBag.conteinerId = guid;
                    conteinerIdAdd = guid.ToString();
                    viewAdd = await RenderPartialViewToString("_AddReceived", newModel);
                    ViewBag.showButtons = false;
                    ViewBag.conteinerId = Guid.NewGuid();
                    viewSaved = await RenderPartialViewToString("_AddReceived", model);
                    AddAuditInfo(AuditConstants.Operations.Append, model.CaseInfo, model.RegNumber, SourceTypeSelectVM.DeliveryItem);
                }
                else
                {
                    saveFailed = MessageConstant.Values.SaveFailed;
                    viewAdd = await RenderPartialViewToString("_AddReceived", model);
                }
            }
            return Json(new
            {
                isOk,
                messageErr,
                viewAdd,
                viewSaved,
                saveOk,
                saveFailed,
                conteinerIdAdd,
                errors
            });
        }

        [HttpPost]
        public async Task<JsonResult> GetReceivedForToday()
        {
            SetViewbag(userContext.CourtId);
            SetViewbagArea(userContext.CourtId);

            bool isOk = false;
            string messageErr = "";
            List<string> viewsForDay = new List<string>();

            List<DeliveryItem> deliveries = service.GetReceivedForToday(userContext.UserId, DateTime.Now.Date).ToList();
            foreach (var model in deliveries)
            {
                ViewBag.showButtons = false;
                ViewBag.conteinerId = Guid.NewGuid();
                var item = await RenderPartialViewToString("_AddReceived", model);
                viewsForDay.Add(item);
            }
            return Json(new
            {
                isOk = isOk,
                messageErr = messageErr,
                viewsForDay = viewsForDay
            });
        }

        [HttpPost]
        public JsonResult GetDeliveryAreaId(DeliveryItem model)
        {
            var deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaAddressFind(model?.Address, model?.FromCourtId ?? 0);
            return Json(new
            {
                deliveryAreaId = deliveryAreaFind.DeliveryAreaId,
                lawUnitId = deliveryAreaFind.LawUnitId
            });
        }

        [HttpPost]
        public JsonResult GetDeliveryAreaIdEditFromIndex(int deliveryItemId)
        {
            DeliveryItem model = service.getDeliveryItem(deliveryItemId);
            var deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaAddressFind(model?.Address, model?.FromCourtId ?? 0);
            //if (deliveryAreaFind.DeliveryAreaId <= 0 && deliveryAreaFind.DeliveryAreaList.Any(x => x.CourtId == model.CourtId))
            //{
            //    var delivaryArea = deliveryAreaFind.DeliveryAreaList.Where(x => x.CourtId == model.CourtId).First();
            //    deliveryAreaFind.ToCourtId = delivaryArea.CourtId;
            //    deliveryAreaFind.DeliveryAreaId = delivaryArea.Id;
            //    deliveryAreaFind.LawUnitId = delivaryArea.LawUnitId ?? -1;
            //}
            return Json(new
            {
                toCourtId = deliveryAreaFind.ToCourtId,
                deliveryAreaId = deliveryAreaFind.DeliveryAreaId,
                lawUnitId = deliveryAreaFind.LawUnitId
            });
        }
        [HttpPost]
        public JsonResult GetDeliveryAreaAndCourt(int notificationPersonType, int casePersonAddressId, int lawUnitAddressId)
        {
            DeliveryAreaFindVM deliveryAreaFind;
            if (notificationPersonType == 2)
                deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaAddressIdFind(lawUnitAddressId, userContext.CourtId);
            else
                deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaCasePersonAddressIdFind(casePersonAddressId, userContext.CourtId);
            return Json(new
            {
                toCourtId = deliveryAreaFind.ToCourtId,
                deliveryAreaId = deliveryAreaFind.DeliveryAreaId,
                lawUnitId = deliveryAreaFind.LawUnitId,
                deliveryAreaDDL2 = areaService.DeliveryAreaListToDdlSelect2(deliveryAreaFind.DeliveryAreaList),
                toCourtDDL2 = service.GetCourtsSelect2(deliveryAreaFind.DeliveryAreaList),
            });
        }
        [HttpPost]
        public JsonResult GetDeliveryAreaAndCourtDocument(int notificationPersonType, int documentPersonAddressId, int lawUnitAddressId)
        {
            DeliveryAreaFindVM deliveryAreaFind;
            if (notificationPersonType == 2)
                deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaAddressIdFind(lawUnitAddressId, userContext.CourtId);
            else
                deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaDocumentPersonAddressIdFind(documentPersonAddressId, userContext.CourtId);
            return Json(new
            {
                toCourtId = deliveryAreaFind.ToCourtId,
                deliveryAreaId = deliveryAreaFind.DeliveryAreaId,
                lawUnitId = deliveryAreaFind.LawUnitId,
                deliveryAreaDDL2 = areaService.DeliveryAreaListToDdlSelect2(deliveryAreaFind.DeliveryAreaList),
                toCourtDDL2 = service.GetCourtsSelect2(deliveryAreaFind.DeliveryAreaList),
            });
        }
        [HttpGet]
        public async Task<JsonResult> GetDeliveryAreaAndCourtGroup(int notificationPersonType, int addressId, bool allCourtDdl)
        {
            var deliveryAreaFind = (notificationPersonType == 2) ?
                                   deliveryAreaAddressService.DeliveryAreaAddressIdFind(addressId, userContext.CourtId) :
                                   deliveryAreaAddressService.DeliveryAreaCasePersonAddressIdFind(addressId, userContext.CourtId);
            var deliveryAreaName = (await service.GetByIdAsync<DeliveryArea>(deliveryAreaFind.DeliveryAreaId))?.Description;
            var lawUnitName = (await service.GetByIdAsync<LawUnit>(deliveryAreaFind.LawUnitId))?.FullName;
            var toCourtName = (await service.GetByIdAsync<Court>(deliveryAreaFind.ToCourtId))?.Label;
            return Json(new
            {
                toCourtId = deliveryAreaFind.ToCourtId,
                toCourtName,
                deliveryAreaId = deliveryAreaFind.DeliveryAreaId,
                deliveryAreaName,
                lawUnitId = deliveryAreaFind.LawUnitId,
                lawUnitName,
                deliveryAreaDDL2 = areaService.DeliveryAreaListToDdlSelect2(deliveryAreaFind.DeliveryAreaList),
                toCourtDDL2 = service.GetCourtsSelect2(deliveryAreaFind.DeliveryAreaList),
            });
        }

        public IActionResult GetNotificationGroupDeliveryArea()
        {
            NotificationItemVM model = new();
            return PartialView("_NotificationGroupDeliveryArea", model);
        }

        [HttpPost]
        public JsonResult GetDeliveryAreaAndCourtForAddReceived(DeliveryItem deliveryItem)
        {
            var deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaAddressFind(deliveryItem.Address, deliveryItem.FromCourtId);
            if (deliveryAreaFind.DeliveryAreaId <= 0 && deliveryAreaFind.DeliveryAreaList.Any(x => x.CourtId == deliveryItem.CourtId))
            {
                var delivaryArea = deliveryAreaFind.DeliveryAreaList.Where(x => x.CourtId == deliveryItem.CourtId).First();
                deliveryAreaFind.ToCourtId = delivaryArea.CourtId;
                deliveryAreaFind.DeliveryAreaId = delivaryArea.Id;
                deliveryAreaFind.LawUnitId = delivaryArea.LawUnitId ?? -1;
            }
            return Json(new
            {
                toCourtId = deliveryAreaFind.ToCourtId,
                deliveryAreaId = deliveryAreaFind.DeliveryAreaId,
                lawUnitId = deliveryAreaFind.LawUnitId
            });
        }

        [HttpPost]
        [DisableAudit]
        public async Task<JsonResult> SaveTrans(int[] deliveryItemIdsJson, DeliveryItemTransFilterVM filterData)
        {
            var logVM = new DeliveryLogVM()
            {
                Action = "Трансфер",
                PageLabel = DeliveryItemTransFilterVM.GetTitle(filterData.ToNotificationStateId),
                PageUrl = "SaveTrans"
            };

            bool result = await service.SaveTrans(deliveryItemIdsJson, filterData.ToNotificationStateId, filterData.ToNotificationStateId, logVM);
            if (result)
            {
                AddAuditInfo(AuditConstants.Operations.Update, $"{DeliveryItemTransFilterVM.GetTitle(filterData.ToNotificationStateId)}", $"{deliveryItemIdsJson.Length} бр. уведомления", SourceTypeSelectVM.CaseNotification);
            }
            return Json(new
            {
                result = result,
                forId = filterData.ForId
            });
        }
        [HttpPost]
        public async Task<JsonResult> SaveChangeLawUnit(int[] deliveryItemIdsJson, DeliveryItemChangeLawUnitVM filterData)
        {
            var logVM = new DeliveryLogVM()
            {
                Action = "Смяна на призовкар",
                PageLabel = filterData.ToNotificationStateId > 0 ? DeliveryItemTransFilterVM.GetTitle(filterData.ToNotificationStateId) : "Смяна на призовкар",
                PageUrl = "SaveChangeLawUnitm"
            };
            bool result = await service.SaveChangeLawUnit(deliveryItemIdsJson, filterData, logVM);
            if (result)
            {
                var changeInfo = service.ChangeLawUnitAuditInfo(filterData);
                AddAuditInfo(AuditConstants.Operations.Update, $"Смяна на призовкар на {deliveryItemIdsJson.Length} бр. уведомления", changeInfo, SourceTypeSelectVM.CaseNotification);
            }
            return Json(new
            {
                result = result
            });
        }

        [HttpPost]
        public async Task<JsonResult> LoadForId_DDL(DeliveryItemTransFilterVM filterData)
        {
            var items = await service.DeliveryItemTransForIdDDL(filterData);
            return Json(new { forId_ddl = items });
        }

        private async Task<string> RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                ViewEngineResult viewResult = viewEngine.FindView(ControllerContext, viewName, false);

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, DeliveryItemFilterVM filterData)
        {
            filterData.ResetCourtByType(userContext.CourtId);
            var data = service.DeliveryItemSelect(filterData);
            return request.GetResponse(data);
        }

        [HttpPost]
        public async Task<JsonResult> ListDataTrans(IDataTablesRequest request, DeliveryItemTransFilterVM filterData)
        {
            var data = service.DeliveryItemTransSelect(filterData);
            //var list = data.ToList();
            return Json(await data.ToListAsync());
        }

        [HttpPost]
        public IActionResult ListDataChangeLawUnit(IDataTablesRequest request, DeliveryItemChangeLawUnitVM filterData)
        {
            var newLawUnit = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, filterData.CourtId, true);
            var newLawUnitId = newLawUnit.Where(x => x.Value != "-1").Select(x => int.Parse(x.Value)).ToArray();
            var data = service.DeliveryItemChangeLawUnitSelect(filterData, newLawUnitId);
            return Json(data.ToList());
        }

        [HttpPost]
        public JsonResult GetCheckedForToday()
        {
            List<DeliveryItemRecieveVM> deliveries = service.GetCheckedForToday(userContext.UserId, DateTime.Now.Date).ToList();
            return Json(new { deliveries = deliveries });
        }

        public IActionResult EditReturn(int deliveryItemId, string filterJson, bool isFromVisit)
        {
            if (filterJson == null && TempData["filterJson"] != null)
                filterJson = TempData["filterJson"].ToString();
            ViewBag.filterJson = filterJson;
            var model = service.GetDeliveryItemReturn(deliveryItemId);

            var deliveryItem = service.getDeliveryItemWithNotification(deliveryItemId);
            AddAuditInfoDelivery(deliveryItem, AuditConstants.Operations.View, NomenclatureConstants.DeliveryItemAuditLog.EditReturn, true);

            SetViewbag(-1);
            ModelState.Clear();

            int filterType = getFilterTypeFromJson(filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemEditReturn(filterType, deliveryItemId).DeleteOrDisableLast();

            return View(nameof(EditReturn), model);
        }
        public IActionResult NotificatiionEditReturn(int notificationId)
        {
            ViewBag.filterJson = null;
            var model = service.GetDeliveryItemReturnByNotification(notificationId);

            var deliveryItem = service.getDeliveryItemWithNotification(model.Id);
            AddAuditInfoDelivery(deliveryItem, AuditConstants.Operations.View, NomenclatureConstants.DeliveryItemAuditLog.EditReturn, true);

            SetViewbag(-1);
            ModelState.Clear();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEditReturn(notificationId).DeleteOrDisableLast();

            return View(nameof(EditReturn), model);
        }
        public IActionResult NotificationEditReturnDocument(int notificationId)
        {
            ViewBag.filterJson = null;
            var model = service.GetDeliveryItemReturnByDocumentNotification(notificationId);

            SetViewbag(-1);
            ModelState.Clear();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDocumentNotificationEditReturn(notificationId).DeleteOrDisableLast();

            return View(nameof(EditReturn), model);
        }

        [HttpPost]
        public async Task<IActionResult> EditReturnPost(ICollection<IFormFile> returnFiles, DeliveryItemReturnVM model, string filterJson)
        {
            ViewBag.filterJson = filterJson;
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            bool saveResult = false;
            int notificationId = 0;
            var logVM = new DeliveryLogVM()
            {
                Action = "Редакция",
                PageLabel = model.DocumentNotificationId > 0 ? "Върнат отрязък към документ" : "Върнат отрязък",
                PageUrl = " EditReturnPost"
            };
            int? caseId = null;
            if (model.DocumentNotificationId > 0)
            {
                (saveResult, notificationId) = await notificationService.DeliveryItemSaveReturnDocument(model, returnFiles, logVM);
            }
            else
            {
                (saveResult, notificationId) = await notificationService.DeliveryItemSaveReturn(model, returnFiles, logVM);
                if (saveResult)
                {
                    var deliveryItem = service.getDeliveryItemWithNotification(model.Id);
                    AddAuditInfoDelivery(deliveryItem, AuditConstants.Operations.Update, NomenclatureConstants.DeliveryItemAuditLog.EditReturn);
                    caseId = deliveryItem.CaseId;
                }
            }

            if (saveResult)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                if (filterJson == null)
                {

                    if (model.DocumentNotificationId > 0)
                    {
                        return RedirectToAction("Edit", "DocumentNotification", new { id = notificationId });
                    }
                    else
                    {
                        if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseNotification, null, AuditConstants.Operations.View, caseId))
                        {
                            ModelState.Clear();
                            return Index(string.Empty);
                        }
                        return RedirectToAction("Edit", "CaseNotification", new { id = notificationId });
                    }
                }
                else
                {
                    ModelState.Clear();
                    return Index(filterJson);
                }
            }
            else
            {
                if (filterJson == null)
                {
                    var delivery = service.GetById<DeliveryItem>(model.Id);
                    int notificationIdErr = delivery.CaseNotificationId ?? 0;
                    ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEditReturn(notificationIdErr).DeleteOrDisableLast();
                }
                else
                {
                    int filterType = getFilterTypeFromJson(filterJson);
                    ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemEditReturn(filterType, model.Id).DeleteOrDisableLast();
                }
                SetViewbag(-1);
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return View(nameof(EditReturn), model);
            }

        }


        public IActionResult EditState(int id, string filterJson)
        {
            ViewBag.filterJson = filterJson;
            var model = service.getDeliveryItemWithNotification(id);
            model.DeliveryDateCC = model.DeliveryDate;
            if (model.DeliveryDateCC == null)
                model.DeliveryDateCC = DateTime.Now;
            SetViewbagState(model.CaseNotification?.NotificationDeliveryGroupId ?? -1);
            ModelState.Clear();

            int filterType = getFilterTypeFromJson(filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemEditReturn(filterType, id).DeleteOrDisableLast();

            return View(nameof(EditState), model);
        }

        [HttpPost]
        public async Task<IActionResult> EditStatePost(DeliveryItem model, string filterJson)
        {
            ViewBag.filterJson = filterJson;
            var notification = notificationService.GetById<CaseNotification>(model.CaseNotificationId);
            if (notification.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
            {
                ModelState.AddModelError(nameof(model.NotificationStateId), $"Не може да променяте призовки подадени през ЕПЕП");
            }
            if (model.DeliveryDateCC < model.RegDate?.Date)
            {
                ModelState.AddModelError(nameof(model.DeliveryDateCC), $"{MessageConstant.ValidationErrors.DeliveryDateBeforeRegDate} {model.RegDate?.ToString(FormattingConstant.NormalDateFormat)}");
            }
            if (model.DeliveryDateCC > DateTime.Now.AddMinutes(10))
            {
                ModelState.AddModelError(nameof(model.DeliveryDateCC), MessageConstant.ValidationErrors.DeliveryDateFuture);
            }
            int filterType = getFilterTypeFromJson(filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemEditReturn(filterType, model.Id).DeleteOrDisableLast();
            if (!ModelState.IsValid)
            {
                SetViewbagState(notification?.NotificationDeliveryGroupId ?? -1);
                return View(nameof(EditState), model);
            }
            var logVM = new DeliveryLogVM()
            {
                Action = "Редакция",
                PageLabel = "Промяна на статус при разнасяне с куриер/кметство",
                PageUrl = " EditStatePost"
            };
            if (await service.DeliveryItemSaveState(model.Id, model.NotificationStateId, model.DeliveryDateCC, model.DeliveryInfo, logVM))
            {
                this.SaveLogOperation(false, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditState), new { id = model.Id, filterJson });
            }
            else
            {
                SetViewbagState(notification?.NotificationDeliveryGroupId ?? -1);
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return View(nameof(EditState), model);
            }
        }
        #region Out/ResultList
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult OutList()
        {
            DeliveryItemListVM model = new DeliveryItemListVM()
            {
                FromCourtId = userContext.CourtId
            };
            SetViewbagOutList();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryOutList().DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Register23);

            return View(nameof(OutList), model);
        }

        [HttpPost]
        public IActionResult ListDataOut(IDataTablesRequest request, DeliveryItemListVM filterData)
        {
            var data = service.GetDeliveryItemOutReport(filterData, true);
            return request.GetResponse(data);
        }

        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
        public IActionResult ResultList()
        {
            DeliveryItemListVM model = new DeliveryItemListVM()
            {
                FromCourtId = userContext.CourtId
            };
            SetViewbagOutList();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryResultList().DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Register24);

            return View(nameof(ResultList), model);
        }

        [HttpPost]
        public IActionResult ExcelDataOut([AllowHtml] string filterJson)
        {
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            DeliveryItemListVM model = JsonConvert.DeserializeObject<DeliveryItemListVM>(filterJson, dateTimeConverter);
            (var xlsBytes, var fileName) = service.GetDeliveryItemOutToExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, fileName);
        }

        [HttpPost]
        public IActionResult ExcelDataResult([AllowHtml] string filterJson)
        {
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            DeliveryItemListVM model = JsonConvert.DeserializeObject<DeliveryItemListVM>(filterJson, dateTimeConverter);
            (var xlsBytes, var fileName) = service.GetDeliveryItemReportResultToExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, fileName);
        }

        [HttpPost]
        public IActionResult ExcelDataResultnew([AllowHtml] string filterJson)
        {
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            DeliveryItemListVM model = JsonConvert.DeserializeObject<DeliveryItemListVM>(filterJson, dateTimeConverter);
            (var xlsBytes, var fileName) = service.GetDeliveryItemReportResultToExcelNew(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, fileName);
        }
        void SetViewbagOutList()
        {
            ViewBag.FromCourtId_ddl = commonService.CourtForDelivery_SelectDDL(-1);
            ViewBag.LawUnitId_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, userContext.CourtId);
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CaseTypeId_ddl = nomService.GetDDL_CaseType(-1);//GetDropDownList<CaseType>();
        }

        #endregion Out/ResultList

        string IsExists(DeliveryItem model)
        {
            if (model.Id <= 0)
            {
                var oldItem = service.GetDeliveryItemByRegNumber(model.RegNumber);
                if (oldItem != null)
                {
                    return "Има такъв номер призовка";
                }
            }
            return "";
        }
        void SetViewbag(int courtId, int filterType = 0)
        {
            ViewBag.showButtons = true;
            ViewBag.FromCourtId_ddl = commonService.CourtForDelivery_SelectDDL(courtId);
            ViewBag.CourtId_ddl = ViewBag.FromCourtId_ddl;
            ViewBag.AddressTypesDDL = nomService.GetDropDownList<AddressType>();
            ViewBag.CountriesDDL = nomService.GetCountries();
            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(
                NomenclatureConstants.NotificationDeliveryGroup.WithSummons, -1
            );
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupSelect(filterType);
            ViewBag.NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>();
            ViewBag.LawUnitId_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, userContext.CourtId);
            ViewBag.PreparedById_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.OtherEmployee, userContext.CourtId);
            ViewBag.CourtDepartmentId_ddl = courtDepartmentService.Department_SelectDDL(userContext.CourtId, NomenclatureConstants.DepartmentType.Systav);
            ViewBag.IsGenerated_ddl = nomService.GetDDL_IsGenerated();
            ViewBag.IsFastProcess_ddl = nomService.GetDDL_IsGenerated();
        }
        void SetViewbagState(int notificationDeliveryGroupId)
        {
            ViewBag.NotificationDeliveryGroupId = notificationDeliveryGroupId;
            ViewBag.showButtons = true;
            ViewBag.FromCourtId_ddl = commonService.CourtForDelivery_SelectDDL(-1);
            ViewBag.CourtId_ddl = ViewBag.FromCourtId_ddl;
            ViewBag.AddressTypesDDL = nomService.GetDropDownList<AddressType>();
            ViewBag.CountriesDDL = nomService.GetCountries();
            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(
                notificationDeliveryGroupId, -1
            );
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupSelect(0);
            SetHelpFile(HelpFileValues.Summons);
        }
        void SetViewbagArea(int forCourtId)
        {
            ViewBag.DeliveryAreaId_ddl = areaService.DeliveryAreaSelectDDL(forCourtId, false);
            ViewBag.LawUnitId_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, forCourtId);
        }
        void SetViewbagToCourt(DeliveryItemTransFilterVM filterData)
        {
            var states = service.DeliveryItemTransNotificationState(filterData.ToNotificationStateId).AsQueryable().ToSelectList(true);
            if (filterData.ToNotificationStateId == NomenclatureConstants.NotificationState.Received)
            {
                states.Add(new SelectListItem()
                {
                    Text = "Ненасочени",
                    Value = NomenclatureConstants.NotificationState.NoDeliveryArea.ToString()
                });
                states.Add(new SelectListItem()
                {
                    Text = "Всички Статуси",
                    Value = NomenclatureConstants.NotificationState.AllForReceived.ToString()
                });

            }
            ViewBag.NotificationStateId_ddl = states;
            ViewBag.NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>();
            int forCourtId = userContext.CourtId;
            ViewBag.NewLawUnitType_ddl = service.SelectNewLawUnitType();
            var lawUnits = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, forCourtId, true);
            ViewBag.NewLawUnitId_ddl = areaService.RemoveSelectAddNoChange(lawUnits);
            ViewBag.NewCourtId_ddl = commonService.CourtForDelivery_SelectDDL(-1);
            ViewBag.DeliveryAreaId_ddl = areaService.DeliveryAreaSelectDDL(forCourtId, true);
            ViewBag.NewDeliveryAreaId_ddl = areaService.RemoveSelectAddNoChange(areaService.DeliveryAreaSelectDDL(forCourtId, false));
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupSelect(NomenclatureConstants.DeliveryItemFilterType.Inner);
            ViewBag.IsFastProcess_ddl = nomService.GetDDL_IsGenerated();
        }


        void SetViewbagChangeLawUnit()
        {
            int forCourtId = userContext.CourtId;

            ViewBag.NewLawUnitType_ddl = service.SelectNewLawUnitType();
            var lawUnits = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, forCourtId, true);
            ViewBag.NewLawUnitId_ddl = areaService.RemoveSelectAddNoChange(lawUnits);
            ViewBag.LawUnitId_ddl = service.LawUnitForCourt_SelectDdlAllInDeliveryItem(forCourtId, lawUnits);
            ViewBag.NewCourtId_ddl = commonService.CourtForDelivery_SelectDDL(-1);
            var states = nomService.GetDDL_NotificationStateFromDeliveryGroup(
                NomenclatureConstants.NotificationDeliveryGroup.WithSummons,
                NomenclatureConstants.NotificationState.Visited
            );
            var endStates = NomenclatureConstants.NotificationState.NotificationEndState().Select(x => x.ToString());
            states = states.Where(x => !endStates.Any(e => x.Value == e)).ToList();
            ViewBag.NotificationStateId_ddl = states;

            ViewBag.DeliveryAreaId_ddl = areaService.DeliveryAreaSelectDDL(forCourtId, true);
            ViewBag.NewDeliveryAreaId_ddl = areaService.RemoveSelectAddNoChange(areaService.DeliveryAreaSelectDDL(forCourtId, false));
            ViewBag.NotificationTypeId_ddl = nomService.GetDropDownList<NotificationType>();
        }
        private int getFilterTypeFromJson([AllowHtml] string filterJson)
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

        public IActionResult ToCaseNotification(int Id)
        {
            var deliveryItem = service.GetById<DeliveryItem>(Id);
            return RedirectToAction("Edit", "CaseNotification", new { id = deliveryItem.CaseNotificationId });
        }
        [HttpPost]
        public IActionResult DeliveryItem_ExpiredInfo(ExpiredInfoVM model)
        {
            if (service.SaveExpireInfo<DeliveryItem>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseNotification, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseNotificationExpireOK);
                return Json(new { result = true, redirectUrl = model.ReturnUrl });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        public IActionResult IndexHistory(int deliveryItemId, [AllowHtml] string filterJson)
        {
            ViewBag.filterJson = filterJson;
            int filterType = getFilterTypeFromJson(filterJson);

            var deliveryItem = service.getDeliveryItemWithNotification(deliveryItemId);

            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemHistoryOpers(filterType, deliveryItemId).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Summons);

            return View(new DeliveryItemOperLogVM()
            { DeliveryItemId = deliveryItemId }
            );
        }
        public JsonResult ListDataHistory(int deliveryItemId)
        {
            return Json(service.SelectLog(deliveryItemId));
        }
        public async Task<JsonResult> GetLawUnitName(int? deliveryAreaId)
        {
            var name = string.Empty;
            int? lawUnitId = -1;
            if (deliveryAreaId > 0)
            {
                var deliveryArea = await service.GetByIdAsync<DeliveryArea>(deliveryAreaId);
                if (deliveryArea != null)
                {
                    name = (await service.GetByIdAsync<LawUnit>(deliveryArea.LawUnitId))?.FullName;
                    lawUnitId = deliveryArea.LawUnitId;
                }
            }
            return Json(new { name, lawUnitId });
        }
        [HttpGet]
        public IActionResult GetToCourtAllDdl()
        {
            var ddl = nomService.GetCourts()
                                .Select(x => new Select2ItemVM
                                {
                                    Id = x.Value.ToInt(),
                                    Text = x.Text
                                })
                                .ToList();
            return Json(ddl);
        }

        //[HttpGet]
        //public async Task<IActionResult> TestMobile()
        //{
        //    var data = service.GetDeliveryItemMobileVM(userContext.CourtId, userContext.LawUnitId, DateTime.Now.AddDays(-1600), DateTime.Now.AddDays(1));
        //    return Json(data);
        //}


    }
}