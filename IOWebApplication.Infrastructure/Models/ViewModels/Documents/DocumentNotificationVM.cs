// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentNotificationVM
    {
        public int Id { get; set; }
        public long DocumentId { get; set; }
        public long? DocumentResolutionId { get; set; }
        public string NotificationTypeLabel { get; set; }
        public int? NotificationTypeId { get; set; }
        public string RegNumber { get; set; }
        public string PersonName { get; set; }
        public string NotificationStateLabel { get; set; }
        public string HtmlTemplateLabel { get; set; }
        public int? NotificationNumber { get; set; }
        public DateTime RegDate { get; set; }
    }
}
