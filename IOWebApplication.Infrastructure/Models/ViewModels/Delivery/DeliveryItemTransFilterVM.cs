using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemTransFilterVM
    {
        public int ForId { get; set; }

        [Display(Name = "От дата изготвяне")]
        public DateTime? RegDateFrom { get; set; }
        [Display(Name = "До дата изготвяне")]
        public DateTime? RegDateTo { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

         [Display(Name = "Статус")]
        public int NotificationStateId { get; set; }
        
        [Display(Name = "Нов статус")]
        public int ToNotificationStateId { get; set; }


        public static string GetTitle(int toNotificationStateId)
        {
            switch (toNotificationStateId)
            {
                case NomenclatureConstants.NotificationState.Send:
                    return "Изпращане на призовки/съобщения към друг съд за разнасяне";
                case NomenclatureConstants.NotificationState.Received:
                    return "Приемане";// на призовки/съобщения от друг съд за разнасяне";
                case NomenclatureConstants.NotificationState.ForDelivery:
                    return "Към призовкар съд за разнасяне";
            }
            return "Undefined";
        }
        public static string GetForIdLabel(int toNotificationStateId)
        {
            switch (toNotificationStateId)
            {
                case NomenclatureConstants.NotificationState.Send:
                    return "Изпращане към";
                case NomenclatureConstants.NotificationState.Received:
                    return "Приемане от";
                case NomenclatureConstants.NotificationState.ForDelivery:
                    return "За връчване от";
            }
            return "Undefined";
        }
        public static string GetButtonLabel(int toNotificationStateId)
        {
            switch (toNotificationStateId)
            {
                case NomenclatureConstants.NotificationState.Send:
                    return "Изпрати";
                case NomenclatureConstants.NotificationState.Received:
                    return "Приеми";
                case NomenclatureConstants.NotificationState.ForDelivery:
                    return "За връчване";
            }
            return "Undefined";
        }
        public static string GetDateFromLabel(int toNotificationStateId)
        {
            switch (toNotificationStateId)
            {
                case NomenclatureConstants.NotificationState.Send:
                    return "От дата изготвяне";
                case NomenclatureConstants.NotificationState.Received:
                    return "От дата изпращане";
                case NomenclatureConstants.NotificationState.ForDelivery:
                    return "От дата приемане";
            }
            return "Undefined";
        }
        public static string GetDateToLabel(int toNotificationStateId)
        {
            switch (toNotificationStateId)
            {
                case NomenclatureConstants.NotificationState.Send:
                    return "До дата изготвяне";
                case NomenclatureConstants.NotificationState.Received:
                    return "До дата изпращане";
                case NomenclatureConstants.NotificationState.ForDelivery:
                    return "До дата приемане";
            }
            return "Undefined";
        }
        public void initNotificationStateId()
        {
            NotificationStateId = fromNotificationStateId();
        }
        public int fromNotificationStateId()
    {
            switch (ToNotificationStateId)
            {
                case NomenclatureConstants.NotificationState.Send:
                    return NomenclatureConstants.NotificationState.Ready;
                    break;
                case NomenclatureConstants.NotificationState.Received:
                    return NomenclatureConstants.NotificationState.Send;
                    break;
                case NomenclatureConstants.NotificationState.ForDelivery:
                    return NomenclatureConstants.NotificationState.Received;
                    break;
            }
            return 0;
        }
    }

}
