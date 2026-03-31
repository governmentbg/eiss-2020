// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtStampRequestVM
    {
        public byte[] PdfContent { get; set; }

        public int CourtId { get; set; }
        public int SourceType { get; set; }
        public int BlankMode { get; set; }
        public bool WideStamp { get; set; }

        public string StampContent { get; set; }
        public string StampReason { get; set; }
        public string StampLocation { get; set; }
    }

    public class CourtStampInfoVM
    {
        public byte[] CertificateContent { get; set; }
        public string PassHash { get; set; }
    }

    public class CourtStampResponseVM
    {
        public bool Result { get; set; }
        public string StampError { get; set; }

        public byte[] StampedPdfContent { get; set; }
    }
}
