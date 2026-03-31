using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Services
{
    /// <summary>
    /// Base cdn service - eliminates circural refference over IOSignToolService
    /// </summary>
    public class BaseCdnService : IBaseCdnService
    {
        protected IGridFSBucket gridFsBucket;
        protected readonly string fileDbName;
        protected readonly IMongoClient mongoClient;

        protected readonly IRepository repo;
        public BaseCdnService(
            IOptions<CdnConfigVM> optionCdnConfig,
            IRepository _repo,
            IMongoClient _mongoClient
            )
        {
            repo = _repo;
            fileDbName = optionCdnConfig.Value.FileDbName;
            mongoClient = _mongoClient;
        }

        protected void initMongo()
        {
            if (gridFsBucket != null)
            {
                return;
            }

            var database = mongoClient.GetDatabase(fileDbName);
            gridFsBucket = new GridFSBucket(database);
        }

        public Task<string> GetTempFileIdByFilename(string filename)
        {
            return repo.AllReadonly<MongoFile>()
                            .Where(x => x.SourceType == SourceTypeSelectVM.TemporaryFile && x.SourceId == filename)
                            .Select(x => x.FileId)
                            .FirstOrDefaultAsync();
        }

        public IQueryable<CdnItemVM> Select(int sourceType, string sourceId, string fileId = null)
        {
            int[] sourceTypes = new List<int>(){
                sourceType
            }.ToArray();

            return Select(sourceTypes, sourceId, fileId);
        }
        public IQueryable<CdnItemVM> Select(int[] sourceTypes, string sourceId, string fileId = null)
        {
            Expression<Func<MongoFile, bool>> whereSelect = x => x.SourceId == sourceId && sourceTypes.Contains(x.SourceType);

            if (!string.IsNullOrEmpty(fileId))
            {
                whereSelect = x => x.FileId == fileId;
            
            }
            return repo.AllReadonly<MongoFile>()
                            .Where(whereSelect)
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
                                FileSize = x.FileSize,
                                SignituresCount = x.SignituresCount ?? 0,
                                MongoFileTypeCode = (x.MongoFileTypeId > 0) ? x.MongoFileType.Code : null,
                                MongoFileTypeName = (x.MongoFileTypeId > 0) ? x.MongoFileType.Label : null
                            }).OrderBy(x => x.DateUploaded);

        }

        public IQueryable<CdnItemVM> Select(int[] sourceTypes, string[] sourceIds)
        {
            Expression<Func<MongoFile, bool>> whereSelect = x => sourceIds.Contains(x.SourceId) && sourceTypes.Contains(x.SourceType);

            return repo.AllReadonly<MongoFile>()
                            .Where(whereSelect)
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
                                FileSize = x.FileSize,
                                SignituresCount = x.SignituresCount ?? 0
                            }).OrderBy(x => x.DateUploaded);

        }
        public async virtual Task<CdnDownloadResult> GetFileById(string fileId)
        {
            initMongo();
            using (var file = await gridFsBucket.OpenDownloadStreamAsync(ObjectId.Parse(fileId)))
            {
                byte[] fileContent = new byte[(int)file.Length];
                await file.ReadAsync(fileContent, 0, (int)file.Length);


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
            initMongo();
            //await gridFsBucket.DeleteAsync(ObjectId.Parse(id));

            // return await DeleteMongoFileData(id);




            bool result = true;
            var selectedFiles = await repo.AllReadonly<MongoFile>()
                            .Where(x => x.SourceType == request.SourceType && x.SourceId == request.SourceId)
                            .Select(x => x.FileId)
                            .ToListAsync();


            //(request.SourceType, request.SourceId, request.FileId);

            foreach (var _file in selectedFiles)
            {
                await gridFsBucket.DeleteAsync(ObjectId.Parse(_file));

                result &= await DeleteMongoFileData(_file);

                //result &= await MongoCdn_DeleteFile(_file.FileId);
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
                initMongo();
                string mongoFileId = (await gridFsBucket.UploadFromBytesAsync(request.FileName, request.FileContent, options)).ToString();

                if (!string.IsNullOrEmpty(mongoFileId))
                {
                    result.Succeded = await SaveMongoFileData(request, mongoFileId);
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
            initMongo();
            await gridFsBucket.DeleteAsync(ObjectId.Parse(id));

            return await DeleteMongoFileData(id);
        }

        public async Task<bool> DeleteMongoFileData(string mongoFileId)
        {
            return await repo.ExecuteDeleteAsync<MongoFile>(x => x.FileId == mongoFileId) > 0;
            //var saved = await repo.All<MongoFile>(x => x.FileId == mongoFileId).FirstOrDefaultAsync();

            //if (saved != null)
            //{
            //    repo.Delete(saved);
            //    await repo.SaveChangesAsync();
            //    return true;
            //}

            //return false;
        }

        public async Task<bool> SaveMongoFileData(CdnUploadRequest file, string mongoFileId)
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
                    SignituresCount = file.SignituresCount,
                    MongoFileTypeId = file.MongoFileTypeId,
                    UserUploaded = file.UserUploaded,
                    DateUploaded = DateTime.Now
                };

                if (long.TryParse(mongoFile.SourceId, out long parsedId))
                {
                    mongoFile.SourceIdNumber = parsedId;
                }

                //if (file.SignersCount > 0)
                //{
                //    mongoFile.SignersCount = file.SignersCount;
                //}

                await repo.AddAsync(mongoFile);
                await repo.SaveChangesAsync();
                file.MongoFileId = mongoFile.Id;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}