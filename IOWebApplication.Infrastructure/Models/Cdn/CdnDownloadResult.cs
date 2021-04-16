using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.Cdn
{
    public class CdnDownloadResult
    {
        public string FileId { get; set; }

        /// <summary>
        /// Mime type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Actual file name, including extension
        /// </summary>
        public string FileName { get; set; }
        public string FileTitle { get; set; }

        public string FileContentBase64 { get; set; }

        public byte[] GetBytes()
        {
            return Convert.FromBase64String(this.FileContentBase64);
        }
    }
}
