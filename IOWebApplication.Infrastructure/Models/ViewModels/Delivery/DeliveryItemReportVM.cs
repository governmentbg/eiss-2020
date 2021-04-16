using IOWebApplication.Infrastructure.Data.Models.Delivery;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemReportVM
    {
        // номер на призовка/съобщение..., вид документ, вид, номер и година на делото, адрес за връчване, данни за връчване;
        [Display(Name = "№ на призовка / съобщение")]
        public string RegNumber { get; set; }

        [Display(Name = "Вид документ")]
        public string NotificationTypeLabel { get; set; }

        [Display(Name = "Вид, номер и година на дело")]
        public string CaseInfo { get; set; }

        [Display(Name = "Име на лицето")]
        public string PersonName { get; set; }

        [Display(Name = "Адрес за връчване")]
        public string Address { get; set; }

        [Display(Name = "Данни за връчване")]
        public string DeliveryInfo { get; set; }

        [Display(Name = "Датa връщане")]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Датa доставка")]
        public DateTime? DeliveryDate { get; set; }

        [Display(Name = "Състояние")]
        public string NotificationState { get; set; }

        public DeliveryItemOper DeliveryItemOper { get; set; }
    }
}
