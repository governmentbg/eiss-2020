using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rotativa.Extensions;
using System;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class ManageController : BaseController
    {

        private readonly ICdnService cdnService;
        private readonly ITransactionService transactionService;
        public ManageController(
            ICdnService _cdnService,
            ITransactionService _transactionService)
        {
            cdnService = _cdnService;
            transactionService = _transactionService;
        }
        public IActionResult ExpiredInfo(int id, long longId, string stringId, string fileContainer, string submitUrl, string returnUrl, bool otherBool, int? OtherId)
        {
            var model = new ExpiredInfoVM()
            {
                Id = id,
                LongId = longId,
                StringId = stringId,
                ExpireSubmitUrl = submitUrl,
                FileContainerName = fileContainer,
                ReturnUrl = returnUrl,
                OtherBool = otherBool,
                OtherId = OtherId
            };
            SetSourceKey(SourceTypeSelectVM.ExpireObject, model.KeyString, $"ExpInfo{userContext?.UserId}");
            return PartialView(model);
        }

        public async Task<IActionResult> TestSign()
        {
            var html = $"<br/><br/><br/><br/><br/><br/><br/><h3>Тестов документ за подписване</h3><h4>{userContext.FullName}; Създадено на: {DateTime.Now}</h4>";
            byte[] pdfBytes = await (new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true, GetFooterInfoUrl(userContext.CourtId)).GetByte(this.ControllerContext));

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.TestSignPDF,
                SourceId = userContext.LawUnitId.ToString(),
                FileName = "testPdfSign.pdf",
                ContentType = NomenclatureConstants.ContentTypes.Pdf,
                Title = $"Тестов документ за подписване {DateTime.Now}",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };

            if (await cdnService.MongoCdn_AppendUpdate(pdfRequest))
            {
                HttpContext.Response.Clear();
                //return Content("ok");
                //await Task.Delay(1000);

                var userOk = new Uri(Url.Action(nameof(TestSignResult), new { isOk = true }), UriKind.Relative);
                var userFailed = new Uri(Url.Action(nameof(TestSignResult), new { isOk = false }), UriKind.Relative);
                var model = new SignPdfInfo()
                {
                    SourceId = pdfRequest.SourceId,
                    SourceType = pdfRequest.SourceType,
                    DestinationType = pdfRequest.SourceType,
                    Location = userContext.CourtName,
                    Reason = "Тестово подписване на документ",
                    SuccessUrl = userOk,
                    CancelUrl = userFailed,
                    ErrorUrl = userFailed
                };
                return View("_SignPdf", model);
            }
            else
            {
                return RedirectToAction(nameof(TestSignResult), new { isOk = false });
            }
        }
        public IActionResult Sign()
        {


            var userOk = new Uri(Url.Action(nameof(TestSignResult), new { isOk = true }), UriKind.Relative);
            var userFailed = new Uri(Url.Action(nameof(TestSignResult), new { isOk = false }), UriKind.Relative);
            var model = new SignPdfInfo()
            {
                SourceId = userContext.LawUnitId.ToString(),
                SourceType = SourceTypeSelectVM.TestSignPDF,
                DestinationType = SourceTypeSelectVM.TestSignPDF,
                Location = userContext.CourtName,
                Reason = "Тестово подписване на документ",
                SuccessUrl = userOk,
                CancelUrl = userFailed,
                ErrorUrl = userFailed
            };
            return View("_SignPdf", model);

        }

        public IActionResult TestSignResult(bool isOk)
        {
            if (!isOk)
            {
                ViewBag.error = (string)TempData["signError"];
            }
            return View(isOk);
        }

        public async Task<IActionResult> TakeMainGroup(int sourceType)
        {
            var takeResult = await transactionService.TakeForOperation(sourceType);
            if (takeResult.Result)
            {
                return LocalRedirect(transactionService.GetOperationUrl(sourceType, (long)takeResult.ObjectId));
            }
            else
            {
                SetErrorMessage("Данните са междувременно актуализирани.");
                return LocalRedirect(transactionService.GetOperationFailUrl(sourceType));
            }
        }
    }
}