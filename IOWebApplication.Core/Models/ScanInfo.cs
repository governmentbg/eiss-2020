using System;

namespace IOWebApplication.Core.Models
{
    public class ScanInfo
    {
        /// <summary>
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
    }
}
