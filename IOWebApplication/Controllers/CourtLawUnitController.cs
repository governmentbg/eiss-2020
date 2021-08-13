using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOWebApplication.Controllers
{
    public class CourtLawUnitController : BaseController
    {
        private readonly ICourtLawUnitService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICourtGroupService courtGroupService;
        private readonly ICourtOrganizationService courtOrganizationService;

        public CourtLawUnitController(ICourtLawUnitService _service, INomenclatureService _nomService,
                  ICourtGroupService _courtGroupService, ICourtOrganizationService _courtOrganizationService,
                  ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            courtGroupService = _courtGroupService;
            courtOrganizationService = _courtOrganizationService;
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
        /// Страница със служители към съд
        /// </summary>
        /// <param name="periodType"></param>
        /// <param name="lawUnitType"></param>
        /// <returns></returns>
        public IActionResult Index(int periodType, int lawUnitType)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnit(periodType, lawUnitType).DeleteOrDisableLast();

            PeriodType period = service.GetById<PeriodType>(periodType);
            ViewBag.periodTypeId = periodType;
            ViewBag.lawUnitTypeId = lawUnitType;
            ViewBag.lawUnitTypeName = service.GetById<LawUnitType>(lawUnitType).Description;
            ViewBag.periodName = period.Label;
            SetHelpByLawUnitType(lawUnitType);

            return View();
        }

        /// <summary>
        /// Извличане на данни за служители към съд
        /// </summary>
        /// <param name="request"></param>
        /// <param name="periodType"></param>
        /// <param name="lawUnitType"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int periodType, int lawUnitType)
        {
            var data = service.CourtLawUnit_Select(userContext.CourtId, periodType, lawUnitType);

            return request.GetResponse(data);
        }

        public void SetBreadcrums(int periodTypeId, int lawUnitTypeId, int id)
        {
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnitEdit(periodTypeId, lawUnitTypeId, id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnitAdd(periodTypeId, lawUnitTypeId).DeleteOrDisableLast();

            SetHelpByLawUnitType(lawUnitTypeId);
        }

        /// <summary>
        /// Добавяне на служител към съд
        /// </summary>
        /// <param name="periodType"></param>
        /// <param name="lawUnitType"></param>
        /// <returns></returns>
        public IActionResult Add(int periodType, int lawUnitType)
        {
            SetBreadcrums(periodType, lawUnitType, 0);
            var model = new CourtLawUnit()
            {
                CourtId = userContext.CourtId,
                PeriodTypeId = periodType,
                MasterLawUnitTypeId = lawUnitType,
                DateFrom = DateTime.Now
            };
            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на служител в съд
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<CourtLawUnit>(id);
            model.MasterLawUnitTypeId = service.GetById<LawUnit>(model.LawUnitId).LawUnitTypeId;
            SetBreadcrums(model.PeriodTypeId, model.MasterLawUnitTypeId, model.Id);

            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        void SetViewBag(CourtLawUnit model)
        {
            ViewBag.lawUnitTypeName = service.GetById<LawUnitType>(model.MasterLawUnitTypeId).Label;
            ViewBag.periodTypeName = service.GetById<PeriodType>(model.PeriodTypeId).Label;
            if (NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(model.PeriodTypeId))
            {
                ViewBag.CourtOrganizationId_ddl = courtOrganizationService.CourtOrganization_SelectForDropDownList(userContext.CourtId);
                ViewBag.LawUnitPositionId_ddl = nomService.GetDDL_LawUnitPosition(model.MasterLawUnitTypeId);
            }
            if (model.PeriodTypeId == NomenclatureConstants.PeriodTypes.ActAs)
            {
                ViewBag.LawUnitTypeId_ddl = nomService.GetList<LawUnitType>()
                                    .Where(x => NomenclatureConstants.LawUnitTypes.CanActAsPersons.Contains(x.Id))
                                    .OrderBy(x => x.OrderNumber)
                                    .ToSelectList(x => x.Id, x => x.Label);
            }
        }

        /// <summary>
        /// Запис на служител в съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CourtLawUnit model)
        {
            if (model.LawUnitId <= 0)
            {
                ModelState.AddModelError(nameof(CourtLawUnit.LawUnitId), "Изберете служител.");
            }
            if (model.DateTo != null && model.DateTo.ForceStartDate() < model.DateFrom.ForceStartDate())
            {
                ModelState.AddModelError(nameof(CourtLawUnit.DateTo), "Дата до не може да е преди Дата от");
            }

            model.DateFrom = model.DateFrom.ForceStartDate();
            model.DateTo = model.DateTo.ForceEndDate();
            if (!ModelState.IsValid)
            {
                SetBreadcrums(model.PeriodTypeId, model.MasterLawUnitTypeId, model.Id);
                SetViewBag(model);
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            (bool result, string errorMessage) = service.CourtLawUnit_SaveData(model);
            if (result)
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = MessageConstant.Values.SaveFailed;
                SetErrorMessage(errorMessage);
            }
            SetBreadcrums(model.PeriodTypeId, model.MasterLawUnitTypeId, model.Id);
            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult CourtLawUnit_ExpiredInfo(ExpiredInfoVM model)
        {
            var expireObject = service.GetById<CourtLawUnit>(model.Id);
            if (service.SaveExpireInfo<CourtLawUnit>(model))
            {
                SetSuccessMessage(MessageConstant.Values.CourtLawUnitExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Index", "CourtLawUnit", new { periodType = expireObject.PeriodTypeId, lawUnitType = model.OtherId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        /// <summary>
        /// Закачване на групи към служител
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditCourtLawUnitGroup(int id)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnitGroup(id).DeleteOrDisableLast();

            var model = service.GetCourtLawUnitById(id);
            SetViewbagEditCourtLawUnitGroup();

            return View(nameof(EditCourtLawUnitGroup), model);
        }

        /// <summary>
        /// Запис на групи към служител
        /// </summary>
        /// <param name="model"></param>
        /// <param name="groupCodesJson"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditCourtLawUnitGroup(CourtLawUnitGroupVM model, string groupCodesJson)
        {
            SetViewbagEditCourtLawUnitGroup();

            if (!ModelState.IsValid)
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnitGroup(model.CourtLawUnitId).DeleteOrDisableLast();
                return View(nameof(EditCourtLawUnitGroup), model);
            }
            List<MultiSelectTransferPercentVM> codeGroups = JsonConvert.DeserializeObject<List<MultiSelectTransferPercentVM>>(groupCodesJson);
            if (service.CourtLawUnitGroup_SaveData(userContext.CourtId, model.LawUnitId, codeGroups))
            {
                //this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                SaveLogOperation(IO.LogOperation.Models.OperationTypes.Patch, $"{userContext.CourtId}|{model.LawUnitId}");
                return RedirectToAction(nameof(EditCourtLawUnitGroup), new { id = model.CourtLawUnitId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnitGroup(model.CourtLawUnitId).DeleteOrDisableLast();
            return View(nameof(EditCourtLawUnitGroup), model);
        }

        void SetViewbagEditCourtLawUnitGroup()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            SetHelpFile(HelpFileValues.Nom1);
        }

        /// <summary>
        /// Избрани групи за служител
        /// </summary>
        /// <param name="lawUnitId"></param>
        /// <returns></returns>
        public JsonResult CourtLawUnitGroupRightList(int lawUnitId)
        {
            var data = service.CourtLawUnitGroup_Select(userContext.CourtId, lawUnitId);
            return Json(data);
        }

        /// <summary>
        /// Групи за избор за служител
        /// </summary>
        /// <param name="caseGroupId"></param>
        /// <returns></returns>
        public JsonResult CourtLawUnitGroupLeftList(int caseGroupId)
        {
            var data = courtGroupService.CourtGroupForSelect_Select(userContext.CourtId, caseGroupId);
            return Json(data);
        }

        /// <summary>
        /// Страница с Длъжностни лица
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexSpr()
        {
            ViewBag.PeriodTypeId_ddl = nomService.GetDropDownList<PeriodType>();
            var model = new CourtLawUnitFilter()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };
            SetHelpFile(HelpFileValues.Report38);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за Длъжностни лица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="LawUnitId"></param>
        /// <param name="PeriodTypeId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataSpr(IDataTablesRequest request, int LawUnitId, int PeriodTypeId, DateTime? DateFrom, DateTime? DateTo)
        {
            var data = service.CourtLawUnitSpr_Select(LawUnitId, PeriodTypeId, DateFrom ?? NomenclatureExtensions.GetStartYear(), DateTo ?? NomenclatureExtensions.GetEndYear());
            return request.GetResponse(data);
        }


        public IActionResult OrderIndex(bool actualize = false)
        {
            if (actualize)
            {
                if (service.CourtLawUnitOrder_Actualize(userContext.CourtId))
                {
                    SetSuccessMessage("Данните за съдиите са актуализирани успешно");
                }
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnit(NomenclatureConstants.PeriodTypes.Appoint, NomenclatureConstants.LawUnitTypes.Judge);
            SetHelpFile(HelpFileValues.Nom1);

            return View();
        }

        [HttpPost]
        public IActionResult ListData_OrderIndex(IDataTablesRequest request)
        {
            var data = service.CourtLawUnitOrder_Select(userContext.CourtId);
            return request.GetResponse(data);
        }

        public IActionResult OrderChange(int id, bool moveUp)
        {
            var model = service.GetById<CourtLawUnitOrder>(id);
            Func<CourtLawUnitOrder, int?> orderProp = x => x.OrderNumber;
            Expression<Func<CourtLawUnitOrder, int?>> setterProp = (x) => x.OrderNumber;
            var result = service.ChangeOrder<CourtLawUnitOrder>(id, moveUp, orderProp, setterProp, x => x.CourtId == userContext.CourtId);

            return Json(new { result = result });
        }


        /// <summary>
        /// Заместване на съдии
        /// </summary>
        /// <returns></returns>
        public IActionResult Substitution()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForLawUnit(NomenclatureConstants.LawUnitTypes.Judge);
            SetHelpFile(HelpFileValues.Nom1);
            return View();
        }

        [HttpPost]
        public IActionResult ListData_Substitution(IDataTablesRequest request, CourtLawUnitSubstitutionFilter filter)
        {
            var data = service.CourtLawUnitSubstitution_Select(filter);
            return request.GetResponse(data);
        }

        private void SubstitutionSetViewBag()
        {
            SetHelpFile(HelpFileValues.Nom1);
        }

        public IActionResult Substitution_Add()
        {
            SubstitutionSetViewBag();
            var model = new CourtLawUnitSubstitution();
            return View(nameof(Substitution_Edit), model);
        }
        public IActionResult Substitution_Edit(int id)
        {
            var model = service.GetById<CourtLawUnitSubstitution>(id);
            if (model.CourtId != userContext.CourtId)
            {
                return Redirect_Denied();
            }
            SubstitutionSetViewBag();

            return View(nameof(Substitution_Edit), model);
        }

        [HttpPost]
        public IActionResult Substitution_Edit(CourtLawUnitSubstitution model)
        {
            var error = service.CourtLawUnitSubstitution_Validate(model);
            if (!string.IsNullOrEmpty(error))
            {
                ModelState.AddModelError("", error);
            }
            if (!ModelState.IsValid)
            {
                SubstitutionSetViewBag();
                return View(nameof(Substitution_Edit), model);
            }

            int currentId = model.Id;
            if (service.CourtLawUnitSubstitution_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Substitution_Edit), new { id = model.Id });
            }
            SetErrorMessage(MessageConstant.Values.SaveFailed);
            SubstitutionSetViewBag();

            return View(nameof(Substitution_Edit), model);
        }
    }
}