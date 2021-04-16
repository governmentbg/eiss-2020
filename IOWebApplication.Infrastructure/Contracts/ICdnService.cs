using IO.SignTools.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface ICdnService 
    {
        IIOSignToolsService SignTools { get; }
        Task<CdnDownloadResult> MongoCdn_Download(long mongoFileId, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None);
        Task<CdnDownloadResult> MongoCdn_Download(string fileId, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None);
        Task<CdnDownloadResult> MongoCdn_Download(CdnFileSelect request, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None);
        Task<CdnUploadResult> MongoCdn_UploadFile(CdnUploadRequest request);

        Task<bool> MongoCdn_DeleteFile(string id);
        Task<bool> MongoCdn_DeleteFiles(CdnFileSelect request);

        Task<string> LoadHtmlFileTemplate(CdnFileSelect request);
        Task<bool> MongoCdn_AppendUpdate(CdnUploadRequest request);

        bool SaveMongoFileData(CdnUploadRequest file, string mongoFileId);
        bool DeleteMongoFileData(string mongoFileId);

        IEnumerable<CdnItemVM> Select(int sourceType, string sourceId, string fileId = null);
        IEnumerable<CdnItemVM> Select(int[] sourceTypes, string sourceId, string fileId = null);
    }
}
