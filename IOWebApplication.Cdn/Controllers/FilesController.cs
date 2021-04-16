using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace IOWebApplication.Cdn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FilesController : ControllerBase
    {
        private readonly IMongoService mongoService;
        public FilesController(IMongoService _mongoService)
        {
            mongoService = _mongoService;
        }

        [HttpGet]
        public string Get()
        {
            return "It works!";
        }


        [HttpPost]
        [DisableRequestSizeLimit]
        [Route("upload")]
        public async Task<CdnUploadResult> Upload(CdnUploadRequest request)
        {
            CdnUploadResult result = new CdnUploadResult()
            {
                Succeded = false
            };
            try
            {
                result.FileId = await mongoService.UploadAsync(request);                
                result.Succeded = !string.IsNullOrEmpty(result.FileId);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }
            return result;
        }
        [HttpGet]
        [Route("download/{id}")]
        public async Task<FileResult> Download(string id)
        {
            var file = await mongoService.DownloadAsync(ObjectId.Parse(id));
            byte[] fileContent = new byte[(int)file.Length];
            file.Read(fileContent, 0, (int)file.Length);
            return File(fileContent, file.FileInfo.Metadata.GetValue("contentType").AsString, file.FileInfo.Filename);
        }

        [HttpPost]
        [Route("download_info/{id}")]
        public async Task<CdnDownloadResult> DownloadInfo(string id)
        {
            var file = await mongoService.DownloadAsync(ObjectId.Parse(id));
            byte[] fileContent = new byte[(int)file.Length];
            file.Read(fileContent, 0, (int)file.Length);

            CdnDownloadResult result = new CdnDownloadResult()
            {
                FileId = id,
                ContentType = file.FileInfo.Metadata.GetValue("contentType").AsString,
                FileName = file.FileInfo.Filename,
                FileContentBase64 = Convert.ToBase64String(fileContent)
            };

            return result;
        }

        [HttpPost]
        [Route("delete/{id}")]
        public async Task<bool> Delete(string id)
        {
            try
            {
                await mongoService.DeleteAsync(ObjectId.Parse(id));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}
