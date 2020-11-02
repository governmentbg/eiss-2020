// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CasePersonListVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int? CaseSessionId { get; set; }
        public string CaseSessionLabel { get; set; }

        public string Uic { get; set; }
        public string UicTypeLabel { get; set; }

        public string FullName { get; set; }

        public string RoleName { get; set; }
        public int PersonRoleId { get; set; }
        public string PersonRoleLabel { get; set; }
        public int RoleKindId { get; set; }
        public string RoleKindLabel { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int RowNumber { get; set; }
        public bool? ForNotification { get; set; }
        public int? NotificationNumber { get; set; }

        public string CasePersonIdentificator { get; set; }
        public string AddressString { get; set; }
        public string CurrentAddressString { get; set; }
        public string WorkAddressString { get; set; }
        public bool? IsViewPersonSentence { get; set; }
        public bool? IsViewPersonInheritance { get; set; }
        public bool? IsIndividual { get; set; }
        public bool IsArrested { get; set; }
    }
}
