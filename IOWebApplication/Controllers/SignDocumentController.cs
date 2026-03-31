//using AutoMapper.Configuration;
using IO.SignTools.Models;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IOWebApplication.Controllers
{
    public class SignDocumentController : BaseController
    {
        private readonly ILogger logger;
        private readonly ICdnService cdn;
        private readonly IConfiguration config;
        private readonly IWorkTaskService taskService;
        private readonly ICourtStampCertificateService stamptService;

        public SignDocumentController(
            ILogger<SignDocumentController> _logger,
            ICdnService _cdn,
            IWorkTaskService _taskService,
            ICourtStampCertificateService _stamptService,
            IConfiguration _config)
        {
            logger = _logger;
            cdn = _cdn;
            config = _config;
            taskService = _taskService;
            stamptService = _stamptService;
        }

        /*
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
            AddAuditInfo(AuditConstants.Operations.View, "Подписване на документ", "", "Подписване");
            return View(model);
        }
        */

        private async Task<SaveResultVM> verifySign(SignPdfViewModel model)
        {


            if (model.WorkTaskId > 0)
            {

                var workTask = await taskService.GetReadonlyAsync<Infrastructure.Data.Models.Common.WorkTask>(model.WorkTaskId);
                if (workTask != null)
                {
                    if (workTask.TaskStateId == WorkTaskConstants.States.Deleted)
                    {
                        return new SaveResultVM(false, "Задача за подписване е отменена.");
                    }
                    if (workTask.TaskStateId == WorkTaskConstants.States.Redirected)
                    {
                        return new SaveResultVM(false, "Задача за подписване е пренасочена.");
                    }
                    if (workTask.DateCompleted != null)
                    {
                        return new SaveResultVM(false, "Задача за подписване вече е изпълнена.");
                    }
                }
            }

            var fileItem = await cdn.Select(model.SourceType, model.SourceId).FirstOrDefaultAsync();
            if (fileItem.FileId != model.FileId)
            {
                return new SaveResultVM(false, "Заредения от вас документ е актуализиран/подписан от друг потребител. Моля, повторете стъпката по подписване, за да заредите актуалното му съдържание.");
            }

            //Проверки на основен обект към файла преди запис на подписаното съдържание
            switch (fileItem.SourceType)
            {
                case SourceTypeSelectVM.CaseSelectionProtokol:
                    {
                        if (fileItem.SignituresCount > 0)
                        {
                            return new SaveResultVM(false, "Заредения от вас протокол вече е подписан!");
                        }
                        int sourceId = int.Parse(fileItem.SourceId);
                        int protocolStateId = await taskService.GetPropByIdAsync<CaseSelectionProtokol, int>(x => x.Id == sourceId, x => x.SelectionProtokolStateId);
                        if (protocolStateId == NomenclatureConstants.SelectionProtokolState.Signed)
                        {
                            return new SaveResultVM(false, "Заредения от вас протокол вече е подписан!");
                        }
                    }
                    break;
            }

            var ts = Math.Abs(model.ClientCode - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            if (ts > (double)600000)
            {
                return new SaveResultVM(false, "Моля, сверете часовника на вашия компютър.");
            }
            if (model.WorkTaskId > 0)
            {
                var checkResult = await taskService.UpdateBeforeCompleteTask(model.WorkTaskId);
                if (!checkResult.Result)
                {
                    return new SaveResultVM(false, checkResult.ErrorMessage);
                }
            }
            return new SaveResultVM(true);
        }

        private async Task<SaveResultVM> saveSignedContent(CdnUploadRequest uploadRequest, SignPdfViewModel model, string methodType)
        {
            SaveResultVM saveResult = new SaveResultVM(false);
            using (var fileTansaction = taskService.BeginTransaction())
            {
                var prepareResult = await PrepareBeforeSave(uploadRequest, model);
                if (!prepareResult.Result)
                {
                    return prepareResult;
                }

                if (await cdn.MongoCdn_AppendUpdate(uploadRequest))
                {
                    saveResult.Result = true;
                    if (model.WorkTaskId > 0)
                    {
                        try
                        {
                            var taskCompleteResult = await taskService.CompleteTask(model.WorkTaskId);
                            if (!taskCompleteResult)
                            {
                                logger.LogError($"Грешка при приключване на задача {model.WorkTaskId}, PdfController.Sign");
                                saveResult.Result = false;
                                saveResult.ErrorMessage = "Грешка при приключване на задача";
                            }
                            else
                            {
                                var task = await taskService.GetReadonlyAsync<Infrastructure.Data.Models.Common.WorkTask>(model.WorkTaskId);
                                if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.FearProtectsVineyard))
                                {

                                    if (taskService.StopTrackingApplicationUser())
                                    {
                                        logger.LogError($"StopTrackingApplicationUser-{methodType}.Sign TaskId={model.WorkTaskId};SourceId={model.SourceId};SourceType={model.SourceType};FileName:{model.FileName}");
                                    }
                                }
                                saveResult = await taskService.UpdateAfterCompleteTask(task);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"{methodType}.Sign TaskId={model.WorkTaskId};SourceId={model.SourceId};SourceType={model.SourceType};FileName:{model.FileName}");
                            saveResult.Result = false;
                            saveResult.ErrorMessage = "Грешка при приключване на задача за подпис";
                        }
                    }

                    if (saveResult.Result)
                    {
                        fileTansaction.Commit();
                    }
                }
                else
                {
                    logger.LogError($"Грешка при запис на подписан документ. SourceType={model.SourceType}; SourceId={model.SourceId}");
                    saveResult.ErrorMessage = "Грешка при запис на подписан документ";
                }
            }
            return saveResult;
        }

        /// <summary>
        /// Регистриране на документ преди последния печат
        /// </summary>
        /// <param name="uploadRequest"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<SaveResultVM> PrepareBeforeSave(CdnUploadRequest uploadRequest, SignPdfViewModel model)
        {
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request1_2024))
            {
                return new SaveResultVM(true);
            }
            if (uploadRequest.SourceType == SourceTypeSelectVM.TestSignPDF)
            {
                return await prepareBeforeSaveTestSign(uploadRequest, model);
            }
            if (model.WorkTaskId == 0)
            {
                return new SaveResultVM(true);
            }
            var taskModel = await taskService.GetReadonlyAsync<WorkTask>(model.WorkTaskId);
            var signTasks = await taskService.CheckCompletedTasks(taskModel);
            if (signTasks.HasUncompleteTasks)
            {
                return new SaveResultVM(true);
            }
            bool forStamp = false;
            string stampContent = string.Empty;
            int blankMode = 0;
            bool wideStamp = false;
            string newFileName = string.Empty;
            string newFileTitle = string.Empty;


            switch (taskModel.SourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    {
                        var actModel = await taskService.ReadByIdAsync<CaseSessionAct>((int)taskModel.SourceId);
                        if (actModel != null)
                        {
                            if (actModel.RegDate == null)
                            {
                                //Регистриране на акта, ако няма номер
                                ICaseSessionActService actService = (ICaseSessionActService)HttpContext.RequestServices.GetService(typeof(ICaseSessionActService));
                                ICaseSessionActCoordinationService coordinationService = (ICaseSessionActCoordinationService)HttpContext.RequestServices.GetService(typeof(ICaseSessionActCoordinationService));

                                var regResult = await actService.CaseSessionAct_RegisterAct(actModel, "PrepareBeforeSave");
                                if (!regResult.Result)
                                {
                                    return new SaveResultVM(false, "Проблем при регистриране на акт");
                                }

                                var actTypeName = await taskService.GetPropByIdAsync<ActType, string>(actModel.ActTypeId, x => x.Label);

                                newFileName = $"{actTypeName} {actModel.RegNumber}-{actModel.RegDate:dd.MM.yyyy}.pdf";
                                newFileTitle = $"{actTypeName} {actModel.RegNumber}/{actModel.RegDate:dd.MM.yyyy}";

                                var withCoordinations = await coordinationService.CaseSessionActCoordination_Select(actModel.Id, null, NomenclatureConstants.CoordinationTypes.Act)
                                    .AnyAsync(x => NomenclatureConstants.ActCoordinationTypes.WithOpinion.Contains(x.ActCoordinationTypeId));

                                stampContent = $"Рег.№ {actModel.RegNumber} / {actModel.RegDate:dd.MM.yyyy}";
                                if (withCoordinations)
                                {
                                    stampContent += $" с особено мнение";
                                    wideStamp = true;
                                }
                                blankMode = actModel.ActTypeId;
                                forStamp = true;

                                //Запазване на реда на стариншство преди първото постановяване на акта
                                await actService.LawUnit_SaveOrderBy(actModel.Id);
                            }
                        }
                    }
                    break;
                case SourceTypeSelectVM.DocumentResolution:
                    {
                        var resolutionModel = await taskService.ReadByIdAsync<DocumentResolution>((int)taskModel.SourceId);
                        if (resolutionModel != null)
                        {
                            if (resolutionModel.RegDate == null)
                            {
                                //Регистриране на разпореждането, ако няма номер
                                IDocumentResolutionService documentResolutionService = (IDocumentResolutionService)HttpContext.RequestServices.GetService(typeof(IDocumentResolutionService));

                                var regResult = documentResolutionService.Register(resolutionModel);
                                if (!regResult.Result)
                                {
                                    return new SaveResultVM(false, "Проблем при регистриране на разпореждане");
                                }
                                stampContent = $"Рег.№ {resolutionModel.RegNumber} / {resolutionModel.RegDate:dd.MM.yyyy}";
                                blankMode = resolutionModel.ResolutionTypeId;

                                var resolutionTypeName = await taskService.GetPropByIdAsync<ResolutionType, string>(resolutionModel.ResolutionTypeId, x => x.Label);
                                newFileName = $"{resolutionTypeName} {resolutionModel.RegNumber}-{resolutionModel.RegDate:dd.MM.yyyy}.pdf";
                                newFileTitle = $"{resolutionTypeName} {resolutionModel.RegNumber}/{resolutionModel.RegDate:dd.MM.yyyy}";

                                forStamp = true;
                            }
                        }
                    }
                    break;
            }

            if (forStamp)
            {
                var stampResult = await stamptService.Stamp(new CourtStampRequestVM()
                {
                    PdfContent = uploadRequest.FileContent,
                    StampContent = stampContent,
                    SourceType = taskModel.SourceType,
                    BlankMode = blankMode,
                    WideStamp = wideStamp
                });

                if (!stampResult.Result)
                {
                    return new SaveResultVM(false, stampResult.StampError);
                }

                uploadRequest.FileContent = stampResult.StampedPdfContent;
                uploadRequest.FileContentBase64 = Convert.ToBase64String(uploadRequest.FileContent);
                uploadRequest.FileName = newFileName;
                uploadRequest.Title = newFileTitle;
            }

            return new SaveResultVM(true);
        }

        private async Task<SaveResultVM> prepareBeforeSaveTestSign(CdnUploadRequest uploadRequest, SignPdfViewModel model)
        {
            var stampResult = await stamptService.Stamp(new CourtStampRequestVM()
            {
                PdfContent = uploadRequest.FileContent,
                StampContent = DateTime.Now.ToString("Печат: dd.MM.yyyy"),
                SourceType = 0,
                BlankMode = 0,
                WideStamp = false
            });

            if (!stampResult.Result)
            {
                return new SaveResultVM(true);
            }

            uploadRequest.FileContent = stampResult.StampedPdfContent;
            uploadRequest.FileContentBase64 = Convert.ToBase64String(uploadRequest.FileContent);

            return new SaveResultVM(true);
        }

        [HttpPost]
        public async Task<IActionResult> SignPdf(SignPdfViewModel model)
        {
            try
            {
                (byte[] signedPdfFile, string signerEGN) = await cdn.SignTools.EmbedPdfSignature(model.TempFileId, model.Signature);

                if (!string.IsNullOrEmpty(model.SignerUic))
                {
                    bool overrideSignEnabled = config.GetValue<bool>("Environment:GlobalAdmin:OverrideSign", false);
                    if (!(userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator) && overrideSignEnabled))
                    {
                        if (string.Compare(model.SignerUic, signerEGN, StringComparison.InvariantCultureIgnoreCase) != 0)
                        {
                            SetErrorMessage($"Документът трябва да бъде подписан от {model.SignerName}.");
                            return LocalRedirect(model.ErrorUrl);
                        }
                    }
                }
                var verifyResult = await verifySign(model);
                if (!verifyResult.Result)
                {
                    SetErrorMessage(verifyResult.ErrorMessage);
                    return LocalRedirect(model.ErrorUrl);
                }

                if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.FearProtectsVineyard))
                {
                    try
                    {
                        taskService.ClearEntityTracker();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "ClearEntityTrackerError");
                    }
                }


                var uploadRequest = new CdnUploadRequest()
                {
                    SourceId = model.SourceId,
                    SourceType = model.SourceType,
                    ContentType = MediaTypeNames.Application.Pdf,
                    FileContent = signedPdfFile,
                    FileContentBase64 = Convert.ToBase64String(signedPdfFile),
                    FileName = model.FileName,
                    Title = model.FileTitle,
                    SignituresCount = model.SignituresCount + 1
                };

                var saveResult = await saveSignedContent(uploadRequest, model, "SignPdf");
                if (saveResult.Result)
                {
                    AddAuditInfo(AuditConstants.Operations.Update, "Подписване на документ", model.FileTitle, "Подписване");
                    SetSuccessMessage("Документът е успешно подписан");
                    return LocalRedirect(model.SuccessUrl);
                }
                else
                {
                    logger.LogError($"Грешка при запис на подписан документ. SourceType={model.SourceType}; SourceId={model.SourceId}");
                    SetErrorMessage("Грешка при запис на подписан документ");
                    return LocalRedirect(model.ErrorUrl);
                }


                //using (var fileTansaction = await taskService.BeginTransactionAsync())
                //{
                //    if (await cdn.MongoCdn_AppendUpdate(new CdnUploadRequest()
                //    {
                //        SourceId = model.SourceId,
                //        SourceType = model.SourceType,
                //        ContentType = MediaTypeNames.Application.Pdf,
                //        FileContentBase64 = Convert.ToBase64String(signedPdfFile),
                //        FileName = model.FileName,
                //        Title = model.FileTitle,
                //        SignituresCount = model.SignituresCount + 1
                //    }))
                //    {
                //        SaveResultVM saveResult = new SaveResultVM(true);
                //        if (model.WorkTaskId > 0)
                //        {
                //            try
                //            {
                //                if (!(await taskService.CompleteTask(model.WorkTaskId)))
                //                {
                //                    logger.LogError($"Грешка при приключване на задача {model.WorkTaskId}, PdfController.Sign");
                //                    saveResult.Result = false;
                //                    saveResult.ErrorMessage = "Грешка при приключване на задача";
                //                }
                //                else
                //                {
                //                    var task = await taskService.GetReadonlyAsync<Infrastructure.Data.Models.Common.WorkTask>(model.WorkTaskId);
                //                    if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.FearProtectsVineyard))
                //                    {

                //                        if (taskService.StopTrackingApplicationUser())
                //                        {
                //                            logger.LogError($"StopTrackingApplicationUser-Pdf.Sign TaskId={model.WorkTaskId};SourceId={model.SourceId};SourceType={model.SourceType};FileName:{model.FileName}");
                //                        }
                //                    }
                //                    saveResult = await taskService.UpdateAfterCompleteTask(task);
                //                }
                //            }
                //            catch (Exception ex)
                //            {
                //                logger.LogError(ex, $"Pdf.Sign TaskId={model.WorkTaskId};SourceId={model.SourceId};SourceType={model.SourceType};FileName:{model.FileName}");
                //                saveResult.Result = false;
                //                saveResult.ErrorMessage = "Грешка при приключване на задача за подпис";
                //            }
                //        }

                //        if (saveResult.Result)
                //        {
                //            fileTansaction.Commit();
                //            AddAuditInfo(AuditConstants.Operations.Update, "Подписване на документ", model.FileTitle, "Подписване");
                //            SetSuccessMessage("Документът е успешно подписан");
                //        }
                //        else
                //        {
                //            SetErrorMessage(saveResult.ErrorMessage);
                //        }
                //    }
                //    else
                //    {
                //        logger.LogError($"Грешка при запис на подписан документ. SourceType={model.SourceType}; SourceId={model.SourceId}");
                //        SetErrorMessage("Грешка при запис на подписан документ");
                //    }
                //}


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Process Sign PDF result error");
                var signatureException = ex as SignatureValidationException;

                if (signatureException != null &&
                    signatureException.Message == "Signature time is invalid")
                {
                    SetErrorMessage("Моля, сверете часовника на вашия компютър.");
                }
                else
                {
                    SetErrorMessage("Възникна грешка при подписване на документа");
                    TempData["signError"] = ex.Message;
                }

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
            if (file == null)
            {
                var contentDispositionHeaderError = new ContentDisposition
                {
                    Inline = true,
                    FileName = "notFound.html"
                };
                string errorFileContent = @"<html><body><h3>Избрания документ e невалиден или вече е променен.</h3><h4>Моля, повторете процеса по подписване.</h4></body></htm>";

                Response.Headers.Append(new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("Content-Disposition", contentDispositionHeaderError.ToString()));

                return File(System.Text.Encoding.UTF8.GetBytes(errorFileContent), "text/html");
            }
            var contentDispositionHeader = new ContentDisposition
            {
                Inline = true,
                FileName = HttpUtility.UrlPathEncode(file.FileName).Replace(",", "%2C")
            };

            Response.Headers.Append(new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("Content-Disposition", contentDispositionHeader.ToString()));

            return File(Convert.FromBase64String(file.FileContentBase64), file.ContentType);
        }

        [HttpPost]
        public async Task<IActionResult> SignXml(SignPdfViewModel model)
        {
            try
            {
                var signedXmlFile = HttpUtility.HtmlDecode(model.Signature ?? "");

                if (string.IsNullOrEmpty(signedXmlFile))
                {
                    SetErrorMessage($"Невалиден XML файл.");
                    return LocalRedirect(model.ErrorUrl);
                }

                var signerEGN = model.SignerUic;
                //TODO: Да се проверява реалния човек от подписа

                if (!string.IsNullOrEmpty(model.SignerUic))
                {
                    bool overrideSignEnabled = config.GetValue<bool>("Environment:GlobalAdmin:OverrideSign", false);
                    if (!(userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator) && overrideSignEnabled))
                    {
                        if (string.Compare(model.SignerUic, signerEGN, StringComparison.InvariantCultureIgnoreCase) != 0)
                        {
                            SetErrorMessage($"Документът трябва да бъде подписан от {model.SignerName}.");
                            return LocalRedirect(model.ErrorUrl);
                        }
                    }
                }
                var verifyResult = await verifySign(model);
                if (!verifyResult.Result)
                {
                    SetErrorMessage(verifyResult.ErrorMessage);
                    return LocalRedirect(model.ErrorUrl);
                }

                if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.FearProtectsVineyard))
                {
                    try
                    {
                        taskService.ClearEntityTracker();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "ClearEntityTrackerError");
                    }
                }

                var uploadRequest = new CdnUploadRequest()
                {
                    SourceId = model.SourceId,
                    SourceType = model.SourceType,
                    ContentType = MediaTypeNames.Application.Xml,
                    FileContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXmlFile)),
                    FileName = model.FileName,
                    Title = model.FileTitle,
                    SignituresCount = model.SignituresCount + 1
                };

                var saveResult = await saveSignedContent(uploadRequest, model, "SignXml");
                if (saveResult.Result)
                {
                    AddAuditInfo(AuditConstants.Operations.Update, "Подписване на документ", model.FileTitle, "Подписване");
                    SetSuccessMessage("Документът е успешно подписан");
                    return LocalRedirect(model.SuccessUrl);
                }
                else
                {
                    logger.LogError($"Грешка при запис на подписан документ. SourceType={model.SourceType}; SourceId={model.SourceId}");
                    SetErrorMessage("Грешка при запис на подписан документ");
                    return LocalRedirect(model.ErrorUrl);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Process Sign Xml result error");
                var signatureException = ex as SignatureValidationException;

                if (signatureException != null &&
                    signatureException.Message == "Signature time is invalid")
                {
                    SetErrorMessage("Моля, сверете часовника на вашия компютър.");
                }
                else
                {
                    SetErrorMessage("Възникна грешка при подписване на документа");
                    TempData["signError"] = ex.Message;
                }

                return LocalRedirect(model.ErrorUrl);
            }
        }
    }
}
