using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseSessionActLawBaseController : BaseController
    {
        private readonly ICaseSessionActLawBaseService service;
        private readonly INomenclatureService nomService;
        private readonly ICaseSessionService sessionService;
        private readonly ICommonService commonService;

        public CaseSessionActLawBaseController(ICaseSessionActLawBaseService _service, INomenclatureService _nomService,
            ICaseSessionService _sessionService, ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            sessionService = _sessionService;
            commonService = _commonService;
        }

        public IActionResult Index(int caseSessionActId)
        {
            ViewBag.caseSessionActId = caseSessionActId;            
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionAct(caseSessionActId);
            SetHelpFile(HelpFileValues.SessionAct);

            return View();
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseSessionActId)
        {
            var data = service.CaseSessionActLawBase_Select(caseSessionActId);

            return request.GetResponse(data);
        }

        public async Task<IActionResult> Add(int caseSessionActId)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActLawBase, null, AuditConstants.Operations.Append, caseSessionActId))
            {
                return Redirect_Denied();
            }
            var act = await service.GetReadonlyAsync<CaseSessionAct>(caseSessionActId);
            SetViewbag(caseSessionActId, act.CaseId ?? 0);
            var model = new CaseSessionActLawBase()
            {
                CourtId = act.CourtId,
                CaseId = act.CaseId,
                CaseSessionActId = caseSessionActId
            };
            return View(nameof(Edit), model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.CaseSessionActLawBase, id, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await service.GetByIdAsync<CaseSessionActLawBase>(id);
            SetViewbag(model.CaseSessionActId, model.CaseId ?? 0);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        public IActionResult Edit(CaseSessionActLawBase model)
        {
            SetViewbag(model.CaseSessionActId, model.CaseId ?? 0);

            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;
            if (service.CaseSessionActLawBase_SaveData(model))
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSessionActLawBase, model.Id, currentId == 0);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Index), new { caseSessionActId = model.CaseSessionActId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        void SetViewbag(int caseSessionActId, int CaseId)
        {
            ViewBag.LawBaseId_ddl = nomService.GetDDL_LawBase(CaseId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCaseSessionActLawBase(caseSessionActId);
            SetHelpFile(HelpFileValues.SessionAct);
        }
    }
}