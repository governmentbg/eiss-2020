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
        public string PdfId { get; set; }

        /// <summary>
        /// Extracted hash to be signed
        /// </summary>
        public string PdfHash { get; set; }

        /// <summary>
        /// Url to PDF file
        /// </summary>
        public string PdfUrl { get; set; }

        /// <summary>
        /// Name of the PDF file
        /// </summary>
        public string FileName { get; set; }
        public string FileTitle { get; set; }

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
    }
}
