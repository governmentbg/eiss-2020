using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOWebApplication.Controllers
{
    public class CourtGroupController : BaseController
    {
        private readonly ICourtGroupService service;
        private readonly ICourtGroupCodeService codeService;
        private readonly INomenclatureService nomService;
        private readonly ICourtGroupLawUnitService serviceGroupLawUnit;
        private readonly ICommonService commonService;
        public CourtGroupController(ICourtGroupService _service, ICourtGroupCodeService _codeService, INomenclatureService _nomService, ICourtGroupLawUnitService _serviceGroupLawUnit, ICommonService _commonService)
        {
            service = _service;
            codeService = _codeService;
            nomService = _nomService;
            serviceGroupLawUnit = _serviceGroupLawUnit;
            commonService = _commonService;
        }

        /// <summary>
        /// Страница с групи шифри
        /// </summary>
        /// <param name="filterCaseGroupId"></param>
        /// <returns></returns>
        [TitleAudit(Operation = AuditConstants.Operations.List)]
        public IActionResult Index(int filterCaseGroupId, int groupKind = NomenclatureConstants.CourtGroupKinds.JudgeSelection)
        {
            SetViewbag(filterCaseGroupId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtGroups(filterCaseGroupId).DeleteOrDisableLast();
            ViewBag.groupKind = groupKind;
            return View();
        }

        /// <summary>
        /// Извличане на данни за групи шифри
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterCaseGroupId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int filterCaseGroupId, int groupKind)
        {
            var data = service.CourtGroup_Select(userContext.CourtId, filterCaseGroupId, groupKind);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Извличане на данни за История съдии към група
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListHistoryData(IDataTablesRequest request, int id)
        {
            var data = serviceGroupLawUnit.CourtGroup_LawUnitsHistory_Select(id);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на група шифри
        /// </summary>
        /// <param name="filterCaseGroupId"></param>
        /// <returns></returns>
        [DisableAudit]
        public IActionResult Add(int filterCaseGroupId, int groupKind)
        {
            var model = new CourtGroup()
            {
                CourtId = userContext.CourtId,
                CaseGroupId = filterCaseGroupId,
                GroupKind = groupKind
            };
            SetViewbag(filterCaseGroupId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtGroupAdd(filterCaseGroupId).DeleteOrDisableLast();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на група шифри
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filterCaseGroupId"></param>
        /// <returns></returns>
        public IActionResult Edit(int id, int filterCaseGroupId)
        {
            var model = service.GetById<CourtGroup>(id);
            SetViewbag(filterCaseGroupId);
            auditInfo(AuditConstants.Operations.View, model);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForCourtGroupEdit(filterCaseGroupId, id).DeleteOrDisableLast();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на група шифри
        /// </summary>
        /// <param name="model"></param>
        /// <param name="filterCaseGroupId"></param>
        /// <param name="caseCodesJson"></param>
        /// <returns></returns>
        [DisableAudit]
        [HttpPost]
        public async Task<IActionResult> Edit(CourtGroup model, int filterCaseGroupId, string caseCodesJson)
        {
            SetViewbag(filterCaseGroupId);
            List<int> caseCodes = new List<int>();
            string errCodes = "";
            try
            {
                caseCodes = JsonConvert.DeserializeObject<List<int>>(caseCodesJson);
            }
            catch (Exception ex)
            {
                errCodes = "Проблем със списъка с кодове" + ex.Message;
                ModelState.AddModelError("caseCodesJson", errCodes);
            }


            if ((!ModelState.IsValid) || (errCodes != ""))
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;

            if (service.CourtGroup_SaveData(model))
            {
                await codeService.CourtGroupCode_SaveData(model.Id, caseCodes);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                auditInfo(currentId == 0 ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return View(nameof(Edit), model);
            }

        }

        void auditInfo(string operation, CourtGroup model, string add = "")
        {
            var cGroup = service.GetNomCodeById<CaseGroup>(model.CaseGroupId);
            AddAuditInfo(operation, $"{model.Label} ({cGroup})", add, "Група шифри");
        }

        void SetViewbag(int filterCaseGroupId)
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.filterCaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.CaseTypeId_ddl = nomService.GetDDL_CaseTypeForCourt(userContext.CourtId);
            ViewBag.filterCaseGroupId = filterCaseGroupId;
            ViewBag.caseTypeJson = JsonConvert.SerializeObject(nomService.CaseTypeNow(userContext.CourtId).Select(x => new { x.Id, x.Label, x.CaseGroupId }).ToList());
            SetHelpFile(HelpFileValues.Nom17);
        }

        /// <summary>
        /// Избрани група шифри
        /// </summary>
        /// <param name="courtGroupId"></param>
        /// <param name="caseGroupId"></param>
        /// <returns></returns>
        public JsonResult CourtGroupCodeRightList(int courtGroupId, int caseGroupId)
        {
            var data = codeService.CourtGroupCode_Select(userContext.CourtId, courtGroupId, caseGroupId);
            return Json(data);
        }

        /// <summary>
        /// Списък за ибор на групи шифри
        /// </summary>
        /// <param name="caseGroupId"></param>
        /// <param name="caseTypeId"></param>
        /// <returns></returns>
        public JsonResult CourtGroupCodeLeftList(int caseGroupId, int caseTypeId)
        {
            var data = codeService.CourtGroupCodeForSelect_Select(userContext.CourtId, caseGroupId, caseTypeId);
            return Json(data);
        }
        public JsonResult CourtGroupCodeAllList(int courtGroupId, int caseGroupId, int caseTypeId)
        {
            var dataRight = codeService.CourtGroupCode_Select(userContext.CourtId, courtGroupId, caseGroupId);
            var dataLeft = codeService.CourtGroupCodeForSelect_Select(userContext.CourtId, caseGroupId, caseTypeId);
            return Json(new { dataRight = dataRight, dataLeft = dataLeft });
        }

        /// <summary>
        /// Промяна на подредбата на групите
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangeOrder(ChangeOrderModel model)
        {
            var courtGroup = service.GetById<CourtGroup>(model.Id);
            Func<CourtGroup, int?> orderProp = x => x.OrderNumber;
            Expression<Func<CourtGroup, int?>> setterProp = x => x.OrderNumber;
            Expression<Func<CourtGroup, bool>> predicate = x => x.CourtId == courtGroup.CourtId;
            bool result = service.ChangeOrder(model.Id, model.Direction == "up", x => x.OrderNumber, setterProp, predicate);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Проблем при смяна на реда");
            }

            return Ok();
        }

        /// <summary>
        /// Съдии към група
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filterCaseGroupId"></param>
        /// <returns></returns>
        [DisableAudit]
        public IActionResult EditCourtGroupLawUnit(int id, int filterCaseGroupId)
        {
            ViewBag.filterCaseGroupId = filterCaseGroupId;
            var model = service.GetCourtGroupVMById(id);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForEditCourtGroupLawUnit(filterCaseGroupId).DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Nom17);
            return View(nameof(EditCourtGroupLawUnit), model);
        }
        /// <summary>
        /// История съдии към група
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filterCaseGroupId"></param>
        /// <returns></returns>
        public IActionResult HistoryCourtGroupLawUnit(int id, int filterCaseGroupId)
        {
            ViewBag.filterCaseGroupId = filterCaseGroupId;
            var model = service.GetCourtGroupVMById(id);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_ForEditCourtGroupLawUnit(filterCaseGroupId).DeleteOrDisableLast();
            return View(model);
        }

        /// <summary>
        /// Запис на съдии към група
        /// </summary>
        /// <param name="model"></param>
        /// <param name="groupJudgeJson"></param>
        /// <param name="filterCaseGroupId"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableAudit]
        public async Task<IActionResult> EditCourtGroupLawUnit(CourtGroupVM model, string groupJudgeJson, int filterCaseGroupId)
        {
            if (!ModelState.IsValid)
            {
                SetHelpFile(HelpFileValues.Nom17);
                return View(nameof(EditCourtGroupLawUnit), model);
            }
            ViewBag.filterCaseGroupId = filterCaseGroupId;
            model = service.GetCourtGroupVMById(model.Id);
            List<MultiSelectTransferPercentVM> judge_codes;
            try
            {
                judge_codes = JsonConvert.DeserializeObject<List<MultiSelectTransferPercentVM>>(groupJudgeJson);
            }
            catch (Exception ex)
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed + ex.Message);
                return View(nameof(EditCourtGroupLawUnit), model);
            }
            if (await serviceGroupLawUnit.CourtGroupLawUnitSaveData(userContext.CourtId, model.Id, judge_codes))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                SaveLogOperation(IO.LogOperation.Models.OperationTypes.Patch, model.Id);
                var cgroup = service.GetById<CourtGroup>(model.Id);
                auditInfo(AuditConstants.Operations.Patch, cgroup, "Промяна съдии към група");
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            return RedirectToAction(nameof(EditCourtGroupLawUnit), new { id = model.Id, filterCaseGroupId = filterCaseGroupId });
            //return View(nameof(EditCourtGroupLawUnit), model);
        }

        /// <summary>
        /// Списък със записани съдии към група
        /// </summary>
        /// <param name="courtGroupId"></param>
        /// <returns></returns>
        public JsonResult CourtGroupLawUnitRightList(int courtGroupId, int groupKind)
        {
            var data = serviceGroupLawUnit.CourtGroupLawUnitSaved(userContext.CourtId, courtGroupId, groupKind);
            return Json(data);
        }

        /// <summary>
        /// Списък със съдии за добавяне към група
        /// </summary>
        /// <returns></returns>
        public JsonResult CourtGroupLawUnitLeftList(int groupKind)
        {
            var data = serviceGroupLawUnit.CourtGroupLawUnitForSelect(userContext.CourtId, groupKind);
            return Json(data);
        }

        /// <summary>
        /// Групи за DropDownList
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="CaseCodeId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CourtGroup_SelectForDropDownList(int courtId, int CaseCodeId)
        {
            var model = service.CourtGroup_SelectForDropDownList(courtId, CaseCodeId).SingleOrChoose();
            return Json(model);
        }

        #region CourtGroup



        #endregion
    }
}