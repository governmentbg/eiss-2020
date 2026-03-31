// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class AssignedDocumentInfoVM
    {
        public int? CreateCourtId { get; set; }

        [Display(Name = "Въведено в съд")]
        public string CreateCourtName { get; set; }

        public List<AssignedDocumentVM> Documents { get; set; }
    }


    public class AssignedDocumentVM
    {
        public long DocumentId { get; set; }

        [Display(Name = "Документ")]
        public string DocumentNumber { get; set; }

        public int CaseId { get; set; }

        [Display(Name = "Дело")]
        public string CaseNumber { get; set; }

        [Display(Name = "Разпределено в съд")]
        public string CourtName { get; set; }
    }
}
