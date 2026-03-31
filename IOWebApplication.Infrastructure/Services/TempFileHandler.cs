using IO.SignTools.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Services
{
    public class TempFileHandler : ITempFileHandler
    {
        private readonly IBaseCdnService cdn;

        private readonly ILogger logger;

        public TempFileHandler(
            IBaseCdnService _cdn,
            ILogger<TempFileHandler> _logger)
        {
            cdn = _cdn;
            logger = _logger;
        }

        public Task DeleteFile(string filename)
        {
            return cdn.MongoCdn_DeleteFiles(new CdnFileSelect()
            {
                SourceId = filename,
                SourceType = SourceTypeSelectVM.TemporaryFile
            });
        }

        public async Task<byte[]> ReadFile(string filename)
        {
            string fileId = null;
            int failedCount = 0;
            do
            {
                fileId = await cdn.GetTempFileIdByFilename(filename);
                if (string.IsNullOrEmpty(fileId))
                {
                    await Task.Delay(500);
                }
                failedCount++;
            } while (string.IsNullOrEmpty(fileId) && failedCount < 5);

            if (!string.IsNullOrEmpty(fileId))
            {

                failedCount = 0;
                CdnDownloadResult result = null;
                do
                {
                    result = await cdn.GetFileById(fileId);
                    if (result == null)
                    {
                        await Task.Delay(500);
                    }
                    failedCount++;
                } while (result == null && failedCount < 5);

                if (result != null)
                {
                    return Convert.FromBase64String(result.FileContentBase64);
                }
                throw new FileNotFoundException($"File {filename} cannot be downloaded");
            }
            throw new FileNotFoundException($"File {filename} not found");
        }

        public async Task SaveFile(string filename, byte[] data)
        {
            var result = await cdn.MongoCdn_UploadFile(new CdnUploadRequest()
            {
                ContentType = "application/octet-stream",
                FileContentBase64 = Convert.ToBase64String(data),
                FileName = filename,
                SourceId = filename,
                SourceType = SourceTypeSelectVM.TemporaryFile
            });

            if (!result.Succeded)
            {
                logger.LogError(result.ErrorMessage);

                throw new ApplicationException(result.ErrorMessage);
            }
        }
    }
}
