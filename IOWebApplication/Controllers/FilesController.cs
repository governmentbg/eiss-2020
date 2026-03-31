using IO.LogOperation.Models;
using IO.SignTools.Contracts;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;

namespace IOWebApplication.Controllers
{
    public class FilesController : BaseController
    {
        private readonly ICdnService cdnService;
        private readonly ICaseSessionActService actService;
        private readonly IDocumentResolutionService drService;
        private readonly ICaseSessionActCoordinationService coordinationService;
        private readonly IMQEpepService epepService;
        private readonly ICaseSessionFastDocumentService caseSessionFastDocumentService;
        private readonly IDocumentService documentService;
        private readonly IDocumentTemplateService documentTemplateService;
        private readonly IConfiguration config;
        private readonly ICaisBuletinService caisBuletinService;
        private readonly INomenclatureService nomenclatureService;
        private readonly ILogger<FilesController> logger;
        private readonly ILazybleService<IIOSignToolsService> lazyIOSignToolsService;

        public FilesController(
            ICdnService _cdnService,
            IMQEpepService _epepService,
            ICaseSessionActService _actService,
            ICaseSessionActCoordinationService _coordinationService,
            ICaseSessionFastDocumentService _caseSessionFastDocumentService,
            IDocumentService _documentService,
            IDocumentTemplateService _documentTemplateService,
            IDocumentResolutionService _drService,
            ICaisBuletinService _caisBuletinService,
            INomenclatureService _nomenclatureService,
            ILazybleService<IIOSignToolsService> _lazyIOSignToolsService,
            IConfiguration _config,
            ILogger<FilesController> _logger)
        {
            cdnService = _cdnService;
            epepService = _epepService;
            actService = _actService;
            drService = _drService;
            coordinationService = _coordinationService;
            caseSessionFastDocumentService = _caseSessionFastDocumentService;
            documentService = _documentService;
            documentTemplateService = _documentTemplateService;
            caisBuletinService = _caisBuletinService;
            nomenclatureService = _nomenclatureService;
            lazyIOSignToolsService = _lazyIOSignToolsService;
            config = _config;
            logger = _logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [DisableAudit]
        public async Task<IActionResult> GetFileList(int sourceType, string sourceID)
        {
            IEnumerable<CdnItemVM> model = await FileList(sourceType, sourceID);
            return Json(model);
        }
        public async Task<IEnumerable<CdnItemVM>> FileList(int sourceType, string sourceID)
        {
            IEnumerable<CdnItemVM> model;
            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    model = await fileList_Document(sourceID);
                    break;
                case SourceTypeSelectVM.ElectronicDocument:
                    model = await fileList_ElectronicDocument(sourceID);
                    break;
                case SourceTypeSelectVM.CaseSessionAct:
                    model = await fileList_CaseSessionAct(sourceID);
                    break;
                case SourceTypeSelectVM.CaseSessionActAllFiles:
                    model = await fileList_CaseSessionActAllFiles(sourceID);
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    model = await fileList_CaseNotification(sourceID);
                    break;
                case SourceTypeSelectVM.CaseSessionFastDocument:
                    model = await fileList_CaseSessionFastDocument(sourceID);
                    break;
                case SourceTypeSelectVM.DocumentDecision:
                    model = await fileList_DocumentDecision(sourceID);
                    break;
                case SourceTypeSelectVM.DocumentResolutionPdf:
                    model = await fileList_DocumentResolution(sourceID);
                    break;
                case SourceTypeSelectVM.CaseSessionDoc:
                    model = await fileList_CaseSessionDoc(sourceID);
                    break;
                case SourceTypeSelectVM.CasePersonBulletin:
                    model = await fileList_CasePersonBulletin(sourceID);
                    break;
                default:
                    model = await cdnService.Select(sourceType, sourceID).ToListAsync();
                    break;
            }
            return model.Where(x => x.DateExpired == null).ToList();
        }

