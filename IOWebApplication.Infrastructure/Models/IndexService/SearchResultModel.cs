// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.IndexService
{
    public class SearchResultModel
    {
        public int ActId { get; set; }
        public int CaseId { get; set; }

        public string CaseGroupName { get; set; }
        public string CaseTypeName { get; set; }

        public string CaseNumber { get; set; }

        public string OtdelenieName { get; set; }
        public string JudicalCompositionName { get; set; }
        public string ActTypeName { get; set; }
        public string JudgeReporterName { get; set; }

        public string ActNumber { get; set; }
        public DateTime ActDeclaredDate { get; set; }

        public bool IsFinalDoc { get; set; }

        public string[] Highlights { get; set; }
    }

    public class SearchResponseModel
    {
        public int TotalCount { get; set; }
        public IQueryable<SearchResultModel> Items { get; set; }
    }
}
