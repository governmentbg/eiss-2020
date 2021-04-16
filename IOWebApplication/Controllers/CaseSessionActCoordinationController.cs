using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CaseSessionActCoordinationController : BaseController
    {
        private readonly ICaseSessionActCoordinationService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICaseSessionService sessionService;
        private readonly ICaseLawUnitService lawUnitService;

        public CaseSessionActCoordinationController(ICaseSessionActCoordinationService _service, INomenclatureService _nomService, ICommonService _commonService, ICaseSessionService _sessionService, ICaseLawUnitService _lawUnitService)
        {
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            sessionService = _sessionService;
            lawUnitService = _lawUnitService;
        }

        public IActionResult Index(int CaseSessionActId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActCoordination, CaseSessionActId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            SetViewbagCaption(CaseSessionActId);
            return View();
        }

        private void SetViewbagCaption(int CaseSessionActId)
        {
            //var sessionAct = service.GetById<CaseSessionAct>(CaseSessionActId);
            //var actType = nomService.GetById<ActType>(sessionAct.ActTypeId);
            //ViewBag.CaseSessionActName = $"{actType.Label} {sessionAct.RegNumber} / {sessionAct.RegDate:dd.MM.yyyy}";
            //ViewBag.caseSessionActId = sessionAct.Id;

            //var caseSession = sessionService.CaseSessionById(sessionAct.CaseSessionId);
            //ViewBag.CaseSessionName = caseSession.SessionType?.Label + " " + caseSession.DateFrom.ToString("dd.MM.yyyy");
            //ViewBag.caseSessionId = caseSession.Id;

            //var caseCase = service.GetById<Case>(caseSession.CaseId);
            //ViewBag.CaseName = caseCase.RegNumber;
            //ViewBag.caseId = caseCase.Id;

            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(CaseSessionActId);
            SetHelpFile(HelpFileValues.SessionAct);

            ViewBag.ActCoordinationTypeId_ddl = nomService.GetDropDownList<ActCoordinationType>();
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int CaseSessionActId)
        {
            var data = service.CaseSessionActCoordination_Select(CaseSessionActId);
            return request.GetResponse(data);
        }

        public IActionResult Add(int CaseSessionActId)
        {
            var sessionAct = service.GetById<CaseSessionAct>(CaseSessionActId);
            SetViewbagCaption(CaseSessionActId);
            var model = new CaseSessionActCoordination()
            {
                CaseId = sessionAct.CaseId,
                CourtId = sessionAct.CourtId,
                CaseSessionActId = CaseSessionActId,
            };

            return View(nameof(Edit), model);
        }

        public IActionResult Edit(int id)
        {
            var model = service.GetById<CaseSessionActCoordination>(id);
            if (model == null)
            {
                throw new NotFoundException("Търсеното от Вас съгласуване на акт не е намерено и/или нямате достъп до него.");
            }
            if (!CheckAccess(service, SourceTypeSelectVM.CaseSessionActCoordination, model.CaseSessionActId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            SetViewbagCaption(model.CaseSessionActId);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult Edit(CaseSessionActCoordination model)
        {
            SetViewbagCaption(model.CaseSessionActId);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CaseSessionActCoordination_SaveData(model))
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
    }
}