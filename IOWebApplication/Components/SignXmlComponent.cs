using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    public class SignXmlComponent : ViewComponent
    {
        private readonly ILogger logger;
        private readonly ICdnService cdn;

        public SignXmlComponent(
            ICdnService cdn,
            ILogger<SignPdfComponent> logger
            )
        {
            this.logger = logger;
            this.cdn = cdn;
        }

        public async Task<IViewComponentResult> InvokeAsync(SignPdfInfo info, string viewName = "")
        {
            SignPdfViewModel model = new SignPdfViewModel()
            {
                SuccessUrl = info.SuccessUrl.ToString(),
                ErrorUrl = info.ErrorUrl.ToString(),
                CancelUrl = info.CancelUrl.ToString(),
                SourceId = info.SourceId,
                SourceType = info.DestinationType,
                SignerName = info.SignerName,
                SignerUic = info.SignerUic,
                WorkTaskId = info.WorkTaskId
            };

            try
            {
                var xml = await cdn.MongoCdn_Download(new CdnFileSelect() { FileId = info.FileId, SourceId = info.SourceId, SourceType = info.SourceType });
                if (xml == null)
                {
                    model.ErrorMessage = "Невалиден файл за подписване";
                    return await Task.FromResult<IViewComponentResult>(View("Error", model));
                }

                var xmlData = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(xml.FileContentBase64));

                model.PreviewPdfUrl = Url.Action("GetFile", "SignDocument", new { pdfContentId = info.PreviewFileId });
                model.FileHash = xmlData;
                model.FileName = xml.FileName;
                model.FileTitle = xml.FileTitle;
                model.FileId = xml.FileId;
                model.SignituresCount = xml.SignituresCount ?? 0;

            }
            catch (ArgumentException aex)
            {
                logger.LogError(aex, "SignXml Error");
                model.ErrorMessage = "Невалиден файл за подписване";

                return await Task.FromResult<IViewComponentResult>(View("Error", model));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SignXml Error");
                return await Task.FromResult<IViewComponentResult>(View("Error", model));
            }

            return await Task.FromResult<IViewComponentResult>(View(viewName, model));
        }
    }
}