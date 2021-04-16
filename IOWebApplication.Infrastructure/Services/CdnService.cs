using IO.SignTools.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Services
{
    public class CdnService : BaseCdnService, ICdnService
    {
        public IIOSignToolsService SignTools { get; }


        public CdnService(
            IRepository _repo,
            IIOSignToolsService _signTools,
            IConfiguration _config,
            IMongoClient mongoClient) : base(_config, _repo, mongoClient)
        {
            SignTools = _signTools;
        }



        public async Task<CdnDownloadResult> MongoCdn_Download(long mongoFileId, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            int _id = (int)mongoFileId;
            string fileId = repo.AllReadonly<MongoFile>().Where(x => x.Id == _id).Select(x => x.FileId).FirstOrDefault();

            return await MongoCdn_Download(fileId, postProcess);
        }

        public async Task<CdnDownloadResult> MongoCdn_Download(string fileId, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            return await MongoCdn_Download(new CdnFileSelect() { FileId = fileId }, postProcess);
        }

        public async Task<CdnDownloadResult> MongoCdn_Download(CdnFileSelect request, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            string fileId = request.FileId;
            string title = String.Empty;
            var fileItem = Select(request.SourceType, request.SourceId, request.FileId).FirstOrDefault();
            if (fileItem == null)
            {
                return null;
            }
            title = fileItem.Title;


            CdnDownloadResult downloadInfo = await GetFileById(fileItem, postProcess);
            downloadInfo.FileTitle = title;

            return downloadInfo;
        }

        private async Task<CdnDownloadResult> GetFileById(CdnItemVM fileItem, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            using (var file = await gridFsBucket.OpenDownloadStreamAsync(ObjectId.Parse(fileItem.FileId)))
            {
                byte[] fileContent = new byte[(int)file.Length];
                file.Read(fileContent, 0, (int)file.Length);
                byte[] newContent = fileContent;
                if (fileItem.FileName.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        switch (postProcess)
                        {
                            case CdnFileSelect.PostProcess.Flatten:

                                {
                                    string addText = "";
                                    int[] actSourceTypes = { SourceTypeSelectVM.CaseSessionActPdf, SourceTypeSelectVM.CaseSessionActDepersonalized };
                                    if (actSourceTypes.Contains(fileItem.SourceType))
                                    {
                                        int actId = int.Parse(fileItem.SourceId);
                                        var actModel = repo.AllReadonly<CaseSessionAct>().Where(x => x.Id == actId).FirstOrDefault();
                                        if (actModel != null)
                                        {
                                            if (actModel.ActDeclaredDate != null)
                                            {
                                                addText = $" Актът е постановен на {actModel.ActDeclaredDate:dd.MM.yyyy}";
                                            }
                                        }
                                    }
                                    newContent = flattenSignatures(fileContent, addText);

                                }
                                break;
                        }
                    }
                    catch { }
                }

                CdnDownloadResult result = new CdnDownloadResult()
                {
                    FileId = fileItem.FileId,
                    ContentType = file.FileInfo.Metadata.GetValue("contentType").AsString,
                    FileName = file.FileInfo.Filename,
                    FileContentBase64 = Convert.ToBase64String(newContent)
                };

                await file.CloseAsync();

                return result;
            }
        }
        private byte[] flattenSignatures(byte[] signedDoc, string additionalText = "")
        {
            using (MemoryStream ms = new MemoryStream(signedDoc))
            {
                return SignTools.FlattenSignature(ms, additionalText).flattenPdf;
            }
        }
       

        public async Task<string> LoadHtmlFileTemplate(CdnFileSelect request)
        {
            var downloadInfo = await MongoCdn_Download(request);
            if (downloadInfo != null)
            {
                return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(downloadInfo.FileContentBase64));
            }

            return string.Empty;
        }

        public async Task<bool> MongoCdn_AppendUpdate(CdnUploadRequest request)
        {
            var files = Select(request.SourceType, request.SourceId);
            foreach (var file in files)
            {
                DeleteMongoFileData(file.FileId);
            }
            var uploadResult = await MongoCdn_UploadFile(request);
            if (uploadResult.Succeded)
            {
                request.FileId = uploadResult.FileId;
            }

            return !string.IsNullOrEmpty(uploadResult.FileId);
        }
    }
}