        private async Task<IEnumerable<CdnItemVM>> fileList_CaseSessionDoc(string sourceID)
        {
            //Файл към съпровождащи документи, представени в заседание
            var caseSessionDoc = await caseSessionFastDocumentService.GetByIdAsync<CaseSessionDoc>(int.Parse(sourceID));

            var model = new List<CdnItemVM>();
            model.AddRange(await cdnService.Select(SourceTypeSelectVM.CaseSessionDoc, caseSessionDoc.Id.ToString()).ToListAsync());
            model.AddRange(await cdnService.Select(SourceTypeSelectVM.Document, caseSessionDoc.DocumentId.ToString()).ToListAsync());

            return model;
        }

        private async Task<IEnumerable<CdnItemVM>> fileList_CaseSessionFastDocument(string sourceID)
        {
            //Файл към съпровождащи документи, представени в заседание
            var caseSessionFastDocument = await caseSessionFastDocumentService.GetByIdAsync<CaseSessionFastDocument>(int.Parse(sourceID));
            var caseSessionFastDocuments = caseSessionFastDocumentService.CaseSessionFastDocument_SelectByInitId((caseSessionFastDocument.CaseSessionFastDocumentInitId != null ? (caseSessionFastDocument.CaseSessionFastDocumentInitId ?? 0) : caseSessionFastDocument.Id)).ToList();

            var model = new List<CdnItemVM>();

            foreach (var caseSessionFast in caseSessionFastDocuments)
            {
                model.AddRange(await cdnService.Select(SourceTypeSelectVM.CaseSessionFastDocument, caseSessionFast.Id.ToString()).ToListAsync());
            }

            return model;
        }

        private async Task<List<CdnItemVM>> fileList_Document(string sourceID)
        {
            long documentId = 0;
            try
            {
                documentId = long.Parse(sourceID);
            }
            catch (Exception ex) { }
            if (documentId == 0)
            {
                return new List<CdnItemVM>();
            }
            //Файл към акт/протокол
            var model = await cdnService.Select(SourceTypeSelectVM.Document, sourceID).ToListAsync();
            var pdfFiles = (await cdnService.Select(SourceTypeSelectVM.DocumentPdf, sourceID).ToListAsync()).SetCanDelete(false);
            var apiFiles = (await cdnService.Select(SourceTypeSelectVM.DocumentFromElectronicDocument, sourceID).ToListAsync()).SetCanDelete(false);
            model.AddRange(pdfFiles);
            model.AddRange(apiFiles);

            //Ако доумента има свързан DocumentTemplate, да вземе и всички файлове за обекта, за който е DocumentTemplate
            var documentTemplate = documentTemplateService.DocumentTemplate_SelectByDocumentId(long.Parse(sourceID));
            if (documentTemplate != null)
            {
                switch (documentTemplate.SourceType)
                {
                    case SourceTypeSelectVM.ExchangeDoc:
                        model.AddRange((await cdnService.Select(documentTemplate.SourceType, documentTemplate.SourceId.ToString()).ToListAsync()).SetCanDelete(false));
                        break;
                    default:
                        break;
                }
            }

            return model;
        }

        private async Task<IEnumerable<CdnItemVM>> fileList_ElectronicDocument(string sourceID)
        {
            //Файл към акт/протокол
            var model = (await cdnService.Select(SourceTypeSelectVM.ElectronicDocumentAllFiles, sourceID).ToListAsync()).SetCanDelete(false);

            return model;
        }

        private async Task<IEnumerable<CdnItemVM>> fileList_CaseSessionActAllFiles(string sourceID)
        {
            var cdnItemVMs = new List<CdnItemVM>();
            cdnItemVMs.AddRange(await fileList_CaseSessionAct(sourceID));
            cdnItemVMs.AddRange((await cdnService.Select(SourceTypeSelectVM.CaseSessionActManualUpload, sourceID).ToListAsync()).SetCanDelete(false));
            return cdnItemVMs;
        }

