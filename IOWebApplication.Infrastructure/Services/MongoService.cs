// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Cdn;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Services
{
    public class MongoService : MongoDbCdnContext, IMongoService
    {
        private readonly ICdnService cdnService;
        public MongoService(IConfiguration config, ICdnService _cdnService) : base(config)
        {
            cdnService = _cdnService;
        }

        public async Task<string> UploadAsync(CdnUploadRequest file)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>();
            metadata.Add("contentType", file.ContentType);
            metadata.Add("sourceType", file.SourceType);
            metadata.Add("sourceId", file.SourceId);
            metadata.Add("title", file.Title);

            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument(metadata)
            };
            file.FileContent = Convert.FromBase64String(file.FileContentBase64);
            var mongoFileId = (await GridFsBucket.UploadFromBytesAsync(file.FileName, file.FileContent, options)).ToString();
            if (!string.IsNullOrEmpty(mongoFileId))
            {
                cdnService.SaveMongoFileData(file, mongoFileId);
            }
            return mongoFileId;
        }



        public async Task<bool> AnyAsync(ObjectId id)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", id);
            return await GridFsBucket.Find(filter).AnyAsync();
        }
        public Task<bool> AnyAsync(string fileName)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Where(x => x.Filename == fileName);
            return GridFsBucket.Find(filter).AnyAsync();
        }
        public async Task DeleteAsync(string fileName)
        {
            var fileInfo = await GetFileInfoAsync(fileName);
            if (fileInfo != null)
            {
                await DeleteAsync(fileInfo.Id);
            }
        }
        public async Task DeleteAsync(ObjectId id)
        {
            if (await AnyAsync(id))
            {
                await GridFsBucket.DeleteAsync(id);
            }
            cdnService.DeleteMongoFileData(id.ToString());
        }
        private async Task<GridFSFileInfo> GetFileInfoAsync(string fileName)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, fileName);
            var fileInfo = await GridFsBucket.Find(filter).FirstOrDefaultAsync();
            return fileInfo;
        }
        public async Task<GridFSDownloadStream<ObjectId>> DownloadAsync(ObjectId id)
        {
            return await GridFsBucket.OpenDownloadStreamAsync(id);
        }
        public async Task<GridFSDownloadStream<ObjectId>> DownloadAsync(string fileName)
        {
            return await GridFsBucket.OpenDownloadStreamByNameAsync(fileName);
        }
        public IEnumerable<MongoItemVM> GetAllFilesByContentType(string contentType, int skip, int take)
        {
            var filter = Builders<GridFSFileInfo>.Filter
                .Eq(info => info.Metadata, new BsonDocument(new BsonElement("contentType", contentType)));
            var options = new GridFSFindOptions
            {
                Limit = take,
                Skip = skip,
            };

            var stream = GridFsBucket.Find(filter, options)
                .ToList()
                .Select(s => new MongoItemVM
                {
                    Id = s.Id,
                    Filename = s.Filename,
                    MetaData = s.Metadata,
                    Length = s.Length + "",
                    UploadDateTime = s.UploadDateTime,
                }).ToList();
            return stream;
        }
        public IEnumerable<MongoItemVM> GetAllFiles(int skip, int take)
        {
            var options = new GridFSFindOptions
            {
                Limit = take,
                Skip = skip,
            };

            var stream = GridFsBucket
                .Find(new BsonDocumentFilterDefinition<GridFSFileInfo<ObjectId>>(new BsonDocument()), options)
                .ToList()
               .Select(s => new MongoItemVM
               {
                   Id = s.Id,
                   Filename = s.Filename,
                   MetaData = s.Metadata,
                   Length = s.Length + "",
                   UploadDateTime = s.UploadDateTime,
               }).ToList();
            return stream;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
