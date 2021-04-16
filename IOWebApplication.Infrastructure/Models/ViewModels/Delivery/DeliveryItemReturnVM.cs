using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemReturnVM
    {
        public int Id { get; set; }
   
        [Display(Name = "Изготвена в съд")]
        public string FromCourtName { get; set; }

        [Display(Name = "Регистрационен номер")]
        public string RegNumber { get; set; }
        [Display(Name = "Име на лицето")]
        public string PersonName { get; set; }

        [Display(Name = "Адрес на лицето")]
        public string Address { get; set; }

        [Display(Name = "Район за доставка")]
        public string AreaName { get; set; }

        [Display(Name = "Призовкар")]
        public string LawUnitName { get; set; }

        [Display(Name = "Статус")]
        public int NotificationStateId { get; set; }

        [Display(Name = "Статус")]
        public string NotificationState { get; set; }

        [Display(Name = "Данни за връщане")]
        public string ReturnInfo { get; set; }
        
        [Display(Name = "Дата на връщане")]
        [Required(ErrorMessage = "Въведете дата на връщане")]
        public DateTime? ReturnDate { get; set; }
        public int? CaseNotificationId { get; set; }
        public bool IsForReturn { get; set; }
        public int? NotificationDeliveryGroupId { get; set; }
    }
}
