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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CourtLawUnitController : BaseController
    {
        private readonly ICourtLawUnitService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICourtGroupService courtGroupService;
        private readonly ICourtOrganizationService courtOrganizationService;

        public CourtLawUnitController(ICourtLawUnitService _service, 
                                      INomenclatureService _nomService,
                                      ICourtGroupService _courtGroupService, 
                                      ICourtOrganizationService _courtOrganizationService,
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
        public async Task<IActionResult> Index(int periodType, int lawUnitType)
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnit(periodType, lawUnitType).DeleteOrDisableLast();

            ViewBag.periodTypeId = periodType;
            ViewBag.periodName = await service.GetPropByIdAsync<PeriodType, string>(x => x.Id == periodType, x => x.Label);
            ViewBag.lawUnitTypeId = lawUnitType;
            ViewBag.lawUnitTypeName = await service.GetPropByIdAsync<LawUnitType, string>(x => x.Id == lawUnitType, x => x.Description);
            addToAudit(AuditConstants.Operations.List, new CourtLawUnit() { PeriodTypeId = periodType, MasterLawUnitTypeId = lawUnitType });
            SetHelpByLawUnitType(lawUnitType);

            return View(new CourtLawUnitFilter());
        }

        /// <summary>
        /// Извличане на данни за служители към съд
        /// </summary>
        /// <param name="request"></param>       
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, CourtLawUnitFilter filter)
        {
            var data = service.CourtLawUnit_Select(userContext.CourtId, filter);

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
            addToAudit(AuditConstants.Operations.View, model);
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
            addToAudit(AuditConstants.Operations.View, model);
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
                if (currentId == 0)
                {
                    addToAudit(AuditConstants.Operations.Append, model);
                }
                else
                {
                    addToAudit(AuditConstants.Operations.Update, model);
                }
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

        void addToAudit(string operation, CourtLawUnit model)
        {
            var periodType = commonService.GetPropById<PeriodType, string>(x => x.Id == model.PeriodTypeId, x => x.Label);
            var lawunitType = commonService.GetPropById<LawUnitType, string>(x => x.Id == model.MasterLawUnitTypeId, x => x.Label);
            var baseInfo = string.Empty;
            var addInfo = string.Empty;
            var operationType = $"{lawunitType} : {periodType}";
            if (model.LawUnitId > 0)
            {
                var luName = commonService.GetPropById<LawUnit, string>(x => x.Id == model.LawUnitId, x => x.FullName);
                baseInfo = luName;
                addInfo = $"от {model.DateFrom:dd.MM.yyyy} до {model.DateTo:dd.MM.yyyy}";
            }

            AddAuditInfo(operation, baseInfo, addInfo, operationType);
        }

        void addToAuditSubstitution(string operation, CourtLawUnitSubstitution model)
        {
            var baseInfo = string.Empty;
            var addInfo = string.Empty;
            var operationType = $"Заместване на съдия";
            if (model.LawUnitId > 0 && model.SubstituteLawUnitId > 0)
            {
                baseInfo = commonService.GetPropById<LawUnit, string>(x => x.Id == model.LawUnitId, x => x.FullName);
                var subtName = commonService.GetPropById<LawUnit, string>(x => x.Id == model.SubstituteLawUnitId, x => x.FullName);

                addInfo = $"Заместник: {subtName} от {model.DateFrom:dd.MM.yyyy} до {model.DateTo:dd.MM.yyyy}";
            }

            AddAuditInfo(operation, baseInfo, addInfo, operationType);
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

        #region CourtLawUnitAssistant

        /// <summary>
        /// Извличане на данни за служители към съд
        /// </summary>
        /// <param name="request"></param>       
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCourtLawUnitAssistant(IDataTablesRequest request, CourtLawUnitAssistantFilterViewModel filter)
        {
            var data = service.CourtLawUnitAssistant_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Конфигуриране на breadcrumbs за асистент/помощник/секретар
        /// </summary>
        /// <param name="courtLawUnitId">Идентификатор на CourtLawUnit</param>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task SetBreadcrumsCourtLawUnitAssistant(int courtLawUnitId, int id)
        {
            var model = await service.GetByIdAsync<CourtLawUnit>(courtLawUnitId);
            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnitAssistantEdit(model.PeriodTypeId, model.LawUnitTypeId ?? 0, courtLawUnitId, id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnitAssistantAdd(model.PeriodTypeId, model.LawUnitTypeId ?? 0, courtLawUnitId).DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне на асистент/помощник/секретар
        /// </summary>
        /// <param name="courtLawUnitId">Идентификатор на CourtLawUnit</param>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <returns></returns>
        public async Task<IActionResult> AddCourtLawUnitAssistant(int courtLawUnitId, int courtId)
        {
            CurrentContext_SetObjectInfo("Добавяне на съдебен помощник/секретар/деловодител");
            await SetBreadcrumsCourtLawUnitAssistant(courtLawUnitId, 0);
            var model = new CourtLawUnitAssistantEditViewModel()
            {
                CourtLawUnitId = courtLawUnitId,
                CourtId = courtId
            };

            await SetViewBagCourtLawUnitAssistant();
            return View(nameof(EditCourtLawUnitAssistant), model);
        }

        /// <summary>
        /// Редакция на асистент/помощник/секретар
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCourtLawUnitAssistant(int id)
        {
            CurrentContext_SetObjectInfo("Зареждане на данни за редакция на съдебен помощник/секретар/деловодител");
            CourtLawUnitAssistantEditViewModel model = await service.GetCourtLawUnitAssistantById(id);
            await SetBreadcrumsCourtLawUnitAssistant(model.CourtLawUnitId, id);
            await SetViewBagCourtLawUnitAssistant();
            return View(nameof(EditCourtLawUnitAssistant), model);
        }

        /// <summary>
        /// Запис на асистент/помощник/секретар
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditCourtLawUnitAssistant(CourtLawUnitAssistantEditViewModel model)
        {
            await SetBreadcrumsCourtLawUnitAssistant(model.CourtLawUnitId, model.Id);
            await SetViewBagCourtLawUnitAssistant();

            if (!ModelState.IsValid)
                return View(nameof(EditCourtLawUnitAssistant), model);

            string _isvalid = await IsValidCourtLawUnitAssistant(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCourtLawUnitAssistant), model);
            }

            bool result = await service.CourtLawUnitAssistant_SaveData(model);
            if (result)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCourtLawUnitAssistant), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditCourtLawUnitAssistant), model);
        }

        /// <summary>
        /// Метод валидиращ данните за запис
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<string> IsValidCourtLawUnitAssistant(CourtLawUnitAssistantEditViewModel model)
        {
            if (model.LawUnitId < 1)
                return "Изберете секретар";

            if (model.JudgeRoleId < 1)
                return "Изберете роля";

            if (await service.IsExistsCourtLawUnitAssistant(model.CourtLawUnitId, model.LawUnitId, model.Id))
                return "Този секретар е добавен";

            return string.Empty;
        }

        /// <summary>
        /// Зареждане на списъци за добавяне редкация на асистент/помощник/секретар
        /// </summary>
        /// <returns></returns>
        private async Task SetViewBagCourtLawUnitAssistant()
        {
            ViewBag.JudgeRoleId_ddl = (await nomService.GetDropDownListAsync<JudgeRole>()).Where(x => NomenclatureConstants.JudgeRole.ManualRoles.Contains(int.Parse(x.Value)) || x.Value == "-1").ToList();
        }

        /// <summary>
        /// Метод сторниращ секретар към заседание
        /// </summary>
        /// <param name="model">Модел попълнен от потребител за сторно на запис</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CourtLawUnitAssistant_ExpiredInfo(ExpiredInfoVM model)
        {
            if (await service.CourtLawUnitAssistantExpired(model.Id))
            {
                SetSuccessMessage(MessageConstant.Values.CourtLawUnitExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Edit", "CourtLawUnit", new { id = model.OtherId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        #endregion

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
            addToAudit(AuditConstants.Operations.View, model, 0);
            return View(nameof(EditCourtLawUnitGroup), model);
        }

        /// <summary>
        /// Запис на групи към служител
        /// </summary>
        /// <param name="model"></param>
        /// <param name="groupCodesJson"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditCourtLawUnitGroup(CourtLawUnitGroupVM model, string groupCodesJson)
        {
            SetViewbagEditCourtLawUnitGroup();

            if (!ModelState.IsValid)
            {
                ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtLawUnitGroup(model.CourtLawUnitId).DeleteOrDisableLast();
                return View(nameof(EditCourtLawUnitGroup), model);
            }

            //var serializeOptions = new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //    WriteIndented = true
            //};
            List<MultiSelectTransferPercentVM> codeGroups = JsonTextSerializer.Deserialize<List<MultiSelectTransferPercentVM>>(groupCodesJson);
            if (await service.CourtLawUnitGroup_SaveData(userContext.CourtId, model.LawUnitId, codeGroups))
            {
                //this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                SaveLogOperation(IO.LogOperation.Models.OperationTypes.Patch, $"{userContext.CourtId}|{model.LawUnitId}");
                addToAudit(AuditConstants.Operations.Update, model, 0);
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

        void addToAudit(string operation, CourtLawUnitGroupVM model, int groupCount)
        {
            var baseInfo = string.Empty;
            var addInfo = string.Empty;
            var operationType = $"Групи към съдия";

            baseInfo = model.LawUnitName;
            addInfo = $"Брой групи: {groupCount}";


            AddAuditInfo(operation, baseInfo, addInfo, operationType);
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
        [TitleAudit(Operation = Infrastructure.Constants.AuditConstants.Operations.List)]
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
            AddAuditInfo(AuditConstants.Operations.List, "Ред на старшинство", "", "Съдия : Назначаване");
            return View();
        }

        [HttpPost]
        public IActionResult ListData_OrderIndex(IDataTablesRequest request)
        {
            var data = service.CourtLawUnitOrder_Select(userContext.CourtId);
            return request.GetResponse(data);
        }

        public IActionResult OrderChangeCombo(int currentRowNo)
        {
            var data = service.CourtLawUnitOrder_Select(userContext.CourtId);
            var model = new CourtLawunitOrderComboVM()
            {
                CurrentRowNo = currentRowNo,
                CurrentLawunit = data.Where(x => x.RowNo == currentRowNo).Select(x => $"{x.RowNo}. {x.LawUnitName}").FirstOrDefault()
            };
            ViewBag.NewRowNo_ddl = new SelectList(data
                                                    .Where(x => x.RowNo != currentRowNo)
                                                    .Select(x => new
                                                    {
                                                        Value = x.RowNo,
                                                        Text = $"{x.RowNo}. {x.LawUnitName}"
                                                    })
                                , "Value", "Text").AddAllItem().ToList();
            return PartialView(model);
        }

        [HttpPost]
        public IActionResult OrderChangeCombo(CourtLawunitOrderComboVM model)
        {
            var result = service.CourtLawUnitOrder_ComboSave(model);
            return Json(result);
        }

        public IActionResult OrderChange(int id, bool moveUp)
        {
            var model = service.GetById<CourtLawUnitOrder>(id);
            Func<CourtLawUnitOrder, int?> orderProp = x => x.OrderNumber;
            Expression<Func<CourtLawUnitOrder, int?>> setterProp = (x) => x.OrderNumber;
            var result = service.ChangeOrder<CourtLawUnitOrder>(id, moveUp, orderProp, setterProp, x => x.CourtId == userContext.CourtId);

            var fullName = service.GetPropById<LawUnit, string>(x => x.Id == model.LawUnitId, x => x.FullName);
            var dir = (moveUp) ? "нагоре" : "надолу";
            AddAuditInfo(AuditConstants.Operations.Update, "Ред на старшинство", $"{fullName} - {dir}", "Съдия : Назначаване");

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
            addToAuditSubstitution(AuditConstants.Operations.List, new CourtLawUnitSubstitution());
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
            addToAuditSubstitution(AuditConstants.Operations.View, model);
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
            addToAuditSubstitution(AuditConstants.Operations.View, model);

            return View(nameof(Substitution_Edit), model);
        }

        [HttpPost]
        public async Task<IActionResult> Substitution_Edit(CourtLawUnitSubstitution model)
        {
            var error = await service.CourtLawUnitSubstitution_Validate(model);
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
            if (await service.CourtLawUnitSubstitution_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                if (currentId == 0)
                {
                    addToAuditSubstitution(AuditConstants.Operations.Append, model);
                }
                else
                {
                    addToAuditSubstitution(AuditConstants.Operations.Update, model);
                }
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Substitution_Edit), new { id = model.Id });
            }
            SetErrorMessage(MessageConstant.Values.SaveFailed);
            SubstitutionSetViewBag();

            return View(nameof(Substitution_Edit), model);
        }

        #region Група Централизирано разпределение ГД

        /// <summary>
        /// Зареждане на листове за преглед на данни за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <returns></returns>
        async Task SetViewbagIndexCentralDistributionCC()
        {
            ViewBag.CourtId_ddl = await commonService.GetDDL_Court(NomenclatureConstants.CourtType.RegionalCourt);
        }

        /// <summary>
        /// Зарежда страница с данни за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="courtGroupKind">Kind на група</param>
        /// <returns></returns>
        public async Task<IActionResult> IndexCentralDistributionCC(int courtGroupKind)
        {
            await SetViewbagIndexCentralDistributionCC();
            return View(courtGroupKind);
        }

        /// <summary>
        /// Зарежда страница с данни за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="courtGroupKind">Kind на група</param>
        /// <returns></returns>
        public async Task<IActionResult> IndexCentralDistributionCCWithoutEdit(int courtGroupKind)
        {
            await SetViewbagIndexCentralDistributionCC();
            return View(courtGroupKind);
        }

        /// <summary>
        /// Извличане на данни за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="request">IDataTablesRequest</param>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataCentralDistribution(IDataTablesRequest request, CourtLawUnitGroupCCFilterVM filter)
        {
            var data = service.GetDataCentralDistributionCC(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Зареждане на листове за добавяне/редакция на данни за служители в Група Централизирано разпределение ГД
        /// </summary>
        /// <returns></returns>
        private async Task SetViewBagCentralDistributionCC()
        {
            ViewBag.CourtId_ddl = await commonService.GetDDL_Court(NomenclatureConstants.CourtType.RegionalCourt);
        }

        /// <summary>
        /// Добавяне на служител в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="courtGroupKind">Kind на група</param>
        /// <returns></returns>
        public async Task<IActionResult> AddCentralDistributionCCLawUnitGroup(int courtGroupKind)
        {
            CurrentContext_SetObjectInfo("Добавяне на служител централизирана група");
            var model = new CourtLawUnitGroupCCEditVM()
            {
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now,
                CourtGroupKind = courtGroupKind,
                LoadIndex = 100,
                LoadIndexOld = 100,
            };

            await SetViewBagCentralDistributionCC();
            return View(nameof(EditCentralDistributionCCLawUnitGroup), model);
        }

        /// <summary>
        /// Редакция на служител в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditCentralDistributionCCLawUnitGroup(int id)
        {
            CurrentContext_SetObjectInfo("Редакция на служител централизирана група");
            CourtLawUnitGroupCCEditVM model = await service.GetCentralDistributionCCEditById(id);
            await SetViewBagCentralDistributionCC();
            ViewBag.desc = System.Web.HttpUtility.UrlDecode(model.LoadIndexDescription);
            return View(nameof(EditCentralDistributionCCLawUnitGroup), model);
        }

        /// <summary>
        /// Валидация на данни преди запис/редакция на служител в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private async Task<string> ValidateCentralDistributionCCLawUnitGroup(CourtLawUnitGroupCCEditVM model)
        {
            if (model.CourtId < 1)
                return "Иаберете съд";

            if (model.LawUnitId < 1)
                return "Изберете служител";

            if (await service.IsExistLawUnitCentralDistributionCC(model.LawUnitId, model.CourtGroupKind, model.Id < 1 ? null : model.Id))
                return "Този служител е вече добавен";

            if (model.DateTo != null)
            {
                if (string.IsNullOrEmpty(model.DateToDescription))
                    return "Попълнете пояснение за дата до";
            }

            if (model.LoadIndex <= 1)
                return "Въведете натовареност по-голяма от 0";

            if (model.LoadIndex > 100)
                return "Въведете натовареност по-малка или равна на 100";

            if (model.LoadIndex != model.LoadIndexOld)
            {
                if (string.IsNullOrEmpty(model.LoadIndexDescription))
                    return "Попълнете пояснение за промяна на натовареност";
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис на служител в Група Централизирано разпределение ГД
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditCentralDistributionCCLawUnitGroup(CourtLawUnitGroupCCEditVM model)
        {
            await SetViewBagCentralDistributionCC();

            if (!ModelState.IsValid)
            {
                return View(nameof(EditCentralDistributionCCLawUnitGroup), model);
            }

            string _isvalid = await ValidateCentralDistributionCCLawUnitGroup(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditCentralDistributionCCLawUnitGroup), model);
            }

            bool isInsert = model.Id < 1;
            int? saveId = await service.SavelCourtLawUnitGroupCentralDistributionCC(model);
            if (saveId != null)
            {
                SaveLogOperation(isInsert, saveId);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditCentralDistributionCCLawUnitGroup), new { id = saveId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(EditCentralDistributionCCLawUnitGroup), model);
        }

        /// <summary>
        /// Метод извличащ данни за назначени служители в съд
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="lawUnitId">Служител за редакция и да се провери дали го има в списъка, ако е с конфигурирана дата до</param>
        /// <returns></returns>
        public async Task<IActionResult> GetDDL_CommonCourtLawUnitCentralDistributionCC(int courtId, int? lawUnitId = null)
        {
            return Json(await service.GetDDL_CommonCourtLawUnitCentralDistributionCC(courtId, lawUnitId));
        }

        #endregion
    }
}