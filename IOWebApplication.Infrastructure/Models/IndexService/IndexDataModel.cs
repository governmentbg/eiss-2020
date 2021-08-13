// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.IndexService
{
    public class IndexDataModel
    {
        public string Id { get; set; }

        public int CourtId { get; set; }
        public int? CaseId { get; set; }
        public int? CaseSessionActId { get; set; }

        public string DateUploaded { get; set; }

        public string SourceType { get; set; }

        public string SourceId { get; set; }

        public string FileName { get; set; }

        public string Content { get; set; }

    }
}
