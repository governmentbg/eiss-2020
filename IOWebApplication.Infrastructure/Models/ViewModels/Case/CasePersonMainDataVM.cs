// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseMainDataVM
    {
        public int Id { get; set; }
        public int? CaseSessionId { get; set; }
        public int? CaseSessionActId { get; set; }
        public int? CaseStateId { get; set; }
        public bool IsND { get; set; }
        public int? CaseTypeId { get; set; }
        [Display(Name = "Вид списък")]
        public int? NotificationListTypeId { get; set; }
        public long? DocumentId { get; set; }
    }
}
