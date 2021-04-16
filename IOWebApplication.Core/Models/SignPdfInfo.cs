using System;

namespace IOWebApplication.Core.Models
{
    public class SignPdfInfo
    {
        /// <summary>
        /// Reason field
        /// </summary>
        private string reason;

        /// <summary>
        /// Location field
        /// </summary>
        private string location;

        /// <summary>
        /// URL to be redirected after signing
        /// </summary>
        public Uri SuccessUrl { get; set; }

        /// <summary>
        /// URL to be redirected after signing
        /// </summary>
        public Uri ErrorUrl { get; set; }

        /// <summary>
        /// URL to be redirected if user cancel
        /// Must be GET
        /// </summary>
        public Uri CancelUrl { get; set; }

        /// <summary>
        /// Identifier of the source document
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// Type of the attached file
        /// </summary>
        public int SourceType { get; set; }

        /// <summary>
        /// ObjectID of MongoFile
        /// </summary>
        public string FileId { get; set; }


        /// <summary>
        /// Type of the signed attached file
        /// </summary>
        public int DestinationType { get; set; }


        public string SignerName { get; set; }
        public string SignerUic { get; set; }

        /// <summary>
        /// Reason for signing document
        /// </summary>
        public string Reason 
        { 
            get => reason ?? string.Empty; 
            set => reason = value; 
        }

        /// <summary>
        /// Location of signing
        /// </summary>
        public string Location 
        { 
            get => location ?? string.Empty; 
            set => location = value; 
        }
    }
}
