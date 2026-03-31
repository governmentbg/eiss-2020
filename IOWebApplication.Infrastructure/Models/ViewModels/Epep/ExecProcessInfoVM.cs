// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using RestEpep = IOWebApplication.Infrastructure.Models.Integrations.EpepRest;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class ExecProcessInfoVM
    {
        public RestEpep.ExecProcessDetailsVM Data { get; set; }

        public DateTime DateWrt { get; set; }

        public int CaseSessionActId { get; set; }
        public int ExecListId { get; set; }
    }

    public class ExecAccessDocumentVM
    {
        public string CourtName { get; set; }
        public string CaseType { get; set; }
        public string CaseNumber { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string ApplicantName { get; set; }
        public string ActType { get; set; }
        public string ActNumber { get; set; }
        public DateTime AccessDate { get; set; }
        public string AccessKey { get; set; }
        public string UserName { get; set; }
    }
}
