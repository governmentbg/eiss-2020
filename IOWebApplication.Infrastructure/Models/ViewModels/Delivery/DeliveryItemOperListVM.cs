using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemOperListVM
    {
        public int Id { get; set; }
        public int DeliveryItemId { get; set; }
        public int DeliveryOperId { get; set; }

        [Display(Name = "Дата")]
        public DateTime DateOper { get; set; }

        [Display(Name = "Изготвена в съд")]
        public string FromCourtName { get; set; }

        [Display(Name = "Съд")]
        public string ToCourtName { get; set; }

        [Display(Name = "Район за доставка")]
        public string AreaName { get; set; }

        [Display(Name = "Призовкар")]
        public string LawUnitName { get; set; }

        [Display(Name = "Статус")]
        public string NotificationStateName { get; set; }
        
        [Display(Name = "Посещение")]
        public string OperName { get; set; }

        [Display(Name = "Данни за уведомяване")]
        public string DeliveryInfo { get; set; }
        public int NotificationStateId { get; set; }


    }
}
