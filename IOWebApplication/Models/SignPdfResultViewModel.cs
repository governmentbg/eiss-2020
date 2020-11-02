// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Models
{
    /// <summary>
    /// Result of signing PDF document
    /// </summary>
    public class SignPdfResultViewModel
    {
        /// <summary>
        /// Identifier of PDF to be signed
        /// </summary>
        public string PdfId { get; set; }

        /// <summary>
        /// PDF Signature
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Name of the PDF file
        /// </summary>
        public string FileName { get; set; }
        public string FileTitle { get; set; }

        /// <summary>
        /// Result code returned by StampitLSM
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Identifier of the source document
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// Type of the attached file
        /// </summary>
        public int SourceType { get; set; }

        /// <summary>
        /// URL to be redirected after signing
        /// </summary>
        public string SuccessUrl { get; set; }

        /// <summary>
        /// URL to be redirected after signing
        /// </summary>
        public string ErrorUrl { get; set; }

        public string SignerName { get; set; }
        public string SignerUic { get; set; }
    }
}
