// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IO.SignTools.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Services
{
    public class TempFileHandler : ITempFileHandler
    {
        private readonly ICdnService cdn;

        private readonly ILogger logger;

        public TempFileHandler(
            ICdnService _cdn,
            ILogger<TempFileHandler> _logger)
        {
            cdn = _cdn;
            logger = _logger;
        }

        public async Task DeleteFile(string filename)
        {
            await cdn.MongoCdn_DeleteFiles(new CdnFileSelect()
            {
                SourceId = filename,
                SourceType = SourceTypeSelectVM.TemporaryFile
            });
        }

        public async Task<byte[]> ReadFile(string filename)
        {
            var result = await cdn.MongoCdn_Download(new CdnFileSelect() 
            {
                SourceId = filename,
                SourceType = SourceTypeSelectVM.TemporaryFile
            });

            if (result != null)
            {
                return Convert.FromBase64String(result.FileContentBase64);
            }

            throw new FileNotFoundException($"File { filename } not found");
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
