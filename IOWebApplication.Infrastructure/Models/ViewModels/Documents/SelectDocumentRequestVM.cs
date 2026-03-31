// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class SelectDocumentRequestVM
    {
        [Display(Name = "Изберете документ за изтегляне на данни")]
        public long? SelectDocumentRequestId { get; set; }

        [Display(Name = "С изтеглянето на данни от документ, текущите ще бъдат заличени!")]
        public bool SelectDocumentConscent { get; set; }
    }
}
