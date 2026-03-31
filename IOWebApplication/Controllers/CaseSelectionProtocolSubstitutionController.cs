using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rotativa.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseSelectionProtocolSubstitutionController : BaseController
    {
        private readonly ICaseSelectionProtocolSubstitutionService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICdnService cdnService;
        private readonly ILogger logger;
        private readonly ICaseSelectionProtokolService selectionProtocolservice;
        private readonly ICourtLoadPeriodService courtLoadPeriodService;


        public CaseSelectionProtocolSubstitutionController(

            ICaseSelectionProtocolSubstitutionService _service,
            INomenclatureService _nomService,
            ICommonService _commonService,
            ICourtDepartmentService _courtDepartmentService,
            ICdnService _cdnService,
            ILogger<CaseSessionActController> logger,
            ICaseSelectionProtokolService _selectionProtocolservice,
            ICourtLoadPeriodService _courtLoadPeriodService

            )
        {

            service = _service;
            nomService = _nomService;
            commonService = _commonService;

            cdnService = _cdnService;
            this.logger = logger;
            selectionProtocolservice = _selectionProtocolservice;
            courtLoadPeriodService = _courtLoadPeriodService;
        }



        public async Task<IActionResult> Index()
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Търсене в списъчен екран  протоколи за съдебни актове ");
            CaseSelectionProtocolSubstitutionFilterVM filter = new CaseSelectionProtocolSubstitutionFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31),

            };
            ViewBag.CourtGroupId_ddl = nomService.GetDDL_CourtGroup_Substitution(userContext.CourtId, true);

            return View(filter);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, CaseSelectionProtocolSubstitutionFilterVM model)
        {
            var data = service.CaseSelectionProtolSubstitution_sel(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        [HttpPost]
        public IActionResult ListDataCaseSelectionSubstitution(IDataTablesRequest request, int caseId)
        {
            var data = service.GetCaseSelectionSubstitution(caseId);
            return request.GetResponse(data);
        }

        public IActionResult Edit()
        {
            //if (!CheckAccess(service, SourceTypeSelectVM.CaseSelectionProtokol, null, AuditConstants.Operations.Append, caseId))
            //{
            //    return Redirect_Denied();
            //}
            //var caseRegnumber = service.GetPropById<Case, string>(x => x.Id == caseId, x => x.RegNumber);
            //if (string.IsNullOrEmpty(caseRegnumber))
            //{
            //    SetErrorMessage("Данните по делото не са обновени.");
            //    return RedirectToAction("Edit", "Case", new { id = caseId });
            //}
            //var tcase = service.GetById<Case>(caseId);
            var model = new CaseSelectionProtokolVM()
            {
                CaseId = 1,
                CourtId = userContext.CourtId,
                CaseGroupId = 1,
                CaseCodeId = 0,
                CourtGroupId = null,
                SelectionModeId = 1,
                JudgeRoleId = NomenclatureConstants.JudgeRole.JudgeReporter,
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31),
                IsProtokolNoSelection = false
            };


            ViewBag.CourtGroupId_ddl = nomService.GetDDL_CourtGroup_Substitution(userContext.CourtId, true);

            return View(model);
        }



        void SetViewBagLawUnits(int selectionMode)
        {
            ViewBag.states = nomService.GetSelectionLawUnitState(selectionMode);
            ViewBag.statesExclude = nomService.GetSelectionLawUnitState(selectionMode, true);
        }

        public IActionResult LawUnits_LoadByGroup(int courtGroupId, int substitutedLawUnitId)
        {

            IEnumerable<CaseSelectionProtokolLawUnitVM> model = null;
            ViewBag.RoleId = NomenclatureConstants.JudgeRole.JudgeReporter;
            model = selectionProtocolservice.LawUnit_LoadJudge(courtGroupId, 1, userContext.CourtId, NomenclatureConstants.JudgeRole.JudgeReporter, substitutedLawUnitId);
            SetViewBagLawUnits(NomenclatureConstants.SelectionMode.SelectByGroups);
            return PartialView("_LoadedLawUnits", model);
        }

        [RequestFormLimits(ValueCountLimit = 15000)]
        [HttpPost]
        public IActionResult Edit(CaseSelectionProtokolVM model)
        {
            ViewBag.CourtGroupId_ddl = nomService.GetDDL_CourtGroup_Substitution(userContext.CourtId, true);
            ViewBag.RoleId = NomenclatureConstants.JudgeRole.JudgeReporter;
            SetViewBagLawUnits(NomenclatureConstants.SelectionMode.SelectByGroups);
            //var checkLock = service.CheckCaseLock(model.CaseId);

            //if (!checkLock)
            //{
            //    SetErrorMessage("Непозволена операция, моля проверете последните протоколи!");
            //    return RedirectToAction(nameof(Index), new { id = model.CaseId });
            //}

            //logger.LogCritical($"[POST] Edit {DateTime.Now.ToString("mm:ss.FFF")}");
            bool ExitByTime = false;

            ValidateModel(model);


            if (!ModelState.IsValid)
            {

                return View("Edit", model);
            }
            var currentId = model.Id;
            string errorMessage = "";
            bool save = true;


            if (!service.SubstitutionSelectionProtokol_SaveData(model, ref errorMessage))
                save = false;




            if (save == true)
            {
                SetAuditContext(service, SourceTypeSelectVM.CaseSelectionProtokolSubstitution, model.Id, currentId == 0);

                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                if (errorMessage == "")
                    errorMessage = MessageConstant.Values.SaveFailed;
                if (ExitByTime)
                {
                    errorMessage = MessageConstant.Values.TimeoutSelectProtokol;
                }
                SetErrorMessage(errorMessage);

                return View("Edit", model);
            }
            //В случай няма наличен състав се отива на подпис

            //var saved_model = service.CaseSelectionProtokol_Preview(model.Id);
            //var comparentmentLisCount = service.GetJudgeComprentmetList((saved_model.SelectedLawUnitId ?? 0), saved_model.CourtId, saved_model.CaseId).Count;
            //logger.LogCritical($"Edit done  {DateTime.Now.ToString("mm: ss.FFF")}");

            //  var res = service.CaseSelectionProtokol_UpdateBeforeDocForSign(saved_model);
            return RedirectToAction("PreviewDoc", new { id = model.Id });


            //    return RedirectToAction("PreviewDoc", new { id = model.Id });
        }


        void ValidateModel(CaseSelectionProtokolVM model)
        {
            if (model.CourtGroupId < 1)
            {
                ModelState.AddModelError("", "Моля, въведете Група заместване.");
            }

            if (!model.SubstitudedJudgeId.HasValue || model.SubstitudedJudgeId < 1)
            {
                ModelState.AddModelError("", "Моля, въведете Заместван съдия.");
            }

            if (model.DateFrom >= model.DateTo)
            {
                ModelState.AddModelError("", "Моля, въведете коректни дати за заместване.");
            }


            if (string.IsNullOrEmpty(model.DescriptionSubstitution))
            {
                ModelState.AddModelError("", "Моля, въведете Причина за заместване.");
            }
            if (model.LawUnits.Count < 1)
            {
                ModelState.AddModelError("", "Моля, заредете списък с участници за пазпределение.");
            }
            string errorDescription = "";
            for (int i = 0; i < model.LawUnits.Count(); i++)
            {
                var lawUnit = model.LawUnits[i];




                if (lawUnit.StateId == NomenclatureConstants.SelectionProtokolLawUnitState.Exclude && lawUnit.Description == null)
                {
                    errorDescription = "Моля, въведете причина за неучастие на " + lawUnit.LawUnitFullName;
                    ModelState.AddModelError($"{nameof(CaseSelectionProtokolVM.LawUnits)}[{i}].{nameof(CaseSelectionProtokolLawUnitVM.Description)}", errorDescription);
                }


            }




        }

        public async Task<IActionResult> Preview(int id)
        {
            var protocol = await service.GetSelectionProtocolSubstitutionByID(id);



            return View(protocol);
        }
        public async Task<IActionResult> PreviewDoc(int id)
        {
            var protocol = await service.GetSelectionProtocolSubstitutionByID(id);



            return View(protocol);
        }

        public async Task<IActionResult> SendForSign(int id)
        {
            {
                Uri urlSuccess = new Uri(Url.Action("SignedDoc", "CaseSelectionProtocolSubstitution", new { id = id }), UriKind.Relative);
                Uri url = new Uri(Url.Action("PreviewDoc", "CaseSelectionProtocolSubstitution", new { id = id }), UriKind.Relative);

                var signModel = new Core.Models.SignPdfInfo()
                {
                    SourceId = id.ToString(),
                    SourceType = SourceTypeSelectVM.CaseSelectionProtokolSubstitution,
                    DestinationType = SourceTypeSelectVM.CaseSelectionProtokolSubstitution,
                    Location = "Sofia",
                    Reason = "Sign",
                    SuccessUrl = urlSuccess,
                    CancelUrl = url,
                    ErrorUrl = url
                };
                var protokolModel = await service.GetSelectionProtocolSubstitutionByID(id);
                if (protokolModel != null)
                {
                    signModel.SignerName = protokolModel.UserName;
                    signModel.SignerUic = protokolModel.UserUIK;
                }
                return View("_SignPdf", signModel);
            }
        }



        public async Task<IActionResult> SignDoc(int id)
        {
            var protokolModel = await service.GetSelectionProtocolSubstitutionByID(id);

            string html = "";


            html = await this.RenderPartialViewAsync("~/Views/CaseSelectionProtocolSubstitution/", "Preview.cshtml", protokolModel, true);

            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseSelectionProtokolSubstitution,
                SourceId = id.ToString(),
                FileName = "selectionProtokol.pdf",
                ContentType = "application/pdf",
                Title = $"Протокол от избор на заместващ съдия {protokolModel.SelectedLawUnitName} ",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            if (await cdnService.MongoCdn_AppendUpdate(pdfRequest))
            {
                return RedirectToAction(nameof(SendForSign), new { id = id });
            }
            else
            {
                SetErrorMessage("Проблем при създаване на протокол!");
                return RedirectToAction(nameof(PreviewDoc), new { id = id });
            }


        }
        public async Task<IActionResult> SignedDoc(int id)
        {

            var protocolId = await service.SelectionProtokolSubstitution_SignUpdate(id);
            SetSuccessMessage("Протоколът беше подписан успешно!");


            if (protocolId > 0)
            { return RedirectToAction("PreviewDoc", new { id = protocolId }); }
            else
            { return RedirectToAction("Index"); }

        }

    }
}