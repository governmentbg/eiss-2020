using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
//using AutoMapper.Configuration;
using IO.SignTools.Contracts;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IOWebApplication.Controllers
{
    public class PdfController : BaseController
    {
        private readonly ILogger logger;
        private readonly ICdnService cdn;
        private readonly IConfiguration config;

        public PdfController(
            ILogger<PdfController> _logger,
            ICdnService _cdn,
            IConfiguration _config)
        {
            logger = _logger;
            cdn = _cdn;
            config = _config;
        }

        [HttpGet]
        public IActionResult SignPdf(string sourceId, int sourceType)
        {
            if (String.IsNullOrEmpty(sourceId) || sourceType < 1)
            {
                SetErrorMessage("Документът не е намерен.");

                return this.RedirectToAction("Index", "Home");
            }

            Uri url = new Uri(Url.Action("Index", "Home"), UriKind.Relative);

            var model = new SignPdfInfo()
            {
                SourceId = sourceId,
                SourceType = sourceType,
                DestinationType = sourceType,
                Location = "Sofia",
                Reason = "Test",
                SuccessUrl = url,
                CancelUrl = url,
                ErrorUrl = url
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SignPdf(SignPdfResultViewModel model)
        {
            try
            {
                bool overrideSignEnabled = config.GetValue<bool>("Environment:GlobalAdmin:OverrideSign", false);
                (byte[] resultFile, string EGN) = await cdn.SignTools.EmbedPdfSignature(model.PdfId, model.Signature);

                if (!string.IsNullOrEmpty(model.SignerUic))
                {
                    if (!(userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator) && overrideSignEnabled))
                    {
                        if (string.Compare(model.SignerUic, EGN, StringComparison.InvariantCultureIgnoreCase) != 0)
                        {
                            SetErrorMessage($"Документът трябва да бъде подписан от {model.SignerName}.");
                            return LocalRedirect(model.ErrorUrl);
                        }
                    }
                }


                //MemoryStream msPdf = new MemoryStream(resultFile);
                //var signers = signTools.GetSignerInfo(msPdf);

                await cdn.MongoCdn_AppendUpdate(new CdnUploadRequest()
                {
                    SourceId = model.SourceId,
                    SourceType = model.SourceType,
                    ContentType = MediaTypeNames.Application.Pdf,
                    FileContentBase64 = Convert.ToBase64String(resultFile),
                    FileName = model.FileName,
                    Title = model.FileTitle
                });

                SetSuccessMessage("Документът е успешно подписан");

                return LocalRedirect(model.SuccessUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Process Sign PDF result error");
                SetErrorMessage("Възникна грешка при подписване на документа");

                return LocalRedirect(model.ErrorUrl);
            }
        }

        [HttpGet]
        public IActionResult CheckLSMErrorCode(int errorCode)
        {
            var errorMessage = cdn.SignTools.GetLSMErrorMessage(errorCode)?.Bg;

            return new JsonResult(new { errorCode, errorMessage });
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetFile(string pdfContentId)
        {
            var file = await cdn.MongoCdn_Download(pdfContentId);
            var contentDispositionHeader = new ContentDisposition
            {
                Inline = true,
                FileName = file.FileName
            };

            Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());

            return File(Convert.FromBase64String(file.FileContentBase64), file.ContentType);
        }
    }
}
