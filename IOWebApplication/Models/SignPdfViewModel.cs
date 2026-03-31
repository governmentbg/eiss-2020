namespace IOWebApplication.Models
{
    /// <summary>
    /// Information needed for signing PDF document
    /// </summary>
    public class SignPdfViewModel
    {
        /// <summary>
        /// Identifier of PDF to be signed
        /// </summary>
        public string TempFileId { get; set; }

        /// <summary>
        /// Extracted hash to be signed
        /// </summary>
        public string FileHash { get; set; }

        /// <summary>
        /// Url to PDF file
        /// </summary>
        public string PreviewPdfUrl { get; set; }

        /// <summary>
        /// PDF Signature
        /// </summary>
        public string Signature { get; set; }

        public int SignituresCount { get; set; }

        /// <summary>
        /// Name of the PDF file
        /// </summary>
        public string FileName { get; set; }
        public string FileTitle { get; set; }
        public string FileId { get; set; }

        /// <summary>
        /// URL to be redirected after signing
        /// </summary>
        public string SuccessUrl { get; set; }

        /// <summary>
        /// URL to be redirected after signing
        /// </summary>
        public string ErrorUrl { get; set; }

        /// <summary>
        /// URL to be redirected if user cancel
        /// Must be GET
        /// </summary>
        public string CancelUrl { get; set; }

        /// <summary>
        /// Identifier of the source document
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// Type of the attached file
        /// </summary>
        public int SourceType { get; set; }

        public string SignerName { get; set; }
        public string SignerUic { get; set; }
        public string ErrorMessage { get; set; }
        public long ClientCode { get; set; }

        public long WorkTaskId { get; set; }

        public int ErrorCode { get; set; }

    }
}
