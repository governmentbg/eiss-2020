using System;

namespace IOWebApplication.Core.Models
{
    public class ScanInfoViewModel
    {
        // <summary>
        /// URL to be redirected after signing
        /// </summary>
        public Uri ReturnUrl { get; set; }

        /// <summary>
        /// Identifier of the source document
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// Type of the attached file
        /// </summary>
        public int SourceType { get; set; }

        public string Mode { get; set; }

        /// <summary>
        /// Name of scanned document
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Title of the scanned document
        /// </summary>
        public string Title { get; set; }


        /// <summary>
        /// Base64 encoded file content
        /// </summary>
        public string FileContent { get; set; }
    }
}
