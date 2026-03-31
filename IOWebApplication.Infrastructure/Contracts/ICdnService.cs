using IO.SignTools.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface ICdnService 
    {
        IIOSignToolsService SignTools { get; }
        Task<CdnDownloadResult> MongoCdn_Download(int mongoFileId, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None);
        Task<CdnDownloadResult> MongoCdn_Download(string fileId, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None);
        Task<CdnDownloadResult> MongoCdn_Download(CdnFileSelect request, CdnFileSelect.PostProcess postProcess = CdnFileSelect.PostProcess.None);
        Task<CdnUploadResult> MongoCdn_UploadFile(CdnUploadRequest request);

        Task<bool> MongoCdn_DeleteFile(string id);
        Task<bool> MongoCdn_DeleteFiles(CdnFileSelect request);

        Task<string> LoadHtmlFileTemplate(CdnFileSelect request);
        Task<bool> MongoCdn_AppendUpdate(CdnUploadRequest request);

        Task<bool> SaveMongoFileData(CdnUploadRequest file, string mongoFileId);
        Task<bool> DeleteMongoFileData(string mongoFileId);

        IQueryable<CdnItemVM> Select(int sourceType, string sourceId, string fileId = null);
        IQueryable<CdnItemVM> Select(int[] sourceTypes, string sourceId, string fileId = null);

        IQueryable<CdnItemVM> Select(int[] sourceTypes, string[] sourceIds);
    }
}
