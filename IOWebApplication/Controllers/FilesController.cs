using IO.LogOperation.Models;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<FilesController> logger;

        public FilesController(
            ICdnService _cdnService,
            IMQEpepService _epepService,
            ICaseSessionActService _actService,
            ICaseSessionActCoordinationService _coordinationService,
            ICaseSessionFastDocumentService _caseSessionFastDocumentService,
            IDocumentService _documentService,
            IDocumentTemplateService _documentTemplateService,
            IDocumentResolutionService _drService,
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
            config = _config;
            logger = _logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [DisableAudit]
        public IActionResult GetFileList(int sourceType, string sourceID)
        {
            List<CdnItemVM> model = FileList(sourceType, sourceID);
            return Json(model);
        }
        public List<CdnItemVM> FileList(int sourceType, string sourceID)
        {
            List<CdnItemVM> model;
            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    model = fileList_Document(sourceID);
                    break;
                case SourceTypeSelectVM.CaseSessionAct:
                    model = fileList_CaseSessionAct(sourceID);
                    break;
                case SourceTypeSelectVM.CaseSessionActAllFiles:
                    model = fileList_CaseSessionActAllFiles(sourceID);
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    model = fileList_CaseNotification(sourceID);
                    break;
                case SourceTypeSelectVM.CaseSessionFastDocument:
                    model = fileList_CaseSessionFastDocument(sourceID);
                    break;
                case SourceTypeSelectVM.DocumentDecision:
                    model = fileList_DocumentDecision(sourceID);
                    break;
                case SourceTypeSelectVM.DocumentResolutionPdf:
                    model = fileList_DocumentResolution(sourceID);
                    break;
                case SourceTypeSelectVM.CaseSessionDoc:
                    model = fileList_CaseSessionDoc(sourceID);
                    break;
                default:
                    model = cdnService.Select(sourceType, sourceID).ToList();
                    break;
            }
            return model.Where(x => x.DateExpired == null).ToList();
        }

        private List<CdnItemVM> fileList_CaseSessionDoc(string sourceID)
        {
            //Файл към съпровождащи документи, представени в заседание
            var caseSessionDoc = caseSessionFastDocumentService.GetById<CaseSessionDoc>(int.Parse(sourceID));

            var model = new List<CdnItemVM>();
            model.AddRange(cdnService.Select(SourceTypeSelectVM.CaseSessionDoc, caseSessionDoc.Id.ToString()).ToList());
            model.AddRange(cdnService.Select(SourceTypeSelectVM.Document, caseSessionDoc.DocumentId.ToString()).ToList());

            return model;
        }

        private List<CdnItemVM> fileList_CaseSessionFastDocument(string sourceID)
        {
            //Файл към съпровождащи документи, представени в заседание
            var caseSessionFastDocument = caseSessionFastDocumentService.GetById<CaseSessionFastDocument>(int.Parse(sourceID));
            var caseSessionFastDocuments = caseSessionFastDocumentService.CaseSessionFastDocument_SelectByInitId((caseSessionFastDocument.CaseSessionFastDocumentInitId != null ? (caseSessionFastDocument.CaseSessionFastDocumentInitId ?? 0) : caseSessionFastDocument.Id)).ToList();

            var model = new List<CdnItemVM>();

            foreach (var caseSessionFast in caseSessionFastDocuments)
            {
                model.AddRange(cdnService.Select(SourceTypeSelectVM.CaseSessionFastDocument, caseSessionFast.Id.ToString()).ToList());
            }

            return model;
        }

        private List<CdnItemVM> fileList_Document(string sourceID)
        {
            //Файл към акт/протокол
            var model = cdnService.Select(SourceTypeSelectVM.Document, sourceID).ToList();
            var pdfFiles = cdnService.Select(SourceTypeSelectVM.DocumentPdf, sourceID).ToList().SetCanDelete(false).ToList();
            var apiFiles = cdnService.Select(SourceTypeSelectVM.DocumentFileFromAPI, sourceID).ToList().SetCanDelete(false).ToList();
            model.AddRange(pdfFiles);
            model.AddRange(apiFiles);

            //Ако доумента има свързан DocumentTemplate, да вземе и всички файлове за обекта, за който е DocumentTemplate
            var documentTemplate = documentTemplateService.DocumentTemplate_SelectByDocumentId(long.Parse(sourceID));
            if (documentTemplate != null)
            {
                switch (documentTemplate.SourceType)
                {
                    case SourceTypeSelectVM.ExchangeDoc:
                        model.AddRange(cdnService.Select(documentTemplate.SourceType, documentTemplate.SourceId.ToString()).SetCanDelete(false).ToList());
                        break;
                    default:
                        break;
                }
            }

            return model;
        }

        private List<CdnItemVM> fileList_CaseSessionActAllFiles(string sourceID)
        {
            var cdnItemVMs = new List<CdnItemVM>();
            cdnItemVMs.AddRange(fileList_CaseSessionAct(sourceID));
            cdnItemVMs.AddRange(cdnService.Select(SourceTypeSelectVM.CaseSessionActManualUpload, sourceID).ToList().SetCanDelete(false).ToList());
            return cdnItemVMs;
        }

        private List<CdnItemVM> fileList_CaseSessionAct(string sourceID)
        {
            //Файл към акт/протокол, обезличен акт, мотиви, обезличени мотиви
            int[] publicSourceTypes = new List<int>() {
                SourceTypeSelectVM.CaseSessionActDepersonalized,
                SourceTypeSelectVM.CaseSessionActMotiveDepersonalized
            }.ToArray();

            int[] privateSourceTypes = new List<int>() {
                SourceTypeSelectVM.CaseSessionActPdf,
                SourceTypeSelectVM.CaseSessionActMotivePdf
            }.ToArray();

            var result = new List<CdnItemVM>();

            if (actService.CheckActPrivateFileAccess(int.Parse(sourceID)))
            {
                result.AddRange(cdnService.Select(privateSourceTypes, sourceID).ToList().SetCanDelete(false).ToList());
            }
            result.AddRange(cdnService.Select(publicSourceTypes, sourceID).ToList().SetCanDelete(false).ToList());

            if (actService.CheckActPrivateFileAccess(int.Parse(sourceID)))
            {
                var coordinations = coordinationService.CaseSessionActCoordination_Select(int.Parse(sourceID))
                        .Where(x => (x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.AcceptWithOpinion) || (x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.DontAccept)).ToList();
                foreach (var coordination in coordinations)
                {
                    var coordinationFiles = cdnService.Select(SourceTypeSelectVM.CaseSessionActCoordinationPdf, coordination.Id.ToString()).ToList().SetCanDelete(false);
                    //Добавя файловете към особените мнения
                    result.AddRange(coordinationFiles);
                }
            }
            return result;
        }

        private List<CdnItemVM> fileList_DocumentResolution(string sourceID)
        {
            var result = new List<CdnItemVM>();

            if (drService.CheckActBlankAccess(false, int.Parse(sourceID)).canAccess)
            {
                result.AddRange(cdnService.Select(SourceTypeSelectVM.DocumentResolutionPdf, sourceID).ToList().SetCanDelete(false).ToList());
            }
            return result;
        }

        private List<CdnItemVM> fileList_CaseNotification(string sourceID)
        {
            //Файл към акт/протокол, обезличен акт, мотиви, обезличени мотиви
            int[] sourceTypes = new List<int>() {
                SourceTypeSelectVM.CaseNotificationPrint,
                SourceTypeSelectVM.CaseNotificationReturn
            }.ToArray();
            var model = cdnService.Select(sourceTypes, sourceID).SetCanDelete(false).ToList();

            return model;
        }

        public PartialViewResult UploadFile(int sourceType, string sourceId, string container, string defaultTitle)
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
        public IActionResult ExpireFile(ExpiredInfoVM model)
        {
            if (!string.IsNullOrEmpty(model.StringId))
            {
                var fileInfo = cdnService.Select(0, null, model.StringId).FirstOrDefault();
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

                        var fileDeleteInfo = $"Премахване на файл {fileInfo.FileName}";
                        var baseInfo = "";
                        var _context = CurrentContext;
                        if (_context.IsRead)
                        {
                            baseInfo = _context.Info.BaseObject;
                        }
                        AddAuditInfo(AuditConstants.Operations.Delete, baseInfo, $"{fileDeleteInfo} - {model.DescriptionExpired}");
                        LogFileOperation(fileInfo.SourceType, fileInfo.SourceId, fileDeleteInfo, OperationTypes.Patch);
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
            //var actModel = service.GetById<CaseSessionAct>(model.Id);
            //if (service.SaveExpireInfo<CaseSessionAct>(model))
            //{
            //    taskService.ExpireAllUnfinishedTasks(SourceTypeSelectVM.CaseSessionAct, model.Id);
            //    SetAuditContextDelete(service, SourceTypeSelectVM.CaseSessionAct, model.Id);
            //    SetSuccessMessage(MessageConstant.Values.ActExpireOK);
            //    return Json(new { result = true, redirectUrl = Url.Action("Preview", "CaseSession", new { id = actModel.CaseSessionId }) });
            //}
            //else
            //{
            //    return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            //}
        }


        public IActionResult DeleteFile(string cdnFileId)
        {
            if (!string.IsNullOrEmpty(cdnFileId))
            {
                var fileInfo = cdnService.Select(0, null, cdnFileId).FirstOrDefault();
                if (epepService.SaveExpireInfo<MongoFile>(new ExpiredInfoVM() { Id = fileInfo.MongoFileId }))
                {
                    epepService.AppendFile(new CdnUploadRequest()
                    {
                        FileId = fileInfo.FileId,
                        SourceType = fileInfo.SourceType,
                        SourceId = fileInfo.SourceId
                    }, EpepConstants.ServiceMethod.Delete);
                    LogFileOperation(fileInfo.SourceType, fileInfo.SourceId, $"Премахване на файл {fileInfo.FileName}:{fileInfo.Title}", OperationTypes.Patch);

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
        public async Task<FileResult> Preview(string id)
        {
            var model = await cdnService.MongoCdn_Download(id, CdnFileSelect.PostProcess.Flatten);

            var contentDispositionHeader = new ContentDisposition
            {
                Inline = true,
                FileName = HttpUtility.UrlPathEncode(model.FileName).Replace(",", "%2C")
            };

            Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());

            byte[] fileBytes = Convert.FromBase64String(model.FileContentBase64);


            return File(fileBytes, model.ContentType);
        }



        [AllowAnonymous]
        public async Task<FileResult> Download(string id)
        {
            var model = await cdnService.MongoCdn_Download(id);
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

            Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());

            byte[] fileBytes = Convert.FromBase64String(model.FileContentBase64);


            return File(fileBytes, model.ContentType);
        }

        public PartialViewResult FileListView(int sourceType, string sourceID)
        {
            var model = FileList(sourceType, sourceID);
            return PartialView(model);
        }

        private List<CdnItemVM> fileList_DocumentDecision(string sourceID)
        {
            var model = cdnService.Select(SourceTypeSelectVM.Document, sourceID).SetCanDelete(false).ToList();

            var decision = documentService.DocumentDecision_SelectForDocument(long.Parse(sourceID));
            if (decision != null)
            {
                var decisionFiles = cdnService.Select(SourceTypeSelectVM.DocumentDecision, sourceID).SetCanDelete(true).ToList();
                model.AddRange(decisionFiles);
            }
            return model;
        }
    }
}
