using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseSessionDocController : BaseController
    {
        private readonly ICaseSessionDocService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;

        public CaseSessionDocController(ICaseSessionDocService _service, INomenclatureService _nomService, ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
        }

        public IActionResult Index(int id)
        {
            var caseSession = service.GetById<CaseSession>(id);
            ViewBag.caseId = caseSession.CaseId;
            ViewBag.CaseSessionId = id;
            return View();
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int CaseSessionId)
        {
            var data = service.CaseSessionDoc_Select(CaseSessionId);
            return request.GetResponse(data);
        }

        public IActionResult Edit(int id)
        {
            var model = service.GetById<CaseSessionDoc>(id);
            SetViewbag(model.CaseSessionId);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult Edit(CaseSessionDoc model)
        {
            SetViewbag(model.CaseSessionId);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CaseSessionDoc_SaveData(model))
            {
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

        private void SetViewbag(int CaseSessionId)
        {
            ViewBag.SessionDocStateId_ddl = nomService.GetDropDownList<SessionDocState>();
            ViewBag.breadcrumbsEdit = commonService.Breadcrumbs_GetForCaseSession(CaseSessionId);
            SetHelpFile(HelpFileValues.SessionDoc);
        }

        [HttpPost]
        public IActionResult CaseSessionDoc_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionDoc, model.Id, AuditConstants.Operations.Delete))
            {
                return Redirect_Denied();
            }

            var expireObject = service.GetById<CaseSessionDoc>(model.Id);

            if (expireObject.SessionDocStateId != NomenclatureConstants.SessionDocState.Presented)
            {
                return Json(new { result = false, message = "Може да премахнете документ, само със статус: представен." });
            }

            if (service.SaveExpireInfo<CaseSessionDoc>(model))
            {
                SetAuditContextDelete(service, SourceTypeSelectVM.CaseSessionDoc, model.Id);
                SetSuccessMessage(MessageConstant.Values.CaseSessionExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action("Preview", "CaseSession", new { id = expireObject.CaseSessionId }) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }

        [HttpGet]
        public IActionResult AddCaseSessionDoc(int CeaseSessionId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionDoc, null, AuditConstants.Operations.ChoiceByList, CeaseSessionId))
            {
                return Redirect_Denied();
            }
            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { id = CeaseSessionId });
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(CeaseSessionId);
            SetHelpFile(HelpFileValues.SessionDoc);

            CheckListViewVM checkListView = service.CheckListViewVM_Fill(CeaseSessionId);

            if (checkListView.checkListVMs.Count < 1)
            {
                SetErrorMessage("Няма документи към това дело");
                return RedirectToAction("Preview", "CaseSession", new { id = CeaseSessionId });
            }

            return View("CheckListViewVM", checkListView);
        }

        [HttpPost]
        public IActionResult AddCaseSessionDoc(CheckListViewVM model)
        {
            if (service.CaseSessionDoc_SaveDataAdd(model))
            {
                CheckAccess(service, SourceTypeSelectVM.CaseSessionDoc, null, AuditConstants.Operations.ChoiceByList, model.ObjectId);
                CheckListViewVM checkListView = service.CheckListViewVM_Fill(model.ObjectId);
                if (checkListView.checkListVMs.Count < 1)
                {
                    SetSuccessMessage(MessageConstant.Values.SaveOK + " Няма документи за добавяне." );
                    return RedirectToAction("Preview", "CaseSession", new { id = model.ObjectId });
                }
                else
                    SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            ViewBag.backUrl = Url.Action("Preview", "CaseSession", new { id = model.ObjectId });
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSession(model.ObjectId);
            return RedirectToAction("AddCaseSessionDoc", "CaseSessionDoc", new { CeaseSessionId = model.ObjectId });
        }
    }
}