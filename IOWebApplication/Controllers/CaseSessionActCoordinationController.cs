using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index(int CaseSessionActId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActCoordination, 0, AuditConstants.Operations.Update, CaseSessionActId))
            {
                return Redirect_Denied();
            }
            SetViewbagCoordination(CaseSessionActId);
            return View();
        }

        private void SetViewbagCoordination(int CaseSessionActId)
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
        public IActionResult ListData(IDataTablesRequest request, int CaseSessionActId, int CoordinationType)
        {
            var data = service.CaseSessionActCoordination_Select(CaseSessionActId, null, CoordinationType);
            return request.GetResponse(data);
        }

        public IActionResult Add(int CaseSessionActId)
        {
            var sessionAct = service.GetReadonly<CaseSessionAct>(CaseSessionActId);
            SetViewbagCoordination(CaseSessionActId);
            var model = new CaseSessionActCoordination()
            {
                CaseId = sessionAct.CaseId,
                CourtId = sessionAct.CourtId,
                CaseSessionActId = CaseSessionActId,
            };

            return View(nameof(Edit), model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await service.GetByIdAsync<CaseSessionActCoordination>(id);
            if (model == null)
            {
                return NotFoundError("Търсеното от Вас съгласуване на акт не е намерено и/или нямате достъп до него.");
            }
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActCoordination, id, AuditConstants.Operations.Update, model.CaseSessionActId))
            {
                return Redirect_Denied();
            }
            var actModel = await service.GetReadonlyAsync<CaseSessionAct>(model.CaseSessionActId);
            ViewBag.canUpdate = (actModel.ActDeclaredDate == null && model.CoordinationType == NomenclatureConstants.CoordinationTypes.Act) ||
                                (actModel.ActMotivesDeclaredDate == null && model.CoordinationType == NomenclatureConstants.CoordinationTypes.Motive);
            SetViewbagCoordination(model.CaseSessionActId);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult Edit(CaseSessionActCoordination model)
        {
            SetViewbagCoordination(model.CaseSessionActId);
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