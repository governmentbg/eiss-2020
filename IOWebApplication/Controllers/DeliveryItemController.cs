using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.OData;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        public DeliveryItemController(
            IDeliveryItemService _deliveryItemService,
            ICommonService _commonService,
            ICompositeViewEngine _viewEngine,
            INomenclatureService _nomService,
            IDeliveryAreaService _areaService,
            IDeliveryAreaAddressService _deliveryAreaAddressService,
            ICourtLawUnitService _courtLawUnitService,
            ICaseNotificationService _notificationService,
            ICdnService _cdnService
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
            SetViewbag(userContext.CourtId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItems(model.FilterType).DeleteOrDisableLast();
            return View(nameof(Index), model);
        }

        [HttpPost]
        public IActionResult Index(string filterJson)
        {
            DeliveryItemFilterVM model = null;
            if (!string.IsNullOrEmpty(filterJson))
            {
                var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
                model = JsonConvert.DeserializeObject<DeliveryItemFilterVM>(filterJson, dateTimeConverter);
            }
            return LoadIndex(model);
        }


        public IActionResult IndexTrans(int toNotificationStateId)
        {
            DeliveryItemTransFilterVM model = new DeliveryItemTransFilterVM();
            model.ToNotificationStateId = toNotificationStateId;
            model.initNotificationStateId();
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
            var model = service.getDeliveryItem(id);
            ModelState.Clear();
            int filterType = getFilterTypeFromJson(filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemEditRaion(filterType, id).DeleteOrDisableLast();
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
            if (service.DeliveryItemSaveArea(model.Id, model.CourtId, model.DeliveryAreaId, model.LawUnitId))
            {
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

        [HttpPost]
        public IActionResult CheckReceived(string filterJson)
        {
            ViewBag.filterJson = filterJson;
            SetViewbagArea(userContext.CourtId);
            return View();
        }

        [HttpPost]
        public JsonResult CheckReceivedEdit(string regNumber)
        {
            bool saveIfErr = false;
            SetViewbagArea(userContext.CourtId);
            string messageErr = "";
            DeliveryItemRecieveVM delivery = service.SaveRecieved(regNumber, saveIfErr, out messageErr);
            if (delivery == null)
                return Json(new { messageErr = messageErr });
            else
                return Json(new { delivery = delivery });
        }

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
                if (service.DeliveryItemSaveDataAddReceived(model))
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
            var deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaAddressFind(model?.Address);
            return Json(new
            {
                deliveryAreaId = deliveryAreaFind.DeliveryAreaId,
                lawUnitId = deliveryAreaFind.LawUnitId
            });
        }

        [HttpPost]
        public JsonResult GetDeliveryAreaAndCourt(int notificationPersonType, int casePersonAddressId, int lawUnitAddressId)
        {
            DeliveryAreaFindVM deliveryAreaFind;
            if (notificationPersonType == 2)
                deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaAddressIdFind(lawUnitAddressId);
            else
                deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaCasePersonAddressIdFind(casePersonAddressId);
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
        public JsonResult GetDeliveryAreaAndCourtForAddReceived(DeliveryItem deliveryItem)
        {
            var deliveryAreaFind = deliveryAreaAddressService.DeliveryAreaAddressFind(deliveryItem.Address);
            return Json(new
            {
                toCourtId = deliveryAreaFind.ToCourtId,
                deliveryAreaId = deliveryAreaFind.DeliveryAreaId,
                lawUnitId = deliveryAreaFind.LawUnitId
            });
        }

        [HttpPost]
        public JsonResult SaveTrans(int[] deliveryItemIds, DeliveryItemTransFilterVM filterData)
        {
            bool result = service.SaveTrans(deliveryItemIds, filterData.ToNotificationStateId, filterData.ToNotificationStateId);
            return Json(new
            {
                result = result,
                forId = filterData.ForId
            });
        }
        [HttpPost]
        public JsonResult SaveChangeLawUnit(int[] deliveryItemIds, DeliveryItemChangeLawUnitVM filterData)
        {
            bool result = service.SaveChangeLawUnit(deliveryItemIds, filterData);
            return Json(new
            {
                result = result
            });
        }

        [HttpPost]
        public JsonResult LoadForId_DDL(DeliveryItemTransFilterVM filterData)
        {
            var courts = service.DeliveryItemTransForIdDDL(filterData);
            return Json(courts);
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
        public JsonResult ListDataTrans(IDataTablesRequest request, DeliveryItemTransFilterVM filterData)
        {
            var data = service.DeliveryItemTransSelect(filterData, false);
            return Json(data.ToList());
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

            SetViewbag(-1);
            ModelState.Clear();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCaseNotificationEditReturn(notificationId).DeleteOrDisableLast();

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
            (bool saveResult, int notificationId) = await notificationService.DeliveryItemSaveReturn(model, returnFiles);
            if (saveResult)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                if (filterJson == null)
                {
                    return RedirectToAction("Edit", "CaseNotification", new { id = notificationId });
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
        public IActionResult EditStatePost(DeliveryItem model, string filterJson)
        {
            ViewBag.filterJson = filterJson;
            var notification = notificationService.GetById<CaseNotification>(model.CaseNotificationId);
            if (model.DeliveryDateCC < model.RegDate?.Date)
            {
                ModelState.AddModelError(nameof(model.DeliveryDateCC), $"{MessageConstant.ValidationErrors.DeliveryDateBeforeRegDate} {model.RegDate?.ToString(FormattingConstant.NormalDateFormat)}");
            }
            if (model.DeliveryDateCC > DateTime.Now.AddMinutes(10))
            {
                ModelState.AddModelError(nameof(model.DeliveryDateCC), MessageConstant.ValidationErrors.DeliveryDateFuture);
            }
            if (!ModelState.IsValid)
            {
                SetViewbagState(notification?.NotificationDeliveryGroupId ?? -1);
                return View(nameof(EditState), model);
            }

            if (service.DeliveryItemSaveState(model.Id, model.NotificationStateId, model.DeliveryDateCC, model.DeliveryInfo))
            {
                this.SaveLogOperation(false, model.Id);
                return RedirectToAction(nameof(EditState), new { id = model.Id, filterJson });
            }
            else
            {
                int filterType = getFilterTypeFromJson(filterJson);
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForDeliveryItemEditReturn(filterType, model.Id).DeleteOrDisableLast();
                SetViewbagState(notification?.NotificationDeliveryGroupId ?? -1);
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return View(nameof(EditState), model);
            }
        }
        #region Out/ResultList
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
        public IActionResult ExcelDataOut(string filterJson)
        {
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            DeliveryItemListVM model = JsonConvert.DeserializeObject<DeliveryItemListVM>(filterJson, dateTimeConverter);
            (var xlsBytes, var fileName) = service.GetDeliveryItemOutToExcel(model);
            return File(xlsBytes, System.Net.Mime.MediaTypeNames.Application.Rtf, fileName);
        }

        [HttpPost]
        public IActionResult ExcelDataResult(string filterJson)
        {
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            DeliveryItemListVM model = JsonConvert.DeserializeObject<DeliveryItemListVM>(filterJson, dateTimeConverter);
            (var xlsBytes, var fileName) = service.GetDeliveryItemReportResultToExcel(model);
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
        void SetViewbag(int courtId)
        {
            ViewBag.showButtons = true;
            ViewBag.FromCourtId_ddl = commonService.CourtForDelivery_SelectDDL(courtId);
            ViewBag.CourtId_ddl = ViewBag.FromCourtId_ddl;
            ViewBag.AddressTypesDDL = nomService.GetDropDownList<AddressType>();
            ViewBag.CountriesDDL = nomService.GetCountries();
            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(
                NomenclatureConstants.NotificationDeliveryGroup.WithSummons, -1
            );
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupSelect();
        }
        void SetViewbagState(int notificationDeliveryGroupId)
        {
            ViewBag.showButtons = true;
            ViewBag.FromCourtId_ddl = commonService.CourtForDelivery_SelectDDL(-1);
            ViewBag.CourtId_ddl = ViewBag.FromCourtId_ddl;
            ViewBag.AddressTypesDDL = nomService.GetDropDownList<AddressType>();
            ViewBag.CountriesDDL = nomService.GetCountries();
            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(
                notificationDeliveryGroupId, -1
            );
            ViewBag.NotificationDeliveryGroupId_ddl = service.NotificationDeliveryGroupSelect();
            SetHelpFile(HelpFileValues.Summons);
        }
        void SetViewbagArea(int forCourtId)
        {
            ViewBag.DeliveryAreaId_ddl = areaService.DeliveryAreaSelectDDL(forCourtId, false);
            ViewBag.LawUnitId_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, forCourtId);
        }
        void SetViewbagToCourt(DeliveryItemTransFilterVM filterData)
        {
            ViewBag.NotificationStateId_ddl = service.DeliveryItemTransNotificationState(filterData.ToNotificationStateId).AsQueryable().ToSelectList(true);
            ViewBag.ForId_ddl = service.DeliveryItemTransForIdDDL(filterData);
        }
        void SetViewbagChangeLawUnit()
        {
            int forCourtId = userContext.CourtId;

            ViewBag.NewLawUnitType_ddl = service.SelectNewLawUnitType();
            var lawUnits = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.MessageDeliverer, forCourtId, true);
            ViewBag.NewLawUnitId_ddl = areaService.RemoveSelectAddNoChange(lawUnits);
            ViewBag.LawUnitId_ddl = service.LawUnitForCourt_SelectDdlAllInDeliveryItem(forCourtId, lawUnits);
            ViewBag.NewCourtId_ddl = commonService.CourtForDelivery_SelectDDL(-1);
            ViewBag.NotificationStateId_ddl = nomService.GetDDL_NotificationStateFromDeliveryGroup(
                NomenclatureConstants.NotificationDeliveryGroup.WithSummons,
                NomenclatureConstants.NotificationState.Visited
            );
            ViewBag.DeliveryAreaId_ddl = areaService.DeliveryAreaSelectDDL(forCourtId, true);
            ViewBag.NewDeliveryAreaId_ddl = areaService.RemoveSelectAddNoChange(areaService.DeliveryAreaSelectDDL(forCourtId, false));
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

        public IActionResult ToCaseNotification(int Id)
        {
            var deliveryItem = service.GetById<DeliveryItem>(Id);
            return RedirectToAction("Edit", "CaseNotification", new { id = deliveryItem.CaseNotificationId });
        }
        [HttpPost]
        public IActionResult  DeliveryItem_ExpiredInfo(ExpiredInfoVM model)
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
    }
}