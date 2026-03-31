using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    public class SignPdfComponent : ViewComponent
    {
        private readonly ILogger logger;
        private readonly ICdnService cdn;
        private readonly IWorkTaskService taskService;
        private readonly IUserContext userContext;

        public SignPdfComponent(
            ICdnService cdn,
            ILogger<SignPdfComponent> logger,
            IWorkTaskService taskService,
            IUserContext userContext
            )
        {
            this.logger = logger;
            this.cdn = cdn;
            this.taskService = taskService;
            this.userContext = userContext;
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

            if (info.WorkTaskId > 0 && userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {
                var taskModel = await taskService.GetReadonlyAsync<WorkTask>(info.WorkTaskId);
                switch (taskModel.SourceType)
                {
                    case SourceTypeSelectVM.CaseSessionAct:
                        var signInfo = await taskService.CheckCompletedTasks(taskModel);
                        if (!signInfo.HasUncompleteTasks)
                        {
                            ViewBag.signComfirmMessage = await taskService.MakeSignComfirmMessage(taskModel);
                        }
                        break;
                    default:
                        break;
                }
            }

            try
            {
                var pdf = await cdn.MongoCdn_Download(new CdnFileSelect() { FileId = info.FileId, SourceId = info.SourceId, SourceType = info.SourceType });
                if (pdf == null)
                {
                    model.ErrorMessage = "Невалиден файл за подписване";
                    return await Task.FromResult<IViewComponentResult>(View("Error", model));
                }
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(pdf.FileContentBase64)))
                {
                    var (hash, tempPdfId) = await cdn.SignTools.GetPdfHash(ms, info.Reason, info.Location);

                    model.PreviewPdfUrl = Url.Action("GetFile", "SignDocument", new { pdfContentId = pdf.FileId });
                    model.TempFileId = tempPdfId;
                    model.FileHash = hash;
                    model.FileName = pdf.FileName;
                    model.FileTitle = pdf.FileTitle;
                    model.FileId = pdf.FileId;
                    model.SignituresCount = pdf.SignituresCount ?? 0;
                }
            }
            catch (ArgumentException aex)
            {
                logger.LogError(aex, "SignPdf Error");
                model.ErrorMessage = "Невалиден файл за подписване";

                return await Task.FromResult<IViewComponentResult>(View("Error", model));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SignPdf Error");
                return await Task.FromResult<IViewComponentResult>(View("Error", model));
            }

            return await Task.FromResult<IViewComponentResult>(View(viewName, model));
        }
    }
}
