using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.IO;

namespace IOWebApplication.Infrastructure.Models.Cdn
{
    public class CdnItemVM : CdnUploadRequest
    {
        public DateTime DateUploaded { get; set; }
        public DateTime? DateExpired { get; set; }
        public string SafeFileName
        {
            get
            {
                return this.FileName.Replace("\'", "");
            }
        }
        public int FileSize { get; set; }
        public string FileIcon
        {
            get
            {
                switch (Path.GetExtension(this.FileName).ToLower())
                {
                    case ".jpeg":
                    case ".jpg":
                    case ".bmp":
                    case ".png":
                    case ".tiff":
                        return "fa fa-file-image-o";
                    case ".pdf":
                        return "fa fa-file-pdf-o";
                    case ".docx":
                    case ".doc":
                        return "fa fa-file-word-o";
                    case ".xls":
                    case ".xlsx":
                        return "fa  fa-file-excel-o";
                    default:
                        return "fa  fa-file";
                }
            }
        }

        public bool CanDelete { get; set; }
        public bool CanPreview
        {
            get
            {
                switch (Path.GetExtension(this.FileName).ToLower())
                {
                    case ".jpeg":
                    case ".jpg":
                    case ".bmp":
                    case ".png":
                    case ".tiff":
                    case ".pdf":
                        return true;
                }
                return false;
            }
        }

        public bool HasSignerInfo
        {

            get
            {
                return SourceType == SourceTypeSelectVM.DocumentFromElectronicDocument &&
                    Path.GetExtension(this.FileName).ToLower() == ".pdf";
            }
        }

        public string MongoFileTypeCode { get; set; }
        public string MongoFileTypeName { get; set; }
        public string DisplayName
        {
            get
            {
                return MongoFileTypeName ?? Title;
            }
        }

        public CdnItemVM()
        {
            CanDelete = true;
        }
    }
}
