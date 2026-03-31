using DnsClient.Internal;
using IO.SignTools.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly ILogger<CdnService> logger;
        public IIOSignToolsService SignTools { get; }


        public CdnService(
            IRepository _repo,
            IIOSignToolsService _signTools,
            ILogger<CdnService> logger,
            IOptions<CdnConfigVM> optionCdnConfig,
            IMongoClient mongoClient) : base(optionCdnConfig, _repo, mongoClient)
        {
            SignTools = _signTools;
            this.logger = logger;
        }



        public async Task<CdnDownloadResult> MongoCdn_Download(int mongoFileId, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            string fileId = await repo.AllReadonly<MongoFile>().Where(x => x.Id == mongoFileId).Select(x => x.FileId).FirstOrDefaultAsync();

            return await MongoCdn_Download(fileId, postProcess);
        }

        public Task<CdnDownloadResult> MongoCdn_Download(string fileId, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            return MongoCdn_Download(new CdnFileSelect() { FileId = fileId }, postProcess);
        }

        public async Task<CdnDownloadResult> MongoCdn_Download(CdnFileSelect request, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            string fileId = request.FileId;
            string title = String.Empty;

            var fileItem = await Select(request.SourceType, request.SourceId, request.FileId).FirstOrDefaultAsync();
            if (fileItem == null)
            {
                return null;
            }
            title = fileItem.Title;

            CdnDownloadResult downloadInfo = await GetFileById(fileItem, postProcess);
            downloadInfo.FileTitle = title;
            downloadInfo.SignituresCount = fileItem.SignituresCount;
            downloadInfo.DateUploaded = fileItem.DateUploaded;
            downloadInfo.SourceType = fileItem.SourceType;
            downloadInfo.SourceId = fileItem.SourceId;
            downloadInfo.MongoTypeCode = fileItem.MongoFileTypeCode;

            return downloadInfo;
        }

        private async Task<CdnDownloadResult> GetFileById(CdnItemVM fileItem, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None)
        {
            initMongo();

            using (var file = await gridFsBucket.OpenDownloadStreamAsync(ObjectId.Parse(fileItem.FileId)))
            {
                byte[] fileContent = new byte[(int)file.Length];
                await file.ReadAsync(fileContent, 0, (int)file.Length);
                byte[] newContent = fileContent;
                if (fileItem.FileName.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        switch (modifyPostProcessBySource(fileItem.SourceType, postProcess))
                        {
                            case CdnFileSelect.PostProcess.Flatten:

                                {
                                    string addText = "";


                                    int[] actSourceTypes = { SourceTypeSelectVM.CaseSessionActPdf, SourceTypeSelectVM.CaseSessionActDepersonalized };
                                    if (actSourceTypes.Contains(fileItem.SourceType))
                                    {
                                        int actId = int.Parse(fileItem.SourceId);
                                        var actModel = await repo.AllReadonly<CaseSessionAct>()
                                                                .Where(x => x.Id == actId)
                                                                .Select(x => new
                                                                {
                                                                    x.ActDeclaredDate,
                                                                    x.ActTypeId
                                                                })
                                                                .FirstOrDefaultAsync();
                                        if (actModel != null && actModel.ActDeclaredDate != null
                                            // След Заявка 1, 2024г този текст се премахва
                                            && actModel.ActDeclaredDate.Value.Year <= 2024
                                            )
                                        {
                                            if (actModel.ActTypeId == NomenclatureConstants.ActType.Protokol)
                                            {
                                                addText = $" Протоколът е подписан на {actModel.ActDeclaredDate:dd.MM.yyyy}";
                                            }
                                            else
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
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"FileID= {fileItem.FileId}");
                    }
                }

                CdnDownloadResult result = new CdnDownloadResult()
                {
                    FileId = fileItem.FileId,
                    ContentType = (!file.FileInfo.Metadata.GetValue("contentType").IsBsonNull) ? file.FileInfo.Metadata.GetValue("contentType").AsString : "application/octet-stream",
                    FileName = file.FileInfo.Filename,
                    FileContentBase64 = Convert.ToBase64String(newContent)
                };



                await file.CloseAsync();

                return result;
            }
        }

        private CdnFileSelect.PostProcess modifyPostProcessBySource(int sourceType, CdnFileSelect.PostProcess processMode)
        {
            int[] eissCreatedFiles = {
                SourceTypeSelectVM.DocumentPdf,
                SourceTypeSelectVM.DocumentFileFromAPI,
                SourceTypeSelectVM.DocumentResolutionPdf,
                SourceTypeSelectVM.CaseSessionActPdf,
                SourceTypeSelectVM.CaseSessionActDepersonalized,
                SourceTypeSelectVM.CaseSessionActMotivePdf,
                SourceTypeSelectVM.CaseSessionActMotiveDepersonalized,
                SourceTypeSelectVM.CaseSessionActCoordinationPdf,
                SourceTypeSelectVM.CaseSelectionProtokol,
                SourceTypeSelectVM.CaseSelectionProtokolFile,
                SourceTypeSelectVM.ExecList,
                SourceTypeSelectVM.TestSignPDF,
                SourceTypeSelectVM.CaseSelectionChangeProtokol,
                SourceTypeSelectVM.CasePersonBulletin,
                SourceTypeSelectVM.CaseDeactivate
            };

            if (eissCreatedFiles.Contains(sourceType))
            {
                return processMode;
            }
            return CdnFileSelect.PostProcess.None;
        }
        private byte[] flattenSignatures(byte[] signedDoc, string additionalText = "")
        {
            if (signedDoc == null || signedDoc.Length == 0)
            {
                return signedDoc;
            }
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
            var files = await Select(request.SourceType, request.SourceId).ToArrayAsync();
            foreach (var file in files)
            {
                await DeleteMongoFileData(file.FileId);
            }
            var uploadResult = await MongoCdn_UploadFile(request);
            if (uploadResult.Succeded)
            {
                request.FileId = uploadResult.FileId;
            }

            return uploadResult.Succeded;
        }
    }
}
