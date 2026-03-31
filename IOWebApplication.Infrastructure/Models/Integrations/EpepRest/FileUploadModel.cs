// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    public class FileUploadModel
    {
        public Guid? AttachedDocumentId { get; set; }
        public int AttachedType { get; set; }
        public Guid ParentGid { get; set; }
        public string FileContent { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string MimeType { get; set; }
        public string FileTypeCode { get; set; }
    }

    public class FileSelectModel
    {
        public Guid AttachedDocumentId { get; set; }
        public int AttachedType { get; set; }
        public Guid ParentGid { get; set; }
    }

    public class FileBlobVM
    {
        public string FileName { get; set; }
        public string FileContent { get; set; }
        public byte[] BinaryContent { get; set; }
    }
}
