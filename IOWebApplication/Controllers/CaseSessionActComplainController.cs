// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using Microsoft.AspNetCore.Mvc;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Controllers
{
    public class CaseSessionActComplainController : BaseController
    {
        private readonly ICaseSessionActComplainService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICaseSessionDocService sessionDocService;
        private readonly ICasePersonService casePersonService;
        private readonly ICaseMigrationService caseMigrationService;
        private readonly ICaseLifecycleService caseLifecycleService;

        public CaseSessionActComplainController(ICaseSessionActComplainService _service,
                                                INomenclatureService _nomService,
                                                ICommonService _commonService,
                                                ICaseSessionDocService _sessionDocService,
                                                ICasePersonService _casePersonService,
                                                ICaseMigrationService _caseMigrationService,
                                                ICaseLifecycleService _caseLifecycleService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            sessionDocService = _sessionDocService;
            casePersonService = _casePersonService;
            caseMigrationService = _caseMigrationService;
            caseLifecycleService = _caseLifecycleService;
        }

        #region CaseSessionActComplain

        public IActionResult Index(int caseSessionActId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActComplain, null, AuditConstants.Operations.View, caseSessionActId))
            {
                return Redirect_Denied();
            }
            var caseSessionAct = service.GetById<CaseSessionAct>(caseSessionActId);
            ViewBag.caseSessionActId = caseSessionActId;
            ViewBag.IsAdd = ((caseSessionAct.ActStateId != NomenclatureConstants.SessionActState.Project) && (!string.IsNullOrEmpty(caseSessionAct.RegNumber)));
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(caseSessionAct.CaseSessionId);
            SetHelpFile(HelpFileValues.SessionAct);

            return View();
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseSessionActId)
        {
            var data = service.CaseSessionActComplain_Select(caseSessionActId);
            return request.GetResponse(data);
        }

        public IActionResult IndexSpr()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.ActResultId_ddl = nomService.GetDDL_ActResult();
            ViewBag.ActComplainIndexId_ddl = nomService.GetDDL_ActComplainIndexByCourtType();
            var filter = new CaseSessionActComplainFilterVM()
            {
                DateFrom = NomenclatureExtensions.GetStartYear(),
                DateTo = NomenclatureExtensions.GetEndYear(),
            };

            return View(filter);
        }

        [HttpPost]
        public IActionResult ListDataSpr(IDataTablesRequest request, DateTime? DateFrom, DateTime? DateTo, DateTime? DateFromActReturn, DateTime? DateToActReturn, DateTime? DateFromSendDocument, DateTime? DateToSendDocument, int CaseGroupId, int CaseTypeId, string CaseRegNumber, string ActRegNumber, int RegNumFrom, int RegNumTo, int ActComplainIndexId, int ActResultId, int JudgeReporterId)
        {
            var data = service.CaseSessionActComplainSpr_Select(DateFrom ?? NomenclatureExtensions.GetStartYear(), DateTo ?? NomenclatureExtensions.GetEndYear(), DateFromActReturn, DateToActReturn, DateFromSendDocument, DateToSendDocument, CaseGroupId, CaseTypeId, CaseRegNumber ?? string.Empty, ActRegNumber ?? string.Empty, RegNumFrom, RegNumTo, ActComplainIndexId, ActResultId, JudgeReporterId);
            return request.GetResponse(data);
        }

        public IActionResult Add(int caseSessionActId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActComplain, null, AuditConstants.Operations.Append, caseSessionActId))
            {
                return Redirect_Denied();
            }
            var caseSessionAct = service.GetById<CaseSessionAct>(caseSessionActId);
            SetViewbag(caseSessionActId);
            var model = new CaseSessionActComplain()
            {
                CourtId = caseSessionAct.CourtId,
                CaseId = caseSessionAct.CaseId,
                CaseSessionActId = caseSessionActId,
            };
            return View(nameof(Edit), model);
        }

        public IActionResult Edit(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActComplain, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.GetById<CaseSessionActComplain>(id);
            SetViewbag(model.CaseSessionActId);
            return View(nameof(Edit), model);
        }

        private string IsValid(CaseSessionActComplain model)
        {
            if (model.ComplainDocumentId < 1)
                return "Изберете съпровождащ документ";


            if (model.ComplainStateId < 1)
                return "Изберете статус";

            if (model.Id < 1)
            {
                if (service.IsExistComplain(model.CaseId ?? 0, model.ComplainDocumentId))
                    return "Има обжалване с този съпровождащ документ по това дело";
            }

            return string.Empty;
        }

        [HttpPost]
        public IActionResult Edit(CaseSessionActComplain model)
        {
            SetViewbag(model.CaseSessionActId);

            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CaseSessionActComplain_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionActComplain, model.Id, currentId == 0);
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

        void SetViewbag(int caseSessionActId)
        {
            var caseSessionAct = service.GetById<CaseSessionAct>(caseSessionActId);

            ViewBag.ComplainDocumentId_ddl = service.GetDropDownList_GetDocumentCaseInfo(caseSessionAct.CaseSessionId);
            ViewBag.ComplainStateId_ddl = nomService.GetDropDownList<ComplainState>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionActComplain(caseSessionActId);
            SetHelpFile(HelpFileValues.SessionAct);
        }

        [HttpPost]
        public IActionResult CaseSessionActComplain_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActComplain, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }
            var expireObject = service.GetById<CaseSessionActComplain>(model.Id);
            if (caseMigrationService.IsExistMigrationWithAct(expireObject.CaseSessionActId))
            {
                return Json(new { result = false, message = "Акта е изпратен в по-висша инстанция за обжалване." });
            }

            if (service.SaveExpireInfo<CaseSessionActComplain>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseSessionActComplain, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseSessionActComplainExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Index", "CaseSessionActComplain", new { caseSessionActId = expireObject.CaseSessionActId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        #endregion

        #region CaseSessionActComplainResult

        [HttpPost]
        public IActionResult ListDataResult(IDataTablesRequest request, int CaseSessionActComplainId)
        {
            var data = service.CaseSessionActComplainResult_Select(CaseSessionActComplainId);
            return request.GetResponse(data);
        }

        public IActionResult AddResult(int CaseSessionActComplainId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActComplainResult, null, AuditConstants.Operations.Append, CaseSessionActComplainId))
            {
                return Redirect_Denied();
            }
            var caseSessionActComplain = service.GetById<CaseSessionActComplain>(CaseSessionActComplainId);
            SetViewbagResult(CaseSessionActComplainId);
            var model = new CaseSessionActComplainResultEditVM()
            {
                CaseId = caseSessionActComplain.CaseId ?? 0,
                CourtId = caseSessionActComplain.CourtId ?? 0,
                CaseSessionActComplainId = CaseSessionActComplainId,
                DateResult = DateTime.Now,
                CaseSessionActComplains = service.GetCheckListCaseSessionActComplains(caseSessionActComplain.Id, caseSessionActComplain.CaseSessionActId),
                IsStartNewLifecycle = false
            };
            return View(nameof(EditResult), model);
        }

        public IActionResult EditResult(int id)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActComplainResult, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = service.CaseSessionActComplainResult_GetById(id);
            model.IsStartNewLifecycle = false;
            var caseSessionActComplain = service.GetById<CaseSessionActComplain>(model.CaseSessionActComplainId);
            model.CaseSessionActComplains = service.GetCheckListCaseSessionActComplains(caseSessionActComplain.Id, caseSessionActComplain.CaseSessionActId);
            SetViewbagResult(model.CaseSessionActComplainId);
            return View(nameof(EditResult), model);
        }

        private string IsValidResult(CaseSessionActComplainResultEditVM model)
        {
            if (model.ComplainCaseId < 1)
                return "Изберете дело";

            if (model.CaseSessionActId < 1)
                return "Изберете акт";

            if (model.ActResultId < 1)
                return "Изберете резултат";

            if (model.DateResult == null)
                return "Въведете дата";

            return string.Empty;
        }

        [HttpPost]
        public IActionResult EditResult(CaseSessionActComplainResultEditVM model)
        {
            if (model.CaseSessionActComplains == null)
                model.CaseSessionActComplains = new List<CheckListVM>();

            SetViewbagResult(model.CaseSessionActComplainId);

            if (!ModelState.IsValid)
            {
                return View(nameof(EditResult), model);
            }

            string _isvalid = IsValidResult(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditResult), model);
            }

            var currentId = model.Id;
            if (service.CaseSessionActComplainResult_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionActComplainResult, model.Id, currentId == 0);
                var caseSessionActComplain = service.GetById<CaseSessionActComplain>(model.CaseSessionActComplainId);
                model.CaseSessionActComplains = service.GetCheckListCaseSessionActComplains(caseSessionActComplain.Id, caseSessionActComplain.CaseSessionActId);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditResult), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditResult), model);
        }

        void SetViewbagResult(int CaseSessionActComplainId)
        {
            var caseSessionActComplain = service.GetById<CaseSessionActComplain>(CaseSessionActComplainId);
            var caseSessionAct = service.GetById<CaseSessionAct>(caseSessionActComplain.CaseSessionActId);
            var caseSession = service.GetById<CaseSession>(caseSessionAct.CaseSessionId);

            ViewBag.ComplainCaseId_ddl = caseMigrationService.GetDropDownList_CourtCase(caseSession.CaseId);
            ViewBag.ActResultId_ddl = nomService.GetDropDownList<ActResult>();
            
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionActComplainEdit(CaseSessionActComplainId);
            ViewBag.hasIsStartNewLifecycle = caseLifecycleService.CaseLifecycle_IsAllLifcycleClose(caseSession.CaseId);
            SetHelpFile(HelpFileValues.SessionAct);
        }

        [HttpGet]
        public IActionResult GetDDL_ActResult(int CaseFromId, int CaseSessionActComplainId)
        {
            var model = nomService.GetDDL_ActResult(CaseFromId, CaseSessionActComplainId);
            return Json(model);
        }

        #endregion

        #region CaseSessionActComplainPerson

        [HttpPost]
        public IActionResult ListDataCaseSessionActComplainPerson(IDataTablesRequest request, int CaseSessionActComplainId)
        {
            var data = service.CaseSessionActComplainPerson_Select(CaseSessionActComplainId);
            return request.GetResponse(data);
        }

        public IActionResult AddCaseSessionActComplainPerson(int CaseSessionActComplainId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActComplainResult, null, AuditConstants.Operations.Update, CaseSessionActComplainId))
            {
                return Redirect_Denied();
            }
            var checkListViewVM = service.CheckListViewVM_FillCasePerson(CaseSessionActComplainId);
            if (checkListViewVM.checkListVMs.Count < 1)
            {
                SetErrorMessage("Няма лица в делото за да изберете");
                return RedirectToAction("Edit", "CaseSessionActComplain", new { id = CaseSessionActComplainId });
            }

            ViewBag.backUrl = Url.Action("Edit", "CaseSessionActComplain", new { id = CaseSessionActComplainId });
            return View("CheckListViewVM", checkListViewVM);
        }

        [HttpPost]
        public IActionResult AddCaseSessionActComplainPerson(CheckListViewVM model)
        {
            if (service.CaseSessionActComplainPerson_SaveData(model))
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            CheckAccess(service, SourceTypeSelectVM.CaseSessionActComplainResult, null, AuditConstants.Operations.Update, model.ObjectId);

            ViewBag.backUrl = Url.Action("Edit", "CaseSessionActComplain", new { id = model.ObjectId });
            return View("CheckListViewVM", model);
        }

        #endregion
    }
}