        private async Task<List<CdnItemVM>> fileList_CaseSessionAct(string sourceID)
        {
            //Файл към акт/протокол, обезличен акт, мотиви, обезличени мотиви
            int[] publicSourceTypes = new List<int>() {
                SourceTypeSelectVM.CaseSessionActDepersonalized,
                SourceTypeSelectVM.CaseSessionActMotiveDepersonalized,
                SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedPdf
            }.ToArray();

            int[] privateSourceTypes = new List<int>() {
                SourceTypeSelectVM.CaseSessionActPdf,
                SourceTypeSelectVM.CaseSessionActMotivePdf
            }.ToArray();

            var result = new List<CdnItemVM>();
            var canAccessPrivateFiles = await actService.CheckActPrivateFileAccess(int.Parse(sourceID));

            if (canAccessPrivateFiles)
            {
                result.AddRange((await cdnService.Select(privateSourceTypes, sourceID).ToListAsync()).SetCanDelete(false));
            }
            result.AddRange((await cdnService.Select(publicSourceTypes, sourceID).ToListAsync()).SetCanDelete(false));

            var coordinations = await coordinationService.CaseSessionActCoordination_Select(int.Parse(sourceID))
                    .Where(x => NomenclatureConstants.ActCoordinationTypes.WithOpinion.Contains(x.ActCoordinationTypeId))
                    .Select(x => x.Id)
                    .ToArrayAsync();

            foreach (var coordinationId in coordinations)
            {
                if (canAccessPrivateFiles)
                {
                    var coordinationFiles = (await cdnService.Select(SourceTypeSelectVM.CaseSessionActCoordinationPdf, coordinationId.ToString()).ToListAsync()).SetCanDelete(false);
                    //Добавя файловете към особените мнения
                    result.AddRange(coordinationFiles);
                }

                var coordinationDepersonalizedFiles = (await cdnService.Select(SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedPdf, coordinationId.ToString()).ToListAsync()).SetCanDelete(false);
                //Добавя файловете към особените мнения
                result.AddRange(coordinationDepersonalizedFiles);
            }
            return result;
        }

        private async Task<List<CdnItemVM>> fileList_DocumentResolution(string sourceID)
        {
            var result = new List<CdnItemVM>();

            if (drService.CheckActBlankAccess(false, int.Parse(sourceID)).canAccess)
            {
                result.AddRange((await cdnService.Select(SourceTypeSelectVM.DocumentResolutionPdf, sourceID).ToListAsync()).SetCanDelete(false));
            }
            return result;
        }

        private async Task<IEnumerable<CdnItemVM>> fileList_CaseNotification(string sourceID)
        {
            //Файл към акт/протокол, обезличен акт, мотиви, обезличени мотиви
            int[] sourceTypes = new List<int>() {
                SourceTypeSelectVM.CaseNotificationPrint,
                SourceTypeSelectVM.CaseNotificationReturn
            }.ToArray();
            var model = (await cdnService.Select(sourceTypes, sourceID).ToListAsync()).SetCanDelete(false);

            return model;
        }

        private async Task<IEnumerable<CdnItemVM>> fileList_CasePersonBulletin(string sourceID)
        {


            var model = new List<CdnItemVM>();
            //Стария файл на бюлетин, закачен към самия бюлетин
            model.AddRange(await cdnService.Select(SourceTypeSelectVM.CasePersonBulletin, sourceID).ToListAsync());

            //Зареждат се файловете само на подписаните версии на бюлетина + последния вариант (когато не е подписан)

            var buletinFiles = await caisBuletinService.SelectFiles(int.Parse(sourceID))
                                                    .OrderByDescending(x => x.Id)
                                                    .ToListAsync();


            int[] st = { SourceTypeSelectVM.CasePersonBulletinPdf };
            //Новите pdf файлове за преглед, към всеки един CasePersonSentenceBulletinFile при подписване

            foreach (var bFile in buletinFiles)
            {
                var files = await cdnService.Select(SourceTypeSelectVM.CasePersonBulletinPdf, bFile.Id.ToString()).ToListAsync();
                foreach (var file in files)
                {
                    file.Title = $"{file.Title} - {bFile.CaisStatus}";
                }
                model.AddRange(files);
            }

            return model;
        }

