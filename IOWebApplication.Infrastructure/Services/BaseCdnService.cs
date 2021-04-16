using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
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
    /// <summary>
    /// Base cdn service - eliminates circural refference over IOSignToolService
    /// </summary>
    public class BaseCdnService : IBaseCdnService
    {
        protected readonly IGridFSBucket gridFsBucket;
        protected readonly IRepository repo;
        public BaseCdnService(
            IConfiguration _config,
            IRepository _repo,
            IMongoClient mongoClient
            )
        {
            repo = _repo;
            string fileDbName = _config.GetValue<string>("FileDbName");
            var database = mongoClient.GetDatabase(fileDbName);
            gridFsBucket = new GridFSBucket(database);
        }
        public IEnumerable<CdnItemVM> Select(int sourceType, string sourceId, string fileId = null)
        {
            int[] sourceTypes = new List<int>(){
                sourceType
            }.ToArray();

            return Select(sourceTypes, sourceId, fileId);
        }
        public IEnumerable<CdnItemVM> Select(int[] sourceTypes, string sourceId, string fileId = null)
        {
            if (!string.IsNullOrEmpty(fileId))
            {
                return repo.AllReadonly<MongoFile>()
                                .Where(x => x.FileId == fileId)
                                .Select(x => new CdnItemVM
                                {
                                    MongoFileId = x.Id,
                                    SourceType = x.SourceType,
                                    SourceId = x.SourceId,
                                    FileId = x.FileId,
                                    Title = x.Title ?? x.FileName,
                                    FileName = x.FileName,
                                    UserUploaded = x.UserUploaded,
                                    DateUploaded = x.DateUploaded,
                                    DateExpired = x.DateExpired,
                                    FileSize = x.FileSize
                                }).OrderBy(x => x.DateUploaded);
            }
            else
            {
                return repo.AllReadonly<MongoFile>()
                                .Where(x => x.SourceId == sourceId && sourceTypes.Contains(x.SourceType))
                                .Select(x => new CdnItemVM
                                {
                                    MongoFileId = x.Id,
                                    SourceType = x.SourceType,
                                    SourceId = x.SourceId,
                                    FileId = x.FileId,
                                    Title = x.Title ?? x.FileName,
                                    FileName = x.FileName,
                                    UserUploaded = x.UserUploaded,
                                    DateUploaded = x.DateUploaded,
                                    DateExpired = x.DateExpired,
                                    FileSize = x.FileSize
                                }).OrderBy(x => x.DateUploaded);

            }
        }
        public async virtual Task<CdnDownloadResult> GetFileById(string fileId)
        {
            using (var file = await gridFsBucket.OpenDownloadStreamAsync(ObjectId.Parse(fileId)))
            {
                byte[] fileContent = new byte[(int)file.Length];
                file.Read(fileContent, 0, (int)file.Length);


                CdnDownloadResult result = new CdnDownloadResult()
                {
                    FileId = fileId,
                    ContentType = file.FileInfo.Metadata.GetValue("contentType").AsString,
                    FileName = file.FileInfo.Filename,
                    FileContentBase64 = Convert.ToBase64String(fileContent)
                };

                await file.CloseAsync();

                return result;
            }
        }

        public async Task<bool> MongoCdn_DeleteFiles(CdnFileSelect request)
        {
            bool result = true;
            var selectedFiles = Select(request.SourceType, request.SourceId, request.FileId);

            foreach (var _file in selectedFiles)
            {
                result &= await MongoCdn_DeleteFile(_file.FileId);
            }

            return result;
        }

        public async Task<CdnUploadResult> MongoCdn_UploadFile(CdnUploadRequest request)
        {
            CdnUploadResult result = new CdnUploadResult() { Succeded = false };
            Dictionary<string, object> metadata = new Dictionary<string, object>();
            metadata.Add("contentType", request.ContentType);
            metadata.Add("sourceType", request.SourceType);
            metadata.Add("sourceId", request.SourceId);
            metadata.Add("title", request.Title);

            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument(metadata)
            };

            request.FileContent = Convert.FromBase64String(request.FileContentBase64);

            try
            {
                string mongoFileId = (await gridFsBucket.UploadFromBytesAsync(request.FileName, request.FileContent, options)).ToString();

                if (!string.IsNullOrEmpty(mongoFileId))
                {
                    result.Succeded = SaveMongoFileData(request, mongoFileId);
                }

                result.FileId = mongoFileId;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }


        public async Task<bool> MongoCdn_DeleteFile(string id)
        {
            await gridFsBucket.DeleteAsync(ObjectId.Parse(id));

            return DeleteMongoFileData(id);
        }

        public bool DeleteMongoFileData(string mongoFileId)
        {
            var saved = repo.All<MongoFile>(x => x.FileId == mongoFileId).FirstOrDefault();

            if (saved != null)
            {
                repo.Delete(saved);
                repo.SaveChanges();
                return true;
            }

            return false;
        }

        public bool SaveMongoFileData(CdnUploadRequest file, string mongoFileId)
        {
            try
            {
                var mongoFile = new MongoFile()
                {
                    FileId = mongoFileId,
                    SourceType = file.SourceType,
                    SourceId = file.SourceId,
                    Title = file.Title,
                    FileSize = file.FileContent.Length,
                    FileName = file.FileName,
                    UserUploaded = file.UserUploaded,
                    DateUploaded = DateTime.Now
                };

                if (long.TryParse(mongoFile.SourceId, out long parsedId))
                {
                    mongoFile.SourceIdNumber = parsedId;
                }

                if (file.SignersCount > 0)
                {
                    mongoFile.SignersCount = file.SignersCount;
                }

                repo.Add(mongoFile);
                repo.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}