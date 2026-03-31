// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IO.SignTools.Contracts;

// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplicationApi.Services.Mocks
{
    public class MockCdnService : ICdnService
    {
        public IIOSignToolsService SignTools => throw new System.NotImplementedException();

        public Task<bool> DeleteMongoFileData(string mongoFileId)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> LoadHtmlFileTemplate(CdnFileSelect request)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> MongoCdn_AppendUpdate(CdnUploadRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> MongoCdn_DeleteFile(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> MongoCdn_DeleteFiles(CdnFileSelect request)
        {
            throw new System.NotImplementedException();
        }

        public Task<CdnDownloadResult> MongoCdn_Download(int mongoFileId, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            throw new System.NotImplementedException();
        }

        public Task<CdnDownloadResult> MongoCdn_Download(string fileId, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            throw new System.NotImplementedException();
        }

        public Task<CdnDownloadResult> MongoCdn_Download(CdnFileSelect request, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            throw new System.NotImplementedException();
        }

        public Task<CdnUploadResult> MongoCdn_UploadFile(CdnUploadRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> SaveMongoFileData(CdnUploadRequest file, string mongoFileId)
        {
            return Task.FromResult(false);
        }

        public IQueryable<CdnItemVM> Select(int sourceType, string sourceId, string fileId = null)
        {
            throw new System.NotImplementedException();
        }

        public IQueryable<CdnItemVM> Select(int[] sourceTypes, string sourceId, string fileId = null)
        {
            throw new System.NotImplementedException();
        }

        public IQueryable<CdnItemVM> Select(int[] sourceTypes, string[] sourceIds)
        {
            throw new System.NotImplementedException();
        }
    }
}
