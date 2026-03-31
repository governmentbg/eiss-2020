using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseMigrationController : BaseController
    {
        private readonly ICaseMigrationService migService;
        private readonly INomenclatureService nomService;
        private readonly ICaseService caseService;
        private readonly ICaseSessionActService actService;
        private readonly ICommonService commonService;

        public CaseMigrationController(
            ICaseMigrationService _migService,
            INomenclatureService _nomService,
            ICaseService _caseService,
            ICaseSessionActService _actService,
            ICommonService _commonService
            )
        {
            migService = _migService;
            nomService = _nomService;
            caseService = _caseService;
            actService = _actService;
            commonService = _commonService;
        }

        /// <summary>
        /// Страница с движения за дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int caseId)
        {
            if (!await CheckAccessAsync(migService, SourceTypeSelectVM.CaseMigration, null, AuditConstants.Operations.View, caseId))
            {
                return Redirect_Denied();
            }
            await SetViewBag(new CaseMigrationUnionVM() { CaseId = caseId });
            return View();
        }

        /// <summary>
        /// Извличане на данни за движения за дело
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int caseId)
        {
            var data = migService.Select(caseId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на движение
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Add(int caseId)
        {
            if (!await CheckAccessAsync(migService, SourceTypeSelectVM.CaseMigration, null, AuditConstants.Operations.Append, caseId))
            {
                return Redirect_Denied();
            }
            var model = await migService.InitNewMigration(caseId);
            await SetViewBag(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на движение
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {

            var model = await migService.GetByIdAsync<CaseMigration>(id);

            if (!await CheckAccessAsync(migService, SourceTypeSelectVM.CaseMigration, id, AuditConstants.Operations.Update, model.CaseId))
            {
                return Redirect_Denied();
            }

            //if (model.OutDocumentId > 0)
            //{
            //    CurrentContext.CanChange = false;
            //}

            await SetViewBag(model);
            return View(model);
        }

        /// <summary>
        /// Запис на движение
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(CaseMigration model)
        {
            if (model.SendToTypeId == NomenclatureConstants.CaseMigrationSendTo.Court && (model.SendToCourtId ?? 0) <= 0)
            {
                if (model.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendCase_FromRandomAssignment)
                {
                    model.SendToCourtId = NomenclatureConstants.Courts.RandomAssignment;
                }
                else
                {
                    ModelState.AddModelError(nameof(CaseMigration.SendToCourtId), "Изберете съд");
                }
            }
            if (model.SendToTypeId == NomenclatureConstants.CaseMigrationSendTo.Institution && (model.SendToInstitutionId ?? 0) <= 0)
            {
                ModelState.AddModelError(nameof(CaseMigration.SendToInstitutionId), "Изберете институция");
            }
            if (model.SendToTypeId == NomenclatureConstants.CaseMigrationSendTo.Court)
            {
                if (NomenclatureConstants.CaseMigrationTypes.RequireReturnCaseTypes.Contains(model.CaseMigrationTypeId) && (model.ReturnCaseId ?? 0) <= 0)
                {
                    ModelState.AddModelError(nameof(CaseMigration.ReturnCaseId), "Изберете дело, подлежащо на връщане");
                }
                if (NomenclatureConstants.CaseMigrationTypes.RequireActs.Contains(model.CaseMigrationTypeId) && (model.CaseSessionActId ?? 0) <= 0)
                {
                    ModelState.AddModelError(nameof(CaseMigration.CaseSessionActId), "Изберете Обжалван акт");
                }
            }
            if (!ModelState.IsValid)
            {
                await SetViewBag(model);
                return View(model);
            }
            var checkResult = migService.CheckData(model);
            if (!checkResult.Result)
            {
                SetErrorMessage(checkResult.ErrorMessage);
                await SetViewBag(model);
                return View(model);
            }

            var currentId = model.Id;
            if (migService.SaveData(model))
            {
                await CheckAccessAsync(migService, SourceTypeSelectVM.CaseMigration, model.Id, (currentId == 0) ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model.CaseId);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                await SetViewBag(model);
                return View(model);
            }
        }

        /// <summary>
        /// Приемане на движение
        /// </summary>
        /// <param name="id"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AcceptMigration(int id, int caseId, bool isNewInterval)
        {
            if (!await CheckAccessAsync(migService, SourceTypeSelectVM.CaseMigration, id, AuditConstants.Operations.Update, caseId))
            {
                return Redirect_Denied();
            }
            if (migService.AcceptCaseMigration(id, caseId, null, isNewInterval).Result)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage("Проблем при приемане на движение на дело.");
            }
            return RedirectToAction(nameof(Index), new { caseId = caseId });
        }


        /// <summary>
        /// Обединяване на дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> UnionCase(int caseId)
        {
            var caseInfo = await caseService.GetCaseInfo(caseId);
            var model = new CaseMigrationUnionVM()
            {
                CaseId = caseId
            };
            model.CaseInfo = $"{caseInfo.RegNumber} ({caseInfo.CaseTypeCode})";
            await SetViewBag(model);
            return View(model);
        }

        /// <summary>
        /// Запис на обединяване на дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UnionCase(CaseMigrationUnionVM model)
        {
            if (model.CaseToUnionId == 0)
            {
                ModelState.AddModelError(nameof(CaseMigrationUnionVM.CaseToUnionId), "Дело, което ще се обедини с текущото");
            }
            if (!ModelState.IsValid)
            {
                await SetViewBag(model);
                return View(model);
            }
            if (migService.UnionCase(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage("Проблем при приемане на движение на дело.");
            }
            return RedirectToAction(nameof(Index), new { caseId = model.CaseId });
        }

        /// <summary>
        /// Приемане на дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AcceptToUse(int caseId)
        {
            var model = new CaseMigrationFindCaseVM()
            {
                CaseId = caseId
            };

            var caseInfo = await caseService.GetCaseInfo(caseId);
            model.CaseInfo = $"{caseInfo.RegNumber} ({caseInfo.CaseTypeCode})";
            SetViewBag(model);
            return View(model);
        }

        /// <summary>
        /// Запис на приемане на дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AcceptToUse(CaseMigrationFindCaseVM model)
        {
            int lastMigrationId = migService.GetLastMigrationAcceptToUse(model);
            if (lastMigrationId == 0)
            {
                ModelState.AddModelError(nameof(CaseMigrationFindCaseVM.FromCaseId), "По избраното дело не съществува неприето движение към текущия съд.");
            }
            if (!ModelState.IsValid)
            {
                SetViewBag(model);
                return View(model);
            }

            if (migService.AcceptCaseMigration(lastMigrationId, model.CaseId, model.Description).Result)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage("Проблем при приемане на движение на дело.");
            }
            return RedirectToAction(nameof(Index), new { caseId = model.CaseId });
        }



        async Task SetViewBag(CaseMigration model)
        {
            ViewBag.CaseMigrationTypeId_ddl = migService.Get_MigrationTypes(NomenclatureConstants.CaseMigrationDirections.Outgoing, null, model.CaseId);
            ViewBag.SendToInstitutionTypeId_ddl = await nomService.GetDropDownListAsync<InstitutionType>();
            ViewBag.ReturnCaseId_ddl = migService.GetDropDownList_ReturnCase(model.CaseId);
            ViewBag.ApealCaseSessionActId_ddl = (await actService.GetDDL_CanAppealAct(model.CaseId)).SetSelected(model.CaseSessionActId);
            ViewBag.AllEnforecedCaseSessionActId_ddl = (await actService.GetDropDownList_CaseSessionActEnforced(model.CaseId)).SetSelected(model.CaseSessionActId);

            ViewBag.caseId = model.CaseId;

            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(model.CaseId);
            SetHelpFile(HelpFileValues.CaseMigration);
        }

        async Task SetViewBag(CaseMigrationUnionVM model)
        {
            ViewBag.caseId = model.CaseId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(model.CaseId);
            var priorInfo = await migService.GetPriorCaseInfo(model.CaseId);
            if (priorInfo != null && !priorInfo.HasMigrations)
            {
                ViewBag.initMigration = true;
            }
            SetHelpFile(HelpFileValues.CaseMigration);
        }

        void SetViewBag(CaseMigrationFindCaseVM model)
        {
            ViewBag.caseId = model.CaseId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(model.CaseId);
            SetHelpFile(HelpFileValues.CaseMigration);
        }

        public async Task<IActionResult> ConnectWithPriorCase(int caseId)
        {
            if (!await CheckAccessAsync(migService, SourceTypeSelectVM.Case, caseId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }
            var model = await migService.GetPriorCaseInfo(caseId);
            if (model == null)
            {
                SetErrorMessage("Невалидни данни за движение");
                return RedirectToAction(nameof(Index), new { caseId });
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            ViewBag.CaseMigrationTypeId_ddl = migService.Get_MigrationTypes(NomenclatureConstants.CaseMigrationDirections.Outgoing, NomenclatureConstants.CaseMigrationTypes.ConnectCaseMigrations);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConnectWithPriorCase(CaseMigrationPriorVM model)
        {
            var saveResult = await migService.SaveData_PriorCase(model);
            if (saveResult.Result)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                this.SaveLogOperation(true, saveResult.ObjectId, null, nameof(Edit));
            }
            else
            {
                SetSuccessMessage(MessageConstant.Values.SaveFailed);
            }
            return RedirectToAction(nameof(Index), new { caseId = model.CaseId });
        }
    }
}