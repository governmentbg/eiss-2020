using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemFilterVM
    {
        // 1 Призовки в текущия съд
        // 2 Призовки за разнасяне от друг съд
        // 3 Призовки изпратени за разнасяне в друг съд
        public int FilterType { get; set; }

        [Display(Name = "Изготвена в")]
        public int FromCourtId { get; set; }

        [Display(Name = "Изпратена към")]
        public int CourtId { get; set; }

        [Display(Name = "От дата изпращане")]
        public DateTime? DateSendFrom { get; set; }

        [Display(Name = "До дата изпращане")]
        public DateTime? DateSendTo { get; set; }

        [Display(Name = "От дата приемане")]
        public DateTime? DateAcceptedFrom { get; set; }

        [Display(Name = "До дата приемане")]
        public DateTime? DateAcceptedTo { get; set; }

        [Display(Name = "Статус")]
        public int NotificationStateId { get; set; }

        [Display(Name = "Вид известяване")]
        public int NotificationDeliveryGroupId { get; set; }

        [Display(Name = "Рег. номер")]
        public string RegNumber { get; set; }
        
        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }
        public string NoAutoLoad { get; set; }
        [Display(Name = "Тип на документа")]
        public int NotificationTypeId { get; set; }
        [Display(Name = "Призовкар")]
        public int LawUnitId { get; set; }
        public string getDeliveryTypeName()
        {
            switch (FilterType)
            {
                case NomenclatureConstants.DeliveryItemFilterType.Inner:
                    return "Призовки/съобщения в текущия съд";
                case NomenclatureConstants.DeliveryItemFilterType.FromOther:
                    return "Призовки/съобщения изготвени в друг съд";
                case NomenclatureConstants.DeliveryItemFilterType.ToOther:
                    return "Призовки/съобщения изпратени за разнасяне в друг съд";
                default:
                    return "";
            }
        }
        public void ResetCourtByType(int cortId)
        {
            switch (FilterType)
            {
                case NomenclatureConstants.DeliveryItemFilterType.Inner:
                    FromCourtId = cortId;
                    CourtId = cortId;
                    break;
                case NomenclatureConstants.DeliveryItemFilterType.FromOther:
                    CourtId = cortId;
                    break;
                case NomenclatureConstants.DeliveryItemFilterType.ToOther:
                    FromCourtId = cortId;
                    break;
            }

        }
    }
}
