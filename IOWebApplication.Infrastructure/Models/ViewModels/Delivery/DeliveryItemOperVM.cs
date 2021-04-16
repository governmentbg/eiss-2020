using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemOperVM
    {
        public int Id { get; set; }
        public int DeliveryItemId { get; set; }

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

        [Display(Name = "Посещение")]
        public int DeliveryOperId { get; set; }

        [Display(Name = "Статус")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете статус")]
        public int NotificationStateId { get; set; }
        
        [Display(Name = "Данни за уведомяване")]
        public string DeliveryInfo { get; set; }

        [Display(Name = "Причина")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете причина")]
        public int? DeliveryReasonId { get; set; }

        [Display(Name = "Дата и час на посещение")]
        [Required(ErrorMessage = "Въведете дата на посещение")]
        public DateTime? DateOper { get; set; }

        [Display(Name = "Към дело:")]
        public string CaseInfo { get; set; }

        public string Long { get; set; }

        public string Lat { get; set; }
    }
}
