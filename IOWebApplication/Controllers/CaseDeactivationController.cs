using DataTables.AspNet.Core;
using ICSharpCode.SharpZipLib.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Rotativa.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CaseDeactivationController : BaseController
    {

        private readonly ICaseDeactivationService service;
        private readonly ICdnService cdnService;
        private readonly ICaseSessionService caseSessionService;
        private readonly ICaseSessionActService caseSessionActService;

        public CaseDeactivationController(
            ICaseDeactivationService _service,
            ICdnService _cdnService,
            ICaseSessionService _caseSessionService,
            ICaseSessionActService _caseSessionActService)
        {
            this.service = _service;
            this.cdnService = _cdnService;
            caseSessionService = _caseSessionService;
            caseSessionActService = _caseSessionActService;
        }

        public IActionResult Index()
        {
            var model = new CaseDeactivationFilterVM()
            {
                DateFrom = new DateTime(DateTime.Now.Year, 1, 1)
            };
            SetHelpFile(HelpFileValues.AnnulledCasesRegister);
            return View(model);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, CaseDeactivationFilterVM filter)
        {
            var data = service.Select(filter);
            return request.GetResponse(data);
        }

        public IActionResult Add()
        {
            var model = new CaseDeactivation();
            ViewBag.courtId = userContext.CourtId;
            SetHelpFile(HelpFileValues.AnnulledCasesRegister);
            return View(model);
        }

        private string IsValid(CaseDeactivation model)
        {
            if (caseSessionActService.IsExistCaseSessionActByCase(model.CaseId))
            {
                return "Не може да анулирате дело в което има активни актове.";
            }

            if (caseSessionService.IsExistCaseSession(model.CaseId))
            {
                return "Не може да анулирате дело в което има активни заседания.";
            }

            return string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Add(CaseDeactivation model)
        {
            SetHelpFile(HelpFileValues.AnnulledCasesRegister);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(model);
            }

            var saveResult = service.Add(model);
            if (saveResult.Result)
            {
                SaveLogOperation(true, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);

                await prepareProtokolFile(model.Id);

                return RedirectToAction(nameof(SendForSign), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(saveResult.ErrorMessage);
            }

            return View(model);
        }
        private async Task prepareProtokolFile(int id)
        {
            var model = service.Select(new CaseDeactivationFilterVM { Id = id }).FirstOrDefault();
            var htmlALL = await this.RenderPartialViewAsync("~/Views/CaseDeactivation/", "_Protokol.cshtml", model, true);
            byte[] pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = htmlALL }).GetByte(this.ControllerContext);

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.CaseDeactivate,
                SourceId = id.ToString(),
                FileName = "sessionAct.pdf",
                ContentType = NomenclatureConstants.ContentTypes.Pdf,
                Title = $"Протокол за анулиране на дело {model.CaseNumber}",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            await cdnService.MongoCdn_AppendUpdate(pdfRequest);
        }
        public async Task<IActionResult> View(int id)
        {
            var model = service.Select(new CaseDeactivationFilterVM { Id = id }).FirstOrDefault();
            ViewBag.html = await this.RenderPartialViewAsync("~/Views/CaseDeactivation/", "_Protokol.cshtml", model, true);
            SetHelpFile(HelpFileValues.AnnulledCasesRegister);
            return View(model);
        }

        public IActionResult SendForSign(int id)
        {
            var saved = service.Select(new CaseDeactivationFilterVM { Id = id }).FirstOrDefault();
            if (saved.DeclaredDate != null)
            {
                return RedirectToAction(nameof(View), new { id });
            }
            Uri urlSuccess = new Uri(Url.Action("Signed", "CaseDeactivation", new { id = id }), UriKind.Relative);
            Uri url = new Uri(Url.Action("View", "CaseDeactivation", new { id = id }), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.CaseDeactivate,
                DestinationType = SourceTypeSelectVM.CaseDeactivate,
                Location = userContext.CourtName,
                Reason = "Подписване на протокол за анулиране",
                SuccessUrl = urlSuccess,
                CancelUrl = url,
                ErrorUrl = url,
                SignerName = saved.DeactivateUserName,
                SignerUic = saved.DeactivateUserUIC
            };

            return View("_SignPdf", model);
        }

        public IActionResult Signed(int id)
        {
            var model = service.Select(new CaseDeactivationFilterVM { Id = id }).FirstOrDefault();
            if (model.DeclaredDate == null)
            {
                if (service.DeclareDeactivation(id))
                {
                    SetSuccessMessage("Протоколът за анулиране беше подписан успешно.");
                    return RedirectToAction(nameof(View), new { id = id });
                }
            }
            return RedirectToAction(nameof(View), new { id = id });
        }
    }
}
