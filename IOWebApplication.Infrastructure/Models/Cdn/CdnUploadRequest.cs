using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.Cdn
{
    public class CdnUploadRequest : CdnFileSelect
    {
        /// <summary>
        /// Friendly name of content
        /// </summary>
        [Display(Name = "Описание на файла")]
        public string Title { get; set; }
        /// <summary>
        /// Actual file name, including extension
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Mime type
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// Binary content of the file
        /// </summary>
        public byte[] FileContent { get; set; }

        public string FileContentBase64 { get; set; }
        /// <summary>
        /// UserId of user uploaded
        /// </summary>
        public string UserUploaded { get; set; }
        /// <summary>
        /// Element id of div tag file container
        /// </summary>
        public string FileContainer { get; set; }

        public int SignituresCount { get; set; }

        public bool FileUploadEnabled { get; set; }

        public int MaxFileSize { get; set; }

        public CdnUploadRequest()
        {
            SignituresCount = 0;
        }
    }
}
