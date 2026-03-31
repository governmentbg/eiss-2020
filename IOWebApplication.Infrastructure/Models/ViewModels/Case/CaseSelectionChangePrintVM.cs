// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSelectionChangePrintVM
    {
        public int Id { get; set; }
        public int ChangeTypeId { get; set; }
        public string ChangeTypeLabel { get; set; }
        public string CourtName { get; set; }
        public string CourtGroupName { get; set; }

        public string FromLawunitName { get; set; }
        public string ToLawunitName { get; set; }
        public string ToLawunitDepartmentName { get; set; }

        public string UserName { get; set; }
        public DateTime DateWrt { get; set; }
        public string Description { get; set; }
        public int ChangeStateId { get; set; }

        public List<CaseSelectionChangePrintListVM> CaseList { get; set; }
    }

    public class CaseSelectionChangePrintListVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string RegNumber { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime DocumentDate { get; set; }
        public string CaseTypeLabel { get; set; }
        public string CaseCodeLabel { get; set; }
        public string CourtGroupLabel { get; set; }
        public string LoadGroupLabel { get; set; }
        public string FromLawunitName { get; set; }
        public string ToLawunitName { get; set; }
    }
}
