// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class AttachedDocument
    {
        public string GUID { get; set; }
        public string MimeType { get; set; }
        public string FileName { get; set; }
        public string Size { get; set; }
        public string Content { get; set; }
    }
}
