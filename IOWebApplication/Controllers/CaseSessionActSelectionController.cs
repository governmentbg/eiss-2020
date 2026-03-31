using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Rotativa.Extensions;
using System;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseSessionActSelectionController : BaseController
    {
        private readonly ICaseSessionActSelectionService serviceSelection;
        private readonly ICaseSessionActService service;
        private readonly INomenclatureService nomService;
        private readonly ICommonService commonService;
        private readonly ICdnService cdnService;
        private readonly ICourtDepartmentService courtDepartmentService;


        public CaseSessionActSelectionController(
            ICaseSessionActSelectionService _serviceSelection,
            ICaseSessionActService _service,
            INomenclatureService _nomService,
            ICommonService _commonService,
            ICourtDepartmentService _courtDepartmentService,
            ICdnService _cdnService
            )
        {
            serviceSelection = _serviceSelection;
            service = _service;
            nomService = _nomService;
            commonService = _commonService;
            courtDepartmentService = _courtDepartmentService;
            cdnService = _cdnService;
        }



        public async Task<IActionResult> Index()
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Търсене в списъчен екран  протоколи за съдебни актове ");
            CaseSessionActSelectionProtocolFilterVM filter = new CaseSessionActSelectionProtocolFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31),
                IsFinal = false,
                IsCanceling = false,
                IsBeacameFinal = false,
            };

            //  SetHelpFile(HelpFileValues.CourtActsandProtocols);
            ViewBag.ActResultId_ddl = await nomService.GetDDL_ActComplainResult(false, true);
            return View(filter);
        }
        public async Task<IActionResult> ActList()
        {
            if (!await CheckAccessAsync(service, SourceTypeSelectVM.Case, null, AuditConstants.Operations.View))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            CurrentContext_SetObjectInfo("Списъчен екран Съдебни актове за протоколи");
            CaseSessionActSelectionProtocolFilterVM filter = new CaseSessionActSelectionProtocolFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                DateTo = new DateTime(DateTime.Now.Year, 12, 31),
                IsFinal = false,
                IsCanceling = false,
                IsBeacameFinal = false,
            };

            //SetHelpFile(HelpFileValues.CourtActsandProtocols);
            ViewBag.ActResultId_ddl = await nomService.GetDDL_ActComplainResult(false, true);

            return View(filter);
        }

        [HttpPost]
        public IActionResult ListDataSpr(IDataTablesRequest request, CaseSessionActSelectionProtocolFilterVM model)
        {
            var data = serviceSelection.CaseSessionActSelectionProtol_sel(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        [HttpPost]
        public IActionResult ListDataActsForSelection(IDataTablesRequest request, CaseSessionActSelectionProtocolFilterVM model)
        {
            var data = serviceSelection.CaseSessionActSelectionAct_sel(userContext.CourtId, model);
            return request.GetResponse(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSelection(CaseSessionActSelectionProtocolFilterVM model)
        {
            var id = await serviceSelection.CreateActSelection(userContext.CourtId, model);

            if (id > 0)
            {
                return RedirectToAction("Signdoc", new { id = id });
            }
            else

            { return RedirectToAction("Index"); }
        }



        public async Task<IActionResult> Preview(int id)
        {
            var protocol = await serviceSelection.GetSelectionActProtocolByID(id);



            return View(protocol);
        }
        public async Task<IActionResult> PreviewDoc(int id)
        {
            var protocol = await serviceSelection.GetSelectionActProtocolByID(id);



            return View(protocol);
        }
        public async Task<IActionResult> SendForSign(int id)
        {
            {
                Uri urlSuccess = new Uri(Url.Action("SignedDoc", "CaseSessionActSelection", new { id = id }), UriKind.Relative);
                Uri url = new Uri(Url.Action("PreviewDoc", "CaseSessionActSelection", new { id = id }), UriKind.Relative);

                var signModel = new Core.Models.SignPdfInfo()
                {
                    SourceId = id.ToString(),
                    SourceType = SourceTypeSelectVM.CaseSessionActSelection,
                    DestinationType = SourceTypeSelectVM.CaseSessionActSelection,
                    Location = "Sofia",
                    Reason = "Sign",
                    SuccessUrl = urlSuccess,
                    CancelUrl = url,
                    ErrorUrl = url
                };
                var protokolModel = await serviceSelection.GetSelectionActProtocolByID(id);
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
            var protokolModel = await serviceSelection.GetSelectionActProtocolByID(id);

            string html = "";


            html = await this.RenderPartialViewAsync("~/Views/CaseSessionActSelection/", "Preview.cshtml", protokolModel, true);

            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseSessionActSelection,
                SourceId = id.ToString(),
                FileName = "selectionProtokol.pdf",
                ContentType = "application/pdf",
                Title = $"ЗА ИЗБОР НА СЪДЕБНИ АКТОВЕ НА СЛУЧАЕН ПРИНЦИП {protokolModel.SelectedLawUnitName} ",
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

            var protocolId = await serviceSelection.ActSelectionProtokol_SignUpdate(id);
            SetSuccessMessage("Протоколът беше подписан успешно!");


            if (protocolId > 0)
            { return RedirectToAction("PreviewDoc", new { id = protocolId }); }
            else
            { return RedirectToAction("Index"); }

        }
    }
}