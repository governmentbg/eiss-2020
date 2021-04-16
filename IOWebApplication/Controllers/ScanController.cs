using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using IO.LogOperation.Models;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class ScanController : BaseController
    {
        private readonly ICdnService cdnService;

        public ScanController(ICdnService _cdnService)
        {
            cdnService = _cdnService;
        }

        [HttpGet]
        public IActionResult Scan(string returnUrl, string sourceId, int sourceType)
        {
            var info = new ScanInfoViewModel()
            {
                ReturnUrl = new Uri(returnUrl),
                SourceId = sourceId,
                SourceType = sourceType
            };

            return View(info);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Scan(ScanInfoViewModel model)
        {
            if (string.IsNullOrEmpty(model.FileName))
            {
                model.FileName = $"ScannedDocument{DateTime.Now.ToString("ddMMyyHHmmss")}.pdf";
            } 
            else if(!model.FileName.EndsWith(".pdf"))
            {
                model.FileName += ".pdf";
            }

            CdnUploadRequest fileModel = new CdnUploadRequest()
            {
                ContentType = MediaTypeNames.Application.Pdf,
                FileContentBase64 = model.FileContent,
                FileName = model.FileName,
                SourceId = model.SourceId,
                SourceType = model.SourceType,
                Title = model.Title,
                UserUploaded = userContext.UserId
            };

            var response = await cdnService.MongoCdn_UploadFile(fileModel);
            if (response != null && response.Succeded)
            {
                fileModel.FileId = response.FileId;
                SaveLogOperation("Scan", "Scan", $"Добавен нов файл {model.FileName}:{model.Title}", OperationTypes.Patch, fileModel.SourceId);

                return Redirect(model.ReturnUrl.AbsoluteUri);
            }
            else
            {
                TempData[MessageConstant.ErrorMessage] = MessageConstant.Values.FileUploadFailed;
                
                return Redirect(model.ReturnUrl.AbsoluteUri);
            }
        }
    }
}