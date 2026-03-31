// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.IndexService
{
    public class IndexDataModel
    {
        public string Id { get; set; }

        public int CourtId { get; set; }
        public int? CaseId { get; set; }
        public int? CaseSessionActId { get; set; }

        public string CaseNumber { get; set; }

        public int? CaseGroupId { get; set; }
        public int? CaseTypeId { get; set; }

        public int? OtdelenieId { get; set; }
        public int? JudicalCompositionId { get; set; }
        public int? ActTypeId { get; set; }
        public string ActNumber { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime ActDeclaredDate { get; set; }
        public bool IsFinalDoc { get; set; }
        public int? JudgeReporterId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateUploaded { get; set; }

        public string SourceType { get; set; }

        public string SourceId { get; set; }

        public string FileName { get; set; }

        public string Content { get; set; }
    }
}