        public async Task<PartialViewResult> UploadFile(int sourceType, string sourceId, string container, string defaultTitle, string fileGroup = null)
        {
            CdnUploadRequest model = new CdnUploadRequest()
            {
                SourceType = sourceType,
                SourceId = sourceId,
                FileContainer = container,
                Title = defaultTitle
            };

            var maxFileSize = documentService.SystemParam_Select($"max_filesize_sourcetype_{sourceType}");
            if (maxFileSize != null)
            {
                try
                {
                    model.MaxFileSize = int.Parse(maxFileSize.ParamValue);
                }
                catch { }
            }

            var fileTypes = await nomenclatureService.GetDDL_MongoFileTypes(fileGroup);
            if (fileTypes.Count > 0)
            {
                ViewBag.MongoFileTypeId_ddl = fileTypes.AddAllItem().ToList();
            }

            model.FileUploadEnabled = config.GetValue<bool>("Environment:FileUploadEnabled");
            return PartialView(model);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFile(ICollection<IFormFile> files, CdnUploadRequest model)
        {
            if (files != null && files.Count() > 0)
            {
                string result = "failed";
                if (model.MaxFileSize > 0)
                {
                    long maxSize = (long)model.MaxFileSize * 1024 * 1024;
                    if (files.Any(x => x.Length > maxSize))
                    {
                        return Content("max_size");
                    }
                }

                if (model.MongoFileTypeId != null && model.MongoFileTypeId <= 0)
                {
                    return Content("inv_mt");
                }

                //Проверка за разширение на файлове
                string[] disabledFileExt = { "exe", "bin", "dll", "cab", "zip", "msi", "rar", "arj", "7z", "tar", "wim" };

                string[] whiteListExt = documentService.SystemParam_SelectStringValues(NomenclatureConstants.SystemParamName.FileUpload_IncludeExtentions);
                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLower();

                    if (!whiteListExt.Any())
                    {
                        if (disabledFileExt.Contains(ext) || string.IsNullOrEmpty(ext))
                        {
                            return Content("inv_ext");
                        }
                    }
                    else
                    {
                        if (!whiteListExt.Contains(ext))
                        {
                            return Content("inv_ext");
                        }
                    }
                }
                foreach (var file in files)
                {

                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        model.FileContentBase64 = Convert.ToBase64String(ms.ToArray());
                    }
                    model.ContentType = file.ContentType;
                    model.FileName = Path.GetFileName(file.FileName);

                    var response = await cdnService.MongoCdn_UploadFile(model);
                    if (response != null && response.Succeded)
                    {
                        model.FileId = response.FileId;
                        epepService.AppendFile(model, EpepConstants.ServiceMethod.Add);
                        LogFileOperation(model.SourceType, model.SourceId, $"Добавен нов файл {model.FileName}:{model.Title}", OperationTypes.Patch);

                        long _si = 0;
                        if (long.TryParse(model.SourceId, out _si))
                        {
                            var auditContext = await caseSessionFastDocumentService.GetCurrentContextAsync(model.SourceType, _si, AuditConstants.Operations.View);
                            AddAuditInfo(AuditConstants.Operations.Append, $"{auditContext.Info.BaseObject},{auditContext.Info.ObjectInfo}", model.FileName, SourceTypeSelectVM.Files);
                        }

                        if (model.SourceType == SourceTypeSelectVM.Document)
                        {
                            long documentId = long.Parse(model.SourceId);
                            var docCourtId = await documentService.GetPropByIdAsync<IOWebApplication.Infrastructure.Data.Models.Documents.Document, int>(x => x.Id == documentId, x => x.CourtId);
                            if (docCourtId == NomenclatureConstants.Courts.RandomAssignment)
                            {
                                var assignedDocuments = await documentService.GetAssignedDocumentList(documentId);


                                //При добавяне на файл към документ от Централна регистратура, той се копира във всички разпределени документи
                                foreach (var assDocId in assignedDocuments)
                                {
                                    model.SourceId = assDocId.ToString();
                                    var assResponse = await cdnService.MongoCdn_UploadFile(model);
                                    if (assResponse.Succeded)
                                    {
                                        epepService.AppendFile(model, EpepConstants.ServiceMethod.Add);
                                        LogFileOperation(model.SourceType, model.SourceId, $"Добавен нов файл {model.FileName}:{model.Title} в Централизирана Регистратура", OperationTypes.Patch);
                                    }
                                }

                            }
                        }

                        result = "ok";
                    }
                    else
                    {
                        if (response != null)
                        {
                            logger.LogError($"UploadFile Error from CDN: {response.ErrorMessage}");
                        }
                        result = "failed";
                        break;
                    }
                }
                return Content(result);
            }
            else
            {
                return Content("failed");
            }
        }

