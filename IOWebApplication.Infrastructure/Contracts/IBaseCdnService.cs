using IOWebApplication.Infrastructure.Models.Cdn;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IBaseCdnService
    {
        Task<CdnUploadResult> MongoCdn_UploadFile(CdnUploadRequest request);

        Task<bool> MongoCdn_DeleteFile(string id);
        Task<bool> MongoCdn_DeleteFiles(CdnFileSelect request);

        IQueryable<CdnItemVM> Select(int sourceType, string sourceId, string fileId = null);
        Task<CdnDownloadResult> GetFileById(string id);
        Task<string> GetTempFileIdByFilename(string filename);
    }
}
