using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CasePersonListVM : NamesBase
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int? CaseSessionId { get; set; }
        public string CaseSessionLabel { get; set; }

        //Наследен е обекта NamesBase за да може да се използват методите за съкращаване на имената
        //public string Uic { get; set; }
        public new string UicTypeLabel { get; set; }

        //public string FullName { get; set; }
        //public string FullNameWithShortMiddle { get; set; }

        public string RoleName { get; set; }
        public int PersonRoleId { get; set; }
        public string PersonRoleLabel { get; set; }
        public string PersonRoleBigForumLabel { get; set; }
        public string PersonRoleShortForumLabel { get; set; }
        public int RoleKindId { get; set; }
        public string RoleKindLabel { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int RowNumber { get; set; }
        public bool? ForNotification { get; set; }
        public int? NotificationNumber { get; set; }

        public string CasePersonIdentificator { get; set; }

        public string AllAddressString { get; set; }

        public string AddressString { get; set; }
        public string CurrentAddressString { get; set; }
        public string WorkAddressString { get; set; }
        public bool? IsViewPersonSentence { get; set; }
        public bool? IsViewPersonInheritance { get; set; }
        public bool? IsIndividual { get; set; }
        public bool IsArrested { get; set; }
        public string LinkForPersonString { get; set; }
    }
}