        private void LogFileOperation(int sourceType, string sourceId, string fileInfo, OperationTypes operation)
        {
            string controllerName = string.Empty;
            string actionName = string.Empty;
            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    controllerName = "document";
                    actionName = "edit";
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(controllerName))
            {
                SaveLogOperation(controllerName, actionName, fileInfo, operation, sourceId);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExpireFile(ExpiredInfoVM model)
        {
            if (!CheckSourceKey(SourceTypeSelectVM.ExpireObject, model.KeyString, $"ExpInfo{userContext?.UserId}"))
            {
                return SourceKeyExpireJsonError();
            }
            if (!model.IsValidDescription)
            {
                return SourceKeyExpireJsonErrorDescription();
            }

            if (!string.IsNullOrEmpty(model.StringId))
            {
                var fileInfo = await cdnService.Select(0, null, model.StringId).FirstOrDefaultAsync();
                model.Id = fileInfo.MongoFileId;
                if (epepService.SaveExpireInfo<MongoFile>(model))
                {
                    if (fileInfo.SourceType == SourceTypeSelectVM.Document)
                    {
                        epepService.AppendFile(new CdnUploadRequest()
                        {
                            FileId = fileInfo.FileId,
                            SourceType = fileInfo.SourceType,
                            SourceId = fileInfo.SourceId
                        }, EpepConstants.ServiceMethod.Delete);

                        var fileDeleteInfo = fileInfo.FileName;
                        var baseInfo = "";
                        var _context = CurrentContext;
                        if (_context.IsRead)
                        {
                            baseInfo = _context.Info.BaseObject;
                        }
                        AddAuditInfo(AuditConstants.Operations.Delete, baseInfo, $"{fileDeleteInfo} - {model.DescriptionExpired}", SourceTypeSelectVM.Files);
                        LogFileOperation(fileInfo.SourceType, fileInfo.SourceId, fileDeleteInfo, OperationTypes.Delete);
                    }
                    if (fileInfo.SourceType == SourceTypeSelectVM.CaseNotificationReturn)
                    {
                        var fileDeleteInfo = fileInfo.FileName;
                        var context = await drService.GetCurrentContextAsync(SourceTypeSelectVM.CaseNotification, fileInfo.SourceId.ToInt(), AuditConstants.Operations.Delete, null);
                        AddAuditInfo(AuditConstants.Operations.Delete, context?.Info?.ObjectInfo, $"{fileDeleteInfo} - {model.DescriptionExpired}", SourceTypeSelectVM.Files);
                        LogFileOperation(fileInfo.SourceType, fileInfo.SourceId, fileDeleteInfo, OperationTypes.Delete);
                    }
                    return Json(new { result = true, fileContainer = model.FileContainerName });
                }
                else
                {
                    return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
                }
            }
            else
            {
                return Json(new { result = false, message = "Няма избран файл." });
            }
        }


        public async Task<IActionResult> DeleteFile(string cdnFileId)
        {
            if (!string.IsNullOrEmpty(cdnFileId))
            {
                var fileInfo = await cdnService.Select(0, null, cdnFileId).FirstOrDefaultAsync();
                if (epepService.SaveExpireInfo<MongoFile>(new ExpiredInfoVM() { Id = fileInfo.MongoFileId }))
                {
                    epepService.AppendFile(new CdnUploadRequest()
                    {
                        FileId = fileInfo.FileId,
                        SourceType = fileInfo.SourceType,
                        SourceId = fileInfo.SourceId
                    }, EpepConstants.ServiceMethod.Delete);
                    LogFileOperation(fileInfo.SourceType, fileInfo.SourceId, $"Премахване на файл {fileInfo.FileName}:{fileInfo.Title}", OperationTypes.Patch);
                    long _si = 0;
                    if (long.TryParse(fileInfo.SourceId, out _si))
                    {
                        var auditContext = await caseSessionFastDocumentService.GetCurrentContextAsync(fileInfo.SourceType, _si, AuditConstants.Operations.View);
                        AddAuditInfo(AuditConstants.Operations.Delete, $"{auditContext.Info.BaseObject},{auditContext.Info.ObjectInfo}", fileInfo.FileName, SourceTypeSelectVM.Files);
                    }
                    return Content("ok");
                }
                else
                {
                    return Content("failed");
                }
            }
            else
            {
                return Content("failed");
            }
        }
        [AllowAnonymous]
        public async Task<IActionResult> Preview(string id)
        {
            var model = await cdnService.MongoCdn_Download(id, CdnFileSelect.PostProcess.Flatten);
            if (model == null)
            {
                return RedirectToAction(nameof(HomeController.Error), "Home", new { message = "Търсеният от Вас документ не е намерен или не е достъпен в момента" });
            }

            var contentDispositionHeader = new ContentDisposition()
            {
                Inline = true,
                FileName = HttpUtility.UrlPathEncode(model.FileName).Replace(",", "%2C")
            };
            if (model != null)
            {
                await auditInfoFiles(model.SourceType, model.SourceId);
            }

            Response.Headers.Append("Content-Disposition", contentDispositionHeader.ToString());

            byte[] fileBytes = Convert.FromBase64String(model.FileContentBase64);
            if (!string.IsNullOrEmpty(model.FileName))
                if (model.FileName.EndsWith("pdf", StringComparison.InvariantCultureIgnoreCase))
                {
                    model.ContentType = NomenclatureConstants.ContentTypes.Pdf;
                }

            return File(fileBytes, model.ContentType);
        }

        private async Task auditInfoFiles(int sourceType, string sourceId)
        {

            CurrentContextModel _context = null;
            switch (sourceType)
            {
                case SourceTypeSelectVM.CaseSessionActPdf:
                case SourceTypeSelectVM.CaseSessionActDepersonalized:
                    _context = await actService.GetCurrentContextAsync(SourceTypeSelectVM.CaseSessionAct, int.Parse(sourceId), AuditConstants.Operations.View, null);
                    break;
                case SourceTypeSelectVM.DocumentResolutionPdf:
                    _context = await actService.GetCurrentContextAsync(SourceTypeSelectVM.DocumentResolutionPdf, int.Parse(sourceId), AuditConstants.Operations.View, null);
                    break;
                default:
                    break;
            }
            if (_context != null)
            {
                AddAuditInfo(_context.Info.Operation, _context.Info.BaseObject, _context.Info.ObjectInfo, _context.Info.SourceType);
            }


        }



        [AllowAnonymous]
        public async Task<IActionResult> Download(string id)
        {
            var model = await cdnService.MongoCdn_Download(id);
            if (model == null)
            {
                return RedirectToAction(nameof(HomeController.Error), "Home", new { message = "Търсеният от Вас документ не е намерен или не е достъпен в момента" });
            }
            return File(Convert.FromBase64String(model.FileContentBase64), model.ContentType, model.FileName);
        }
        [AllowAnonymous]
        public async Task<FileResult> TestDL(int id)
        {
            var model = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSelectionProtokol, SourceId = id.ToString() }, CdnFileSelect.PostProcess.Flatten);
            return File(Convert.FromBase64String(model.FileContentBase64), model.ContentType, model.FileName);
        }

        public async Task<FileResult> DownloadST(int st, string si)
        {
            var model = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = st, SourceId = si });

            return File(Convert.FromBase64String(model.FileContentBase64), model.ContentType, model.FileName);
        }

        [AllowAnonymous]
        public async Task<FileResult> PreviewST(int st, string si)
        {
            var model = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = st, SourceId = si }, CdnFileSelect.PostProcess.Flatten);

            var contentDispositionHeader = new ContentDisposition
            {
                Inline = true,
                FileName = HttpUtility.UrlPathEncode(model.FileName).Replace(",", "%2C")
            };

            Response.Headers.Append("Content-Disposition", contentDispositionHeader.ToString());

            byte[] fileBytes = Convert.FromBase64String(model.FileContentBase64);


            return File(fileBytes, model.ContentType);
        }

        public async Task<PartialViewResult> FileListView(int sourceType, string sourceID)
        {
            var model = await FileList(sourceType, sourceID);
            return PartialView(model);
        }

        private async Task<List<CdnItemVM>> fileList_DocumentDecision(string sourceID)
        {
            var model = (await fileList_Document(sourceID)).SetCanDelete(false).ToList();

            var decision = documentService.DocumentDecision_SelectForDocument(long.Parse(sourceID));
            if (decision != null)
            {
                var decisionFiles = (await cdnService.Select(SourceTypeSelectVM.DocumentDecision, sourceID).ToListAsync()).SetCanDelete(true);
                model.AddRange(decisionFiles);
            }
            return model;
        }

        public async Task<IActionResult> GetFileListByCase(int caseId, int? sourceType = null)
        {
            if (!await CheckAccessAsync(actService, SourceTypeSelectVM.Case, caseId, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }
            var result = await actService.SelectPdfFilesBySourceType(caseId, sourceType);
            return Json(result.OrderByDescending(x => x.DateUploaded).Select(x => new SelectListItem
            {
                Value = x.FileId,
                Text = x.Title
            }));
        }

        public async Task<IActionResult> FileSignerInfo(string fileId)
        {
            List<FileSignerInfoVM> model = new();
            CdnDownloadResult fileInfo = null;
            try
            {
                fileInfo = await cdnService.MongoCdn_Download(fileId);

                byte[] fileContent = fileInfo.GetBytes();
                string fileName = fileInfo.FileName;

                var signtoolsService = lazyIOSignToolsService.Service;


                var _fileName = System.IO.Path.GetFileName(fileName).ToLower();
                var fileExt = System.IO.Path.GetExtension(_fileName).TrimStart('.');

                if (fileExt == "p7m")
                {
                    fileExt = "p7s";
                }
                var signerInfo = signtoolsService.GetSignerInfo(fileContent, fileExt);
                if (signerInfo != null)
                {
                    foreach (var signer in signerInfo)
                    {
                        model.Add(new FileSignerInfoVM()
                        {
                            Name = signer.Name,
                            Identifier = signer.Pid,
                            SignedOn = signer.SignedOn,
                            CertificateNumber = signer.CertificateNumber,
                            Issuer = signer.Issuer,
                            ValidTo = signer.ValidTo
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                model = null;
            }
            if (model == null || model.Count == 0)
            {
                if (fileInfo != null && SourceTypeSelectVM.DocumentEpepFileInfo.Contains(fileInfo.SourceType))
                {
                    var epepInfo = await epepService.GetEpepUserInfo(fileInfo);
                    if (epepInfo != null)
                    {
                        model = new List<FileSignerInfoVM>()
                        {
                            epepInfo
                        };
                    }
                }
            }
            return PartialView("_FileSignerInfo", model);
        }
    }
}
