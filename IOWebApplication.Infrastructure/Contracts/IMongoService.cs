// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.Cdn;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IMongoService : IDisposable
    {
        Task<string> UploadAsync(CdnUploadRequest file);
        Task<bool> AnyAsync(ObjectId id);
        Task<bool> AnyAsync(string fileName);
        Task DeleteAsync(string fileName);
        Task DeleteAsync(ObjectId id);
        Task<GridFSDownloadStream<ObjectId>> DownloadAsync(string fileName);
        Task<GridFSDownloadStream<ObjectId>> DownloadAsync(ObjectId id);
        IEnumerable<MongoItemVM> GetAllFilesByContentType(string contentType, int skip, int take);
        IEnumerable<MongoItemVM> GetAllFiles(int skip, int take);
    }
}
