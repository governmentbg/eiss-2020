// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class NotificationPrintFilterVM
    {
        public int? CaseId { get; set; }
        public int? CaseSessionId { get; set; }
        public int? CaseSessionActId { get; set; }
        public bool IsList { get; set; }
        public int? NotificationListTypeId { get; set; }

        [Display(Name = "От номер")]
        public int FromRowNumber { get; set; }

        [Display(Name = "До номер")]
        public int ToRowNumber { get; set; }
    }
}
