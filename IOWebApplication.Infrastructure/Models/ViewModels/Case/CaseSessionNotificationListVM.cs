// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionNotificationListVM
    {
        public int Id { get; set; }
        public int CaseSessionId { get; set; }
        public string PersonName { get; set; }
        public string PersonRole { get; set; }
        public int PersonId { get; set; }
        public int RowNumber { get; set; }
        public int NotificationPersonType { get; set; }
        public int? NotificationListTypeId { get; set; }
        public string AddressString { get; set; }
        public DateTime? DateSend { get; set; }
        public string Remark { get; set; }
        public int PersonType { get; set; }
        public int RoleKindId { get; set; }
        public string RoleKindLabel { get; set; }
    }
}
