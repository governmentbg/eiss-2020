using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemMobileVM
    {
        public int Id { get; set; }
        public DateTime ItemDate { get; set; }
        public string RegNumber { get; set; }
        public int CourtId { get; set; }
        public string CaseInfo { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public int StateId { get; set; }
        public int VisitCount { get; set; }
        public int ReasonId { get; set; }
        public DateTime? LastVisit { get; set; }
        public bool HaveHolidayVisit { get; set; }
        public int NotificationTypeId { get; set; }
    }
}
