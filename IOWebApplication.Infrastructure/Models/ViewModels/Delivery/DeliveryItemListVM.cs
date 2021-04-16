using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemListVM
    {
//        период на разпределяне на призовки/съобщения/уведомления(получаване от призовкар) – от дата/до дата или за конкретна дата; съд, вид дело, съдебен призовкар и т.н.
        [Display(Name = "Изготвена в")]
        public int FromCourtId { get; set; }

        [Display(Name = "От дата изпращане")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата изпращане")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Призовкар")]
        public int LawUnitId { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Основен вид дело")]
        public string CaseGroupLabel { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeLabel { get; set; }

        [Display(Name = "Страна/Участник(част от име)")]
        public string PersonName { get; set; }
    }
}
