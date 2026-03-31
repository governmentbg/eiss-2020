// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class MediationNotificationListVM
    {
        public int Id { get; set; }
        public int MediationSessionId { get; set; }
        public string PersonName { get; set; }
        public string PersonRole { get; set; }
        public int PersonId { get; set; }
        public int? RowNumber { get; set; }
        public int RoleKindId { get; set; }
        public string RoleKindLabel { get; set; }
        public bool IsDeleted { get; set; }
        public bool? IsDeceased { get; set; }
        public string LinkForPersonString { get; set; }
        public string Remark { get; set; }
        public DateTime? DateTo { get; set; }

        public DateTime? DateExpired { get; set; }
        public List<CaseSessionNotificationListNotificationVM> Notifications { get; set; }
    }
}
