// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentLinkToOtherVM
    {
        public long DocumentId { get; set; }
        [Display(Name = "Съд")]
        public string CourtName { get; set; }
        [Display(Name = "Вид документ")]
        public string DocumentTypeName { get; set; }
        [Display(Name = "Рег.номер")]
        public string DocumentNumber { get; set; }
        [Display(Name = "Дата")]
        public DateTime DocumentDate { get; set; }
    }
}
